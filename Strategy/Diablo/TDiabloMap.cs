using Strategy.Diablo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;
using System.Windows.Forms;

namespace Strategy
{
    class TDiabloMap : TMap
    {
        // colormaps index for COF effect
        public enum CofPalettes { Base = 0, Trans25 = 53, Trans50 = 309, Trans75 = 565, Alpha = 821, Luminance = 1077, AlphaBright = 1461};
        //public static int[][] PalettesLum = new int[256][];
        public static TPalette Palette = new TPalette();
        public Vector2 GridOffset;
        public int WorldHeight;
        public int WorldWidth;
        public string BasePath;
        public int WallsLayersCount = 1;

        public void ReadTileSet(string filename, string ext)
        {
            filename = filename.Substring(0, filename.Length - 4) + ext;
            if (!File.Exists(filename)) return;
            var fStream = new FileStream(filename, FileMode.Open);
            var reader = new BinaryReader(fStream);
            int verMajor = reader.ReadInt32();
            int verMinor = reader.ReadInt32();
            if (verMajor == 7 && verMinor == 6)
            {
                reader.ReadBytes(260);
                int wallsCount = reader.ReadInt32();
                int filePos = reader.ReadInt32();
                var walls = new List<TDiabloWall>();
                for (int i = 0; i < wallsCount; i++)
                {
                    var wall = new TDiabloWall();
                    wall.ReadHeader(reader);
                    walls.Add(wall);
                    Walls.Add(wall);
                }
                foreach (var wall in walls)
                    wall.ReadTiles(reader);
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
            var tilesHeaderPos = 276 + Walls.Count * 96;
            for (int i = 0; i < Walls.Count; i++)
            {
                var wall = (TDiabloWall)Walls[i];
                wall.WriteHeader(writer, tilesHeaderPos);
                tilesHeaderPos += wall.EncodedSize;
            }
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
            x /= 5 * TDiabloTile.Width / 2;
            y /= 5 * TDiabloTile.Height / 2;
            return new Vector2(x, (y + ((int)x & 1)) / 2);
        }
        public override Vector2 Map2WorldTransform(float x, float y)
        {
            y = 2 * y - ((int)x & 1);
            x *= 5;
            y *= 5;
            x += GridOffset.X;
            y += GridOffset.Y;
            return new Vector2(x + y, y - x) / 2;
        }

        TCell[,] UntransformFromHexMapping()
        {
            Width = WorldWidth + WorldHeight;
            Height = Width / 2;
            var cells = new TCell[Height, Width];
            //for (int y = 0; y < Height; y++)
            //    for (int x = 0; x < Width; x++)
            //    {
            //        var pos = Map2WorldTransform(x, y);
            //        if (pos.Y >= 0 && pos.Y < WorldHeight && pos.X >= 0 && pos.X < WorldWidth)
            //        {
            //            var cell = Cells[(int)pos.Y, (int)pos.X];
            //            cell.X = x;
            //            cell.Y = y;
            //            var cellPos = cell.Position;
            //            cell.Bounds = new Rectangle((int)cellPos.X, (int)cellPos.Y, 5 * TDiabloTile.Width, 5 * TDiabloTile.Height);
            //            cell.Bounds.Offset(- 5 * TDiabloTile.Width / 2, - 5 * TDiabloTile.Height / 2);
            //            cells[y, x] = cell;
            //        }
            //        //else
            //        //    cells[y, x] = Game.Cells[0, 0];
            //    }
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var cell = Cells[5 * y, 5 * x];
                    var pos = World2MapTransform(cell.X, cell.Y);
                    if (pos.Y >= 0 && pos.Y < Height && pos.X >= 0 && pos.X < Width)
                    {
                        cell.X = (int)pos.X;
                        cell.Y = (int)pos.Y;
                        var cellPos = cell.Position;
                        cell.Bounds = new Rectangle((int)cellPos.X, (int)cellPos.Y, 5 * TDiabloTile.Width, 5 * TDiabloTile.Height);
                        cell.Bounds.Offset(-5 * TDiabloTile.Width / 2, -5 * TDiabloTile.Height / 2);
                        cells[cell.Y, cell.X] = cell;
                    }
                }
            return cells;
        }

