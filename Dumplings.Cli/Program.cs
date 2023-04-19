using Dumplings.Checking;
using Dumplings.Helpers;
using Dumplings.Scanning;
using Dumplings.Stats;
using NBitcoin;
using NBitcoin.RPC;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Dumplings.Cli
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Logger.InitializeDefaults();

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Logger.LogInfo("Parsing arguments...");
            ParseArgs(args, out Command command, out NetworkCredential rpcCred, out var host, out string connString);

            var rpcConf = new RPCCredentialString
            {
                UserPassword = rpcCred
            };
            var outputFolder = GetOutputFolder(args);
            string filePath = null;
            if (!string.IsNullOrEmpty(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
                filePath = Path.Combine(outputFolder, $"{command}_{DateTime.Now:yyMMdd_HHmmss}.txt");
                Console.WriteLine($"Output will be written to: {filePath}");
            }

            var client = new RPCClient(rpcConf, host, Network.Main);

            using (BenchmarkLogger.Measure(operationName: $"{command} Command"))
            {
                try
                {
                    if (GetShouldSync(args))
                    {
                        var scanner = new Scanner(client);
                        await scanner.ScanAsync(rescan: false);
                    }
                    else if (GetShouldResync(args))
                    {
                        var scanner = new Scanner(client);
                        await scanner.ScanAsync(rescan: true);
                    }

                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles, client, filePath, connString);

                    if (command == Command.Check)
                    {
                        var checker = new Checker(loadedScannerFiles);
                        checker.Check();
                    }
                    else if (command == Command.CountCoinJoins)
                    {
                        stat.CalculateAndUploadMonthlyCoinJoins();
                    }
                    else if (command == Command.MonthlyVolumes)
                    {
                        stat.CalculateAndUploadMonthlyVolumes();
                    }
                    else if (command == Command.DailyVolumes)
                    {
                        stat.CalculateAndUploadDailyVolumes();
                    }
                    else if (command == Command.FreshBitcoins)
                    {
                        stat.CalculateAndUploadFreshBitcoins();
                    }
                    else if (command == Command.FreshBitcoinAmounts)
                    {
                        stat.CalculateFreshBitcoinAmounts();
                    }
                    else if (command == Command.FreshBitcoinsDaily)
                    {
                        stat.CalculateAndUploadFreshBitcoinsDaily();
                    }
                    else if (command == Command.NeverMixed)
                    {
                        stat.CalculateAndUploadNeverMixed();
                    }
                    else if (command == Command.CoinJoinEquality)
                    {
                        stat.CalculateEquality();
                    }
                    else if (command == Command.CoinJoinIncome)
                    {
                        stat.CalculateIncome();
                    }
                    else if (command == Command.PostMixConsolidation)
                    {
                        stat.CalculateAndUploadPostMixConsolidation();
                    }
                    else if (command == Command.SmallerThanMinimum)
                    {
                        stat.CalculateSmallerThanMinimumWasabiInputs();
                    }
                    else if (command == Command.MonthlyEqualVolumes)
                    {
                        stat.CalculateMonthlyEqualVolumes();
                    }
                    else if (command == Command.AverageUserCount)
                    {
                        stat.CalculateMonthlyAverageMonthlyUserCounts();
                    }
                    else if (command == Command.AverageNetworkFeePaidByUserPerCoinjoin)
                    {
                        stat.CalculateMonthlyNetworkFeePaidByUserPerCoinjoin();
                    }
                    else if (command == Command.Records)
                    {
                        stat.CalculateRecords();
                    }
                    else if (command == Command.UniqueCountPercent)
                    {
                        stat.CalculateUniqueCountPercent();
                    }
                    else if (command == Command.ListFreshBitcoins)
                    {
                        stat.ListFreshBitcoins();
                    }
                    else if (command == Command.UnspentCapacity)
                    {
                        stat.CalculateAndUploadUnspentCapacity(client);
                    }
                    else if (command == Command.WasabiCoordStats)
                    {
                        stat.CalculateWasabiCoordStats(GetXpub(args));
                    }
                    else if (command == Command.WabiSabiCoordStats)
                    {
                        stat.CalculateWabiSabiCoordStats(GetXpub(args));
                    }
                    else if (command == Command.Upload)
                    {
                        stat.UploadToDatabase();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }

            Console.WriteLine();

            if (!GetNoWaitOnExit(args))
            {
                Console.WriteLine("Press a button to exit...");
                Console.ReadKey();
            }
        }

        private static ExtPubKey[] GetXpub(string[] args)
        {
            var xpubUserArg = "--xpub=";
            foreach (var arg in args)
            {
                var idx = arg.IndexOf(xpubUserArg, StringComparison.Ordinal);
                if (idx == 0)
                {
                    return arg.Substring(idx + xpubUserArg.Length).Split(',')
                        .Select(str => ExtPubKey.Parse(str, Network.Main)).ToArray();
                }
            }

            return null;
        }

        private static bool GetNoWaitOnExit(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.Contains("--nowaitonexit", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool GetShouldSync(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.Contains("--sync", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool GetShouldResync(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.Contains("--resync", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetOutputFolder(string[] args)
        {
            string folderPath = null;

            var folderPathArg = "--outfolder=";
            foreach (var arg in args)
            {
                var idx = arg.IndexOf(folderPathArg, StringComparison.Ordinal);
                if (idx == 0)
                {
                    folderPath = arg.Substring(idx + folderPathArg.Length);
                    Directory.CreateDirectory(folderPath);
                    return folderPath;
                }
            }

            return folderPath;
        }

        private static void ParseArgs(string[] args, out Command command, out NetworkCredential cred, out string host, out string connString)
        {
            string rpcUser = null;
            string rpcPassword = null;
            connString = null;
            host = null;
            command = (Command)Enum.Parse(typeof(Command), args[0], ignoreCase: true);

            var rpcUserArg = "--rpcuser=";
            var rpcPasswordArg = "--rpcpassword=";
            var hostArg = "--host=";
            var connStringArg = "--conn=";
            foreach (var arg in args)
            {
                var idx = arg.IndexOf(rpcUserArg, StringComparison.Ordinal);
                if (idx == 0)
                {
                    rpcUser = arg.Substring(idx + rpcUserArg.Length);
                }

                idx = arg.IndexOf(rpcPasswordArg, StringComparison.Ordinal);
                if (idx == 0)
                {
                    rpcPassword = arg.Substring(idx + rpcPasswordArg.Length);
                }

                idx = arg.IndexOf(hostArg, StringComparison.Ordinal);
                if (idx == 0)
                {
                    host = arg.Substring(idx + hostArg.Length).Trim();
                }

                idx = arg.IndexOf(connStringArg, StringComparison.Ordinal);
                if (idx == 0)
                {
                    connString = arg.Substring(idx + connStringArg.Length).Trim();
                }
            }

            cred = new NetworkCredential(rpcUser.Trim(), rpcPassword.Trim());
        }
    }
}
