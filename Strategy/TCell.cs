using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace Strategy
{
    public class TCell
    {
        public int X;
        public int Y;
        public Vector2 Position { get { return Map.Map2ViewTransform(X, Y); } }
        public Rectangle Bounds;
        //{ 
        //    get 
        //    {
        //        var tileSize = new Point(Game.Map.TileWidth, Game.Map.TileHeight);
        //        var pos = Position;
        //        return new Rectangle((int)pos.X - tileSize.X / 2, (int)pos.Y - tileSize.Y / 2, tileSize.X, tileSize.Y);
        //    } 
        //}
        public TMap Map;
        public TTile Floor;
        public TWall Wall;
        public TTile Roof;
        public TTile Piece;
        public TNpc Npc;
        public int EventIdx;
        public bool Collision;
        public bool IsVisible;
        public TCell Parent;
        public Bitmap CollisionMask;
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
            var worldPos = Map.Map2WorldTransform(X, Y);
            var mapPos = Map.World2MapTransform(worldPos.X + offX, worldPos.Y + offY);
            if (mapPos.X < 0 || mapPos.X >= Map.Width) return null;
            if (mapPos.Y < 0 || mapPos.Y >= Map.Height) return null;
            return Map.Cells[(int)mapPos.Y, (int)mapPos.X];
        }
    }
}