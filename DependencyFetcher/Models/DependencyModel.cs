using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyFetcher.Models
{
    public class Dependency
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Uri RepositoryLink { get; set; }
        public string TargetLocation { get; set; }

        public string? BuildCommand { get; set; }
        public string? BuildArgs { get; set; }
    }
}
