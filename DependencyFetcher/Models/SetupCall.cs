using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyFetcher.Models
{
    public class SetupCall
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }

        public SetupCall(string name, string command, string arguments)
        {
            Name = name;
            Command = command;
            Arguments = arguments;
        }
    }
}
