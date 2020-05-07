# Setup & Run

1. Get Git: https://git-scm.com/downloads
1. Get .NET Core SDK: https://www.microsoft.com/net/download
1. Get Bitcoin Knots: https://bitcoinknots.org/
1. Make sure your bitcoin.conf has the following entries: `txindex = 1`, `server = 1`, `rpcuser = user`, `rpcpassword = password`, `listen = 1`, `rpcworkqueue = 128`
1. Run Knots and wait until it's synchronized.
1. `git clone https://github.com/nopara73/Dumplings.git`
1. `cd Dumplings/Dumplings.Cli`
1. `dotnet run -c Release -- sync --rpcuser=user --rpcpassword=password`

# Usage

1. Run Knots, wait until it's synchronized.
1. `cd Dumplings/Dumplings.Cli`

## Synchronization

In order to run other commands and local storage to be up to date you have to synchronize Dumplings. You can synchronize it with the `sync` command. This will go through the blockchain and find and save transactions that other commands work with.

`dotnet run -c Release -- sync --rpcuser=user --rpcpassword=password`

If you want to resync, then use the `resync` command.

`dotnet run -c Release -- resync --rpcuser=user --rpcpassword=password`