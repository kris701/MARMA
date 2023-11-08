using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Models.PDDL.Problem;
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
    public class TotalGoalGeneratorTests
    {
        [ClassInitialize]
        public static async Task InitialiseAsync(TestContext context)
        {
            await GitFetcher.CheckAndDownloadBenchmarksAsync("https://github.com/kris701/P9-Test-Data", "benchmarks");
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl", 6)]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl", 12)]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl", 23)]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl", 67)]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl", 9)]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl", 22)]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl", 147)]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl", 147)]
        public void Can_GenerateTotalGoal(string domainFile, string problemFile, int expectedGoals)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var domain = parser.ParseAs<DomainDecl>(new FileInfo(domainFile));
            var problem = parser.ParseAs<ProblemDecl>(new FileInfo(problemFile));
            StaticPredicateDetector.GenerateStaticPredicates(domain);

            // ACT
            var news = TotalGoalGenerator.GenerateTotalGoal(problem, domain);

            // ASSERT
            Assert.AreEqual(expectedGoals, news.Count);
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
        public void Can_GenerateTotalGoal_Uniques(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var domain = parser.ParseAs<DomainDecl>(new FileInfo(domainFile));
            var problem = parser.ParseAs<ProblemDecl>(new FileInfo(problemFile));
            StaticPredicateDetector.GenerateStaticPredicates(domain);

            // ACT
            var news = TotalGoalGenerator.GenerateTotalGoal(problem, domain);

            // ASSERT
            Assert.AreEqual(news.Count, news.Distinct().Count());
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
        public void Can_GenerateTotalGoal_CopyGoals(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var domain = parser.ParseAs<DomainDecl>(new FileInfo(domainFile));
            var problem = parser.ParseAs<ProblemDecl>(new FileInfo(problemFile));
            StaticPredicateDetector.GenerateStaticPredicates(domain);

            // ACT
            var news = TotalGoalGenerator.GenerateTotalGoal(problem, domain);
            var copy = TotalGoalGenerator.CopyTotalGoal();
            copy.Add(new PredicateExp(""));

            // ASSERT
            Assert.AreNotEqual(news.Count, copy.Count);
        }
    }
}
