using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dumplings.Stats
{
    public class Coin
    {
        public Coin(DateTimeOffset blockTime, uint256 txid, uint index, Script script, Money amount)
        {
            BlockTime = blockTime;
            Txid = txid;
            Index = index;
            Script = script;
            Amount = amount;
        }

        public DateTimeOffset BlockTime { get; }
        public uint256 Txid { get; }
        public uint Index { get; }
        public Script Script { get; }
        public Money Amount { get; }

        public override string ToString() => $"{BlockTime.UtcTicks}::{Txid}::{Index}::{Script}::{Amount}";

        public static Coin FromString(string coinString)
        {
            var parts = coinString.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            return new(
                new DateTimeOffset(long.Parse(parts[0]), TimeSpan.Zero),
                uint256.Parse(parts[1]),
                uint.Parse(parts[2]),
                new Script(parts[3]),
                Money.Parse(parts[4]));
        }
    }
}
