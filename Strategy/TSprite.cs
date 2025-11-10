using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Strategy
{
    public class TSprite: TCollectItem, IComparable<TSprite>
    {
        public int Order;
        public int X, Y;
        public int Id;
        //public int Index;
        public bool Flipped;

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
                    if (ViewAngle >= Animation.Sequences[sequence].Length) ViewAngle = 0;
                    ActFrameIdx = 0;
                }
            }
        }
        int viewAngleUnflipped;
        public int ViewAngle
        {
            get { return Flipped ? 8 - viewAngleUnflipped : viewAngleUnflipped; }
            set {
                viewAngleUnflipped = value;
                Flipped = value >= Animation.Sequences[Sequence].Length;
                //Flipped = viewAngleUnflipped > 4;
                //if (Flipped) 
                //    viewAngleUnflipped = 8 - viewAngleUnflipped; 
            }
        }
        public TFrame[] Frames { get { return Animation.Sequences[Sequence][ViewAngle]; } }
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
            gc.DrawImage(ActFrame.Image, X, Y);
        }

        int IComparable<TSprite>.CompareTo(TSprite other)
        {
            var y1 = Bounds.Bottom;
            var y2 = other.Bounds.Bottom;
            return y1 == y2 ? Order.CompareTo(other.Order) : y1.CompareTo(y2);
        }

        public virtual Rectangle Bounds { get; set; }

        public virtual bool IsVisibleInRect(Rectangle rect)
        {
            rect.Intersect(Bounds);
            return rect.Width != 0 && rect.Height != 0;
        }

        public bool HasAPoint(Point p)
        {
            var bounds = Bounds;
            if (Animation != null && bounds.Contains(p.X, p.Y))
            {
                var pixel = ActFrame.Image.GetPixel(p.X - bounds.X, p.Y - bounds.Y);
                if (pixel.A > 0)
                    return true;
            }
            return false;
        }
    }
}
