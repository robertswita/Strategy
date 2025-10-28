using Common;
using Strategy.Diablo;
using Strategy.Dispel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TMap
    {
        public virtual int TileWidth { get { return TTile.Width; } }
        public virtual int TileHeight { get { return TTile.Height; } }
        public string MapName;
        public static string GamePath;
        public List<TTile> Floors = new List<TTile>();
        public List<TTile> BlockTiles = new List<TTile>();
        public TCollect<TBlockTile> Walls = new TCollect<TBlockTile>();
        public List<TBlockTile> Roofs = new List<TBlockTile>();
        public TCollect<TAnimation> Animations = new TCollect<TAnimation>();
        public TCollect<TSprite> Sprites = new TCollect<TSprite>();
        public TCell[,] Cells;
        public List<TCell> FreeCells = new List<TCell>();
        public List<List<string>> Dialogs;
        public List<List<string>> DialogTree;
        public static double ResourceRatio = 0.1;
        public static double ArtifactRatio = 0.03;
        public virtual Vector2 World2ViewTransform(float x, float y) { 
            return new Vector2(x * TileWidth, y * TileHeight); 
        }
        public virtual Vector2 View2MapTransform(float x, float y) {
            return new Vector2(x / TileWidth, y / TileHeight);
        }
        public virtual Vector2 Map2WorldTransform(float x, float y) { return new Vector2(x, y); }
        public Vector2 View2WorldTransform(float x, float y)
        {
            var v = View2MapTransform(x, y);
            return Map2WorldTransform(v.X, v.Y);
        }
        public Vector2 World2MapTransform(float x, float y) 
        {
            var v = World2ViewTransform(x, y);
            return View2MapTransform(v.X, v.Y);
        }
        public Vector2 Map2ViewTransform(float x, float y) 
        {
            var v = Map2WorldTransform(x, y);
            return World2ViewTransform(v.X, v.Y);
        }

        //TCell[,] UntransformFromHexMapping()
        //{
        //    var cells = new TCell[Height, Width];
        //    for (int y = 0; y < Height; y++)
        //        for (int x = 0; x < Width; x++)
        //        {
        //            //var pos = UnTransformGrid(x, 2 * y + (x & 1));
        //            var pos = UnTransformGrid(x, y);
        //            if (pos.Y >= 0 && pos.Y < WorldHeight && pos.X >= 0 && pos.X < WorldWidth)
        //            {
        //                var cell = Game.Cells[(int)pos.Y, (int)pos.X];
        //                cell.X = x;
        //                cell.Y = y;
        //                cells[y, x] = cell;
        //            }
        //            //else
        //            //    cells[y, x] = Game.Cells[0, 0];
        //        }
        //    return cells;
        //}

        //TCell[,] TransformToHexMapping()
        //{
        //    var cells = new TCell[WorldHeight, WorldWidth];
        //    for (int y = 0; y < Height; y++)
        //        for (int x = 0; x < Width; x++)
        //        {
        //            //var pos = UnTransformGrid(x, 2 * y + (x & 1));
        //            var pos = UnTransformGrid(x, y);
        //            if (pos.Y >= 0 && pos.Y < WorldHeight && pos.X >= 0 && pos.X < WorldWidth)
        //            {
        //                var cell = Game.Cells[y, x];
        //                //cell.X = (int)pos.X;
        //                //cell.Y = (int)pos.Y;
        //                cells[(int)pos.Y, (int)pos.X] = cell;
        //            }
        //        }
        //    return cells;
        //}

        public virtual void RebuildMapView() { }
        TGame game;
        public TGame Game
        {
            get { return game; }
            set { game = value; game.Map = this; }
        }
        public int Width = 1;
        public int Height = 1;
        //public Size MapSize;
        //public virtual Bitmap Image { get; set; }
        public virtual void WriteMap(string path) { }

        public virtual void ReadMap(string path)
        {
            if (!Directory.Exists(path + "/Tiles")) return;
            //var path = Path.GetDirectoryName(Path.GetDirectoryName(filename));// Path.GetDirectoryName(filePath) + "/bin/Debug";
            var files = Directory.GetFiles(path + "/Tiles", "*.bmp");
            for (int i = 0; i < files.Length; i++)
            {
                var tile = new TTile();
                tile.Index = i;
                var im = ReadTexture(files[i]);
                var bounds = new RectangleF(0, 0, 1.5f * im.Width, 1.5f * im.Height);
                var bmp = new Bitmap((int)(bounds.Width * 1.25), (int)bounds.Height);
                var gc = Graphics.FromImage(bmp);
                gc.DrawImage(im, bounds);
                tile.Image = bmp;
                BlockTiles.Add(tile);
            }
            files = Directory.GetFiles(path + "/Resources", "*.bmp");
            Game.ResImages = new List<Image>();
            for (int i = 0; i < files.Length; i++)
                Game.ResImages.Add(ReadTexture(files[i]));
            files = Directory.GetFiles(path + "/Artifacts", "*.bmp");
            Game.ArtifactImages = new List<Image>();
            for (int i = 0; i < files.Length; i++)
                Game.ArtifactImages.Add(ReadTexture(files[i]));
            LoadMap(path + "/Maps/map.bmp");
            var resCount = FreeCells.Count * ResourceRatio;
            for (int i = 0; i < resCount; i++)
            {
                var idx = TGame.Random.Next(FreeCells.Count);
                var cell = FreeCells[idx];
                FreeCells.RemoveAt(idx);
                var res = new TResource();
                res.Type = (TResource.ResType)TGame.Random.Next(Game.ResImages.Count);
                res.Index = (int)res.Type;
                cell.Piece = res;
            }
            var artifactCount = FreeCells.Count * ArtifactRatio;
            for (int i = 0; i < artifactCount; i++)
            {
                var idx = TGame.Random.Next(FreeCells.Count);
                var cell = FreeCells[idx];
                FreeCells.RemoveAt(idx);
                var artifact = new TArtifact();
                artifact.Index = TGame.Random.Next(Game.ArtifactImages.Count);
                cell.Piece = artifact;
            }
        }

        Bitmap ReadTexture(string fileName)
        {
            var bmp = new Bitmap(fileName);
            var pal = bmp.Palette;
            for (int i = 0; i < bmp.Palette.Entries.Length; i++)
            {
                if (pal.Entries[i].ToArgb() == Color.Cyan.ToArgb())
                    pal.Entries[i] = Color.FromArgb(0, 0, 0, 0);
                if (pal.Entries[i].ToArgb() == Color.Magenta.ToArgb())
                    pal.Entries[i] = Color.FromArgb(128, 0, 0, 0);
                if ((uint)pal.Entries[i].ToArgb() == 0xffff96ff)
                    pal.Entries[i] = Color.FromArgb(0, 0, 0, 0);
            }
            bmp.Palette = pal;
            return bmp;
        }

        public void LoadMap(string filename)
        {
            var bmp = new Bitmap(filename);
            Height = bmp.Height;
            Width = bmp.Width;
            Cells = new TCell[Height, Width];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var cell = new TCell();
                    cell.Map = this;
                    cell.X = x;
                    cell.Y = y;
                    var cellPos = Map2ViewTransform(x, y);
                    cell.Bounds = new Rectangle((int)cellPos.X, (int)cellPos.Y, TTile.Width, TTile.Height);
                    cell.Bounds.Offset(-TTile.Width / 2, -TTile.Height / 2);
                    if (bmp.GetPixel(x, y).R == 0)
                    {
                        //var tile = new TTile();
                        //tile.ImageIndex = TGame.Random.Next(Game.BlockTiles.Count);
                        //cell.Piece = tile;
                        var wall = new TBlockTile();
                        var tile = BlockTiles[TGame.Random.Next(BlockTiles.Count)];
                        wall.Tiles.Add(tile);
                        wall.X = (int)cell.Position.X;
                        wall.Y = (int)cell.Position.Y;
                        var bounds = cell.Bounds;
                        //bounds.Inflate((int)(1.25 * bounds.Width), (int)(1.25 * bounds.Height));
                        wall.Bounds = bounds;
                        wall.Order = y * Width + x;
                        Walls.Add(wall);
                    }
                    else
                        FreeCells.Add(cell);
                    Cells[y, x] = cell;
                }
            Game.MapView = bmp;
        }



    }
}
