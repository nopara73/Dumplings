using Dumplings.Displaying;
using Dumplings.Helpers;
using Dumplings.Rpc;
using Dumplings.Scanning;
using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using Dumplings.Cli;
using System.Data;

namespace Dumplings.Stats
{
    public class Statista
    {
        public Statista(ScannerFiles scannerFiles, RPCClient rpc)
        {
            ScannerFiles = scannerFiles;
            Rpc = rpc;
        }

        public RPCClient Rpc { get; }
        public ScannerFiles ScannerFiles { get; }

        public void CalculateAndUploadMonthlyVolumes()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, decimal> otheriResults = CalculateMonthlyVolumes(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, decimal> wasabiResults = CalculateMonthlyVolumes(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, decimal> wasabi2Results = CalculateMonthlyVolumes(ScannerFiles.Wasabi2CoinJoins);
                Dictionary<YearMonth, decimal> samuriResults = CalculateMonthlyVolumes(ScannerFiles.SamouraiCoinJoins);

                UploadToDatabase("MonthlyVolumes", wasabiResults, wasabi2Results, samuriResults, otheriResults);
            }
        }

        private void UploadToDatabase(string table, Dictionary<YearMonth, decimal> wasabiResults, Dictionary<YearMonth, decimal> wasabi2Results, Dictionary<YearMonth, decimal> samuriResults, Dictionary<YearMonth, decimal> otheriResults)
        {
            MySqlConnection conn = Connect.InitDb();
            foreach (var yearMonth in wasabi2Results
            .Keys
            .Concat(otheriResults.Keys)
            .Concat(samuriResults.Keys)
            .Concat(wasabiResults.Keys)
            .Distinct()
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month))
            {
                if (!otheriResults.TryGetValue(yearMonth, out decimal otheri))
                {
                    otheri = 0;
                }
                if (!wasabiResults.TryGetValue(yearMonth, out decimal wasabi))
                {
                    wasabi = 0;
                }
                if (!samuriResults.TryGetValue(yearMonth, out decimal samuri))
                {
                    samuri = 0;
                }
                if (!wasabi2Results.TryGetValue(yearMonth, out decimal wasabi2))
                {
                    wasabi2 = 0;
                }

                string check = $"CALL check{table}(@d);";
                MySqlCommand comm = new MySqlCommand(check, conn);
                comm.Parameters.AddWithValue("@d", DateTime.Parse($"{yearMonth}"));
                comm.Parameters["@d"].Direction = ParameterDirection.Input;
                conn.Open();
                MySqlDataReader reader = comm.ExecuteReader();
                bool write = false;
                while (reader.Read())
                {
                    if (reader[0].ToString() == "0")
                    {
                        write = true;
                    }
                }
                reader.Close();
                conn.Close();
                if (write)
                {
                    string sql = $"CALL store{table}(@d,@w,@w2,@s,@o);";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@d", DateTime.Parse($"{yearMonth}"));
                    cmd.Parameters["@d"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@w", wasabi);
                    cmd.Parameters["@w"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@w2", wasabi2);
                    cmd.Parameters["@w2"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@s", samuri);
                    cmd.Parameters["@s"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@o", otheri);
                    cmd.Parameters["@o"].Direction = ParameterDirection.Input;
                    conn.Open();
                    int res = cmd.ExecuteNonQuery();
                    conn.Close();
                }
                else
                {
                    string sql = $"CALL update{table}(@d,@w,@w2,@s,@o);";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@d", DateTime.Parse($"{yearMonth}"));
                    cmd.Parameters["@d"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@w", wasabi);
                    cmd.Parameters["@w"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@w2", wasabi2);
                    cmd.Parameters["@w2"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@s", samuri);
                    cmd.Parameters["@s"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@o", otheri);
                    cmd.Parameters["@o"].Direction = ParameterDirection.Input;
                    conn.Open();
                    int res = cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void CalculateMonthlyEqualVolumes()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, Money> otheri = CalculateMonthlyEqualVolumes(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, Money> wasabi2 = CalculateMonthlyEqualVolumes(ScannerFiles.Wasabi2CoinJoins);
                Dictionary<YearMonth, Money> wasabi = CalculateMonthlyEqualVolumes(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> samuri = CalculateMonthlyEqualVolumes(ScannerFiles.SamouraiCoinJoins);
                Display.DisplayOtheriWasabiSamuriResults(otheri, wasabi2, wasabi, samuri);
            }
        }

        public void CalculateAndUploadNeverMixed()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, decimal> otheri = CalculateNeverMixed(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, decimal> wasabi2 = CalculateNeverMixed(ScannerFiles.Wasabi2CoinJoins);
                Dictionary<YearMonth, decimal> wasabi = CalculateNeverMixed(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, decimal> samuri = CalculateNeverMixedFromTx0s(ScannerFiles.SamouraiCoinJoins, ScannerFiles.SamouraiTx0s);

                UploadToDatabase("NeverMixed", wasabi, wasabi2, samuri, otheri);
            }
        }

        public void CalculateEquality()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, ulong> otheri = CalculateEquality(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, ulong> wasabi2 = CalculateEquality(ScannerFiles.Wasabi2CoinJoins);
                Dictionary<YearMonth, ulong> wasabi = CalculateEquality(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, ulong> samuri = CalculateEquality(ScannerFiles.SamouraiCoinJoins);
                Display.DisplayOtheriWasabiSamuriResults(otheri, wasabi2, wasabi, samuri);
            }
        }

        public void CalculateAndUploadPostMixConsolidation()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, decimal> otheri = CalculateAveragePostMixInputs(ScannerFiles.OtherCoinJoinPostMixTxs);
                Dictionary<YearMonth, decimal> wasabi2 = CalculateAveragePostMixInputs(ScannerFiles.Wasabi2PostMixTxs);
                Dictionary<YearMonth, decimal> wasabi = CalculateAveragePostMixInputs(ScannerFiles.WasabiPostMixTxs);
                Dictionary<YearMonth, decimal> samuri = CalculateAveragePostMixInputs(ScannerFiles.SamouraiPostMixTxs);

                UploadToDatabase("PostMixConsolidation", wasabi, wasabi2, samuri, otheri);
            }
        }

        public void CalculateSmallerThanMinimumWasabiInputs()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, decimal> wasabi = CalculateSmallerThanMinimumWasabiInputs(ScannerFiles.WasabiCoinJoins);
                Display.DisplayWasabiResults(wasabi);
            }
        }

        public void CalculateIncome()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, Money> wasabi = CalculateWasabiIncome(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, Money> samuri = CalculateSamuriIncome(ScannerFiles.SamouraiTx0s);
                Display.DisplayWasabiSamuriResults(wasabi, samuri);
            }
        }

        public void CalculateAndUploadFreshBitcoins()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonth, decimal> otheriResults = CalculateFreshBitcoins(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonth, decimal> wasabi2Results = CalculateFreshBitcoins(ScannerFiles.Wasabi2CoinJoins);
                Dictionary<YearMonth, decimal> wasabiResults = CalculateFreshBitcoins(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonth, decimal> samuriResults = CalculateFreshBitcoinsFromTX0s(ScannerFiles.SamouraiTx0s, ScannerFiles.SamouraiCoinJoinHashes);

                UploadToDatabase("FreshCoins", wasabiResults, wasabi2Results, samuriResults, otheriResults);
            }
        }

        public void CalculateUniqueCountPercent()
        {
            var uniqueCountPercents = new Dictionary<YearMonthDay, List<(int uniqueOutCount, int uniqueInCount, double uniqueOutCountPercent, double uniqueInCountPercent)>>();

            // IsWasabi2Cj is because there were false positives and I don't want to spend a week to run the algo from the beginning to scan everything.
            foreach (var cj in ScannerFiles.Wasabi2CoinJoins.Where(x => x.IsWasabi2Cj()))
            {
                int uniqueOutCount = cj.GetIndistinguishableOutputs(includeSingle: true).Count(x => x.count == 1);
                int uniqueInCount = cj.GetIndistinguishableInputs(includeSingle: true).Count(x => x.count == 1);

                double uniqueOutCountPercent = uniqueOutCount / (cj.Outputs.Count() / 100d);
                double uniqueInCountPercent = uniqueInCount / (cj.Inputs.Count() / 100d);

                var key = cj.BlockInfo.YearMonthDay;
                if (uniqueCountPercents.ContainsKey(key))
                {
                    uniqueCountPercents[key].Add((uniqueOutCount, uniqueInCount, uniqueOutCountPercent, uniqueInCountPercent));
                }
                else
                {
                    uniqueCountPercents.Add(key, new List<(int uniqueOutCount, int uniqueInCount, double uniqueOutCountPercent, double uniqueInCountPercent)> { (uniqueOutCount, uniqueInCount, uniqueOutCountPercent, uniqueInCountPercent) });
                }
            }

            Display.DisplayOtheriWasabiSamuriResults(uniqueCountPercents);
        }

        public void CalculateRecords()
        {
            var mostInputs = new Dictionary<int, VerboseTransactionInfo>();
            var mostOutputs = new Dictionary<int, VerboseTransactionInfo>();
            var mostInputsAndOutputs = new Dictionary<int, VerboseTransactionInfo>();
            var largestVolumes = new Dictionary<Money, VerboseTransactionInfo>();
            var largestCjEqualities = new Dictionary<ulong, VerboseTransactionInfo>();

            var smallestUnequalOutputs = new Dictionary<int, VerboseTransactionInfo>();
            var smallestUnequalInputs = new Dictionary<int, VerboseTransactionInfo>();

            // IsWasabi2Cj is because there were false positives and I don't want to spend a week to run the algo from the beginning to scan everything.
            foreach (var cj in ScannerFiles.Wasabi2CoinJoins.Where(x => x.IsWasabi2Cj()))
            {
                var inputCount = cj.Inputs.Count();
                var outputCount = cj.Outputs.Count();
                var inOutSum = inputCount + outputCount;
                var totalVolume = cj.Inputs.Sum(x => x.PrevOutput.Value);
                var cjEquality = cj.CalculateCoinJoinEquality();

                var unequalOutputs = cj.GetIndistinguishableOutputs(true).Count(x => x.count == 1);
                var unequalInputs = cj.GetIndistinguishableInputs(true).Count(x => x.count == 1);
                var unequalOutputVol = cj.GetIndistinguishableOutputs(true).Where(x => x.count == 1).Sum(x => x.value);
                var unequalInputVol = cj.GetIndistinguishableInputs(true).Where(x => x.count == 1).Sum(x => x.value);

                if (!mostInputs.Keys.Any(x => x >= inputCount))
                {
                    mostInputs.Add(inputCount, cj);
                }
                if (!mostOutputs.Keys.Any(x => x >= outputCount))
                {
                    mostOutputs.Add(outputCount, cj);
                }
                if (!mostInputsAndOutputs.Keys.Any(x => x >= inOutSum))
                {
                    mostInputsAndOutputs.Add(inOutSum, cj);
                }
                if (!largestVolumes.Keys.Any(x => x >= totalVolume))
                {
                    largestVolumes.Add(totalVolume, cj);
                }
                if (!largestCjEqualities.Keys.Any(x => x >= cjEquality))
                {
                    largestCjEqualities.Add(cjEquality, cj);
                }

                if (!smallestUnequalOutputs.Keys.Any(x => x <= unequalOutputs))
                {
                    smallestUnequalOutputs.Add(unequalOutputs, cj);
                }
                if (!smallestUnequalInputs.Keys.Any(x => x <= unequalInputs))
                {
                    smallestUnequalInputs.Add(unequalInputs, cj);
                }
            }

            Display.DisplayRecords(mostInputs, mostOutputs, mostInputsAndOutputs, largestVolumes, largestCjEqualities, smallestUnequalOutputs, smallestUnequalInputs);
        }

        public void CalculateFreshBitcoinAmounts()
        {
            var wasabi = GetFreshBitcoinAmounts(ScannerFiles.WasabiCoinJoins);
            var lastAmounts = wasabi.SelectMany(x => x.Value).Select(x => x.ToDecimal(MoneyUnit.BTC)).Reverse().Take(100000).Reverse().ToArray();
            Console.WriteLine("Calculations for the last 100 000 Wasabi coinjoins' fresh bitcoin input amounts.");
            Console.WriteLine($"Average:    {lastAmounts.Average()}");
            Console.WriteLine($"Median:     {lastAmounts.Median()}");
            Console.WriteLine($"8 Decimals: {lastAmounts.Where(x => BitConverter.GetBytes(decimal.GetBits(x)[3])[2] == 8).Count()}");
            Console.WriteLine($"7 Decimals: {lastAmounts.Where(x => BitConverter.GetBytes(decimal.GetBits(x)[3])[2] == 7).Count()}");
            Console.WriteLine($"6 Decimals: {lastAmounts.Where(x => BitConverter.GetBytes(decimal.GetBits(x)[3])[2] == 6).Count()}");
            Console.WriteLine($"5 Decimals: {lastAmounts.Where(x => BitConverter.GetBytes(decimal.GetBits(x)[3])[2] == 5).Count()}");
            Console.WriteLine($"4 Decimals: {lastAmounts.Where(x => BitConverter.GetBytes(decimal.GetBits(x)[3])[2] == 4).Count()}");
            Console.WriteLine($"3 Decimals: {lastAmounts.Where(x => BitConverter.GetBytes(decimal.GetBits(x)[3])[2] == 3).Count()}");
            Console.WriteLine($"2 Decimals: {lastAmounts.Where(x => BitConverter.GetBytes(decimal.GetBits(x)[3])[2] == 2).Count()}");
            Console.WriteLine($"1 Decimals: {lastAmounts.Where(x => BitConverter.GetBytes(decimal.GetBits(x)[3])[2] == 1).Count()}");
            Console.WriteLine($"0 Decimals: {lastAmounts.Where(x => BitConverter.GetBytes(decimal.GetBits(x)[3])[2] == 0).Count()}");
            Console.WriteLine($"Average Hamming Weight: {lastAmounts.Select(x => ((ulong)(x * 100000000)).HammingWeight()).Average()}");
            Console.WriteLine($"Median Hamming Weight:  {lastAmounts.Select(x => ((ulong)(x * 100000000)).HammingWeight()).Median()}");
        }

        public void CalculateMonthlyAverageMonthlyUserCounts()
        {
            using (BenchmarkLogger.Measure())
            {
                IDictionary<YearMonth, int> otheri = CalculateAverageUserCounts(ScannerFiles.OtherCoinJoins);
                IDictionary<YearMonth, int> wasabi = CalculateAverageUserCounts(ScannerFiles.WasabiCoinJoins);
                IDictionary<YearMonth, int> samuri = CalculateAverageUserCounts(ScannerFiles.SamouraiCoinJoins);
                Display.DisplayOtheriWasabiSamuriResults(otheri, wasabi, samuri);
            }
        }

        public void CalculateMonthlyNetworkFeePaidByUserPerCoinjoin()
        {
            using (BenchmarkLogger.Measure())
            {
                IDictionary<YearMonth, Money> otheri = CalculateAverageNetworkFeePaidByUserPerCoinjoin(ScannerFiles.OtherCoinJoins);
                IDictionary<YearMonth, Money> wasabi = CalculateAverageNetworkFeePaidByUserPerCoinjoin(ScannerFiles.WasabiCoinJoins);
                IDictionary<YearMonth, Money> samuri = CalculateAverageNetworkFeePaidByUserPerCoinjoin(ScannerFiles.SamouraiCoinJoins);
                Display.DisplayOtheriWasabiSamuriResults(otheri, wasabi, samuri);
            }
        }

        private IDictionary<YearMonth, Money> CalculateAverageNetworkFeePaidByUserPerCoinjoin(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonth, List<Money>>();
            foreach (var tx in txs)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);
                    var userCount = tx.GetIndistinguishableOutputs(includeSingle: false).OrderByDescending(x => x.count).First().count;
                    var feePerUser = tx.NetworkFee / userCount;

                    if (myDic.TryGetValue(yearMonth, out var current))
                    {
                        myDic[yearMonth].Add(feePerUser);
                    }
                    else
                    {
                        myDic.Add(yearMonth, new List<Money> { feePerUser });
                    }
                }
            }

            var retDic = new Dictionary<YearMonth, Money>();
            foreach (var kv in myDic)
            {
                decimal decVal = kv.Value.Select(x => x.ToDecimal(MoneyUnit.BTC)).Average();
                Money val = Money.Coins(decVal);
                retDic.Add(kv.Key, val);
            }
            return retDic;
        }

        private IDictionary<YearMonth, int> CalculateAverageUserCounts(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonth, List<int>>();
            foreach (var tx in txs)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);
                    var userCount = tx.GetIndistinguishableOutputs(includeSingle: false).OrderByDescending(x => x.count).First().count;

                    if (myDic.TryGetValue(yearMonth, out var current))
                    {
                        myDic[yearMonth].Add(userCount);
                    }
                    else
                    {
                        myDic.Add(yearMonth, new List<int> { userCount });
                    }
                }
            }

