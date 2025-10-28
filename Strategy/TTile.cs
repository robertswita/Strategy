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
        public static int Width = 64;
        public static int Height = 64;
        public virtual void ReadImage(BinaryReader reader) { }
        public virtual void WriteImage(BinaryWriter writer) { }
    }
}
