using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using System.Text;
using System.Collections;
using Strategy.Diablo;
using System.Drawing.Imaging;
using System.Reflection;
using Strategy.Dispel;

namespace Strategy
{
    class TDiabloMap : TMap
    {
        public override int TileWidth { get { return TDiabloTile.Width; } }
        public override int TileHeight { get { return TDiabloTile.Height; } }
        // colormaps index for COF effect
        public enum CofPalettes { Base = 0, Trans25 = 53, Trans50 = 309, Trans75 = 565, Alpha = 821, Luminance = 1077, AlphaBright = 1461};
        public static int[][] Palettes = new int[7][];


        public static int[] Palette;
        public static int[] Palette332;
        Vector2 GridOffset;
        int WorldHeight;
        int WorldWidth;
        string BasePath;
        static TDiabloMap()
        {
            Palette = CreateRGB332();
        }

        //public static int Rgb8To32(int byte0, int byte1)
        //{
        //    int color = (byte0 & 0x1F) << 3 | (byte0 & 0xE0) << 5;
        //    color |= (byte1 & 7) << 13 | (byte1 & 0xF8) << 16;
        //    if (color > 0) color |= unchecked((int)0xFF000000);
        //    return color;
        //}

        public static int[] CreateRGB332()
        {
            var palette = new int[256];
            for (int r = 0; r < 8; r++)
                for (int g = 0; g < 8; g++)
                    for (int b = 0; b < 4; b++)
                    {
                        var R = r << 5 | r << 2 | r >> 1;
                        var G = g << 5 | g << 2 | g >> 1;
                        var B = b << 6 | b << 4 | b << 2 | b;
                        palette[r * 32 + g * 4 + b] = Color.FromArgb(R, G, B).ToArgb();
                    }
            return palette;
        }

        public static int Rgb32To8(int color)
        {
            return (color & 0xC0) >> 6 | (color & 0xE000) >> 11 | (color & 0xE00000) >> 16;
        }

        public void ReadTileSet(string filename, string ext)
        {
            filename = filename.Substring(0, filename.Length - 4) + ext;
            var fStream = new FileStream(filename, FileMode.Open);
            var reader = new BinaryReader(fStream);
            int verMajor = reader.ReadInt32();
            int verMinor = reader.ReadInt32();
            if (verMajor == 7 && verMinor == 6)
            {
                reader.ReadBytes(260);
                int blockTilesCount = reader.ReadInt32();
                int filePos = reader.ReadInt32();
                var blockTiles = new List<TDiabloWall>();
                for (int i = 0; i < blockTilesCount; i++)
                {
                    var blockTile = new TDiabloWall();
                    blockTile.ReadHeader(reader);
                    blockTiles.Add(blockTile);
                    Walls.Add(blockTile);
                }
                foreach (var blockTile in blockTiles)
                    blockTile.ReadTiles(reader);
            }
            fStream.Close();
        }

        public void WriteTileSet(string filename, string ext)
        {
            filename = filename.Substring(0, filename.Length - 4) + ext;
            var fStream = new FileStream(filename, FileMode.Create);
            var writer = new BinaryWriter(fStream);
            writer.Write(7);
            writer.Write(6);
            writer.Write(new byte[260]);
            writer.Write(Walls.Count);
            writer.Write(276);
            for (int i = 0; i < Walls.Count; i++)
                (Walls[i] as TDiabloWall).WriteHeader(writer);
            for (int i = 0; i < Walls.Count; i++)
                (Walls[i] as TDiabloWall).WriteTiles(writer);
            fStream.Close();
        }


        public override Vector2 World2ViewTransform(float x, float y)
        {
            var v = new Vector2(x - y, y + x) - GridOffset;
            return new Vector2(v.X * TDiabloTile.Width / 2, v.Y * TDiabloTile.Height / 2);
        }
        public override Vector2 View2MapTransform(float x, float y)
        {
            x /= TDiabloTile.Width / 2;
            y /= TDiabloTile.Height / 2;
            return new Vector2(x, (y - ((int)x & 1)) / 2);
        }
        public override Vector2 Map2WorldTransform(float x, float y)
        {
            y = 2 * y + ((int)x & 1);
            x += GridOffset.X;
            y += GridOffset.Y;
            return new Vector2(x + y, y - x) / 2;
        }

