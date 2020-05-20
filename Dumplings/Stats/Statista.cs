using Dumplings.Helpers;
using Dumplings.Rpc;
using Dumplings.Scanning;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumplings.Stats
{
    public class Statista
    {
        public Statista(ScannerFiles scannerFiles)
        {
            ScannerFiles = scannerFiles;
        }

        public ScannerFiles ScannerFiles { get; }

        public void CalculateMonthlyVolumes()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, Money> otherCoinJoinVolumes = CalculateMonthlyVolumes(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, Money> wasabiVolumes = CalculateMonthlyVolumes(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> samouraiVolumes = CalculateMonthlyVolumes(ScannerFiles.SamouraiCoinJoins);

                Console.WriteLine($"Month;Otheri;Wasabi;Samuri");

                foreach (var yearMonth in wasabiVolumes
                    .Keys
                    .Concat(otherCoinJoinVolumes.Keys)
                    .Concat(samouraiVolumes.Keys)
                    .Distinct()
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month))
                {
                    if (!otherCoinJoinVolumes.TryGetValue(yearMonth, out Money otheri))
                    {
                        otheri = Money.Zero;
                    }
                    if (!wasabiVolumes.TryGetValue(yearMonth, out Money wasabi))
                    {
                        wasabi = Money.Zero;
                    }
                    if (!samouraiVolumes.TryGetValue(yearMonth, out Money samuri))
                    {
                        samuri = Money.Zero;
                    }

                    Console.WriteLine($"{yearMonth};{otheri.ToDecimal(MoneyUnit.BTC):0};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0}");
                }
            }
        }

        private static Dictionary<YearMonth, Money> CalculateMonthlyVolumes(IEnumerable<VerboseTransactionInfo> txs)
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

        public void CalculateFreshBitcoins()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, Money> freshOtherCoinJoins = CalculateFreshBitcoinsFromCoinJoins(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, Money> freshWasabiCoinJoins = CalculateFreshBitcoinsFromCoinJoins(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> freshSamouraiCoinJoins = CalculateFreshBitcoinsFromTX0s(ScannerFiles.SamouraiTx0s, ScannerFiles.SamouraiCoinJoinHashes);

                Console.WriteLine($"Month;Otheri;Wasabi;Samuri");

                foreach (var yearMonth in freshWasabiCoinJoins
                    .Keys
                    .Concat(freshOtherCoinJoins.Keys)
                    .Concat(freshSamouraiCoinJoins.Keys)
                    .Distinct()
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month))
                {
                    if (!freshOtherCoinJoins.TryGetValue(yearMonth, out Money otheri))
                    {
                        otheri = Money.Zero;
                    }
                    if (!freshWasabiCoinJoins.TryGetValue(yearMonth, out Money wasabi))
                    {
                        wasabi = Money.Zero;
                    }
                    if (!freshSamouraiCoinJoins.TryGetValue(yearMonth, out Money samuri))
                    {
                        samuri = Money.Zero;
                    }

                    Console.WriteLine($"{yearMonth};{otheri.ToDecimal(MoneyUnit.BTC):0};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0}");
                }
            }
        }

        private static Dictionary<YearMonth, Money> CalculateFreshBitcoinsFromCoinJoins(IEnumerable<VerboseTransactionInfo> txs)
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

        private static Dictionary<YearMonth, Money> CalculateFreshBitcoinsFromTX0s(IEnumerable<VerboseTransactionInfo> tx0s, IEnumerable<uint256> cjHashes)
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
    }
}
