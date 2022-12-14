#!/bin/bash

# exit when any command fails
set -e

# Make sure to have 'Config.txt' in the same file as this script.
# The following variables need to be imported from the config file:

# RPCUSER & RPCPASSWD = RPC credentials for Bitcoin Kntos,
# RPCHOST = Node/endpoint to get blockhain info from.

source Config.txt

echo "Syncronizing blockchain"
dotnet run -c Release -- Upload --rpcuser=$RPCUSER --rpcpassword=$RPCPASSWD --host=$RPCHOST --nowaitonexit

echo "Script Ended!"
