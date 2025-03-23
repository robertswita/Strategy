using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TAnimation
    {
        public List<TFrame[][]> Sequences = new List<TFrame[][]>();
        public List<TAnimation> Source;
        public int Index;
        public string Name;
        public bool Short6Seq;
    }
}
