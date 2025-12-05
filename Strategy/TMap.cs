using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;

namespace Strategy
{
    public class TMap
    {
        public string MapName;
        public static string GamePath;
        public int Width = 1;
        public int Height = 1;
        public TCollect<TTile> Floors = new TCollect<TTile>();
        public TCollect<TTile> WallTiles = new TCollect<TTile>();
        public TCollect<TWall> Walls = new TCollect<TWall>();
        public List<TWall> Roofs = new List<TWall>();
        public TCollect<TAnimation> Animations = new TCollect<TAnimation>();
        public TCollect<TSprite> Sprites = new TCollect<TSprite>();
        public TCell[,] Cells;
        public List<List<string>> Dialogs;
        public List<List<string>> DialogTree;
        public virtual bool CollisionsEnabled { get; set; }

        public virtual Vector2 World2ViewTransform(float x, float y) { 
            return new Vector2(x * TTile.Width, y * TTile.Height); 
        }
        public virtual Vector2 View2MapTransform(float x, float y) {
            return new Vector2(x / TTile.Width, y / TTile.Height);
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

        public virtual void RebuildMapView() { }
        TGame game;
        public TGame Game
        {
            get { return game; }
            set { game = value; game.Map = this; }
        }

        public static TMap MapTileSet(TCollect<TTile> tiles)
        {
            var map = new TMap();
            int mapSize = (int)Math.Sqrt(tiles.Count) + 1;
            map.Height = mapSize;
            map.Width = mapSize;
            map.Cells = new TCell[mapSize, mapSize];
            for (int y = 0; y < mapSize; y++)
                for (int x = 0; x < mapSize; x++)
                {
                    var cell = new TCell();
                    cell.Map = map;
                    cell.X = x;
                    cell.Y = y;
                    var cellPos = map.Map2ViewTransform(x, y);
                    cell.Bounds = new Rectangle((int)cellPos.X, (int)cellPos.Y, TTile.Width, TTile.Height);
                    cell.Bounds.Offset(-TTile.Width / 2, -TTile.Height / 2);
                    var tileIdx = x * mapSize + y;
                    if (tileIdx < tiles.Count)
                        cell.Floor = tiles[tileIdx];
                    map.Cells[y, x] = cell;
                }
            return map;
        }

        public virtual void WriteMap(string path) { }

        public virtual void ReadMap(string path) { }

    }
}