        TCell[,] TransformToHexMapping()
        {
            var cells = new TCell[5 * WorldHeight, 5 * WorldWidth];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var pos = Map2WorldTransform(x, y);
                    if (pos.Y >= 0 && pos.Y < 5 * WorldHeight && pos.X >= 0 && pos.X < 5 * WorldWidth)
                    {
                        var cell = Cells[y, x];
                        //cell.X = (int)pos.X;
                        //cell.Y = (int)pos.Y;
                        cells[(int)pos.Y, (int)pos.X] = cell;
                    }
                }
            return cells;
        }

        public void MapTileSet(TCollect<TTile> tiles)
        {
            var blockSize = 5 * TDiabloTile.Width;
            int mapSize = (int)Math.Ceiling(Math.Sqrt(Walls.Count)) + 1;
            Height = 2 * blockSize * mapSize / (5 * TDiabloTile.Width);
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
            var blockX = 0;
            var blockY = 0;
            var rowSize = 0;
            for (int idx = 0; idx < Walls.Count; idx++)
            {
                var wall = (TDiabloWall)Walls[idx];
                wall.X += blockX;
                wall.Y += blockY - wall.Bounds.Y;
                wall.Bounds = new Rectangle(wall.X, blockY, wall.Bounds.Width, wall.Bounds.Height);
                if (wall.Bounds.Height > rowSize)
                    rowSize = wall.Bounds.Height;
                blockX += wall.Width;
                if (blockX > mapSize * blockSize)
                {
                    blockX = 0;
                    blockY += rowSize;
                    rowSize = 0;
                }
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
                BasePath = Path.GetFileName(dirName) + "\\" + BasePath;
                dirName = Path.GetDirectoryName(dirName);
            }
            if (dirName == null) dirName = filename;
            GamePath = Path.GetDirectoryName(dirName);
            var actName = Path.GetDirectoryName(Path.GetDirectoryName(filename));
            actName = Path.GetFileName(actName).ToLower();
            if (!actName.StartsWith("act"))
                actName = "Units";
            var palPath = GamePath + "\\D2\\Data\\Global\\Palette\\" + actName + "\\pal.dat";
            if (File.Exists(palPath))
            {
                Palette.Load(palPath);
                var palRefsPath = palPath.Substring(0, palPath.Length - 4) + ".refs";
                if (File.Exists(palRefsPath))
                    Palette.LoadRefs(palRefsPath);
                else
                    Palette.SaveRefs(palRefsPath);
            }
            else
                Palette.Colors = TPalette.CreateRGB332();
                //var fStream = new FileStream(palPath, FileMode.Open, FileAccess.Read);
                //var reader = new BinaryReader(fStream);
                //Palette.Colors = new int[256];
                //for (int i = 0; i < Palette.Length; i++)
                //{
                //    var r = reader.ReadByte();
                //    var g = reader.ReadByte();
                //    var b = reader.ReadByte();
                //    var a = reader.ReadByte();
                //    if (r + g + b > 0) a = 255;
                //    //var a = r + g + b > 0 ? 255 : 0;
                //    Palette.Colors[i] = Color.FromArgb(a, r, g, b).ToArgb();
                //}
                //reader.Close();
                //var palettesIdx = new CofPalettes[] { CofPalettes.Base, CofPalettes.Trans25, CofPalettes.Trans50, CofPalettes.Trans75, CofPalettes.Alpha, CofPalettes.Luminance, CofPalettes.AlphaBright };
                //reader.BaseStream.Position = (int)CofPalettes.Luminance * 256;
                //for (int i = 0; i < Palette.Length; i++)
                //{
                //    var pal = new byte[Palette.Length];
                //    //reader.BaseStream.Position = (int)palettesIdx[i] * 256;
                //    for (int j = 0; j < pal.Length; j++)
                //    {
                //        var lum = reader.ReadByte();
                //        var g = reader.ReadByte();
                //        var b = reader.ReadByte();
                //        var a = reader.ReadByte();
                //        if (i == 0 && r + g + b > 0) a = 255;
                //        pal[j] = Color.FromArgb(a, r, g, b).ToArgb();
                //    }
                //    PalettesLum[i] = pal;
                //}
                //Palette = Palettes[0];
                //reader.Close();
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

        public int ActNo;
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
            WorldWidth = reader.ReadInt32() + 1;
            WorldHeight = reader.ReadInt32() + 1;
            GridOffset.X = - 5 * WorldHeight;
            if (WorldHeight % 2 != 0)
                GridOffset.Y = 5;
            ReadCells(reader);
            ReadObjects(reader);
            var pathsCount = reader.ReadInt32();
            //LoadSubstitutions(reader);
            //LoadNPCs(reader);
            reader.Close();
            Cells = UntransformFromHexMapping();
            Game.Board.ScrollPos = new PointF(0.5f, 0.5f);
            Cursor.Current = Cursors.Default;
        }

