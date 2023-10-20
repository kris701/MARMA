using PDDLSharp.Models.PDDL.Domain;

namespace MetaActionGenerator
{
    public class RemoveEffectParameters : BaseMetaGenerator
    {
        /// <summary>
        /// "C_{eff}, eliminates a parameter appearing in the effects, removing any precondition and/or effect that depends on it."
        /// </summary>
        /// <param name="actions"></param>
        /// <returns></returns>
        public override List<ActionDecl> Generate(List<ActionDecl> actions)
        {
            List<ActionDecl> metaActions = new List<ActionDecl>();

            foreach (var act in actions)
            {
                foreach (var arg in act.Parameters.Values)
                {
                    if (act.Effects.FindNames(arg.Name).Count > 0)
                    {
                        var newMetaAction = act.Copy();

                        newMetaAction.Parameters.Values.RemoveAll(x => x.Name == arg.Name);
                        RemoveMe(newMetaAction.Preconditions, arg.Name);
                        RemoveMe(newMetaAction.Effects, arg.Name);

                        RemoveUnusedParameters(newMetaAction);
                        metaActions.Add(newMetaAction);
                    }
                }
            }

            return metaActions;
        }
    }
}
