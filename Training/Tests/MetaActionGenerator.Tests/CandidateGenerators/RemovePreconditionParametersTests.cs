using MetaActionGenerator.CandidateGenerators;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Tools;

namespace MetaActionGenerator.Tests.CandidateGenerators
{
    [TestClass]
    public class RemovePreconditionParametersTests
    {
        [ClassInitialize]
        public static async Task InitialiseAsync(TestContext context)
        {
            await GitFetcher.CheckAndDownloadBenchmarksAsync("https://github.com/kris701/P9-Test-Data", "benchmarks");
        }

        [TestMethod]
        [DataRow("benchmarks/sokoban/domain.pddl", 0, 1)]
        [DataRow("benchmarks/sokoban/domain.pddl", 1, 1)]
        [DataRow("benchmarks/satellite/domain.pddl", 0, 0)]
        [DataRow("benchmarks/satellite/domain.pddl", 1, 0)]
        [DataRow("benchmarks/satellite/domain.pddl", 2, 0)]
        [DataRow("benchmarks/satellite/domain.pddl", 3, 3)]
        [DataRow("benchmarks/satellite/domain.pddl", 4, 3)]
        public void Can_RemovePreconditionParameters_CorrectAmountOfMetaActions(string domain, int actionID, int expectedAmount)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            ICandidateGenerator generator = new RemovePreconditionParameters();

            // ACT
            var result = generator.Generate(new List<ActionDecl>() { decl.Actions[actionID] });

            // ASSERT
            Assert.AreEqual(expectedAmount, result.Count);
        }

        [TestMethod]
        [DataRow("benchmarks/sokoban/domain.pddl", 0, 0, "?from", "?to")]
        [DataRow("benchmarks/sokoban/domain.pddl", 1, 0, "?rloc", "?bloc", "?floc", "?b")]
        [DataRow("benchmarks/satellite/domain.pddl", 3, 0, "?s", "?i")]
        [DataRow("benchmarks/satellite/domain.pddl", 3, 1, "?i", "?d")]
        [DataRow("benchmarks/satellite/domain.pddl", 3, 2, "?i")]
        [DataRow("benchmarks/satellite/domain.pddl", 4, 0, "?s", "?d", "?m")]
        [DataRow("benchmarks/satellite/domain.pddl", 4, 1, "?d", "?i", "?m")]
        [DataRow("benchmarks/satellite/domain.pddl", 4, 2, "?d", "?m")]
        public void Can_RemovePreconditionParameters_CorrectParameters(string domain, int actionID, int metaID, params string[] expectedParams)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            ICandidateGenerator generator = new RemovePreconditionParameters();

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
