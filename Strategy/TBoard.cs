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
        public List<TSprite> VisibleSprites = new List<TSprite>();
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
                transform.Translate(-ScrollPos.X * Game.Map.Width * TCell.Scale.X, -ScrollPos.Y * Game.Map.Height * TCell.Height);
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
            //p.X -= TGame.TileWidth;
            //p.Y -= TGame.TileHeight;
            p = Unproject(p);
            //p.X /= TGame.TileWidth / 2;
            //p.Y /= TGame.TileHeight;
            var v = TDispelMap.YUnHex(new Vector2(p.X, p.Y));
            if (v.X < 0 || v.X >= Game.Map.Width) return null;
            if (v.Y < 0 || v.Y >= Game.Map.Height) return null;
            return Game.Cells[(int)v.Y, (int)v.X];
        }

        public int DialogId;
        int PrevDialogId;
        int NextDialogId;
        public void ProcessDialog(int option = 0)
        {
            DialogBox1.Visible = false;
            DialogBox2.Visible = false;
            if (DialogId == 0) return;
            if (PrevDialogId == 0)
            {
                //DialogId = Game.ActiveNpc.DialogId;
                DialogBox1.ForeColor = Color.Lime;
                DialogBox1.Text = Game.Map.Dialogs[DialogId][1];
                DialogBox1.Visible = true;
            }
            else
            {
                if (NextDialogId > 0)
                {
                    var prevDlgLine = Game.Map.DialogTree[DialogId];
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
                NextDialogId = nextDialogId;
                if (NextDialogId == 0)
                {
                    DialogId = 0;
                    if (Game.ActiveScript != null)
                        Game.ActiveScript.IsWaiting = false;
                }
                nextDialogId = int.Parse(dlgLine[7]);
                if (nextDialogId > 0)
                {
                    DialogBox2.ForeColor = color;
                    DialogBox2.Text = Game.Map.Dialogs[nextDialogId][1];
                    DialogBox2.Visible = true;
                }
            }
            PrevDialogId = DialogId;
        }

        //TNpc ActiveNpc;
        PointF StartScrollPos;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Right)
            {
                var scrollOffset = Unproject(e.Location) - new SizeF(Unproject(StartScrollPos));
                scrollOffset.X /= Game.Map.Width * TCell.Scale.X;
                scrollOffset.Y /= Game.Map.Height * TCell.Scale.Y;
                ScrollPos -= new SizeF(scrollOffset);
                StartScrollPos = e.Location;
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            StartScrollPos = e.Location;
            if (e.Button == MouseButtons.Right) return;
            DialogBox2.Visible = false;
            DialogBox1.Visible = false;
            Game.ActiveNpc = null;
            var cell = CellAtPos(e.Location);
            if (cell != null && cell.EventIdx > 0)
                Game.OnEvent(cell.EventIdx);
            else
            {
                var pos = Unproject(e.Location);
                foreach (var sprite in VisibleSprites)
                {
                    var p = new Point((int)pos.X, (int)pos.Y);
                    //if (!sprite.Bounds.Contains((int)pos.X, (int)pos.Y)) continue;
                    if (!sprite.HasAPoint(p)) continue;
                    if (sprite is TNpc)
                    {
                        Game.ActiveNpc = (TNpc)sprite;
                        //DialogId = 0;
                        //DialogBox1.ForeColor = Color.Lime;
                        //DialogBox1.Text = Game.Map.Dialogs[Game.ActiveNpc.DialogId][1];
                        //DialogBox1.Visible = true;
                        if (PrevDialogId == 0)
                            DialogId = Game.ActiveNpc.DialogId;
                        ProcessDialog();
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
                        if (element.EventId > 0)
                            Game.OnEvent(element.EventId);
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

        Point[] GetRhomb(Rectangle rc)
        {
            var center = new Point((rc.Left + rc.Right) / 2, (rc.Top + rc.Bottom) / 2);
            var pts = new Point[4];
            pts[0] = new Point(rc.Left, center.Y);
            pts[1] = new Point(center.X, rc.Bottom);
            pts[2] = new Point(rc.Right, center.Y);
            pts[3] = new Point(center.X, rc.Top);
            return pts;
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
            var sx = (int)TCell.Scale.X;
            var sy = TCell.Height;
            vp = new Rectangle(vp.Left / sx, vp.Top / sy, vp.Width / sx, vp.Height / sy);
            vp.Offset(1, 1);
            vp.Inflate(2, 2);
            vp.Intersect(new Rectangle(0, 0, Game.Map.Width, Game.Map.Height));
            //if ((vp.Top & 1) != 0)
            //    vp.Offset(0, 1);
            Font font = new Font("Arial", 10f / Zoom);
            var eventTiles = new List<Rectangle>();
            var collisionTiles = new List<Rectangle>();
            if (Game.Cells != null)
                for (int y_ = vp.Top; y_ < vp.Bottom; y_++)
                    for (int x_ = vp.Left; x_ < vp.Right; x_++)
                    {
                        //var x = x_;// + (y_ & 1);
                        //var y = 2 * y_ + (x & 1);// (y_ - (x_ & 1)) / 2;
                        //var rc = new Rectangle((x - 1) * sx, (y - 1) * sy / 2, 2 * sx, sy);
                        //if (x_ >= Game.Map.Width || y_ >= Game.Map.Height || x_ < 0 || y_ < 0) continue;
                        var cell = Game.Cells[y_, x_];
                        if (cell == null) continue;
                        //var pos = cell.Position;
                        //var rc = new Rectangle((int)pos.X, (int)pos.Y, TGame.TileWidth, TGame.TileHeight);
                        //rc.Offset(-TGame.TileWidth / 2, -TGame.TileHeight / 2);
                        var rc = cell.Bounds;
                        //if (!cell.IsVisible)
                        //{
                        //    gc.FillRectangle(Brushes.Black, rc);
                        //    continue;
                        //}
                        if (cell.EventIdx > 0)
                            eventTiles.Add(rc);
                        //if (cell.Collision)
                        //    collisionTiles.Add(rc);
                        //if (cell.GroundTile != null && cell.GroundTile.ImageIndex < Game.GroundTiles.Count)
                        //{
                        //    var gTile = Game.GroundTiles[cell.GroundTile.ImageIndex];
                        //    gc.DrawImage(gTile, rc.Left, rc.Top);
                        //    //gc.DrawPolygon(Pens.White, GetRhomb(cell.Bounds));
                        //    //gc.DrawString(cell.GroundTile.ImageIndex.ToString(), font, Brushes.White, rc.Location);
                        //}
                        if (cell.GroundTile != null)
                        {
                            gc.DrawImage(cell.GroundTile.Image, rc.Left, rc.Top);
                            //gc.DrawPolygon(Pens.Red, GetRhomb(cell.Bounds));
                            //gc.DrawRectangle(Pens.Red, rc);
                            //gc.DrawString(cell.GroundTile.ImageIndex.ToString(), font, Brushes.White, rc.Location);
                        }
                        //var piece = cell.Piece;
                        //if (piece is TTile)
                        //{
                        //    var image = Game.BlockTilesImages[piece.ImageIndex];
                        //    //rc.Inflate(0.5f, 0.5f);                        
                        //    gc.DrawImage(image, x * sx, y * sy);
                        //}
                        //else if (piece is TResource)
                        //{
                        //    var image = Game.ResImages[piece.ImageIndex];
                        //    //rc = new RectangleF(x - 1, y, 2, 1);
                        //    gc.DrawImage(image, rc);
                        //}
                        //else if (piece is TArtifact)
                        //{
                        //    var image = Game.ArtifactImages[piece.ImageIndex];
                        //    //rc = new RectangleF(x - 1, y, 2, 1);
                        //    gc.DrawImage(image, rc);
                        //}
                        //else if (piece is THero)
                        //{
                        //    var hero = (THero)piece;
                        //    //rc.Inflate(-0.25f, -0.25f);
                        //    gc.FillRectangle(new SolidBrush(hero.Player.ID), rc);
                        //    gc.DrawString(hero.Name, font, Brushes.Black, rc);
                        //}
                    }
            vp = new Rectangle(vp.Left * sx, vp.Top * sy, vp.Width * sx, vp.Height * sy);
            VisibleSprites.Clear();
            foreach (var column in Game.Walls)
                if (column.IsVisibleInRect(vp))
                    VisibleSprites.Add(column);
            foreach (var sprite in Game.Sprites)
                if (sprite.IsVisibleInRect(vp))
                    VisibleSprites.Add(sprite);
            VisibleSprites.Sort();
            foreach (var sprite in VisibleSprites)
            {
                sprite.Draw(gc);
                //if (sprite is TElement)
                //    gc.DrawRectangle(Pens.White, sprite.Bounds);
            }
            foreach (var roofTile in Game.RoofTiles)
                if (roofTile.IsVisibleInRect(vp) && Zoom < 1)
                    roofTile.Draw(gc);
            foreach (var rc in eventTiles)
                gc.DrawPolygon(Pens.Red, GetRhomb(rc));
            foreach (var rc in collisionTiles)
                gc.DrawPolygon(Pens.Blue, GetRhomb(rc));
            if (Game.ActiveNpc != null)
                foreach (var cell in Game.ActiveNpc.Path)
                {
                    //var pos = cell.Position;
                    //var rc = new Rectangle((int)pos.X, (int)pos.Y, TCell.Width, TGame.TileHeight);
                    //rc.Offset(-TGame.TileWidth / 2, -TGame.TileHeight / 2);
                    gc.DrawPolygon(Pens.Yellow, GetRhomb(cell.Bounds));
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
            var option = sender == DialogBox1 ? 0 : 1;
            ProcessDialog(option);
        }
    }
}
