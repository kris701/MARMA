using System;
using Tools;

namespace PreBuilder
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Recreating path...");
            PathHelper.RecratePath("build");

            Console.WriteLine("Building...");
            PathHelper.RecratePath("build");

            // Train
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
            if (ArgsCallerBuilder.GetDotnetBuilder("MetaActions.Train").Run() != 0)
                throw new Exception("'MetaActions.Train' Build failed!");

            // Test
            if (ArgsCallerBuilder.GetRustBuilder("reconstruction").Run() != 0)
                throw new Exception("Reconstruction build failed!");
            if (ArgsCallerBuilder.GetDotnetBuilder("MetaActions.Test").Run() != 0)
                throw new Exception("'MetaActions.Test' Build failed!");

            Console.WriteLine("Copying build files...");
            CopyFiles("MetaActionGenerator/bin/Release/net7.0", "build/MetaActionGenerator/bin/Release/net7.0");
            CopyFiles("StackelbergCompiler/bin/Release/net7.0", "build/StackelbergCompiler/bin/Release/net7.0");
            CopyFiles("StackelbergVerifier/bin/Release/net7.0", "build/StackelbergVerifier/bin/Release/net7.0");
            CopyFiles("MacroExtractor/bin/Release/net7.0", "build/MacroExtractor/bin/Release/net7.0");
            CopyFiles("macros/target/release", "build/macros/target/release");
            CopyFiles("MetaActions.Train/bin/Release/net7.0", "build/MetaActions.Train/bin/Release/net7.0");

            CopyFiles("reconstruction/target/release", "build/reconstruction/target/release");
            CopyFiles("MetaActions.Test/bin/Release/net7.0", "build/MetaActions.Test/bin/Release/net7.0");

            Console.WriteLine("Copying dependencies...");
            CopyFilesRecursively("Dependencies", "build/Dependencies");
        }

        private static void CopyFiles(string path, string to)
        {
            PathHelper.RecratePath(to);
            foreach (var file in new DirectoryInfo(path).GetFiles())
                File.Copy(file.FullName, Path.Combine(to, file.Name));
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                }
                catch { }
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                } catch { }
            }
        }
    }
}