using Common;
using Strategy.Dispel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Strategy.Diablo
{
    class TDiabloTile : TTile
    {
        public static int Width = 32;
        public static int Height = 16;
        public bool HasRleFormat;
        public byte[] EncodedPixels;
        public bool IsEmpty;
        public void ReadHeader(BinaryReader reader)
        {
            X = reader.ReadInt16();
            Y = reader.ReadInt16();
            var zeros = reader.ReadInt16();
            var gridX = reader.ReadByte();
            var gridY = reader.ReadByte();
            HasRleFormat = reader.ReadInt16() != 1;
            EncodedPixels = new byte[reader.ReadInt32()];
            zeros = reader.ReadInt16();
        }

        public void WriteHeader(BinaryWriter writer)
        {
            var headerFilePos = writer.BaseStream.Position;
            writer.Write((short)X);
            writer.Write((short)Y);
            writer.Write((short)0);
            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write(HasRleFormat ? (short)0 : (short)1);
            Encode();
            writer.Write(EncodedPixels.Length);
            writer.Write((short)0);
        }

        public override void ReadImage(BinaryReader reader)
        {
            EncodedPixels = reader.ReadBytes(EncodedPixels.Length);
            Image = HasRleFormat ? DecodeRle(EncodedPixels) : DecodeIsometric(EncodedPixels);
            EncodedPixels = null;
        }
        public override void WriteImage(BinaryWriter writer)
        {
            writer.Write(EncodedPixels);
            EncodedPixels = null;
        }
        public void Encode()
        {
            if (EncodedPixels == null) 
                EncodedPixels = HasRleFormat ? EncodeRle(Image) : EncodeIsometric(Image);
        }
        Bitmap DecodeIsometric(byte[] pixels)
        {
            int pos = 0;
            var pixmap = new TPixmap(Width, Height - 1);
            for (int y = 0; y < pixmap.Height; y++)
            {
                var n = y < pixmap.Height / 2 ? y : pixmap.Height - 1 - y;
                var r = 2 + 2 * n;
                for (int x = pixmap.Width / 2 - r; x < pixmap.Width / 2 + r; x++)
                {
                    pixmap[x, y] = TDiabloMap.Palette.Colors[pixels[pos]]; pos++;
                }
            }
            return pixmap.Image;
        }

        byte[] EncodeIsometric(Bitmap bmp)
        {
            int pos = 0;
            var pixels = new byte[Height * Width / 2];
            var pixmap = new TPixmap(Width, Height - 1);
            pixmap.Image = bmp;
            for (int y = 0; y < pixmap.Height; y++)
            {
                var n = y < pixmap.Height / 2 ? y : pixmap.Height - 1 - y;
                var r = 2 + 2 * n;
                for (int x = pixmap.Width / 2 - r; x < pixmap.Width / 2 + r; x++)
                {
                    var color = pixmap[x, y];
                    IsEmpty &= color == 0;
                    pixels[pos] = TDiabloMap.Palette.GetNearestColor(color);
                    pos++;
                }
            }
            return pixels;
        }
        public Bitmap DecodeRle(byte[] pixels)
        {
            int pos = 0;
            var pixmap = new TPixmap(Width, 2 * Height);
            for (int y = 0; y < pixmap.Height; y++)
            {
                if (pos >= pixels.Length) break;
                var segBegin = 0;
                var segEnd = 0;
                do
                {
                    segBegin = segEnd;
                    segBegin += pixels[pos]; pos++;
                    segEnd = segBegin;
                    segEnd += pixels[pos]; pos++;
                    for (int x = segBegin; x < segEnd; x++)
                    {
                        var pixel = TDiabloMap.Palette.Colors[pixels[pos]];
                        pixmap[x, y] = pixel; pos++;
                    }
                } while (segEnd > segBegin);
            }
            return pixmap.Image;
        }
        public byte[] EncodeRle(Bitmap bmp)
        {
            var pixels = new List<byte>();
            var pixmap = new TPixmap(Width, 2 * Height);
            pixmap.Image = bmp;
            for (int y = 0; y < pixmap.Height; y++)
            {
                var segBegin = 0;
                var segEnd = 0;
                do
                {
                    segBegin = segEnd;
                    while (segEnd < pixmap.Width && pixmap[segEnd, y] == 0) segEnd++;
                    if (segEnd == pixmap.Width) segBegin = segEnd; // for Siramy's sake only?
                    pixels.Add((byte)(segEnd - segBegin));
                    segBegin = segEnd;
                    while (segEnd < pixmap.Width && pixmap[segEnd, y] != 0) segEnd++;
                    pixels.Add((byte)(segEnd - segBegin));
                    for (int x = segBegin; x < segEnd; x++)
                    {
                        var color = pixmap[x, y];
                        IsEmpty &= color == 0;
                        pixels.Add(TDiabloMap.Palette.GetNearestColor(color));
                    }
                } while (segEnd > segBegin);
            }
            return pixels.ToArray();
        }

        public static TDiabloTile GetContourTile()
        {
            var contour = new TDiabloTile();
            var pixmap = new TPixmap(Width, Height - 1);
            for (int y = 0; y < pixmap.Height; y++)
            {
                var n = y < pixmap.Height / 2 ? y : pixmap.Height - 1 - y;
                var r = 2 + 2 * n;
                pixmap[pixmap.Width / 2 - r, y] = unchecked((int)0xFF000000);
                pixmap[pixmap.Width / 2 + r - 1, y] = unchecked((int)0xFF000000);
            }
            contour.Image = pixmap.Image;
            return contour;
        }

    }
}
