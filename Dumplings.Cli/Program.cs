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
            ParseArgs(args, out Command command, out NetworkCredential rpcCred, out var host);

            var rpcConf = new RPCCredentialString
            {
                UserPassword = rpcCred
            };

            var client = new RPCClient(rpcConf, host, Network.Main);

            using (BenchmarkLogger.Measure(operationName: $"{command} Command"))
            {
                var outputFolder = GetOutputFolder(args);
                try
                {
                    if (command == Command.Resync)
                    {
                        var scanner = new Scanner(client);
                        await scanner.ScanAsync(rescan: true);
                    }
                    else
                    {
                        var scanner = new Scanner(client);
                        await scanner.ScanAsync(rescan: false);
                    }

                    string filePath = null;
                    if (!string.IsNullOrEmpty(outputFolder))
                    {
                        Directory.CreateDirectory(outputFolder);
                        filePath = Path.Combine(outputFolder, $"Dump{DateTime.Now:yyMMdd_HHmmss}.txt");
                    }
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles, client, filePath);

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
                        stat.CalculateFreshBitcoinsDaily();
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
                        stat.CalculateUnspentCapacity(client);
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

        private static void ParseArgs(string[] args, out Command command, out NetworkCredential cred, out string host)
        {
            string rpcUser = null;
            string rpcPassword = null;
            host = null;
            command = (Command)Enum.Parse(typeof(Command), args[0], ignoreCase: true);

            var rpcUserArg = "--rpcuser=";
            var rpcPasswordArg = "--rpcpassword=";
            var hostArg = "--host=";
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
            }

            cred = new NetworkCredential(rpcUser.Trim(), rpcPassword.Trim());
        }
    }
}
