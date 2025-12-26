using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TResource: TTile
    {
        public enum ResType { Chest, Crystals, Gems, Gold, Iron, Stone, Sulfur, Wood };
        public ResType Type;
        public int Value = 1;
    }
}
