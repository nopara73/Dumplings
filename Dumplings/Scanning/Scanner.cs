using Dumplings.Helpers;
using Dumplings.Rpc;
using NBitcoin;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dumplings.Scanning
{
    public class Scanner
    {
        public Scanner(RPCClient rpc)
        {
            Rpc = rpc;
            Directory.CreateDirectory(WorkFolder);
        }

        public const string WorkFolder = "Scanner";
        public static readonly string LastProcessedBlockHeightPath = Path.Combine(WorkFolder, "LastProcessedBlockHeight.txt");

        public RPCClient Rpc { get; }

        private decimal PercentageDone { get; set; } = 0;
        private decimal PreviousPercentageDone { get; set; } = -1;

        public async Task ScanAsync(bool rescan)
        {
            if (rescan)
            {
                Logger.LogWarning("Rescanning...");
            }
            if (rescan && Directory.Exists(WorkFolder))
            {
                Directory.Delete(WorkFolder, true);
                Directory.CreateDirectory(WorkFolder);
            }

            ulong height = Constants.FirstJoinMarketBlock;
            if (File.Exists(LastProcessedBlockHeightPath))
            {
                height = ulong.Parse(File.ReadAllText(LastProcessedBlockHeightPath)) + 1;
                Logger.LogWarning($"{height - Constants.FirstJoinMarketBlock + 1} blocks already processed. Continue scanning...");
            }

            var bestHeight = (ulong)await Rpc.GetBlockCountAsync().ConfigureAwait(false);

            Logger.LogInfo($"Last processed block: {height - 1}.");
            ulong totalBlocks = bestHeight - height + 1;
            Logger.LogInfo($"About {totalBlocks} ({totalBlocks / 144} days) blocks will be processed.");

            while (true)
            {
                var blockTasks = new List<Task<VerboseBlockInfo>>();

                var batchClient = Rpc.PrepareBatch();
                // Default rpcworkqueue is 16 and GetBlockAsync is working with 2 requests.
                ulong parallelBlocks = 32;
                ulong maxHeight = Math.Min(height + parallelBlocks, bestHeight);
                ulong h;
                for (h = height; h < maxHeight; h++)
                {
                    blockTasks.Add(batchClient.GetVerboseBlockAsync(h));
                }
                if (blockTasks.Any())
                {
                    height = h - 1;
                }
                await batchClient.SendBatchAsync().ConfigureAwait(false);

                Parallel.ForEach(blockTasks, async (blockTask) =>
                {
                    var block = await blockTask.ConfigureAwait(false);

                    Parallel.ForEach(block.Transactions, (tx) =>
                    {

                    });
                });

                decimal totalBlocksPer100 = totalBlocks / 100m;
                ulong blocksLeft = bestHeight - height;
                ulong processedBlocks = totalBlocks - blocksLeft;
                PercentageDone = processedBlocks / totalBlocksPer100;
                bool displayProgress = (PercentageDone - PreviousPercentageDone) >= 1;
                if (displayProgress)
                {
                    Logger.LogInfo($"Progress: {(int)PercentageDone}%, Current height: {height}.");
                    PreviousPercentageDone = PercentageDone;
                }
                if (bestHeight <= height)
                {
                    // Refresh bestHeight and if still no new block, then end here.
                    bestHeight = (ulong)await Rpc.GetBlockCountAsync().ConfigureAwait(false);
                    if (bestHeight <= height)
                    {
                        break;
                    }
                }

                File.WriteAllText(LastProcessedBlockHeightPath, height.ToString());
                GC.Collect();

                height++;
            }
        }
    }
}
