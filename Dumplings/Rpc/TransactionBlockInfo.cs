using Dumplings.Stats;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dumplings.Rpc
{
    public class TransactionBlockInfo
    {
        public TransactionBlockInfo(uint256 blockHash, DateTimeOffset? blockTime, uint? blockIndex)
        {
            BlockHash = blockHash;
            BlockTime = blockTime;
            BlockIndex = blockIndex;
        }

        public uint256 BlockHash { get; }
        public DateTimeOffset? BlockTime { get; }
        public YearMonthDay YearMonthDay => BlockTime.HasValue ? new YearMonthDay(BlockTime.Value.Year, BlockTime.Value.Month, BlockTime.Value.Day) : null;
        public uint? BlockIndex { get; }
    }
}
