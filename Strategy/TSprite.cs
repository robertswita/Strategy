using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

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
            set {
                if (sequence != value)
                {
                    sequence = value;
                    ViewAngle++;
                    if (ViewAngle > 7) ViewAngle = 0;
                    ActFrameIdx = 0;
                }
            }
        }
        int viewAngleUnflipped;
        public int ViewAngle
        {
            get { return Flipped ? 8 - viewAngleUnflipped : viewAngleUnflipped; }
            set {
                Flipped = false;
                viewAngleUnflipped = value;
                if (viewAngleUnflipped > 4) 
                { 
                    Flipped = true; 
                    viewAngleUnflipped = 8 - viewAngleUnflipped; 
                } 
            }
        }
        bool Flipped;
        //public int Width, Height;
        public Rectangle Bounds;
        public TFrame[] Frames { get { return Animation.Sequences[Sequence][viewAngleUnflipped]; } }
        public float ActFrameIdx;
        public TFrame ActFrame { get { return Frames[(int)ActFrameIdx]; } }
        public virtual void NextFrame()
        {
            ActFrameIdx += Frames.Length * TGame.FPS / 1000f;
            if (ActFrameIdx >= Frames.Length)
                ActFrameIdx = 0;
        }

        public virtual void Draw(Graphics gc)
        {
            var actFrame = ActFrame;
            if (Flipped)
            {
                gc.ScaleTransform(-1, 1);
                gc.DrawImage(actFrame.Image, -X - actFrame.Offset.X, Y - actFrame.Offset.Y);
                gc.ScaleTransform(-1, 1);
            }
            else
                gc.DrawImage(actFrame.Image, X - actFrame.Offset.X, Y - actFrame.Offset.Y);
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
