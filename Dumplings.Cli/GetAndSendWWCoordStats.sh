#!/bin/bash

# exit when any command fails
set -e

# Make sure to have 'Config.txt' in the same file as this script.
# The following variables need to be imported from the config file:

# RPCUSER & RPCPASSWD = RPC credentials for Bitcoin Kntos,
# OUTFOLDER = Folder where Dumplings can save files with desired data.
# WW1XPUBS & WW2XPUBS = Coordinator Xpubs, separated by coma if more than one.

source Config.txt

echo "Getting Coordinator Stats"
dotnet run -c Release -- WasabiCoordStats --xpub=$WW1XPUBS --rpcuser=$RPCUSER --rpcpassword=$RPCPASSWD --outfolder=$OUTFOLDER --nowaitonexit
dotnet run -c Release -- WabiSabiCoordStats --xpub=$WW2XPUBS --rpcuser=$RPCUSER --rpcpassword=$RPCPASSWD --outfolder=$OUTFOLDER --nowaitonexit

# Send files from OUTFOLDER to email address

echo "Script Ended!"
