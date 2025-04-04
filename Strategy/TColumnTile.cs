using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TColumnTile : TSprite
    {
        public List<TCell> Cells = new List<TCell>();
        public List<Image> Images;
        //public int X_;
        //public int Y_;

        public override void Draw(Graphics gc)
        {
            for (var n = 0; n < Cells.Count; n++)
            {
                var cell = Cells[n];
                gc.DrawImage(Images[cell.Piece.ImageIndex], X, Y + n * TCell.Height);
                //if (CellProps != null && CellProps[Cells.Count + n] == 0)
                //{
                //    gc.DrawRectangle(Pens.Magenta, cell.X, cell.Y, TGame.TileWidth, TGame.TileHeight);
                //}
            }
        }
    }
}
