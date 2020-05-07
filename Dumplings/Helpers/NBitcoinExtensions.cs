using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dumplings.Helpers
{
    public static class NBitcoinExtensions
    {
        public static async Task<Block> DownloadBlockAsync(this Node node, uint256 hash, CancellationToken cancellationToken)
        {
            if (node.State == NodeState.Connected)
            {
                node.VersionHandshake(cancellationToken);
            }

            using var listener = node.CreateListener();
            var getdata = new GetDataPayload(new InventoryVector(node.AddSupportedOptions(InventoryType.MSG_BLOCK), hash));
            await node.SendMessageAsync(getdata);
            cancellationToken.ThrowIfCancellationRequested();

            // Bitcoin Core processes the messages sequentially and does not send a NOTFOUND message if the remote node is pruned and the data not available.
            // A good way to get any feedback about whether the node knows the block or not is to send a ping request.
            // If block is not known by the remote node, the pong will be sent immediately, else it will be sent after the block download.
            ulong pingNonce = RandomUtils.GetUInt64();
            await node.SendMessageAsync(new PingPayload() { Nonce = pingNonce });
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var message = listener.ReceiveMessage(cancellationToken);
                if (message.Message.Payload is NotFoundPayload ||
                    message.Message.Payload is PongPayload p && p.Nonce == pingNonce)
                {
                    throw new InvalidOperationException($"Disconnected local node, because it does not have the block data.");
                }
                else if (message.Message.Payload is BlockPayload b && b.Object?.GetHash() == hash)
                {
                    return b.Object;
                }
            }
        }

        /// <summary>
        /// A rating of the coinjoin based on the number of equal outputs.
        /// https://github.com/zkSNACKs/WalletWasabi/issues/2940
        /// </summary>
        public static decimal CalculateWasabiCoinJoinQuality(this Transaction me)
        {
            decimal wcq = 0m;
            foreach (var (value, count) in me.GetIndistinguishableOutputs(includeSingle: false))
            {
                // Number of participants (o.count) gained (o.count) anonymity for (o.value.ToDecimal(MoneyUnit.BTC)) bitcoins.
                wcq += count * count * value.ToDecimal(MoneyUnit.BTC);
            }
            return wcq;
        }

        public static IEnumerable<Coin> GetCoins(this TxOutList me, Script script)
        {
            return me.AsCoins().Where(c => c.ScriptPubKey == script);
        }

        /// <summary>
        /// Based on transaction data, it decides if it's possible that native segwit script played a par in this transaction.
        /// </summary>
        public static bool PossiblyP2WPKHInvolved(this Transaction me)
        {
            // We omit Guard, because it's performance critical in Wasabi.
            // We start with the inputs, because, this check is faster.
            // Note: by testing performance the order does not seem to affect the speed of loading the wallet.
            foreach (TxIn input in me.Inputs)
            {
                if (input.ScriptSig is null || input.ScriptSig == Script.Empty)
                {
                    return true;
                }
            }
            foreach (TxOut output in me.Outputs)
            {
                if (output.ScriptPubKey.IsScriptType(ScriptType.P2WPKH))
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<(Money value, int count)> GetIndistinguishableOutputs(this Transaction me, bool includeSingle)
        {
            return me.Outputs.GroupBy(x => x.Value)
                .ToDictionary(x => x.Key, y => y.Count())
                .Select(x => (x.Key, x.Value))
                .Where(x => includeSingle || x.Value > 1);
        }

        public static int GetAnonymitySet(this Transaction me, int outputIndex)
        {
            // 1. Get the output corresponting to the output index.
            var output = me.Outputs[outputIndex];
            // 2. Get the number of equal outputs.
            int equalOutputs = me.GetIndistinguishableOutputs(includeSingle: true).Single(x => x.value == output.Value).count;
            // 3. Anonymity set cannot be larger than the number of inputs.
            var inputCount = me.Inputs.Count;
            var anonSet = Math.Min(equalOutputs, inputCount);
            return anonSet;
        }

        public static int GetAnonymitySet(this Transaction me, uint outputIndex) => me.GetAnonymitySet((int)outputIndex);

        public static Money Percentage(this Money me, decimal perc)
        {
            return Money.Satoshis(me.Satoshi / 100m * perc);
        }

        public static int GetWholeBTC(this Money me)
        {
            return (int)me.ToDecimal(MoneyUnit.BTC);
        }

        public static decimal ToUsd(this Money me, decimal btcExchangeRate)
        {
            return me.ToDecimal(MoneyUnit.BTC) * btcExchangeRate;
        }

        public static bool VerifyMessage(this BitcoinWitPubKeyAddress address, uint256 messageHash, byte[] signature)
        {
            PubKey pubKey = PubKey.RecoverCompact(messageHash, signature);
            return pubKey.WitHash == address.Hash;
        }

        /// <summary>
        /// If scriptpubkey is already present, just add the value.
        /// </summary>
        public static void AddWithOptimize(this TxOutList me, Money money, Script scriptPubKey)
        {
            TxOut found = me.FirstOrDefault(x => x.ScriptPubKey == scriptPubKey);
            if (found != null)
            {
                found.Value += money;
            }
            else
            {
                me.Add(money, scriptPubKey);
            }
        }

        /// <summary>
        /// If scriptpubkey is already present, just add the value.
        /// </summary>
        public static void AddWithOptimize(this TxOutList me, Money money, IDestination destination)
        {
            me.AddWithOptimize(money, destination.ScriptPubKey);
        }

        /// <summary>
        /// If scriptpubkey is already present, just add the value.
        /// </summary>
        public static void AddWithOptimize(this TxOutList me, TxOut txOut)
        {
            me.AddWithOptimize(txOut.Value, txOut.ScriptPubKey);
        }

        /// <summary>
        /// If scriptpubkey is already present, just add the value.
        /// </summary>
        public static void AddRangeWithOptimize(this TxOutList me, IEnumerable<TxOut> collection)
        {
            foreach (var txOut in collection)
            {
                me.AddWithOptimize(txOut);
            }
        }

        public static string ToZpub(this ExtPubKey extPubKey, Network network)
        {
            var data = extPubKey.ToBytes();
            var version = network == Network.Main
                ? new byte[] { 0x04, 0xB2, 0x47, 0x46 }
                : new byte[] { 0x04, 0x5F, 0x1C, 0xF6 };

            return Encoders.Base58Check.EncodeData(version.Concat(data).ToArray());
        }

        public static string ToZPrv(this ExtKey extKey, Network network)
        {
            var data = extKey.ToBytes();
            var version = network == Network.Main
                ? new byte[] { 0x04, 0xB2, 0x43, 0x0C }
                : new byte[] { 0x04, 0x5F, 0x18, 0xBC };

            return Encoders.Base58Check.EncodeData(version.Concat(data).ToArray());
        }

        private static Dictionary<uint256, RawTransactionInfo> RawTransactionInfoCache { get; } = new Dictionary<uint256, RawTransactionInfo>();
        private static object RawTransactionInfoCacheLock { get; } = new object();
        public static async Task<RawTransactionInfo> GetRawTransactionInfoWithCacheAsync(this RPCClient rpc, uint256 txid)
        {
            lock (RawTransactionInfoCacheLock)
            {
                if (RawTransactionInfoCache.TryGetValue(txid, out RawTransactionInfo found)) return found;
            }

            var txi = await rpc.GetRawTransactionInfoAsync(txid);

            lock (RawTransactionInfoCacheLock)
            {
                RawTransactionInfoCache.Add(txi.TransactionId, txi);
            }

            return txi;
        }

        public static async Task StopAsync(this RPCClient rpc)
        {
            await rpc.SendCommandAsync("stop");
        }

        public static void SortByAmount(this TxOutList list)
        {
            list.Sort((x, y) => x.Value.CompareTo(y.Value));
        }

        public static void SortByAmount(this TxInList list, List<Coin> coins)
        {
            var map = new Dictionary<TxIn, Coin>();
            foreach (var coin in coins)
            {
                map.Add(list.Single(x => x.PrevOut == coin.Outpoint), coin);
            }
            list.Sort((x, y) => map[x].Amount.CompareTo(map[y].Amount));
        }

        public static Money GetTotalFee(this FeeRate me, int vsize)
        {
            return Money.Satoshis(Math.Round(me.SatoshiPerByte * vsize));
        }
    }
}
