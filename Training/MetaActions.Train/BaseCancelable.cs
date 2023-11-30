using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MetaActions.Train
{
    public abstract class BaseCancelable : ICancelable
    {
        public string Name { get; }
        public int RunID { get; set; }
        public CancellationTokenSource CancellationToken { get; }
        internal Process? _activeProcess;

        protected BaseCancelable(string name, int runID, CancellationTokenSource cancellationToken)
        {
            Name = name;
            RunID = runID;
            CancellationToken = cancellationToken;
            CancellationToken.Token.Register(Kill);
        }

        internal void Print(string text, ConsoleColor color)
        {
            ConsoleHelper.WriteLineColor($"\t[{Name}] {text}", color);
        }

        public void Kill()
        {
            if (_activeProcess != null)
            {
                try
                {
                    _activeProcess.Kill(true);
                    while (!_activeProcess.HasExited)
                        _activeProcess.Kill(true);
                    _activeProcess.WaitForExit();
                }
                catch {
                    Console.WriteLine("Could not kill process???");
                }
            }
        }
    }
}
