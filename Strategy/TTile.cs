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
    public class TTile: TCollectItem
    {
        public int X, Y;
        public Bitmap Image;
        public static int Width = 64;
        public static int Height = 64;
        static Rectangle bounds = new Rectangle(0, 0, Width, Height);
        //public virtual int Width { get { return 64; } }
        //public virtual int Height { get { return 64; } }
        public virtual Rectangle Bounds { get => bounds; }
        public virtual void ReadImage(BinaryReader reader) { }
        public virtual void WriteImage(BinaryWriter writer) { }

        public virtual bool IsEqual(TTile other)
        {
            if (Image.Width != other.Image.Width || Image.Height != other.Image.Height) 
                return false;
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
