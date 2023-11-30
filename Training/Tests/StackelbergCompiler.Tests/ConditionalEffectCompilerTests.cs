using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Tools;

namespace StackelbergCompiler.Tests
{
    [TestClass]
    public class ConditionalEffectCompilerTests
    {
        [ClassInitialize]
        public static async Task InitialiseAsync(TestContext context)
        {
            await GitFetcher.CheckAndDownloadBenchmarksAsync("https://github.com/kris701/MARMA-Test-Data", "benchmarks");
        }

        #region Problem

        [TestMethod]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_MoveConstantsToProblem(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();
            Assert.IsNotNull(decl.Domain.Constants);

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            Assert.IsNull(result.Domain.Constants);
            Assert.IsNotNull(result.Problem.Objects);
            foreach (var cons in decl.Domain.Constants.Constants)
                Assert.IsTrue(result.Problem.Objects.Objs.Contains(cons));
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_GenerateLeaderInits(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            Assert.IsNotNull(decl.Problem.Init);
            Assert.IsNotNull(result.Problem.Init);
            foreach (var item in decl.Problem.Init.Predicates)
                if (item is PredicateExp pred && !StaticPredicateDetector.StaticPredicates.Any(x => x == pred.Name))
                    Assert.IsTrue(result.Problem.Init.Predicates.Any(x => x is PredicateExp cPred && cPred.Name == $"{ReservedNames.LeaderStatePrefix}{pred.Name}"));
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_InsertTotalGoalIntoInit(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();
            StaticPredicateDetector.GenerateStaticPredicates(decl.Domain);
            TotalGoalGenerator.GenerateTotalGoal(decl.Problem, decl.Domain);
            var totalGoal = TotalGoalGenerator.CopyTotalGoal();

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            Assert.IsNotNull(result.Problem.Init);
            foreach (var item in totalGoal)
                Assert.IsTrue(result.Problem.Init.Predicates.Any(x => x is PredicateExp cPred && cPred.Name == item.Name));
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_ReplaceGoalWithTotalGoal(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();
            StaticPredicateDetector.GenerateStaticPredicates(decl.Domain);
            TotalGoalGenerator.GenerateTotalGoal(decl.Problem, decl.Domain);
            var totalGoal = TotalGoalGenerator.CopyTotalGoal();

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            Assert.IsNotNull(result.Problem.Goal);
            Assert.IsInstanceOfType(result.Problem.Goal.GoalExp, typeof(AndExp));
            if (result.Problem.Goal.GoalExp is AndExp gAnd)
                Assert.AreEqual(totalGoal.Count, gAnd.Children.Count);
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_InsertTurnPredicateIntoInits(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            Assert.IsNotNull(result.Problem.Init);
            Assert.IsTrue(result.Problem.Init.Any(x => x is PredicateExp pred && pred.Name == ReservedNames.LeaderTurnPredicate && pred.Arguments.Count == 0));
        }

        #endregion

        #region Domain

        [TestMethod]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_RemoveConstantsFromDomain(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();
            Assert.IsNotNull(decl.Domain.Constants);

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            Assert.IsNull(result.Domain.Constants);
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_SplitActionsIntoLeaderFollowerVersions(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            foreach (var act in decl.Domain.Actions)
            {
                Assert.IsTrue(result.Domain.Actions.Any(x => x.Name.StartsWith(ReservedNames.LeaderActionPrefix)));
                Assert.IsTrue(result.Domain.Actions.Any(x => x.Name.StartsWith(ReservedNames.FollowerActionPrefix)));
            }
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_InsertAndIntoAllActions(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            foreach (var act in result.Domain.Actions)
            {
                Assert.IsInstanceOfType(act.Preconditions, typeof(AndExp));
                Assert.IsInstanceOfType(act.Effects, typeof(AndExp));
            }
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_GenerateLeaderPredicates(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            Assert.IsNotNull(decl.Domain.Predicates);
            Assert.IsNotNull(result.Domain.Predicates);
            foreach (var pred in decl.Domain.Predicates.Predicates)
                if (!StaticPredicateDetector.StaticPredicates.Any(x => x == pred.Name))
                    Assert.IsTrue(result.Domain.Predicates.Predicates.Any(x => x.Name == $"{ReservedNames.LeaderStatePrefix}{pred.Name}"));
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_GenerateGoalPredicates(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            Assert.IsNotNull(decl.Domain.Predicates);
            Assert.IsNotNull(result.Domain.Predicates);
            foreach (var pred in decl.Domain.Predicates.Predicates)
                if (!StaticPredicateDetector.StaticPredicates.Any(x => x == pred.Name))
                    Assert.IsTrue(result.Domain.Predicates.Predicates.Any(x => x.Name == $"{ReservedNames.IsGoalPrefix}{pred.Name}"));
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_UpdateLeaderActionsPredicatesAndEffects(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            foreach (var act in result.Domain.Actions)
            {
                if (act.Name.StartsWith(ReservedNames.LeaderActionPrefix) && !act.Name.EndsWith(metaAction.Name))
                {
                    var allPre = act.Preconditions.FindTypes<PredicateExp>();
                    foreach (var pred in allPre)
                        if (!StaticPredicateDetector.StaticPredicates.Any(x => x == pred.Name) && pred.Name != ReservedNames.LeaderTurnPredicate)
                            Assert.IsTrue(pred.Name.StartsWith(ReservedNames.LeaderStatePrefix));

                    int followers = 0;
                    int leaders = 0;
                    if (act.Effects is AndExp effAnd)
                    {
                        foreach (var child in effAnd.Children)
                        {
                            if (child is PredicateExp pred)
                            {
                                if (pred.Name.StartsWith(ReservedNames.LeaderStatePrefix))
                                    leaders++;
                                else
                                    followers++;
                            }
                        }
                    }
                    Assert.AreEqual(leaders, followers);
                }
            }
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_InsertConditionalEffectsToFollowerActions(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            foreach (var act in result.Domain.Actions)
            {
                if (act.Name.StartsWith(ReservedNames.FollowerActionPrefix))
                {
                    var effPreds = act.Effects.FindTypes<PredicateExp>();
                    int pos = 0;
                    int neg = 0;
                    foreach (var pred in effPreds)
                    {
                        if (pred.Parent is NotExp)
                            neg++;
                        if (pred.Parent is AndExp)
                            pos++;
                    }
                    Assert.AreEqual(act.Effects.FindTypes<WhenExp>().Count, pos * 2 + neg * 2);
                }
            }
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_UpdateAndInsertMetaActionToFit(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();
            metaAction.Name = "meta-name";

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            bool any = false;
            foreach (var act in result.Domain.Actions)
            {
                if (act.Name.EndsWith(metaAction.Name))
                {
                    Assert.IsFalse(any);
                    any = true;
                    var allPre = act.Preconditions.FindTypes<PredicateExp>();
                    foreach (var pred in allPre)
                        if (!StaticPredicateDetector.StaticPredicates.Any(x => x == pred.Name) && pred.Name != ReservedNames.LeaderTurnPredicate)
                            Assert.IsTrue(pred.Name.StartsWith(ReservedNames.LeaderStatePrefix));

                    if (act.Effects is AndExp effAnd)
                    {
                        Assert.IsTrue(effAnd.Any(x => x is NotExp not && not.Child is PredicateExp pred && pred.Name == ReservedNames.LeaderTurnPredicate));
                        Assert.AreEqual(
                            metaAction.Effects.FindTypes<PredicateExp>().Count * 2 + 1,
                            effAnd.Children.Count);
                    }
                }
            }
            Assert.IsTrue(any);
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_InsertTurnPredicateIntoActionsPreconditions(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();
            metaAction.Name = "meta-name";

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            foreach (var act in result.Domain.Actions)
            {
                if (act.Name.StartsWith(ReservedNames.LeaderActionPrefix) && act.Preconditions is AndExp leaderPreAnd)
                    Assert.IsTrue(leaderPreAnd.Any(x => x is PredicateExp pred && pred.Name == ReservedNames.LeaderTurnPredicate));
                else if (act.Name.StartsWith(ReservedNames.FollowerActionPrefix) && act.Preconditions is AndExp followerPreAnd)
                    Assert.IsTrue(followerPreAnd.Any(x => x is NotExp not && not.Child is PredicateExp pred && pred.Name == ReservedNames.LeaderTurnPredicate));
            }
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl")]
        public void Can_InsertTurnPredicateIntoPredicates(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();
            metaAction.Name = "meta-name";

            // ACT
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());

            // ASSERT
            Assert.IsNotNull(result.Domain.Predicates);
            Assert.IsTrue(result.Domain.Predicates.Predicates.Any(x => x.Name == ReservedNames.LeaderTurnPredicate));
        }

        #endregion
    }
}
