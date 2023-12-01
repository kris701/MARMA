using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Tools;

namespace MacroExtractor.Tests
{
    [TestClass]
    public class MacroExtractorTests
    {
        [ClassInitialize]
        public static async Task InitialiseAsync(TestContext context)
        {
            await GitFetcher.CheckAndDownloadBenchmarksAsync("https://github.com/kris701/MARMA-Test-Data", "benchmarks");
        }

        private bool FoundMatch(List<NameExp> names, string[] match)
        {
            if (names.Count != match.Length)
                return false;
            for (int i = 0; i < names.Count; i++)
                if (names[i].Name != match[i])
                    return false;
            return true;
        }

        #region ExtractMacros

        [TestMethod]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", 2)]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1", 7)]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/meta-4", 3)]
        public void Can_ExtractMacros_Count(string domain, string plansPath, int expectedMacroCount)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            var extractor = new MacroExtractor();
            var plans = new List<string>();
            foreach (var file in new DirectoryInfo(plansPath).GetFiles())
                plans.Add(file.FullName);

            // ACT
            var result = extractor.ExtractMacros(decl, plans);

            // ASSERT
            Assert.AreEqual(expectedMacroCount, result.Count);
        }

        [TestMethod]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/meta-4")]
        public void Can_ExtractMacros_UniqueMacrosOnly(string domain, string plansPath)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            var extractor = new MacroExtractor();
            var plans = new List<string>();
            foreach (var file in new DirectoryInfo(plansPath).GetFiles())
                plans.Add(file.FullName);

            // ACT
            var result = extractor.ExtractMacros(decl, plans);

            // ASSERT
            foreach (var item in result)
            {
                foreach (var other in result)
                {
                    if (item != other)
                        Assert.IsFalse(item.Equals(other));
                }
            }
        }

        [TestMethod]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", "$meta_1")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1", "$meta_1")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/meta-4", "$meta_4")]
        public void Can_ExtractMacros_MetaAction(string domain, string plansPath, string metaName)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            var extractor = new MacroExtractor();
            var plans = new List<string>();
            foreach (var file in new DirectoryInfo(plansPath).GetFiles())
                plans.Add(file.FullName);

            // ACT
            var result = extractor.ExtractMacros(decl, plans);

            // ASSERT
            foreach (var item in result)
                Assert.AreEqual(metaName, item.MetaAction.ActionName);
        }

        [TestMethod]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", "?0", "?1", "?1", "?2")]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", "?0", "?1", "?2", "?3")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/meta-4", "?0", "?1")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1", "?0", "?1", "?2", "?3", "?2", "?4", "?5")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1", "?0", "?1", "?2", "?3", "?4", "?5", "?6")]
        public void Can_ExtractMacros_MetaAction_Arguments(string domain, string plansPath, params string[] args)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            var extractor = new MacroExtractor();
            var plans = new List<string>();
            foreach (var file in new DirectoryInfo(plansPath).GetFiles())
                plans.Add(file.FullName);

            // ACT
            var result = extractor.ExtractMacros(decl, plans);

            // ASSERT
            Assert.IsTrue(result.Count > 0);
            var all = result.Where(x => FoundMatch(x.MetaAction.Arguments, args));
            bool any = false;
            foreach (var item in all)
            {
                Assert.AreEqual(item.MetaAction.Arguments.Count, args.Length);
                for (int i = 0; i < args.Length; i++)
                    Assert.AreEqual(args[i], item.MetaAction.Arguments[i].Name);
                any = true;
            }
            Assert.IsTrue(any);
        }

        [TestMethod]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", "$macro_1")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/meta-4", "$macro_4")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1", "$macro_1")]
        public void Can_ExtractMacros_MacroAction(string domain, string plansPath, string macroName)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            var extractor = new MacroExtractor();
            var plans = new List<string>();
            foreach (var file in new DirectoryInfo(plansPath).GetFiles())
                plans.Add(file.FullName);

            // ACT
            var result = extractor.ExtractMacros(decl, plans);

            // ASSERT
            foreach (var item in result)
                Assert.AreEqual(macroName, item.Macro.Name);
        }

        [TestMethod]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", "?0", "?1", "?2", "?O0")]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", "?0", "?1", "?2", "?3", "?O0")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/meta-4", "?O0", "?0", "?1")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/meta-4", "?0", "?1", "?O0")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/meta-4", "?0", "?O0", "?O1", "?1")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1", "?0", "?1", "?2", "?3", "?4", "?5", "?O0")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1", "?0", "?1", "?2", "?3", "?O0", "?4", "?5", "?O1")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1", "?0", "?1", "?2", "?3", "?4", "?O0", "?5", "?O1")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1", "?0", "?1", "?2", "?3", "?4", "?O0", "?5")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1", "?0", "?1", "?2", "?3", "?4", "?5", "?6", "?O0")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1", "?0", "?1", "?2", "?3", "?4", "?5", "?O0", "?6")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1", "?0", "?1", "?2", "?3", "?O0", "?4", "?5", "?6", "?O1")]
        public void Can_ExtractMacros_MacroAction_Arguments(string domain, string plansPath, params string[] args)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            var extractor = new MacroExtractor();
            var plans = new List<string>();
            foreach (var file in new DirectoryInfo(plansPath).GetFiles())
                plans.Add(file.FullName);

            // ACT
            var result = extractor.ExtractMacros(decl, plans);

            // ASSERT
            Assert.IsTrue(result.Count > 0);
            var all = result.Where(x => FoundMatch(x.Macro.Parameters.Values, args));
            bool any = false;
            foreach (var item in all)
            {
                Assert.AreEqual(item.Macro.Parameters.Values.Count, args.Length);
                for (int i = 0; i < args.Length; i++)
                    Assert.AreEqual(args[i], item.Macro.Parameters.Values[i].Name);
                any = true;
            }
            Assert.IsTrue(any);
        }

        [TestMethod]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/meta-4")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1")]
        public void Can_ExtractMacros_RemovedPrefixes(string domain, string plansPath)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            var extractor = new MacroExtractor();
            var plans = new List<string>();
            foreach (var file in new DirectoryInfo(plansPath).GetFiles())
                plans.Add(file.FullName);

            // ACT
            var result = extractor.ExtractMacros(decl, plans);

            // ASSERT
            foreach (var item in result)
            {
                foreach (var rep in item.Replacements)
                {
                    foreach (var seq in rep.Plan)
                    {
                        Assert.IsFalse(seq.ActionName.StartsWith("attack_"));
                        Assert.IsFalse(seq.ActionName.StartsWith("fix_"));
                    }
                }
            }
        }

        [TestMethod]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1")]
        [DataRow("benchmarks/ferry/domain.pddl", "benchmarks/ferry/meta-4")]
        [DataRow("benchmarks/rovers/domain.pddl", "benchmarks/rovers/meta-1")]
        public void Can_ExtractMacros_RemovedSufixes(string domain, string plansPath)
        {
            // ARRANGE
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var decl = parser.ParseAs<DomainDecl>(new FileInfo(domain));
            var extractor = new MacroExtractor();
            var plans = new List<string>();
            foreach (var file in new DirectoryInfo(plansPath).GetFiles())
                plans.Add(file.FullName);

            // ACT
            var result = extractor.ExtractMacros(decl, plans);

            // ASSERT
            foreach (var item in result)
            {
                foreach (var rep in item.Replacements)
                {
                    foreach (var seq in rep.Plan)
                    {
                        Assert.IsFalse(seq.ActionName.EndsWith("_0"));
                        Assert.IsFalse(seq.ActionName.EndsWith("_1"));
                        Assert.IsFalse(seq.ActionName.EndsWith("_2"));
                        Assert.IsFalse(seq.ActionName.EndsWith("_3"));
                        Assert.IsFalse(seq.ActionName.EndsWith("_4"));
                        Assert.IsFalse(seq.ActionName.EndsWith("_5"));
                    }
                }
            }
        }

        #endregion
    }
}
