using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dumplings.Rpc
{
    public class SmartRawTransactionInfo
    {
        public Transaction Transaction { get; internal set; }
        public TransactionBlockInfo TransactionBlockInfo { get; internal set; }
    }
}
