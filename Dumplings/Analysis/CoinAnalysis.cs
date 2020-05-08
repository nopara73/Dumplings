using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumplings.Analysis
{
    public class CoinAnalysis
    {
        public Coin Coin { get; }
        public IEnumerable<(Coin coin, decimal distance)> Inputs { get; }
        public IEnumerable<(Coin coin, decimal distance)> Outputs { get; }

        public CoinAnalysis(Coin coin, IEnumerable<(Coin coin, decimal distance)> inputs, IEnumerable<(Coin coin, decimal distance)> outputs)
        {
            Coin = coin;
            Inputs = inputs;
            Outputs = outputs;
        }

        public override string ToString()
        {
            return $"{Coin.Value} - inputs: {string.Join(' ', Inputs.Select(x => x.coin.Value + "(" + decimal.Round(x.distance, 2, MidpointRounding.AwayFromZero) + ")"))} | outputs: {string.Join(' ', Outputs.Select(x => x.coin.Value + "(" + decimal.Round(x.distance, 2, MidpointRounding.AwayFromZero) + ")"))}";
        }
    }
}
