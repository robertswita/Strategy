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
            //var pal = TPalette.CreateRGB332();
            //var pen = new Pen(Color.FromArgb(pal[Index % 256]));
            for (var n = 0; n < Tiles.Count; n++)
            {
                var tile = Tiles[n];
                //gc.DrawImage(Images[cell.Piece.ImageIndex], X, Y + n * TCell.Height);
                gc.DrawImage(tile.Image, X + tile.X, Y + tile.Y);
                //gc.DrawRectangle(pen, new Rectangle(X + tile.X, Y + tile.Y, 32, 32));
                //if (CellProps != null && CellProps[Cells.Count + n] == 0)
                //{
                //    gc.DrawRectangle(Pens.Magenta, cell.X, cell.Y, TGame.TileWidth, TGame.TileHeight);
                //}
            }
            //gc.DrawRectangle(Pens.Red, Bounds);
        }

        public bool IsEqual(TWall other)
        {
            //if (Tiles.Count != other.Tiles.Count)
            //    return false;
            if (Bounds.Width != other.Bounds.Width || Bounds.Height != other.Bounds.Height) 
                return false;
            for (var n = 0; n < Tiles.Count; n++)
                if (!Tiles[n].IsEqual(other.Tiles[n]))
                    return false;
            return true;
        }

    }
}
