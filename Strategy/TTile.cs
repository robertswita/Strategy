using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TTile: TPiece
    {
        public virtual void ReadImage(BinaryReader reader) { }
    }
}
