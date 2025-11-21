using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class THealItem: TItem
    {
        public int Life;
        public int MagicPoints;
        public int Power;
        public int Agility;
        public int Wisdom;
        public int Strength;

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Life = reader.ReadInt16();
            MagicPoints = reader.ReadInt16();
            Power = reader.ReadInt16();
            Agility = reader.ReadInt16();
            Wisdom = reader.ReadInt16();
            Strength = reader.ReadInt16();
        }
    }
}
