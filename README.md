# Setup & Run

1. Get Git: https://git-scm.com/downloads
1. Get .NET Core SDK: https://www.microsoft.com/net/download
1. Get Bitcoin Knots: https://bitcoinknots.org/
1. Make sure your bitcoin.conf has the following entries: `txindex = 1`, `server = 1`, `rpcuser = user`, `rpcpassword = password`, `listen = 1`, `rpcworkqueue = 128`
1. Run Knots and wait until it's synchronized.
1. `git clone https://github.com/nopara73/Dumplings.git`
1. `cd Dumplings/Dumplings.Cli`
1. `dotnet run -- sync --rpcuser=user --rpcpassword=password`

# Usage

1. Run Knots, wait until it's synchronized.
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

![](https://i.imgur.com/wNyMFDx.png)

From these files you can create many kind of statistics quickly or just create the existing statistics. You can download a Scanner folder that's synced up to 2020-05-19 from [here](https://drive.google.com/open?id=1jKM19xMKsm1fwA_HbgXfDwYGjUpLLacU).

The expected location of the scanner folder is `Dumplings\Dumplings.Cli\bin\Debug\netcoreapp3.1\Scanner` on Windows and `Dumplings/Dumplings.Cli/Scanner/` on Linux (and probably on OSX.)

After synchronization you can create statistics as follows: `dotnet run -- COMMAND --rpcuser=user --rpcpassword=password`, where the `COMMAND` is the command for the statistics you want to create.
