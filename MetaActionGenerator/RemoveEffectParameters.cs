using PDDLSharp.Models.PDDL.Domain;

namespace MetaActionGenerator
{
    public class RemoveEffectParameters : BaseMetaGenerator
    {
        public RemoveEffectParameters(DomainDecl declaration) : base(declaration)
        {
        }

        public override List<ActionDecl> Generate(List<ActionDecl> actions)
        {
            List<ActionDecl> metaActions = new List<ActionDecl>();

            foreach (var act in actions)
            {
                foreach (var arg in act.Parameters.Values)
                {
                    if (act.Effects.FindNames(arg.Name).Count > 0)
                    {
                        var newMetaAction = act.Copy(null);

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
