using Dumplings.Helpers;
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
            // Make sure all post mix txs are post mix txs.
        }

        private void CheckSamouraiTx0Completeness()
        {
            using (BenchmarkLogger.Measure())
            {
                Logger.LogInfo("Making sure that all Samourai TX0 have been found by making sure all inputs in Samourai CoinJoins are coming from found TX0 transactions.");
                var missed = new HashSet<uint256>();
                foreach (var inputTxId in ScannerFiles.SamouraiCoinJoins.SelectMany(x => x.Inputs).Select(x => x.OutPoint.Hash))
                {
                    if (!ScannerFiles.SamouraiTx0S.Select(x => x.Id).Contains(inputTxId))
                    {
                        missed.Add(inputTxId);
                    }
                }

                if(missed.Any())
                {
                    Logger.LogWarning($"Missed {missed.Count} Samourai TX0 transactions. This is {Math.per}")
                }
                else
                {
                    Logger.LogInfo("Successfully found all Samourai TX0 transactions");
                }
            }
        }
    }
}
