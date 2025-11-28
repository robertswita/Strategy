using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Strategy.Dispel
{
    class TDispelInteractive: TInteractive
    {
        public int RequiredItem1Id;
        public int RequiredItem1Type;
        public int RequiredItem2Id;
        public int RequiredItem2Type;
        public int Gold;
        public int Item1Id;
        public int Item1Type;
        public TDispelMap Map { get { return (TDispelMap)Collect.Owner; } }
        public override void Read(BinaryReader reader)
        {
            Id = reader.ReadByte();
            var unk = reader.ReadByte();
            var modelIdx = reader.ReadByte();
            Animation = Map.InteractiveAnims[modelIdx];
            var name = TDispelMap.Encoding.GetString(reader.ReadBytes(32));// 0xcd
            var zidx = name.IndexOf('\0');
            if (zidx >= 0) name = name.Substring(0, zidx);
            Name = name;
            Type = (TInteractiveType)reader.ReadByte();
            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            ViewAngle = reader.ReadByte(); // rotation
            unk = reader.ReadByte();
            unk = reader.ReadByte();
            unk = reader.ReadByte();
            var unkInt = reader.ReadInt32();
            Closed = reader.ReadInt32();// "closed", AsInt32(), "chest 0-open, 1-closed");
            RequiredItem1Id = reader.ReadByte(); // lower bound
            RequiredItem1Type = reader.ReadByte();
            unk = reader.ReadByte();
            unk = reader.ReadByte();
            RequiredItem2Id = reader.ReadByte(); // upper bound
            RequiredItem2Type = reader.ReadByte();
            unk = reader.ReadByte();
            unk = reader.ReadByte();
            unkInt = reader.ReadInt32();
            unkInt = reader.ReadInt32();
            unkInt = reader.ReadInt32();
            unkInt = reader.ReadInt32();
            Gold = reader.ReadInt32();
            Item1Id = reader.ReadByte(); // lower bound
            Item1Type = reader.ReadByte();
            unk = reader.ReadByte();
            unk = reader.ReadByte();
            ItemCount = reader.ReadInt32();
            var bytes = reader.ReadBytes(10 * sizeof(int));
            EventId = reader.ReadInt32();
            MessageId = reader.ReadInt32(); // "id from message.scr for signs");
            unkInt = reader.ReadInt32();
            unkInt = reader.ReadInt32();
            bytes = reader.ReadBytes(32);
            Visibility = reader.ReadByte();
            bytes = reader.ReadBytes(3);
            var pos = Map.World2ViewTransform(x, y);
            //pos = Map2ViewTransform(pos.X, pos.Y);
            //var cell = Game.Cells[(int)pos.Y, (int)pos.X];
            //pos = cell.Position;
            //var pos = ViewTransform(x, y);
            X = (int)pos.X + TDispelTile.Width / 2;
            Y = (int)pos.Y + TDispelTile.Height / 2;
            //element.X -= element.ActFrame.Offset.X;
            //element.Y -= element.ActFrame.Offset.Y;
            Bounds = new Rectangle(X, Y, Frames[0].Bounds.Width, Frames[0].Bounds.Height);
            //element.Bounds.Offset(-element.ActFrame.Offset.X, -element.ActFrame.Offset.Y);
            Map.Sprites.Add(this);

        }
    }
}