            var retDic = new Dictionary<YearMonth, int>();
            foreach (var kv in myDic)
            {
                retDic.Add(kv.Key, (int)kv.Value.Average());
            }
            return retDic;
        }

        public void CalculateFreshBitcoinsDaily()
        {
            using (BenchmarkLogger.Measure())
            {
                Dictionary<YearMonthDay, Money> otheri = CalculateFreshBitcoinsDaily(ScannerFiles.OtherCoinJoins);
                Dictionary<YearMonthDay, Money> wasabi2 = CalculateFreshBitcoinsDaily(ScannerFiles.Wasabi2CoinJoins);
                Dictionary<YearMonthDay, Money> wasabi = CalculateFreshBitcoinsDaily(ScannerFiles.WasabiCoinJoins);
                Dictionary<YearMonthDay, Money> samuri = CalculateFreshBitcoinsDailyFromTX0s(ScannerFiles.SamouraiTx0s, ScannerFiles.SamouraiCoinJoinHashes);
                Display.DisplayOtheriWasabiSamuriResults(otheri, wasabi2, wasabi, samuri);
            }
        }

        private Dictionary<YearMonth, decimal> CalculateMonthlyVolumes(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonth, decimal>();

            foreach (var tx in txs)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);
                    decimal sum = tx.Outputs.Sum(x => x.Value.ToDecimal(MoneyUnit.BTC));
                    if (myDic.TryGetValue(yearMonth, out decimal current))
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

