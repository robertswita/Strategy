using Strategy.Dispel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TAnimation: TCollectItem
    {
        public List<TFrame[][]> Sequences = new List<TFrame[][]>();
        //public List<TAnimation> Source;
        //public int Index;
        public string Name;
        public TMap CreatePreviewMap(string filename)
        {
            var previewMap = new TMap();
            var s = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(s);
            Read(reader);
            previewMap.Animations.Add(this);
            var posX = 0;
            var posY = 0;
            for (var j = 0; j < Sequences.Count; j++)
            {
                var sequence = Sequences[j];
                posX = 0;
                for (var k = 0; k < sequence.Length; k++)
                {
                    var sprite = new TSprite();
                    sprite.Animation = this;
                    sprite.Sequence = j;
                    sprite.ViewAngle = k;
                    var width = sprite.Frames[0].Bounds.Width;
                    var height = sprite.Frames[0].Bounds.Height;
                    sprite.X = posX;
                    sprite.Y = posY;
                    posX += 2 * width;
                    sprite.Bounds = new Rectangle(sprite.X, sprite.Y, width, height);
                    previewMap.Sprites.Add(sprite);
                }
                posY += 2 * sequence[0][0].Bounds.Height;
            }
            previewMap.Width = 2 * posX / TTile.Width + 4;
            previewMap.Height = 2 * posY / TTile.Height + 2;
            previewMap.Cells = new TCell[previewMap.Height, previewMap.Width];
            reader.Close();
            return previewMap;
        }

    }
}
