using Dumplings.Analysis;
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

            ulong height = 0;
            if (File.Exists(LastProcessedBlockHeightPath))
            {
                height = ulong.Parse(File.ReadAllText(LastProcessedBlockHeightPath)) + 1;
                Logger.LogWarning($"{height - 1} blocks already processed. Continue scanning...");
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
                    bool isJoinMarketCj = false;
                    bool isSamouraiCj = false;
                    var indistinguishableOutputs = tx.GetIndistinguishableOutputs(includeSingle: false);
                    if (tx.Inputs.All(x => x.Coinbase is null) && indistinguishableOutputs.Any())
                    {
                        (Money value, int count) = indistinguishableOutputs.MaxBy(x => x.count);
                        isWasabiCj =
                            count >= 10
                            && tx.Inputs.Count() >= count
                            && value.Almost(Constants.ApproximateWasabiBaseDenomination, Constants.WasabiBaseDenominationPrecision);

                        // IDENTIFY SAMOURAI COINJOINS
                        isSamouraiCj =
                               tx.Inputs.Count() == 5
                               && tx.Outputs.Count() == 5
                               && tx.Outputs.Select(x => x.Value).Distinct().Count() == 1
                               && Constants.SamouraiPools.Any(x => x.Almost(tx.Outputs.First().Value, Money.Coins(0.01m)));


                        // IDENTIFY JOINMARKET COINJOINS
                        // https://github.com/nopara73/WasabiVsSamourai/issues/2#issuecomment-558775856
                        if (!isWasabiCj && !isSamouraiCj)
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
                        foreach (var tx in joinmarketTxs)
                        {
                            Console.WriteLine($"JM: {tx.Id}");
                        }
                        foreach (var tx in wasabiTxs)
                        {
                            Console.WriteLine($"WW: {tx.Id}");
                        }
                        foreach (var tx in samouraiTxs)
                        {
                            Console.WriteLine($"SW: {tx.Id}");
                        }
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
            var onePcOfCj = bestEqualOutputValue.Percentage(1m);
            var minMaxFee = Money.Coins(0.0001m);
            var maxFee = onePcOfCj > minMaxFee ? onePcOfCj : minMaxFee;
            isJoinMarketCj =
                bestEqualOutputCount >= 2 // ToDo: Investigate false postivies: "could be 2 in theory too, but will give a lot of false positives likely and are rare"
                && bestEqualOutputCount <= 30 // "also, number of equally sized outputs could be limited to 20 or 30, as more aren't practical, due to IRC server rate limits for privmsg's"
                && (bestEqualOutputCount == changeCount || bestEqualOutputCount == changeCount + 1) // ""number of equally sized outputs" == ("number of other outputs" OR "number of other outputs" - 1)"
                && tx.Outputs.Select(x => x.ScriptPubKey).Distinct().Count() >= 3 // https://github.com/citp/BlockSci/blob/67ee51d6c89e145e879181c7f934fddb4ce3b54b/src/heuristics/tx_identification.cpp#L20
                && tx.Inputs.Select(x => x.PrevOutput.ScriptPubKey).Distinct().Count() >= tx.Outputs.Count()
                // && tx.Inputs.Count() <= 17 // Based on the work of Möser et al., who don’t use this heuristic, the vast majority — 92 % — of JoinMarket transactions have no more than 17 inputs.
                && tx.Inputs.All(x => x.PrevOutput.PubkeyType == RpcPubkeyType.TxPubkeyhash || x.PrevOutput.PubkeyType == RpcPubkeyType.TxScripthash)
                && tx.Outputs.Select(x => x.PubkeyType).Distinct().Count() <= 2
                && tx.Inputs.Select(x => x.PrevOutput.Value).Max() <= bestEqualOutputValue + tx.Outputs.Select(x => x.Value).Where(x => x != bestEqualOutputValue).Max() - maxFee
                && bestEqualOutputValue >= Money.Coins(0.001m); // "equally sized output amount above or equal to 0.001 BTC (or even 0.01 BTC, which is current default in joinmarket.cfg)"

            if (isJoinMarketCj)
            {
                // There must be enough inputs to cover all of the
                // outputs(lines 10–32).Specifically, for each change
                // address, there must be a distinct set of inputs that
                // add up to at least the output value v plus the change
                // value minus the max fee(q) that might have been
                // paid to the liquidity providers.For our calculations
                // we set this to be the maximum of .0001 satoshis or
                // 1 % of the CoinJoin output.
                var remainingInputValues = tx.Inputs.Select(x => x.PrevOutput.Value).OrderByDescending(x => x).ToList();
                var changeOutputValues = tx.Outputs.Select(x => x.Value).Where(x => x != bestEqualOutputValue).OrderByDescending(x => x).ToList();
                var remainingOutputValues = tx.Outputs.Select(x => x.Value).ToList();
                var toRemoveChangeOutputValues = new List<Money>();

                if (bestEqualOutputCount == changeCount + 1)
                {
                    // Then there must be an equal output that doesn't have change.
                    Money toRemoveInputValue = null;
                    foreach (var inputValue in remainingInputValues)
                    {
                        if (inputValue.Almost(bestEqualOutputValue, maxFee))
                        {
                            toRemoveInputValue = inputValue;
                            remainingOutputValues.Remove(bestEqualOutputValue);
                            break;
                        }
                    }

                    if (toRemoveInputValue is { })
                    {
                        remainingInputValues.Remove(toRemoveInputValue);
                    }
                }

                foreach (var changeValue in changeOutputValues)
                {
                    var sum = changeValue + bestEqualOutputValue;
                    Money toRemoveInputValue = null;
                    foreach (var inputValue in remainingInputValues)
                    {
                        if (inputValue.Almost(sum, maxFee))
                        {
                            toRemoveInputValue = inputValue;
                            toRemoveChangeOutputValues.Add(changeValue);
                            break;
                        }
                    }

                    if (toRemoveInputValue is { })
                    {
                        remainingInputValues.Remove(toRemoveInputValue);
                    }
                }

                foreach (var changeToRemove in toRemoveChangeOutputValues)
                {
                    remainingOutputValues.Remove(bestEqualOutputValue);
                    remainingOutputValues.Remove(changeToRemove);
                }

                // If subset sum is feasible on the remaining input and output values then do subset sum, otherwise risk false positive.
                // Note maxfee isn't exactly what should be here, but it's a good enough precision to use.
                if (remainingInputValues.Count <= 7 && remainingOutputValues.Count <= 7) // 7x7 is where this aglo is still performant on my computer
                {
                    try
                    {
                        var nonDerivedMapping = new Mapping(new SubSet(remainingInputValues.Select(x => x.ToDecimal(MoneyUnit.BTC)), remainingOutputValues.Select(x => x.ToDecimal(MoneyUnit.BTC)), maxFee.ToDecimal(MoneyUnit.BTC)));
                        var mappings = nonDerivedMapping.AnalyzeWithNopara73Algorithm().ToArray();
                        // If the analysis did not find a mapping that has the exact same number subsets as the participant count, then it's not a CJ.
                        if (!mappings.Any(x => x.SubSets.Count() == bestEqualOutputCount))
                        {
                            isJoinMarketCj = false;
                        }

                    }
                    catch (InvalidOperationException ex)
                    {
                        // Precision issues, doesn't matter let's risk false positive.
                        Logger.LogTrace(ex);
                    }

                }
            }

            return isJoinMarketCj;
        }
    }
}
