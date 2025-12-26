using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Strategy
{
    public partial class CellPropsDlg : Form
    {
        public TCell Cell;
        public CellPropsDlg()
        {
            InitializeComponent();
        }

        Bitmap GetWallImage(TWall wall)
        {
            if (wall == null) return null;
            var wallImage = new Bitmap(wall.Bounds.Width, wall.Bounds.Height);
            var gc = Graphics.FromImage(wallImage);
            //wall.Draw(gc);
            for (var n = 0; n < wall.Tiles.Count; n++)
            {
                var tile = wall.Tiles[n];
                //gc.DrawImage(Images[cell.Piece.ImageIndex], X, Y + n * TCell.Height);
                gc.DrawImage(tile.Image, tile.X, tile.Y);
            }
            return wallImage;
        }
        private void CellPropsDlg_Load(object sender, EventArgs e)
        {
            if (Cell.Floor != null)
            {
                FloorBtn.Image = Cell.Floor.Image;
                if (Cell.Wall != null)
                {
                    var wall = (TWall)Cell.Wall.Tiles[0];
                    WallBtn.Image = GetWallImage(wall);
                }
                RoofBtn.Image = GetWallImage((TWall)Cell.Roof);
                EventBox.Text = Cell.EventIdx.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int.TryParse(EventBox.Text, out Cell.EventIdx);
        }
    }
}
