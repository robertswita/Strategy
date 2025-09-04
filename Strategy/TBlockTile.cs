using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TBlockTile : TSprite
    {
        //public List<TCell> Cells = new List<TCell>();
        public List<TTile> Tiles = new List<TTile>();

        public override void Draw(Graphics gc)
        {
            for (var n = 0; n < Tiles.Count; n++)
            {
                var tile = Tiles[n];
                //gc.DrawImage(Images[cell.Piece.ImageIndex], X, Y + n * TCell.Height);
                gc.DrawImage(tile.Image, X + tile.X, Y + tile.Y);
                //if (CellProps != null && CellProps[Cells.Count + n] == 0)
                //{
                //    gc.DrawRectangle(Pens.Magenta, cell.X, cell.Y, TGame.TileWidth, TGame.TileHeight);
                //}
            }
        }

    }
}
