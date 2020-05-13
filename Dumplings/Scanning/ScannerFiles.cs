using Dumplings.Rpc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dumplings.Scanning
{
    public class ScannerFiles
    {
        public ScannerFiles(
            ulong bestHeight,
            IEnumerable<VerboseTransactionInfo> wasabiCoinJoins,
            IEnumerable<VerboseTransactionInfo> samouraiCoinJoins,
            IEnumerable<VerboseTransactionInfo> otherCoinJoins,
            IEnumerable<VerboseTransactionInfo> samouraiTx0s,
            IEnumerable<VerboseTransactionInfo> wasabiPostMixTxs,
            IEnumerable<VerboseTransactionInfo> samouraiPostMixTxs,
            IEnumerable<VerboseTransactionInfo> otherCoinJoinPostMixTxs)
        {
            BestHeight = bestHeight;
            WasabiCoinJoins = wasabiCoinJoins;
            SamouraiCoinJoins = samouraiCoinJoins;
            OtherCoinJoins = otherCoinJoins;
            SamouraiTx0S = samouraiTx0s;
            WasabiPostMixTxs = wasabiPostMixTxs;
            SamouraiPostMixTxs = samouraiPostMixTxs;
            OtherCoinJoinPostMixTxs = otherCoinJoinPostMixTxs;
        }

        public ulong BestHeight { get; }
        public IEnumerable<VerboseTransactionInfo> WasabiCoinJoins { get; }
        public IEnumerable<VerboseTransactionInfo> SamouraiCoinJoins { get; }
        public IEnumerable<VerboseTransactionInfo> OtherCoinJoins { get; }
        public IEnumerable<VerboseTransactionInfo> SamouraiTx0S { get; }
        public IEnumerable<VerboseTransactionInfo> WasabiPostMixTxs { get; }
        public IEnumerable<VerboseTransactionInfo> SamouraiPostMixTxs { get; }
        public IEnumerable<VerboseTransactionInfo> OtherCoinJoinPostMixTxs { get; }
    }
}
