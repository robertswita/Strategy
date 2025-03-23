using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TSprite: IComparable<TSprite>
    {
        public int Order;
        public int X, Y;
        public int Id;
        //public int ModelIdx;
        public TAnimation Animation;
        int sequence;
        public int Sequence
        {
            get { return sequence; }
            set { sequence = value; ActFrameIdx = 0; }
        }
        public int ViewAngle;
        public bool Flipped;
        //public int Width, Height;
        public Rectangle Bounds;
        public TFrame[] Frames { get { return Animation.Sequences[Sequence][ViewAngle]; } }
        public float ActFrameIdx;
        public TFrame ActFrame { get { return Frames[(int)ActFrameIdx]; } }
        public void NextFrame()
        {
            ActFrameIdx += Frames.Length * TGame.FPS / 1000f;
            if (ActFrameIdx >= Frames.Length)
                ActFrameIdx = 0;
        }

        public virtual void Draw(Graphics gc)
        {
            if (Flipped)
            {
                gc.ScaleTransform(-1, 1);
                gc.DrawImage(ActFrame.Image, -X - ActFrame.Offset.X, Y - ActFrame.Offset.Y);
                gc.ScaleTransform(-1, 1);
            }
            else
                gc.DrawImage(ActFrame.Image, X - ActFrame.Offset.X, Y - ActFrame.Offset.Y);
        }

        int IComparable<TSprite>.CompareTo(TSprite other)
        {
            var y1_ = Bounds.Bottom;// Y + Cells.Count * TGame.TileHeight;
            var y2_ = other.Bounds.Bottom;// other.Y + other.Cells.Count * TGame.TileHeight;
            return y1_ == y2_ ? Order.CompareTo(other.Order) : y1_.CompareTo(y2_);
        }

        public bool IsVisibleInRect(Rectangle rect)
        {
            rect.Intersect(Bounds);
            return rect.Width != 0 && rect.Height != 0;
        }
    }
}
