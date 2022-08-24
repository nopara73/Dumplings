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
        /// July 06, 2018 Twitter announcement - https://twitter.com/wasabiwallet/status/1537911130718228480
        /// </summary>
        public const ulong FirstWasabi2Block = 741213;

        /// <summary>
        /// July 06, 2018 Reddit announcement - https://old.reddit.com/r/Bitcoin/comments/8wk63k/wasabi_privacy_focused_bitcoin_wallet_for_desktop/
        /// </summary>
        public const ulong FirstWasabiBlock = 530500;

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

        public static Money ApproximateWasabiBaseDenomination = Money.Coins(0.1m);
        public static Money WasabiBaseDenominationPrecision = Money.Coins(0.02m);

        public static IEnumerable<Money> SamouraiPools = new Money[]
        {
            Money.Coins(0.001m),
            Money.Coins(0.01m),
            Money.Coins(0.05m),
            Money.Coins(0.5m)
        };
    }
}
