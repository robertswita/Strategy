using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy.Dispel
{
    class DispelTile : TTile
    {
        public override void ReadImage(BinaryReader reader)
        {
            var pixels = reader.ReadBytes(TCell.Width * TCell.Height);
            Image = DecodeIsometric(pixels);
        }

        public Bitmap DecodeIsometric(byte[] pixels)
        {
            int pos = 0;
            var pixmap = new TPixmap(TCell.Width, TCell.Height);
            for (int y = 0; y < pixmap.Height; y++)
            {
                var n = y < pixmap.Height / 2 ? y : pixmap.Height - 1 - y;
                var r = 1 + 2 * n;
                for (int x = pixmap.Width / 2 - r; x < pixmap.Width / 2 + r; x++)
                {
                    pixmap[x, y] = TDispelMap.Rgb16To32(pixels[pos++], pixels[pos++]);
                }
            }
            return pixmap.Image;
        }

    }

}
