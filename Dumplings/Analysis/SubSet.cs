using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumplings.Analysis
{
    public class SubSet
    {
        public IEnumerable<Coin> Inputs { get; }
        public IEnumerable<Coin> Outputs { get; }
        public decimal Precision { get; }

        public SubSet(IEnumerable<decimal> inputs, IEnumerable<decimal> outputs, decimal precision)
            : this(inputs.Select(x => Coin.Random(x)), outputs.Select(x => Coin.Random(x)), precision)
        {
        }

        public SubSet(IEnumerable<Coin> inputs, IEnumerable<Coin> outputs, decimal precision)
        {
            if (!inputs.Sum(x => x.Value).Almost(outputs.Sum(x => x.Value), precision))
            {
                throw new InvalidOperationException("The sum of inputs must be equal to the sum of outputs.");
            }

            Inputs = inputs;
            Outputs = outputs;
            Precision = precision;
        }
    }
}
