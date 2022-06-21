using Dumplings.Stats;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumplings.Displaying
{
    public static class Display
    {
        public static void DisplayOtheriWasabiSamuriResults(IDictionary<YearMonth, int> otheriResults, IDictionary<YearMonth, int> wasabiResults, IDictionary<YearMonth, int> samuriResults)
            => DisplayOtheriWasabiSamuriResults(otheriResults, null, wasabiResults, samuriResults);
        public static void DisplayOtheriWasabiSamuriResults(IDictionary<YearMonth, int> otheriResults, IDictionary<YearMonth, int> wasabi2Results, IDictionary<YearMonth, int> wasabiResults, IDictionary<YearMonth, int> samuriResults)
        {
            var isWW2 = wasabi2Results != null;

            if (isWW2)
            {
                Console.WriteLine($"Month;Otheri;Wasabi2;Samuri");
            }
            else
            {
                Console.WriteLine($"Month;Otheri;Wasabi;Samuri");
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
                    Console.WriteLine($"{yearMonth};{otheri};{wasabi2};{wasabi};{samuri}");

                }
                else
                {
                    Console.WriteLine($"{yearMonth};{otheri};{wasabi};{samuri}");
                }
            }
        }

        public static void DisplayOtheriWasabiSamuriResults(IDictionary<YearMonth, Money> otheriResults, IDictionary<YearMonth, Money> wasabiResults, IDictionary<YearMonth, Money> samuriResults)
            => DisplayOtheriWasabiSamuriResults(otheriResults, null, otheriResults, samuriResults);
        public static void DisplayOtheriWasabiSamuriResults(IDictionary<YearMonth, Money> otheriResults, IDictionary<YearMonth, Money> wasabi2Results, IDictionary<YearMonth, Money> wasabiResults, IDictionary<YearMonth, Money> samuriResults)
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
                if (!samuriResults.TryGetValue(yearMonth, out Money samuri))
                {
                    samuri = Money.Zero;
                }

                if (isWW2)
                {
                    if (!wasabi2Results.TryGetValue(yearMonth, out var wasabi2))
                    {
                        wasabi2 = Money.Zero;
                    }
                    Console.WriteLine($"{yearMonth};{otheri.ToDecimal(MoneyUnit.BTC):0};{wasabi2.ToDecimal(MoneyUnit.BTC):0};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0}");

                }
                else
                {
                    Console.WriteLine($"{yearMonth};{otheri.ToDecimal(MoneyUnit.BTC):0};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0}");
                }

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

            foreach (var yearMonthDay in wasabi2Results
                .Keys
                .Concat(wasabiResults.Keys)
                .Concat(otheriResults.Keys)
                .Concat(samuriResults.Keys)
                .Distinct()
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month))
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

        public static void DisplayWasabiResults(Dictionary<YearMonth, decimal> wasabiResults)
        {
            Console.WriteLine($"Month;Wasabi");

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

                Console.WriteLine($"{yearMonth};{wasabi:0.00}");
            }
        }

        public static void DisplayWasabiSamuriResults(Dictionary<YearMonth, Money> wasabiResults, Dictionary<YearMonth, Money> samuriResults)
        {
            Console.WriteLine($"Month;Wasabi;Samuri");

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

                Console.WriteLine($"{yearMonth};{wasabi.ToDecimal(MoneyUnit.BTC):0.00};{samuri.ToDecimal(MoneyUnit.BTC):0.00}");
            }
        }

        public static void DisplayOtheriWasabiSamuriResults(Dictionary<YearMonth, ulong> otheriResults, Dictionary<YearMonth, ulong> wasabiResults, Dictionary<YearMonth, ulong> samuriResults)
            => DisplayOtheriWasabiSamuriResults(otheriResults, null, wasabiResults, samuriResults);
        public static void DisplayOtheriWasabiSamuriResults(Dictionary<YearMonth, ulong> otheriResults, Dictionary<YearMonth, ulong> wasabi2Results, Dictionary<YearMonth, ulong> wasabiResults, Dictionary<YearMonth, ulong> samuriResults)
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
                if (!samuriResults.TryGetValue(yearMonth, out ulong samuri))
                {
                    samuri = 0;
                }

                if (isWW2)
                {
                    if (!wasabi2Results.TryGetValue(yearMonth, out var wasabi2))
                    {
                        wasabi2 = Money.Zero;
                    }
                    Console.WriteLine($"{yearMonth};{otheri:0};{wasabi2:0};{wasabi:0};{samuri:0}");

                }
                else
                {
                    Console.WriteLine($"{yearMonth};{otheri:0};{wasabi:0};{samuri:0}");
                }

            }
        }

        public static void DisplayOtheriWasabiSamuriResults(IDictionary<YearMonth, decimal> otheriResults, IDictionary<YearMonth, decimal> wasabiResults, IDictionary<YearMonth, decimal> samuriResults)
            => DisplayOtheriWasabiSamuriResults(otheriResults, null, wasabiResults, samuriResults);
        public static void DisplayOtheriWasabiSamuriResults(IDictionary<YearMonth, decimal> otheriResults, IDictionary<YearMonth, decimal> wasabi2Results, IDictionary<YearMonth, decimal> wasabiResults, IDictionary<YearMonth, decimal> samuriResults)
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
                if (!samuriResults.TryGetValue(yearMonth, out decimal samuri))
                {
                    samuri = 0;
                }


                if (isWW2)
                {
                    if (!wasabi2Results.TryGetValue(yearMonth, out var wasabi2))
                    {
                        wasabi2 = 0;
                    }
                    Console.WriteLine($"{yearMonth};{otheri:0.0};{wasabi2:0.0};{wasabi:0.0};{samuri:0.0}");

                }
                else
                {
                    Console.WriteLine($"{yearMonth};{otheri:0.0};{wasabi:0.0};{samuri:0.0}");
                }
            }
        }
    }
}
