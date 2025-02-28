using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Common;

namespace Strategy
{
    public class TGame
    {
        public TCell[,] Cells;
        public List<TCell> FreeCells = new List<TCell>();
        public int RoundCount;
        public List<TPlayer> Players = new List<TPlayer>();
        public static int MaxPlayers = 6;
        public static Color[] PlayersID = new Color[] { Color.Cyan, Color.Magenta, Color.Yellow, Color.Red, Color.Green, Color.Blue }; 
        public static Random Random = new Random();
        public List<Image> TilesImages;
        public List<Image> ResImages;
        public List<Image> ArtifactImages;
        public EventHandler OnResourceChanged;
        public float CellAspect { get { return (float)TileHeight / TileWidth; } }
        public List<TSprite> Sprites = new List<TSprite>();
        public int TileWidth = 64;
        public int TileHeight = 64;

        public void ImportTiles(string filename)
        {
            var map = new TDispelMap();
            map.Game = this;
            var ext = Path.GetExtension(filename);
            if (ext == ".btl" || ext == ".gtl")
            {
                map.LoadTileSet(filename);
            }
            else if (ext == ".map")
            {
                map.LoadMap(filename);
                Map = map.GenerateMap();
                //bmp.Save("dispelMap.png");
            }
        }

        TPlayer activePlayer;
        public TPlayer ActivePlayer
        {
            get { return activePlayer; }
            set
            {
                if (activePlayer != null)
                    for (int i = 0; i < activePlayer.VisibleCells.Count; i++)
                        activePlayer.VisibleCells[i].IsVisible = false;
                activePlayer = value;
                if (activePlayer != null)
                    for (int i = 0; i < activePlayer.VisibleCells.Count; i++)
                        activePlayer.VisibleCells[i].IsVisible = true;
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

        public TGame([CallerFilePath] string filePath = null)
        {
            var path = Path.GetDirectoryName(filePath) + "/bin/Debug";
            var files = Directory.GetFiles(path + "/Tiles", "*.bmp");
            TilesImages = new List<Image>();
            for (int i = 0; i < files.Length; i++)
                TilesImages.Add(ReadTexture(files[i]));
            files = Directory.GetFiles(path + "/Resources", "*.bmp");
            ResImages = new List<Image>();
            for (int i = 0; i < files.Length; i++)
                ResImages.Add(ReadTexture(files[i]));
            files = Directory.GetFiles(path + "/Artifacts", "*.bmp");
            ArtifactImages = new List<Image>();
            for (int i = 0; i < files.Length; i++)
                ArtifactImages.Add(ReadTexture(files[i]));
            Map = new Bitmap(path + "/Maps/map.bmp");
            LoadMap();
        }
        public static double ResourceRatio = 0.1;
        public static double ArtifactRatio = 0.03;
        public void Restart()
        {
            var resCount = FreeCells.Count * ResourceRatio;
            for (int i = 0; i < resCount; i++)
            {
                var idx = Random.Next(FreeCells.Count);
                var cell = FreeCells[idx];
                FreeCells.RemoveAt(idx);
                var res = new TResource();
                res.Type = (TResource.ResType)Random.Next(ResImages.Count);
                res.ImageIndex = (int)res.Type;
                cell.Piece = res;
            }
            var artifactCount = FreeCells.Count * ArtifactRatio;
            for (int i = 0; i < artifactCount; i++)
            {
                var idx = Random.Next(FreeCells.Count);
                var cell = FreeCells[idx];
                FreeCells.RemoveAt(idx);
                var artifact = new TArtifact();
                artifact.ImageIndex = Random.Next(ArtifactImages.Count);
                cell.Piece = artifact;;
            }
            var playerCount = Random.Next(MaxPlayers - 1) + 1;
            for (int i = 0; i < playerCount; i++)
            {
                var player = TPlayer.Generate();
                player.Game = this;
                player.ID = PlayersID[i];
                Players.Add(player);
            }
            ActivePlayer = Players[0];
            for (int i = 0; i < playerCount; i++)
            {
                for (int j = 0; j < ActivePlayer.Heroes.Count; j++)
                {
                    var idx = Random.Next(FreeCells.Count);
                    var cell = FreeCells[idx];
                    FreeCells.RemoveAt(idx);
                    var hero = ActivePlayer.Heroes[j];
                    hero.Cell = cell;
                }
                NextTurn();
            }
                //game.Players[0].Enemies.AddRange(game.Players.GetRange(1, game.Players.Count));
        }

        internal void NextTurn()
        {
            ActivePlayer = Players[(Players.IndexOf(ActivePlayer) + 1) % Players.Count];
            ActivePlayer.SelectedHero = ActivePlayer.Heroes[0];
            for (int i = 0; i < ActivePlayer.Heroes.Count; i++)
                ActivePlayer.Heroes[i].MovesCount = ActivePlayer.Heroes[i].MovesCountMax;
            //ActivePlayer.Heroes.ForEach(item => item.MovesCount = item.MovesCountMax);
        }

        public Bitmap Map;
        public void LoadMap()
        {
            Cells = new TCell[Map.Height, Map.Width];
            for (int y = 0; y < Map.Height; y++)
                for (int x = 0; x < Map.Width; x++)
                {
                    var cell = new TCell();
                    cell.Game = this;
                    cell.X = x;
                    cell.Y = y;
                    if (Map.GetPixel(x, y).R == 0)
                    {
                        var tile = new TTile();
                        tile.ImageIndex = Random.Next(TilesImages.Count);
                        cell.Piece = tile;
                    }
                    else
                        FreeCells.Add(cell);
                    Cells[y, x] = cell;
                }
        }
    }
}
