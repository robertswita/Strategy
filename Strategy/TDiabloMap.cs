using Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Forms;

namespace Strategy
{
    internal class TDiabloMap : TMap
    {
        [Flags]
        enum TMaterial
        {
            Other, Water, WoodObject, InsideStone, OutsideStone, Dirt, Sand, Wood, Lava, Unknown, Snow
        }
        public class TDiabloTile : TTile
        {
            public int GridX, GridY;
            public bool HasRleFormat;
            public int Size;
            public int FileOffset;
        }
        class TDiabloBlockTile : TBlockTile
        {
            public int Direction;
            public int RoofHeight;
            public TMaterial Material;
            public int Width;
            public int Height;
            public int Type; // Orientation
            public int Style; // MainIndex
            public int Seq; // SubIndex
            //public int Sequence;
            public int Rarity;
            public int Unk;
            public byte[] TilesFlags;
            public int BlockHeaderPointer;
            public int BlockHeaderSize;
            public bool Hidden;
            public int ID { get { return Type << 26 | Style << 20 | Seq << 8; } }
        }

        static Color[] Palette;
        static TDiabloMap()
        {
            Palette = new Color[256];
            for (int i = 0; i < 256; i++)
                Palette[i] = Color.FromArgb(i, i, i);
        }

        Vector2 GridOffset;
        int WorldHeight;
        int WorldWidth;
        Dictionary<int, TDiabloBlockTile> BlockTilesHashes = new Dictionary<int, TDiabloBlockTile>();
        public List<TTile> ReadTileSet(string filename, string ext)
        {
            TCell.Width = 32;
            TCell.Height = 16;
            var tiles = new List<TTile>();
            filename = filename.Substring(0, filename.Length - 4) + ext;
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fStream);
            int verMajor = reader.ReadInt32();
            int verMinor = reader.ReadInt32();
            if (verMajor == 7 && verMinor == 6)
            {
                reader.ReadBytes(260);
                int BlockTilesCount = reader.ReadInt32();
                int tileDataStartAdress = reader.ReadInt32();
                reader.BaseStream.Position = tileDataStartAdress;
                var blockTiles = DecodeBlockTiles(reader, BlockTilesCount);
                foreach (var blockTile in blockTiles)
                {
                    tiles.AddRange(ReadBlockTile(reader, blockTile));
                    if (!BlockTilesHashes.Keys.Contains(blockTile.ID))
                        BlockTilesHashes.Add(blockTile.ID, blockTile);
                }
            }
            fStream.Close();
            return tiles;
        }

        List<TDiabloBlockTile> DecodeBlockTiles(BinaryReader reader, int BlockTilesCount)
        {
            var blockTiles = new List<TDiabloBlockTile>();
            for (int i = 0; i < BlockTilesCount; i++)
            {
                var BlockTile = new TDiabloBlockTile();
                BlockTile.Tiles = new List<TTile>();
                BlockTile.Direction = reader.ReadInt32();
                BlockTile.RoofHeight = reader.ReadInt16();
                BlockTile.Material = (TMaterial)reader.ReadInt16();
                BlockTile.Height = reader.ReadInt32();
                BlockTile.Width = reader.ReadInt32();
                var zeros = reader.ReadBytes(4);
                BlockTile.Type = reader.ReadInt32();
                BlockTile.Style = reader.ReadInt32();
                BlockTile.Seq = reader.ReadInt32();
                BlockTile.Rarity = reader.ReadInt32();
                BlockTile.Unk = reader.ReadInt32();
                BlockTile.TilesFlags = reader.ReadBytes(25);
                zeros = reader.ReadBytes(7);
                BlockTile.BlockHeaderPointer = reader.ReadInt32();
                BlockTile.BlockHeaderSize = reader.ReadInt32();
                var tileCount = reader.ReadInt32();
                BlockTile.Tiles.Capacity = tileCount;
                zeros = reader.ReadBytes(12);
                //BlockTilesHashes.Add(BlockTile.ID, BlockTile);
                blockTiles.Add(BlockTile);
            }
            return blockTiles;
        }

