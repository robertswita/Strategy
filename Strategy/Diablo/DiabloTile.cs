using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy.Diablo
{
    class DiabloTile : TTile
    {
        public bool HasRleFormat;
        public int Size;
        public void ReadHeader(BinaryReader reader)
        {
            X = reader.ReadInt16();
            Y = reader.ReadInt16();
            var zeros = reader.ReadInt16();
            var gridX = reader.ReadByte();
            var gridY = reader.ReadByte();
            HasRleFormat = reader.ReadInt16() != 1;
            Size = reader.ReadInt32();
            zeros = reader.ReadInt16();
            var headerSize = reader.ReadInt32();
        }
        public override void ReadImage(BinaryReader reader)
        {
            var pixels = reader.ReadBytes(Size);
            Image = HasRleFormat ? DecodeRle(pixels) : DecodeIsometric(pixels);
        }
        Bitmap DecodeIsometric(byte[] pixels)
        {
            int pos = 0;
            var pixmap = new TPixmap(TCell.Width, TCell.Height - 1);
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
        Bitmap DecodeRle(byte[] pixels)
        {
            int pos = 0;
            var pixmap = new TPixmap(TCell.Width, TCell.Width);
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
    }

}
