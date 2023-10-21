using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;

namespace MetaActionGenerator
{
    public class RemovePreconditionParameters : BaseMetaGenerator
    {
        /// <summary>
        /// "C_{pre} removes parameters that are absent in the effect of the macro"
        /// </summary>
        /// <param name="actions"></param>
        /// <returns></returns>
        public override List<ActionDecl> Generate(List<ActionDecl> actions)
        {
            List<ActionDecl> metaActions = new List<ActionDecl>();

            foreach (var act in actions)
            {
                List<NameExp> removeable = new List<NameExp>();
                foreach (var arg in act.Parameters.Values)
                    if (act.Effects.FindNames(arg.Name).Count == 0)
                        removeable.Add(arg);

                foreach (var remove in removeable)
                {
                    var newMetaAction = act.Copy();

                    newMetaAction.Parameters.Values.RemoveAll(x => x.Name == remove.Name);
                    RemoveName(newMetaAction.Preconditions, remove.Name);

                    metaActions.Add(newMetaAction);
                }
            }

            return metaActions;
        }

    }
}
