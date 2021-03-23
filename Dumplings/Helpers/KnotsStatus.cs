﻿using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dumplings.Helpers
{
    public static class KnotsStatus
    {
        public static async Task CheckAsync(RPCClient client)
        {
            Logger.LogInfo("Checking Bitcoin Knots sync status...");

            var bci = await client.GetBlockchainInfoAsync();

            var missingBlocks = bci.Headers - bci.Blocks;
            if (missingBlocks != 0)
            {
                throw new InvalidOperationException($"Knots is not synchronized. Blocks missing: {missingBlocks}.");
            }

            Logger.LogInfo($"Bitcoin Knots is synchronized. Current height: {bci.Blocks}.");
        }
    }
}
