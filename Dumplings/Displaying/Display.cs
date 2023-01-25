using Dumplings.Rpc;
using Dumplings.Stats;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Dumplings.Cli;
using System.Data;
using System.IO;

namespace Dumplings.Displaying
{
    public static class Display
    {
        public static void DisplayOtheriWasabiSamuriResults(IDictionary<YearMonth, int> otheriResults, IDictionary<YearMonth, int> wasabi2Results, IDictionary<YearMonth, int> wasabiResults, IDictionary<YearMonth, int> samuriResults, out List<string> resultList)
        {
            resultList = new List<string>();
            var isWW2 = wasabi2Results != null;

            if (isWW2)
            {
                resultList.Add($"Month;Otheri;Wasabi2;Samuri");
            }
            else
            {
                resultList.Add($"Month;Otheri;Wasabi;Samuri");
            }

            foreach (var yearMonth in wasabi2Results
                .Keys
                .Concat(wasabiResults.Keys)
                .Concat(otheriResults.Keys)
                .Concat(samuriResults.Keys)
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month))
            {
                if (!otheriResults.TryGetValue(yearMonth, out var otheri))
                {
                    otheri = 0;
                }
                if (!wasabiResults.TryGetValue(yearMonth, out var wasabi))
                {
                    wasabi = 0;
                }
                if (!samuriResults.TryGetValue(yearMonth, out var samuri))
                {
                    samuri = 0;
                }

                if (isWW2)
                {
                    if (!wasabi2Results.TryGetValue(yearMonth, out var wasabi2))
                    {
                        wasabi2 = 0;
                    }
                    resultList.Add($"{yearMonth};{otheri};{wasabi2};{wasabi};{samuri}");
                }
                else
                {
                    resultList.Add($"{yearMonth};{otheri};{wasabi};{samuri}");
                }
            }
            foreach (var line in resultList)
            {
                Console.WriteLine(line);
            }
        }

