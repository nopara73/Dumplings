using NBitcoin;
using NBitcoin.RPC;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

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
            var parsed = JsonDocument.Parse(getBlockResponse).RootElement;
            if (!parsed.TryGetProperty("result", out JsonElement blockInfoJson))
            {
                blockInfoJson = parsed;
            }
            var previousBlockHash = blockInfoJson.GetProperty("previousblockhash").GetString();
            var transaction = new List<VerboseTransactionInfo>();

            var blockInfo = new VerboseBlockInfo(
                hash: uint256.Parse(blockInfoJson.GetProperty("hash").GetString()),
                prevBlockHash: previousBlockHash is { } ? uint256.Parse(previousBlockHash) : uint256.Zero,
                confirmations: blockInfoJson.GetProperty("confirmations").GetUInt64(),
                height: blockInfoJson.GetProperty("height").GetUInt64(),
                blockTime: Utils.UnixTimeToDateTime(blockInfoJson.GetProperty("time").GetUInt32()),
                transactions: transaction
            );

            var array = blockInfoJson.GetProperty("tx").EnumerateArray().ToArray();
            for (uint i = 0; i < array.Length; i++)
            {
                var txJson = array[i];
                var inputs = new List<VerboseInputInfo>();
                var outputs = new List<VerboseOutputInfo>();
                var txBlockInfo = new TransactionBlockInfo(blockInfo.Hash, blockInfo.BlockTime, i);
                var tx = new VerboseTransactionInfo(txBlockInfo, uint256.Parse(txJson.GetProperty("txid").GetString()), inputs, outputs);

                foreach (var txinJson in txJson.GetProperty("vin").EnumerateArray())
                {
                    VerboseInputInfo input;
                    if (txinJson.TryGetProperty("coinbase", out JsonElement cb))
                    {
                        input = new VerboseInputInfo(cb.GetString());
                    }
                    else
                    {
                        input = new VerboseInputInfo(
                            outPoint: new OutPoint(uint256.Parse(txinJson.GetProperty("txid").GetString()), txinJson.GetProperty("vout").GetUInt32()),
                            prevOutput: new VerboseOutputInfo(
                                value: Money.Coins(txinJson.GetProperty("prevout").GetProperty("value").GetDecimal()),
                                scriptPubKey: Script.FromHex(txinJson.GetProperty("prevout").GetProperty("scriptPubKey").GetProperty("hex").GetString()),
                                pubkeyType: txinJson.GetProperty("prevout").GetProperty("scriptPubKey").GetProperty("type").GetString())
                        );
                    }

                    inputs.Add(input);
                }

                foreach (var txoutJson in txJson.GetProperty("vout").EnumerateArray())
                {
                    var output = new VerboseOutputInfo(
                        value: Money.Coins(txoutJson.GetProperty("value").GetDecimal()),
                        scriptPubKey: Script.FromHex(txoutJson.GetProperty("scriptPubKey").GetProperty("hex").GetString()),
                        pubkeyType: txoutJson.GetProperty("scriptPubKey").GetProperty("type").GetString()
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

        private const string VerboseTransactionInfoLineSeparator = ":::";
        private const string VerboseInOutInfoInLineSeparator = "}{";

        public static string ToLine(VerboseTransactionInfo vbi)
        {
            var sb = new StringBuilder();

            sb.Append(vbi.Id);
            sb.Append(VerboseTransactionInfoLineSeparator);
            sb.Append(vbi.BlockInfo?.BlockHash);
            sb.Append(VerboseTransactionInfoLineSeparator);
            sb.Append(vbi.BlockInfo?.BlockIndex);
            sb.Append(VerboseTransactionInfoLineSeparator);
            sb.Append(vbi.BlockInfo.BlockTime.HasValue ? vbi.BlockInfo.BlockTime.Value.ToUnixTimeSeconds() : (long?)null);
            sb.Append(VerboseTransactionInfoLineSeparator);
            sb.Append(string.Join(VerboseInOutInfoInLineSeparator, vbi.Inputs));
            sb.Append(VerboseTransactionInfoLineSeparator);
            sb.Append(string.Join(VerboseInOutInfoInLineSeparator, vbi.Outputs));

            return sb.ToString();
        }

        public static VerboseTransactionInfo VerboseTransactionInfoFromLine(string vti)
        {
            var parts = vti.Split(VerboseTransactionInfoLineSeparator, StringSplitOptions.None);

            var id = parts[0] is null ? null : uint256.Parse(parts[0]);
            var blockHash = parts[1] is null ? null : uint256.Parse(parts[1]);
            var blockIndex = parts[2] is null ? (uint?)null : uint.Parse(parts[2]);
            var blockTime = parts[3] is null ? (DateTimeOffset?)null : DateTimeOffset.FromUnixTimeSeconds(long.Parse(parts[3]));
            var inputs = parts[4]?.Split(VerboseInOutInfoInLineSeparator, StringSplitOptions.None).Select(x => VerboseInputInfo.FromString(x));
            var outputs = parts[5]?.Split(VerboseInOutInfoInLineSeparator, StringSplitOptions.None).Select(x => VerboseOutputInfo.FromString(x));

            return new VerboseTransactionInfo(new TransactionBlockInfo(blockHash, blockTime, blockIndex), id, inputs, outputs);
        }
    }
}
