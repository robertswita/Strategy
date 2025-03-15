using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Strategy
{
    internal class TDispelMap: TMap
    {
        public static int ChunkSize = 25;
        public int WorldWidth;
        public int WorldHeight;
        public Vector2 GridOffset;

        int Rgb16To32(int byte0, int byte1)
        {
            int red = byte1 & 0xF8;
            int green = byte1 << 5 & 0xFF | byte0 >> 3 & 0xFC;
            int blue = byte0 << 3 & 0xFF;
            int alpha = red + green + blue > 0 ? 0xFF : 0;
            return alpha << 24 | red << 16 | green << 8 | blue;
        }
        public List<Image> ReadTileSet(string filename, string ext)
        {
            TGame.TileHeight = 32;
            TGame.TileWidth = 64;
            filename = filename.Substring(0, filename.Length - 4) + ext;
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var pixels = new byte[fStream.Length];
            fStream.Read(pixels, 0, pixels.Length);
            var images = new List<Image>();
            int pos = 0;
            while (pos < pixels.Length)
            {
                var tile = new TPixmap(TGame.TileWidth, TGame.TileHeight);                
                for (int y = 0; y < tile.Height; y++)
                {
                    var n = y < tile.Height / 2 ? y : tile.Height - 1 - y;
                    var r = 1 + 2 * n;
                    for (int x = tile.Width / 2 - r; x < tile.Width / 2 + r; x++)
                        tile[x, y] = Rgb16To32(pixels[pos++], pixels[pos++]);
                }
                images.Add(tile.Image);
            }
            fStream.Close();
            return images;
        }

        public void MapTileSet()
        {
            int mapSize = (int)Math.Sqrt(Game.GroundTiles.Count);
            var map = new Bitmap(mapSize, mapSize);
            Game.Cells = new TCell[map.Height, map.Width];
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    var cell = new TCell();
                    cell.Game = Game;
                    cell.X = x;
                    cell.Y = y;
                    var tile = new TTile();
                    tile.ImageIndex = y * map.Height + x;
                    cell.GroundTile = tile;
                    Game.Cells[y, x] = cell;
                }
            //Game.Map = map;
            Height = map.Height;
            Width = map.Width;
        }

        public override void ReadMap(string filename)
        {
            Game.GroundTiles = ReadTileSet(filename, ".gtl");
            Game.TilesImages = ReadTileSet(filename, ".btl");
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fStream);
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            WorldWidth = width * ChunkSize - 1;
            WorldHeight = height * ChunkSize - 1;
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
            int diagonalSize = WorldWidth + WorldHeight;
            var hexMapSize = new Vector2(diagonalSize, diagonalSize);
            var ratio = new Vector2(0.4f, 0.6f);
            var mapSize = ratio * hexMapSize;
            var offset = (hexMapSize - mapSize) / 2;
            GridOffset.X = -(int)(offset.X);
            GridOffset.Y = WorldHeight - (int)(offset.Y);
            Height = (int)mapSize.Y;
            Width = (int)mapSize.X;
            int propCount = reader.ReadInt32();
            int tileCount = reader.ReadInt32();
            reader.ReadBytes((tileCount - 1) * propCount * sizeof(int));
            tileCount = reader.ReadInt32();
            reader.ReadBytes(tileCount * propCount);
            ReadSprites(reader);
            ReadColumnBuildTiles(reader);
            ReadEvents(reader);
            ReadGroundTiles(reader);           
            ReadRoofBuildTiles(reader);
            UntransformHexMapping();
            //Background = GetBackground();
            reader.Close();
            LoadExtras(filename);
            LoadMonsters(filename);
            LoadNpc(filename);
        }

        private Dictionary<int, string> LoadInfo(string path, int spriteNameColumnIndex)
        {
            return File.ReadLines(path)
                    .Where(line => !line.StartsWith(";"))
                    .Select(line => line.Split(','))
                    .Where(fields => fields[1] != "null")
                    .ToDictionary(fields => int.Parse(fields[0]), fields => fields[spriteNameColumnIndex]);
        }

        void ReadSpriteModels(List<TFrame[][][]> spriteModels, Dictionary<int, string> names, string path)
        {
            for (var i = 0; i < names.Count; i++)
            {
                var filename = path + names.Values.ElementAt(i);
                if (File.Exists(filename))
                {
                    var file = File.OpenRead(filename);
                    var reader = new BinaryReader(file);
                    ReadAnimation(reader);
                    spriteModels[names.Keys.ElementAt(i)] = Game.Animations.Last();
                    reader.Close();
                }
            }
        }
        public void LoadExtras(string filename)
        {
            var mapName = Path.GetFileNameWithoutExtension(filename);
            var gamePath = Path.GetDirectoryName(Path.GetDirectoryName(filename));
            var names = LoadInfo($"{gamePath}/Extra.ini", 1);
            var path = $"{gamePath}/ExtraInGame/";
            var mapRefPath = $"{path}/Ext{mapName}.ref";
            if (!File.Exists(mapRefPath)) return;
            if (Game.ExtraSpriteModels.Count == 0)
            {
                Game.ExtraSpriteModels = new List<TFrame[][][]>(new TFrame[names.Keys.ElementAt(names.Count - 1) + 1][][][]);
                ReadSpriteModels(Game.ExtraSpriteModels, names, path);
            }
            using (var reader = new BinaryReader(File.OpenRead(mapRefPath)))
            {
                var count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    var sprite = new TSprite();
                    var fileId = reader.ReadByte();
                    reader.ReadByte();
                    sprite.ModelIdx = reader.ReadByte();
                    var name = Encoding.ASCII.GetString(reader.ReadBytes(32));// 0xcd
                    var type = reader.ReadByte(); //"7-magic, 6-interactive object, 5-altar, 4-sign, 2-door, 0-chest"
                    var x = reader.ReadInt32();
                    var y = reader.ReadInt32();
                    var pos = TransformGrid(x, y);
                    sprite.X = (int)((pos.X + 1) * TGame.TileWidth / 2);
                    sprite.Y = (int)(pos.Y * TGame.TileHeight / 2);

                    sprite.ViewAngle = reader.ReadByte(); // rotation
                    reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();

                    reader.ReadInt32();
                    sprite.Sequence = reader.ReadInt32();// "closed", AsInt32(), "chest 0-open, 1-closed");

                    var required_item1_id = reader.ReadByte(); // lower bound
                    var required_item1_type = reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    var required_item2_id = reader.ReadByte(); // upper bound
                    var required_item2_type = reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    var gold = reader.ReadInt32();
                    var item1_id = reader.ReadByte(); // lower bound
                    var item1_type = reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    var itemCount = reader.ReadInt32();
                    reader.ReadBytes(10 * sizeof(int));

                    var eventId = reader.ReadInt32();
                    var messageId = reader.ReadInt32(); // "id from message.scr for signs");
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadBytes(32);
                    var visibility = reader.ReadByte();
                    reader.ReadBytes(3);

                    sprite.ModelsSource = Game.ExtraSpriteModels;
                    sprite.Bounds = new Rectangle(sprite.X, sprite.Y, sprite.Frames[0].Bounds.Width, sprite.Frames[0].Bounds.Height);
                    Game.Sprites.Add(sprite);
                }
            }
        }

        public void LoadMonsters(string filename)
        {
            var mapName = Path.GetFileNameWithoutExtension(filename);
            var gamePath = Path.GetDirectoryName(Path.GetDirectoryName(filename));
            var names = LoadInfo($"{gamePath}/Monster.ini", 2);
            var path = $"{gamePath}/MonsterInGame/";
            var mapRefPath = $"{path}/Mon{mapName}.ref";
            if (!File.Exists(mapRefPath)) return;
            if (Game.MonsterSpriteModels.Count == 0)
            {
                Game.MonsterSpriteModels = new List<TFrame[][][]>(new TFrame[names.Keys.ElementAt(names.Count - 1) + 1][][][]);
                ReadSpriteModels(Game.MonsterSpriteModels, names, path);
            }
            using (var reader = new BinaryReader(File.OpenRead(mapRefPath)))
            {
                var count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    var sprite = new TSprite();
                    sprite.ModelsSource = Game.MonsterSpriteModels;
                    var fileId = reader.ReadInt32();
                    sprite.ModelIdx = reader.ReadInt32();
                    sprite.ViewAngle = sprite.ModelsSource[sprite.ModelIdx][0].Length - 1;
                    var x = reader.ReadInt32();
                    var y = reader.ReadInt32();
                    var pos = TransformGrid(x, y);
                    sprite.X = (int)((pos.X  + 1) * TGame.TileWidth / 2);
                    sprite.Y = (int)(pos.Y * TGame.TileHeight / 2);
                    var unk = reader.ReadBytes(5 * sizeof(int));
                    //reader.ReadInt32();
                    //reader.ReadInt32();
                    //reader.ReadInt32();
                    //reader.ReadInt32();
                    //reader.ReadInt32();
                    var lootSlot1Id = reader.ReadByte();
                    var lootSlot1Type = reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    var lootSlot2Id = reader.ReadByte();
                    var lootSlot2Type = reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    var lootSlot3Id = reader.ReadByte();
                    var lootSlot3Type = reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadByte();
                    reader.ReadInt32();
                    reader.ReadInt32();

                    sprite.Bounds = new Rectangle(sprite.X, sprite.Y, sprite.Frames[0].Bounds.Width, sprite.Frames[0].Bounds.Height);
                    Game.Sprites.Add(sprite);
                    Game.AnimatedSprites.Add(sprite);
                }
            }
        }

        public void LoadNpc(string filename)
        {
            var mapName = Path.GetFileNameWithoutExtension(filename);
            var gamePath = Path.GetDirectoryName(Path.GetDirectoryName(filename));
            var names = LoadInfo($"{gamePath}/Npc.ini", 1);
            var path = $"{gamePath}/NpcInGame/";
            var mapRefPath = $"{path}/Npc{mapName}.ref";
            if (!File.Exists(mapRefPath)) return;
            if (Game.NpcSpriteModels.Count == 0)
            {
                Game.NpcSpriteModels = new List<TFrame[][][]>(new TFrame[names.Keys.ElementAt(names.Count - 1) + 1][][][]);
                ReadSpriteModels(Game.NpcSpriteModels, names, path);
            }
            byte FILLER = 0xCD;
            int STRING_MAX_LENGTH = 260;
            using (var reader = new BinaryReader(File.OpenRead(mapRefPath)))
            {
                var count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    var sprite = new TSprite();
                    sprite.ModelsSource = Game.NpcSpriteModels;
                    var fileId = reader.ReadInt32();
                    sprite.ModelIdx = reader.ReadInt32();
                    //sprite.ViewAngle = sprite.ModelsSource[sprite.ModelIdx][0].Length - 1;

                    var name = Encoding.ASCII.GetString(reader.ReadBytes(STRING_MAX_LENGTH));
                    var text = Encoding.ASCII.GetString(reader.ReadBytes(STRING_MAX_LENGTH));

                    var scriptId = reader.ReadInt32();// party/scriptId
                    var showOnEvent = reader.ReadInt32();
                    reader.ReadInt32();
                    var goto1Filled = reader.ReadInt32();
                    var goto2Filled = reader.ReadInt32();
                    var goto3Filled = reader.ReadInt32();
                    var goto4Filled = reader.ReadInt32();// when goto4 not filled its 1, idk why
                    var goto1X = reader.ReadInt32();
                    var goto2X = reader.ReadInt32();
                    var goto3X = reader.ReadInt32();
                    var goto4X = reader.ReadInt32();
                    var goto1Y = reader.ReadInt32();
                    var goto2Y = reader.ReadInt32();
                    var goto3Y = reader.ReadInt32();
                    var goto4Y = reader.ReadInt32();
                    var pos = TransformGrid(goto1X, goto1Y);
                    sprite.X = (int)((pos.X + 1) * TGame.TileWidth / 2);
                    sprite.Y = (int)(pos.Y * TGame.TileHeight / 2);

                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    sprite.ViewAngle = reader.ReadInt32();// 0 = up, clockwise
                    //if (sprite.ViewAngle > 4)
                    //{
                    //    sprite.Flipped = true;
                    //    sprite.ViewAngle = 8 - sprite.ViewAngle;
                    //}
                    var viewsCount = sprite.ModelsSource[sprite.ModelIdx][0].Length;
                    if (sprite.ViewAngle >= viewsCount)
                    {
                        sprite.Flipped = true;
                        sprite.ViewAngle = 2 * viewsCount - 2 - sprite.ViewAngle;
                    }
                    reader.ReadBytes(14 * sizeof(int));

                    var dialogId = reader.ReadInt32();// also text for shop
                    reader.ReadInt32();

                    sprite.Bounds = new Rectangle(sprite.X, sprite.Y, sprite.Frames[0].Bounds.Width, sprite.Frames[0].Bounds.Height);
                    Game.Sprites.Add(sprite);
                    Game.AnimatedSprites.Add(sprite);
                }
            }
        }

        public Vector2 TransformGrid(float x, float y)
        {
            return new Vector2(x + y, y - x) + GridOffset;
        }
        public Vector2 UnTransformGrid(float x_, float y_)
        {
            x_ -= GridOffset.X;
            y_ -= GridOffset.Y;
            return new Vector2(x_ - y_, y_ + x_) / 2;
        }
        void UntransformHexMapping()
        {
            var cells = new TCell[Height, Width];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    var pos = UnTransformGrid(x, y);
                    cells[y, x] = Game.Cells[(int)pos.Y, (int)pos.X];
                }
            Game.Cells = cells;
        }

        private void ReadEvents(BinaryReader reader)
        {
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    if (y == 0 && x == 0) continue;
                    short eventId = reader.ReadInt16();
                    short id = reader.ReadInt16();
                    Game.Cells[y, x].EventIdx = eventId;
                }
        }

        private void ReadGroundTiles(BinaryReader reader)
        {
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    int idx = reader.ReadInt32();
                    var tile = new TTile();
                    tile.ImageIndex = idx >> 10;
                    Game.Cells[y, x].GroundTile = tile;
                    Game.Cells[y, x].Collision = (idx & 1) != 0;
                }
        }
        private void ReadRoofBuildTiles(BinaryReader reader)
        {
            Game.RoofTiles.Clear();
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    int idx = reader.ReadInt32();
                    if (idx > 0 && idx < Game.TilesImages.Count)
                    {
                        var roofTile = new TColumnTile();
                        var pos = TransformGrid(x, y);
                        roofTile.X = (int)(pos.X * TGame.TileWidth / 2);
                        roofTile.Y = (int)((pos.Y - 1) * TGame.TileHeight / 2);
                        roofTile.Bounds = new Rectangle(roofTile.X, roofTile.Y, TGame.TileWidth, TGame.TileHeight);
                        var cell = new TCell();
                        cell.Piece = new TTile() { ImageIndex = idx };
                        roofTile.Cells.Add(cell);
                        Game.RoofTiles.Add(roofTile);
                    }
                    else if (idx > 0)
                        ;
                }
        }

        void ReadColumnBuildTiles(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            int sequencesCount = reader.ReadInt32();
            Game.ColumnTiles.Clear();
            for (int i = 0; i < count * sequencesCount; i++)
            {
                var column = new TColumnTile();
                reader.ReadBytes(260);
                var id = reader.ReadInt32();
                var allTilesCount = 0;
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
                        column.Bounds = Rectangle.FromLTRB(left, top, right, bottom);
                        int x_ = reader.ReadInt32();
                        int y_ = reader.ReadInt32();
                        int x = reader.ReadInt32();
                        int y = reader.ReadInt32();
                        int c1 = reader.ReadInt32(); allTilesCount += c1;
                        int c2 = reader.ReadInt32(); allTilesCount += c2;
                        int tilesCount = reader.ReadInt32(); allTilesCount += tilesCount;
                        column.Order = Game.ColumnTiles.Count;
                        column.X = x_;
                        column.Y = y_;
                        for (int m = 0; m < tilesCount; m++)
                        {
                            var cell = new TCell();
                            cell.X = x_;
                            cell.Y = y_ + m * TGame.TileHeight;
                            var tile = new TTile();
                            tile.ImageIndex = reader.ReadInt16();
                            cell.Piece = tile;
                            column.Cells.Add(cell);
                        }
                    }
                }
                Game.ColumnTiles.Add(column);
                reader.ReadBytes(allTilesCount * 4);
            }
            Game.ColumnTiles.Sort();
        }

        Bitmap ReadImage(byte[] pixels, int width, int height)
        {
            var pos = 0;
            var tile = new TPixmap(width, height);
            for (int y = 0; y < tile.Height; y++)
                for (int x = 0; x < tile.Width; x++)
                    tile[x, y] = Rgb16To32(pixels[pos++], pixels[pos++]);
            return tile.Image;
        }

        List<TFrame> ReadView(BinaryReader reader)
        {
            var view = new List<TFrame>();
            reader.ReadInt32();
            var framesCount = (int)reader.ReadInt64();
            for (int k = 0; k < framesCount; k++)
            {
                var frame = new TFrame();
                var left = reader.ReadInt32();
                var top = reader.ReadInt32();
                var right = reader.ReadInt32();
                var bottom = reader.ReadInt32();
                var x = reader.ReadInt32();
                var y = reader.ReadInt32();
                frame.Offset.X = reader.ReadInt32();
                frame.Offset.Y = reader.ReadInt32();
                var width = reader.ReadInt32();
                var height = reader.ReadInt32();
                frame.Bounds = new Rectangle(x, y, width, height);
                var pixelCount = reader.ReadInt32();
                if (pixelCount > 0)
                {
                    var pixels = reader.ReadBytes(pixelCount * 2);
                    frame.Image = ReadImage(pixels, width, height);
                    view.Add(frame);
                }
            }
            return view;
        }

        public void ReadAnimation(BinaryReader reader)
        {
            var animation = new List<TFrame[][]>();
            int sequencesCount = reader.ReadInt32();
            for (int j = 0; j < sequencesCount; j++)
            {
                var sequence = new List<TFrame[]>();
                var header = reader.ReadBytes(264);
                var viewsCount = reader.ReadInt32();
                for (int k = 0; k < viewsCount; k++)
                {
                    var view = ReadView(reader);
                    if (view.Count > 0)
                        sequence.Add(view.ToArray());
                }
                if (sequence.Count > 0)
                    animation.Add(sequence.ToArray());
            }
            Game.Animations.Add(animation.ToArray());
        }

        private void ReadSprites(BinaryReader reader)
        {
            Game.Animations.Clear();
            int animationsCount = reader.ReadInt32();
            for (int i = 0; i < animationsCount; i++)
                ReadAnimation(reader);
            Game.Sprites.Clear();
            Game.AnimatedSprites.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var sprite = new TSprite();
                sprite.ModelIdx = reader.ReadInt32();
                sprite.ModelsSource = Game.Animations;
                for (int j = 0; j < sprite.Frames.Length; j++)
                {
                    var frame = sprite.Frames[j];
                    int left = reader.ReadInt32();
                    int top = reader.ReadInt32();
                    int right = reader.ReadInt32();
                    int bottom = reader.ReadInt32();
                    sprite.X = reader.ReadInt32();
                    sprite.Y = reader.ReadInt32();
                    frame.Bounds = Rectangle.FromLTRB(left, top, right, bottom);
                    frame.Offset = Point.Empty;
                    sprite.Bounds = Rectangle.Union(sprite.Bounds, frame.Bounds);
                }
                if (sprite.Frames.Length > 1)
                    Game.AnimatedSprites.Add(sprite);
                Game.Sprites.Add(sprite);
            }
        }

        public override Image GetBackground()
        {
            var sx = TGame.TileWidth / 2;
            var sy = TGame.TileHeight / 2;
            var bmp = new Bitmap(Width * sx, Height * sy);
            var gc = Graphics.FromImage(bmp);
            var vp = new Rectangle(0, 0, Width - 2, Height - 1);
            //Font font = new Font("Arial", 10f / Zoom);
            if (Game.Cells != null)
            for (int y_ = vp.Top; y_ < vp.Bottom; y_++)
                for (int x_ = vp.Left; x_ < vp.Right; x_ += 2)
                {
                    var x = x_ + (y_ & 1);
                    var y = y_;
                    //var pos = UnTransformGrid(x, y);
                    //if (pos.X < 0 || pos.X >= Game.Cells.GetLength(1) || pos.Y < 0 || pos.Y >= Game.Cells.GetLength(0)) continue;
                    var rc = new RectangleF(x, y, 2f, 2f);
                    //var cell = Game.Cells[(int)pos.Y , (int)pos.X];
                    var cell = Game.Cells[y, x];
                    //if (!cell.IsVisible)
                    //{
                    //    gc.FillRectangle(Brushes.Black, rc);
                    //    continue;
                    //}
                    if (cell.GroundTile != null)
                    {
                        var gTile = Game.GroundTiles[cell.GroundTile.ImageIndex];
                        //rc.Inflate(0.5f, 0.5f);
                        gc.DrawImage(gTile, x * sx, y * sy);
                        //gc.DrawImage(gTile, rc);
                    }
                    var piece = cell.Piece;
                    if (piece is TTile)
                    {
                        //var image = Game.TilesImages[piece.ImageIndex];
                        ////rc.Inflate(0.5f, 0.5f);                        
                        //gc.DrawImage(image, x * sx, y * sy);
                    }
                    else if (piece is TResource)
                    {
                        var image = Game.ResImages[piece.ImageIndex];
                        rc = new RectangleF(x - 1, y, 2, 1);
                        gc.DrawImage(image, rc);
                    }
                    else if (piece is TArtifact)
                    {
                        var image = Game.ArtifactImages[piece.ImageIndex];
                        rc = new RectangleF(x - 1, y, 2, 1);
                        gc.DrawImage(image, rc);
                    }
                    //else if (piece is THero)
                    //{
                    //    var hero = (THero)piece;
                    //    rc.Inflate(-0.25f, -0.25f);
                    //    gc.FillRectangle(new SolidBrush(hero.Player.ID), rc);
                    //    gc.DrawString(hero.Name, font, Brushes.Black, rc);
                    //}
                }
            //var map = new TPixmap(bmp.Width, bmp.Height);
            //map.Image = bmp;
            return bmp;
        }


    }
}
