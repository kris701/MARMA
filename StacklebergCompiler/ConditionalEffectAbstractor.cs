using PDDLSharp.Models.Domain;
using PDDLSharp.Models.Problem;
using PDDLSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using PDDLSharp.Models.Expressions;

namespace StacklebergCompiler
{
    public class ConditionalEffectAbstractor
    {
        public PDDLDecl AbstractConditionalEffects(DomainDecl domain, ProblemDecl problem)
        {
            var newDomain = domain.Copy();

            newDomain.Actions = GenerateAbstractedActions(newDomain.Actions);

            return new PDDLDecl(newDomain, problem);
        }

        private List<ActionDecl> GenerateAbstractedActions(List<ActionDecl> actions)
        {
            List<ActionDecl> newActions = new List<ActionDecl>();
            foreach (var action in actions)
            {
                if (action.Name.Contains(ReservedNames.FollowerActionPrefix))
                {
                    var baseAction = GenerateBaseFromConditional(action);
                    var whenNodes = action.FindTypes<WhenExp>();
                    newActions.Add(baseAction);
                    if (action.Name != ReservedNames.FollowerReachGoalAction)
                        newActions.Add(GenerateGoalVariantFromConditional(baseAction, whenNodes));
                }
                else
                    newActions.Add(action);
            }
            return newActions;
        }

        private ActionDecl GenerateBaseFromConditional(ActionDecl action)
        {
            var newAction = action.Copy();
            if (newAction.Effects is AndExp and) {
                and.Children.RemoveAll(x => x.GetType() == typeof(WhenExp));
            }
            return newAction;
        }

        private ActionDecl GenerateGoalVariantFromConditional(ActionDecl baseAction, List<WhenExp> whenNodes)
        {
            var newAction = baseAction.Copy();
            if (newAction.Preconditions is AndExp preconditions &&
                newAction.Effects is AndExp effects)
            {
                foreach (var whenNode in whenNodes)
                {
                    preconditions.Children.Add(whenNode.Condition);
                    effects.Children.Add(whenNode.Effect);
                }
            }

            newAction.Name = $"{newAction.Name}{ReservedNames.GoalActionSufix}";

            return newAction;
        }
    }
}
