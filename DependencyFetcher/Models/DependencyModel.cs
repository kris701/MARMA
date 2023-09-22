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
        public string TargetLocation { get; set; }
        public List<SetupCall> SetupCalls { get; set; }

        public Dependency(string name, string description, string targetLocation, List<SetupCall> setupCalls)
        {
            Name = name;
            Description = description;
            TargetLocation = targetLocation;
            SetupCalls = setupCalls;
        }
    }
}
