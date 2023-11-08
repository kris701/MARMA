using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Parsers.PDDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActionGenerator.Tests
{
    [TestClass]
    public class MetaActionGeneratorTests
    {
        [TestMethod]
        [DataRow("../../../../../Dependencies/learning-benchmarks/blocksworld/domain.pddl")]
        [DataRow("../../../../../Dependencies/learning-benchmarks/childsnack/domain.pddl")]
        [DataRow("../../../../../Dependencies/learning-benchmarks/ferry/domain.pddl")]
        [DataRow("../../../../../Dependencies/learning-benchmarks/floortile/domain.pddl")]
        [DataRow("../../../../../Dependencies/learning-benchmarks/miconic/domain.pddl")]
        [DataRow("../../../../../Dependencies/learning-benchmarks/rovers/domain.pddl")]
        [DataRow("../../../../../Dependencies/learning-benchmarks/satellite/domain.pddl")]
        [DataRow("../../../../../Dependencies/learning-benchmarks/sokoban/domain.pddl")]
        [DataRow("../../../../../Dependencies/learning-benchmarks/spanner/domain.pddl")]
        [DataRow("../../../../../Dependencies/learning-benchmarks/transport/domain.pddl")]
        public void Can_GenerateSomeMetaActions(string domain)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            var generator = new MetaActionGenerator();

            // ACT
            var result = generator.GenerateMetaActions(decl.Actions);

            // ASSERT
            Assert.IsTrue(result.Count > 0);
        }
    }
}
