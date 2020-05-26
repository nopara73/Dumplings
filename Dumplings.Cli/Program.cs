using Dumplings.Checking;
using Dumplings.Helpers;
using Dumplings.Scanning;
using Dumplings.Stats;
using NBitcoin;
using NBitcoin.RPC;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dumplings.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Logger.InitializeDefaults();

            Logger.LogInfo("Parsing arguments...");
            ParseArgs(args, out Command command, out NetworkCredential rpcCred);

            var rpcConf = new RPCCredentialString
            {
                UserPassword = rpcCred
            };
            var client = new RPCClient(rpcConf, Network.Main);

            Logger.LogInfo("Checking Bitcoin Knots sync status...");

            var bci = await client.GetBlockchainInfoAsync();

            var missingBlocks = bci.Headers - bci.Blocks;
            if (missingBlocks != 0)
            {
                throw new InvalidOperationException($"Knots is not synchronized. Blocks missing: {missingBlocks}.");
            }

            Logger.LogInfo($"Bitcoin Knots is synchronized. Current height: {bci.Blocks}.");

            using (BenchmarkLogger.Measure(operationName: $"{command} Command"))
            {
                if (command == Command.Resync)
                {
                    var scanner = new Scanner(client);
                    await scanner.ScanAsync(rescan: true);
                }
                else if (command == Command.Sync)
                {
                    var scanner = new Scanner(client);
                    await scanner.ScanAsync(rescan: false);
                }
                else if (command == Command.Check)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var checker = new Checker(loadedScannerFiles);
                    checker.Check();
                }
                else if (command == Command.MonthlyVolumes)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles, client);
                    stat.CalculateMonthlyVolumes();
                }
                else if (command == Command.FreshBitcoins)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles, client);
                    stat.CalculateFreshBitcoins();
                }
                else if (command == Command.NeverMixed)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles, client);
                    stat.CalculateNeverMixed();
                }
                else if (command == Command.CoinJoinEquality)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles, client);
                    stat.CalculateEquality();
                }
                else if (command == Command.CoinJoinIncome)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles, client);
                    stat.CalculateIncome();
                }
                else if (command == Command.PostMixConsolidation)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles, client);
                    stat.CalculatePostMixConsolidation();
                }
                else if (command == Command.SmallerThanMinimum)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles, client);
                    stat.CalculateSmallerThanMinimumWasabiInputs();
                }
                else if (command == Command.MonthlyEqualVolumes)
                {
                    var loadedScannerFiles = Scanner.Load();
                    var stat = new Statista(loadedScannerFiles, client);
                    stat.CalculateMonthlyEqualVolumes();
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press a button to exit...");
            Console.ReadKey();
        }

        private static void ParseArgs(string[] args, out Command command, out NetworkCredential cred)
        {
            string rpcUser = null;
            string rpcPassword = null;
            command = (Command)Enum.Parse(typeof(Command), args[0], ignoreCase: true);

            var rpcUserArg = "--rpcuser=";
            var rpcPasswordArg = "--rpcpassword=";
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
            }

            cred = new NetworkCredential(rpcUser, rpcPassword);
        }
    }
}
