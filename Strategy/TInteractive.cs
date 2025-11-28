using System;
using System.Collections.Generic;
using System.Drawing;

namespace Strategy
{
    public enum TInteractiveType { Chest, Type1, Door, Type3, Sign, Altar, Interactive, Magic }
    public class TInteractive: TSprite
    {
        public string Name;
        public TInteractiveType Type;
        public int Closed;
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
