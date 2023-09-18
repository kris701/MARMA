using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class ProjectHelper
    {
        public static string GetProjectPath()
        {
            var current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current.Name != "P9")
                current = current.Parent;
            return current.FullName;
        }
    }
}
