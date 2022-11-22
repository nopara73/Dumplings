using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dumplings.Analysis
{
    public class Mapping
    {
        public IEnumerable<SubSet> SubSets { get; }
        public decimal Precision { get; }

        public Analysis Analysis { get; private set; }

        /// <summary>
        /// Coinjoins the submapping into one single transaction.
        /// </summary>
        public Mapping Join()
        {
            var subSetInputs = SubSets.SelectMany(x => x.Inputs);
            var suSetOutputs = SubSets.SelectMany(x => x.Outputs);

            return new Mapping(new SubSet(subSetInputs, suSetOutputs, Precision));
        }

        /// <summary>
        /// Create a non-derived mapping.
        /// </summary>
        public Mapping(SubSet subSet)
          : this(new List<SubSet> { subSet })
        {
        }

        public Mapping(IEnumerable<SubSet> subSets)
        {
            if (subSets.Select(x => x.Precision).Distinct().Count() != 1)
            {
                throw new InvalidOperationException("All subsets must have the same precision.");
            }

            SubSets = subSets;
            Precision = subSets.First().Precision;
        }

        public override string ToString()
        {
            return string.Join(" | ", SubSets.Select(x => $"{string.Join(',', x.Inputs)} -> {string.Join(',', x.Outputs)}"));
        }

        /// <summary>
        /// Loosly optimized. Has no recursion.
        /// </summary>
        public IEnumerable<Mapping> AnalyzeWithNopara73Algorithm()
        {
            var mappings = new List<Mapping>();

            foreach (var subSet in SubSets)
            {
                var outputPartitions = Partitioning.GetAllPartitions(subSet.Outputs.ToArray());
                var inputPartitions = Partitioning.GetAllPartitions(subSet.Inputs.ToArray());

                foreach (var inputPartition in inputPartitions)
                {
                    foreach (var outputPartition in outputPartitions.Where(x => x.Length == inputPartition.Length))
                    {
                        var remainingOutputPartition = outputPartition;
                        var validPartition = true;
                        var subSetsBuilder = new List<SubSet>();
                        foreach (var inputPartitionPart in inputPartition)
                        {
                            var foundValidOutputPartitionPart = remainingOutputPartition.FirstOrDefault(x => x.Sum(x => x.Value).Almost(inputPartitionPart.Sum(x => x.Value), Precision));
                            // https://www.comsys.rwth-aachen.de/fileadmin/papers/2017/2017-maurer-trustcom-coinjoin.pdf
                            // input partitions that include a set
                            // with a sum that is not a sub sum of the outputs cannot
                            // be part of a mapping
                            if (foundValidOutputPartitionPart is null)
                            {
                                validPartition = false;
                                break;
                            }
                            else
                            {
                                subSetsBuilder.Add(new SubSet(inputPartitionPart, foundValidOutputPartitionPart, Precision));
                            }
                        }

                        if (validPartition)
                        {
                            var mapping = new Mapping(subSetsBuilder);
                            mappings.Add(mapping);
                            yield return mapping;
                        }
                    }
                }
            }

            Analysis = new Analysis(mappings);
        }
    }
}
