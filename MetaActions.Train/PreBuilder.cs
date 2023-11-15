using Tools;

namespace MetaActions.Learn
{
    public static class PreBuilder
    {
        public static void BuildToolchain()
        {
            if (ArgsCallerBuilder.GetDotnetBuilder("MetaActionGenerator").Run() != 0)
                throw new Exception("'MetaActionGenerator' Build failed!");
            if (ArgsCallerBuilder.GetDotnetBuilder("StackelbergCompiler").Run() != 0)
                throw new Exception("'StackelbergCompiler' Build failed!");
            if (ArgsCallerBuilder.GetDotnetBuilder("StackelbergVerifier").Run() != 0)
                throw new Exception("'StackelbergVerifier' Build failed!");
            if (ArgsCallerBuilder.GetDotnetBuilder("MacroExtractor").Run() != 0)
                throw new Exception("'MacroExtractor' Build failed!");
            if (ArgsCallerBuilder.GetRustBuilder("macros").Run() != 0)
                throw new Exception("'macros' Build failed!");

        }
    }
}
