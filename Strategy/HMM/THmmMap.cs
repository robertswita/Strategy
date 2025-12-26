using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy.HMM
{
    class THmmMap : TMap
    {
        public List<TCell> FreeCells = new List<TCell>();
        public static double ResourceRatio = 0.1;
        public static double ArtifactRatio = 0.03;
        public List<Bitmap> ResImages;
        public List<Bitmap> ArtifactImages;

        public override void ReadMap(string path)
        {
            if (!Directory.Exists(path + "/Tiles")) return;
            //var path = Path.GetDirectoryName(Path.GetDirectoryName(filename));// Path.GetDirectoryName(filePath) + "/bin/Debug";
            var bgImage = new Bitmap(path + "/Background.png");
            Game.Board.BackgroundImage = bgImage;
            var files = Directory.GetFiles(path + "/Tiles", "*.bmp");
            for (int i = 0; i < files.Length; i++)
            {
                var tile = new TTile();
                //tile.Index = i;
                var im = ReadTexture(files[i]);
                var bounds = new RectangleF(0, 0, 1.5f * im.Width, 1.5f * im.Height);
                var bmp = new Bitmap((int)bounds.Width, (int)bounds.Height);
                var gc = Graphics.FromImage(bmp);
                gc.DrawImage(im, bounds);
                tile.Image = bmp;
                Floors.Add(tile);
            }
            files = Directory.GetFiles(path + "/Resources", "*.bmp");
            ResImages = new List<Bitmap>();
            for (int i = 0; i < files.Length; i++)
                ResImages.Add(ReadTexture(files[i]));
            files = Directory.GetFiles(path + "/Artifacts", "*.bmp");
            ArtifactImages = new List<Bitmap>();
            for (int i = 0; i < files.Length; i++)
                ArtifactImages.Add(ReadTexture(files[i]));
            LoadMap(path + "/Maps/map.bmp");
            var resCount = FreeCells.Count * ResourceRatio;
            for (int i = 0; i < resCount; i++)
            {
                var idx = TGame.Random.Next(FreeCells.Count);
                var cell = FreeCells[idx];
                FreeCells.RemoveAt(idx);
                var res = new TResource();
                res.Type = (TResource.ResType)TGame.Random.Next(ResImages.Count);
                //res.Index = (int)res.Type;
                res.Image = ResImages[(int)res.Type];
                cell.Piece = res;
            }
            var artifactCount = FreeCells.Count * ArtifactRatio;
            for (int i = 0; i < artifactCount; i++)
            {
                var idx = TGame.Random.Next(FreeCells.Count);
                var cell = FreeCells[idx];
                FreeCells.RemoveAt(idx);
                var artifact = new TArtifact();
                //artifact.Index = TGame.Random.Next(Game.ArtifactImages.Count);
                artifact.Image = ArtifactImages[TGame.Random.Next(ArtifactImages.Count)];
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
                        cell.Floor = Floors[TGame.Random.Next(Floors.Count)];
                    else
                        FreeCells.Add(cell);
                    Cells[y, x] = cell;
                }
            Game.MapView = bmp;
        }

        public void Init()
        {
            for (int i = 0; i < Game.Players.Count; i++)
            {
                for (int j = 0; j < Game.ActivePlayer.Heroes.Count; j++)
                {
                    var idx = TGame.Random.Next(FreeCells.Count);
                    var cell = FreeCells[idx];
                    FreeCells.RemoveAt(idx);
                    var hero = Game.ActivePlayer.Heroes[j];
                    hero.Cell = cell;
                }
                Game.NextTurn();
            }
        }


    }
}
