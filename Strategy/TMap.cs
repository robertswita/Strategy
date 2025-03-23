using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TMap
    {
        public string MapName;
        public string GamePath;
        public List<List<string>> Dialogs;
        public List<List<string>> DialogTree;
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
        public virtual void ReadMap(string filename)
        {
            var path = Path.GetDirectoryName(Path.GetDirectoryName(filename));// Path.GetDirectoryName(filePath) + "/bin/Debug";
            var files = Directory.GetFiles(path + "/Tiles", "*.bmp");
            Game.GroundTilesImages = new List<Image>();
            Game.BlockTilesImages = new List<Image>();
            for (int i = 0; i < files.Length; i++)
                Game.BlockTilesImages.Add(ReadTexture(files[i]));
            files = Directory.GetFiles(path + "/Resources", "*.bmp");
            Game.ResImages = new List<Image>();
            for (int i = 0; i < files.Length; i++)
                Game.ResImages.Add(ReadTexture(files[i]));
            files = Directory.GetFiles(path + "/Artifacts", "*.bmp");
            Game.ArtifactImages = new List<Image>();
            for (int i = 0; i < files.Length; i++)
                Game.ArtifactImages.Add(ReadTexture(files[i]));
            LoadMap(path + "/Maps/map.bmp");

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
            Game.Cells = new TCell[Height, Width];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var cell = new TCell();
                    cell.Game = Game;
                    cell.X = x;
                    cell.Y = y;
                    if (bmp.GetPixel(x, y).R == 0)
                    {
                        var tile = new TTile();
                        tile.ImageIndex = TGame.Random.Next(Game.BlockTilesImages.Count);
                        cell.Piece = tile;
                    }
                    else
                        Game.FreeCells.Add(cell);
                    Game.Cells[y, x] = cell;
                }
            Game.MapView = bmp;
        }



    }
}
