using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Windows.Forms;

namespace Strategy
{
    class TDiabloMap : TMap
    {
        class TDiabloTile: TTile
        {
            public bool HasRleFormat;
            public int Size;
            public void ReadHeader(BinaryReader reader)
            {
                X = reader.ReadInt16();
                Y = reader.ReadInt16();
                var zeros = reader.ReadInt16();
                var gridX = reader.ReadByte();
                var gridY = reader.ReadByte();
                HasRleFormat = reader.ReadInt16() != 1;
                Size = reader.ReadInt32();
                zeros = reader.ReadInt16();
                var headerSize = reader.ReadInt32();
            }
            public override void ReadImage(BinaryReader reader)
            {
                var pixels = reader.ReadBytes(Size);
                Image = HasRleFormat ? DecodeRle(pixels) : DecodeIsometric(pixels);
            }
            Bitmap DecodeIsometric(byte[] pixels)
            {
                int pos = 0;
                var pixmap = new TPixmap(TCell.Width, TCell.Height - 1);
                for (int y = 0; y < pixmap.Height; y++)
                {
                    var n = y < pixmap.Height / 2 ? y : pixmap.Height - 1 - y;
                    var r = 2 + 2 * n;
                    for (int x = pixmap.Width / 2 - r; x < pixmap.Width / 2 + r; x++)
                    {
                        pixmap[x, y] = Palette[pixels[pos]]; pos++;
                    }
                }
                return pixmap.Image;
            }
            Bitmap DecodeRle(byte[] pixels)
            {
                int pos = 0;
                var pixmap = new TPixmap(TCell.Width, TCell.Width);
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
                            pixmap[x, y] = Palette[pixels[pos]]; pos++;
                        }
                    } while (segEnd > segBegin);
                }
                return pixmap.Image;
            }
        }

        [Flags] enum TMaterial { Other, Water, WoodObject, InsideStone, OutsideStone, Dirt, Sand, Wood, Lava, Unknown, Snow }
        enum TWallType { 
            Floor, LeftRight, TopBottom, LeftTopCorner_Top, LeftTopCorner_Left, 
            RightTopCorner, LeftBottomCorner, RightBottomCorner, LeftRightDoor, TopBottomDoor,
            Special1, Special2, Special3, Shadow, ShadowObject,
            Roof, LowerWall
        }
        class TDiabloBlockTile : TBlockTile
        {
            public int Direction;
            public int RoofHeight;
            public TMaterial Material;
            public int Width;
            public int Height;
            public TWallType Type; // Orientation
            public int Style; // MainIndex
            public int Seq; // SubIndex
            //public int Sequence;
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
                    var tile = new TDiabloTile();
                    tile.ReadHeader(reader);
                    Tiles.Add(tile);
                }
                foreach (var tile in Tiles)
                    ((TDiabloTile)tile).ReadImage(reader);
            }
        }

        static int[] Palette;
        static TDiabloMap()
        {
            TCell.Width = 32;
            TCell.Height = 16;
            Palette = new int[256];
            for (int i = 0; i < 256; i++)
                Palette[i] = Color.FromArgb(i, i, i).ToArgb();
        }

        Vector2 GridOffset;
        int WorldHeight;
        int WorldWidth;
        public void ReadTileSet(string filename, string ext)
        {
            filename = filename.Substring(0, filename.Length - 4) + ext;
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fStream);
            int verMajor = reader.ReadInt32();
            int verMinor = reader.ReadInt32();
            if (verMajor == 7 && verMinor == 6)
            {
                reader.ReadBytes(260);
                int blockTilesCount = reader.ReadInt32();
                int filePos = reader.ReadInt32();
                var blockTiles = new List<TDiabloBlockTile>();
                for (int i = 0; i < blockTilesCount; i++)
                {
                    var blockTile = new TDiabloBlockTile();
                    blockTile.ReadHeader(reader);
                    blockTiles.Add(blockTile);
                    Game.Walls.Add(blockTile);
                }
                foreach (var blockTile in blockTiles)
                    blockTile.ReadTiles(reader);
            }
            fStream.Close();
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
                    //else
                    //    cells[y, x] = Game.Cells[0, 0];
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
            var blockSize = 10 * TCell.Width;
            int mapSize = (int)Math.Ceiling(Math.Sqrt(Game.Walls.Count * 5)) + 1;
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
            for (int idx = 0; idx < Game.Walls.Count; idx++)
            {
                var blockTile = Game.Walls[idx];
                blockTile.X = (idx % mapSize) * blockSize;
                blockTile.Y = idx / mapSize * blockSize;
                blockTile.Bounds = new Rectangle(blockTile.X, blockTile.Y - blockSize, blockSize, blockSize);
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

        public void ReadPalette(string filename)
        {
            var dirName = filename;
            while (dirName != null && Path.GetFileName(dirName) != "d2")
                dirName = Path.GetDirectoryName(dirName);
            if (dirName == null) dirName = filename;
            GamePath = Path.GetDirectoryName(dirName);
            var actName = Path.GetDirectoryName(Path.GetDirectoryName(filename));
            actName = Path.GetFileName(actName);
            if (!actName.StartsWith("ACT"))
                actName = "Units";
            var palPath = GamePath + "\\D2\\Data\\Global\\Palette\\" + actName + "\\pal.dat";
            var fStream = new FileStream(palPath, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fStream);
            Palette = new int[256];
            for (int i = 0; i < Palette.Length; i++)
            {
                var b = reader.ReadByte();
                var g = reader.ReadByte();
                var r = reader.ReadByte();
                //var a = reader.ReadByte();
                //if (r + g + b > 0) a = 255;
                Palette[i] = Color.FromArgb(r, g, b).ToArgb();
            }
            fStream.Close();
        }

        int ActNo;
        int SubstitutionType;
        TVersion Version;
        public override void ReadMap(string filename)
        {
            Cursor.Current = Cursors.WaitCursor;
            MapName = Path.GetFileNameWithoutExtension(filename);
            ReadPalette(filename);
            Game.Walls.Clear();
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fStream);
            Version = (TVersion)reader.ReadInt32();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Width++;
            Height++;
            WorldWidth = Width * 5;
            WorldHeight = Height * 5;
            GridOffset.X = -WorldHeight;
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
            ReadCells(reader);
            ReadObjects(reader);
            //LoadSubstitutions(reader);
            //LoadNPCs(reader);
            reader.Close();
            Width = WorldWidth + WorldHeight;
            Height = Width / 2;
            Game.Cells = UntransformFromHexMapping();
            Game.Board.ScrollPos = new PointF(0.5f, 0.5f);
            Cursor.Current = Cursors.Default;
        }

        void ReadWalls(BinaryReader reader, List<TDiabloBlockTile>[,,] blockTiles)
        {
            var typeLookup = new int[]{
                0x00, 0x01, 0x02, 0x01, 0x02, 0x03, 0x03, 0x05, 0x05, 0x06,
                0x06, 0x07, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E,
                0x0F, 0x10, 0x11, 0x12, 0x14,};
            var walls = new List<TDiabloBlockTile>();
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var tileInfo = reader.ReadInt32();
                    if (tileInfo == 0) continue;
                    var blockTile = new TDiabloBlockTile();
                    blockTile.Seq = tileInfo >> 8 & 0x3F;
                    blockTile.Style = tileInfo >> 20 & 0x3F;
                    blockTile.Hidden = tileInfo < 0;
                    //blockTile.Rarity = tileInfo & 0x3F;
                    //blockTile.Property1 = tileInfo & 0xFF;
                    //blockTile.Unk1 = tileInfo >> 14 & 0x3F;
                    //blockTile.Unk2 = tileInfo >> 26 & 0x1F;
                    walls.Add(blockTile);
                }
            if (Version < TVersion.HasWalls)
                ReadFloors(reader, blockTiles);
            var wallIdx = 0;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var wallType = reader.ReadInt32();
                    if (wallType == 0) continue;
                    var wall = walls[wallIdx]; wallIdx++;
                    if (ActNo == 0 && wallType < typeLookup.Length)
                        wallType = typeLookup[wallType];
                    wall.Type = (TWallType)wallType;
                    var selection = blockTiles[wallType, wall.Style, wall.Seq];
                    if (selection != null)
                    {
                        wall.Rarity = TGame.Random.Next(selection.Count);
                        var wallTile = selection[wall.Rarity];
                        wall.Tiles = wallTile.Tiles;
                        var pos = TransformGrid(5 * x, 5 * y);
                        var cell = new TCell();
                        cell.X = (int)pos.X;
                        cell.Y = (int)pos.Y;
                        pos = cell.Position;
                        wall.X = (int)(pos.X - 2 * TCell.Width);
                        wall.Y = (int)pos.Y;
                        if (wall.Type == TWallType.Roof)
                        {
                            wall.Y -= wallTile.RoofHeight;
                            Game.RoofTiles.Add(wall);
                        }
                        else
                        {
                            if (wall.Type < TWallType.Roof)
                                wall.Y += 5 * TCell.Height;
                            Game.Walls.Add(wall);
                        }
                        wall.Bounds = new Rectangle(wall.X, wall.Y + wallTile.Height, wallTile.Width, Math.Abs(wallTile.Height));
                        if (wall.Type == TWallType.LeftTopCorner_Top)
                        {
                            var extraTile = new TDiabloBlockTile();
                            extraTile.Type = TWallType.LeftTopCorner_Left;
                            extraTile.Style = wall.Style;
                            extraTile.Seq = wall.Seq;
                            selection = blockTiles[(int)extraTile.Type, extraTile.Style, extraTile.Seq];
                            extraTile.Tiles = selection[wall.Rarity].Tiles;
                            extraTile.X = wall.X;
                            extraTile.Y = wall.Y;
                            extraTile.Bounds = wall.Bounds;
                            Game.Walls.Add(extraTile);
                        }
                    }
                }
        }

        void ReadFloors(BinaryReader reader, List<TDiabloBlockTile>[,,] blockTiles)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var tileInfo = reader.ReadInt32();
                    if (tileInfo == 0) continue;
                    var floor = new TDiabloBlockTile();
                    floor.Seq = tileInfo >> 8 & 0x3F;
                    floor.Style = tileInfo >> 20 & 0x3F;
                    floor.Hidden = tileInfo < 0;
                    var selection = blockTiles[(int)floor.Type, floor.Style, floor.Seq];
                    if (selection != null)
                    {
                        floor = selection[TGame.Random.Next(selection.Count)];
                        for (int u = 0; u < 5; u++)
                            for (int v = 0; v < 5; v++)
                            {
                                var cell = Game.Cells[5 * y + u, 5 * x + v];
                                var tileIdx = (4 - u) * 5 + 4 - v;
                                if (tileIdx < floor.Tiles.Count)
                                    cell.GroundTile = floor.Tiles[tileIdx];
                            }
                    }
                }
        }

        void ReadShadows(BinaryReader reader)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var tileInfo = reader.ReadInt32();
                    //if (tileInfo == 0) continue;
                }
        }

        void ReadSubstitutions(BinaryReader reader)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var tileInfo = reader.ReadInt32();
                    //if (tileInfo == 0) continue;
                }
        }

        void ReadCells(BinaryReader reader)
        {
            if (Version >= TVersion.HasAct)
                ActNo = 1 + reader.ReadInt32();
            if (Version >= TVersion.HasSubtitutionLayers)
                SubstitutionType = reader.ReadInt32();
            if (Version >= TVersion.HasFiles)
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
                    //Game.GroundTiles.AddRange(ReadTileSet(fileName, ".dt1"));
                    ReadTileSet(fileName, ".dt1");
                }
            }
            var blockTiles = new List<TDiabloBlockTile>[64, 64, 64];
            foreach (var blockTile in Game.Walls)
            {
                var dbTile = (TDiabloBlockTile)blockTile;
                if (blockTiles[(int)dbTile.Type, dbTile.Style, dbTile.Seq] == null)
                    blockTiles[(int)dbTile.Type, dbTile.Style, dbTile.Seq] = new List<TDiabloBlockTile>();
                blockTiles[(int)dbTile.Type, dbTile.Style, dbTile.Seq].Add(dbTile);
            }
            Game.Walls.Clear();
            if (Version >= TVersion.HasUnknownBytes1Low && Version <= TVersion.HasUnknownBytes1High)
                reader.ReadBytes(8);
            if (Version < TVersion.HasWalls)
            {
                ReadWalls(reader, blockTiles);
                ReadSubstitutions(reader);
                ReadShadows(reader);
            }
            else
            {
                var wallsLayersCount = reader.ReadInt32();
                var floorsLayersCount = Version < TVersion.HasFloors ? 1 : reader.ReadInt32();
                for (int i = 0; i < wallsLayersCount; i++)
                    ReadWalls(reader, blockTiles);
                for (int i = 0; i < floorsLayersCount; i++)
                    ReadFloors(reader, blockTiles);
                ReadShadows(reader);
                if (SubstitutionType == 1 || SubstitutionType == 2)
                    ReadSubstitutions(reader);
            }
        }
        void ReadObjects(BinaryReader reader)
        {
            var objCount = reader.ReadInt32();
            for (int i = 0; i < objCount; i++)
            {
                var obj = new TElement();
                obj.Type = (TElementType)reader.ReadInt32();
                obj.Id = reader.ReadInt32();
                obj.X = reader.ReadInt32();
                obj.Y = reader.ReadInt32();
                obj.EventId = reader.ReadInt32();
                obj.Bounds = new Rectangle(obj.X, obj.Y, TCell.Width, TCell.Height);
                //Game.Sprites.Add(obj);
            }
        }
    }

}
