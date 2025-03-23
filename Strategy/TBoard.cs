using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Numerics;
using System.Security.Policy;

namespace Strategy
{
    public partial class TBoard : UserControl
    {
        TGame game;
        public TGame Game
        {
            get { return game; }
            set { game = value; if (game != null) game.Board = this; }
        }
        public float Zoom { get; set; }
        PointF _ScrollPos;
        public PointF ScrollPos
        {
            get { return _ScrollPos; }
            set
            {
                if (value != _ScrollPos)
                {
                    _ScrollPos.X = Math.Max(Math.Min(value.X, 1), 0);
                    _ScrollPos.Y = Math.Max(Math.Min(value.Y, 1), 0);
                    OnScroll(null);
                    Invalidate();
                }
            }
        }

        public TBoard()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        public Matrix Transform
        {
            get
            {
                var transform = new Matrix();
                transform.Translate(Width / 2f, Height / 2f);
                transform.Scale(Zoom, Zoom);
                //transform.Scale(Zoom * TGame.TileWidth / 2, Zoom * TGame.TileHeight / 2);
                //transform.Translate(-ScrollPos.X * Game.Map.Width, -ScrollPos.Y * Game.Map.Height);
                transform.Translate(-ScrollPos.X * Game.Map.Width * TGame.TileWidth / 2, -ScrollPos.Y * Game.Map.Height * TGame.TileHeight / 2);
                return transform;
            }
        }

        public PointF Unproject(PointF p)
        {
            var pts = new PointF[] { p };
            var transform = Transform;
            transform.Invert();
            transform.TransformPoints(pts);
            return pts[0];
        }

        public PointF Project(PointF p)
        {
            var pts = new PointF[] { p };
            Transform.TransformPoints(pts);
            return pts[0];
        }

        public Rectangle Viewport
        {
            get
            {
                var tl = Unproject(Point.Empty);
                var br = Unproject(new Point(Width, Height));
                return Rectangle.FromLTRB((int)tl.X, (int)tl.Y, (int)br.X, (int)br.Y);
            }
        }

        public TCell CellAtPos(PointF p)
        {
            p = Unproject(p);
            p.X /= TGame.TileWidth / 2;
            p.Y /= TGame.TileHeight / 2;
            if (p.X < 0 || p.X >= Game.Map.Width) return null;
            if (p.Y < 0 || p.Y >= Game.Map.Height) return null;
            return Game.Cells[(int)p.Y, (int)p.X];
        }

        int DialogId;
        TNpc ActiveNpc;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            DialogBox2.Visible = false;
            DialogBox1.Visible = false;
            var cell = CellAtPos(e.Location);
            if (cell != null && cell.EventIdx > 0)
                Game.OnEvent(cell.EventIdx);
            else
            {
                var pos = Unproject(e.Location);
                foreach (var sprite in Game.Sprites)
                {
                    if (!sprite.Bounds.Contains((int)pos.X, (int)pos.Y)) continue;
                    if (sprite is TNpc)
                    {
                        ActiveNpc = (TNpc)sprite;
                        DialogId = 0;
                        DialogBox1.ForeColor = Color.Lime;
                        DialogBox1.Text = Game.Map.Dialogs[ActiveNpc.DialogId][1];
                        DialogBox1.Visible = true;
                        break;
                    }
                    else if (sprite is TElement)
                    {
                        var element = (TElement)sprite;
                        if (element.Type == TElementType.Chest)
                        {
                            if (element.Closed == 0)
                                element.Sequence = 2;
                        }
                        break;
                    }
                    else if (sprite is TMonster)
                    {
                        var monster = (TMonster)sprite;
                        monster.Sequence++;
                        if (monster.Sequence == monster.Animation.Sequences.Count)
                            monster.Sequence = 0;
                        break;
                    }
                }
            }
            //var hero = Game.ActivePlayer.SelectedHero;
            //if (hero != null)
            //{
            //    var cell = CellAtPos(e.Location);
            //    if (cell == null) return;
            //    if (cell == hero.StopCell && hero.Path.Count > 1)
            //    {
            //        for (int i = 1; i < hero.Path.Count; i++)
            //        {
            //            if (hero.MovesCount == 0) break;
            //            hero.Cell = hero.Path[i];
            //            hero.MovesCount--;
            //        }
            //        hero.Path = new List<TCell>();
            //        Invalidate();
            //    }
            //    else if (cell.IsVisible)
            //    {
            //        if (cell.Piece != null && cell.Piece is TTile)
            //            return;
            //        hero.StopCell = cell;
            //        Invalidate();
            //    }
            //}
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Game == null) return;
            var gc = e.Graphics;
            gc.InterpolationMode = InterpolationMode.NearestNeighbor;
            gc.PixelOffsetMode = PixelOffsetMode.Half;
            gc.Transform = Transform;

