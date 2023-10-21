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
                throw new Exception("'MetaActionGenerator' Build failed!");
            if (ArgsCallerBuilder.GetDotnetBuilder("StacklebergCompiler").Run() != 0)
                throw new Exception("'StacklebergCompiler' Build failed!");
            if (ArgsCallerBuilder.GetDotnetBuilder("StackelbergVerifier").Run() != 0)
                throw new Exception("'StackelbergVerifier' Build failed!");
            if (ArgsCallerBuilder.GetRustBuilder("macros").Run() != 0)
                throw new Exception("'macros' Build failed!");

        }
    }
}
