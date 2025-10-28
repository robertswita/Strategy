using Strategy.Diablo;
using Strategy.Dispel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Strategy
{
    public partial class StrategyForm : Form
    {
        TGame Game;
        public float ScrollStep {  get { return 0.01f / Board.Zoom; } }
        [DllImport("winmm.dll")] public static extern uint timeBeginPeriod(uint uMilliseconds);
        [DllImport("winmm.dll")] public static extern uint timeEndPeriod(uint uMilliseconds);

        public StrategyForm()
        {
            InitializeComponent();
            //timeBeginPeriod(1);
            Game = new TGame();
            Board.Game = Game;
            Game.Map.ReadMap(Application.StartupPath);
            //var map = new TMap();
            //map.Game = Game;
            Game.MapView = new Bitmap(MapView.Width, MapView.Height);
            Board.MouseWheel += TBoard1_MouseWheel;
            Board.Scroll += BoardScroll;
            Game.Restart();
        }

        private void BoardScroll(object sender, ScrollEventArgs e)
        {
            MapView.Invalidate();
        }

        private void MapView_Paint(object sender, PaintEventArgs e)
        {
            var mapSize = Game.Map.Map2ViewTransform(Game.Map.Width, Game.Map.Height);
            e.Graphics.DrawImage(Game.MapView, 0, 0);
            e.Graphics.ScaleTransform(MapView.Width / mapSize.X, MapView.Height / mapSize.Y);
            e.Graphics.DrawRectangle(Pens.Red, Rectangle.Round(Board.Viewport));
            MapNameLbl.Text = Game.Map.MapName;
        }

        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (Cursor.Position.X < Margin.Left)
                Board.ScrollPos -= new SizeF(ScrollStep, 0);
            else if (Cursor.Position.X > Width - Margin.Right)
                Board.ScrollPos += new SizeF(ScrollStep, 0);
            if (Cursor.Position.Y < Margin.Top)
                Board.ScrollPos -= new SizeF(0, ScrollStep);
            else if (Cursor.Position.Y > Height - Margin.Bottom)
                Board.ScrollPos += new SizeF(0, ScrollStep);
            foreach (var sprite in Game.Map.Sprites)
            {
                var actFrame = sprite.ActFrame;
                sprite.NextFrame();
                if (actFrame != sprite.ActFrame)
                    Board.Invalidate();
            }
            Game.ActiveScript?.Run();
        }

        private void MapView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var p = new PointF(e.X, e.Y);
                p.X /= MapView.Width;
                p.Y /= MapView.Height;
                Board.ScrollPos = p;
            }
        }

        private void TBoard1_MouseWheel(object sender, MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta < 0)
                Board.Zoom *= 0.9f;
            else
                Board.Zoom *= 1.1f;
            var timerInterval = (int)(100 / Board.Zoom);
            var minTime = 1000 / TGame.FPS;
            if (timerInterval < minTime) timerInterval = minTime;
            PlayTimer.Interval = timerInterval;
            Board.Invalidate();
            MapView.Invalidate();
        }

        private void HerosList_SelectedIndexChanged(object sender, EventArgs e)
        {
            Board.Game.ActivePlayer.SelectedHero = Board.Game.ActivePlayer.Heroes[HerosList.SelectedIndex];
            var hero = Board.Game.ActivePlayer.SelectedHero;
            var p = new PointF(hero.Cell.X, hero.Cell.Y);
            p.X /= Board.Game.Map.Width;
            p.Y /= Board.Game.Map.Height;
            Board.ScrollPos = p;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Board.Game.NextTurn();
            HerosList.DataSource = Board.Game.ActivePlayer.Heroes;
            HerosList.DisplayMember = "Name";
            HerosList.BackColor = Board.Game.ActivePlayer.ID;
        }

        private void ResourceChanged(object sender, EventArgs e)
        {
            var activePlayer = Board.Game.ActivePlayer;
            var names = Enum.GetNames(typeof(TResource.ResType));
            ResourceView.Items.Clear();
            for (int i = 0; i < activePlayer.Resources.Length; i++)
            {
                var item = new ListViewItem(names[i], i);
                item.SubItems.Add(activePlayer.Resources[i].ToString());
                ResourceView.Items.Add(item);
            }
        }

        private void StrategyForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.T: ImportMap(); break;
            }
        }

        void ImportMap()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Game = new TGame();
                Game.MapView = new Bitmap(MapView.Width, MapView.Height);
                Game.Players.Add(new TPlayer());
                Game.ActivePlayer = Game.Players[0];
                Board.Game = Game;
                Board.Game.ImportMap(openFileDialog1.FileName);
                if (Path.GetExtension(openFileDialog1.FileName).ToLower() == ".bmp")
                {
                    Game.OnResourceChanged = ResourceChanged;
                    HerosList.DataSource = Game.ActivePlayer.Heroes;
                    HerosList.DisplayMember = "Name";
                    HerosList.BackColor = Game.ActivePlayer.ID;
                }
                Board.Invalidate();
                MapView.Invalidate();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ImportMap();
        }

        private void panel1_BackColorChanged(object sender, EventArgs e)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FilterIndex == 1)
                    ConvertToDiablo(saveFileDialog1.FileName);
                else
                    Game.Map.WriteMap(saveFileDialog1.FileName);
            }

        }

        private void StrategyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //timeEndPeriod(1);
        }

        void ConvertToDiablo(string filename)
        {
            if (Game.Map is TDispelMap)
            {
                var diabloGame = new TGame();
                var diabloMap = new TDiabloMap();
                diabloMap.Game = diabloGame;
                diabloMap.ReadPalette(filename);
                //diabloMap.Width = Game.Map.Width * 2 / 5;
                //diabloMap.Height = Game.Map.Height * 2 / 5;
                //for (int i = 0; i < Game.GroundTiles.Count; i++)
                //{
                //    var dispelTile = Game.GroundTiles[i];
                //    var tileBmp = dispelTile.Image;
                //    var diabloBmp = new Bitmap(tileBmp.Width - 2, tileBmp.Height - 1);
                //    var gc = Graphics.FromImage(diabloBmp);
                //    var tileRect = new Rectangle(1, 0, tileBmp.Width - 2, tileBmp.Height);
                //    var diabloRect = new Rectangle(0, 0, diabloBmp.Width, diabloBmp.Height);
                //    gc.DrawImage(tileBmp, diabloRect, tileRect, GraphicsUnit.Pixel);
                //    diabloRect.Width = DiabloTile.Width;
                //    diabloRect.Height = DiabloTile.Height;
                //    var wall = new DiabloWall();
                //    wall.Width = 2 * DiabloTile.Width;
                //    wall.Height = 2 * DiabloTile.Height;
                //    diabloGame.Walls.Add(wall);
                //    wall.Type = TWallType.LowerWall;
                //    wall.Style = diabloGame.Walls.Count >> 6;
                //    wall.Seq = diabloGame.Walls.Count & 63;
                //    wall.X = dispelTile.X;
                //    wall.Y = dispelTile.Y;
                //    for (int y = 0; y < 2; y++)
                //        for (int x = 0; x < 2; x++)
                //        {
                //            tileBmp = new Bitmap(DiabloTile.Width, DiabloTile.Height);
                //            gc = Graphics.FromImage(tileBmp);
                //            //tileRect = new Rectangle(0, 0, tileBmp.Width, tileBmp.Height);
                //            diabloRect.X = (x + y) * DiabloTile.Width / 2;
                //            diabloRect.Y = (y - x + 1) * DiabloTile.Height / 2;
                //            gc.DrawImage(diabloBmp, 0, 0, diabloRect, GraphicsUnit.Pixel);
                //            var tile = new DiabloTile();
                //            tile.Image = tileBmp;
                //            tile.X = diabloRect.X;
                //            tile.Y = diabloRect.Y;
                //            wall.Tiles.Add(tile);
                //        }
                //}

                var mapViewSize = Game.Map.Map2ViewTransform(Game.Map.Width, Game.Map.Height);
                var dispelBmp = new Bitmap((int)mapViewSize.X, (int)mapViewSize.Y);
                var gc = Graphics.FromImage(dispelBmp);
                for (int y = 0; y < Game.Map.Height; y++)
                    for (int x = 0; x < Game.Map.Width; x++)
                    {
                        var cell = Game.Map.Cells[y, x];
                        var bounds = cell.Bounds;
                        if (cell.Floor != null)
                            gc.DrawImage(cell.Floor.Image, bounds.X, bounds.Y);
                    }
                var tileW = 5 * TDiabloTile.Width;
                var tileH = 5 * TDiabloTile.Height;
                var w = (dispelBmp.Width + tileW - 1) / tileW;
                var h = (dispelBmp.Height + tileH / 2 - 1) / (tileH / 2);
                var extraTilesCount = dispelBmp.Width % TDiabloTile.Width > TDiabloTile.Width / 2 ? 1 : 0;
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w + extraTilesCount * (y % 2); x++)
                    {
                        var pos = new Point(x * tileW, y * tileH / 2);
                        if (y % 2 == 0) pos.X += tileW / 2;
                    }

                diabloMap.WriteTileSet(filename, ".dt1");
                var blockTiles = diabloMap.Walls;
                diabloMap.Walls = new TCollect<TBlockTile>();
                diabloMap.Walls.ItemType = typeof(TDiabloWall);
                for (int i = 0; i < Game.Map.Walls.Count; i++)
                {
                    var dispelTile = Game.Map.Walls[i];
                    var diabloWallIdx = dispelTile.Order;
                    var diabloWall = blockTiles[diabloWallIdx];
                    diabloMap.Walls.Add(diabloWall);
                }
                diabloMap.WriteMap(filename);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
        }
    }
}
