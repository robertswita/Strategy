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
                    pixmap[x, y] = TPalette.Rgb16To32(pixels[pos++], pixels[pos++]);
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
                pixmap[pixmap.Width / 2 - r, y] = TPalette.Rgb16To32(0, 1);
                pixmap[pixmap.Width / 2 + r - 1, y] = TPalette.Rgb16To32(0, 1);
            }
            contour.Image = pixmap.Image;
            return contour;
        }

    }

}
