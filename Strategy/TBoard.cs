using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Strategy
{
    public partial class TBoard : UserControl
    {
        public TGame Game;
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
        public static PointF GridShear;
        public static PointF GridOffset;
        //public static int WorldHeight;

        public TBoard()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Game = new TGame();
            Game.Restart();
        }

        public static PointF TransformGrid(int  x, int y)
        {
            var x_ = x + GridShear.X * y + GridOffset.X;
            var y_ = y + GridShear.Y * x + GridOffset.Y;
            return new PointF(x_, y_);
        }
        public static PointF UnTransformGrid(float x_, float y_)
        {
            var denom = 1 - GridShear.X * GridShear.Y;
            var x = (x_ - GridShear.X * (y_ - GridOffset.Y) - GridOffset.X) / denom;
            var y = (y_ - GridShear.Y * (x_ - GridOffset.X) - GridOffset.Y) / denom;
            return new PointF(x, y);
        }

        public Matrix Transform
        {
            get
            {
                var transform = new Matrix();
                transform.Translate(Width / 2f, Height / 2f);
                transform.Scale(Zoom, Zoom);
                transform.Translate(-ScrollPos.X * Game.Map.Width * TGame.TileWidth / 2, -ScrollPos.Y * Game.Map.Height * TGame.TileHeight);
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
                //var tl = Unproject(UnTransformGrid(0, 0));
                //var br = Unproject(UnTransformGrid(Width, Height));
                //var bl = Unproject(UnTransformGrid(0, Height));
                //var tr = Unproject(UnTransformGrid(Width, 0));
                //var tl = Unproject(new Point(0, 0));
                //tl = UnTransformGrid((int)tl.X, (int)tl.Y);
                //var br = Unproject(new Point(Width, Height));
                //br = UnTransformGrid((int)br.X, (int)br.Y);
                //var bl = Unproject(new Point(0, Height));
                //bl = UnTransformGrid((int)br.X, (int)br.Y);
                //var tr = Unproject(new Point(Width, 0));
                //tr = UnTransformGrid((int)br.X, (int)br.Y);
                //var l = tl.X;
                //if (br.X < l) l = br.X;
                //if (bl.X < l) l = bl.X;
                //if (tr.X < l) l = tr.X;
                //var r = tl.X;
                //if (br.X > r) r = br.X;
                //if (bl.X > r) r = bl.X;
                //if (tr.X > r) r = tr.X;
                //var t = tl.Y;
                //if (br.Y < t) t = br.Y;
                //if (bl.Y < t) t = bl.Y;
                //if (tr.Y < t) t = tr.Y;
                //var b = tl.Y;
                //if (br.Y > b) b = br.Y;
                //if (bl.Y > b) b = bl.Y;
                //if (tr.Y > b) b = tr.Y;
                //return Rectangle.FromLTRB((int)l, (int)t, (int)r, (int)b);
                return Rectangle.FromLTRB((int)tl.X, (int)tl.Y, (int)br.X, (int)br.Y);
            }
        }

        public TCell CellAtPos(PointF p)
        {
            p = Unproject(p);
            if (p.X < 0 || p.X >= Game.Map.Width) return null;
            if (p.Y < 0 || p.Y >= Game.Map.Height) return null;
            return Game.Cells[(int)p.Y, (int)p.X];
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var hero = Game.ActivePlayer.SelectedHero;
            if (hero != null)
            {
                var cell = CellAtPos(e.Location);
                if (cell == null) return;
                if (cell == hero.StopCell && hero.Path.Count > 1)
                {
                    for (int i = 1; i < hero.Path.Count; i++)
                    {
                        if (hero.MovesCount == 0) break;
                        hero.Cell = hero.Path[i];
                        hero.MovesCount--;
                    }
                    hero.Path = new List<TCell>();
                    Invalidate();
                }
                else if (cell.IsVisible)
                {
                    if (cell.Piece != null && cell.Piece is TTile)
                        return;
                    hero.StopCell = cell;
                    Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var gc = e.Graphics;
            gc.InterpolationMode = InterpolationMode.NearestNeighbor;
            gc.PixelOffsetMode = PixelOffsetMode.Half;
            gc.Transform = Transform;
            //gc.DrawImage(Game.Map, 0, 0);

            var vp = Viewport;
            var sx = TGame.TileWidth / 2;
            var sy = TGame.TileHeight / 2;
            vp = new Rectangle(vp.Left / sx, vp.Top / sy, vp.Width / sx, vp.Height / sy);
            //var mapRect = new Rectangle((int)GridOffset.X, (int)GridOffset.Y, 2 * Game.Map.Width, Game.Map.Height);
            //vp.Intersect(mapRect);
            if ((vp.Left & 1) != 0)
                vp.Offset(1, 0);
            //vp.Inflate(2, 2);
            //var bmp = new Bitmap((int)(vp.Width * Game.TileWidth/2), (int)(vp.Height * Game.TileHeight / 2));
            //var gcBmp = Graphics.FromImage(bmp);
            Font font = new Font("Arial", 10f / Zoom);
            for (int y_ = vp.Top; y_ < vp.Bottom; y_++)
                for (int x_ = vp.Left; x_ < vp.Right; x_+= 2)
                {
                    var x = x_ + (y_ & 1);
                    var y = y_;
                    var pos = UnTransformGrid(x, y);
                    //pos.X += Game.Map.Width;
                    //pos.Y += Game.Map.Height;
                    if (pos.X < 0 || pos.X >= Game.Cells.GetLength(1) || pos.Y < 0 || pos.Y >= Game.Cells.GetLength(0)) continue;
                    //x *= Game.TileWidth / 2;
                    //y *= Game.TileHeight / 2;
                    var rc = new RectangleF(x, y, 2f, 2f);
                    var cell = Game.Cells[(int)pos.Y , (int)pos.X];
                    //if (!cell.IsVisible)
                    //{
                    //    gc.FillRectangle(Brushes.Black, rc);
                    //    continue;
                    //}
                    if (cell.GroundTile != null)
                    {
                        var gTile = Game.GroundTiles[cell.GroundTile.ImageIndex];
                        //rc.Inflate(0.5f, 0.5f);
                        gc.DrawImage(gTile, x * sx, y * sy);
                    }
                    var piece = cell.Piece;
                    if (piece is TTile)
                    {
                        var image = Game.TilesImages[piece.ImageIndex];
                        //rc.Inflate(0.5f, 0.5f);                        
                        gc.DrawImage(image, x * sx, y * sy);
                    }
                    else if (piece is TResource)
                    {
                        var image = Game.ResImages[piece.ImageIndex];
                        rc = new RectangleF(x - 1, y, 2, 1);
                        gc.DrawImage(image, rc);
                    }
                    else if (piece is TArtifact)
                    {
                        var image = Game.ArtifactImages[piece.ImageIndex];
                        rc = new RectangleF(x - 1, y, 2, 1);
                        gc.DrawImage(image, rc);
                    }
                    else if (piece is TSprite)
                    {
                        var hero = (TSprite)piece;
                        rc.Inflate(-0.25f, -0.25f);
                        gc.FillRectangle(new SolidBrush(hero.Player.ID), rc);
                        gc.DrawString(hero.Name, font, Brushes.Black, rc);
                    }
                }
            foreach (var column in Game.TiledObjects)
            {
                for (var n = 0; n < column.Cells.Count; n++)
                {
                    var cell = column.Cells[n];
                    var image = Game.TilesImages[cell.Piece.ImageIndex];
                    gc.DrawImage(image, cell.X + 32, cell.Y + 16);
                }
            }

            //bmp.Dispose();
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

        }

    }
}
