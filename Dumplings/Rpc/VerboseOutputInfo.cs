using NBitcoin;
using System;

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

        private const string Separator = "+";

        public override string ToString()
        {
            return $"{Value.Satoshi}{Separator}{ScriptPubKey.ToHex()}{Separator}{PubkeyType}";
        }

        internal static VerboseOutputInfo FromString(string x)
        {
            var parts = x.Split(Separator, StringSplitOptions.None);

            var val = parts[0] is null ? null : Money.Satoshis(long.Parse(parts[0]));
            var script = parts[1] is null ? null : Script.FromHex(parts[1]);
            var t = parts[2] is null ? RpcPubkeyType.Unknown : Enum.Parse<RpcPubkeyType>(parts[2]);

            return new VerboseOutputInfo(val, script, t);
        }
    }
}
