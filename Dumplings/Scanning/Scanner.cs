using Dumplings.Analysis;
using Dumplings.Helpers;
using Dumplings.Rpc;
using Microsoft.Extensions.Caching.Memory;
using NBitcoin;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            BitcoinStatus.CheckAsync(rpc).GetAwaiter().GetResult();
        }

        //public const string WorkFolder = @"C:\Users\user\source\repos\Dumplings\Dumplings.Cli\bin\Release\netcoreapp3.1\Scanner";
        public const string WorkFolder = "Scanner";

        public static readonly string LastProcessedBlockHeightPath = Path.Combine(WorkFolder, "LastProcessedBlockHeight.txt");
        public static readonly string WasabiCoinJoinsPath = Path.Combine(WorkFolder, "WasabiCoinJoins.txt");
        public static readonly string Wasabi2CoinJoinsPath = Path.Combine(WorkFolder, "Wasabi2CoinJoins.txt");
        public static readonly string SamouraiCoinJoinsPath = Path.Combine(WorkFolder, "SamouraiCoinJoins.txt");
        public static readonly string SamouraiTx0sPath = Path.Combine(WorkFolder, "SamouraiTx0s.txt");
        public static readonly string OtherCoinJoinsPath = Path.Combine(WorkFolder, "OtherCoinJoins.txt");
        public static readonly string Wasabi2PostMixTxsPath = Path.Combine(WorkFolder, "Wasabi2PostMixTxs.txt");
        public static readonly string WasabiPostMixTxsPath = Path.Combine(WorkFolder, "WasabiPostMixTxs.txt");
        public static readonly string SamouraiPostMixTxsPath = Path.Combine(WorkFolder, "SamouraiPostMixTxs.txt");
        public static readonly string OtherCoinJoinPostMixTxsPath = Path.Combine(WorkFolder, "OtherCoinJoinPostMixTxs.txt");

        public RPCClient Rpc { get; }

        private decimal PercentageDone { get; set; } = 0;
        private decimal PreviousPercentageDone { get; set; } = -1;
        public static HashSet<long> Wasabi2Denominations { get; } = CreateWasabi2Denominations().ToHashSet();

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
            var allWasabi2CoinJoinSet = new HashSet<uint256>();
            var allWasabiCoinJoinSet = new HashSet<uint256>();
            var allSamouraiCoinJoinSet = new HashSet<uint256>();
            var allOtherCoinJoinSet = new HashSet<uint256>();
            var allSamouraiTx0Set = new HashSet<uint256>();

            var opreturnTransactionCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 100000 });

            ulong startingHeight = Constants.FirstWasabiBlock;
            ulong height = startingHeight;
            if (File.Exists(LastProcessedBlockHeightPath))
            {
                height = ReadBestHeight() + 1;
                allSamouraiCoinJoinSet = Enumerable.ToHashSet(ReadSamouraiCoinJoins().Select(x => x.Id));
                allWasabi2CoinJoinSet = Enumerable.ToHashSet(ReadWasabi2CoinJoins().Select(x => x.Id));
                allWasabiCoinJoinSet = Enumerable.ToHashSet(ReadWasabiCoinJoins().Select(x => x.Id));
                allOtherCoinJoinSet = Enumerable.ToHashSet(ReadOtherCoinJoins().Select(x => x.Id));
                allSamouraiTx0Set = Enumerable.ToHashSet(ReadSamouraiTx0s().Select(x => x.Id));
                Logger.LogWarning($"{height - startingHeight + 1} blocks already processed. Continue scanning...");
            }

            var bestHeight = (ulong)await Rpc.GetBlockCountAsync().ConfigureAwait(false);

            Logger.LogInfo($"Last processed block: {height - 1}.");
            ulong totalBlocks = bestHeight - height + 1;
            Logger.LogInfo($"About {totalBlocks} ({totalBlocks / 144} days) blocks will be processed.");

            var stopWatch = new Stopwatch();
            var processedBlocksWhenSwStarted = CalculateProcessedBlocks(height, bestHeight, totalBlocks);
            stopWatch.Start();

            while (height <= bestHeight)
            {
                var block = await Rpc.GetVerboseBlockAsync(height, safe: false).ConfigureAwait(false);

                var wasabiCoinJoins = new List<VerboseTransactionInfo>();
                var wasabi2CoinJoins = new List<VerboseTransactionInfo>();
                var samouraiCoinJoins = new List<VerboseTransactionInfo>();
                var samouraiTx0s = new List<VerboseTransactionInfo>();
                var otherCoinJoins = new List<VerboseTransactionInfo>();
                var wasabiPostMixTxs = new List<VerboseTransactionInfo>();
                var wasabi2PostMixTxs = new List<VerboseTransactionInfo>();
                var samouraiPostMixTxs = new List<VerboseTransactionInfo>();
                var otherCoinJoinPostMixTxs = new List<VerboseTransactionInfo>();

                foreach (var tx in block.Transactions)
                {
                    if (tx.Outputs.Count() > 2 && tx.Outputs.Any(x => TxNullDataTemplate.Instance.CheckScriptPubKey(x.ScriptPubKey)))
                    {
                        opreturnTransactionCache.Set(tx.Id, tx, new MemoryCacheEntryOptions().SetSize(1));
                    }

                    bool isWasabiCj = false;
                    bool isWasabi2Cj = false;
                    bool isSamouraiCj = false;
                    bool isOtherCj = false;
                    if (tx.Inputs.All(x => x.Coinbase is null))
                    {
                        var indistinguishableOutputs = tx.GetIndistinguishableOutputs(includeSingle: false).ToArray();
                        if (indistinguishableOutputs.Any())
                        {
                            var outputs = tx.Outputs.ToArray();
                            var inputs = tx.Inputs.Select(x => x.PrevOutput).ToArray();
                            var outputValues = outputs.Select(x => x.Value);
                            var inputValues = inputs.Select(x => x.Value);
                            var outputCount = outputs.Length;
                            var inputCount = inputs.Length;
                            (Money mostFrequentEqualOutputValue, int mostFrequentEqualOutputCount) = indistinguishableOutputs.OrderByDescending(x => x.count).First(); // Segwit only inputs.
                            var isNativeSegwitOnly = tx.Inputs.All(x => x.PrevOutput.ScriptPubKey.IsScriptType(ScriptType.P2WPKH)) && tx.Outputs.All(x => x.ScriptPubKey.IsScriptType(ScriptType.P2WPKH)); // Segwit only outputs.

                            // IDENTIFY WASABI 2 COINJOINS
                            if (block.Height >= Constants.FirstWasabi2Block)
                            {
                                isWasabi2Cj =
                                    tx.Inputs.All(x => x.PrevOutput.ScriptPubKey.IsScriptType(ScriptType.P2WPKH) || x.PrevOutput.ScriptPubKey.IsScriptType(ScriptType.Taproot))
                                    && inputCount >= 50 // 50 was the minimum input count at the beginning of Wasabi 2.
                                    && inputValues.SequenceEqual(inputValues.OrderByDescending(x => x)) // Inputs are ordered descending.
                                    && outputValues.SequenceEqual(outputValues.OrderByDescending(x => x)) // Outputs are ordered descending.
                                    && outputValues.Count(x => Wasabi2Denominations.Contains(x.Satoshi)) > outputCount * 0.8; // Most of the outputs contains the denomination.
                            }

                            // IDENTIFY WASABI COINJOINS
                            if (!isWasabi2Cj && block.Height >= Constants.FirstWasabiBlock)
                            {
                                // Before Wasabi had constant coordinator addresses and different base denominations at the beginning.
                                if (block.Height < Constants.FirstWasabiNoCoordAddressBlock)
                                {
                                    isWasabiCj = tx.Outputs.Any(x => Constants.WasabiCoordScripts.Contains(x.ScriptPubKey)) && indistinguishableOutputs.Any(x => x.count > 2);
                                }
                                else
                                {
                                    var uniqueOutputCount = tx.GetIndistinguishableOutputs(includeSingle: true).Count(x => x.count == 1);
                                    isWasabiCj =
                                        isNativeSegwitOnly
                                        && mostFrequentEqualOutputCount >= 10 // At least 10 equal outputs.
                                        && inputCount >= mostFrequentEqualOutputCount // More inptuts than most frequent equal outputs.
                                        && mostFrequentEqualOutputValue.Almost(Constants.ApproximateWasabiBaseDenomination, Constants.WasabiBaseDenominationPrecision) // The most frequent equal outputs must be almost the base denomination.
                                        && uniqueOutputCount >= 2; // It's very likely there's at least one change and at least one coord output those have unique values.
                                }
                            }

                            // IDENTIFY SAMOURAI COINJOINS
                            if (block.Height >= Constants.FirstSamouraiBlock)
                            {
                                // Pinpointing and Measuring Wasabi and Samourai CoinJoins in the Bitcoin Ecosystem
                                // Slightly improved on SCDH
                                // Samourai CoinJoin Detection Heuristic (SCDH) If a transaction t has exactly
                                // five uniform outputs that equal p BTC and if it has precisely five inputs, with at
                                // least one and at most three equal p BTC, while the remaining two to four inputs
                                // are between p and p +0.0011 BTC, then t is a Samourai Whirlpool CoinJoin
                                // transaction.
                                var poolSize = tx.Outputs.First().Value;
                                var poolSizedInputCount = tx.Inputs.Count(x => x.PrevOutput.Value == poolSize);
                                isSamouraiCj =
                                   isNativeSegwitOnly
                                   && inputCount >= 5 && inputCount <= 10
                                   && outputCount >= 5 && outputCount <= 10
                                   && inputCount == outputCount
                                   && outputValues.Distinct().Count() == 1 // Outputs are always equal.
                                   && Constants.SamouraiPools.Any(x => x == poolSize) // Just to be sure match Samourai's pool sizes.
                                   && poolSizedInputCount >= 1
                                   && tx.Inputs.Where(x => x.PrevOutput.Value != poolSize).All(x => x.PrevOutput.Value.Almost(poolSize, Money.Coins(0.0011m)));
                            }

                            // IDENTIFY OTHER EQUAL OUTPUT COINJOIN LIKE TRANSACTIONS
                            if (!isWasabi2Cj && !isWasabiCj && !isSamouraiCj)
                            {
                                isOtherCj =
                                    indistinguishableOutputs.Length == 1 // If it isn't then it'd be likely a multidenomination CJ, which only Wasabi does.
                                    && mostFrequentEqualOutputCount == outputCount - mostFrequentEqualOutputCount // Rarely it isn't, but it helps filtering out false positives.
                                    && outputs.Select(x => x.ScriptPubKey).Distinct().Count() >= mostFrequentEqualOutputCount // Otherwise more participants would be single actors which makes no sense.
                                    && inputs.Select(x => x.ScriptPubKey).Distinct().Count() >= mostFrequentEqualOutputCount // Otherwise more participants would be single actors which makes no sense.
                                    && inputValues.Max() <= mostFrequentEqualOutputValue + outputValues.Where(x => x != mostFrequentEqualOutputValue).Max() - Money.Coins(0.0001m); // I don't want to run expensive subset sum, so this is a shortcut to at least filter out false positives.
                            }

                            if (isWasabi2Cj)
                            {
                                wasabi2CoinJoins.Add(tx);
                                allWasabi2CoinJoinSet.Add(tx.Id);
                            }
                            else if (isWasabiCj)
                            {
                                wasabiCoinJoins.Add(tx);
                                allWasabiCoinJoinSet.Add(tx.Id);
                            }
                            else if (isSamouraiCj)
                            {
                                samouraiCoinJoins.Add(tx);
                                allSamouraiCoinJoinSet.Add(tx.Id);
                            }
                            else if (isOtherCj)
                            {
                                otherCoinJoins.Add(tx);
                                allOtherCoinJoinSet.Add(tx.Id);
                            }
                        }

                        foreach (var inputTxId in tx.Inputs.Select(x => x.OutPoint.Hash))
                        {
                            if (!isWasabi2Cj && allWasabi2CoinJoinSet.Contains(inputTxId) && !wasabi2PostMixTxs.Any(x => x.Id == tx.Id))
                            {
                                // Then it's a post mix tx.
                                wasabi2PostMixTxs.Add(tx);
                                if (isOtherCj)
                                {
                                    // Then it's false positive detection.
                                    isOtherCj = false;
                                    allOtherCoinJoinSet.Remove(tx.Id);
                                    otherCoinJoins.Remove(tx);
                                }
                            }

                            if (!isWasabiCj && allWasabiCoinJoinSet.Contains(inputTxId) && !wasabiPostMixTxs.Any(x => x.Id == tx.Id))
                            {
                                // Then it's a post mix tx.
                                wasabiPostMixTxs.Add(tx);
                                if (isOtherCj)
                                {
                                    // Then it's false positive detection.
                                    isOtherCj = false;
                                    allOtherCoinJoinSet.Remove(tx.Id);
                                    otherCoinJoins.Remove(tx);
                                }
                            }

                            if (!isSamouraiCj && allSamouraiCoinJoinSet.Contains(inputTxId) && !samouraiPostMixTxs.Any(x => x.Id == tx.Id))
                            {
                                // Then it's a post mix tx.
                                samouraiPostMixTxs.Add(tx);
                                if (isOtherCj)
                                {
                                    // Then it's false positive detection.
                                    isOtherCj = false;
                                    allOtherCoinJoinSet.Remove(tx.Id);
                                    otherCoinJoins.Remove(tx);
                                }
                            }

                            if (!isOtherCj && allOtherCoinJoinSet.Contains(inputTxId) && !otherCoinJoinPostMixTxs.Any(x => x.Id == tx.Id))
                            {
                                // Then it's a post mix tx.
                                otherCoinJoinPostMixTxs.Add(tx);
                            }
                        }
                    }
                }

                foreach (var txid in samouraiCoinJoins.SelectMany(x => x.Inputs).Select(x => x.OutPoint.Hash).Where(x => !allSamouraiCoinJoinSet.Contains(x) && !allSamouraiTx0Set.Contains(x)).Distinct())
                {
                    if (!opreturnTransactionCache.TryGetValue(txid, out VerboseTransactionInfo vtxi))
                    {
                        var tx0 = await Rpc.GetSmartRawTransactionInfoAsync(txid).ConfigureAwait(false);
                        var verboseOutputs = new List<VerboseOutputInfo>(tx0.Transaction.Outputs.Count);
                        foreach (var o in tx0.Transaction.Outputs)
                        {
                            var voi = new VerboseOutputInfo(o.Value, o.ScriptPubKey);
                            verboseOutputs.Add(voi);
                        }

                        var verboseInputs = new List<VerboseInputInfo>(tx0.Transaction.Inputs.Count);
                        foreach (var i in tx0.Transaction.Inputs)
                        {
                            var tx = await Rpc.GetRawTransactionAsync(i.PrevOut.Hash).ConfigureAwait(false);
                            var o = tx.Outputs[i.PrevOut.N];
                            var voi = new VerboseOutputInfo(o.Value, o.ScriptPubKey);
                            var vii = new VerboseInputInfo(i.PrevOut, voi);
                            verboseInputs.Add(vii);
                        }

                        vtxi = new VerboseTransactionInfo(tx0.TransactionBlockInfo, txid, verboseInputs, verboseOutputs);
                    }

                    if (vtxi is { })
                    {
                        allSamouraiTx0Set.Add(txid);
                        samouraiTx0s.Add(vtxi);

                        if (allOtherCoinJoinSet.Contains(txid))
                        {
                            // Then it's false positive detection.
                            // It happens like 10-ish times, so it shouldn't be too expensive to rewrite the file.
                            allOtherCoinJoinSet.Remove(txid);
                            var found = otherCoinJoins.FirstOrDefault(x => x.Id == txid);
                            if (found is { })
                            {
                                otherCoinJoins.Remove(found);
                            }
                            else
                            {
                                var allOtherCoinJoins = ReadOtherCoinJoins().Where(x => x.Id != txid);
                                File.WriteAllLines(OtherCoinJoinsPath, allOtherCoinJoins.Select(x => RpcParser.ToLine(x)));
                            }
                        }
                    }
                }

                decimal totalBlocksPer100 = totalBlocks / 100m;
                ulong processedBlocks = CalculateProcessedBlocks(height, bestHeight, totalBlocks);
                PercentageDone = processedBlocks / totalBlocksPer100;
                bool displayProgress = (PercentageDone - PreviousPercentageDone) >= 0.1m;
                if (displayProgress)
                {
                    var blocksWithinElapsed = processedBlocks - processedBlocksWhenSwStarted;
                    ulong blocksLeft = bestHeight - height;
                    var elapsed = stopWatch.Elapsed;

                    if (blocksWithinElapsed != 0)
                    {
                        var estimatedTimeLeft = (elapsed / blocksWithinElapsed) * blocksLeft;

                        Logger.LogInfo($"Progress: {PercentageDone:0.#}%, Current height: {height}, Estimated time left: {estimatedTimeLeft.TotalHours:0.#} hours.");

                        PreviousPercentageDone = PercentageDone;

                        processedBlocksWhenSwStarted = processedBlocks;
                        stopWatch.Restart();
                    }
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
                File.AppendAllLines(Wasabi2CoinJoinsPath, wasabi2CoinJoins.Select(x => RpcParser.ToLine(x)));
                File.AppendAllLines(WasabiCoinJoinsPath, wasabiCoinJoins.Select(x => RpcParser.ToLine(x)));
                File.AppendAllLines(SamouraiCoinJoinsPath, samouraiCoinJoins.Select(x => RpcParser.ToLine(x)));
                File.AppendAllLines(SamouraiTx0sPath, samouraiTx0s.Select(x => RpcParser.ToLine(x)));
                File.AppendAllLines(OtherCoinJoinsPath, otherCoinJoins.Select(x => RpcParser.ToLine(x)));
                File.AppendAllLines(Wasabi2PostMixTxsPath, wasabi2PostMixTxs.Select(x => RpcParser.ToLine(x)));
                File.AppendAllLines(WasabiPostMixTxsPath, wasabiPostMixTxs.Select(x => RpcParser.ToLine(x)));
                File.AppendAllLines(SamouraiPostMixTxsPath, samouraiPostMixTxs.Select(x => RpcParser.ToLine(x)));
                File.AppendAllLines(OtherCoinJoinPostMixTxsPath, otherCoinJoinPostMixTxs.Select(x => RpcParser.ToLine(x)));

                height++;
            }
        }

        private static IEnumerable<VerboseTransactionInfo> ReadWasabi2CoinJoins()
        {
            return File.ReadAllLines(Wasabi2CoinJoinsPath).Select(x => RpcParser.VerboseTransactionInfoFromLine(x));
        }

        private static IEnumerable<VerboseTransactionInfo> ReadWasabiCoinJoins()
        {
            return File.ReadAllLines(WasabiCoinJoinsPath).Select(x => RpcParser.VerboseTransactionInfoFromLine(x));
        }

        private static IEnumerable<VerboseTransactionInfo> ReadSamouraiCoinJoins()
        {
            return File.ReadAllLines(SamouraiCoinJoinsPath).Select(x => RpcParser.VerboseTransactionInfoFromLine(x));
        }

        private static IEnumerable<VerboseTransactionInfo> ReadOtherCoinJoins()
        {
            return File.ReadAllLines(OtherCoinJoinsPath).Select(x => RpcParser.VerboseTransactionInfoFromLine(x));
        }

        private static IEnumerable<VerboseTransactionInfo> ReadSamouraiTx0s()
        {
            return File.ReadAllLines(SamouraiTx0sPath).Select(x => RpcParser.VerboseTransactionInfoFromLine(x));
        }

        private static IEnumerable<VerboseTransactionInfo> ReadWasabi2PostMixTxs()
        {
            return File.ReadAllLines(Wasabi2PostMixTxsPath).Select(x => RpcParser.VerboseTransactionInfoFromLine(x));
        }

        private static IEnumerable<VerboseTransactionInfo> ReadWasabiPostMixTxs()
        {
            return File.ReadAllLines(WasabiPostMixTxsPath).Select(x => RpcParser.VerboseTransactionInfoFromLine(x));
        }

        private static IEnumerable<VerboseTransactionInfo> ReadSamouraiPostMixTxs()
        {
            return File.ReadAllLines(SamouraiPostMixTxsPath).Select(x => RpcParser.VerboseTransactionInfoFromLine(x));
        }

        private static IEnumerable<VerboseTransactionInfo> ReadOtherCoinJoinPostMixTxs()
        {
            return File.ReadAllLines(OtherCoinJoinPostMixTxsPath).Select(x => RpcParser.VerboseTransactionInfoFromLine(x));
        }

        private static ulong ReadBestHeight()
        {
            return ulong.Parse(File.ReadAllText(LastProcessedBlockHeightPath));
        }

        public static ScannerFiles Load()
        {
            return new ScannerFiles(
                ReadBestHeight(),
                ReadWasabi2CoinJoins(),
                ReadWasabiCoinJoins(),
                ReadSamouraiCoinJoins(),
                ReadOtherCoinJoins(),
                ReadSamouraiTx0s(),
                ReadWasabi2PostMixTxs(),
                ReadWasabiPostMixTxs(),
                ReadSamouraiPostMixTxs(),
                ReadOtherCoinJoinPostMixTxs());
        }

        private static ulong CalculateProcessedBlocks(ulong height, ulong bestHeight, ulong totalBlocks)
        {
            ulong blocksLeft = bestHeight - height;
            ulong processedBlocks = totalBlocks - blocksLeft;
            return processedBlocks;
        }

        private static IOrderedEnumerable<long> CreateWasabi2Denominations()
        {
            long maxSatoshis = 134375000000;
            long minSatoshis = 5000;
            var denominations = new HashSet<long>();

            // Powers of 2
            for (int i = 0; i < int.MaxValue; i++)
            {
                var denom = (long)Math.Pow(2, i);

                if (denom < minSatoshis)
                {
                    continue;
                }

                if (denom > maxSatoshis)
                {
                    break;
                }

                denominations.Add(denom);
            }

            // Powers of 3
            for (int i = 0; i < int.MaxValue; i++)
            {
                var denom = (long)Math.Pow(3, i);

                if (denom < minSatoshis)
                {
                    continue;
                }

                if (denom > maxSatoshis)
                {
                    break;
                }

                denominations.Add(denom);
            }

            // Powers of 3 * 2
            for (int i = 0; i < int.MaxValue; i++)
            {
                var denom = (long)Math.Pow(3, i) * 2;

                if (denom < minSatoshis)
                {
                    continue;
                }

                if (denom > maxSatoshis)
                {
                    break;
                }

                denominations.Add(denom);
            }

            // Powers of 10 (1-2-5 series)
            for (int i = 0; i < int.MaxValue; i++)
            {
                var denom = (long)Math.Pow(10, i);

                if (denom < minSatoshis)
                {
                    continue;
                }

                if (denom > maxSatoshis)
                {
                    break;
                }

                denominations.Add(denom);
            }

            // Powers of 10 * 2 (1-2-5 series)
            for (int i = 0; i < int.MaxValue; i++)
            {
                var denom = (long)Math.Pow(10, i) * 2;

                if (denom < minSatoshis)
                {
                    continue;
                }

                if (denom > maxSatoshis)
                {
                    break;
                }

                denominations.Add(denom);
            }

            // Powers of 10 * 5 (1-2-5 series)
            for (int i = 0; i < int.MaxValue; i++)
            {
                var denom = (long)Math.Pow(10, i) * 5;

                if (denom < minSatoshis)
                {
                    continue;
                }

                if (denom > maxSatoshis)
                {
                    break;
                }

                denominations.Add(denom);
            }

            return denominations.OrderByDescending(x => x);
        }
    }
}