        public override void WriteMap(string filename)
        {
            Cursor.Current = Cursors.WaitCursor;
            ReadPalette(filename);
            var fStream = new FileStream(filename, FileMode.Create);
            var writer = new BinaryWriter(fStream);
            writer.Write((int)TVersion.HasUnknownBytes2);
            writer.Write(WorldWidth - 1);
            writer.Write(WorldHeight - 1);
            if (Game != null)
                Cells = TransformToHexMapping();
            WriteCells(writer);
            WriteObjects(writer);
            var pathsCount = 0;
            writer.Write(pathsCount);
            //LoadSubstitutions(reader);
            //LoadNPCs(reader);
            writer.Close();
            if (Game != null)
                Cells = UntransformFromHexMapping();
            Cursor.Current = Cursors.Default;
        }


        void ReadWallsLayer(BinaryReader reader, List<TDiabloWall>[,,] blockTiles, int layerIdx)
        {
            var walls = new List<TDiabloWall>();
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var tileInfo = reader.ReadInt32();
                    if (tileInfo == 0) continue;
                    var wall = new TDiabloWall();
                    wall.Seq = tileInfo >> 8 & 0x3F;
                    wall.Style = tileInfo >> 20 & 0x3F;
                    wall.Hidden = tileInfo < 0;
                    //blockTile.Property1 = tileInfo & 0xFF;
                    //blockTile.Unk1 = tileInfo >> 14 & 0x3F;
                    //blockTile.Unk2 = tileInfo >> 26 & 0x1F;
                    walls.Add(wall);
                }
            int[] typeLookup = null;
            if (Version < TVersion.HasAct)
                typeLookup = new int[] {
                0x00, 0x01, 0x02, 0x01, 0x02, 0x03, 0x03, 0x05, 0x05, 0x06,
                0x06, 0x07, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E,
                0x0F, 0x10, 0x11, 0x12, 0x14 };
            if (Version < TVersion.HasWalls)
                ReadFloors(reader, blockTiles);
            var wallIdx = 0;
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var cell = Cells[5 * y, 5 * x];
                    var wallType = reader.ReadInt32();
                    if (wallType == 0) continue;
                    var wall = walls[wallIdx]; wallIdx++;
                    if (typeLookup != null && wallType < typeLookup.Length)
                        wallType = typeLookup[wallType];
                    wall.Type = (TWallType)wallType;
                    var selection = blockTiles[wallType, wall.Style, wall.Seq];
                    if (selection != null)
                    {
                        if (cell.Wall == null) cell.Wall = new TWall();
                        cell.Wall.Tiles.Add(wall);
                        wall.Rarity = TGame.Random.Next(selection.Count);
                        var wallTile = selection[wall.Rarity];
                        wall.Tiles = wallTile.Tiles;
                        wall.Order = layerIdx * WorldWidth * WorldHeight + y * WorldWidth + x;
                        var pos = World2ViewTransform(5 * x, 5 * y);
                        wall.X = (int)pos.X - 5 * TDiabloTile.Width / 2;
                        wall.Y = (int)pos.Y - 5 * TDiabloTile.Height / 2;
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
                        wall.Bounds = new Rectangle(wall.X, Math.Min(wall.Y, wall.Y + wallTile.Height), wallTile.Width, Math.Abs(wallTile.Height));
                        if (wall.Type == TWallType.LeftTopCorner_Top)
                        {
                            var extraTile = new TDiabloWall();
                            extraTile.Type = TWallType.LeftTopCorner_Left;
                            extraTile.Style = wall.Style;
                            extraTile.Seq = wall.Seq;
                            selection = blockTiles[(int)extraTile.Type, extraTile.Style, extraTile.Seq];
                            if (selection != null)
                            {
                                extraTile.Tiles = selection[wall.Rarity].Tiles;
                                extraTile.X = wall.X;
                                extraTile.Y = wall.Y;
                                extraTile.Bounds = wall.Bounds;
                                Walls.Add(extraTile);
                            }
                        }
                        else if (wall.Type >= TWallType.Special1 && wall.Type <= TWallType.Special2)
                            cell.EventIdx = wall.Type - TWallType.Special1 + 1;
                    }
                }
        }

        void WriteWallsLayer(BinaryWriter writer, int layerIdx)
        {
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var tileInfo = 0;
                    var cell = Cells[5 * y, 5 * x];
                    if (cell.Wall != null && layerIdx < cell.Wall.Tiles.Count)
                    {
                        var wall = (TDiabloWall)cell.Wall.Tiles[layerIdx];
                            tileInfo = wall.Style << 20 | wall.Seq << 8 | 0x81;
                        if (wall.Hidden)
                            tileInfo |= unchecked((int)0x80000000);
                    }
                    writer.Write(tileInfo);
                }
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var tileInfo = 0;
                    var cell = Cells[5 * y, 5 * x];
                    if (cell.Wall != null && layerIdx < cell.Wall.Tiles.Count)
                    {
                        var wall = (TDiabloWall)cell.Wall.Tiles[layerIdx];
                        tileInfo = (int)wall.Type;
                    }
                    writer.Write(tileInfo);
                }
        }

        void ReadFloors(BinaryReader reader, List<TDiabloWall>[,,] blockTiles)
        {
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var tileInfo = reader.ReadInt32();
                    if (tileInfo == 0) continue;
                    var wall = new TDiabloWall();
                    wall.Seq = tileInfo >> 8 & 0x3F;
                    wall.Style = tileInfo >> 20 & 0x3F;
                    wall.Hidden = tileInfo < 0;
                    var selection = blockTiles[(int)wall.Type, wall.Style, wall.Seq];
                    if (selection != null)
                    {
                        var floor = selection[TGame.Random.Next(selection.Count)];
                        if (floor.Image == null)
                        {
                            var floorImage = new Bitmap(floor.Width, 80);
                            var gc = Graphics.FromImage(floorImage);
                            for (int u = 0; u < 5; u++)
                                for (int v = 0; v < 5; v++)
                                {
                                    //var cell = Cells[5 * y + 4 - u, 5 * x + 4 - v];
                                    var tileIdx = u * 5 + v;
                                    if (tileIdx < floor.Tiles.Count)
                                    {
                                        var tile = floor.Tiles[tileIdx];
                                        gc.DrawImage(tile.Image, tile.X, tile.Y);
                                    }
                                }
                            floor.Image = floorImage;
                        }
                        Cells[5 * y, 5 * x].Floor = floor;
                    }
                }
        }

        void WriteFloors(BinaryWriter writer)
        {
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var tileInfo = 0;
                    var cell = Cells[5 * y, 5 * x];
                    if (cell.Floor != null)
                    {
                        var floor = (TDiabloWall)cell.Floor;
                        tileInfo = floor.Style << 20 | floor.Seq << 8 | 0xc2;
                    }
                    writer.Write(tileInfo);
                }
        }

        void ReadShadows(BinaryReader reader)
        {
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var tileInfo = reader.ReadInt32();
                    //if (tileInfo == 0) continue;
                }
        }
        void WriteShadows(BinaryWriter writer)
        {
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    writer.Write(0);
                }
        }

        void ReadSubstitutions(BinaryReader reader)
        {
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var tileInfo = reader.ReadInt32();
                    //if (tileInfo == 0) continue;
                }
        }

        void WriteSubstitutions(BinaryWriter writer)
        {
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
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
                //Floors = new List<TTile>();
                int filesCount = reader.ReadInt32();
                for (int i = 0; i < filesCount; i++)
                {
                    var fileName = ReadZString(reader);
                    if (fileName.StartsWith("C:\\"))
                        fileName = fileName.Substring(2);
                    //string fileName = Encoding.ASCII.GetString(bytes.ToArray());
                    fileName = GamePath + fileName;
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
            Cells = new TCell[5 * WorldHeight, 5 * WorldWidth];
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var cell = new TCell();
                    cell.Map = this;
                    cell.X = 5 * x;
                    cell.Y = 5 * y;
                    Cells[cell.Y, cell.X] = cell;
                }
            //Floors = new List<TTile>(new TTile[Walls.Count]);
            Walls.Clear();
            if (Version >= TVersion.HasUnknownBytes1Low && Version <= TVersion.HasUnknownBytes1High)
                reader.ReadBytes(8);
            if (Version < TVersion.HasWalls)
            {
                ReadWallsLayer(reader, blockTiles, 0);
                ReadSubstitutions(reader);
                ReadShadows(reader);
            }
            else
            {
                WallsLayersCount = reader.ReadInt32();
                var floorsLayersCount = Version < TVersion.HasFloors ? 1 : reader.ReadInt32();
                for (int i = 0; i < WallsLayersCount; i++)
                    ReadWallsLayer(reader, blockTiles, i);
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
            var filename = Encoding.ASCII.GetBytes($"\\D2\\{BasePath}{MapName}.tg1\0");
            //var warps = Encoding.ASCII.GetBytes($"\\D2\\data\\global\\tiles\\act1\\barracks\\warp.tg1\0");
            writer.Write(filename);
            //writer.Write(warps);
            //var blockTiles = new List<TDiabloWall>[64, 64, 64];
            //foreach (var blockTile in Walls)
            //{
            //    var dbTile = (TDiabloWall)blockTile;
            //    if (blockTiles[(int)dbTile.Type, dbTile.Style, dbTile.Seq] == null)
            //        blockTiles[(int)dbTile.Type, dbTile.Style, dbTile.Seq] = new List<TDiabloWall>();
            //    blockTiles[(int)dbTile.Type, dbTile.Style, dbTile.Seq].Add(dbTile);
            //}
            writer.Write(WallsLayersCount);
            var floorsLayersCount = 1;
            writer.Write(floorsLayersCount);
            for (int i = 0; i < WallsLayersCount; i++)
                WriteWallsLayer(writer, i);
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
                    //obj.Flipped = false;
                }
                var pos = World2ViewTransform(x, y);
                obj.X = (int)(pos.X - 4 * TDiabloTile.Width / 2);
                obj.Y = (int)(pos.Y - 7 * TDiabloTile.Height / 2);
                obj.Y -= obj.ActFrame.Bounds.Height;
                //obj.X -= obj.ActFrame.Bounds.Width;
                obj.Bounds = new Rectangle(obj.X, obj.Y, obj.ActFrame.Bounds.Width, obj.ActFrame.Bounds.Height);
                Sprites.Add(obj);
            }
        }

        void WriteObjects(BinaryWriter writer)
        {
            writer.Write(0);
        }
    }

}
