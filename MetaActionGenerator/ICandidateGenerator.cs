using PDDLSharp.Models.PDDL.Domain;

namespace MetaActionGenerator
{
    public interface ICandidateGenerator
    {
        public DomainDecl Declaration { get; }
        public List<ActionDecl> Generate(List<ActionDecl> actions);
    }
}
