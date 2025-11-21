using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Strategy.Dispel
{
    class TDispelMonster: TMonster
    {
        public TDispelMap Map { get { return (TDispelMap)Collect.Owner; } }
        public override void Read(BinaryReader reader)
        {
            //var monster = new TMonster();
            Order = X;
            Id = reader.ReadInt32();
            var modelIdx = reader.ReadInt32();
            Animation = Map.MonsterAnims[modelIdx];
            ViewAngle = Animation.Sequences[0].Length - 1;
            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            var pos = Map.World2ViewTransform(x, y);
            X = (int)pos.X;// + TCell.Width;
            Y = (int)pos.Y;// + TCell.Height;
            var unk = reader.ReadBytes(5 * sizeof(int));
            //reader.ReadInt32();
            //reader.ReadInt32();
            //reader.ReadInt32();
            //reader.ReadInt32();
            //reader.ReadInt32();
            LootSlot1Id = reader.ReadByte();
            LootSlot1Type = reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            LootSlot2Id = reader.ReadByte();
            LootSlot2Type = reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            LootSlot3Id = reader.ReadByte();
            LootSlot3Type = reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadInt32();
            reader.ReadInt32();

            Bounds = new Rectangle(X, Y, Frames[0].Bounds.Width, Frames[0].Bounds.Height);
            Bounds.Offset(-ActFrame.Offset.X, -ActFrame.Offset.Y);
            Map.Sprites.Add(this);

        }
    }
}
