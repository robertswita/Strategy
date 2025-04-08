using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Strategy
{
    public partial class StrategyForm : Form
    {
        TGame Game;
        public float ScrollStep {  get { return 0.01f / Board.Zoom; } }

        public StrategyForm()
        {
            InitializeComponent();
            Game = new TGame();
            Board.Game = Game;
            var map = new TMap();
            map.Game = Game;
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
            e.Graphics.DrawImage(Game.MapView, 0, 0);
            var scaleX = MapView.Width / (Board.Game.Map.Width * TCell.Scale.X);
            var scaleY = MapView.Height / (Board.Game.Map.Height * TCell.Scale.Y);
            e.Graphics.ScaleTransform(scaleX, scaleY / 2);
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
            foreach (var sprite in Game.AnimatedSprites)
            {
                var actFrame = sprite.ActFrame;
                sprite.NextFrame();
                if (actFrame != sprite.ActFrame)
                    Board.Invalidate();
            }
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
                var map = Game.Map as TDispelMap;
                if (map != null)
                    map.WriteMap(saveFileDialog1.FileName);
            }

        }
    }
}
