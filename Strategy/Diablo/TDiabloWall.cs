using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Strategy.Diablo
{
    [Flags] enum TMaterial { Other, Water, WoodObject, InsideStone, OutsideStone, Dirt, Sand, Wood, Lava, Unknown, Snow }
    enum TWallType
    {
        Floor, LeftRight, TopBottom, LeftTopCorner_Top, LeftTopCorner_Left,
        RightTopCorner, LeftBottomCorner, RightBottomCorner, LeftRightDoor, TopBottomDoor,
        Special1, Special2, Special3, Shadow, ShadowObject,
        Roof, LowerWall
    }
    class TDiabloWall : TWall
    {
        public int TileInfo;
        public int Direction;
        public int RoofHeight;
        public TMaterial Material;
        public int Width;
        public int Height;
        public TWallType Type; // Orientation
        public int Style; // MainIndex
        public int Seq; // SubIndex
        public int Rarity;
        public int Unk;
        public byte[] TilesFlags = new byte[25];
        public bool Hidden;
        public int EncodedSize;
        public int ReadHeader(BinaryReader reader)
        {
            Direction = reader.ReadInt32();
            RoofHeight = reader.ReadInt16();
            Material = (TMaterial)reader.ReadInt16();
            Height = reader.ReadInt32();
            Width = reader.ReadInt32();
            var zeros = reader.ReadBytes(4);
            Type = (TWallType)reader.ReadInt32();
            Style = reader.ReadInt32();
            Seq = reader.ReadInt32();
            Rarity = reader.ReadInt32();
            Unk = reader.ReadInt32();
            TilesFlags = reader.ReadBytes(25);
            zeros = reader.ReadBytes(7);
            int tilesHeaderPos = reader.ReadInt32();
            EncodedSize = reader.ReadInt32();
            var tileCount = reader.ReadInt32();
            Tiles.Capacity = tileCount;
            zeros = reader.ReadBytes(12);
            return tilesHeaderPos;
        }
        public void WriteHeader(BinaryWriter writer, int tilesHeaderPos)
        {
            writer.Write(Direction);
            writer.Write((short)RoofHeight);
            writer.Write((short)Material);
            writer.Write(Height);
            writer.Write(Width);
            writer.Write(new byte[4]);
            writer.Write((int)Type);
            writer.Write(Style);
            writer.Write(Seq);
            writer.Write(Rarity);
            writer.Write(Unk);
            writer.Write(TilesFlags);
            writer.Write(new byte[7]);
            writer.Write(tilesHeaderPos);
            EncodedSize = 20 * Tiles.Count;
            for (int j = 0; j < Tiles.Count; j++)
            {
                var tile = (TDiabloTile)Tiles[j];
                tile.Encode();
                EncodedSize += tile.EncodedPixels.Length;
            }
            writer.Write(EncodedSize);
            writer.Write(Tiles.Count);
            writer.Write(new byte[12]);
        }
        public void ReadTiles(BinaryReader reader)
        {
            for (int i = 0; i < Tiles.Capacity; i++)
            {
                var tile = new TDiabloTile();
                tile.ReadHeader(reader);
                var dataFileOffset = reader.ReadInt32();
                Tiles.Add(tile);
                var tileBounds = new Rectangle(tile.X, tile.Y, TDiabloTile.Width, TDiabloTile.Height);
                if (tile.HasRleFormat)
                    tileBounds.Height *= 2;
                Bounds = Rectangle.Union(Bounds, tileBounds);
            }
            foreach (var tile in Tiles)
                ((TDiabloTile)tile).ReadImage(reader);
        }

        public void WriteTiles(BinaryWriter writer)
        {
            var dataFileOffset = Tiles.Count * 20;
            for (int i = 0; i < Tiles.Count; i++)
            {
                var tile = (TDiabloTile)Tiles[i];
                tile.WriteHeader(writer);
                writer.Write(dataFileOffset);
                dataFileOffset += tile.EncodedPixels.Length;
            }
            foreach (var tile in Tiles)
                ((TDiabloTile)tile).WriteImage(writer);
        }
    }

}
