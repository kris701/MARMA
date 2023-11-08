using MetaActionGenerator.CandidateGenerators;
using PDDLSharp.Models;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Toolkit.MutexDetector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActionGenerator
{
    public class MetaActionGenerator
    {
        public List<ActionDecl> GenerateMetaActions(List<ActionDecl> from)
        {
            var metaActions = new List<ActionDecl>();

            metaActions.AddRange(new RemovePreconditionParameters().Generate(from));
            metaActions.AddRange(new RemoveEffectParameters().Generate(from));
            metaActions.AddRange(new RemoveAdditionalEffects().Generate(from));

            metaActions = SanetizeMetaActions(metaActions);
            metaActions = RemoveDuplicateMetaActions(metaActions);

            return metaActions;
        }

        public List<ActionDecl> SanetizeMetaActions(List<ActionDecl> metaActions)
        {
            metaActions.RemoveAll(x => (x.Effects is IWalkable effWalk && effWalk.Count() == 0));
            metaActions.RemoveAll(x => (x.Effects is IListable effList && effList.Count() == 0));
            metaActions.RemoveAll(x => (x.Effects.Equals(x.Preconditions)));
            return metaActions;
        }

        public List<ActionDecl> RemoveDuplicateMetaActions(List<ActionDecl> metaActions)
        {
            return metaActions.DistinctBy(x => 
                x.Parameters.GetHashCode() ^ 
                x.Preconditions.GetHashCode() ^ 
                x.Effects.GetHashCode()).ToList();
        }
    }
}
