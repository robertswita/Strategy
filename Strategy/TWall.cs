using System.Collections.Generic;
using System.Drawing;

namespace Strategy
{
    public class TWall : TSprite
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
            //gc.DrawRectangle(Pens.Red, Bounds);
        }

        public bool IsEqual(TWall other)
        {
            if (Tiles.Count != other.Tiles.Count)
                return false;
            for (var n = 0; n < Tiles.Count; n++)
                if (!Tiles[n].IsEqual(other.Tiles[n]))
                    return false;
            return true;
        }

    }
}
