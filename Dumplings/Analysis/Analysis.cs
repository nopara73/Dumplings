using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumplings.Analysis
{
    public class Analysis
    {
        public IEnumerable<Mapping> Mappings { get; }
        public Mapping NonDerivedMapping { get; }
        public IEnumerable<CoinAnalysis> InputAnalyses { get; }
        public IEnumerable<CoinAnalysis> OutputAnalyses { get; }

        public decimal CalculateCoinJoinAmbiguity()
        {
            var cja = 0m;
            var analyzed = new HashSet<CoinPair>();
            foreach (var anal in InputAnalyses)
            {
                foreach (var coin in anal.Inputs.Where(x => !analyzed.Contains(new CoinPair(anal.Coin, x.coin))))
                {
                    cja += (anal.Coin.Value + coin.coin.Value) / coin.distance;
                    analyzed.Add(new CoinPair(anal.Coin, coin.coin));
                }

                // No duplication here, so no need the contains.
                foreach (var coin in anal.Outputs)
                {
                    cja += (anal.Coin.Value + coin.coin.Value) / coin.distance;
                    analyzed.Add(new CoinPair(anal.Coin, coin.coin));
                }
            }

            analyzed.Clear();
            foreach (var anal in OutputAnalyses)
            {
                foreach (var coin in anal.Outputs.Where(x => !analyzed.Contains(new CoinPair(anal.Coin, x.coin))))
                {
                    cja += (anal.Coin.Value + coin.coin.Value) / coin.distance;
                    analyzed.Add(new CoinPair(anal.Coin, coin.coin));
                }

                // Input-output paris were already analyzed when we went through the inputs, so we don't need to do it again.
            }

            return cja;
        }

        public Analysis(IEnumerable<Mapping> mappings)
        {
            Mappings = mappings;
            NonDerivedMapping = mappings.Single(x => x.SubSets.Count() == 1);

            var mappingCount = mappings.Count();
            var inputAnalyses = new List<CoinAnalysis>();
            foreach (var input in NonDerivedMapping.SubSets.Single().Inputs)
            {
                var inputDistances = new List<(Coin coin, decimal distance)>();
                foreach (var input2 in NonDerivedMapping.SubSets.Single().Inputs.Except(new[] { input }))
                {
                    var commonMappingCount = Mappings.Count(x => x.SubSets.Any(y => y.Inputs.Contains(input) && y.Inputs.Contains(input2)));
                    inputDistances.Add((input2, (decimal)commonMappingCount / mappingCount));
                }

                var outputDistances = new List<(Coin coin, decimal distance)>();
                foreach (var output in NonDerivedMapping.SubSets.Single().Outputs)
                {
                    var commonMappingCount = Mappings.Count(x => x.SubSets.Any(y => y.Inputs.Contains(input) && y.Outputs.Contains(output)));
                    outputDistances.Add((output, (decimal)commonMappingCount / mappingCount));
                }

                inputAnalyses.Add(new CoinAnalysis(input, inputDistances, outputDistances));
            }
            InputAnalyses = inputAnalyses;

            var outputAnalyses = new List<CoinAnalysis>();
            foreach (var output in NonDerivedMapping.SubSets.Single().Outputs)
            {
                var outputDistances = new List<(Coin coin, decimal distance)>();
                foreach (var output2 in NonDerivedMapping.SubSets.Single().Outputs.Except(new[] { output }))
                {
                    var commonMappingCount = Mappings.Count(x => x.SubSets.Any(y => y.Outputs.Contains(output) && y.Outputs.Contains(output2)));
                    outputDistances.Add((output2, (decimal)commonMappingCount / mappingCount));
                }

                var inputDistances = new List<(Coin coin, decimal distance)>();
                foreach (var input in NonDerivedMapping.SubSets.Single().Inputs)
                {
                    var commonMappingCount = Mappings.Count(x => x.SubSets.Any(y => y.Outputs.Contains(output) && y.Inputs.Contains(input)));
                    inputDistances.Add((input, (decimal)commonMappingCount / mappingCount));
                }

                outputAnalyses.Add(new CoinAnalysis(output, outputDistances, inputDistances));
            }
            OutputAnalyses = outputAnalyses;
        }
    }
}
