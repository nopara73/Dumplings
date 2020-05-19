using Dumplings.Helpers;
using Dumplings.Rpc;
using Dumplings.Scanning;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumplings.Checking
{
    public class Checker
    {
        public Checker(ScannerFiles scannerFiles)
        {
            ScannerFiles = scannerFiles;
        }

        public ScannerFiles ScannerFiles { get; }

        public void Check()
        {
            // Make sure we found all Samourai TX0s.
            CheckSamouraiTx0Completeness();
            // Make sure no duplicated transactions have been saved.
            CheckDuplication();
            // Make sure no intesecting coinjoins were identified.
            CheckIntersection();
        }

        private void CheckIntersection()
        {
            using (BenchmarkLogger.Measure())
            {
                CheckIntersections(ScannerFiles.WasabiCoinJoinHashes, nameof(ScannerFiles.WasabiCoinJoins), ScannerFiles.SamouraiCoinJoinHashes, nameof(ScannerFiles.SamouraiCoinJoins));
                CheckIntersections(ScannerFiles.WasabiCoinJoinHashes, nameof(ScannerFiles.WasabiCoinJoins), ScannerFiles.OtherCoinJoinHashes, nameof(ScannerFiles.OtherCoinJoins));

                CheckIntersections(ScannerFiles.SamouraiCoinJoinHashes, nameof(ScannerFiles.SamouraiCoinJoins), ScannerFiles.OtherCoinJoinHashes, nameof(ScannerFiles.OtherCoinJoins));

                CheckIntersections(ScannerFiles.SamouraiTx0Hashes, nameof(ScannerFiles.SamouraiTx0s), ScannerFiles.WasabiCoinJoinHashes, nameof(ScannerFiles.WasabiCoinJoins));
                CheckIntersections(ScannerFiles.SamouraiTx0Hashes, nameof(ScannerFiles.SamouraiTx0s), ScannerFiles.SamouraiCoinJoinHashes, nameof(ScannerFiles.SamouraiCoinJoins));
                CheckIntersections(ScannerFiles.SamouraiTx0Hashes, nameof(ScannerFiles.SamouraiTx0s), ScannerFiles.OtherCoinJoinHashes, nameof(ScannerFiles.OtherCoinJoins));

                CheckIntersections(ScannerFiles.WasabiPostMixTxHashes, nameof(ScannerFiles.WasabiPostMixTxs), ScannerFiles.OtherCoinJoinHashes, nameof(ScannerFiles.OtherCoinJoins));
                CheckIntersections(ScannerFiles.SamouraiPostMixTxHashes, nameof(ScannerFiles.SamouraiPostMixTxs), ScannerFiles.OtherCoinJoinHashes, nameof(ScannerFiles.OtherCoinJoins));

                var wasabiSamouraiCommonPostMixKnownExceptions = new[]
                {
                    new uint256("52025ff6a0ace2790fb56fbc1283a28827e4e774723999685f29feb81fb43c4d")
                };
                CheckIntersections(ScannerFiles.WasabiPostMixTxHashes.Except(wasabiSamouraiCommonPostMixKnownExceptions), nameof(ScannerFiles.WasabiPostMixTxs), ScannerFiles.SamouraiPostMixTxHashes.Except(wasabiSamouraiCommonPostMixKnownExceptions), nameof(ScannerFiles.SamouraiPostMixTxs));
            }
        }

        private void CheckIntersections(IEnumerable<uint256> txs1, string txs1Name, IEnumerable<uint256> txs2, string txs2Name)
        {
            var common = txs1.Intersect(txs2);
            if (common.Any())
            {
                Logger.LogWarning($"Common transactions found in {txs1Name} and {txs2Name}.");
                foreach (var txid in common)
                {
                    Logger.LogInfo($"Common: {txid}.");
                }
            }
            else
            {
                Logger.LogInfo($"Success! No common transactions found in {txs1Name} and {txs2Name}.");
            }
        }

        private void CheckDuplication()
        {
            using (BenchmarkLogger.Measure())
            {
                CheckDuplications(ScannerFiles.WasabiCoinJoinHashes, nameof(ScannerFiles.WasabiCoinJoins));
                CheckDuplications(ScannerFiles.SamouraiCoinJoinHashes, nameof(ScannerFiles.SamouraiCoinJoins));
                CheckDuplications(ScannerFiles.OtherCoinJoinHashes, nameof(ScannerFiles.OtherCoinJoins));
                CheckDuplications(ScannerFiles.SamouraiTx0Hashes, nameof(ScannerFiles.SamouraiTx0s));
                CheckDuplications(ScannerFiles.WasabiPostMixTxHashes, nameof(ScannerFiles.WasabiPostMixTxs));
                CheckDuplications(ScannerFiles.SamouraiPostMixTxHashes, nameof(ScannerFiles.SamouraiPostMixTxs));
                CheckDuplications(ScannerFiles.OtherCoinJoinPostMixTxHashes, nameof(ScannerFiles.OtherCoinJoinPostMixTxs));
            }
        }

        private void CheckDuplications(IEnumerable<uint256> txs, string where)
        {
            var txids = txs.ToArray();
            var duplicated = new HashSet<uint256>();
            for (int i = 0; i < txids.Length; i++)
            {
                var current = txids[i];
                for (int j = i + 1; j < txids.Length; j++)
                {
                    var another = txids[j];
                    if (current == another)
                    {
                        duplicated.Add(current);
                        break;
                    }
                }
            }

            if (duplicated.Any())
            {
                Logger.LogWarning($"Duplicated transactions found in {where}.");
                foreach (var txid in duplicated)
                {
                    Logger.LogInfo($"Duplicated: {txid}.");
                }
            }
            else
            {
                Logger.LogInfo($"Success! No duplicated transactions found in {where}.");
            }
        }

        private void CheckSamouraiTx0Completeness()
        {
            using (BenchmarkLogger.Measure())
            {
                var missed = new HashSet<uint256>();
                var cjInputs = ScannerFiles.SamouraiCoinJoins.SelectMany(x => x.Inputs).Select(x => x.OutPoint.Hash).ToHashSet();
                var tx0AndCjHashes = ScannerFiles.SamouraiTx0Hashes.Union(ScannerFiles.SamouraiCoinJoinHashes).ToHashSet();
                foreach (var inputTxId in cjInputs.Except(tx0AndCjHashes))
                {
                    missed.Add(inputTxId);
                    Logger.LogInfo($"Missed TX0: {inputTxId}.");
                }

                if (missed.Any())
                {
                    Logger.LogWarning($"Missed {missed.Count} Samourai TX0 transactions out of {cjInputs.Count} total coinjoin inputs.");
                }
                else
                {
                    Logger.LogInfo("Success! All Samourai TX0 transactions are found.");
                }
            }
        }
    }
}
