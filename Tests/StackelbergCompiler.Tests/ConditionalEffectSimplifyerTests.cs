using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Tools;

namespace StackelbergCompiler.Tests
{
    [TestClass]
    public class ConditionalEffectSimplifyerTests
    {
        [ClassInitialize]
        public static async Task InitialiseAsync(TestContext context)
        {
            await GitFetcher.CheckAndDownloadBenchmarksAsync("https://github.com/kris701/P9-Test-Data", "benchmarks");
        }

        [TestMethod]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p01.pddl", 24)]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/base_cases/p10.pddl", 24)]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p01.pddl", 62)]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/base_cases/p06.pddl", 62)]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p01.pddl", 71)]
        [DataRow("benchmarks/childsnack/domain.pddl", "benchmarks/childsnack/base_cases/p10.pddl", 71)]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p01.pddl", 71)]
        [DataRow("benchmarks/sokoban/domain.pddl", "benchmarks/sokoban/base_cases/p10.pddl", 71)]
        public void Can_SimplifyActions_TotalActionCount(string domainFile, string problemFile, int totalCount)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();
            metaAction.Name = "meta-name";
            var compiled = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());
            var simplifyer = new ConditionalEffectSimplifyer();

            // ACT
            var result = simplifyer.SimplifyConditionalEffects(compiled.Domain, compiled.Problem);

            // ASSERT
            Assert.AreEqual(totalCount, result.Domain.Actions.Count);
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
        public void Can_SimplifyActions_EachAction(string domainFile, string problemFile)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseDecl(new FileInfo(domainFile), new FileInfo(problemFile));
            var compiler = new ConditionalEffectCompiler();
            var metaAction = decl.Domain.Actions[0].Copy();
            metaAction.Name = "meta-name";
            var compiled = compiler.GenerateConditionalEffects(decl.Domain, decl.Problem, metaAction.Copy());
            var simplifyer = new ConditionalEffectSimplifyer();

            // ACT
            var result = simplifyer.SimplifyConditionalEffects(compiled.Domain, compiled.Problem);

            // ASSERT
            foreach (var compiledAct in compiled.Domain.Actions)
            {
                if (!compiledAct.Name.EndsWith(metaAction.Name) && compiledAct.Name.StartsWith(ReservedNames.FollowerActionPrefix))
                {
                    if (compiledAct.Effects is AndExp and)
                    {
                        var predCount = and.Count(x => x is not WhenExp);
                        var simplifiedCount = result.Domain.Actions.Count(x => x.Name.Substring(0, x.Name.LastIndexOf('_')) == compiledAct.Name);
                        Assert.AreEqual(simplifiedCount, GeneratePermutations(predCount));
                    }
                }
            }
        }

        private int GeneratePermutations(int count, int index = 0)
        {
            if (index >= count)
                return 1;
            int permutaitons = 0;
            permutaitons += GeneratePermutations(count, index + 1);
            permutaitons += GeneratePermutations(count, index + 1);
            return permutaitons;
        }
    }

}
