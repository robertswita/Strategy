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
        TCell cell = new TCell();
        public CellPropsDlg()
        {
            InitializeComponent();
        }

        Bitmap GetWallImage(TWall wall)
        {
            if (wall == null || wall.Bounds.Width == 0) return null;
            var wallImage = new Bitmap(wall.Bounds.Width, wall.Bounds.Height);
            var gc = Graphics.FromImage(wallImage);
            var bounds = new Rectangle();
            for (var n = 0; n < wall.Tiles.Count; n++)
            {
                var tile = wall.Tiles[n];
                var tileBounds = tile.Bounds;
                tileBounds.Offset(tile.X, tile.Y);
                if (n == 0)
                    bounds = tileBounds;
                else
                    bounds = Rectangle.Union(bounds, tileBounds);
            }
            for (var n = 0; n < wall.Tiles.Count; n++)
            {
                var tile = wall.Tiles[n];
                gc.DrawImage(tile.Image, tile.X - bounds.X, tile.Y - bounds.Y);
            }
            return wallImage;
        }
        private void CellPropsDlg_Load(object sender, EventArgs e)
        {
            FloorBox.Maximum = Cell.Map.Floors.Count - 1;
            WallBox.Maximum = Cell.Map.Walls.Count - 1;
            RoofBox.Maximum = Cell.Map.Walls.Count - 1;
            WallLayerBox.Maximum = Cell.Walls.Count - 1;
            cell.Floor = Cell.Floor;
            foreach (var wall in Cell.Walls)
                cell.Walls.Add(wall);
            cell.Roof = Cell.Roof;
            cell.EventIdx = Cell.EventIdx;
            if (Cell.Floor != null)
            {
                FloorView.Image = Cell.Floor.Image;
                FloorBox.Value = Cell.Floor.Index;
            }
            if (Cell.Walls.Count > 0)
            {
                WallLayerBox.Value = 0;
                var wall = (TWall)Cell.Walls[0];
                WallBox.Value = wall.Index;
            }
            if (Cell.Roof != null)
                RoofBox.Value = Cell.Roof.Index;
            EventBox.Text = Cell.EventIdx.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int.TryParse(EventBox.Text, out Cell.EventIdx);
            Cell.Floor = cell.Floor;
            for (var i = 0; i < cell.Walls.Count; i++)
            {
                if (i >= Cell.Walls.Count)
                {
                    var wall = new TWall();
                    wall.X = Cell.X;
                    wall.Y = Cell.Y;
                    Cell.Walls.Add(wall);
                    Cell.Map.Walls.Add(wall);
                }
                Cell.Walls[i].Tiles = cell.Walls[i].Tiles;
            }
            Cell.Roof = cell.Roof;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (FloorBox.Value < 0)
            {
                cell.Floor = null;
                FloorView.Image = null;
            }
            else
            {
                cell.Floor = Cell.Map.Floors[(int)FloorBox.Value];
                FloorView.Image = cell.Floor.Image;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FloorBox.Value = -1;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            RoofBox.Value = -1;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            WallBox.Value = -1;
        }

        private void RoofBox_ValueChanged(object sender, EventArgs e)
        {
            if (RoofBox.Value < 0)
            {
                cell.Roof = null;
                RoofView.Image = null;
            }
            else
            {
                cell.Roof = Cell.Map.Walls[(int)RoofBox.Value];
                RoofView.Image = cell.Roof.Image;
            }
        }

        private void WallBox_ValueChanged(object sender, EventArgs e)
        {
            TWall wall = null;
            if (WallBox.Value < 0)
                WallView.Image = null;
            else
            {
                wall = Cell.Map.Walls[(int)WallBox.Value];
                WallView.Image = GetWallImage(wall);
            }
            cell.Walls[(int)WallLayerBox.Value] = wall;
        }

        private void WallLayerBox_ValueChanged(object sender, EventArgs e)
        {
            WallBox.Enabled = WallLayerBox.Value >= 0;
            if (!WallBox.Enabled)
            {
                WallBox.Value = -1;
            }
        }
    }
}
