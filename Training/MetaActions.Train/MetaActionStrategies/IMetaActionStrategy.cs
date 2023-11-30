using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActions.Train.MetaActionStrategies
{
    public interface IMetaActionStrategy
    {
        public int MacroCount { get; }
        public List<FileInfo> GetMetaActions(FileInfo domain, List<FileInfo> trainingProblems);
    }
}
