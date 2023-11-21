using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActions.Train
{
    public interface ICancelable
    {
        public string Name { get; }
        public int RunID { get; }
        public CancellationTokenSource CancellationToken { get; }
        public void Kill();
    }
}
