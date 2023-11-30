using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActions.Train.VerificationStrategies
{
    public class ValidMetaAction
    {
        public FileInfo MetaAction { get; }
        public List<FileInfo> Replacements { get; }

        public ValidMetaAction(FileInfo metaAction, List<FileInfo> replacements)
        {
            MetaAction = metaAction;
            Replacements = replacements;
        }

        public ValidMetaAction(FileInfo metaAction)
        {
            MetaAction = metaAction;
            Replacements = new List<FileInfo>();
        }
    }
}
