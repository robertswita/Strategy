﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Strategy
{
    public class TGame
    {
        public static int FPS = 20;
        public TBoard Board;
        public TCell[,] Cells;
        public List<TCell> FreeCells = new List<TCell>();
        public int RoundCount;
        public List<TPlayer> Players = new List<TPlayer>();
        public static int MaxPlayers = 6;
        public static Color[] PlayersID = new Color[] { Color.Cyan, Color.Magenta, Color.Yellow, Color.Red, Color.Green, Color.Blue }; 
        public static Random Random = new Random();
        public List<Image> GroundTilesImages;
        public List<Image> BlockTilesImages;
        public List<Image> ResImages;
        public List<Image> ArtifactImages;
        public List<TColumnTile> RoofTiles = new List<TColumnTile>();
        public EventHandler OnResourceChanged;
        //public float CellAspect { get { return (float)TileHeight / TileWidth; } }
        public List<TSprite> Sprites = new List<TSprite>();
        public List<TAnimation> Animations = new List<TAnimation>();
        //public static int TileWidth = 64;
        //public static int TileHeight = 64;
        //public Size MapSize;
        public List<TColumnTile> ColumnTiles = new List<TColumnTile>();
        public List<TSprite> AnimatedSprites = new List<TSprite>();
        public TMap Map;
        public Image MapView;
        public List<TAnimation> ExtraSpriteModels = new List<TAnimation>();
        public List<TAnimation> MonsterSpriteModels = new List<TAnimation>();
        public List<TAnimation> NpcSpriteModels = new List<TAnimation>();
        public Dictionary<int, string> MapNames;

        public void ImportMap(string filename)
        {
            var ext = Path.GetExtension(filename).ToLower();
            if (ext == ".btl" || ext == ".gtl")
            {
                var map = new TDispelMap();
                map.Game = this;
                GroundTilesImages = map.ReadTileSet(filename, ext);
                map.ReadTileSet(filename, ext);
                map.MapTileSet();
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
                var s = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var reader = new BinaryReader(s);
                Animations.Clear();
                Sprites.Clear();
                AnimatedSprites.Clear();
                var animation = map.ReadAnimation(reader);
                Animations.Add(animation);
                var posX = 0;
                var posY = 0;
                for (var j = 0; j < animation.Sequences.Count; j++)
                {
                    var sequence = animation.Sequences[j];
                    posX = 0;
                    for (var k = 0; k < sequence.Length; k++)
                    {
                        var sprite = new TSprite();
                        sprite.Animation = animation;
                        sprite.Sequence = j;
                        sprite.ViewAngle = k;
                        var width = sprite.Frames[0].Bounds.Width;
                        var height = sprite.Frames[0].Bounds.Height;
                        sprite.X = posX;
                        sprite.Y = posY;
                        posX += width;
                        sprite.Bounds = new Rectangle(sprite.X, sprite.Y, width, height);
                        Sprites.Add(sprite);
                        if (sprite.Frames.Length > 1)
                            AnimatedSprites.Add(sprite);
                    }
                    posY += sequence[0][0].Bounds.Height;
                }
                map.Width = 2 * posX / TCell.Width + 4;
                map.Height = 2 * posY / TCell.Height + 2;
            }
        }

        public void OnEvent(int eventIdx)
        {
            var eventName = eventIdx.ToString("D4");
            var scr = new TIniReader($"{Map.GamePath}/Ref/Event{eventName}.scr");
            var commands = scr["ACT"];
            foreach (var command in commands)
            {
                if (command[0] == "teleport")
                {
                    var x = float.Parse(command[1]);
                    var y = float.Parse(command[2]);
                    var mapIdx = int.Parse(command[3]);
                    var mapName = MapNames[mapIdx];
                    if (mapName != Map.MapName)
                        Map.ReadMap($"{Map.GamePath}/Map/{mapName}.map");
                    var pos = (Map as TDispelMap).TransformGrid(x, y);
                    pos.X /= Map.Width;
                    pos.Y /= Map.Height;
                    Board.ScrollPos = new PointF(pos.X, pos.Y);
                }
                else if (command[0] == "setmap")
                {
                    scr = new TIniReader($"{Map.GamePath}/Ref/Map.ini");
                    var mapLine = scr[""][int.Parse(command[1])];
                    var x = float.Parse(mapLine[2]);
                    var y = float.Parse(mapLine[3]);
                    var mapIdx = int.Parse(mapLine[4]);
                    var mapName = MapNames[mapIdx];
                    Map.ReadMap($"{Map.GamePath}/Map/{mapName}.map");
                    var pos = (Map as TDispelMap).TransformGrid(x, y);
                    pos.X /= Map.Width;
                    pos.Y /= Map.Height;
                    Board.ScrollPos = new PointF(pos.X, pos.Y);
                }
            }
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

        //public TGame([CallerFilePath] string filePath = null)
        //{
        //    var path = Path.GetDirectoryName(filePath) + "/bin/Debug";
        //    var files = Directory.GetFiles(path + "/Tiles", "*.bmp");
        //    GroundTiles = new List<Image>();
        //    TilesImages = new List<Image>();
        //    for (int i = 0; i < files.Length; i++)
        //        TilesImages.Add(ReadTexture(files[i]));
        //    files = Directory.GetFiles(path + "/Resources", "*.bmp");
        //    ResImages = new List<Image>();
        //    for (int i = 0; i < files.Length; i++)
        //        ResImages.Add(ReadTexture(files[i]));
        //    files = Directory.GetFiles(path + "/Artifacts", "*.bmp");
        //    ArtifactImages = new List<Image>();
        //    for (int i = 0; i < files.Length; i++)
        //        ArtifactImages.Add(ReadTexture(files[i]));
        //    Map = new Bitmap(path + "/Maps/map.bmp");
        //    LoadMap();
        //}
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

                //game.Players[0].Enemies.AddRange(game.Players.GetRange(1, game.Players.Count));
        }

        public void Init()
        {
            for (int i = 0; i < Players.Count; i++)
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
