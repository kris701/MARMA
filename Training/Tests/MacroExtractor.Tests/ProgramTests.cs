using PDDLSharp.Models.FastDownward.Plans;
using PDDLSharp.Models.PDDL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MacroExtractor.Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        [DataRow(1)]
        [DataRow(10)]
        [DataRow(50)]
        public void Can_GenerateCorrectAmountOfFiles(int count)
        {
            // ARRANGE
            var metaName = "meta_1";
            var repairSequences = new List<RepairSequence>();
            for (int i = 0; i < count; i++)
            {
                repairSequences.Add(new RepairSequence(
                    new GroundedAction(metaName, "a", "b"),
                    new ActionDecl("macro_1"),
                    new ActionPlan(new List<GroundedAction>()
                    {
                    new GroundedAction("action_1"),
                    new GroundedAction("action_2")
                    })
                    ));
            }
            var targetPath = Path.Combine("outPath", metaName);
            PathHelper.RecratePath(targetPath);

            // ACT
            Program.OutputReconstructionData(repairSequences, "outPath");

            // ASSERT
            Assert.AreEqual(repairSequences.Count * 2, new DirectoryInfo(targetPath).GetFiles().Count());
        }
    }
}
