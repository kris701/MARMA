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
    public class RemovePreconditionParameters : BaseMetaGenerator
    {
        public RemovePreconditionParameters(DomainDecl declaration) : base(declaration)
        {
        }

        public override List<ActionDecl> Generate(List<ActionDecl> actions)
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

    }
}
