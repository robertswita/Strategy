using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public enum TElementType { Chest, Type1, Door, Type3, Sign, Altar, Interactive, Magic }
    public class TElement: TSprite
    {
        public string Name;
        public TElementType Type;
        public int Closed;
        public int RequiredItem1Id;
        public int RequiredItem1Type;
        public int RequiredItem2Id;
        public int RequiredItem2Type;
        public int Gold;
        public int Item1Id;
        public int Item1Type;
        public int ItemCount;
        public int EventId;
        public int MessageId;
        public int Visibility;

        public override void Draw(Graphics gc)
        {
            var actFrame = ActFrame;
            gc.DrawImage(actFrame.Image, X - actFrame.Offset.X, Y - actFrame.Offset.Y);
        }

        public override Rectangle Bounds
        {
            get
            {
                var bounds = ActFrame.Bounds;
                bounds.X = X - ActFrame.Offset.X;
                bounds.Y = Y - ActFrame.Offset.Y;
                return bounds;
            }
        }
    }
}
