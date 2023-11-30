using MetaActionGenerator.CandidateGenerators;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Tools;

namespace MetaActionGenerator.Tests.CandidateGenerators
{
    [TestClass]
    public class RemoveAdditionalEffectsTests
    {
        [ClassInitialize]
        public static async Task InitialiseAsync(TestContext context)
        {
            await GitFetcher.CheckAndDownloadBenchmarksAsync("https://github.com/kris701/MARMA-Test-Data", "benchmarks");
        }

        [TestMethod]
        [DataRow("benchmarks/sokoban/domain.pddl", 0, 2)]
        [DataRow("benchmarks/sokoban/domain.pddl", 1, 4)]
        [DataRow("benchmarks/satellite/domain.pddl", 0, 3)]
        [DataRow("benchmarks/satellite/domain.pddl", 1, 2)]
        [DataRow("benchmarks/satellite/domain.pddl", 2, 2)]
        [DataRow("benchmarks/satellite/domain.pddl", 3, 1)]
        [DataRow("benchmarks/satellite/domain.pddl", 4, 2)]
        public void Can_RemoveAdditionalEffects_CorrectAmountOfMetaActions(string domain, int actionID, int expectedAmount)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            ICandidateGenerator generator = new RemoveAdditionalEffects();

            // ACT
            var result = generator.Generate(new List<ActionDecl>() { decl.Actions[actionID] });

            // ASSERT
            Assert.AreEqual(expectedAmount, result.Count);
        }

        [TestMethod]
        [DataRow("benchmarks/sokoban/domain.pddl", 0, 0, "?to")]
        [DataRow("benchmarks/sokoban/domain.pddl", 0, 1)]
        [DataRow("benchmarks/sokoban/domain.pddl", 1, 0, "?bloc", "?floc", "?b")]
        [DataRow("benchmarks/sokoban/domain.pddl", 1, 1)]
        [DataRow("benchmarks/sokoban/domain.pddl", 1, 2, "?rloc", "?bloc")]
        [DataRow("benchmarks/sokoban/domain.pddl", 1, 3, "?rloc", "?bloc", "?floc", "?dir")]
        [DataRow("benchmarks/satellite/domain.pddl", 0, 0)]
        [DataRow("benchmarks/satellite/domain.pddl", 0, 1)]
        [DataRow("benchmarks/satellite/domain.pddl", 0, 2)]
        [DataRow("benchmarks/satellite/domain.pddl", 1, 0, "?s")]
        [DataRow("benchmarks/satellite/domain.pddl", 1, 1, "?i")]
        [DataRow("benchmarks/satellite/domain.pddl", 2, 0, "?s")]
        [DataRow("benchmarks/satellite/domain.pddl", 2, 1, "?i")]
        [DataRow("benchmarks/satellite/domain.pddl", 3, 0, "?s", "?d")]
        [DataRow("benchmarks/satellite/domain.pddl", 4, 0, "?s", "?i", "?m")]
        [DataRow("benchmarks/satellite/domain.pddl", 4, 1, "?s", "?d", "?i")]
        public void Can_RemoveAdditionalEffects_CorrectParameters(string domain, int actionID, int metaID, params string[] expectedParams)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            ICandidateGenerator generator = new RemoveAdditionalEffects();

            // ACT
            var result = generator.Generate(new List<ActionDecl>() { decl.Actions[actionID] });

            // ASSERT
            Assert.IsTrue(result.Count > metaID);
            Assert.AreEqual(expectedParams.Length, result[metaID].Parameters.Values.Count);
            for (int i = 0; i < expectedParams.Length; i++)
                Assert.AreEqual(expectedParams[i], result[metaID].Parameters.Values[i].Name);
        }
    }
}
