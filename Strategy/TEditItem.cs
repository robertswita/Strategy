using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public enum TBonusType { Normal, Fire, StealMagic };
    public class TEditItem: THealItem
    {
        public int Evasion;
        public int Precision;
        public int Attack;
        public int Defence;
        public int Magic;
        public int Immunity;
        public int Extra;//Solvent
        public TBonusType BonusType;

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            Evasion = reader.ReadInt16();
            Precision = reader.ReadInt16();
            Attack = reader.ReadInt16();
            Defence = reader.ReadInt16();
            Magic = reader.ReadInt16();
            Immunity = reader.ReadInt16();
            Extra = reader.ReadInt16();
            BonusType = (TBonusType)reader.ReadInt16();
        }

    }
}
