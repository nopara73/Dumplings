using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dumplings.Helpers
{
    public static class BitcoinStatus
    {
        public static async Task CheckAsync(RPCClient client)
        {
            Logger.LogInfo("Checking Bitcoin sync status...");

            var bci = await client.GetBlockchainInfoAsync();

            var missingBlocks = bci.Headers - bci.Blocks;
            if (missingBlocks != 0)
            {
                throw new InvalidOperationException($"Bitcoin is not synchronized. Blocks missing: {missingBlocks}.");
            }

            Logger.LogInfo($"Bitcoin is synchronized. Current height: {bci.Blocks}.");
        }
    }
}
