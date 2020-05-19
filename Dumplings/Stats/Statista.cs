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
                Dictionary<YearMonth, Money> wasabiVolumes = CalculateMonthlyVolumes(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> samouraiVolumes = CalculateMonthlyVolumes(ScannerFiles.SamouraiCoinJoins);
                Dictionary<YearMonth, Money> otherCoinJoinVolumes = CalculateMonthlyVolumes(ScannerFiles.OtherCoinJoins);

                Console.WriteLine($"Month;Wasabi;Samuri;Otheri");

                foreach (var yearMonth in wasabiVolumes
                    .Keys
                    .Concat(samouraiVolumes.Keys)
                    .Concat(otherCoinJoinVolumes.Keys)
                    .Distinct()
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month))
                {
                    if (!wasabiVolumes.TryGetValue(yearMonth, out Money wasabi))
                    {
                        wasabi = Money.Zero;
                    }
                    if (!samouraiVolumes.TryGetValue(yearMonth, out Money samuri))
                    {
                        samuri = Money.Zero;
                    }
                    if (!otherCoinJoinVolumes.TryGetValue(yearMonth, out Money otheri))
                    {
                        otheri = Money.Zero;
                    }

                    Console.WriteLine($"{yearMonth};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0};{otheri.ToDecimal(MoneyUnit.BTC):0}");
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
    }
}
