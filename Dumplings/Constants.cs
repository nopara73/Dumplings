using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dumplings
{
    public static class Constants
    {
        /// <summary>
        /// January 09, 2015 Bitcointalk announcement - https://bitcointalk.org/index.php?topic=919116.msg10096718
        /// </summary>
        public const ulong FirstJoinMarketBlock = 392300;
        /// <summary>
        /// July 06, 2018 Reddit announcement - https://old.reddit.com/r/Bitcoin/comments/8wk63k/wasabi_privacy_focused_bitcoin_wallet_for_desktop/
        /// </summary>
        public const ulong FirstWasabiBlock = 530500;

        public const ulong FirstWabiSabiBlock = 741200;
        /// <summary>
        /// June 17, 2019 Reddit announcement - https://old.reddit.com/r/Bitcoin/comments/c1niri/samourai_whirlpool_coinjoin_implementation_is_now/
        /// </summary>
        public const ulong FirstSamouraiBlock = 570000;

        public const ulong FirstWasabiNoCoordAddressBlock = 610000;

        public static IEnumerable<Script> WasabiCoordScripts = new Script[]
        {
            BitcoinAddress.Create("bc1qs604c7jv6amk4cxqlnvuxv26hv3e48cds4m0ew", Network.Main).ScriptPubKey,
            BitcoinAddress.Create("bc1qa24tsgchvuxsaccp8vrnkfd85hrcpafg20kmjw", Network.Main).ScriptPubKey
        };

        public static IEnumerable<Script> WabiSabiCoordScripts = new Script[]
        {
            BitcoinAddress.Create("bc1qsnynr0nfa233q7s8k495ank2nwc0xw4zq2c6d2", Network.Main).ScriptPubKey,
            BitcoinAddress.Create("bc1qqgfu8x6c4fg8vtt0zfjl0uj6m9c853p9lfs9c8", Network.Main).ScriptPubKey
        };

        public static Money ApproximateWasabiBaseDenomination = Money.Coins(0.1m);
        public static Money WasabiBaseDenominationPrecision = Money.Coins(0.02m);

        public static IEnumerable<Money> SamouraiPools = new Money[]
        {
            Money.Coins(0.01m),
            Money.Coins(0.05m),
            Money.Coins(0.5m)
        };

        public static long[] StdDenoms = new[]
        {
            5000L, 6561L, 8192L, 10000L, 13122L, 16384L, 19683L, 20000L, 32768L, 39366L, 50000L, 59049L, 65536L, 100000L, 118098L,
            131072L, 177147L, 200000L, 262144L, 354294L, 500000L, 524288L, 531441L, 1000000L, 1048576L, 1062882L, 1594323L, 2000000L,
            2097152L, 3188646L, 4194304L, 4782969L, 5000000L, 8388608L, 9565938L, 10000000L, 14348907L, 16777216L, 20000000L,
            28697814L, 33554432L, 43046721L, 50000000L, 67108864L, 86093442L, 100000000L, 129140163L, 134217728L, 200000000L,
            258280326L, 268435456L, 387420489L, 500000000L, 536870912L, 774840978L, 1000000000L, 1073741824L, 1162261467L,
            2000000000L, 2147483648L, 2324522934L, 3486784401L, 4294967296L, 5000000000L, 6973568802L, 8589934592L, 10000000000L,
            10460353203L, 17179869184L, 20000000000L, 20920706406L, 31381059609L, 34359738368L, 50000000000L, 62762119218L,
            68719476736L, 94143178827L, 100000000000L, 137438953472L
        };
    }
}
