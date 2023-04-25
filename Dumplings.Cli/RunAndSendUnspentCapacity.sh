#!/bin/bash

# exit when any command fails
set -e

# Make sure to have 'Config.txt' in the same file as this script.
# The following variables need to be imported from the config file:

# RPCUSER & RPCPASSWD = RPC credentials for Bitcoin Kntos,
# UCOUTFOLDER = Folder where Dumplings can save files with desired data.
# CONN = Connection string to database. ("Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;")

source Config.txt

echo "Syncronizing blockchain"
dotnet run -c Release -- UnspentCapacity --rpcuser=$RPCUSER --rpcpassword=$RPCPASSWD --outfolder=$UCOUTFOLDER --conn=$CONN --nowaitonexit --sync &>> /home/dumplings/Logs.txt

echo "Script Ended!"