            var vp = Viewport;
            var sx = TGame.TileWidth / 2;
            var sy = TGame.TileHeight / 2;
            vp = new Rectangle(vp.Left / sx, vp.Top / sy, vp.Width / sx, vp.Height / sy);
            vp.Inflate(2, 2);
            vp.Intersect(new Rectangle(0, 0, Game.Map.Width - 2, Game.Map.Height - 1));
            if ((vp.Left & 1) != 0)
                vp.Offset(1, 0);
            Font font = new Font("Arial", 10f / Zoom);
            var eventTiles = new List<Rectangle>();
            var collisionTiles = new List<Rectangle>();
            if (Game.Cells != null)
                for (int y_ = vp.Top; y_ < vp.Bottom; y_++)
                    for (int x_ = vp.Left; x_ < vp.Right; x_ += 2)
                    {
                        var x = x_ + (y_ & 1);
                        var y = y_;
                        var rc = new Rectangle((x - 1) * sx, (y - 1) * sy, 2 * sx, 2 * sy);
                        var cell = Game.Cells[y, x];
                        //if (!cell.IsVisible)
                        //{
                        //    gc.FillRectangle(Brushes.Black, rc);
                        //    continue;
                        //}
                        if (cell.EventIdx > 0)
                            eventTiles.Add(rc);
                        //if (cell.Collision)
                        //    collisionTiles.Add(rc);
                        if (cell.GroundTile != null)
                        {
                            var gTile = Game.GroundTilesImages[cell.GroundTile.ImageIndex];
                            gc.DrawImage(gTile, rc.Left, rc.Top);
                        }
                        var piece = cell.Piece;
                        if (piece is TTile)
                        {
                            var image = Game.BlockTilesImages[piece.ImageIndex];
                            //rc.Inflate(0.5f, 0.5f);                        
                            gc.DrawImage(image, x * sx, y * sy);
                        }
                        else if (piece is TResource)
                        {
                            var image = Game.ResImages[piece.ImageIndex];
                            //rc = new RectangleF(x - 1, y, 2, 1);
                            gc.DrawImage(image, rc);
                        }
                        else if (piece is TArtifact)
                        {
                            var image = Game.ArtifactImages[piece.ImageIndex];
                            //rc = new RectangleF(x - 1, y, 2, 1);
                            gc.DrawImage(image, rc);
                        }
                        else if (piece is THero)
                        {
                            var hero = (THero)piece;
                            //rc.Inflate(-0.25f, -0.25f);
                            gc.FillRectangle(new SolidBrush(hero.Player.ID), rc);
                            gc.DrawString(hero.Name, font, Brushes.Black, rc);
                        }
                    }
            vp = new Rectangle(vp.Left * sx, vp.Top * sy, vp.Width * sx, vp.Height * sy);
            var spriteList = new List<TSprite>();
            foreach (var column in Game.ColumnTiles)
                if (column.IsVisibleInRect(vp))
                    spriteList.Add(column);
            foreach (var sprite in Game.Sprites)
                if (sprite.IsVisibleInRect(vp))
                    spriteList.Add(sprite);
            spriteList.Sort();
            foreach (var sprite in spriteList)
                sprite.Draw(gc);
            foreach (var roofTile in Game.RoofTiles)
                if (roofTile.IsVisibleInRect(vp) && Zoom < 1)
                    roofTile.Draw(gc);
            foreach (var rc in eventTiles)
            {
                var center = new Point(rc.Left + sx, rc.Top + sy);
                var pts = new Point[4];
                pts[0] = new Point(rc.Left, center.Y);
                pts[1] = new Point(center.X, rc.Bottom);
                pts[2] = new Point(rc.Right, center.Y);
                pts[3] = new Point(center.X, rc.Top);
                gc.DrawPolygon(Pens.Red, pts);
            }
            foreach (var rc in collisionTiles)
            {
                var center = new Point(rc.Left + sx, rc.Top + sy);
                var pts = new Point[4];
                pts[0] = new Point(rc.Left, center.Y);
                pts[1] = new Point(center.X, rc.Bottom);
                pts[2] = new Point(rc.Right, center.Y);
                pts[3] = new Point(center.X, rc.Top);
                gc.DrawPolygon(Pens.Blue, pts);
            }

