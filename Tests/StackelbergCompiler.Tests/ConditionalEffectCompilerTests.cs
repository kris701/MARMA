using CommandLine;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackelbergCompiler.Tests
{
    [TestClass]
    public class ConditionalEffectCompilerTests
    {
        [ClassInitialize]
        public static async Task InitialiseAsync(TestContext context)
        {
            await GitFetcher.CheckAndDownloadBenchmarksAsync("https://github.com/kris701/P9-Test-Data", "benchmarks");
        }

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
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction);

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
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction);

            // ASSERT
            Assert.IsNotNull(decl.Problem.Init);
            Assert.IsNotNull(result.Problem.Init);
            foreach(var item in decl.Problem.Init.Predicates)
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
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction);

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
            var result = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction);

            // ASSERT
            Assert.IsNotNull(result.Problem.Goal);
            Assert.IsInstanceOfType(result.Problem.Goal.GoalExp, typeof(AndExp));
            if (result.Problem.Goal.GoalExp is AndExp gAnd)
                Assert.AreEqual(totalGoal.Count, gAnd.Children.Count);
        }
    }
}
