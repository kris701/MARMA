using PDDLSharp.Models.Domain;
using PDDLSharp.Models.Problem;
using PDDLSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using PDDLSharp.Models.Expressions;

namespace StacklebergCompiler
{
    public class ConditionalEffectAbstractor
    {
        public PDDLDecl AbstractConditionalEffects(DomainDecl domain, ProblemDecl problem)
        {
            var newDomain = domain.Copy();

            newDomain.Actions = GenerateAbstractedActions(newDomain.Actions);

            return new PDDLDecl(newDomain, problem);
        }

        private List<ActionDecl> GenerateAbstractedActions(List<ActionDecl> actions)
        {
            List<ActionDecl> newActions = new List<ActionDecl>();

            foreach(var action in actions)
            {
                if (action.Name.StartsWith(ReservedNames.FollowerActionPrefix))
                    newActions.AddRange(GeneratePossibleActions(action));
                else
                    newActions.Add(action);
            }

            return newActions;
        }

        private List<ActionDecl> GeneratePossibleActions(ActionDecl source)
        {
            List<ActionDecl> newActions = new List<ActionDecl>();
            if (source.Effects is AndExp and)
            {
                and.Children.RemoveAll(x => x is WhenExp);
                var trues = GenerateTrue(and.Children);
                var permutations = GeneratePermutations(and.Children.Count, 0, new List<bool>());
                int counter = 0;
                foreach (var permutation in permutations)
                {
                    var newAct = source.Copy();

                    for (int i = 0; i < permutation.Count; i++)
                    {
                        var pred = GetActualPredicate(and.Children[i]);
                        if (newAct.Preconditions is AndExp preAnd && newAct.Effects is AndExp effAnd)
                        {
                            if (permutation[i])
                            {
                                preAnd.Children.Add(CopyAndPrefixPredicate(pred, ReservedNames.LeaderStatePrefix));
                                if (trues[i])
                                    effAnd.Children.Add(CopyAndPrefixPredicate(pred, ReservedNames.IsGoalPrefix));
                                else
                                    effAnd.Children.Add(new NotExp(CopyAndPrefixPredicate(pred, ReservedNames.IsGoalPrefix)));
                            }
                            else
                            {
                                preAnd.Children.Add(new NotExp(CopyAndPrefixPredicate(pred, ReservedNames.LeaderStatePrefix)));
                                if (trues[i])
                                    effAnd.Children.Add(new NotExp(CopyAndPrefixPredicate(pred, ReservedNames.IsGoalPrefix)));
                                else
                                    effAnd.Children.Add(CopyAndPrefixPredicate(pred, ReservedNames.IsGoalPrefix));
                            }
                        }
                    }

                    newAct.Name = $"{newAct.Name}_{counter++}";
                    newActions.Add(newAct);
                }
            }
            return newActions;
        }

        private PredicateExp GetActualPredicate(IExp item)
        {
            if (item is NotExp not)
                if (not.Child is PredicateExp nPred)
                    return nPred;
            if (item is PredicateExp pred)
                return pred;
            throw new Exception("Expected a predicate");
        }

        private PredicateExp CopyAndPrefixPredicate(PredicateExp pred, string name)
        {
            var copy = pred.Copy();
            copy.Name = $"{name}{copy.Name}";
            return copy;
        }

        private List<bool> GenerateTrue(List<IExp> items)
        {
            List<bool> returnList = new List<bool>();

            foreach (var item in items)
            {
                if (item is NotExp)
                    returnList.Add(false);
                else
                    returnList.Add(true);
            }

            return returnList;
        }

        private List<List<bool>> GeneratePermutations(int count, int index, List<bool> source)
        {
            List<List<bool>> returnList = new List<List<bool>>();
            if (index >= count)
            {
                returnList.Add(source);
                return returnList;
            }

            var trueSource = source.Copy();
            trueSource.Insert(index, true);
            returnList.AddRange(GeneratePermutations(count, index + 1, trueSource));

            var falseSource = source.Copy();
            falseSource.Insert(index, false);
            returnList.AddRange(GeneratePermutations(count, index + 1, falseSource));

            return returnList;
        }
    }
}
