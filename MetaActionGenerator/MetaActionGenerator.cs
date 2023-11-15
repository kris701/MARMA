using MetaActionGenerator.CandidateGenerators;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Models.SAS;
using PDDLSharp.Tools;

namespace MetaActionGenerator
{
    public class MetaActionGenerator
    {
        public List<ActionDecl> GenerateMetaActions(List<ActionDecl> from)
        {
            var metaActions = new List<ActionDecl>();

            metaActions.AddRange(new RemovePreconditionParameters().Generate(from));
            metaActions.AddRange(new RemoveEffectParameters().Generate(from));
            metaActions.AddRange(new RemoveAdditionalEffects().Generate(from));

            metaActions = SanetizeMetaActions(metaActions);
            metaActions = RemoveDuplicateMetaActions(metaActions);

            return metaActions;
        }

        public List<ActionDecl> SanetizeMetaActions(List<ActionDecl> metaActions)
        {
            var asOps = new List<Operator>();
            foreach (var act in metaActions)
                asOps.Add(ToOperator(act));

            var sanetized = new List<ActionDecl>();
            for (int i = 0; i < asOps.Count; i++)
            {
                if (asOps[i].Add.Length == 0 && asOps[i].Del.Length == 0)
                    continue;

                bool areEqual = true;
                if (asOps[i].Pre.Length == 0 && (asOps[i].Add.Length != 0 || asOps[i].Del.Length != 0))
                    areEqual = false;

                foreach (var pre in asOps[i].Pre)
                {
                    if (asOps[i].Del.Length + asOps[i].Add.Length != asOps[i].Pre.Length)
                        areEqual = false;
                    else if (!asOps[i].Del.Any(x => AreFactsEqual(pre, x)) && !asOps[i].Add.Any(x => AreFactsEqual(pre, x)))
                        areEqual = false;

                    if (!areEqual)
                        break;
                }
                if (!areEqual)
                    sanetized.Add(metaActions[i]);
            }
            return sanetized;
        }

        public List<ActionDecl> RemoveDuplicateMetaActions(List<ActionDecl> metaActions)
        {
            var asOps = new List<Operator>();
            foreach (var act in metaActions)
                asOps.Add(ToOperator(act));

            var nonDuplicates = new List<ActionDecl>();
            for (int i = 0; i < asOps.Count; i++)
            {
                bool any = false;
                for (int j = i + 1; j < asOps.Count; j++)
                {
                    if (i != j)
                    {
                        if (AreOperatorsEqual(asOps[i], asOps[j]))
                        {
                            any = true;
                            break;
                        }
                    }
                }
                if (!any)
                    nonDuplicates.Add(metaActions[i]);
            }

            return nonDuplicates;
        }

        private Operator ToOperator(ActionDecl act)
        {
            var args = new List<string>();
            foreach (var arg in act.Parameters.Values)
                args.Add(arg.Name);

            var effFacts = ExtractFactsFromExp(act.Effects);
            var add = effFacts[true];
            var del = effFacts[false];

            var preFacts = ExtractFactsFromExp(act.Preconditions);
            var pre = preFacts[true];
            if (preFacts[false].Count > 0)
            {
                foreach (var fact in preFacts[false])
                {
                    var nFact = GetNegatedOf(fact);
                    pre.Add(nFact);

                    bool addToAdd = false;
                    bool addToDel = false;
                    if (add.Any(x => AreFactsEqual(fact, x)))
                        addToDel = true;
                    if (del.Any(x => AreFactsEqual(fact, x)))
                        addToAdd = true;

                    if (addToAdd)
                        add.Add(nFact);
                    if (addToDel)
                        del.Add(nFact);
                }
            }

            var newOp = new Operator(act.Name, args.ToArray(), pre.ToArray(), add.ToArray(), del.ToArray());
            return newOp;
        }

        private Dictionary<bool, HashSet<Fact>> ExtractFactsFromExp(IExp exp, bool possitive = true)
        {
            var facts = new Dictionary<bool, HashSet<Fact>>();
            facts.Add(true, new HashSet<Fact>());
            facts.Add(false, new HashSet<Fact>());

            switch (exp)
            {
                case NumericExp: break;
                case EmptyExp: break;
                case PredicateExp pred: facts[possitive].Add(GetFactFromPredicate(pred)); break;
                case NotExp not: facts = MergeDictionaries(facts, ExtractFactsFromExp(not.Child, !possitive)); break;
                case AndExp and:
                    foreach (var child in and.Children)
                        facts = MergeDictionaries(facts, ExtractFactsFromExp(child, possitive));
                    break;
                default:
                    throw new ArgumentException($"Cannot translate node type '{exp.GetType().Name}'");
            }

            return facts;
        }

        private Dictionary<bool, HashSet<Fact>> MergeDictionaries(Dictionary<bool, HashSet<Fact>> dict1, Dictionary<bool, HashSet<Fact>> dict2)
        {
            var resultDict = new Dictionary<bool, HashSet<Fact>>();
            foreach (var key in dict1.Keys)
                resultDict.Add(key, dict1[key]);
            foreach (var key in dict2.Keys)
                resultDict[key].AddRange(dict2[key]);

            return resultDict;
        }

        private Fact GetFactFromPredicate(PredicateExp pred)
        {
            var name = pred.Name;
            var args = new List<string>();
            foreach (var arg in pred.Arguments)
                args.Add(arg.Name);
            var newFact = new Fact(name, args.ToArray());
            return newFact;
        }

        private Fact GetNegatedOf(Fact fact)
        {
            var newFact = new Fact($"$neg-{fact.Name}", fact.Arguments);
            return newFact;
        }

        private bool AreOperatorsEqual(Operator op1, Operator op2)
        {
            if (op1.Name != op2.Name)
                return false;
            if (op1.Arguments.Length != op2.Arguments.Length)
                return false;
            for (int i = 0; i < op1.Arguments.Length; i++)
                if (op1.Arguments[i] != op2.Arguments[i])
                    return false;
            foreach (var pre in op1.Pre)
                if (!op2.Pre.Any(x => AreFactsEqual(pre, x)))
                    return false;
            foreach (var add in op1.Add)
                if (!op2.Add.Any(x => AreFactsEqual(add, x)))
                    return false;
            foreach (var del in op1.Del)
                if (!op2.Del.Any(x => AreFactsEqual(del, x)))
                    return false;
            return true;
        }

        private bool AreFactsEqual(Fact f1, Fact f2)
        {
            if (f1.Name != f2.Name)
                return false;
            if (f1.Arguments.Length != f2.Arguments.Length)
                return false;
            for (int i = 0; i < f1.Arguments.Length; i++)
                if (f1.Arguments[i] != f2.Arguments[i])
                    return false;
            return true;
        }
    }
}
