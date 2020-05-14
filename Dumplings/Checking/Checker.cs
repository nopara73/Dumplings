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
        }

        private void CheckDuplication()
        {
            CheckDuplications(ScannerFiles.WasabiCoinJoins, nameof(ScannerFiles.WasabiCoinJoins));
            CheckDuplications(ScannerFiles.SamouraiCoinJoins, nameof(ScannerFiles.SamouraiCoinJoins));
            CheckDuplications(ScannerFiles.OtherCoinJoins, nameof(ScannerFiles.OtherCoinJoins));
            CheckDuplications(ScannerFiles.SamouraiTx0s, nameof(ScannerFiles.SamouraiTx0s));
            CheckDuplications(ScannerFiles.WasabiPostMixTxs, nameof(ScannerFiles.WasabiPostMixTxs));
            CheckDuplications(ScannerFiles.SamouraiPostMixTxs, nameof(ScannerFiles.SamouraiPostMixTxs));
            CheckDuplications(ScannerFiles.OtherCoinJoinPostMixTxs, nameof(ScannerFiles.OtherCoinJoinPostMixTxs));
        }

        private void CheckDuplications(IEnumerable<VerboseTransactionInfo> txs, string where)
        {
            using (BenchmarkLogger.Measure())
            {
                var txids = txs.Select(x => x.Id).ToArray();
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
                        Logger.LogWarning($"Duplicated: {txid}");
                    }
                }
                else
                {
                    Logger.LogWarning($"No duplicated transactions found in {where}.");
                }
            }
        }

        private void CheckSamouraiTx0Completeness()
        {
            using (BenchmarkLogger.Measure())
            {
                Logger.LogInfo("Making sure that all Samourai TX0 have been found by making sure all inputs in Samourai CoinJoins are coming from found TX0 transactions.");
                var missed = new HashSet<uint256>();
                var cjInputs = ScannerFiles.SamouraiCoinJoins.SelectMany(x => x.Inputs).Select(x => x.OutPoint.Hash).ToHashSet();
                foreach (var inputTxId in cjInputs)
                {
                    if (!ScannerFiles.SamouraiTx0s.Select(x => x.Id).Contains(inputTxId) && !ScannerFiles.SamouraiCoinJoins.Select(x => x.Id).Contains(inputTxId))
                    {
                        missed.Add(inputTxId);
                        Logger.LogWarning($"Missed TX0: {inputTxId}.");
                    }
                }

                if (missed.Any())
                {
                    Logger.LogWarning($"Missed {missed.Count} Samourai TX0 transactions out of {cjInputs.Count} total coinjoin inputs.");
                }
                else
                {
                    Logger.LogInfo("Successfully found all Samourai TX0 transactions");
                }
            }
        }
    }
}
