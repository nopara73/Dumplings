using NBitcoin;

namespace Dumplings.Rpc
{
    public class VerboseOutputInfo
    {
        public VerboseOutputInfo(Money value, Script scriptPubKey, string pubkeyType)
            : this(value, scriptPubKey, RpcParser.ConvertPubkeyType(pubkeyType))
        {
        }

        public VerboseOutputInfo(Money value, Script scriptPubKey)
            : this(value, scriptPubKey, RpcParser.GetPubkeyType(scriptPubKey))
        {
        }

        public VerboseOutputInfo(Money value, Script scriptPubKey, RpcPubkeyType pubkeyType)
        {
            Value = value;
            ScriptPubKey = scriptPubKey;
            PubkeyType = pubkeyType;
        }

        public Money Value { get; }

        public Script ScriptPubKey { get; }

        public RpcPubkeyType PubkeyType { get; }
    }
}
