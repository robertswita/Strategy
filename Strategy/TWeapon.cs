using System;
using System.Collections.Generic;
using System.IO;

namespace Strategy
{
    public class TWeapon: TEditItem
    {
        public int RequiredPower;
        public int RequiredAgility;
        public int RequiredMagic;
        public int RequiredWisdom;

        public override void Read(BinaryReader reader)
        {
            base.Read(reader);
            RequiredPower = reader.ReadInt32();
            RequiredAgility = reader.ReadInt32();
            RequiredMagic = reader.ReadInt32();
            RequiredWisdom = reader.ReadInt32();
        }
    }
}