        private Dictionary<YearMonth, Money> CalculateMonthlyEqualVolumes(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonth, Money>();

            foreach (var tx in txs)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);
                    var sum = tx.GetIndistinguishableOutputs(includeSingle: false).Sum(x => x.value * x.count);
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

        private Dictionary<YearMonth, decimal> CalculateFreshBitcoins(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonth, decimal>();
            foreach (var day in CalculateFreshBitcoinsDaily(txs))
            {
                var yearMonth = day.Key.ToYearMonth();
                decimal sum = day.Value.ToDecimal(MoneyUnit.BTC);
                if (myDic.TryGetValue(yearMonth, out decimal current))
                {
                    myDic[yearMonth] = current + sum;
                }
                else
                {
                    myDic.Add(yearMonth, sum);
                }
            }
            return myDic;
        }

        private Dictionary<YearMonth, decimal> CalculateFreshBitcoinsFromTX0s(IEnumerable<VerboseTransactionInfo> tx0s, IEnumerable<uint256> cjHashes)
        {
            var myDic = new Dictionary<YearMonth, decimal>();
            foreach (var day in CalculateFreshBitcoinsDailyFromTX0s(tx0s, cjHashes))
            {
                var yearMonth = day.Key.ToYearMonth();
                decimal sum = day.Value.ToDecimal(MoneyUnit.BTC);
                if (myDic.TryGetValue(yearMonth, out decimal current))
                {
                    myDic[yearMonth] = current + sum;
                }
                else
                {
                    myDic.Add(yearMonth, sum);
                }
            }
            return myDic;
        }

