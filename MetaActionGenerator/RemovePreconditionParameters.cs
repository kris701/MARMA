using PDDLSharp.Models;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActionGenerator
{
    public class RemovePreconditionParameters : ICandidateGenerator
    {
        public DomainDecl Declaration { get; }

        public RemovePreconditionParameters(DomainDecl declaration)
        {
            Declaration = declaration;
        }

        public List<ActionDecl> Generate(List<ActionDecl> actions)
        {
            List<ActionDecl> metaActions = new List<ActionDecl>();

            foreach(var act in actions)
            {
                List<NameExp> removeable = new List<NameExp>();
                foreach(var arg in act.Parameters.Values)
                {
                    if (act.Effects.FindNames(arg.Name).Count == 0)
                        removeable.Add(arg);
                }

                foreach(var remove in removeable)
                {
                    var newMetaAction = act.Copy(null);

                    newMetaAction.Parameters.Values.RemoveAll(x => x.Name == remove.Name);
                    RemoveMe(newMetaAction.Preconditions, remove.Name);

                    metaActions.Add(newMetaAction);
                }
            }

            return metaActions;
        }

        private bool RemoveMe(INode node, string name)
        {
            if (node is PredicateExp pred)
            {
                if (pred.FindNames(name).Count > 0)
                    return true;
            }
            else if (node is AndExp and)
            {
                List<IExp> newChildren = new List<IExp>();
                foreach (var child in and.Children)
                    if (!RemoveMe(child, name))
                        newChildren.Add(child);
                and.Children = newChildren;
            }
            return false;
        }
    }
}
