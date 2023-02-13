#!/bin/bash

# exit when any command fails
set -e

# Time Period: 1 day (every midnight)
# Starting the RunAndSendDumplings.sh script syncs and runs Dumplings with the Upload command, updating the stats website with fresh data.

# Make sure to have 'Config.txt' in the same file as this script.
# The following variables need to be imported from the config file:

# RPCUSER & RPCPASSWD = RPC credentials for Bitcoin Kntos,
# CONN = Connection string to database. ("Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;")

source Config.txt

echo "Starting Upload Script!"
dotnet run -c Release -- Upload --rpcuser=$RPCUSER --rpcpassword=$RPCPASSWD --conn=$CONN --nowaitonexit

echo "Script Ended!"
