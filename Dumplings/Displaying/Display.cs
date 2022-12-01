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

            MySqlConnection conn = Connect.InitDb();
            foreach (var yearMonth in wasabiResults
                .Keys
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

                //sw.WriteLine($"{yearMonth};{otheri.ToDecimal(MoneyUnit.BTC):0};{wasabi.ToDecimal(MoneyUnit.BTC):0};{samuri.ToDecimal(MoneyUnit.BTC):0}");

                //Console.WriteLine(DateTime.Parse($"{yearMonth}"));

                string sql = "CALL storeFreshCoins(@d,@o,@w,@s);";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@d", DateTime.Parse($"{yearMonth}"));
                cmd.Parameters["@d"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@o", Money.FromUnit(otheri, MoneyUnit.BTC));
                cmd.Parameters["@o"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@w", Money.FromUnit(wasabi, MoneyUnit.BTC));
                cmd.Parameters["@w"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@s", Money.FromUnit(samuri, MoneyUnit.BTC));
                cmd.Parameters["@s"].Direction = ParameterDirection.Input;
                conn.Open();
                int res = cmd.ExecuteNonQuery();
                conn.Close();
            }
            FileStream fs = new FileStream("wasabifreshbitcoins.csv", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //sw.WriteLine($"Month2;Otheri;Wasabi;Samuri");

            if (isWW2)
            {
                sw.WriteLine($"Month;Otheri;Wasabi2;Samuri");
            }
            else
            {
                sw.WriteLine($"Month;Otheri;Wasabi;Samuri");
            }
            foreach (var yearMonth in wasabiResults
                    .Keys
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

                sw.WriteLine($"{yearMonth};{Money.FromUnit(otheri, MoneyUnit.BTC):0};{Money.FromUnit(wasabi, MoneyUnit.BTC):0};{Money.FromUnit(samuri, MoneyUnit.BTC):0}");
            }
            sw.Close();
            fs.Close();
            Console.WriteLine("Successful write to files.");

            // end of sql process.

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

        public static void DisplayOtheriWasabiSamuriResults(Dictionary<YearMonthDay, List<(int uniqueOutCount, int uniqueInCount, double uniqueOutCountPercent, double uniqueInCountPercent)>> uniqueCountPercents)
        {
            Console.WriteLine($"Date;uniqueOutCount;uniqueInCount;uniqueOutCountPercent;uniqueInCountPercent");

            foreach (var record in uniqueCountPercents.OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month).ThenBy(x => x.Key.Day))
            {
                Console.WriteLine($"{record.Key};{record.Value.Median(x => x.uniqueOutCount):0.0};{record.Value.Median(x => x.uniqueInCount):0.0};{record.Value.Median(x => x.uniqueOutCountPercent):0.0};{record.Value.Median(x => x.uniqueInCountPercent):0.0}");
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
                
                MySqlConnection conn = Connect.InitDb();                

                string check = "CALL checkMonthlyCoinJoins(@d);";
                MySqlCommand comm = new MySqlCommand(check, conn);
                comm.Parameters.AddWithValue("@d", DateTime.Parse($"{yearMonth}"));
                comm.Parameters["@d"].Direction = ParameterDirection.Input;
                conn.Open();
                MySqlDataReader reader = comm.ExecuteReader();
                bool write = false;
                while(reader.Read())
                {
                    if(reader[0].ToString() == "0")
                    {
                        write = true;
                    } 
                }
                reader.Close();
                conn.Close();
                if(write)
                {
                    string sql = "CALL storeMonthlyCoinJoins(@d,@w,@s);";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@d", DateTime.Parse($"{yearMonth}"));
                    cmd.Parameters["@d"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@w", Math.Round(wasabi.ToDecimal(MoneyUnit.BTC),3));             
                    cmd.Parameters["@w"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@s", Math.Round(samuri.ToDecimal(MoneyUnit.BTC),3));
                    cmd.Parameters["@s"].Precision = 8;
                    cmd.Parameters["@s"].Scale = 5;
                    cmd.Parameters["@s"].Direction = ParameterDirection.Input;
                    conn.Open();
                    int res = cmd.ExecuteNonQuery();
                    conn.Close();
                }

                //Console.WriteLine($"{yearMonth};{wasabi.ToDecimal(MoneyUnit.BTC):0.00};{samuri.ToDecimal(MoneyUnit.BTC):0.00}");
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

        public static void DisplayRecords(Dictionary<int, VerboseTransactionInfo> mostInputs, Dictionary<int, VerboseTransactionInfo> mostOutputs, Dictionary<int, VerboseTransactionInfo> mostInputsAndOutputs, Dictionary<Money, VerboseTransactionInfo> largestVolumes, Dictionary<ulong, VerboseTransactionInfo> largestCjEqualities, Dictionary<int, VerboseTransactionInfo> smallestUnequalOutputs, Dictionary<int, VerboseTransactionInfo> smallestUnequalInputs)
        {
            Console.WriteLine();
            Console.WriteLine("Input count records:");
            foreach (var cj in mostInputs)
            {
                Console.WriteLine($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key}\t{cj.Value.Id}");
            }
            Console.WriteLine();
            Console.WriteLine("Output count records:");
            foreach (var cj in mostOutputs)
            {
                Console.WriteLine($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key}\t{cj.Value.Id}");
            }
            Console.WriteLine();
            Console.WriteLine("Input count + output count records:");
            foreach (var cj in mostInputsAndOutputs)
            {
                Console.WriteLine($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key}\t{cj.Value.Id}");
            }
            Console.WriteLine();
            Console.WriteLine("Volume records:");
            foreach (var cj in largestVolumes)
            {
                Console.WriteLine($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key.ToDecimal(MoneyUnit.BTC):0}\t{cj.Value.Id}");
            }
            Console.WriteLine();
            Console.WriteLine("Equality records:");
            foreach (var cj in largestCjEqualities)
            {
                Console.WriteLine($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key}\t{cj.Value.Id}");
            }
            Console.WriteLine();
            Console.WriteLine("Unique output count records:");
            foreach (var cj in smallestUnequalOutputs)
            {
                Console.WriteLine($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key}\t{cj.Value.Id}");
            }
            Console.WriteLine();
            Console.WriteLine("Unique input count records:");
            foreach (var cj in smallestUnequalInputs)
            {
                Console.WriteLine($"{cj.Value.BlockInfo.YearMonthDay}:\t{cj.Key}\t{cj.Value.Id}");
            }
        }
    }
}
