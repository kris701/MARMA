using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActions.Train.VerificationStrategies
{
    public interface IVerificationStrategy
    {
        public int CheckedCounter { get; }
        public List<ValidMetaAction> CurrentlyValidMetaActions { get; }
        public List<ValidMetaAction> VerifyMetaActions(FileInfo domain, List<FileInfo> allMetaActions, List<FileInfo> verificationProblem);
    }
}
