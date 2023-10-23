using PDDLSharp.Models.PDDL.Domain;
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
        public ActionSequence RepairSequence { get; set; }

        public ReconstructionPair(ActionDecl macro, ActionSequence repairSequence)
        {
            Macro = macro;
            RepairSequence = repairSequence;
        }
    }
}
