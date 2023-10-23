using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.Plans;
using PDDLSharp.Toolkit.MacroGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroExtractor
{
    public class ReconstructionPair
    {
        public ActionDecl Macro { get; set; }
        public GroundedAction MetaAction { get; set; }

        public ReconstructionPair(ActionDecl macro, GroundedAction metaAction)
        {
            Macro = macro;
            MetaAction = metaAction;
        }
    }
}
