using PDDLSharp.Models.PDDL.Domain;

namespace MetaActionGenerator
{
    public interface ICandidateGenerator
    {
        public List<ActionDecl> Generate(List<ActionDecl> actions);
    }
}
