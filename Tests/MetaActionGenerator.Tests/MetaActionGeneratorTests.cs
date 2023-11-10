using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Tools;
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
        [ClassInitialize]
        public static async Task InitialiseAsync(TestContext context)
        {
            await GitFetcher.CheckAndDownloadBenchmarksAsync("https://github.com/kris701/P9-Test-Data", "benchmarks");
        }

        [TestMethod]
        [DataRow("benchmarks/blocksworld/domain.pddl")]
        [DataRow("benchmarks/childsnack/domain.pddl")]
        [DataRow("benchmarks/ferry/domain.pddl")]
        [DataRow("benchmarks/floortile/domain.pddl")]
        [DataRow("benchmarks/miconic/domain.pddl")]
        [DataRow("benchmarks/rovers/domain.pddl")]
        [DataRow("benchmarks/satellite/domain.pddl")]
        [DataRow("benchmarks/sokoban/domain.pddl")]
        [DataRow("benchmarks/spanner/domain.pddl")]
        [DataRow("benchmarks/transport/domain.pddl")]
        public void Can_GenerateSomeMetaActions(string domain)
        {
            if (!File.Exists(domain))
            {
                Assert.Inconclusive("Could not find dependencies!");
                return;
            }

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

        [TestMethod]
        public void Can_SanetizeMetaActions_IfEffectsEmpty()
        {
            // ARRANGE
            var act = new ActionDecl("act");
            var metaActions = new List<ActionDecl>() { act };
            var generator = new MetaActionGenerator();

            // ACT
            metaActions = generator.SanetizeMetaActions(metaActions);

            // ASSERT
            Assert.AreEqual(0, metaActions.Count);
        }

        [TestMethod]
        public void Cant_SanetizeMetaActions_IfEffectsNotEmpty()
        {
            // ARRANGE
            var act = new ActionDecl("act");
            act.Effects = new AndExp(new List<IExp>() {
                new PredicateExp("abc")
            });
            var metaActions = new List<ActionDecl>() { act };
            var generator = new MetaActionGenerator();

            // ACT
            metaActions = generator.SanetizeMetaActions(metaActions);

            // ASSERT
            Assert.AreNotEqual(0, metaActions.Count);
        }

        [TestMethod]
        public void Can_SanetizeMetaActions_EffectsSameAsPreconditions()
        {
            // ARRANGE
            var act = new ActionDecl("act");
            act.Effects = new AndExp(new List<IExp>() {
                new PredicateExp("abc")
            });
            act.Preconditions = new AndExp(new List<IExp>() {
                new PredicateExp("abc")
            });
            var metaActions = new List<ActionDecl>() { act };
            var generator = new MetaActionGenerator();

            // ACT
            metaActions = generator.SanetizeMetaActions(metaActions);

            // ASSERT
            Assert.AreEqual(0, metaActions.Count);
        }

        [TestMethod]
        public void Can_RemoveDuplicateMetaActions()
        {
            // ARRANGE
            var act1 = new ActionDecl("act1");
            act1.Preconditions = new PredicateExp("abc");
            var act2 = new ActionDecl("act1");
            act2.Preconditions = new PredicateExp("abc");
            var metaActions = new List<ActionDecl>() { act1, act2 };
            var generator = new MetaActionGenerator();

            // ACT
            metaActions = generator.RemoveDuplicateMetaActions(metaActions);

            // ASSERT
            Assert.AreEqual(1, metaActions.Count);
        }
    }
}
