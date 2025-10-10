using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    class DiabloWall : TBlockTile
    {
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
        public byte[] TilesFlags;
        public bool Hidden;
        public void ReadHeader(BinaryReader reader)
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
            int headerFilePos = reader.ReadInt32();
            int headerSize = reader.ReadInt32();
            var tileCount = reader.ReadInt32();
            Tiles.Capacity = tileCount;
            zeros = reader.ReadBytes(12);
        }
        public void ReadTiles(BinaryReader reader)
        {
            for (int i = 0; i < Tiles.Capacity; i++)
            {
                var tile = new DiabloTile();
                tile.ReadHeader(reader);
                Tiles.Add(tile);
            }
            foreach (var tile in Tiles)
                ((DiabloTile)tile).ReadImage(reader);
        }
    }

}
