using PDDLSharp.Models;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActionGenerator
{
    public class RemoveAdditionalEffects : BaseMetaGenerator
    {
        public RemoveAdditionalEffects(DomainDecl declaration) : base(declaration)
        {
        }

        public override List<ActionDecl> Generate(List<ActionDecl> actions)
        {
            List<ActionDecl> metaActions = new List<ActionDecl>();
            ICandidateGenerator removeEffect = new RemoveEffectParameters(Declaration);
            var removed = removeEffect.Generate(actions);

            foreach(var act in removed)
            {
                var removedPredicates = GetRemovedPredicates(actions.First(x => x.Name == act.Name), act);

                foreach(var pred in removedPredicates)
                {
                    var newMetaAction = act.Copy(null);

                    RemoveMe(newMetaAction.Preconditions, pred.Name);
                    RemoveMe(newMetaAction.Effects, pred.Name);

                    RemoveUnusedParameters(newMetaAction);
                    metaActions.Add(newMetaAction);
                }
            }

            return metaActions;
        }

        private List<PredicateExp> GetRemovedPredicates(ActionDecl original, ActionDecl from)
        {
            List<PredicateExp> returnList = new List<PredicateExp>();

            var originalPreds = original.FindTypes<PredicateExp>();
            var fromPreds = from.FindTypes<PredicateExp>();
            foreach (var item in originalPreds.DistinctBy(x => x.Name))
            {
                if (originalPreds.Count(x => x.Name == item.Name) != fromPreds.Count(x => x.Name == item.Name))
                    returnList.Add(item);
            }

            return returnList;
        }
    }
}
