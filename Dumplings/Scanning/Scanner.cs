using Dumplings.Helpers;
using Dumplings.Rpc;
using MoreLinq;
using NBitcoin;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dumplings.Scanning
{
    public class Scanner
    {
        public Scanner(RPCClient rpc)
        {
            Rpc = rpc;
            Directory.CreateDirectory(WorkFolder);
        }

        public const string WorkFolder = "Scanner";
        public static readonly string LastProcessedBlockHeightPath = Path.Combine(WorkFolder, "LastProcessedBlockHeight.txt");

        public RPCClient Rpc { get; }

        private decimal PercentageDone { get; set; } = 0;
        private decimal PreviousPercentageDone { get; set; } = -1;

        public async Task ScanAsync(bool rescan)
        {
            if (rescan)
            {
                Logger.LogWarning("Rescanning...");
            }
            if (rescan && Directory.Exists(WorkFolder))
            {
                Directory.Delete(WorkFolder, true);
            }
            Directory.CreateDirectory(WorkFolder);

            ulong height = Constants.FirstJoinMarketBlock;
            if (File.Exists(LastProcessedBlockHeightPath))
            {
                height = ulong.Parse(File.ReadAllText(LastProcessedBlockHeightPath)) + 1;
                Logger.LogWarning($"{height - Constants.FirstJoinMarketBlock + 1} blocks already processed. Continue scanning...");
            }

            var bestHeight = (ulong)await Rpc.GetBlockCountAsync().ConfigureAwait(false);

            Logger.LogInfo($"Last processed block: {height - 1}.");
            ulong totalBlocks = bestHeight - height + 1;
            Logger.LogInfo($"About {totalBlocks} ({totalBlocks / 144} days) blocks will be processed.");

            while (height <= bestHeight)
            {
                var block = await Rpc.GetVerboseBlockAsync(height).ConfigureAwait(false);

                var wasabiTxs = new List<VerboseTransactionInfo>();
                var samouraiTxs = new List<VerboseTransactionInfo>();
                var joinmarketTxs = new List<VerboseTransactionInfo>();
                var txsLock = new object();

                Parallel.ForEach(block.Transactions, (dotnetBrainfuckTx) =>
                {
                    VerboseTransactionInfo tx = dotnetBrainfuckTx;

                    // IDENTIFY WASABI COINJOINS
                    bool isWasabiCj = false;
                    var indistinguishableOutputs = tx.GetIndistinguishableOutputs(includeSingle: false);
                    if (block.Height >= Constants.FirstWasabiBlock)
                    {
                        if (block.Height > Constants.FirstWasabiNoCoordAddressBlock)
                        {
                            if (indistinguishableOutputs.Any())
                            {
                                (Money value, int count) = indistinguishableOutputs.MaxBy(x => x.count);
                                isWasabiCj =
                                    count >= 10
                                    && tx.Inputs.Count() >= count
                                    && value.Almost(Constants.ApproximateWasabiBaseDenomination, Constants.WasabiBaseDenominationPrecision);
                            }
                            else
                            {
                                isWasabiCj = false;
                            }
                        }
                        else
                        {
                            isWasabiCj = tx.Outputs.Any(x => Constants.WasabiCoordScripts.Contains(x.ScriptPubKey)) && indistinguishableOutputs.Any(x => x.count > 2);
                        }
                    }

                    // IDENTIFY SAMOURAI COINJOINS
                    bool isSamouraiCj = false;
                    if (block.Height >= Constants.FirstSamouraiBlock)
                    {
                        isSamouraiCj =
                            tx.Inputs.Count() == 5
                            && tx.Outputs.Count() == 5
                            && tx.Outputs.Select(x => x.Value).Distinct().Count() == 1
                            && Constants.SamouraiPools.Any(x => x.Almost(tx.Outputs.First().Value, Money.Coins(0.01m)));
                    }


                    // IDENTIFY JOINMARKET COINJOINS
                    // https://github.com/nopara73/WasabiVsSamourai/issues/2#issuecomment-558775856
                    bool isJoinMarketCj = false;
                    if (block.Height >= Constants.FirstJoinMarketBlock)
                    {
                        if (indistinguishableOutputs.Any() && !isWasabiCj && !isSamouraiCj)
                        {
                            isJoinMarketCj = IsJoinMarketTx(tx, indistinguishableOutputs);
                        }
                        else
                        {
                            isJoinMarketCj = false;
                        }
                    }

                    lock (txsLock)
                    {
                        if (isWasabiCj)
                        {
                            wasabiTxs.Add(tx);
                        }
                        else if (isSamouraiCj)
                        {
                            samouraiTxs.Add(tx);
                        }
                        else if (isJoinMarketCj)
                        {
                            joinmarketTxs.Add(tx);
                        }
                    }

                });

                decimal totalBlocksPer100 = totalBlocks / 100m;
                ulong blocksLeft = bestHeight - height;
                ulong processedBlocks = totalBlocks - blocksLeft;
                PercentageDone = processedBlocks / totalBlocksPer100;
                bool displayProgress = (PercentageDone - PreviousPercentageDone) >= 1;
                if (displayProgress)
                {
                    Logger.LogInfo($"Progress: {(int)PercentageDone}%, Current height: {height}.");
                    PreviousPercentageDone = PercentageDone;
                }
                if (bestHeight <= height)
                {
                    // Refresh bestHeight and if still no new block, then end here.
                    bestHeight = (ulong)await Rpc.GetBlockCountAsync().ConfigureAwait(false);
                    if (bestHeight <= height)
                    {
                        break;
                    }
                }

                File.WriteAllText(LastProcessedBlockHeightPath, height.ToString());
                lock (txsLock)
                {
                    if (joinmarketTxs.Any() || wasabiTxs.Any() || samouraiTxs.Any())
                    {
                        Logger.LogInfo($"Block {height}, JM: {joinmarketTxs.Count}, WW: {wasabiTxs.Count}, SW: {samouraiTxs.Count}");
                    }
                }

                height++;
            }
        }

        private static bool IsJoinMarketTx(VerboseTransactionInfo tx, IEnumerable<(Money value, int count)> indistinguishableOutputs)
        {
            bool isJoinMarketCj;
            (Money bestEqualOutputValue, int bestEqualOutputCount) = indistinguishableOutputs.MaxBy(x => x.count);
            var changeCount = tx.Outputs.Count() - bestEqualOutputCount;
            isJoinMarketCj =
                bestEqualOutputCount >= 2 // ToDo: Investigate false postivies: "could be 2 in theory too, but will give a lot of false positives likely and are rare"
                && bestEqualOutputCount <= 30 // "also, number of equally sized outputs could be limited to 20 or 30, as more aren't practical, due to IRC server rate limits for privmsg's"
                && (bestEqualOutputCount == changeCount || bestEqualOutputCount == changeCount - 1) // ""number of equally sized outputs" == ("number of other outputs" OR "number of other outputs" - 1)"
                && tx.Inputs.Count() >= tx.Outputs.Count()
                && bestEqualOutputValue >= Money.Coins(0.001m); // "equally sized output amount above or equal to 0.001 BTC (or even 0.01 BTC, which is current default in joinmarket.cfg)"

            if (isJoinMarketCj)
            {
                // "no more than one output with different address type than others"
                var outputTypes = tx.Outputs.Select(x => x.PubkeyType).Distinct();
                if (outputTypes.Count() != 1)
                {
                    if (outputTypes.Count() == 2)
                    {
                        var firstType = outputTypes.First();
                        var secondType = outputTypes.Take(2).Last();

                        var firstTypeCount = tx.Outputs.Count(x => x.PubkeyType == firstType);
                        var secondTypeCount = tx.Outputs.Count(x => x.PubkeyType == secondType);

                        var differentTypes = new List<RpcPubkeyType>();
                        if (firstTypeCount == 1)
                        {
                            differentTypes.Add(firstType);
                        }
                        else if (secondTypeCount == 1)
                        {
                            differentTypes.Add(secondType);
                        }

                        if (!differentTypes.Any())
                        {
                            isJoinMarketCj = false;
                        }
                        else
                        {
                            // that must be one of equally sized ones
                            if (!tx.Outputs.Where(x => x.Value == bestEqualOutputValue).Any(x => differentTypes.Contains(x.PubkeyType)))
                            {
                                isJoinMarketCj = false;
                            }
                        }
                    }
                    else
                    {
                        isJoinMarketCj = false;
                    }

                    if (isJoinMarketCj)
                    {
                        // only following combinations then are allowed: n P2PKH, n P2PKH + 1 P2SH, n P2SH, n P2SH + 1 P2PKH, n P2SH + 1 bech32
                        var p2pkhCount = tx.Outputs.Count(x => x.PubkeyType == RpcPubkeyType.TxPubkeyhash);
                        var p2shCount = tx.Outputs.Count(x => x.PubkeyType == RpcPubkeyType.TxScripthash);
                        var bech32Count = tx.Outputs.Count(x => x.PubkeyType == RpcPubkeyType.TxWitnessV0Keyhash);
                        var outputCount = tx.Outputs.Count();

                        isJoinMarketCj =
                            p2pkhCount == outputCount
                            || (p2pkhCount == outputCount - 1 && p2shCount == 1)
                            || p2shCount == outputCount
                            || (p2shCount == outputCount - 1 && p2pkhCount == 1)
                            || (p2shCount == outputCount - 1 && bech32Count == 1);
                    }
                }
            }

            return isJoinMarketCj;
        }
    }
}
