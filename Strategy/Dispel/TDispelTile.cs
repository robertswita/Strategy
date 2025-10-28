using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Strategy.Dispel
{
    class TDispelTile : TTile
    {
        public static int Width = 64;
        public static int Height = 32;
        public static int Rgb16To32(int byte0, int byte1)
        {
            var b = (byte0 & 0x1F) << 3;
            var g = (byte0 & 0xE0) >> 3 | (byte1 & 7) << 5;
            var r = byte1 & 0xF8;
            int color = b | b >> 5 | g << 8 | (g & 0xC0) << 2 | r << 16 | (r & 0xE0) << 11;
            if (color > 0) color |= unchecked((int)0xFF000000);
            return color;
        }
        public static byte[] Rgb32To16(int color)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)((color & 0xF8) >> 3 | (color & 0x1C00) >> 5);
            bytes[1] = (byte)((color & 0xF8) >> 16 | (color & 0xE000) >> 13);
            return bytes;
        }

        public override void ReadImage(BinaryReader reader)
        {
            var pixels = reader.ReadBytes(Width * Height);
            Image = DecodeIsometric(pixels);
        }

        public Bitmap DecodeIsometric(byte[] pixels)
        {
            int pos = 0;
            var pixmap = new TPixmap(Width, Height);
            for (int y = 0; y < pixmap.Height; y++)
            {
                var n = y < pixmap.Height / 2 ? y : pixmap.Height - 1 - y;
                var r = 1 + 2 * n;
                for (int x = pixmap.Width / 2 - r; x < pixmap.Width / 2 + r; x++)
                {
                    pixmap[x, y] = Rgb16To32(pixels[pos++], pixels[pos++]);
                }
            }
            return pixmap.Image;
        }

        public static TDispelTile GetContourTile()
        {
            var contour = new TDispelTile();
            var pixmap = new TPixmap(Width, Height);
            for (int y = 0; y < pixmap.Height; y++)
            {
                var n = y < pixmap.Height / 2 ? y : pixmap.Height - 1 - y;
                var r = 1 + 2 * n;
                pixmap[pixmap.Width / 2 - r, y] = Rgb16To32(0, 1);
                pixmap[pixmap.Width / 2 + r - 1, y] = Rgb16To32(0, 1);
            }
            contour.Image = pixmap.Image;
            return contour;
        }

    }

}
