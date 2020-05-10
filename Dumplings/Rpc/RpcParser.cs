using NBitcoin;
using NBitcoin.RPC;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumplings.Rpc
{
    public static class RpcParser
    {
        public static RpcPubkeyType ConvertPubkeyType(string pubKeyType)
        {
            return pubKeyType switch
            {
                "nonstandard" => RpcPubkeyType.TxNonstandard,
                "pubkey" => RpcPubkeyType.TxPubkey,
                "pubkeyhash" => RpcPubkeyType.TxPubkeyhash,
                "scripthash" => RpcPubkeyType.TxScripthash,
                "multisig" => RpcPubkeyType.TxMultisig,
                "nulldata" => RpcPubkeyType.TxNullData,
                "witness_v0_keyhash" => RpcPubkeyType.TxWitnessV0Keyhash,
                "witness_v0_scripthash" => RpcPubkeyType.TxWitnessV0Scripthash,
                "witness_unknown" => RpcPubkeyType.TxWitnessUnknown,
                _ => RpcPubkeyType.Unknown
            };
        }

        public static RpcPubkeyType GetPubkeyType(Script scriptPubKey)
        {
            if (scriptPubKey.IsScriptType(ScriptType.MultiSig))
            {
                return RpcPubkeyType.TxMultisig;
            }
            if (scriptPubKey.IsScriptType(ScriptType.P2PK))
            {
                return RpcPubkeyType.TxPubkey;
            }
            if (scriptPubKey.IsScriptType(ScriptType.P2PKH))
            {
                return RpcPubkeyType.TxPubkeyhash;
            }
            if (scriptPubKey.IsScriptType(ScriptType.P2SH))
            {
                return RpcPubkeyType.TxScripthash;
            }
            if (scriptPubKey.IsScriptType(ScriptType.P2WPKH))
            {
                return RpcPubkeyType.TxWitnessV0Keyhash;
            }
            if (scriptPubKey.IsScriptType(ScriptType.P2WSH))
            {
                return RpcPubkeyType.TxWitnessV0Scripthash;
            }
            if (scriptPubKey.IsScriptType(ScriptType.Witness))
            {
                return RpcPubkeyType.TxWitnessUnknown;
            }
            if (TxNullDataTemplate.Instance.CheckScriptPubKey(scriptPubKey))
            {
                return RpcPubkeyType.TxNullData;
            }

            return RpcPubkeyType.TxNonstandard;
        }

        public static VerboseBlockInfo ParseVerboseBlockResponse(string getBlockResponse)
        {
            var blockInfoJson = JObject.Parse(getBlockResponse);
            var previousBlockHash = blockInfoJson.Value<string>("previousblockhash");
            var transaction = new List<VerboseTransactionInfo>();

            var blockInfo = new VerboseBlockInfo(
                hash: uint256.Parse(blockInfoJson.Value<string>("hash")),
                prevBlockHash: previousBlockHash is { } ? uint256.Parse(previousBlockHash) : uint256.Zero,
                confirmations: blockInfoJson.Value<ulong>("confirmations"),
                height: blockInfoJson.Value<ulong>("height"),
                blockTime: Utils.UnixTimeToDateTime(blockInfoJson.Value<uint>("time")),
                transactions: transaction
            );

            JToken[] array = blockInfoJson["tx"].ToArray();
            for (uint i = 0; i < array.Length; i++)
            {
                JToken txJson = (JToken)array[i];
                var inputs = new List<VerboseInputInfo>();
                var outputs = new List<VerboseOutputInfo>();
                var txBlockInfo = new TransactionBlockInfo(blockInfo.Hash, blockInfo.BlockTime, i);
                var tx = new VerboseTransactionInfo(txBlockInfo, Transaction.Parse(txJson.Value<string>("hex"), Network.Main), inputs, outputs);

                foreach (var txinJson in txJson["vin"])
                {
                    VerboseInputInfo input;
                    if (txinJson["coinbase"] is { })
                    {
                        input = new VerboseInputInfo(txinJson["coinbase"].Value<string>());
                    }
                    else
                    {
                        input = new VerboseInputInfo(
                            outPoint: new OutPoint(uint256.Parse(txinJson.Value<string>("txid")), txinJson.Value<uint>("vout")),
                            prevOutput: new VerboseOutputInfo(
                                value: Money.Coins(txinJson["prevout"].Value<decimal>("value")),
                                scriptPubKey: Script.FromHex(txinJson["prevout"]["scriptPubKey"].Value<string>("hex")),
                                pubkeyType: txinJson["prevout"]["scriptPubKey"].Value<string>("type"))
                        );
                    }

                    inputs.Add(input);
                }

                foreach (var txoutJson in txJson["vout"])
                {
                    var output = new VerboseOutputInfo(
                        value: Money.Coins(txoutJson.Value<decimal>("value")),
                        scriptPubKey: Script.FromHex(txoutJson["scriptPubKey"].Value<string>("hex")),
                        pubkeyType: txoutJson["scriptPubKey"].Value<string>("type")
                    );

                    outputs.Add(output);
                }

                transaction.Add(tx);
            }

            return blockInfo;
        }

        public static SmartRawTransactionInfo ParseSmartRawTransactionInfoResponse(JToken json)
        {
            var tbi = new TransactionBlockInfo(
                blockHash: json["blockhash"] is { } ? uint256.Parse(json.Value<string>("blockhash")) : null,
                blockTime: json["blocktime"] is { } ? Utils.UnixTimeToDateTime(json.Value<long>("blocktime")) : (DateTimeOffset?)null,
                blockIndex: json.Value<uint>("nTx"));
            return new SmartRawTransactionInfo
            {
                Transaction = Transaction.Parse(json.Value<string>("hex"), Network.Main),
                TransactionBlockInfo = tbi
            };
        }
    }
}
