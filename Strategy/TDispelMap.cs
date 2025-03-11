using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;

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
            GridOffset.X = -(int)Math.Ceiling(offset.X);
            GridOffset.Y = WorldHeight - (int)Math.Ceiling(offset.Y);
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
                    if (idx > 0)
                    {
                        var roofTile = new TSprite();
                        var pos = TransformGrid(x, y);
                        roofTile.X = (int)((pos.X + 1) * TGame.TileWidth / 2);
                        roofTile.Y = (int)((pos.Y + 1) * TGame.TileHeight / 2);
                        roofTile.Bounds = new Rectangle(roofTile.X, roofTile.Y, TGame.TileWidth, TGame.TileHeight);
                        roofTile.Images = new Image[] { Game.TilesImages[idx] };
                        Game.RoofTiles.Add(roofTile);
                    }
                }
        }

        void ReadColumnBuildTiles(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            int viewsCount = reader.ReadInt32();
            Game.ColumnTiles.Clear();
            for (int i = 0; i < count * viewsCount; i++)
            {
                var column = new TColumnTile();
                reader.ReadBytes(260);
                var id = reader.ReadInt32();
                var allTilesCount = 0;
                var sequencesCount = reader.ReadInt32();
                for (int j = 0; j < sequencesCount; j++)
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

        Image ReadImage(byte[] pixels, int width, int height)
        {
            var pos = 0;
            var tile = new TPixmap(width, height);
            for (int y = 0; y < tile.Height; y++)
                for (int x = 0; x < tile.Width; x++)
                    tile[x, y] = Rgb16To32(pixels[pos++], pixels[pos++]);
            return tile.Image;
        }

        List<Image> ReadSequence(BinaryReader reader)
        {
            var sequence = new List<Image>();
            reader.ReadInt32();
            var framesCount = (int)reader.ReadInt64();
            for (int k = 0; k < framesCount; k++)
            {
                var left = reader.ReadInt32();
                var top = reader.ReadInt32();
                var right = reader.ReadInt32();
                var bottom = reader.ReadInt32();
                var x = reader.ReadInt32();
                var y = reader.ReadInt32();
                var mapX = reader.ReadInt32();
                var mapY = reader.ReadInt32();
                var width = reader.ReadInt32();
                var height = reader.ReadInt32();
                var pixelCount = reader.ReadInt32();
                var pixels = reader.ReadBytes(pixelCount * 2);
                var frame = ReadImage(pixels, width, height);
                sequence.Add(frame);
            }
            return sequence;
        }

        public void ReadAnimation(BinaryReader reader)
        {
            var animation = new List<Image[][]>();
            int viewsCount = reader.ReadInt32();
            for (int j = 0; j < viewsCount; j++)
            {
                var view = new List<Image[]>();
                var header = reader.ReadBytes(264);
                var sequencesCount = reader.ReadInt32();
                for (int k = 0; k < sequencesCount; k++)
                {
                    var sequence = ReadSequence(reader);
                    if (sequence.Count > 0)
                        view.Add(sequence.ToArray());
                }
                if (view.Count > 0)
                    animation.Add(view.ToArray());
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
                var framesIndex = reader.ReadInt32();
                sprite.Images = Game.Animations[framesIndex][0][0];
                for (int j = 0; j < sprite.Images.Length; j++)
                {
                    int left = reader.ReadInt32();
                    int top = reader.ReadInt32();
                    int right = reader.ReadInt32();
                    int bottom = reader.ReadInt32();
                    // assuming that all frames have the same size & position
                    sprite.X = reader.ReadInt32();
                    sprite.Y = reader.ReadInt32();
                    sprite.Bounds = Rectangle.FromLTRB(left, top, right, bottom);
                }
                if (sprite.Images.Length > 1)
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
