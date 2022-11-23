using Dumplings.Rpc;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumplings.Scanning
{
    public class ScannerFiles
    {
        public ScannerFiles(
            ulong bestHeight,
            IEnumerable<VerboseTransactionInfo> wasabi2CoinJoins,
            IEnumerable<VerboseTransactionInfo> wasabiCoinJoins,
            IEnumerable<VerboseTransactionInfo> samouraiCoinJoins,
            IEnumerable<VerboseTransactionInfo> otherCoinJoins,
            IEnumerable<VerboseTransactionInfo> samouraiTx0s,
            IEnumerable<VerboseTransactionInfo> wasabi2PostMixTxs,
            IEnumerable<VerboseTransactionInfo> wasabiPostMixTxs,
            IEnumerable<VerboseTransactionInfo> samouraiPostMixTxs,
            IEnumerable<VerboseTransactionInfo> otherCoinJoinPostMixTxs)
        {
            BestHeight = bestHeight;

            Wasabi2CoinJoins = wasabi2CoinJoins.ToArray();
            WasabiCoinJoins = wasabiCoinJoins.ToArray();
            SamouraiCoinJoins = samouraiCoinJoins.ToArray();
            OtherCoinJoins = otherCoinJoins.ToArray();
            SamouraiTx0s = samouraiTx0s.ToArray();
            Wasabi2PostMixTxs = wasabi2PostMixTxs.ToArray();
            WasabiPostMixTxs = wasabiPostMixTxs.ToArray();
            SamouraiPostMixTxs = samouraiPostMixTxs.ToArray();
            OtherCoinJoinPostMixTxs = otherCoinJoinPostMixTxs.ToArray();

            WasabiCoinJoinHashes = WasabiCoinJoins.Select(x => x.Id).ToArray();
            Wasabi2CoinJoinHashes = Wasabi2CoinJoins.Select(x => x.Id).ToArray();
            SamouraiCoinJoinHashes = SamouraiCoinJoins.Select(x => x.Id).ToArray();
            OtherCoinJoinHashes = OtherCoinJoins.Select(x => x.Id).ToArray();
            SamouraiTx0Hashes = SamouraiTx0s.Select(x => x.Id).ToArray();
            WasabiPostMixTxHashes = WasabiPostMixTxs.Select(x => x.Id).ToArray();
            Wasabi2PostMixTxHashes = Wasabi2PostMixTxs.Select(x => x.Id).ToArray();
            SamouraiPostMixTxHashes = SamouraiPostMixTxs.Select(x => x.Id).ToArray();
            OtherCoinJoinPostMixTxHashes = OtherCoinJoinPostMixTxs.Select(x => x.Id).ToArray();
        }

        public ulong BestHeight { get; }
        public IEnumerable<VerboseTransactionInfo> WasabiCoinJoins { get; }
        public IEnumerable<VerboseTransactionInfo> Wasabi2CoinJoins { get; }
        public IEnumerable<VerboseTransactionInfo> SamouraiCoinJoins { get; }
        public IEnumerable<VerboseTransactionInfo> OtherCoinJoins { get; }
        public IEnumerable<VerboseTransactionInfo> SamouraiTx0s { get; }
        public IEnumerable<VerboseTransactionInfo> WasabiPostMixTxs { get; }
        public IEnumerable<VerboseTransactionInfo> Wasabi2PostMixTxs { get; }
        public IEnumerable<VerboseTransactionInfo> SamouraiPostMixTxs { get; }
        public IEnumerable<VerboseTransactionInfo> OtherCoinJoinPostMixTxs { get; }

        public IEnumerable<uint256> WasabiCoinJoinHashes { get; }
        public IEnumerable<uint256> Wasabi2CoinJoinHashes { get; }
        public IEnumerable<uint256> SamouraiCoinJoinHashes { get; }
        public IEnumerable<uint256> OtherCoinJoinHashes { get; }
        public IEnumerable<uint256> SamouraiTx0Hashes { get; }
        public IEnumerable<uint256> WasabiPostMixTxHashes { get; }
        public IEnumerable<uint256> Wasabi2PostMixTxHashes { get; }
        public IEnumerable<uint256> SamouraiPostMixTxHashes { get; }
        public IEnumerable<uint256> OtherCoinJoinPostMixTxHashes { get; }
    }
}
