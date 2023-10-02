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
        private readonly string LeaderPrefix = "fix_";
        private readonly string FollowerPrefix = "attack_";
        private readonly string MetaActionPrefix = "meta_";

        public PDDLDecl AbstractConditionalEffects(DomainDecl domain, ProblemDecl problem)
        {
            var newDomain = domain.Copy();
            newDomain.Actions.Clear();

            foreach (var action in domain.Actions)
            {
                if (action.Name.Contains(FollowerPrefix))
                {
                    var baseAction = GenerateBaseFromConditional(action);
                    newDomain.Actions.Add(baseAction);

                    var whenNodes = action.FindTypes<WhenExp>();
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

                    newAction.Name = $"{newAction.Name}_1";
                    newDomain.Actions.Add(newAction);
                }
                else
                    newDomain.Actions.Add(action);
            }

            return new PDDLDecl(newDomain, problem);
        }

        private ActionDecl GenerateBaseFromConditional(ActionDecl action)
        {
            var newAction = action.Copy();
            if (newAction.Effects is AndExp and) {
                and.Children.RemoveAll(x => x.GetType() == typeof(WhenExp));
            }
            return newAction;
        }
    }
}
