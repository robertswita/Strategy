using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Strategy
{
    public class TMonster: TSprite
    {
        public int LootSlot1Id;
        public int LootSlot1Type;
        public int LootSlot2Id;
        public int LootSlot2Type;
        public int LootSlot3Id;
        public int LootSlot3Type;
        public static float ShadowScale = 0.6f;
        public static float ShadowShear = 0.6f;
        public static ColorMatrix ShadowColorMatrix = new ColorMatrix();

        static TMonster()
        {
            ShadowColorMatrix.Matrix00 = 0;
            ShadowColorMatrix.Matrix11 = 0;
            ShadowColorMatrix.Matrix22 = 0;
            ShadowColorMatrix.Matrix33 = 0.6f;
        }

        public override void Draw(Graphics gc)
        {
            var actFrame = ActFrame;
            var pos = new Point(X - actFrame.Offset.X, Y - actFrame.Offset.Y);
            if (Flipped) { pos.X = -X - actFrame.Offset.X; gc.ScaleTransform(-1, 1); }
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(ShadowColorMatrix);
            var shadowHeight = (int)(actFrame.Bounds.Height * ShadowScale);
            var shadowShear = (int)(actFrame.Bounds.Height * ShadowShear);
            if (Flipped) shadowShear = -shadowShear;
            var bounds = new Rectangle(0, 0, actFrame.Bounds.Width, actFrame.Bounds.Height);
            var shadowPts = new Point[3];
            shadowPts[2] = pos; shadowPts[2].Y += bounds.Height;
            shadowPts[0] = shadowPts[2] + new Size(shadowShear, -shadowHeight);
            shadowPts[1] = shadowPts[0]; shadowPts[1].X += bounds.Width;
            //imageAttributes.SetThreshold(1);
            gc.DrawImage(actFrame.Image, shadowPts, bounds, GraphicsUnit.Pixel, imageAttributes);
            gc.DrawImage(actFrame.Image, pos.X, pos.Y);
            if (Flipped) gc.ScaleTransform(-1, 1);
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
