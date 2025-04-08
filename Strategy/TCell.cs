using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Strategy
{
    public class TCell
    {
        public int X;
        public int Y;
        public static int Width = 64;
        public static int Height = 64;
        public static Vector2 Scale { get { return new Vector2(Width / 2, Height / 2); } }
        public Vector2 Position { get { return new Vector2(X * Scale.X, (2 * Y + (X & 1)) * Scale.Y); } }
        public Rectangle Bounds 
        { 
            get 
            {
                var pos = Position - Scale;
                return new Rectangle((int)pos.X, (int)pos.Y, Width, Height); 
            } 
        }
        public TGame Game;
        public TPiece Piece;
        public TPiece GroundTile;
        public int EventIdx;
        public bool Collision;
        public bool IsVisible;
        public TCell Parent;
        public TNpc Npc;
        static int[] neighMask = new int[] { 1, -1, 1, 0, 1, 1, 0, 1, -1, 1, -1, 0, -1, -1, 0, -1 };
        public List<TCell> Neighbors
        {
            get
            {
                var neighbors = new List<TCell>();
                for (int i = 0; i < neighMask.Length; i += 2)
                {
                    //var x = X + neighMask[i];
                    //var y = Y + neighMask[i + 1];
                    //if (x < 0 || x >= Game.Map.Width) continue;
                    //if (y < 0 || y >= Game.Map.Height) continue;
                    //var neigh = Game.Cells[y, x];
                    neighbors.Add(GetNeighbour(neighMask[i], neighMask[i + 1]));
                }
                return neighbors;
            }
        }

        public TCell GetNeighbour(int offX, int offY)
        {
            //return Game.Cells[X + offX, Y + offY];
            var worldPos = Game.Map.UnTransformGrid(X, Y);
            var mapPos = Game.Map.TransformGrid(worldPos.X + offX, worldPos.Y + offY);
            if (mapPos.X < 0 || mapPos.X >= Game.Map.Width) return null;
            if (mapPos.Y < 0 || mapPos.Y >= Game.Map.Height) return null;
            return Game.Cells[(int)mapPos.Y, (int)mapPos.X];
        }
    }
}