        public static void DisplayOtheriWasabiSamuriResults(IDictionary<YearMonth, Money> otheriResults, IDictionary<YearMonth, Money> wasabiResults, IDictionary<YearMonth, Money> samuriResults, out List<string> resultList)
        {
            resultList = new();

            resultList.Add($"Month;Otheri;Wasabi;Samuri");

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

                resultList.Add($"{yearMonth};{otheri.ToDecimal(MoneyUnit.BTC):0};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0}");
            }
            foreach (var line in resultList)
            {
                Console.WriteLine(line);
            }
        }

        public static void DisplayOtheriWasabiSamuriResults(Dictionary<YearMonthDay, Money> otheriResults, Dictionary<YearMonthDay, Money> wasabi2Results, Dictionary<YearMonthDay, Money> wasabiResults, Dictionary<YearMonthDay, Money> samuriResults)
        {
            var isWW2 = wasabi2Results != null;

            if (isWW2)
            {
                Console.WriteLine($"Month;Otheri;Wasabi2;Wasabi;Samuri");
            }
            else
            {
                Console.WriteLine($"Month;Otheri;Wasabi;Samuri");
            }

            YearMonthDay[] yearMonthDays = wasabi2Results
                            .Keys
                            .Concat(wasabiResults.Keys)
                            .Concat(otheriResults.Keys)
                            .Concat(samuriResults.Keys)
                            .Distinct()
                            .OrderBy(x => x.Year)
                            .ThenBy(x => x.Month)
                            .ThenBy(x => x.Day)
                            .ToArray();
            foreach (var yearMonthDay in yearMonthDays)
            {
                if (!otheriResults.TryGetValue(yearMonthDay, out Money otheri))
                {
                    otheri = Money.Zero;
                }
                if (!wasabiResults.TryGetValue(yearMonthDay, out Money wasabi))
                {
                    wasabi = Money.Zero;
                }
                if (!samuriResults.TryGetValue(yearMonthDay, out Money samuri))
                {
                    samuri = Money.Zero;
                }

                if (isWW2)
                {
                    if (!wasabi2Results.TryGetValue(yearMonthDay, out var wasabi2))
                    {
                        wasabi2 = Money.Zero;
                    }
                    Console.WriteLine($"{yearMonthDay};{otheri.ToDecimal(MoneyUnit.BTC):0};{wasabi2.ToDecimal(MoneyUnit.BTC):0};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0}");
                }
                else
                {
                    Console.WriteLine($"{yearMonthDay};{otheri.ToDecimal(MoneyUnit.BTC):0};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0}");
                }
            }
        }

        public static void DisplayOtheriWasabiSamuriResults(Dictionary<YearMonthDay, List<(int uniqueOutCount, int uniqueInCount, double uniqueOutCountPercent, double uniqueInCountPercent)>> uniqueCountPercents, out List<string> resultList)
        {
            resultList = new();
            string header = $"Date;uniqueOutCount;uniqueInCount;uniqueOutCountPercent;uniqueInCountPercent";

            Console.WriteLine(header);
            resultList.Add(header);

            foreach (var record in uniqueCountPercents.OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month).ThenBy(x => x.Key.Day))
            {
                string line = $"{record.Key};{record.Value.Median(x => x.uniqueOutCount):0.0};{record.Value.Median(x => x.uniqueInCount):0.0};{record.Value.Median(x => x.uniqueOutCountPercent):0.0};{record.Value.Median(x => x.uniqueInCountPercent):0.0}";
                Console.WriteLine(line);
                resultList.Add(line);
            }
        }

        public static void DisplayWasabiResults(Dictionary<YearMonth, decimal> wasabiResults, out List<string> resultList)
        {
            resultList = new();

            resultList.Add($"Month;Wasabi");

            foreach (var yearMonth in wasabiResults
                .Keys
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month))
            {
                if (!wasabiResults.TryGetValue(yearMonth, out decimal wasabi))
                {
                    wasabi = 0;
                }

                resultList.Add($"{yearMonth};{wasabi:0.00}");
            }
            foreach (var line in resultList)
            {
                Console.WriteLine(line);
            }
        }

        public static void DisplayWasabiSamuriResults(Dictionary<YearMonth, Money> wasabiResults, Dictionary<YearMonth, Money> samuriResults, out List<string> resultList)
        {
            resultList = new();

            resultList.Add($"Month;Wasabi;Samuri");

            foreach (var yearMonth in wasabiResults
                .Keys
                .Concat(samuriResults.Keys)
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month))
            {
                if (!wasabiResults.TryGetValue(yearMonth, out Money wasabi))
                {
                    wasabi = Money.Zero;
                }
                if (!samuriResults.TryGetValue(yearMonth, out Money samuri))
                {
                    samuri = Money.Zero;
                }

                resultList.Add($"{yearMonth};{wasabi.ToDecimal(MoneyUnit.BTC):0.00};{samuri.ToDecimal(MoneyUnit.BTC):0.00}");
            }
            foreach (var line in resultList)
            {
                Console.WriteLine(line);
            }
        }

        public static void DisplayRecords(Dictionary<int, VerboseTransactionInfo> mostInputs, Dictionary<int, VerboseTransactionInfo> mostOutputs, Dictionary<int, VerboseTransactionInfo> mostInputsAndOutputs, Dictionary<Money, VerboseTransactionInfo> largestVolumes, Dictionary<ulong, VerboseTransactionInfo> largestCjEqualities, Dictionary<int, VerboseTransactionInfo> smallestUnequalOutputs, Dictionary<int, VerboseTransactionInfo> smallestUnequalInputs, out List<string> resultList)
        {
            resultList = new();
            resultList.Add("\nInput count records:");
            foreach (var cj in mostInputs)
            {
                resultList.Add($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key}\t{cj.Value.Id}");
            }

            resultList.Add("\nOutput count records:");
            foreach (var cj in mostOutputs)
            {
                resultList.Add($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key}\t{cj.Value.Id}");
            }

            resultList.Add("\nInput count + output count records:");
            foreach (var cj in mostInputsAndOutputs)
            {
                resultList.Add($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key}\t{cj.Value.Id}");
            }

            resultList.Add("\nVolume records:");
            foreach (var cj in largestVolumes)
            {
                resultList.Add($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key.ToDecimal(MoneyUnit.BTC):0}\t{cj.Value.Id}");
            }

            resultList.Add("\nEquality records:");
            foreach (var cj in largestCjEqualities)
            {
                resultList.Add($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key}\t{cj.Value.Id}");
            }

            resultList.Add("\nUnique output count records:");
            foreach (var cj in smallestUnequalOutputs)
            {
                resultList.Add($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key}\t{cj.Value.Id}");
            }

            resultList.Add("\nUnique input count records:");
            foreach (var cj in smallestUnequalInputs)
            {
                resultList.Add($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key}\t{cj.Value.Id}");
            }
        }

        public static void DisplayOtheriWasabiWabiSabiSamuriResults(Dictionary<YearMonth, decimal> otheriResults, Dictionary<YearMonth, decimal> wasabiResults, Dictionary<YearMonth, decimal> wasabi2Results, Dictionary<YearMonth, decimal> samuriResults, out List<string> resultList)
        {
            resultList = new();

            resultList.Add($"Month;Otheri;Wasabi;Wasabi2;Samuri");

            foreach (var yearMonth in wasabi2Results
                .Keys
                .Concat(wasabiResults.Keys)
                .Concat(otheriResults.Keys)
                .Concat(samuriResults.Keys)
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
                if (!wasabi2Results.TryGetValue(yearMonth, out decimal wasabi2))
                {
                    wasabi2 = 0;
                }
                if (!samuriResults.TryGetValue(yearMonth, out decimal samuri))
                {
                    samuri = 0;
                }
                resultList.Add($"{yearMonth};{otheri:0};{wasabi2:0};{wasabi:0};{samuri:0}");
            }
            foreach (var line in resultList)
            {
                Console.WriteLine(line);
            }
        }

        public static void DisplayOtheriWasabiWabiSabiSamuriResults(Dictionary<YearMonth, ulong> otheriResults, Dictionary<YearMonth, ulong> wasabiResults, Dictionary<YearMonth, ulong> wasabi2Results, Dictionary<YearMonth, ulong> samuriResults, out List<string> resultList)
        {
            resultList = new();

            resultList.Add($"Month;Otheri;Wasabi;Wasabi2;Samuri");

            foreach (var yearMonth in wasabi2Results
                .Keys
                .Concat(wasabiResults.Keys)
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
                if (!wasabi2Results.TryGetValue(yearMonth, out ulong wasabi2))
                {
                    wasabi2 = 0;
                }
                if (!samuriResults.TryGetValue(yearMonth, out ulong samuri))
                {
                    samuri = 0;
                }
                resultList.Add($"{yearMonth};{otheri:0};{wasabi2:0};{wasabi:0};{samuri:0}");
            }
            foreach (var line in resultList)
            {
                Console.WriteLine(line);
            }
        }

        internal static void DisplayOtheriWasabiWabiSabiSamuriResults(Dictionary<YearMonth, Money> otheriResults, Dictionary<YearMonth, Money> wasabiResults, Dictionary<YearMonth, Money> wasabi2Results, Dictionary<YearMonth, Money> samuriResults, out List<string> resultList)
        {
            resultList = new List<string>();

            resultList.Add($"Month;Otheri;Wasabi2;Wasabi;Samuri");

            foreach (var yearMonth in wasabi2Results
                .Keys
                .Concat(wasabiResults.Keys)
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
                if (!wasabi2Results.TryGetValue(yearMonth, out Money wasabi2))
                {
                    wasabi2 = Money.Zero;
                }
                if (!samuriResults.TryGetValue(yearMonth, out Money samuri))
                {
                    samuri = Money.Zero;
                }
                resultList.Add($"{yearMonth};{otheri.ToDecimal(MoneyUnit.BTC):0};{wasabi2.ToDecimal(MoneyUnit.BTC):0};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0}");
            }
            foreach (var line in resultList)
            {
                Console.WriteLine(line);
            }
        }

        public static void DisplayOtheriWasabiWabiSabiSamuriResults(Dictionary<YearMonthDay, Money> otheriResults, Dictionary<YearMonthDay, Money> wasabiResults, Dictionary<YearMonthDay, Money> wasabi2Results, Dictionary<YearMonthDay, Money> samuriResults, out List<string> resultList)
        {
            resultList = new List<string>();

            resultList.Add($"Month;Otheri;Wasabi2;Wasabi;Samuri");

            foreach (var yearMonthDay in wasabi2Results
                .Keys
                .Concat(wasabiResults.Keys)
                .Concat(otheriResults.Keys)
                .Concat(samuriResults.Keys)
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ThenBy(x => x.Day))
            {
                if (!otheriResults.TryGetValue(yearMonthDay, out Money otheri))
                {
                    otheri = Money.Zero;
                }
                if (!wasabiResults.TryGetValue(yearMonthDay, out Money wasabi))
                {
                    wasabi = Money.Zero;
                }
                if (!wasabi2Results.TryGetValue(yearMonthDay, out Money wasabi2))
                {
                    wasabi2 = Money.Zero;
                }
                if (!samuriResults.TryGetValue(yearMonthDay, out Money samuri))
                {
                    samuri = Money.Zero;
                }
                resultList.Add($"{yearMonthDay};{otheri.ToDecimal(MoneyUnit.BTC):0};{wasabi2.ToDecimal(MoneyUnit.BTC):0};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0}");
            }
            foreach (var line in resultList)
            {
                Console.WriteLine(line);
            }
        }
    }
}