        TCell[,] UntransformFromHexMapping()
        {
            var cells = new TCell[Height, Width];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var pos = Map2WorldTransform(x, y);
                    if (pos.Y >= 0 && pos.Y < WorldHeight && pos.X >= 0 && pos.X < WorldWidth)
                    {
                        var cell = Cells[(int)pos.Y, (int)pos.X];
                        cell.X = x;
                        cell.Y = y;
                        var cellPos = cell.Position;
                        cell.Bounds = new Rectangle((int)cellPos.X, (int)cellPos.Y, TDiabloTile.Width, TDiabloTile.Height);
                        cell.Bounds.Offset(-TDiabloTile.Width / 2, -TDiabloTile.Height / 2);
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
                    var pos = Map2WorldTransform(x, y);
                    if (pos.Y >= 0 && pos.Y < WorldHeight && pos.X >= 0 && pos.X < WorldWidth)
                    {
                        var cell = Cells[y, x];
                        //cell.X = (int)pos.X;
                        //cell.Y = (int)pos.Y;
                        cells[(int)pos.Y, (int)pos.X] = cell;
                    }
                }
            return cells;
        }

        public void MapTileSet(List<TTile> tiles)
        {
            var blockSize = 10 * TDiabloTile.Width;
            int mapSize = (int)Math.Ceiling(Math.Sqrt(Walls.Count * 5)) + 1;
            Height = 5 * mapSize;
            Width = 2 * Height;
            Cells = new TCell[Height, Width];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var cell = new TCell();
                    cell.Map = this;
                    cell.X = x;
                    cell.Y = y;
                    Cells[y, x] = cell;
                }
            for (int idx = 0; idx < Walls.Count; idx++)
            {
                var blockTile = Walls[idx];
                blockTile.X = (idx % mapSize) * blockSize;
                blockTile.Y = idx / mapSize * blockSize;
                blockTile.Bounds = new Rectangle(blockTile.X, blockTile.Y - blockSize, blockSize, blockSize);
            }
        }

