using Dumplings.Helpers;
using Dumplings.Rpc;
using Dumplings.Scanning;
using NBitcoin;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumplings.Stats
{
    public class Statista
    {
        public Statista(ScannerFiles scannerFiles, RPCClient rpc)
        {
            ScannerFiles = scannerFiles;
            Rpc = rpc;
        }

        public ScannerFiles ScannerFiles { get; }
        public RPCClient Rpc { get; }

        public void CalculateMonthlyVolumes()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, Money> otherCoinJoinVolumes = CalculateMonthlyVolumes(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, Money> wasabiVolumes = CalculateMonthlyVolumes(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> samouraiVolumes = CalculateMonthlyVolumes(ScannerFiles.SamouraiCoinJoins);
                DisplayResults(otherCoinJoinVolumes, wasabiVolumes, samouraiVolumes);
            }
        }

        public void CalculateNeverMixed()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, Money> freshOtherCoinJoins = CalculateNeverMixed(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, Money> freshWasabiCoinJoins = CalculateNeverMixed(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> freshSamouraiCoinJoins = CalculateNeverMixedFromTx0s(ScannerFiles.SamouraiCoinJoins, ScannerFiles.SamouraiTx0s);
                DisplayResults(freshOtherCoinJoins, freshWasabiCoinJoins, freshSamouraiCoinJoins);
            }
        }

        public void CalculateEquality()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, ulong> freshOtherCoinJoins = CalculateEquality(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, ulong> freshWasabiCoinJoins = CalculateEquality(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, ulong> freshSamouraiCoinJoins = CalculateEquality(ScannerFiles.SamouraiCoinJoins);
                DisplayResults(freshOtherCoinJoins, freshWasabiCoinJoins, freshSamouraiCoinJoins);
            }
        }

        public void CalculateFreshBitcoins()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, Money> freshOtherCoinJoins = CalculateFreshBitcoins(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, Money> freshWasabiCoinJoins = CalculateFreshBitcoins(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> freshSamouraiCoinJoins = CalculateFreshBitcoinsFromTX0s(ScannerFiles.SamouraiTx0s, ScannerFiles.SamouraiCoinJoinHashes);
                DisplayResults(freshOtherCoinJoins, freshWasabiCoinJoins, freshSamouraiCoinJoins);
            }
        }

        private void DisplayResults(Dictionary<YearMonth, Money> otheriResults, Dictionary<YearMonth, Money> wasabiResults, Dictionary<YearMonth, Money> samuriResults)
        {
            Console.WriteLine($"Month;Otheri;Wasabi;Samuri");

            foreach (var yearMonth in wasabiResults
                .Keys
                .Concat(otheriResults.Keys)
                .Concat(samuriResults.Keys)
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month))
            {
                if (!otheriResults.TryGetValue(yearMonth, out Money otheri))
                {
                    otheri = Money.Zero;
                }
                if (!wasabiResults.TryGetValue(yearMonth, out Money wasabi))
                {
                    wasabi = Money.Zero;
                }
                if (!samuriResults.TryGetValue(yearMonth, out Money samuri))
                {
                    samuri = Money.Zero;
                }

                Console.WriteLine($"{yearMonth};{otheri.ToDecimal(MoneyUnit.BTC):0};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0}");
            }
        }

        private void DisplayResults(Dictionary<YearMonth, ulong> otheriResults, Dictionary<YearMonth, ulong> wasabiResults, Dictionary<YearMonth, ulong> samuriResults)
        {
            Console.WriteLine($"Month;Otheri;Wasabi;Samuri");

            foreach (var yearMonth in wasabiResults
                .Keys
                .Concat(otheriResults.Keys)
                .Concat(samuriResults.Keys)
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month))
            {
                if (!otheriResults.TryGetValue(yearMonth, out ulong otheri))
                {
                    otheri = 0;
                }
                if (!wasabiResults.TryGetValue(yearMonth, out ulong wasabi))
                {
                    wasabi = 0;
                }
                if (!samuriResults.TryGetValue(yearMonth, out ulong samuri))
                {
                    samuri = 0;
                }

                Console.WriteLine($"{yearMonth};{otheri:0};{wasabi:0};{samuri:0}");
            }
        }

        private Dictionary<YearMonth, Money> CalculateMonthlyVolumes(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonth, Money>();

            foreach (var tx in txs)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);
                    var sum = tx.Outputs.Sum(x => x.Value);
                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonth, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, Money> CalculateFreshBitcoins(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonth, Money>();
            var txHashes = txs.Select(x => x.Id).ToHashSet();

            foreach (var tx in txs)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var sum = Money.Zero;
                    foreach (var input in tx.Inputs.Where(x => !txHashes.Contains(x.OutPoint.Hash)))
                    {
                        sum += input.PrevOutput.Value;
                    }

                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonth, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, Money> CalculateFreshBitcoinsFromTX0s(IEnumerable<VerboseTransactionInfo> tx0s, IEnumerable<uint256> cjHashes)
        {
            var myDic = new Dictionary<YearMonth, Money>();
            // In Samourai in order to identify fresh bitcoins the tx0 input shouldn't come from other samuri coinjoins, nor tx0s.
            var txHashes = tx0s.Select(x => x.Id).Union(cjHashes).ToHashSet();

            foreach (var tx in tx0s)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var sum = Money.Zero;
                    foreach (var input in tx.Inputs.Where(x => !txHashes.Contains(x.OutPoint.Hash)))
                    {
                        sum += input.PrevOutput.Value;
                    }

                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonth, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, Money> CalculateNeverMixed(IEnumerable<VerboseTransactionInfo> coinJoins)
        {
            // Go through all the coinjoins.
            // If a change output is spent and didn't go to coinjoins, then it didn't get remixed.
            var coinJoinInputs =
               coinJoins
                   .SelectMany(x => x.Inputs)
                   .Select(x => x.OutPoint)
                   .ToHashSet();

            var myDic = new Dictionary<YearMonth, Money>();
            VerboseTransactionInfo[] coinJoinsArray = coinJoins.ToArray();
            for (int i = 0; i < coinJoinsArray.Length; i++)
            {
                var reportProgress = ((i + 1) % 100) == 0;
                if (reportProgress)
                {
                    Logger.LogInfo($"{i + 1}/{coinJoinsArray.Length}");
                }
                VerboseTransactionInfo tx = coinJoinsArray[i];
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var sum = Money.Zero;
                    var changeOutputValues = tx.GetIndistinguishableOutputs(includeSingle: true).Where(x => x.count == 1).Select(x => x.value).ToHashSet();
                    VerboseOutputInfo[] outputArray = tx.Outputs.ToArray();
                    for (int j = 0; j < outputArray.Length; j++)
                    {
                        var output = outputArray[j];
                        // If it's a change and it didn't get remixed right away.
                        OutPoint outPoint = new OutPoint(tx.Id, j);
                        if (changeOutputValues.Contains(output.Value) && !coinJoinInputs.Contains(outPoint) && Rpc.GetTxOut(outPoint.Hash, (int)outPoint.N, includeMempool: false) is null)
                        {
                            sum += output.Value;
                        }
                    }

                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonth, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, Money> CalculateNeverMixedFromTx0s(IEnumerable<VerboseTransactionInfo> samuriCjs, IEnumerable<VerboseTransactionInfo> samuriTx0s)
        {
            // Go through all the outputs of TX0 transactions.
            // If an output is spent and didn't go to coinjoins or other TX0s, then it didn't get remixed.
            var samuriTx0CjInputs =
                samuriCjs
                    .SelectMany(x => x.Inputs)
                    .Select(x => x.OutPoint)
                    .Union(
                        samuriTx0s
                            .SelectMany(x => x.Inputs)
                            .Select(x => x.OutPoint))
                            .ToHashSet();

            var myDic = new Dictionary<YearMonth, Money>();
            VerboseTransactionInfo[] samuriTx0sArray = samuriTx0s.ToArray();
            for (int i = 0; i < samuriTx0sArray.Length; i++)
            {
                var reportProgress = ((i + 1) % 100) == 0;
                if (reportProgress)
                {
                    Logger.LogInfo($"{i + 1}/{samuriTx0sArray.Length}");
                }
                VerboseTransactionInfo tx = samuriTx0sArray[i];
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var sum = Money.Zero;
                    VerboseOutputInfo[] outputArray = tx.Outputs.ToArray();
                    for (int j = 0; j < outputArray.Length; j++)
                    {
                        var output = outputArray[j];
                        OutPoint outPoint = new OutPoint(tx.Id, j);
                        if (!samuriTx0CjInputs.Contains(outPoint) && Rpc.GetTxOut(outPoint.Hash, (int)outPoint.N, includeMempool: false) is null)
                        {
                            sum += output.Value;
                        }
                    }

                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonth, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, ulong> CalculateEquality(IEnumerable<VerboseTransactionInfo> coinJoins)
        {
            // CoinJoin Equality metric shows how much equality is gained for bitcoins. It is calculated separately to inputs and outputs and the results are added together.
            // For example if 2 people mix 10 bitcoins only on the output side, then CoinJoin Equality will be 2 * 1 * 10, beause 2 people mixed, both with 1 other person 10 bitcoins on the outputs ide.

            var myDic = new Dictionary<YearMonth, ulong>();
            foreach (var tx in coinJoins)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var equality = tx.CalculateCoinJoinEquality();

                    if (myDic.TryGetValue(yearMonth, out ulong current))
                    {
                        myDic[yearMonth] = current + equality;
                    }
                    else
                    {
                        myDic.Add(yearMonth, equality);
                    }
                }
            }

            return myDic;
        }
    }
}
