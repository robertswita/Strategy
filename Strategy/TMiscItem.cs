using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TMiscItem: THealItem
    {
        public int Evasion;

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Evasion = reader.ReadInt32();
        }
    }
}
