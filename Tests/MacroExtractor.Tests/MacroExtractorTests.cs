﻿using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroExtractor.Tests
{
    [TestClass]
    public class MacroExtractorTests
    {
        [ClassInitialize]
        public static async Task InitialiseAsync(TestContext context)
        {
            await GitFetcher.CheckAndDownloadBenchmarksAsync("https://github.com/kris701/P9-Test-Data", "benchmarks");
        }

        [TestMethod]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", 4)]
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
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", "$meta_1")]
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
            foreach(var item in result)
                Assert.AreEqual(metaName, item.MetaAction.ActionName);
        }

        [TestMethod]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", 0, "?0", "?1", "?1", "?2")]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", 1, "?0", "?1", "?1", "?2")]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", 2, "?0", "?1", "?2", "?3")]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", 3, "?0", "?1", "?2", "?3")]
        public void Can_ExtractMacros_MetaAction_Arguments(string domain, string plansPath, int metaIndex, params string[] args)
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
            Assert.IsTrue(metaIndex < result.Count);
            Assert.AreEqual(result[metaIndex].MetaAction.Arguments.Count, args.Length);
            for (int i = 0; i < args.Length; i++)
                Assert.AreEqual(args[i], result[metaIndex].MetaAction.Arguments[i].Name);
        }

        [TestMethod]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", "$macro_1")]
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
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", 0, "?0", "?1", "?2", "?O0")]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", 1, "?0", "?1", "?2", "?O0")]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", 2, "?0", "?1", "?2", "?3", "?O0")]
        [DataRow("benchmarks/floortile/domain.pddl", "benchmarks/floortile/meta-1", 3, "?0", "?1", "?2", "?3", "?O0")]
        public void Can_ExtractMacros_MacroAction_Arguments(string domain, string plansPath, int metaIndex, params string[] args)
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
            Assert.IsTrue(metaIndex < result.Count);
            Assert.AreEqual(result[metaIndex].Macro.Parameters.Values.Count, args.Length);
            for (int i = 0; i < args.Length; i++)
                Assert.AreEqual(args[i], result[metaIndex].Macro.Parameters.Values[i].Name);
        }
    }
}