        private Dictionary<YearMonthDay, Money> CalculateFreshBitcoinsDaily(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonthDay, Money>();
            var txHashes = txs.Select(x => x.Id).ToHashSet();

            foreach (var tx in txs)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonthDay = new YearMonthDay(blockTimeValue.Year, blockTimeValue.Month, blockTimeValue.Day);

                    var sum = Money.Zero;
                    foreach (var input in tx.Inputs.Where(x => !txHashes.Contains(x.OutPoint.Hash)))
                    {
                        sum += input.PrevOutput.Value;
                    }

                    if (myDic.TryGetValue(yearMonthDay, out Money current))
                    {
                        myDic[yearMonthDay] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonthDay, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonthDay, Money> CalculateFreshBitcoinsDailyFromTX0s(IEnumerable<VerboseTransactionInfo> tx0s, IEnumerable<uint256> cjHashes)
        {
            var myDic = new Dictionary<YearMonthDay, Money>();
            // In Samourai in order to identify fresh bitcoins the tx0 input shouldn't come from other samuri coinjoins, nor tx0s.
            var txHashes = tx0s.Select(x => x.Id).Union(cjHashes).ToHashSet();

            foreach (var tx in tx0s)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonthDay = new YearMonthDay(blockTimeValue.Year, blockTimeValue.Month, blockTimeValue.Day);

                    var sum = Money.Zero;
                    foreach (var input in tx.Inputs.Where(x => !txHashes.Contains(x.OutPoint.Hash)))
                    {
                        sum += input.PrevOutput.Value;
                    }

                    if (myDic.TryGetValue(yearMonthDay, out Money current))
                    {
                        myDic[yearMonthDay] = current + sum;
                    }
                    else
                    {
                        myDic.Add(yearMonthDay, sum);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, List<Money>> GetFreshBitcoinAmounts(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonth, List<Money>>();
            foreach (var day in GetFreshBitcoinAmountsDaily(txs))
            {
                var yearMonth = day.Key.ToYearMonth();
                if (myDic.ContainsKey(yearMonth))
                {
                    myDic[yearMonth].AddRange(day.Value);
                }
                else
                {
                    var l = new List<Money>();
                    l.AddRange(day.Value);
                    myDic.Add(yearMonth, l);
                }
            }
            return myDic;
        }

        private Dictionary<YearMonthDay, List<Money>> GetFreshBitcoinAmountsDaily(IEnumerable<VerboseTransactionInfo> txs)
        {
            var myDic = new Dictionary<YearMonthDay, List<Money>>();
            var txHashes = txs.Select(x => x.Id).ToHashSet();

            foreach (var tx in txs)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonthDay = new YearMonthDay(blockTimeValue.Year, blockTimeValue.Month, blockTimeValue.Day);

                    var amounts = new List<Money>();
                    foreach (var input in tx.Inputs.Where(x => !txHashes.Contains(x.OutPoint.Hash)))
                    {
                        if (input.PrevOutput.Value.ToDecimal(MoneyUnit.BTC) == 0.05005067m)
                        {
                            ;
                        }
                        amounts.Add(input.PrevOutput.Value);
                    }

                    if (myDic.ContainsKey(yearMonthDay))
                    {
                        myDic[yearMonthDay].AddRange(amounts);
                    }
                    else
                    {
                        myDic.Add(yearMonthDay, amounts);
                    }
                }
            }

            return myDic;
        }

        private Dictionary<YearMonth, decimal> CalculateNeverMixed(IEnumerable<VerboseTransactionInfo> coinJoins)
        {
            BitcoinStatus.CheckAsync(Rpc).GetAwaiter().GetResult();
            // Go through all the coinjoins.
            // If a change output is spent and didn't go to coinjoins, then it didn't get remixed.
            var coinJoinInputs =
               coinJoins
                   .SelectMany(x => x.Inputs)
                   .Select(x => x.OutPoint)
                   .ToHashSet();

            var myDic = new Dictionary<YearMonth, decimal>();
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

                    var sum = 0m;
                    var changeOutputValues = tx.GetIndistinguishableOutputs(includeSingle: true).Where(x => x.count == 1).Select(x => x.value).ToHashSet();
                    VerboseOutputInfo[] outputArray = tx.Outputs.ToArray();
                    for (int j = 0; j < outputArray.Length; j++)
                    {
                        var output = outputArray[j];
                        // If it's a change and it didn't get remixed right away.
                        OutPoint outPoint = new OutPoint(tx.Id, j);
                        if (changeOutputValues.Contains(output.Value) && !coinJoinInputs.Contains(outPoint) && Rpc.GetTxOut(outPoint.Hash, (int)outPoint.N, includeMempool: false) is null)
                        {
                            sum += output.Value.ToDecimal(MoneyUnit.BTC);
                        }
                    }

                    if (myDic.TryGetValue(yearMonth, out decimal current))
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

        private Dictionary<YearMonth, decimal> CalculateNeverMixedFromTx0s(IEnumerable<VerboseTransactionInfo> samuriCjs, IEnumerable<VerboseTransactionInfo> samuriTx0s)
        {
            BitcoinStatus.CheckAsync(Rpc).GetAwaiter().GetResult();

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

            var myDic = new Dictionary<YearMonth, decimal>();
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

                    var sum = 0m;
                    VerboseOutputInfo[] outputArray = tx.Outputs.ToArray();
                    for (int j = 0; j < outputArray.Length; j++)
                    {
                        var output = outputArray[j];
                        OutPoint outPoint = new OutPoint(tx.Id, j);
                        if (!samuriTx0CjInputs.Contains(outPoint) && Rpc.GetTxOut(outPoint.Hash, (int)outPoint.N, includeMempool: false) is null)
                        {
                            sum += output.Value.ToDecimal(MoneyUnit.BTC);
                        }
                    }

                    if (myDic.TryGetValue(yearMonth, out decimal current))
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
            // For example if 3 people mix 10 bitcoins only on the output side, then CoinJoin Equality will be 3^2 * 10.

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

        private Dictionary<YearMonth, decimal> CalculateAveragePostMixInputs(IEnumerable<VerboseTransactionInfo> postMixes)
        {
            var myDic = new Dictionary<YearMonth, (int totalTxs, int totalIns, decimal avg)>();
            foreach (var tx in postMixes)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    int ttxs = 1;
                    int tins = tx.Inputs.Count();
                    decimal average = (decimal)tins / ttxs;

                    if (myDic.TryGetValue(yearMonth, out (int totalTxs, int totalIns, decimal) current))
                    {
                        ttxs = current.totalTxs + 1;
                        tins = current.totalIns + tins;
                        average = (decimal)tins / ttxs;
                        myDic[yearMonth] = (ttxs, tins, average);
                    }
                    else
                    {
                        myDic.Add(yearMonth, (ttxs, tins, average));
                    }
                }
            }

            var retDic = new Dictionary<YearMonth, decimal>();
            foreach (var kv in myDic)
            {
                retDic.Add(kv.Key, kv.Value.avg);
            }
            return retDic;
        }

        private Dictionary<YearMonth, decimal> CalculateSmallerThanMinimumWasabiInputs(IEnumerable<VerboseTransactionInfo> postMixes)
        {
            var myDic = new Dictionary<YearMonth, (int totalInputs, int totalSmallerInputs)>();
            foreach (var tx in postMixes)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var (value, tpc) = tx.GetIndistinguishableOutputs(includeSingle: false).OrderByDescending(x => x.count).First();
                    var almostValue = value - Money.Coins(0.0001m);
                    var smallerSum = tx.Outputs.Select(x => x.Value).Where(x => x < almostValue).Sum();
                    var spc = (int)(smallerSum.Satoshi / value.Satoshi);

                    if (myDic.TryGetValue(yearMonth, out (int tins, int tsins) current))
                    {
                        tpc += current.tins;
                        spc += current.tsins;
                        myDic[yearMonth] = (tpc, spc);
                    }
                    else
                    {
                        myDic.Add(yearMonth, (tpc, spc));
                    }
                }
            }

            var retDic = new Dictionary<YearMonth, decimal>();
            foreach (var kv in myDic)
            {
                var perc = (decimal)kv.Value.totalSmallerInputs / kv.Value.totalInputs;
                retDic.Add(kv.Key, perc);
            }
            return retDic;
        }