        private List<TTile> ReadBlockTile(BinaryReader reader, TDiabloBlockTile BlockTile)
        {
            //var images = new List<Image>();
            if (reader.BaseStream.Position != BlockTile.BlockHeaderPointer)
                ;
            for (int i = 0; i < BlockTile.Tiles.Capacity; i++)
            {
                var tile = new TDiabloTile();
                //tile.BlockTile = BlockTile;
                tile.X = reader.ReadInt16();
                tile.Y = reader.ReadInt16();
                reader.ReadInt16();
                tile.GridX = reader.ReadByte();
                tile.GridY = reader.ReadByte();
                tile.HasRleFormat = reader.ReadInt16() != 1;
                tile.Size = reader.ReadInt32();
                reader.ReadInt16();
                tile.FileOffset = reader.ReadInt32();
                BlockTile.Tiles.Add(tile);
            }
            for (int i = 0; i < BlockTile.Tiles.Count; i++)
            {
                var tile = (TDiabloTile)BlockTile.Tiles[i];
                if (reader.BaseStream.Position != BlockTile.BlockHeaderPointer + tile.FileOffset)
                    ;
                var pixels = reader.ReadBytes(tile.Size);
                tile.Image = tile.HasRleFormat ? DecodeRle(tile, pixels) : DecodeIsometric(pixels);

            }
            return BlockTile.Tiles;
        }

        //public Bitmap DecodeIsometric(byte[] pixels)
        //{
        //    var blockWidth = 32;
        //    var blockHeight = 16;
        //    int pos = 0;
        //    var pixmap = new TPixmap(blockWidth, blockHeight);
        //    for (int y = 0; y < pixmap.Height; y++)
        //    {
        //        var n = y < pixmap.Height / 2 ? y : pixmap.Height - 1 - y;
        //        var r = 1 + 2 * n;
        //        for (int x = pixmap.Width / 2 - r; x < pixmap.Width / 2 + r; x++)
        //        {
        //            var brightness = pixels[pos]; pos++;
        //            pixmap[x, y] = Palette[brightness].ToArgb();
        //        }
        //    }
        //    return pixmap.Image;
        //}

        public Bitmap DecodeIsometric(byte[] pixels)
        {
            //var tile = block.Tile;
            var blockWidth = 32;
            var blockHeight = 15;

            int pos = 0;
            var pixmap = new TPixmap(blockWidth, blockHeight);
            for (int y = 0; y < pixmap.Height; y++)
            {
                var n = y < pixmap.Height / 2 ? y : pixmap.Height - 1 - y;
                var r = 2 + 2 * n;
                for (int x = pixmap.Width / 2 - r; x < pixmap.Width / 2 + r; x++)
                {
                    var brightness = pixels[pos++];
                    pixmap[x, y] = Palette[brightness].ToArgb();
                }
            }
            return pixmap.Image;
        }

        Bitmap DecodeRle(TDiabloTile tile, byte[] pixels)
        {
            var blockWidth = 32;
            var blockHeight = 32;
            int pos = 0;
            var pixmap = new TPixmap(blockWidth, blockHeight);
            for (int y = 0; y < pixmap.Height; y++)
            {
                if (pos >= pixels.Length) break;
                var segBegin = 0;
                var segEnd = 0;
                do
                {
                    segBegin = segEnd;
                    segBegin += pixels[pos]; pos++;
                    segEnd = segBegin;
                    segEnd += pixels[pos]; pos++;
                    for (int x = segBegin; x < segEnd; x++)
                    {
                        var brightness = pixels[pos]; pos++;
                        pixmap[x, y] = Palette[brightness].ToArgb();
                    }
                } while (segEnd > segBegin);
            }
            return pixmap.Image;
        }

