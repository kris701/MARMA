using DependencyFetcher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyFetcher
{
    public interface IDependencyChecker
    {
        public DependencyList Dependencies { get; }
        public void CheckDependencies();
    }
}