        private Dictionary<YearMonth, Money> CalculateWasabiIncome(IEnumerable<VerboseTransactionInfo> coinJoins)
        {
            var myDic = new Dictionary<YearMonth, Money>();
            foreach (var tx in coinJoins)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var sum = Money.Zero;
                    foreach (var output in tx.Outputs.Where(x => Constants.WasabiCoordScripts.Contains(x.ScriptPubKey)))
                    {
                        sum += output.Value;
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

        private Dictionary<YearMonth, Money> CalculateSamuriIncome(IEnumerable<VerboseTransactionInfo> tx0s)
        {
            var myDic = new Dictionary<YearMonth, Money>();
            foreach (var tx in tx0s)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    var fee = Money.Zero;
                    var equalOutputValues = tx.GetIndistinguishableOutputs(false).Select(x => x.value).ToHashSet();
                    var feeCandidates = tx.Outputs.Where(x => !equalOutputValues.Contains(x.Value) && !TxNullDataTemplate.Instance.CheckScriptPubKey(x.ScriptPubKey));
                    if (feeCandidates.Count() == 1)
                    {
                        fee = feeCandidates.First().Value;
                    }
                    else
                    {
                        if (equalOutputValues.Count == 0)
                        {
                            List<VerboseOutputInfo> closeEnoughs = FindMostLikelyMixOutputs(feeCandidates, 10);

                            if (closeEnoughs.Any()) // There are like 5 tx from old time, I guess just experiemtns where it's not found.
                            {
                                var closeEnough = closeEnoughs.First().Value;
                                var expectedMaxFee = closeEnough.Percentage(6m); // They do some discounts to ruin user privacy.
                                var closest = feeCandidates.Where(x => x.Value < expectedMaxFee && x.Value != closeEnough).OrderByDescending(x => x.Value).FirstOrDefault();
                                if (closest is { }) // There's no else here.
                                {
                                    fee = closest.Value;
                                }
                            }
                        }
                        else if (equalOutputValues.Count == 1)
                        {
                            var poolDenomination = equalOutputValues.First();
                            var expectedMaxFee = poolDenomination.Percentage(6m); // They do some discounts to ruin user privacy.
                            var closest = feeCandidates.Where(x => x.Value < expectedMaxFee).OrderByDescending(x => x.Value).FirstOrDefault();
                            if (closest is { })
                            {
                                fee = closest.Value;
                            }
                        }
                    }

                    if (myDic.TryGetValue(yearMonth, out Money current))
                    {
                        myDic[yearMonth] = current + fee;
                    }
                    else
                    {
                        myDic.Add(yearMonth, fee);
                    }
                }
            }

            return myDic;
        }

