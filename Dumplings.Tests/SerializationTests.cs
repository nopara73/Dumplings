using Dumplings.Rpc;
using Dumplings.Stats;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dumplings.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void CanDeserializeVerboseTransaction()
        {
            var serialized = "7732a03c8cf133e0475ae37e4f2f49ba77beb631378216889e33e9847aa0049b:::0000000000000000002dddc28874e5a9b2b8c285a09af36d094d91045bb054ae:::1232:::1530763257:::52d47c5f7f1f10346a299a915df44b644386f46eb8df809cbe5ce3ab95ad0618-6-100000000+76a91419e63e8d7e9657b91f3db142a111966239721e6388ac+TxPubkeyhash}{f2cad4dc598347a47325ccacd26e3d75dbc41d17e8119a1e1b5d14b824cf1803-1-63640544+76a914dfa7bf346abd72d5bc2f18a4a66ce04afa92b27388ac+TxPubkeyhash}{e3626340995eb0aae5b27ccd7b9c8223bb2516b99700fb2149f255bacb6805b7-7-41979708+76a914a6edd64f58571b70b51c8a0554483e4a6d3c124188ac+TxPubkeyhash}{78a0a8a59abcf598546903d7bc465859417a5a1df4584d6b80c75463be01360c-1-59754628+76a914e216b950e754b850f96b0d003367c40a48a35c5788ac+TxPubkeyhash}{57a1c4454434f2588979fd7774150e3353a17ae4f93721e60c74d2f40190410b-0-100000800+76a9143844f1b5cb2cd77c0804865c3b0795963427264a88ac+TxPubkeyhash}{640cdeb1a0d3e1dabbcb73c042f52e306f81aa50710b8385f662570a802ba75d-6-83833827+76a91448d2f9a54285c0a82333324c1d967125805ebfe188ac+TxPubkeyhash}{78a0a8a59abcf598546903d7bc465859417a5a1df4584d6b80c75463be01360c-5-59754628+76a9144d85041cd632dc8545a8d879c42b995f30e4ec9788ac+TxPubkeyhash}{704ae5cdf68e0dbfa66a5b74530255c8cee93614a802464fc0936f0a1ed66b7e-5-15370723+76a9142a76248704923d50bbae9060c3d2037de733446e88ac+TxPubkeyhash:::83240446+76a91488c947a8c4f2e935c13ee8b4652f41e37e83dc2a88ac+TxPubkeyhash}{83240446+76a9146c7d459a8b835682a026ce048b9c2ec881a12a0188ac+TxPubkeyhash}{83240446+76a914f24657d6e79048d1dc7959dc272bfb31570b6f3088ac+TxPubkeyhash}{83240446+76a914300b88d512738ad92f11743b6cdd661db9bf74c988ac+TxPubkeyhash}{55492608+76a914a007d99090ec01e43a13706953a69efe0d90b0e688ac+TxPubkeyhash}{83240446+76a914964ffadf3e33730c74484014d06f864b0a68647988ac+TxPubkeyhash}{595381+76a9148c5ca0add7ee7486d301cbe1783a09e6a13d9df288ac+TxPubkeyhash}{18498052+76a9143366e2bbe20d11b7bc2631bf045ad90f688cf2c388ac+TxPubkeyhash}{16760754+76a9147038bcdbfd774755e9e713b5546bd51d551e8aa988ac+TxPubkeyhash}{16775202+76a914eaf8c16b3976d11dc7c1417be3b4027fb4569ce788ac+TxPubkeyhash";
            var deserialized = RpcParser.VerboseTransactionInfoFromLine(serialized);
            Assert.NotNull(deserialized.BlockInfo);
            Assert.NotNull(deserialized.BlockInfo.BlockHash);
            Assert.NotNull(deserialized.BlockInfo.BlockIndex);
            Assert.NotNull(deserialized.BlockInfo.BlockTime);
            Assert.NotNull(deserialized.Id);
            Assert.NotEmpty(deserialized.Inputs);
            Assert.NotEmpty(deserialized.Outputs);
        }

        [Fact]
        public void CanDeserializeCoin()
        {
            var serialized = "636663670340000000::967a9f5f199bc4bfe760be6dbdc4a36f066ede3d01d5ce1dc1955471e19e18fd::1::OP_HASH160 5dbb460b40f627d6afabe63a07df91f384352690 OP_EQUAL::0.99997655";
            var deserialized = Coin.FromString(serialized);
            Assert.Equal(636663670340000000, deserialized.BlockTime.UtcTicks);
            Assert.NotNull(deserialized.Txid);
            Assert.Equal(1u, deserialized.Index);
            Assert.NotNull(deserialized.Script);
            Assert.NotNull(deserialized.Amount);
        }
    }
}
