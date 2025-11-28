using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Strategy.Dispel
{
    class TDispelAnimation: TAnimation
    {
        Bitmap ReadImage(byte[] pixels, int width, int height)
        {
            var pos = 0;
            var tile = new TPixmap(width, height);
            for (int y = 0; y < tile.Height; y++)
                for (int x = 0; x < tile.Width; x++)
                    tile[x, y] = TPalette.Rgb16To32(pixels[pos++], pixels[pos++]);
            return tile.Image;
        }
        public override void Read(BinaryReader reader)
        {
            int sequencesCount = reader.ReadInt32();
            for (int i = 0; i < sequencesCount; i++)
            {
                var sequence = new List<TFrame[]>();
                reader.ReadBytes(264);
                var viewsCount = reader.ReadInt32();
                for (int j = 0; j < viewsCount; j++)
                {
                    var view = new List<TFrame>();
                    reader.ReadInt32();
                    var framesCount = (int)reader.ReadInt64();
                    for (int k = 0; k < framesCount; k++)
                    {
                        var frame = new TFrame();
                        var left = reader.ReadInt32();
                        var top = reader.ReadInt32();
                        var right = reader.ReadInt32();
                        var bottom = reader.ReadInt32();
                        frame.Bounds.X = reader.ReadInt32();
                        frame.Bounds.Y = reader.ReadInt32();
                        frame.Offset.X = reader.ReadInt32();
                        frame.Offset.Y = reader.ReadInt32();
                        frame.Bounds.Width = reader.ReadInt32();
                        frame.Bounds.Height = reader.ReadInt32();
                        var pixelCount = reader.ReadInt32();
                        if (pixelCount > 0)
                        {
                            var pixels = reader.ReadBytes(pixelCount * 2);
                            frame.Image = ReadImage(pixels, frame.Bounds.Width, frame.Bounds.Height);
                            view.Add(frame);
                        }
                    }
                    if (view.Count > 0)
                        sequence.Add(view.ToArray());
                }
                if (sequence.Count > 0)
                    Sequences.Add(sequence.ToArray());
            }
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(Sequences.Count);
            foreach (var sequence in Sequences)
            {
                writer.Write(new byte[264]);
                writer.Write(sequence.Length);
                foreach (var view in sequence)
                {
                    writer.Write(0);
                    writer.Write(view.Length);
                    writer.Write(0);
                    foreach (var frame in view)
                    {
                        writer.Write(frame.Bounds.Left);
                        writer.Write(frame.Bounds.Top);
                        writer.Write(frame.Bounds.Right);
                        writer.Write(frame.Bounds.Bottom);
                        writer.Write(frame.Bounds.Left);
                        writer.Write(frame.Bounds.Top);
                        writer.Write(frame.Offset.X);
                        writer.Write(frame.Offset.Y);
                        writer.Write(frame.Bounds.Width);
                        writer.Write(frame.Bounds.Height);
                        writer.Write(frame.Bounds.Width * frame.Bounds.Height);
                        var tile = new TPixmap(frame.Bounds.Width, frame.Bounds.Height);
                        tile.Image = frame.Image;
                        for (int y = 0; y < tile.Height; y++)
                            for (int x = 0; x < tile.Width; x++)
                                writer.Write(TPalette.Rgb32To16(tile[x, y]));
                    }
                }
            }
        }

    }
}
