using Common;
using Strategy.Dispel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Strategy.Diablo
{
    class TDiabloTile : TTile
    {
        public static int Width = 32;
        public static int Height = 16;
        public bool HasRleFormat;
        //public int Size;
        public byte[] EncodedPixels;
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
            if (HasRleFormat)
            {
                writer.Write((short)0);
                EncodedPixels = EncodeRle(Image);
            }
            else
            {
                writer.Write((short)1);
                EncodedPixels = EncodeIsometric(Image);
            }
            writer.Write(EncodedPixels.Length);
            writer.Write((short)0);
            //writer.Write(writer.BaseStream.Position + 4 - headerFilePos);
        }

        static TDiabloTile()
        {
            Width = 32;
            Height = 16;
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
                    pixmap[x, y] = TDiabloMap.Palette[pixels[pos]]; pos++;
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
                    pixels[pos] = (byte)GetNearestColor(pixmap[x, y]); pos++;
                }
            }
            return pixels;
        }
        Bitmap DecodeRle(byte[] pixels)
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
                        pixmap[x, y] = TDiabloMap.Palette[pixels[pos]]; pos++;
                    }
                } while (segEnd > segBegin);
            }
            return pixmap.Image;
        }
        byte[] EncodeRle(Bitmap bmp)
        {
            //int pos = 0;
            //var pixmap = new TPixmap(TCell.Width, TCell.Width);
            //pixmap.Image = bmp;
            //for (int y = 0; y < pixmap.Height; y++)
            //{
            //    if (pos >= pixels.Length) break;
            //    var segBegin = 0;
            //    var segEnd = 0;
            //    do
            //    {
            //        segBegin = segEnd;
            //        segBegin += pixels[pos]; pos++;
            //        segEnd = segBegin;
            //        segEnd += pixels[pos]; pos++;
            //        for (int x = segBegin; x < segEnd; x++)
            //        {
            //            pixmap[x, y] = TDiabloMap.Palette[pixels[pos]]; pos++;
            //        }
            //    } while (segEnd > segBegin);
            //}
            return null;
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

        public static int GetNearestColor(int color)
        {
            var palIdx = 0;
            var minDist = double.MaxValue;
            for (int i = 0; i < TDiabloMap.Palette.Length; i++)
            {
                var palColor = TDiabloMap.Palette[i];
                var dist = Math.Abs((palColor >> 16 & 0xFF) - (color >> 16 & 0xFF));
                dist += Math.Abs((palColor >> 8 & 0xFF) - (color >> 8 & 0xFF));
                dist += Math.Abs((palColor & 0xFF) - (color & 0xFF));
                if (dist < minDist)
                {
                    minDist = dist;
                    palIdx = i;
                }
            }
            return palIdx;
        }

    }
}
