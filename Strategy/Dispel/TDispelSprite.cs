using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Strategy.Dispel
{
    class TDispelSprite: TSprite
    {
        public TDispelMap Map { get { return (TDispelMap)Collect.Owner; } }
        public override void Read(BinaryReader reader)
        {
            var modelIdx = reader.ReadInt32();
            Animation = Map.Animations[modelIdx];
            for (int j = 0; j < Frames.Length; j++)
            {
                var frame = Frames[j];
                int left = reader.ReadInt32();
                int top = reader.ReadInt32();
                int right = reader.ReadInt32();
                int bottom = reader.ReadInt32();
                X = reader.ReadInt32();
                Y = reader.ReadInt32();
                var frameBounds = Rectangle.FromLTRB(X, Y, right, bottom);
                Bounds = j == 0 ? frameBounds : Rectangle.Union(Bounds, frameBounds);
            }
            Map.Sprites.Add(this);
        }
        public override void Write(BinaryWriter writer)
        {
            writer.Write(Animation.Index);
            foreach (var frame in Frames)
            {
                writer.Write(Bounds.Left);
                writer.Write(Bounds.Top);
                writer.Write(Bounds.Right);
                writer.Write(Bounds.Bottom);
                writer.Write(X);
                writer.Write(Y);
            }
        }


    }
}
