using PDDLSharp.Models.FastDownward.Plans;
using PDDLSharp.Models.PDDL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroExtractor
{
    public class RepairSequence
    {
        public ActionDecl Macro { get; set; }
        public ActionPlan Replacement { get; set; }

        public RepairSequence(ActionDecl macro, ActionPlan replacement)
        {
            Macro = macro;
            Replacement = replacement;
        }

        public override int GetHashCode()
        {
            return Macro.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is RepairSequence seq)
                return seq.GetHashCode() == Macro.GetHashCode();
            return false;
        }
    }
}
