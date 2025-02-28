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
    public partial class StrategyForm : Form
    {
        public float ScrollStep = 0.01f;

        public StrategyForm()
        {
            InitializeComponent();
            HerosList.DataSource = Board.Game.ActivePlayer.Heroes;
            HerosList.DisplayMember = "Name";
            HerosList.BackColor = Board.Game.ActivePlayer.ID;
            Board.MouseWheel += TBoard1_MouseWheel;
            Board.Scroll += BoardScroll;
            Board.Game.OnResourceChanged = ResourceChanged;
        }

        private void BoardScroll(object sender, ScrollEventArgs e)
        {
            MapView.Invalidate();
        }

        private void MapView_Paint(object sender, PaintEventArgs e)
        {
            var scaleX = (float)MapView.Width / Board.Game.Map.Width;
            var scaleY = (float)MapView.Height / Board.Game.Map.Height;
            e.Graphics.ScaleTransform(scaleX, scaleY);
            e.Graphics.DrawImage(Board.Game.Map, new Rectangle(0, 0, Board.Game.Map.Width, Board.Game.Map.Height));
            e.Graphics.DrawRectangle(Pens.Blue, Board.Viewport);
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
                case Keys.T: ImportTiles(); break;
            }
        }

        void ImportTiles()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Board.Game.ImportTiles(openFileDialog1.FileName);
                Board.Invalidate();
            }
        }
    }
}