        public void MapAnimation(string filename)
        {
            ReadPalette(filename);
            var anim = new TDiabloAnimation();
            anim.BasePath = Path.GetDirectoryName(Path.GetDirectoryName(BasePath));
            filename = Path.GetFileName(filename);
            anim.Name = filename;
            anim.Token = filename.Substring(0, 2);
            anim.Mode = filename.Substring(2, 2);
            anim.ClassType = filename.Substring(4, filename.Length - 8);
            anim.ReadCof();
            Animations.Add(anim);
            var posX = 0;
            var posY = 0;
            for (var j = 0; j < anim.Sequences.Count; j++)
            {
                var sequence = anim.Sequences[j];
                posX = 0;
                for (var k = 0; k < sequence.Length; k++)
                {
                    var sprite = new TSprite();
                    sprite.Animation = anim;
                    sprite.Sequence = j;
                    sprite.ViewAngle = k;
                    sprite.Flipped = false;
                    var width = sprite.ActFrame.Bounds.Width;
                    var height = sprite.ActFrame.Bounds.Height;
                    sprite.X = posX;
                    sprite.Y = posY;
                    posX += 2 * width;
                    sprite.Bounds = new Rectangle(sprite.X, sprite.Y, width, height);
                    Sprites.Add(sprite);
                }
                posY += 2 * sequence[0][0].Bounds.Height;
            }
            Width = 2 * posX / TDiabloTile.Width + 4;
            Height = 2 * posY / TDiabloTile.Height + 2;
            Cells = new TCell[Height, Width];
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
            Final = 20
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
            MapName = Path.GetFileNameWithoutExtension(filename);
            var dirName = Path.GetDirectoryName(filename);
            BasePath = "";
            while (dirName != null && Path.GetFileName(dirName).ToLower() != "d2")
            {
                BasePath = Path.GetFileName(dirName) + "/" + BasePath;
                dirName = Path.GetDirectoryName(dirName);
            }
            if (dirName == null) dirName = filename;
            GamePath = Path.GetDirectoryName(dirName);
            var actName = Path.GetDirectoryName(Path.GetDirectoryName(filename));
            actName = Path.GetFileName(actName).ToLower();
            if (!actName.StartsWith("act"))
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
                var a = r + g + b > 0 ? 255 : 0;
                Palette[i] = Color.FromArgb(a, r, g, b).ToArgb();
            }
            //var palettesIdx = new CofPalettes[] { CofPalettes.Base, CofPalettes.Trans25, CofPalettes.Trans50, CofPalettes.Trans75, CofPalettes.Alpha, CofPalettes.Luminance, CofPalettes.AlphaBright };
            //for (int i = 0; i < Palettes.Length; i++)
            //{
            //    var pal = new int[256];
            //    reader.BaseStream.Position = (int)palettesIdx[i] * 256;
            //    for (int j = 0; j < pal.Length; j++)
            //    {
            //        var r = reader.ReadByte();
            //        var g = reader.ReadByte();
            //        var b = reader.ReadByte();
            //        var a = reader.ReadByte();
            //        if (i == 0 && r + g + b > 0) a = 255;
            //        //var a = r + g + b > 0 ? 255 : 0;
            //        pal[j] = Color.FromArgb(a, r, g, b).ToArgb();
            //    }
            //    Palettes[i] = pal;
            //}
            //Palette = Palettes[0];
            fStream.Close();
        }

        public void ReadAct(string filename)
        {
            Cursor.Current = Cursors.WaitCursor;
            var actName = Path.GetFileNameWithoutExtension(filename);
            actName = actName.Substring(actName.Length - 2, 2);
            if (actName == "a1")
            {
                var actInfo = new TIniReader($"{GamePath}/D2/data/global/excel/Levels.txt", '\t')[""];
                var drLgTypeIdx = actInfo[0].IndexOf("DrLgType");
            }
            Cursor.Current = Cursors.Default;
        }

        int ActNo;
        int SubstitutionType;
        TVersion Version = TVersion.Final;
        public override void ReadMap(string filename)
        {
            Cursor.Current = Cursors.WaitCursor;
            ReadPalette(filename);
            Walls.Clear();
            Sprites.Clear();
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
            Cells = new TCell[WorldHeight, WorldWidth];
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var cell = new TCell();
                    cell.Map = this;
                    cell.X = x;
                    cell.Y = y;
                    Cells[y, x] = cell;
                }
            ReadCells(reader);
            ReadObjects(reader);
            //LoadSubstitutions(reader);
            //LoadNPCs(reader);
            reader.Close();
            Width = WorldWidth + WorldHeight;
            Height = Width / 2;
            Cells = UntransformFromHexMapping();
            Game.Board.ScrollPos = new PointF(0.5f, 0.5f);
            Cursor.Current = Cursors.Default;
        }

        public override void WriteMap(string filename)
        {
            Cursor.Current = Cursors.WaitCursor;
            var fStream = new FileStream(filename, FileMode.Create);
            var writer = new BinaryWriter(fStream);
            writer.Write((int)TVersion.Final);
            writer.Write(Width - 1);
            writer.Write(Height - 1);
            //WorldWidth = Width;
            //WorldHeight = Height;
            WriteCells(writer);
            WriteObjects(writer);
            //LoadSubstitutions(reader);
            //LoadNPCs(reader);
            writer.Close();
            //Game.Cells = UntransformFromHexMapping();
            Cursor.Current = Cursors.Default;
        }


        void ReadWalls(BinaryReader reader, List<TDiabloWall>[,,] blockTiles)
        {
            var typeLookup = new int[]{
                0x00, 0x01, 0x02, 0x01, 0x02, 0x03, 0x03, 0x05, 0x05, 0x06,
                0x06, 0x07, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E,
                0x0F, 0x10, 0x11, 0x12, 0x14,};
            var walls = new List<TDiabloWall>();
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var tileInfo = reader.ReadInt32();
                    if (tileInfo == 0) continue;
                    var blockTile = new TDiabloWall();
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
                    if (Version < TVersion.HasAct && wallType < typeLookup.Length)
                        wallType = typeLookup[wallType];
                    wall.Type = (TWallType)wallType;
                    var selection = blockTiles[wallType, wall.Style, wall.Seq];
                    if (selection != null)
                    {
                        wall.Rarity = TGame.Random.Next(selection.Count);
                        var wallTile = selection[wall.Rarity];
                        wall.Tiles = wallTile.Tiles;
                        wall.Order = y * Width + x;
                        var pos = World2ViewTransform(5 * x, 5 * y);
                        wall.X = (int)(pos.X - 5 * TDiabloTile.Width / 2);
                        wall.Y = (int)pos.Y;
                        if (wall.Type == TWallType.Roof)
                        {
                            wall.Y -= wallTile.RoofHeight;
                            Roofs.Add(wall);
                        }
                        else
                        {
                            if (wall.Type < TWallType.Roof)
                                wall.Y += 5 * TDiabloTile.Height;
                            Walls.Add(wall);
                        }
                        wall.Bounds = new Rectangle(wall.X, wall.Y + wallTile.Height, wallTile.Width, Math.Abs(wallTile.Height));
                        if (wall.Type == TWallType.LeftTopCorner_Top)
                        {
                            var extraTile = new TDiabloWall();
                            extraTile.Type = TWallType.LeftTopCorner_Left;
                            extraTile.Style = wall.Style;
                            extraTile.Seq = wall.Seq;
                            selection = blockTiles[(int)extraTile.Type, extraTile.Style, extraTile.Seq];
                            extraTile.Tiles = selection[wall.Rarity].Tiles;
                            extraTile.X = wall.X;
                            extraTile.Y = wall.Y;
                            extraTile.Bounds = wall.Bounds;
                            Walls.Add(extraTile);
                        }
                    }
                }
        }

        void WriteWalls(BinaryWriter writer)
        {
            var wallIdx = 0;
            var walls = new List<TDiabloWall>();
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    //var tileInfo = reader.ReadInt32();
                    //if (tileInfo == 0) continue;
                    if (wallIdx >= Walls.Count)
                    {
                        writer.Write(0);
                        continue;
                    }
                    var blockTile = (TDiabloWall)Walls[wallIdx]; wallIdx++;
                    var tileInfo = blockTile.Style << 20 | blockTile.Seq << 8;
                    writer.Write(tileInfo);
                    walls.Add(blockTile);
                }
            wallIdx = 0;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    //var wallType = TWallType.LowerWall;
                    if (wallIdx >= Walls.Count)
                    {
                        writer.Write(0);
                        continue;
                    }
                    var wall = walls[wallIdx]; wallIdx++;
                    writer.Write((int)wall.Type);
                }
        }

        void ReadFloors(BinaryReader reader, List<TDiabloWall>[,,] blockTiles)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var tileInfo = reader.ReadInt32();
                    if (tileInfo == 0) continue;
                    var floor = new TDiabloWall();
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
                                var cell = Cells[5 * y + u, 5 * x + v];
                                var tileIdx = (4 - u) * 5 + 4 - v;
                                if (tileIdx < floor.Tiles.Count)
                                    cell.Floor = floor.Tiles[tileIdx];
                            }
                    }
                }
        }

        void WriteFloors(BinaryWriter writer)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    writer.Write(0);
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
        void WriteShadows(BinaryWriter writer)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    writer.Write(0);
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

        void WriteSubstitutions(BinaryWriter writer)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    writer.Write(0);
                }
        }

        void ReadCells(BinaryReader reader)
        {
            if (Version >= TVersion.HasAct)
                ActNo = reader.ReadInt32();
            if (Version >= TVersion.HasSubtitutionLayers)
                SubstitutionType = reader.ReadInt32();
            if (Version >= TVersion.HasFiles)
            {
                Floors = new List<TTile>();
                int filesCount = reader.ReadInt32();
                for (int i = 0; i < filesCount; i++)
                {
                    var fileName = ReadZString(reader);
                    if (fileName.StartsWith("C:\\"))
                        fileName = fileName.Substring(2);
                    //string fileName = Encoding.ASCII.GetString(bytes.ToArray());
                    fileName = GamePath + fileName;
                    //Game.GroundTiles.AddRange(ReadTileSet(fileName, ".dt1"));
                    ReadTileSet(fileName, ".dt1");
                }
            }
            var blockTiles = new List<TDiabloWall>[64, 64, 64];
            foreach (var blockTile in Walls)
            {
                var dbTile = (TDiabloWall)blockTile;
                if (blockTiles[(int)dbTile.Type, dbTile.Style, dbTile.Seq] == null)
                    blockTiles[(int)dbTile.Type, dbTile.Style, dbTile.Seq] = new List<TDiabloWall>();
                blockTiles[(int)dbTile.Type, dbTile.Style, dbTile.Seq].Add(dbTile);
            }
            Walls.Clear();
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
        void WriteCells(BinaryWriter writer)
        {
            writer.Write(ActNo);
            writer.Write(SubstitutionType);
            writer.Write(1);
            var filename = Encoding.ASCII.GetBytes($"/D2/{BasePath}{MapName}.dt1\0");
            writer.Write(filename);
            //var blockTiles = new List<DiabloWall>[64, 64, 64];
            //foreach (var blockTile in Game.Walls)
            //{
            //    var dbTile = (DiabloWall)blockTile;
            //    if (blockTiles[(int)dbTile.Type, dbTile.Style, dbTile.Seq] == null)
            //        blockTiles[(int)dbTile.Type, dbTile.Style, dbTile.Seq] = new List<DiabloWall>();
            //    blockTiles[(int)dbTile.Type, dbTile.Style, dbTile.Seq].Add(dbTile);
            //}
            var wallsLayersCount = 1;
            writer.Write(wallsLayersCount);
            var floorsLayersCount = 0;
            writer.Write(floorsLayersCount);
            for (int i = 0; i < wallsLayersCount; i++)
                WriteWalls(writer);
            for (int i = 0; i < floorsLayersCount; i++)
                WriteFloors(writer);
            WriteShadows(writer);
            if (SubstitutionType == 1 || SubstitutionType == 2)
                WriteSubstitutions(writer);
        }

        void ReadObjects(BinaryReader reader)
        {
            var objInfo = new TIniReader($"{GamePath}/D2/data/global/Obj.txt", '\t')[""];
            var baseIdx = objInfo[0].IndexOf("Base");
            var tokenIdx = objInfo[0].IndexOf("Token");
            var modeIdx = objInfo[0].IndexOf("Mode");
            var classIdx = objInfo[0].IndexOf("Class");
            var dirIdx = objInfo[0].IndexOf("Direction");
            var colormapIdx = objInfo[0].IndexOf("Index");
            var armorIdx = objInfo[0].IndexOf("HD");
            var anims = new Dictionary<int, TDiabloAnimation>();
            var objCount = reader.ReadInt32();
            for (int i = 0; i < objCount; i++)
            {
                var obj = new TSprite();
                var type = reader.ReadInt32();
                var id = reader.ReadInt32();
                var x = reader.ReadInt32();
                var y = reader.ReadInt32();
                var flags = reader.ReadInt32();
                obj.Id = ActNo * 210 + (type - 1) * 60 + id + 1;
                anims.TryGetValue(obj.Id, out var anim);
                if (anim == null)
                {
                    anim = new TDiabloAnimation();
                    anim.Token = objInfo[obj.Id][tokenIdx];
                    if (anim.Token == "") continue;
                    anim.BasePath = $"{objInfo[obj.Id][baseIdx]}/{anim.Token}";
                    anim.Mode = objInfo[obj.Id][modeIdx];
                    if (anim.Mode == "") anim.Mode = "NU";
                    anim.ClassType = objInfo[obj.Id][classIdx];
                    if (anim.ClassType == "") anim.ClassType = "HTH";
                    int.TryParse(objInfo[obj.Id][colormapIdx], out anim.ColormapIdx);
                    for (int a = 0; a < 16; a++)
                        anim.ArmorClass[a] = objInfo[obj.Id][armorIdx + a];
                    anim.ReadCof();
                    anims.Add(obj.Id, anim);
                }
                obj.Animation = anim;
                var userDir = objInfo[obj.Id][dirIdx];
                if (userDir != "")
                {
                    var dirCode = 32 / anim.DirectionsCount * int.Parse(userDir) + anim.DirectionsCount % 8;
                    obj.ViewAngle = TDiabloAnimation.Directions[dirCode];
                    obj.Flipped = false;
                }
                var pos = World2ViewTransform(x, y);
                obj.X = (int)(pos.X - 3 * TDiabloTile.Width / 2);
                obj.Y = (int)(pos.Y + 3 * TDiabloTile.Height / 2);
                obj.Y -= obj.ActFrame.Bounds.Height;
                //obj.X -= obj.ActFrame.Bounds.Width;
                obj.Bounds = new Rectangle(obj.X, obj.Y, obj.ActFrame.Bounds.Width, obj.ActFrame.Bounds.Height);
                Sprites.Add(obj);
            }
        }

        void WriteObjects(BinaryWriter writer)
        {
            writer.Write(Sprites.Count);
        }
    }

}
