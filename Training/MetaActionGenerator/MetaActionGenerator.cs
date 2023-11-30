using MetaActionGenerator.CandidateGenerators;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Models.SAS;
using PDDLSharp.Tools;

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
            metaActions.RemoveAll(x => x.Preconditions.Equals(x.Effects));
            metaActions.RemoveAll(x => x.Effects is IListable list && list.Count() == 0);
            return metaActions;
        }

        public List<ActionDecl> RemoveDuplicateMetaActions(List<ActionDecl> metaActions)
        {
            return metaActions.Distinct().ToList();
        }
    }
}
