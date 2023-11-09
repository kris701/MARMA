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
        public GroundedAction MetaAction { get; set; }
        public ActionDecl Macro { get; set; }
        public ActionPlan Replacement { get; set; }

        public RepairSequence(GroundedAction metaAction, ActionDecl macro, ActionPlan replacement)
        {
            MetaAction = metaAction;
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
            {
                if (Macro.Name != seq.Macro.Name) return false;
                if (Macro.Parameters.Values.Count != seq.Macro.Parameters.Values.Count) return false;
                for(int i = 0; i < Macro.Parameters.Values.Count; i++)
                    if (Macro.Parameters.Values[i].Name != seq.Macro.Parameters.Values[i].Name)
                        return false;
                return true;
            }
            return false;
        }
    }
}
