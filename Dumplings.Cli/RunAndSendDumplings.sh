#!/usr/bin/env bash

# exit when any command fails
set -e

# Time Period: 1 day (every midnight)
# Starting the RunAndSendDumplings.sh script syncs and runs Dumplings with the Upload command, updating the stats website with fresh data.

# Make sure to have 'Config.txt' in the same file as this script.
# The following variables need to be imported from the config file:

# RPCUSER & RPCPASSWD = RPC credentials for Bitcoin Kntos,
# CONN = Connection string to database. ("Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;")

source Config.txt

if [[ -n "$RPCCOOKIEFILE" ]]; then
    authparams="--rpccookiefile=$RPCCOOKIEFILE"
else
    authparams="--rpcuser=$RPCUSER --rpcpassword=$RPCPASSWD"
fi

echo "Starting Upload Script!"
dotnet run -c Release -- Upload $authparams --conn=$CONN --nowaitonexit --sync &>> /home/dumplings/Logs.txt

echo "Script Ended!"