        private static List<VerboseOutputInfo> FindMostLikelyMixOutputs(IEnumerable<VerboseOutputInfo> feeCandidates, int percentagePrecision)
        {
            var closeEnoughs = new List<VerboseOutputInfo>();
            foreach (var denom in Constants.SamouraiPools)
            {
                var found = feeCandidates.FirstOrDefault(x => x.Value.Almost(denom, denom.Percentage(percentagePrecision)));
                if (found is { })
                {
                    closeEnoughs.Add(found);
                }
            }

            if (closeEnoughs.Count > 1)
            {
                var newCloseEnoughs = FindMostLikelyMixOutputs(closeEnoughs, percentagePrecision - 1);
                if (newCloseEnoughs.Count == 0)
                {
                    closeEnoughs = closeEnoughs.Take(1).ToList();
                }
                else
                {
                    closeEnoughs = newCloseEnoughs.ToList();
                }
            }

            return closeEnoughs;
        }

        public void ListFreshBitcoins()
        {
            using (BenchmarkLogger.Measure())
            {
                ListFreshBitcoins("freshotheri.txt", ScannerFiles.OtherCoinJoins);
                ListFreshBitcoins("freshwasabi2.txt", ScannerFiles.Wasabi2CoinJoins);
                ListFreshBitcoins("freshwasabi.txt", ScannerFiles.WasabiCoinJoins);
                ListFreshBitcoins("freshsamuri.txt", ScannerFiles.SamouraiTx0s, ScannerFiles.SamouraiCoinJoinHashes);
            }
        }

