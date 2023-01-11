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
using System.Text;
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

                FileStream fileStream = null;
                StreamWriter writer = null;
                TextWriter oldOut = Console.Out;
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

                    if (!string.IsNullOrEmpty(outputFolder))
                    {
                        var filePath = Path.Combine(outputFolder, $"Dump{DateTime.Now:yyMMdd_HHmmss}.txt");
                        try
                        {
                            fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                            writer = new StreamWriter(fileStream);
                            Console.WriteLine($"Console output redirected to: {filePath}.");
                            Console.SetOut(writer);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Cannot open file: {filePath}.");
                            Console.WriteLine(ex.Message);
                        }
                    }
                    var loadedScannerFiles = Scanner.Load();

                    if (command == Command.Check)
                    {
                        var checker = new Checker(loadedScannerFiles);
                        checker.Check();
                    }
                    else if (command == Command.CountCoinJoins)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateAndUploadMonthlyCoinJoins();
                    }
                    else if (command == Command.MonthlyVolumes)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateAndUploadMonthlyVolumes();
                    }
                    else if (command == Command.FreshBitcoins)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateAndUploadFreshBitcoins();
                    }
                    else if (command == Command.FreshBitcoinAmounts)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateFreshBitcoinAmounts();
                    }
                    else if (command == Command.FreshBitcoinsDaily)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateFreshBitcoinsDaily();
                    }
                    else if (command == Command.NeverMixed)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateAndUploadNeverMixed();
                    }
                    else if (command == Command.CoinJoinEquality)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateEquality();
                    }
                    else if (command == Command.CoinJoinIncome)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateIncome();
                    }
                    else if (command == Command.PostMixConsolidation)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateAndUploadPostMixConsolidation();
                    }
                    else if (command == Command.SmallerThanMinimum)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateSmallerThanMinimumWasabiInputs();
                    }
                    else if (command == Command.MonthlyEqualVolumes)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateMonthlyEqualVolumes();
                    }
                    else if (command == Command.AverageUserCount)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateMonthlyAverageMonthlyUserCounts();
                    }
                    else if (command == Command.AverageNetworkFeePaidByUserPerCoinjoin)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateMonthlyNetworkFeePaidByUserPerCoinjoin();
                    }
                    else if (command == Command.Records)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateRecords();
                    }
                    else if (command == Command.UniqueCountPercent)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateUniqueCountPercent();
                    }
                    else if (command == Command.ListFreshBitcoins)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.ListFreshBitcoins();
                    }
                    else if (command == Command.UnspentCapacity)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateUnspentCapacity(client);
                    }
                    else if (command == Command.WasabiCoordStats)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateWasabiCoordStats(GetXpub(args));
                    }
                    else if (command == Command.WabiSabiCoordStats)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateWabiSabiCoordStats(GetXpub(args));
                    }
                    else if (command == Command.Upload)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.UploadToDatabase();
                    }
                    else if (command == Command.DisplayCoinJoinInfo)
                    {
                        var stat = new Statista(loadedScannerFiles, client);
                        stat.CalculateAndDisplayMonthlyCoinJoins();
                    }
                }
                finally
                {
                    writer?.Close();
                    fileStream?.Close();
                }

                Console.SetOut(oldOut);
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
