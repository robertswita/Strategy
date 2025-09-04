using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TItem: TPiece
    {
        public string Name;
        public string Description;

        public virtual void Read(BinaryReader reader)
        {
            var encoding = TDispelMap.Encoding;
            var name = encoding.GetString(reader.ReadBytes(30));// 0xcd
            var zidx = name.IndexOf('\0');
            if (zidx >= 0) name = name.Substring(0, zidx);
            Name = name;
            var description = encoding.GetString(reader.ReadBytes(30));// 0xcd
            zidx = description.IndexOf('\0');
            if (zidx >= 0) description = description.Substring(0, zidx);
            Description = description;
            var bytes = reader.ReadBytes(172);
            var price = reader.ReadInt32();
            var unk = reader.ReadInt16();
            unk = reader.ReadInt16();
        }
    }
}
