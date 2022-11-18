# Setup & Run

1. Get Git: https://git-scm.com/downloads
1. Get .NET Core SDK: https://www.microsoft.com/net/download
1. Get Bitcoin Core: https://bitcoincore.org/ or Bitcoin Knots: https://bitcoinknots.org/
1. Make sure your bitcoin.conf has the following entries: `txindex = 1`, `server = 1`, `rpcuser = user`, `rpcpassword = password`, `listen = 1`, `rpcworkqueue = 128`
1. Run Bitcoin and wait until it's synchronized.
1. `git clone https://github.com/nopara73/Dumplings.git`
1. `cd Dumplings/Dumplings.Cli`
1. `dotnet run -- sync --rpcuser=user --rpcpassword=password`

# Usage

1. Run Bitcoin, wait until it's synchronized.
1. `cd Dumplings/Dumplings.Cli`

## 1. Synchronization

In order to run other commands and local storage to be up to date you have to synchronize Dumplings. You can synchronize it with the `sync` command. This will go through the blockchain and find and save transactions that other commands work with.

`dotnet run -- sync --rpcuser=user --rpcpassword=password`

If you want to resync, then use the `resync` command.

`dotnet run -- resync --rpcuser=user --rpcpassword=password`

## 2. Integrity Checks

After you synchronized you probably want to do some data integrity checks too:

`dotnet run -- check --rpcuser=user --rpcpassword=password`

## 3. Creating Statistics

The software can create statistics after it synchronized. Synchronization will create a folder, called `Scanner` and some files in it:

![](https://i.imgur.com/h7qhWRA.png)

From these files you can create many kind of statistics quickly or just create the existing statistics. You can download a Scanner folder that's synced up to 2020-05-22 from [here](https://drive.google.com/open?id=1IFugzv9DT8GSztw8piS3kAB2eRolhS_t).

The expected location of the scanner folder is `Dumplings\Dumplings.Cli\bin\Debug\netcoreapp3.1\Scanner` on Windows and `Dumplings/Dumplings.Cli/Scanner/` on Linux (and probably on OSX.)

After synchronization you can create statistics as follows: `dotnet run -- COMMAND --rpcuser=user --rpcpassword=password`, where the `COMMAND` is the command for the statistics you want to create.

# Commands & Results

## MonthlyVolumes

Total monthly volume of CoinJoin transactions.

![](https://i.imgur.com/RG5rfmD.png)
![](https://i.imgur.com/edcV7Vg.png)

## FreshBitcoins

> Best proxy for user adoption.

How many non-remixed bitcoins get to CoinJoined monthly.

![](https://i.imgur.com/IPg0F8c.png)

Average count of remixes is a derived metric, which shows how many times a bitcoin participated in mixes in average: `total volume/fresh bitcoins`.

![](https://i.imgur.com/8MlTyWr.png)

## NeverMixed

Bitcoins those were intended to be mixed, but were not.

- For Wasabi and Otheri, it counts CoinJoin change outputs those have been spent, but not in other CoinJoins. (Note 1: the software does not currently examine if the change is coming from CoinJoined inputs. Note 2: more related metrics and more detailed explanation on Wasabi's inefficiency can be found under the [WasabiCjEfficiency repository](https://github.com/nopara73/WasabiCjEfficiency).)
- For Samuri, it counts TX0 transaction outputs those have been spent, but not in other TX0 or coinjoin transactions.

The following is a derived metric: it's the percentage of nevermixed coins to fresh bitcoins.

![](https://i.imgur.com/aGD81yP.png)

# PostMixConsolidation

Average number of inputs in the first non-coinjoin transactions after coinjoins.

![](https://i.imgur.com/GT2pwu9.png)

# FAQ

## What does Wasabi, Samuri, Otheri mean?

- Wasabi: Wasabi Wallet ZeroLink CoinJoins.
- Samuri: Samourai Wallet Whirlpool CoinJoins.
- Otheri: Other CoinJoin looking transactions.

## What about JoinMarket?

[JoinMarket transactions are not obvious to identify with high accuracy solely from Blockchain data.](https://github.com/nopara73/WasabiVsSamourai/issues/2) To identify JM transactions one would need to listen to JM orderbook, which is off-chain data, so that is outside the scope of Dumplings. BlockSci made the best known attempt to identify them, which had only a 94% accuracy rate, which included computationally expensive subsetsum problem solving and left out larger JoinMarket transactions. Leaving out larger JM transactions would lead to misleading results and graphs, so I opted to not do that.

In summary: **Every JoinMarket transactions are "Other CoinJoin Transactions," but not every "Other CoinJoin Transactions" are JoinMarket transactions.**

## What about network level privacy?

**Not all CoinJoins are created equal.** Statistics like this gives the wrong impression that a Samurai CJ is just as good as a Wasabi or a JoinMarket CJ. This is not the case. Just like Blockchain.info's SharedCoin CoinJoins weren't, Samourai CoinJoins aren't trustless either, as these companies know your **xpub,** from which all your present, past and future Bitcoin addresses can be derived, thus users gain no privacy against these companies and the third parties they share your data with, even if they CoinJoin.
