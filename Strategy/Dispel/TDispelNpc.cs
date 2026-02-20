using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Strategy.Dispel
{
    class TDispelNpc : TNpc
    {
        static byte FILLER = 0xCD;
        static int STRING_MAX_LENGTH = 260;
        public override void Read(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            var modelIdx = reader.ReadInt32();
            Animation = (Map as TDispelMap).NpcAnims[modelIdx];
            var name = TDispelMap.Encoding.GetString(reader.ReadBytes(STRING_MAX_LENGTH));
            Name = name.Substring(0, name.IndexOf('\0'));
            name = TDispelMap.Encoding.GetString(reader.ReadBytes(STRING_MAX_LENGTH));
            Description = name.Substring(0, name.IndexOf('\0'));
            ScriptId = reader.ReadInt32();// party/scriptId
            OnShowEvent = reader.ReadInt32();
            var unk = reader.ReadInt32();
            var pathPts = new List<Vector2>();
            for (int j = 0; j < 4; j++)
                if (reader.ReadInt32() != 0) pathPts.Add(new Vector2());
            var pts = new Vector2[4];
            for (int j = 0; j < 4; j++)
                pts[j].X = reader.ReadInt32();
            for (int j = 0; j < 4; j++)
                pts[j].Y = reader.ReadInt32();
            for (int j = 0; j < pathPts.Count; j++)
                if (pts[j].X != 0 || pts[j].Y != 0)
                {
                    pathPts[j] = Map.World2MapTransform(pts[j].X, pts[j].Y + 1);
                }
            pathPts.Add(pathPts[0]);
            Cell = Map.Cells[0, 0];
            CalcPath(pathPts);
            DefaultPath = Path;
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            ViewAngle = reader.ReadInt32();// 0 = up, clockwise
            reader.ReadBytes(14 * sizeof(int));
            DialogId = reader.ReadInt32();// also text for shop
            reader.ReadInt32();
            Map.Sprites.Add(this);
        }
    }
}
