using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TTile: TPiece
    {
        public static int Width = 64;
        public static int Height = 64;
        public virtual void ReadImage(BinaryReader reader) { }
        public virtual void WriteImage(BinaryWriter writer) { }

        public virtual bool IsEqual(TTile other)
        {
            var pixmap = new TPixmap(Image.Width, Image.Height);
            pixmap.Image = Image;
            var otherPixmap = new TPixmap(other.Image.Width, other.Image.Height);
            otherPixmap.Image = other.Image;
            for (int i = 0; i < pixmap.Pixels.Length; i++)
                if (pixmap.Pixels[i] != otherPixmap.Pixels[i])
                    return false;
            return true;
        }
    }
}
