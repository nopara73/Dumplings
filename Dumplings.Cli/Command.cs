using System;
using System.Collections.Generic;
using System.Text;

namespace Dumplings.Cli
{
    public enum Command
    {
        Sync,                       // Syncs the Scanner files from where it was left off.
        Resync,                     // Resync the Scanner files from Wasabi's launch.
        Check,                      // Cross checks the Scanner files to make sure of no bugs.
        MonthlyVolumes,             // Calculate the monthly volumes of different kind of coinjoins.
        FreshBitcoins,              // Calculate how much previously not coinjoined bitcoins come to different kind of coinjoins monthly.
        FreshBitcoinAmounts,        // Create a list of amounts for backport release.
        FreshBitcoinsDaily,         // Calculate how much previously not coinjoined bitcoins come to different kind of coinjoins daily.
        NeverMixed,                 // Calculate monthly volume of bitcoins those were intended to be mixed, but never got mixed.
        CoinJoinEquality,           // Calculate monthly volume of equality gained on bitcoins.
        CoinJoinIncome,             // Calculate monthly income of Wasabi and Samuri.
        PostMixConsolidation,       // Calculate monthly average post-mix consolidation input count.
        SmallerThanMinimum,         // Calculate monthly average percentage of smaller than minimum utxos in Wasabi coinjoins.
        MonthlyEqualVolumes,        // Calculate the monthly volumes of different kind of coinjoins of equal values.
        AverageUserCount,
        AverageNetworkFeePaidByUserPerCoinjoin,
        Records,
        UniqueCountPercent,
        ListFreshBitcoins,          // Lists fresh bitcoins by wallet.
        UnspentCapacity,
        Sake                        // Statistics relevant to set Sake parameters.
    }
}
