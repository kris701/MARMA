using PDDLSharp.Models;
using PDDLSharp.Models.PDDL.Domain;
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

            return metaActions;
        }
    }
}
