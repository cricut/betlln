using System;
using System.Collections.Generic;
using System.Linq;

namespace Betlln.IO
{
    public class MultiFileDemand
    {
        private readonly List<List<FileDemand>> _demandGroups;

        private MultiFileDemand()
        {
            _demandGroups = new List<List<FileDemand>>();
        }

        public static MultiFileDemand This(FileDemand demand)
        {
            MultiFileDemand multiFileDemand = new MultiFileDemand();
            multiFileDemand._demandGroups.Add(new List<FileDemand> {demand});
            return multiFileDemand;
        }

        public MultiFileDemand AndThis(FileDemand demand)
        {
            _demandGroups.First().Add(demand);
            return this;
        }

        public static MultiFileDemand These(IEnumerable<FileDemand> demands)
        {
            MultiFileDemand multiFileDemand = new MultiFileDemand();
            multiFileDemand._demandGroups.Add(new List<FileDemand>());
            multiFileDemand._demandGroups.First().AddRange(demands);
            return multiFileDemand;
        }

        public MultiFileDemand OrJust(FileDemand demand)
        {
            AddAlternative(demand);
            return this;
        }

        public MultiFileDemand OrThese(IEnumerable<FileDemand> demands)
        {
            _demandGroups.Add(new List<FileDemand>());
            _demandGroups.Last().AddRange(demands);
            return this;
        }

        public void AddAlternative(FileDemand demand)
        {
            _demandGroups.Add(new List<FileDemand>());
            _demandGroups.Last().Add(demand);
        }

        public IEnumerable<FileDemand> RequiredItems =>
            _demandGroups.Count == 1
                ? _demandGroups.First()
                : new List<FileDemand>();

        public IEnumerable<IReadOnlyList<FileDemand>> OptionGroups => _demandGroups;

        public bool IsSatisfiedBy(string fileName)
        {
            foreach (List<FileDemand> demandGroup in _demandGroups)
            {
                if (demandGroup.Count > 1)
                {
                    throw new InvalidOperationException();
                }

                if (demandGroup.First().IsSatisfiedBy(fileName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}