            //var SelectedHero = Game.ActivePlayer.SelectedHero;
            ////if (SelectedHero == null) return;
            //var stopCell = SelectedHero.StopCell;
            //if (stopCell != null)
            //{
            //    var rc = new Rectangle(stopCell.X, stopCell.Y, 1, 1);
            //    var pen = new Pen(Color.Black, 3f / Zoom);
            //    if (SelectedHero.MovesCount > 0)
            //        pen.Color = SelectedHero.Player.ID;
            //    gc.TranslateTransform(0.5f, 0.5f);
            //    for (int i = 1; i < SelectedHero.Path.Count; i++)
            //    {
            //        var prevCell = SelectedHero.Path[i - 1];
            //        var cell = SelectedHero.Path[i];
            //        if (i > SelectedHero.MovesCount)
            //            pen.Color = Color.Black;
            //        gc.DrawLine(pen, prevCell.X, prevCell.Y, cell.X, cell.Y);
            //    }
            //    gc.TranslateTransform(-0.5f, -0.5f);
            //    gc.DrawEllipse(pen, rc);
            //}
            //if (DialogBox2.Visible)
            //{
            //    gc.ResetTransform();
            //    var bmp = new Bitmap(richTextBox1.Width, richTextBox1.Height);
            //    richTextBox1.DrawToBitmap(bmp, richTextBox1.ClientRectangle);
            //    bmp.MakeTransparent(richTextBox1.BackColor);
            //    gc.DrawImage(bmp, 0, 0);
            //}

        }

        private void label1_Paint(object sender, PaintEventArgs e)
        {
            var frame = DialogBox1.ClientRectangle;
            frame.Inflate(-5, -5);
            e.Graphics.DrawRectangle(Pens.Lime, frame);
        }

        private void DialogBox1_MouseEnter(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = Color.FromArgb(127, 255, 255, 255);
        }

        private void DialogBox1_MouseLeave(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = Color.Transparent;
        }

        private void DialogBox1_Click(object sender, EventArgs e)
        {
            DialogBox1.Visible = false;
            DialogBox2.Visible = false;
            if (DialogId == 0)
                DialogId = ActiveNpc.DialogId;
            else
            {
                var prevDlgLine = Game.Map.DialogTree[DialogId];
                var option = sender == DialogBox1 ? 0 : 1;
                DialogId = int.Parse(prevDlgLine[6 + option]);
            }
            var dlgLine = Game.Map.DialogTree[DialogId];
            var npc = int.Parse(dlgLine[4]);
            var color = npc == 1 ? Color.Blue : Color.Lime;
            var nextDialogId = int.Parse(dlgLine[6]);
            if (nextDialogId > 0)
            {
                DialogBox1.ForeColor = color;
                DialogBox1.Text = Game.Map.Dialogs[nextDialogId][1];
                DialogBox1.Visible = true;
            }
            nextDialogId = int.Parse(dlgLine[7]);
            if (nextDialogId > 0)
            {
                DialogBox2.ForeColor = color;
                DialogBox2.Text = Game.Map.Dialogs[nextDialogId][1];
                DialogBox2.Visible = true;
            }
        }
    }
}
