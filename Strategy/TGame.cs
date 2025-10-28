using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace Strategy
{
    public class TGame
    {
        public static int FPS = 30;
        public TBoard Board;
        public int RoundCount;
        public List<TPlayer> Players = new List<TPlayer>();
        public static int MaxPlayers = 6;
        public static Color[] PlayersID = new Color[] { Color.Cyan, Color.Magenta, Color.Yellow, Color.Red, Color.Green, Color.Blue }; 
        public static Random Random = new Random();
        public List<Image> ResImages;
        public List<Image> ArtifactImages;
        public EventHandler OnResourceChanged;
        public TMap Map;
        public Image MapView;
        public TCollect<TNpc> Npcs = new TCollect<TNpc>();
        public TNpc ActiveNpc;
        public TNpc MainChar;
        public TScript ActiveScript;
        public Dictionary<int, string> MapNames;
        public List<TCollect<TItem>> Items = new List<TCollect<TItem>>();

        public void ImportMap(string filename)
        {
            var ext = Path.GetExtension(filename).ToLower();
            if (ext == ".btl" || ext == ".gtl")
            {
                var map = new TDispelMap();
                map.Game = this;
                map.Floors = map.ReadTileSet(filename, ext);
                map.MapTileSet(map.Floors);
            }
            else if (ext == ".dt1")
            {
                var map = new TDiabloMap();
                map.Game = this;
                map.ReadPalette(filename);
                Map.Walls.Clear();
                map.ReadTileSet(filename, ext);
                map.MapTileSet(map.Floors);
            }
            else if (ext == ".ds1")
            {
                var map = new TDiabloMap();
                map.Game = this;
                map.ReadMap(filename);
                //Map = map.GenerateMap();
                //bmp.Save("dispelMap.png");
            }
            else if (ext == ".map")
            {
                var map = new TDispelMap();
                map.Game = this;
                map.ReadMap(filename);
                //Map = map.GenerateMap();
                //bmp.Save("dispelMap.png");
            }
            else if (ext == ".bmp")
            {
                var map = new TMap();
                map.Game = this;
                map.ReadMap(filename);
                Init();
            }
            else if (ext == ".spr")
            {
                var map = new TDispelMap();
                map.Game = this;
                map.MapAnimation(filename);
            }
            else if (ext == ".cof")
            {
                var map = new TDiabloMap();
                map.Game = this;
                map.MapAnimation(filename);
            }
        }

        public void OnEvent(int eventIdx)
        {
            var eventName = eventIdx.ToString("D4");
            var scr = new TIniReader($"{TMap.GamePath}/Ref/Event{eventName}.scr");
            ActiveScript = new TScript();
            ActiveScript.Game = this;
            ActiveScript.Commands = scr["ACT"];
            ActiveScript.Variables = scr.LoadVars();
            ActiveScript.Run();
            Board.Invalidate();
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

        public TGame()//[CallerFilePath] string filePath = null)
        {
            //var path = Path.GetDirectoryName(filePath) + "/bin/Debug";
            //var files = Directory.GetFiles(path + "/Tiles", "*.bmp");
            Map = new TMap();
            Map.Game = this;
            //Map.ReadMap(Application.StartupPath);
            //Map.MapName = "/Maps/map.bmp";
            //GroundTiles = new List<TTile>();
            //for (int i = 0; i < files.Length; i++)
            //{
            //    var tile = new TTile();
            //    tile.Image = Map.ReadTexture(files[i]);
            //    GroundTiles.Add(tile);
            //}
            //files = Directory.GetFiles(path + "/Resources", "*.bmp");
            //ResImages = new List<Image>();
            //for (int i = 0; i < files.Length; i++)
            //    ResImages.Add(ReadTexture(files[i]));
            //files = Directory.GetFiles(path + "/Artifacts", "*.bmp");
            //ArtifactImages = new List<Image>();
            //for (int i = 0; i < files.Length; i++)
            //    ArtifactImages.Add(ReadTexture(files[i]));
            ////Map = new Bitmap(path + "/Maps/map.bmp");
            //LoadMap(path + "/Maps/map.bmp");
        }

        //        private void LoadMap(string filename)
        //        {
        //            var bmp = new Bitmap(filename);
        //            Map = new TMap();
        //            Cells = new TCell[map.Height, map.Width];
        //            for (int y = 0; y < map.Height; y++)
        //                for (int x = 0; x < map.Width; x++)
        //{
        //                var cell = new TCell();
        //                cell.Game = this;
        //                cell.X = x;
        //                cell.Y = y;
        //                if (bmp.GetPixel(x, y).R == 0)
        //                {
        //                    var tile = GroundTiles[Random.Next(GroundTiles.Count)];
        //                    cell.Piece = tile;
        //                }
        //                else
        //                    FreeCells.Add(cell);
        //                Cells[y, x] = cell;
        //            }
        //        }

        //        Bitmap ReadTexture(string fileName)
        //        {
        //            var bmp = new Bitmap(fileName);
        //            var pal = bmp.Palette;
        //            for (int i = 0; i < bmp.Palette.Entries.Length; i++)
        //            {
        //                if (pal.Entries[i].ToArgb() == Color.Cyan.ToArgb())
        //                    pal.Entries[i] = Color.FromArgb(0, 0, 0, 0);
        //                if (pal.Entries[i].ToArgb() == Color.Magenta.ToArgb())
        //                    pal.Entries[i] = Color.FromArgb(128, 0, 0, 0);
        //                if ((uint)pal.Entries[i].ToArgb() == 0xffff96ff)
        //                    pal.Entries[i] = Color.FromArgb(0, 0, 0, 0);
        //            }
        //            bmp.Palette = pal;
        //            return bmp;
        //        }

        public void Restart()
        {
            Players.Clear();
            var playerCount = Random.Next(MaxPlayers - 1) + 1;
            for (int i = 0; i < playerCount; i++)
            {
                var player = TPlayer.Generate();
                player.Game = this;
                player.ID = PlayersID[i];
                Players.Add(player);
            }
            ActivePlayer = Players[0];

                //game.Players[0].Enemies.AddRange(game.Players.GetRange(1, game.Players.Count));
        }

        public void Init()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                for (int j = 0; j < ActivePlayer.Heroes.Count; j++)
                {
                    var idx = Random.Next(Map.FreeCells.Count);
                    var cell = Map.FreeCells[idx];
                    Map.FreeCells.RemoveAt(idx);
                    var hero = ActivePlayer.Heroes[j];
                    hero.Cell = cell;
                }
                NextTurn();
            }
        }

        internal void NextTurn()
        {
            ActivePlayer = Players[(Players.IndexOf(ActivePlayer) + 1) % Players.Count];
            ActivePlayer.SelectedHero = ActivePlayer.Heroes[0];
            for (int i = 0; i < ActivePlayer.Heroes.Count; i++)
                ActivePlayer.Heroes[i].MovesCount = ActivePlayer.Heroes[i].MovesCountMax;
            //ActivePlayer.Heroes.ForEach(item => item.MovesCount = item.MovesCountMax);
        }

        //public Bitmap Map;
    }
}
