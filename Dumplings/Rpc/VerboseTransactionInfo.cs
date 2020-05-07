using System.Collections.Generic;
using System.Linq;
using NBitcoin;

namespace Dumplings.Rpc
{
	public class VerboseTransactionInfo
	{
		public VerboseTransactionInfo(uint256 id, IEnumerable<VerboseInputInfo> inputs, IEnumerable<VerboseOutputInfo> outputs)
		{
			Id = id;
			Inputs = inputs;
			Outputs = outputs;
		}

		public uint256 Id { get; }

		public IEnumerable<VerboseInputInfo> Inputs { get; }

		public IEnumerable<VerboseOutputInfo> Outputs { get; }

		public IEnumerable<(Money value, int count)> GetIndistinguishableOutputs(bool includeSingle)
		{
			return Outputs.GroupBy(x => x.Value)
				.ToDictionary(x => x.Key, y => y.Count())
				.Select(x => (x.Key, x.Value))
				.Where(x => includeSingle || x.Value > 1);
		}
	}
}
