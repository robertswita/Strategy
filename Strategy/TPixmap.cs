using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Common
{
    public class TPixmap
    {
        public int[] Pixels;
        public int Width;
        public int Height;
        public int this[int x, int y]
        {
            get { return Pixels[y * Width + x]; }
            set { Pixels[y * Width + x] = value; }
        }
        public TPixmap(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new int[Width * Height];
        }

        public TPixmap HorzCat(TPixmap other)
        {
            var result = new TPixmap(Width + other.Width, Height);
            for (int row = 0; row < result.Height; row++)
            {
                Array.Copy(Pixels, row * Width, result.Pixels, row * result.Width, Width);
                Array.Copy(other.Pixels, row * other.Width, result.Pixels, row * result.Width + Width, other.Width);
            }
            return result;
        }

        public TPixmap VertCat(TPixmap other)
        {
            var result = new TPixmap(Width, Height + other.Height);
            Array.Copy(Pixels, result.Pixels, Pixels.Length);
            Array.Copy(other.Pixels, 0, result.Pixels, Pixels.Length, other.Pixels.Length);
            return result;
        }

        public void Copy(TPixmap src, Rectangle bounds, Point origin)
        {
            for (int row = 0; row < bounds.Height; row++)
                Array.Copy(src.Pixels, (bounds.Y + row) * src.Width + bounds.X, Pixels, (origin.Y + row) * Width + origin.X, bounds.Width);
        }

        public Bitmap Image
        {
            get
            {
                var bmp = new Bitmap(Width, Height);
                var rc = new Rectangle(0, 0, Width, Height);
                var data = bmp.LockBits(rc, ImageLockMode.WriteOnly, bmp.PixelFormat);
                Marshal.Copy(Pixels, 0, data.Scan0, Pixels.Length);
                bmp.UnlockBits(data);
                return bmp;
            }
            set
            {
                var bmp = value;
                var rc = new Rectangle(0, 0, Width, Height);
                var data = bmp.LockBits(rc, ImageLockMode.ReadOnly, bmp.PixelFormat);
                Marshal.Copy(data.Scan0, Pixels, 0, Pixels.Length);
                bmp.UnlockBits(data);
            }
        }

    }
}
