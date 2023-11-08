using MetaActionGenerator.CandidateGenerators;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Parsers.PDDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActionGenerator.Tests.CandidateGenerators
{
    [TestClass]
    public class RemovePreconditionParametersTests
    {
        [TestMethod]
        [DataRow("TestData/sokoban/domain.pddl", 0, 1)]
        [DataRow("TestData/sokoban/domain.pddl", 1, 1)]
        [DataRow("TestData/satellite/domain.pddl", 0, 0)]
        [DataRow("TestData/satellite/domain.pddl", 1, 0)]
        [DataRow("TestData/satellite/domain.pddl", 2, 0)]
        [DataRow("TestData/satellite/domain.pddl", 3, 3)]
        [DataRow("TestData/satellite/domain.pddl", 4, 3)]
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
        [DataRow("TestData/sokoban/domain.pddl", 0, 0, "?from", "?to")]
        [DataRow("TestData/sokoban/domain.pddl", 1, 0, "?rloc", "?bloc", "?floc", "?b")]
        [DataRow("TestData/satellite/domain.pddl", 3, 0, "?s", "?i")]
        [DataRow("TestData/satellite/domain.pddl", 3, 1, "?i", "?d")]
        [DataRow("TestData/satellite/domain.pddl", 3, 2, "?i")]
        [DataRow("TestData/satellite/domain.pddl", 4, 0, "?s", "?d", "?m")]
        [DataRow("TestData/satellite/domain.pddl", 4, 1, "?d", "?i", "?m")]
        [DataRow("TestData/satellite/domain.pddl", 4, 2, "?d", "?m")]
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