        public override Vector2 TransformGrid(float x, float y)
        {
            var v = new Vector2(x - y, y + x) - GridOffset;
            v.Y = (v.Y - ((int)v.X & 1)) / 2;
            return v;
        }
        public override Vector2 UnTransformGrid(float x_, float y_)
        {
            y_ = 2 * y_ + ((int)x_ & 1);
            x_ += GridOffset.X;
            y_ += GridOffset.Y;
            return new Vector2(x_ + y_, y_ - x_) / 2;
        }

        TCell[,] UntransformFromHexMapping()
        {
            var cells = new TCell[Height, Width];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    //var pos = UnTransformGrid(x, 2 * y + (x & 1));
                    var pos = UnTransformGrid(x, y);
                    if (pos.Y >= 0 && pos.Y < WorldHeight && pos.X >= 0 && pos.X < WorldWidth)
                    {
                        var cell = Game.Cells[(int)pos.Y, (int)pos.X];
                        cell.X = x;
                        cell.Y = y;
                        cells[y, x] = cell;
                    }
                    else
                        cells[y, x] = Game.Cells[0, 0];
                }
            return cells;
        }

        TCell[,] TransformToHexMapping()
        {
            var cells = new TCell[WorldHeight, WorldWidth];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    //var pos = UnTransformGrid(x, 2 * y + (x & 1));
                    var pos = UnTransformGrid(x, y);
                    if (pos.Y >= 0 && pos.Y < WorldHeight && pos.X >= 0 && pos.X < WorldWidth)
                    {
                        var cell = Game.Cells[y, x];
                        //cell.X = (int)pos.X;
                        //cell.Y = (int)pos.Y;
                        cells[(int)pos.Y, (int)pos.X] = cell;
                    }
                }
            return cells;
        }

        public void MapTileSet(List<TTile> tiles)
        {
            var blockSize = 5 * TCell.Width;
            int mapSize = (int)Math.Ceiling(Math.Sqrt(BlockTilesHashes.Count * 5)) + 1;
            Height = 5 * mapSize;
            Width = 2 * Height;
            Game.Cells = new TCell[Height, Width];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var cell = new TCell();
                    cell.Game = Game;
                    cell.X = x;
                    cell.Y = y;
                    Game.Cells[y, x] = cell;
                }
            Game.ColumnTiles.Clear();
            for (int n = 0; n < BlockTilesHashes.Count; n++)
            {
                var BlockTile = BlockTilesHashes.ElementAt(n).Value;
                BlockTile.X = (n % mapSize) * blockSize;
                BlockTile.Y = n / mapSize * blockSize;
                BlockTile.Bounds = new Rectangle(BlockTile.X, BlockTile.Y, blockSize, blockSize);
                Game.ColumnTiles.Add(BlockTile);
            }
        }

        public enum TVersion
        {
            HasFiles = 3,
            HasWalls = 4,
            HasAct = 8,
            HasUnknownBytes1Low = 9,
            HasSubtitutionLayers = 10,
            HasSubtitutionGroups = 12,
            HasUnknownBytes1High = 13,
            HasNpcs = 14,
            HasNpcActions = 15,
            HasFloors = 16,
            HasUnknownBytes2 = 18,
        }

        public enum LayerStreamType
        {
            LayerWall1,
            LayerWall2,
            LayerWall3,
            LayerWall4,
            LayerOrientation1,
            LayerOrientation2,
            LayerOrientation3,
            LayerOrientation4,
            LayerFloor1,
            LayerFloor2,
            LayerShadow,
            LayerSubstitute
        }


        public static string ReadZString(BinaryReader reader)
        {
            var result = "";
            var chr = reader.ReadChar();
            while (chr != '\0')
            {
                result += chr;
                chr = reader.ReadChar();
            }
            return result;
        }

        public void LoadPalette(string filename)
        {
            var dirName = filename;
            while (dirName != null && Path.GetFileName(dirName) != "d2")
                dirName = Path.GetDirectoryName(dirName);
            if (dirName == null) dirName = filename;
            GamePath = Path.GetDirectoryName(dirName) + "\\";
            var actName = Path.GetDirectoryName(Path.GetDirectoryName(filename));
            actName = Path.GetFileName(actName) + "\\";
            var palPath = GamePath + "D2\\Data\\Global\\Palette\\" + actName + "Pal.pl2";
            var fStream = new FileStream(palPath, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fStream);
            Palette = new Color[256];
            for (int i = 0; i < Palette.Length; i++)
            {
                var r = reader.ReadByte();
                var g = reader.ReadByte();
                var b = reader.ReadByte();
                var a = reader.ReadByte();
                Palette[i] = Color.FromArgb(r, g, b);
            }
            fStream.Close();
        }

        int ActNo;
        int SubstitutionType;
        public override void ReadMap(string filename)
        {
            Cursor.Current = Cursors.WaitCursor;
            MapName = Path.GetFileNameWithoutExtension(filename);
            LoadPalette(filename);
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fStream);
            var version = (TVersion)reader.ReadInt32();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Width++;
            Height++;
            WorldWidth = Width * 5;
            WorldHeight = Height * 5;
            int diagonalSize = WorldWidth + WorldHeight;
            var mapSize = new Vector2(diagonalSize, diagonalSize);
            GridOffset.X = -WorldWidth;

            if (version >= TVersion.HasAct)
                ActNo = 1 + reader.ReadInt32();
            if (version >= TVersion.HasSubtitutionLayers)
                SubstitutionType = reader.ReadInt32();
            if (version >= TVersion.HasFiles)
            {
                Game.GroundTiles = new List<TTile>();
                int filesCount = reader.ReadInt32();
                for (int i = 0; i < filesCount; i++)
                {
                    var fileName = ReadZString(reader);
                    if (fileName.StartsWith("C:\\"))
                        fileName = fileName.Substring(3);
                    //string fileName = Encoding.ASCII.GetString(bytes.ToArray());
                    fileName = GamePath + fileName;
                    Game.GroundTiles.AddRange(ReadTileSet(fileName, ".dt1"));
                }
            }
            if (version >= TVersion.HasUnknownBytes1Low && version <= TVersion.HasUnknownBytes1High)
                reader.ReadBytes(8);
            var layers = new List<LayerStreamType>();
            if (version < TVersion.HasWalls)
            {
                layers.Add(LayerStreamType.LayerWall1);
                layers.Add(LayerStreamType.LayerFloor1);
                layers.Add(LayerStreamType.LayerOrientation1);
                layers.Add(LayerStreamType.LayerSubstitute);
                layers.Add(LayerStreamType.LayerShadow);
            }
            else
            {
                var wallsCount = reader.ReadInt32();
                for (int i = 0; i < wallsCount; i++)
                {
                    layers.Add(LayerStreamType.LayerWall1 + i);
                    layers.Add(LayerStreamType.LayerOrientation1 + i);
                }
                var floorsCount = version < TVersion.HasFloors ? 1 : reader.ReadInt32();
                for (int i = 0; i < floorsCount; i++)
                    layers.Add(LayerStreamType.LayerFloor1 + i);
                layers.Add(LayerStreamType.LayerShadow);
                if (SubstitutionType == 1 || SubstitutionType == 2)
                    layers.Add(LayerStreamType.LayerSubstitute);
            }

            Game.Cells = new TCell[WorldHeight, WorldWidth];
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var cell = new TCell();
                    cell.Game = Game;
                    cell.X = x;
                    cell.Y = y;
                    Game.Cells[y, x] = cell;
                }
            LoadLayers(reader, layers);
            //LoadObjects(reader);
            //LoadSubstitutions(reader);
            //LoadNPCs(reader);

            reader.Close();
            Height = (int)(mapSize.Y / 2);
            Width = (int)mapSize.X;
            Game.Cells = UntransformFromHexMapping();
            Cursor.Current = Cursors.Default;
        }

        public void LoadLayers(BinaryReader reader, List<LayerStreamType> layers)
        {
            var typeLookup = new int[]{
                0x00, 0x01, 0x02, 0x01, 0x02, 0x03, 0x03, 0x05, 0x05, 0x06,
                0x06, 0x07, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E,
                0x0F, 0x10, 0x11, 0x12, 0x14,};

            Game.ColumnTiles.Clear();
            var walls = new List<TDiabloBlockTile>[4];
            for (int i = 0; i < walls.Length; i++)
                walls[i] = new List<TDiabloBlockTile>();
            for (int i = 0; i < layers.Count; i++)
            {
                var layerStreamType = layers[i];
                var layerIndex = (int)layerStreamType - (int)LayerStreamType.LayerOrientation1;
                var wallIdx = 0;
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                    {
                        //var cell = Game.Cells[5 * y, 5 * x];
                        var tileInfo = reader.ReadInt32();
                        if (tileInfo == 0) continue;
                        var blockTile = new TDiabloBlockTile();
                        blockTile.Seq = tileInfo >> 8 & 0x3F;
                        blockTile.Style = tileInfo >> 20 & 0x3F;
                        blockTile.Hidden = tileInfo < 0;
                        //blockTile.Property1 = tileInfo & 0xFF;
                        //blockTile.Unk1 = tileInfo >> 14 & 0x3F;
                        //blockTile.Unk2 = tileInfo >> 26 & 0x1F;
                        switch (layerStreamType)
                        {
                            case LayerStreamType.LayerWall1:
                            case LayerStreamType.LayerWall2:
                            case LayerStreamType.LayerWall3:
                            case LayerStreamType.LayerWall4:
                                {
                                    Game.ColumnTiles.Add(blockTile);
                                    var wallLayerIndex = layerStreamType - LayerStreamType.LayerWall1;
                                    walls[wallLayerIndex].Add(blockTile);
                                    break;
                                }
                            case LayerStreamType.LayerOrientation1:
                            case LayerStreamType.LayerOrientation2:
                            case LayerStreamType.LayerOrientation3:
                            case LayerStreamType.LayerOrientation4:
                                {
                                    var wallLayerIndex = layerStreamType - LayerStreamType.LayerOrientation1;
                                    var wall = walls[wallLayerIndex][wallIdx]; wallIdx++;
                                    wall.Type = tileInfo;
                                    if (ActNo == 0 && wall.Type < typeLookup.Length)
                                        wall.Type = typeLookup[wall.Type];
                                    BlockTilesHashes.TryGetValue(wall.ID, out blockTile);
                                    if (blockTile != null)
                                    {
                                        wall.Tiles = blockTile.Tiles;
                                        var pos = TransformGrid(5 * x, 5 * y);
                                        var cell = new TCell();
                                        cell.X = (int)pos.X;
                                        cell.Y = (int)pos.Y;
                                        pos = cell.Position;
                                        wall.X = (int)(pos.X - 2 * TCell.Width);
                                        wall.Y = (int)(pos.Y + 5 * TCell.Height);
                                        wall.Bounds = new Rectangle(wall.X, wall.Y, wall.Width, wall.Height);
                                    }
                                    break;
                                }

                            case LayerStreamType.LayerFloor1:
                            case LayerStreamType.LayerFloor2:
                                {
                                    var floorIndex = layerStreamType - LayerStreamType.LayerFloor1;
                                    BlockTilesHashes.TryGetValue(blockTile.ID, out blockTile);
                                    if (blockTile != null)
                                        for (int u = 0; u < 5; u++)
                                            for (int v = 0; v < 5; v++)
                                            {
                                                var cellTile = Game.Cells[5 * y + u, 5 * x + v];
                                                cellTile.GroundTile = blockTile.Tiles[(4 - u) * 5 + 4 - v];
                                            }
                                    break;
                                }

                            case LayerStreamType.LayerShadow:
                                {
                                    break;
                                }

                            case LayerStreamType.LayerSubstitute:
                                {
                                    break;
                                }
                        }
                    }
            }
        }
    }

}
