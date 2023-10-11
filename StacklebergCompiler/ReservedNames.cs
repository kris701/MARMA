using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StacklebergCompiler
{
    public static class ReservedNames
    {
        // Prefixes
        public static readonly string LeaderActionPrefix = "fix_";
        public static readonly string FollowerActionPrefix = "attack_";
        public static readonly string MetaActionPrefix = "meta_";
        public static readonly string LeaderStatePrefix = "leader-state-";
        public static readonly string IsGoalPrefix = "is-goal-";

        // Reserved Predicates
        public static readonly string LeaderTurnPredicate = "leader-turn";
    }
}
