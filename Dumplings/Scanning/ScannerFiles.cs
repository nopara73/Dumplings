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
            IEnumerable<VerboseTransactionInfo> wasabiCoinJoins,
            IEnumerable<VerboseTransactionInfo> wabiSabiCoinJoins,
            IEnumerable<VerboseTransactionInfo> samouraiCoinJoins,
            IEnumerable<VerboseTransactionInfo> otherCoinJoins,
            IEnumerable<VerboseTransactionInfo> samouraiTx0s,
            IEnumerable<VerboseTransactionInfo> wasabiPostMixTxs,
            IEnumerable<VerboseTransactionInfo> samouraiPostMixTxs,
            IEnumerable<VerboseTransactionInfo> otherCoinJoinPostMixTxs)
        {
            BestHeight = bestHeight;

            WasabiCoinJoins = wasabiCoinJoins.ToArray();
            SamouraiCoinJoins = samouraiCoinJoins.ToArray();
            OtherCoinJoins = otherCoinJoins.ToArray();
            SamouraiTx0s = samouraiTx0s.ToArray();
            WasabiPostMixTxs = wasabiPostMixTxs.ToArray();
            WabiSabiCoinJoins = wabiSabiCoinJoins.ToArray();
            SamouraiPostMixTxs = samouraiPostMixTxs.ToArray();
            OtherCoinJoinPostMixTxs = otherCoinJoinPostMixTxs.ToArray();

            WasabiCoinJoinHashes = WasabiCoinJoins.Select(x => x.Id).ToArray();
            WabiSabiCoinJoinHashes = WabiSabiCoinJoins.Select(x => x.Id).ToArray();
            SamouraiCoinJoinHashes = SamouraiCoinJoins.Select(x => x.Id).ToArray();
            OtherCoinJoinHashes = OtherCoinJoins.Select(x => x.Id).ToArray();
            SamouraiTx0Hashes = SamouraiTx0s.Select(x => x.Id).ToArray();
            WasabiPostMixTxHashes = WasabiPostMixTxs.Select(x => x.Id).ToArray();
            SamouraiPostMixTxHashes = SamouraiPostMixTxs.Select(x => x.Id).ToArray();
            OtherCoinJoinPostMixTxHashes = OtherCoinJoinPostMixTxs.Select(x => x.Id).ToArray();
        }

        public ulong BestHeight { get; }
        public IEnumerable<VerboseTransactionInfo> WasabiCoinJoins { get; }
        public IEnumerable<VerboseTransactionInfo> WabiSabiCoinJoins { get; }

        public IEnumerable<VerboseTransactionInfo> SamouraiCoinJoins { get; }
        public IEnumerable<VerboseTransactionInfo> OtherCoinJoins { get; }
        public IEnumerable<VerboseTransactionInfo> SamouraiTx0s { get; }
        public IEnumerable<VerboseTransactionInfo> WasabiPostMixTxs { get; }
        public IEnumerable<VerboseTransactionInfo> SamouraiPostMixTxs { get; }
        public IEnumerable<VerboseTransactionInfo> OtherCoinJoinPostMixTxs { get; }

        public IEnumerable<uint256> WasabiCoinJoinHashes { get; }
        public IEnumerable<uint256> WabiSabiCoinJoinHashes { get; }
        public IEnumerable<uint256> SamouraiCoinJoinHashes { get; }
        public IEnumerable<uint256> OtherCoinJoinHashes { get; }
        public IEnumerable<uint256> SamouraiTx0Hashes { get; }
        public IEnumerable<uint256> WasabiPostMixTxHashes { get; }
        public IEnumerable<uint256> SamouraiPostMixTxHashes { get; }
        public IEnumerable<uint256> OtherCoinJoinPostMixTxHashes { get; }
    }
}
