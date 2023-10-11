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
    public abstract class BaseMetaGenerator : ICandidateGenerator
    {
        public DomainDecl Declaration { get; }

        protected BaseMetaGenerator(DomainDecl declaration)
        {
            Declaration = declaration;
        }

        public abstract List<ActionDecl> Generate(List<ActionDecl> actions);

        internal bool RemoveMe(INode node, string name)
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
