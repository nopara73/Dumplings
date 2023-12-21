#!/usr/bin/env bash

# exit when any command fails
set -e

# Make sure to have 'Config.txt' in the same file as this script.
# The following variables need to be imported from the config file:

# RPCUSER & RPCPASSWD = RPC credentials for Bitcoin Kntos,
# UCOUTFOLDER = Folder where Dumplings can save files with desired data.
# CONN = Connection string to database. ("Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;")

source Config.txt

if [[ -n "$RPCCOOKIEFILE" ]]; then
    authparams="--rpccookiefile=$RPCCOOKIEFILE"
else
    authparams="--rpcuser=$RPCUSER --rpcpassword=$RPCPASSWD"
fi

echo "Syncronizing blockchain"
dotnet run -c Release -- UnspentCapacity $authparams --outfolder=$UCOUTFOLDER --conn=$CONN --nowaitonexit --sync &>> /home/dumplings/Logs.txt

echo "Script Ended!"
