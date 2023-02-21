#!/bin/bash

# exit when any command fails
set -e

# Time Period: 1 month (every beginning of month)
# GetAndSendWWCoordStats.sh will run Dumplings with the WasabiCoordStats and WabiSabiCoordStats commands, with an --outfolder parameter to save coordinator statistics data in file with the usual CSV format.
# These files needs to be sent to Balint at the beginning of every month.

# Make sure to have 'Config.txt' in the same file as this script.
# The following variables need to be imported from the config file:

# RPCUSER & RPCPASSWD = RPC credentials for Bitcoin Kntos,
# OUTFOLDER = Folder where Dumplings can save files with desired data.
# WW1XPUBS & WW2XPUBS = Coordinator Xpubs, separated by coma if more than one.

source Config.txt

echo "Getting Coordinator Stats"
dotnet run -c Release -- WasabiCoordStats --xpub=$WW1XPUBS --rpcuser=$RPCUSER --rpcpassword=$RPCPASSWD --outfolder=$OUTFOLDER --nowaitonexit --sync
dotnet run -c Release -- WabiSabiCoordStats --xpub=$WW2XPUBS --rpcuser=$RPCUSER --rpcpassword=$RPCPASSWD --outfolder=$OUTFOLDER --nowaitonexit

echo "Script Ended!"
