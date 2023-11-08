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
    public class RemoveEffectParametersTests
    {
        [TestMethod]
        [DataRow("TestData/sokoban/domain.pddl", 0, 2)]
        [DataRow("TestData/sokoban/domain.pddl", 1, 4)]
        [DataRow("TestData/satellite/domain.pddl", 0, 3)]
        [DataRow("TestData/satellite/domain.pddl", 1, 2)]
        [DataRow("TestData/satellite/domain.pddl", 2, 2)]
        [DataRow("TestData/satellite/domain.pddl", 3, 1)]
        [DataRow("TestData/satellite/domain.pddl", 4, 2)]
        public void Can_RemoveEffectParameters_CorrectAmountOfMetaActions(string domain, int actionID, int expectedAmount)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            ICandidateGenerator generator = new RemoveEffectParameters();

            // ACT
            var result = generator.Generate(new List<ActionDecl>() { decl.Actions[actionID] });

            // ASSERT
            Assert.AreEqual(expectedAmount, result.Count);
        }

        [TestMethod]
        [DataRow("TestData/sokoban/domain.pddl", 0, 0, "?to")]
        [DataRow("TestData/sokoban/domain.pddl", 0, 1, "?from")]
        [DataRow("TestData/sokoban/domain.pddl", 1, 0, "?bloc", "?floc", "?dir", "?b")]
        [DataRow("TestData/sokoban/domain.pddl", 1, 1, "?rloc", "?floc", "?b")]
        [DataRow("TestData/sokoban/domain.pddl", 1, 2, "?rloc", "?bloc", "?dir", "?b")]
        [DataRow("TestData/sokoban/domain.pddl", 1, 3, "?rloc", "?bloc", "?floc", "?dir")]
        [DataRow("TestData/satellite/domain.pddl", 0, 0, "?d_new", "?d_prev")]
        [DataRow("TestData/satellite/domain.pddl", 0, 1, "?s", "?d_prev")]
        [DataRow("TestData/satellite/domain.pddl", 0, 2, "?s", "?d_new")]
        [DataRow("TestData/satellite/domain.pddl", 1, 0, "?s")]
        [DataRow("TestData/satellite/domain.pddl", 1, 1, "?i")]
        [DataRow("TestData/satellite/domain.pddl", 2, 0, "?s")]
        [DataRow("TestData/satellite/domain.pddl", 2, 1, "?i")]
        [DataRow("TestData/satellite/domain.pddl", 3, 0, "?s", "?d")]
        [DataRow("TestData/satellite/domain.pddl", 4, 0, "?s", "?i", "?m")]
        [DataRow("TestData/satellite/domain.pddl", 4, 1, "?s", "?d", "?i")]
        public void Can_RemoveEffectParameters_CorrectParameters(string domain, int actionID, int metaID, params string[] expectedParams)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            ICandidateGenerator generator = new RemoveEffectParameters();

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