        private void ListFreshBitcoins(string filePath, IEnumerable<VerboseTransactionInfo> samouraiTx0s, IEnumerable<uint256> samouraiCoinJoinHashes)
        {
            // In Samourai in order to identify fresh bitcoins the tx0 input shouldn't come from other samuri coinjoins, nor tx0s.
            var txHashes = samouraiTx0s.Select(x => x.Id).Union(samouraiCoinJoinHashes).ToHashSet();

            foreach (var tx in samouraiTx0s)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    File.AppendAllLines(filePath, tx.Inputs.Where(x => !txHashes.Contains(x.OutPoint.Hash)).Select(x => new Coin(blockTime.Value, x.OutPoint.Hash, x.OutPoint.N, x.PrevOutput.ScriptPubKey, x.PrevOutput.Value).ToString()));
                }
            }
        }

        private void ListFreshBitcoins(string filePath, IEnumerable<VerboseTransactionInfo> coinjoins)
        {
            var txHashes = coinjoins.Select(x => x.Id).ToHashSet();

            foreach (var tx in coinjoins)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    File.AppendAllLines(filePath, tx.Inputs.Where(x => !txHashes.Contains(x.OutPoint.Hash)).Select(x => new Coin(blockTime.Value, x.OutPoint.Hash, x.OutPoint.N, x.PrevOutput.ScriptPubKey, x.PrevOutput.Value).ToString()));
                }
            }
        }

        public void CalculateUnspentCapacity(RPCClient rpc)
        {
            var ucWW1 = Money.Zero;
            var ucWW2 = Money.Zero;
            var ucSW = Money.Zero;

            YearMonthDay prevYMD = null;
            foreach (var tx in ScannerFiles.WasabiCoinJoins
                .Concat(ScannerFiles.Wasabi2CoinJoins)
                .Concat(ScannerFiles.SamouraiCoinJoins)
                .OrderBy(x => x.BlockInfo.BlockTime))
            {
                if (prevYMD is null)
                {
                    prevYMD = tx.BlockInfo.YearMonthDay;
                }
                else if (prevYMD != tx.BlockInfo.YearMonthDay)
                {
                    Console.WriteLine($"{prevYMD}\t{ucWW2.ToString(false, false)}\t{ucWW1.ToString(false, false)}\t{ucSW.ToString(false, false)}");
                    prevYMD = tx.BlockInfo.YearMonthDay;
                }

                var outs = tx.Outputs.ToArray();
                for (int i = 0; i < outs.Length; i++)
                {
                    var o = outs[i];
                    if (rpc.GetTxOut(tx.Id, i) is not null)
                    {
                        if (ScannerFiles.WasabiCoinJoinHashes.Contains(tx.Id))
                        {
                            ucWW1 += o.Value;
                        }
                        else if (ScannerFiles.Wasabi2CoinJoinHashes.Contains(tx.Id))
                        {
                            ucWW2 += o.Value;
                        }
                        else if (ScannerFiles.SamouraiCoinJoinHashes.Contains(tx.Id))
                        {
                            ucSW += o.Value;
                        }
                    }
                }
            }

            Console.WriteLine($"{prevYMD}\t{ucWW2.ToString(false, false)}\t{ucWW1.ToString(false, false)}\t{ucSW.ToString(false, false)}");
        }

        public void CalculateAndUploadMonthlyCoinJoins()
        {
            Dictionary<YearMonth, decimal> wasabiResults = CalculateCoinJoinsPerMonth(ScannerFiles.WasabiCoinJoins);
            Dictionary<YearMonth, decimal> wasabi2Results = CalculateCoinJoinsPerMonth(ScannerFiles.Wasabi2CoinJoins);
            Dictionary<YearMonth, decimal> samuriResults = CalculateCoinJoinsPerMonth(ScannerFiles.SamouraiCoinJoins);
            Dictionary<YearMonth, decimal> otheriResults = CalculateCoinJoinsPerMonth(ScannerFiles.OtherCoinJoins);

            UploadToDatabase("MonthlyCoinJoins", wasabiResults, wasabi2Results, samuriResults, otheriResults);
        }

        private Dictionary<YearMonth, decimal> CalculateCoinJoinsPerMonth(IEnumerable<VerboseTransactionInfo> coinJoins)
        {
            var myDic = new Dictionary<YearMonth, decimal>();

            foreach (var tx in coinJoins)
            {
                var blockTime = tx.BlockInfo.BlockTime;
                if (blockTime.HasValue)
                {
                    var blockTimeValue = blockTime.Value;
                    var yearMonth = new YearMonth(blockTimeValue.Year, blockTimeValue.Month);

                    if (myDic.TryGetValue(yearMonth, out decimal current))
                    {
                        myDic[yearMonth] = current + 1;
                    }
                    else
                    {
                        myDic.Add(yearMonth, 1);
                    }
                }
            }
            return myDic;
        }

        public void CalculateWasabiCoordStats(ExtPubKey[] xpubs)
        {
            using (BenchmarkLogger.Measure())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                var scripts = Constants.WasabiCoordScripts.ToHashSet();
                foreach (var xpub in xpubs)
                {
                    for (int i = 0; i < 100_000; i++)
                    {
                        scripts.Add(xpub.Derive(0, false).Derive(i, false).PubKey.WitHash.ScriptPubKey);
                    }
                }

                DateTimeOffset? lastCoinJoinTime = null;

                foreach (var tx in ScannerFiles.WasabiCoinJoins.Skip(5))
                {
                    var coordOutput = tx.Outputs.FirstOrDefault(x => scripts.Contains(x.ScriptPubKey));
                    if (coordOutput is null)
                    {
                        continue;
                    }

                    double vSizeEstimation = 10.75 + tx.Outputs.Count() * 31 + tx.Inputs.Count() * 67.75;

                    var blockTime = tx.BlockInfo.BlockTime;

                    if (lastCoinJoinTime.HasValue && (lastCoinJoinTime - blockTime).Value.Duration() > TimeSpan.FromDays(7))
                    {
                        throw new InvalidOperationException("No CoinJoin for a week");
                    }

                    lastCoinJoinTime = blockTime;

                    Console.Write($"{blockTime.Value.UtcDateTime.ToString("MM.dd.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)};");
                    Console.Write($"{tx.Id};");

                    var totalFee = (tx.Inputs.Sum(x => x.PrevOutput.Value) - tx.Outputs.Sum(x => x.Value));

                    Console.Write($"{string.Format("{0:0.00}", (double)(totalFee / vSizeEstimation))};");
                    Console.Write($"{coordOutput.ScriptPubKey.GetDestinationAddress(Network.Main)};");
                    Console.Write($"{coordOutput.Value};");

                    var outputs = tx.GetIndistinguishableOutputs(includeSingle: false);
                    var currentDenom = outputs.OrderByDescending(x => x.count).First().value;
                    foreach (var (value, count) in outputs.Where(x => x.value >= currentDenom))
                    {
                        Console.Write($"{value};{count};");
                    }
                    Console.WriteLine();
                }
            }
        }

        public void CalculateWabiSabiCoordStats(ExtPubKey[] xpubs)
        {
            using (BenchmarkLogger.Measure())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                var scripts = new HashSet<Script>();
                foreach (var xpub in xpubs)
                {
                    for (int i = 0; i < 100_000; i++)
                    {
                        scripts.Add(xpub.Derive(0, false).Derive(i, false).PubKey.WitHash.ScriptPubKey);
                    }
                }

                DateTimeOffset? lastCoinJoinTime = null;

                foreach (var tx in ScannerFiles.Wasabi2CoinJoins)
                {
                    // It's OK if it's null, because there are rounds with no coord fee.
                    var coordOutput = tx.Outputs.SingleOrDefault(x => scripts.Contains(x.ScriptPubKey));

                    double vSizeEstimation = 10.75 + tx.Outputs.Count() * 31 + tx.Inputs.Count() * 67.75;

                    var blockTime = tx.BlockInfo.BlockTime;

                    if (lastCoinJoinTime.HasValue && (lastCoinJoinTime - blockTime).Value.Duration() > TimeSpan.FromDays(7))
                    {
                        throw new InvalidOperationException("No CoinJoin for a week");
                    }

                    lastCoinJoinTime = blockTime;

                    Console.Write($"{blockTime.Value.UtcDateTime.ToString("MM.dd.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)};");
                    Console.Write($"{tx.Id};");

                    var totalFee = (tx.Inputs.Sum(x => x.PrevOutput.Value) - tx.Outputs.Sum(x => x.Value));

                    Console.Write($"{string.Format("{0:0.00}", (double)(totalFee / vSizeEstimation))};");
                    Console.Write($"{(coordOutput is null ? "_______there-was-no-coordinator-fee_______" : coordOutput.ScriptPubKey.GetDestinationAddress(Network.Main).ToString())};");
                    Console.Write($"{(coordOutput is null ? Money.Zero : coordOutput.Value)};");

                    var outputs = tx.GetIndistinguishableOutputs(includeSingle: false);
                    var currentDenom = outputs.OrderByDescending(x => x.count).First().value;
                    foreach (var (value, count) in outputs.Where(x => x.value >= currentDenom))
                    {
                        Console.Write($"{value};{count};");
                    }
                    Console.WriteLine();
                }
            }
        }

        public void UploadToDatabase()
        {
            CalculateAndUploadMonthlyVolumes();
            CalculateAndUploadFreshBitcoins();
            CalculateAndUploadMonthlyCoinJoins();
            //CalculateAndUploadNeverMixed();
            //CalculateAndUploadPostMixConsolidation();
        }
    }
}
