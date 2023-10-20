using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MetaActions.Learn
{
    public static class PreBuilder
    {
        public static void BuildToolchain()
        {
            if (ArgsCallerBuilder.GetDotnetBuilder("MetaActionGenerator").Run() != 0)
                throw new Exception("Build failed!");
            if (ArgsCallerBuilder.GetDotnetBuilder("StacklebergCompiler").Run() != 0)
                throw new Exception("Build failed!");
            if (ArgsCallerBuilder.GetDotnetBuilder("StackelbergVerifier").Run() != 0)
                throw new Exception("Build failed!");
            if (ArgsCallerBuilder.GetRustBuilder("macros").Run() != 0)
                throw new Exception("Build failed!");

        }
    }
}
