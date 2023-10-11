using PDDLSharp.Models.PDDL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActionGenerator
{
    public interface ICandidateGenerator
    {
        public List<ActionDecl> Generate(List<ActionDecl> actions);
    }
}
