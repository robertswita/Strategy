using Strategy.Diablo;
using Strategy.Dispel;
using Strategy.HMM;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace Strategy
{
    public partial class StrategyForm : Form
    {
        TGame Game;
        public float ScrollStep { get { return 0.01f / Board.Zoom; } }
        [DllImport("winmm.dll")] public static extern uint timeBeginPeriod(uint uMilliseconds);
        [DllImport("winmm.dll")] public static extern uint timeEndPeriod(uint uMilliseconds);

        public StrategyForm()
        {
            InitializeComponent();
            //timeBeginPeriod(1);
            Game = new TGame();
            Board.Game = Game;
            var map = new THmmMap();
            map.Game = Game;
            map.ReadMap(Application.StartupPath);
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
                case Keys.T: button2_Click(null, null); break;
            }
        }

        void ImportMap(string filename)
        {
            Game = new TGame();
            Game.MapView = new Bitmap(MapView.Width, MapView.Height);
            Game.Players.Add(new TPlayer());
            Game.ActivePlayer = Game.Players[0];
            Board.Game = Game;

            var ext = Path.GetExtension(filename).ToLower();
            if (ext == ".btl" || ext == ".gtl")
            {
                var dispelMap = new TDispelMap();
                //map.Game = Game;
                var tiles = dispelMap.ReadTileSet(filename, ext);
                //map.MapTileSet(map.Floors);
                var map = TMap.MapTileSet(tiles);
                map.Game = Game;
            }
            else if (ext == ".dt1")
            {
                var map = new TDiabloMap();
                map.Game = Game;
                map.ReadPalette(filename);
                Game.Map.Walls.Clear();
                map.ReadTileSet(filename, ext);
                map.MapTileSet(map.Floors);
            }
            else if (ext == ".ds1")
            {
                var map = new TDiabloMap();
                map.Game = Game;
                map.ReadMap(filename);
                //Map = map.GenerateMap();
                //bmp.Save("dispelMap.png");
            }
            else if (ext == ".map")
            {
                var map = new TDispelMap();
                map.Game = Game;
                map.ReadMap(filename);
                //Map = map.GenerateMap();
                //bmp.Save("dispelMap.png");
            }
            else if (ext == ".bmp")
            {
                var map = new THmmMap();
                map.Game = Game;
                map.ReadMap(filename);
                map.Init();
                Game.OnResourceChanged = ResourceChanged;
                HerosList.DataSource = Game.ActivePlayer.Heroes;
                HerosList.DisplayMember = "Name";
                HerosList.BackColor = Game.ActivePlayer.ID;
            }
            else if (ext == ".spr")
            {
                var anim = new TDispelAnimation();
                var map = anim.CreatePreviewMap(filename);
                map.Game = Game;
            }
            else if (ext == ".cof")
            {
                var anim = new TDiabloAnimation();
                var diabloMap = new TDiabloMap();
                diabloMap.ReadPalette(filename);
                anim.BasePath = Path.GetDirectoryName(Path.GetDirectoryName(diabloMap.BasePath));
                var name = Path.GetFileName(filename);
                anim.Name = name;
                anim.Token = name.Substring(0, 2);
                anim.Mode = name.Substring(2, 2);
                anim.ClassType = name.Substring(4, name.Length - 8);
                anim.ReadCof();
                var map = anim.CreatePreviewMap(filename);
                map.Game = Game;
            }
            Board.Invalidate();
            MapView.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                ImportMap(openFileDialog1.FileName);
        }

        //private void panel1_BackColorChanged(object sender, EventArgs e)
        //{
        //    SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        //    SetStyle(ControlStyles.Opaque, true);
        //}

        private void button3_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FilterIndex == 1 && Game.Map is TDispelMap)
                {
                    var converter = new TMapConverter();
                    converter.Game = Game;
                    converter.Export(saveFileDialog1.FileName);
                }
                else if (saveFileDialog1.FilterIndex == 1 && Game.Map is TDiabloMap)
                    Game.Map.WriteMap(saveFileDialog1.FileName);
                else if (saveFileDialog1.FilterIndex == 3 && Game.Map is TDiabloMap)
                    (Game.Map as TDiabloMap).WriteTileSet(saveFileDialog1.FileName, ".dt1");
            }

        }

        private void StrategyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //timeEndPeriod(1);
        }

        private void floorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            menuItem.Checked = !menuItem.Checked;
            var layer = (TDrawLayers)(1 << menuItem.GetCurrentParent().Items.IndexOf(menuItem));
            if (menuItem.Checked)
                Board.ShowedLayers |= layer;
            else
                Board.ShowedLayers ^= layer;
            Board.Invalidate();
        }

        private void dumpFloorDuplicatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var converter = new TMapConverter();
            converter.Game = Game;
            var debugBmp = converter.GetDuplicatesBmp();
            debugBmp.Save("duplicates.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private void dumpConversionGridsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var converter = new TMapConverter();
            converter.Game = Game;
            var debugBmp = converter.GetGridsBmp();
            debugBmp.Save("convertGrids.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

    }
}
