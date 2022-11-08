#!/bin/bash

# exit when any command fails
set -e

FILE1='LastProcessedBlockHeight.txt'
FILE2='OtherCoinJoins.txt'
FILE3='SamouraiPostMixTxs.txt'
FILE4='OtherCoinJoinPostMixTxs.txt'
FILE5='SamouraiCoinJoins.txt'
FILE6='SamouraiTx0s.txt'
FILE7='WabiSabiCoinJoins.txt'
FILE8='WasabiPostMixTxs.txt'
FILE9='WasabiCoinJoins.txt'

# Make sure to have 'Config.txt' in the same file as this script.
# The following variables need to be imported from the config file:

# HOST = FTP server address,
# FTPUSER & FTPPASSWD = FTP login credentials,
# RPCUSER & RPCPASSWD = RPC credentials for Bitcoin Kntos,
# SCANNERFOLDER = Where to look for the coinjoin stat files to send

source Config.txt

echo "Syncronizing blockchain"
dotnet run -c Release -- sync --rpcuser=$RPCUSER --rpcpassword=$RPCPASSWD --host=$RPCHOST --nowaitonexit

cd $SCANNERFOLDER

cat > UPDATEREADY.txt << EOF
true
EOF

echo "Starting FTP upload"

ftp -n $HOST <<END_SCRIPT
quote USER $FTPUSER
quote PASS $FTPPASSWD
put $FILE1
put $FILE2
put $FILE3
put $FILE4
put $FILE5
put $FILE6
put $FILE7
put $FILE8
put $FILE9
put UPDATEREADY.txt
quit
END_SCRIPT

echo "Finished FTP upload!"
echo "Removing files"
rm $FILE2
rm $FILE3
rm $FILE4
rm $FILE5
rm $FILE6
rm $FILE7
rm $FILE8
rm $FILE9
rm UPDATEREADY.txt

echo "Files removed!"
