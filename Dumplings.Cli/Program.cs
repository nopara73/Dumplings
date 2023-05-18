using Dumplings.Checking;
using Dumplings.Helpers;
using Dumplings.Scanning;
using Dumplings.Stats;
using NBitcoin;
using NBitcoin.RPC;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dumplings.Cli
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Logger.InitializeDefaults();

            Logger.LogInfo("Parsing arguments...");
            ParseArgs(args, out Command command, out NetworkCredential rpcCred);

            var rpcConf = new RPCCredentialString
            {
                UserPassword = rpcCred
            };
            var client = new RPCClient(rpcConf, Network.Main);

            using (BenchmarkLogger.Measure(operationName: $"{command} Command"))
            {
                if (command == Command.Resync)
                {
                    var scanner = new Scanner(client);
                    await scanner.ScanAsync(rescan: true);
                }
                //else if (command == Command.Sync)
                {
                    var scanner = new Scanner(client);
                    await scanner.ScanAsync(rescan: false);
                }
                if (command == Command.Check)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var checker = new Checker(loadedScannerFiles);
                    checker.Check();
                }
                else if (command == Command.MonthlyVolumes)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateMonthlyVolumes();
                }
                else if (command == Command.FreshBitcoins)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateFreshBitcoins();
                }
                else if (command == Command.FreshBitcoinAmounts)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateFreshBitcoinAmounts();
                }
                else if (command == Command.FreshBitcoinsDaily)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateFreshBitcoinsDaily();
                }
                else if (command == Command.NeverMixed)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateNeverMixed(client);
                }
                else if (command == Command.CoinJoinEquality)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateEquality();
                }
                else if (command == Command.CoinJoinIncome)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateIncome();
                }
                else if (command == Command.PostMixConsolidation)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculatePostMixConsolidation();
                }
                else if (command == Command.SmallerThanMinimum)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateSmallerThanMinimumWasabiInputs();
                }
                else if (command == Command.MonthlyEqualVolumes)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateMonthlyEqualVolumes();
                }
                else if (command == Command.AverageUserCount)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateMonthlyAverageMonthlyUserCounts();
                }
                else if (command == Command.AverageNetworkFeePaidByUserPerCoinjoin)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateMonthlyNetworkFeePaidByUserPerCoinjoin();
                }
                else if (command == Command.Records)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateRecords();
                }
                else if (command == Command.UniqueCountPercent)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateUniqueCountPercent();
                }
                else if (command == Command.ListFreshBitcoins)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.ListFreshBitcoins();
                }
                else if (command == Command.UnspentCapacity)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateUnspentCapacity(client);
                }
                else if (command == Command.Sake)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles);
                    stat.CalculateSakeRelevantStats(client);
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }

        private static void ParseArgs(string[] args, out Command command, out NetworkCredential cred)
        {
            string rpcUser = null;
            string rpcPassword = null;
            command = (Command)Enum.Parse(typeof(Command), args[0], ignoreCase: true);

            var rpcUserArg = "--rpcuser=";
            var rpcPasswordArg = "--rpcpassword=";
            var rpcCookieFileArg = "--rpccookiefile=";
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

                idx = arg.IndexOf(rpcCookieFileArg, StringComparison.Ordinal);
                if (idx == 0)
                {
                    string rpcCookieFile = arg.Substring(idx + rpcCookieFileArg.Length);
                    string[] rpcCookieData = File.ReadAllText(rpcCookieFile).Split(":");
                    rpcUser = rpcCookieData[0];
                    rpcPassword = rpcCookieData[1];
                }
            }

            cred = new NetworkCredential(rpcUser, rpcPassword);
        }
    }
}
