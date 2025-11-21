using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Strategy.Dispel
{
    class TDispelWall : TWall
    {
        public TDispelMap Map { get { return (TDispelMap)Collect.Owner; } }
        public override void Read(BinaryReader reader)
        {
            reader.ReadInt32(); // = 1
            reader.ReadBytes(260);
            Id = reader.ReadInt32();
            var viewsCount = reader.ReadInt32();
            for (int j = 0; j < viewsCount; j++)
            {
                reader.ReadInt32();
                var framesCount = reader.ReadInt64();
                for (int k = 0; k < framesCount; k++)
                {
                    int left = reader.ReadInt32();
                    int top = reader.ReadInt32();
                    int right = reader.ReadInt32();
                    int bottom = reader.ReadInt32();
                    X = reader.ReadInt32();// - 1 * TDispelTile.Width;
                    Y = reader.ReadInt32();// + TDispelTile.Height;
                    var x_ = reader.ReadInt32();
                    var y_ = reader.ReadInt32();
                    reader.ReadInt32(); // = 1
                    int tilesCount = reader.ReadInt32();
                    reader.ReadInt32(); // = tilesCount
                    Order = Map.Walls.Count;
                    Bounds = Rectangle.FromLTRB(left, top, right, bottom);
                    for (int m = 0; m < tilesCount; m++)
                    {
                        var tile = Map.WallTiles[reader.ReadInt16()];
                        tile.Y = m * TDispelTile.Height;
                        Tiles.Add(tile);
                    }
                }
            }
            for (int m = 0; m < 2 * Tiles.Count; m++)
                reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(new byte[260]);
            writer.Write(Id);
            writer.Write(1); // viewsCount
            writer.Write(0);
            writer.Write(1); // framesCount
            writer.Write(0);
            writer.Write(Bounds.Left);
            writer.Write(Bounds.Top);
            writer.Write(Bounds.Right);
            writer.Write(Bounds.Bottom);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Tiles[0].X);
            writer.Write(Tiles[0].Y);
            writer.Write(1);
            writer.Write(Tiles.Count);
            writer.Write(Tiles.Count);
            var props = new List<int>();
            for (int m = 0; m < Tiles.Count; m++)
            {
                var idx = (short)Tiles[m].Index;
                writer.Write(idx);
                props.Add(Map.BTilesProps[2 * idx]);
                props.Add(Map.BTilesProps[2 * idx + 1]);
            }
            for (int m = 0; m < props.Count; m++)
                writer.Write(props[m]);
        }
    }
}
