using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;

namespace MetaActionGenerator.CandidateGenerators
{
    public class RemoveAdditionalEffects : BaseMetaGenerator
    {
        /// <summary>
        /// "C_{inv} takes the C_{eff} candidates, and remove other preconditions and effects with the same predicate as the ones removed by the C_{eff} method."
        /// </summary>
        /// <param name="actions"></param>
        /// <returns></returns>
        public override List<ActionDecl> Generate(List<ActionDecl> actions)
        {
            List<ActionDecl> metaActions = new List<ActionDecl>();
            ICandidateGenerator removeEffect = new RemoveEffectParameters();
            var removed = removeEffect.Generate(actions);

            foreach (var act in removed)
            {
                var removedPredicates = GetRemovedPredicates(actions.First(x => x.Name == act.Name), act);

                var newMetaAction = act.Copy();
                foreach (var pred in removedPredicates)
                {
                    RemoveName(newMetaAction.Preconditions, pred.Name);
                    RemoveName(newMetaAction.Effects, pred.Name);
                }
                RemoveUnusedParameters(newMetaAction);
                metaActions.Add(newMetaAction);
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
