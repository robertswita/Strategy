using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Strategy
{
    internal class TDispelMap
    {
        public TGame Game;
        public static int ChunkSize = 25;
        public int WorldWidth;
        public int WorldHeight;
        //TPixmap ReadTile()
        public List<Image> LoadTileSet(string filename, string ext)
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
                    {
                        var byte0 = pixels[pos++];
                        var byte1 = pixels[pos++];
                        int red = byte1 & 0xF8;
                        int green = byte1 << 5 & 0xFF | byte0 >> 3 & 0xFC;
                        int blue = byte0 << 3 & 0xFF;
                        int alpha = red + green + blue > 0 ? 0xFF : 0;
                        tile[x, y] = alpha << 24 | red << 16 | green << 8 | blue;
                    }
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
            Game.Map = map;
        }

        public void LoadMap(string filename)
        {
            Game.GroundTiles = LoadTileSet(filename, ".gtl");
            Game.TilesImages = LoadTileSet(filename, ".btl");
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fStream);
            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            if (width <= 0 || width > 30 || height <= 0 || height > 30)
                throw new ArgumentException($"Map have incorrect size of {width}x{height}");
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
            TBoard.GridShear = new PointF(1, -1f);
            TBoard.GridOffset.X = -(int)offset.X;
            TBoard.GridOffset.Y = WorldHeight - (int)offset.Y;
            //TBoard.WorldHeight = WorldHeight;
            var map = new Bitmap((int)mapSize.X, (int)mapSize.Y);
            //var map = new Bitmap(WorldWidth, WorldHeight);
            Game.Map = map;
            int propCount = reader.ReadInt32();
            int tileCount = reader.ReadInt32();
            reader.ReadBytes((tileCount - 1) * propCount * sizeof(int));
            tileCount = reader.ReadInt32();
            reader.ReadBytes(tileCount * propCount);
            ReadSprites(reader);
            ReadInternalSprites(reader);
            ReadTiledObjects(reader);
            ReadEvents(reader);
            ReadGroundTiles(reader);           
            ReadRoofTiles(reader);
            Game.TiledObjects.Sort();
            //map.InternalSpriteInfos.Sort(new SpriteSorter());
            //map.TiledObjectInfos.Sort(new BtlSorter());
        }

        //public Bitmap GenerateMap()
        //{
        //    int diagonalSize = (WorldWidth + WorldHeight) * Game.TileHeight;
        //    var hexMapSize = new Vector2(diagonalSize, diagonalSize / 2);
        //    var ratio = new Vector2(0.4f, 0.6f);
        //    var mapSize = ratio * hexMapSize;
        //    var offset = (hexMapSize - mapSize) / 2;
        //    var bmp = new Bitmap((int)mapSize.X, (int)mapSize.Y);
        //    var gc = Graphics.FromImage(bmp);
        //    for (int y = 0; y < WorldHeight; y++)
        //        for (int x = 0; x < WorldWidth; x++)
        //        {
        //            var pos = TBoard.TransformGrid(x, y);
        //            pos.X *= Game.TileHeight;
        //            pos.Y *= Game.TileHeight / 2;
        //            pos.X -= offset.X;
        //            pos.Y -= offset.Y;
        //            //if (pos.X < 0 || pos.X >= bmp.Width || pos.Y < 0 || pos.Y >= bmp.Height)
        //            //    continue;
        //            var cell = Game.Cells[y, x];
        //            var piece = cell.Piece;
        //            if (piece is TTile)
        //            {
        //                var image = Game.GroundTiles[piece.ImageIndex];
        //                gc.DrawImage(image, (int)pos.X, (int)pos.Y);
        //            }
        //    }
        //    return bmp;
        //}
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
        private void ReadRoofTiles(BinaryReader reader)
        {
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    int idx = reader.ReadInt32();
                    //if (idx < Game.TilesImages.Count)
                    //{
                    //    var tile = new TTile();
                    //    tile.ImageIndex = idx;
                    //    Game.Cells[y, x].Piece = tile;
                    //    //map.SetRoofBtl(x, y, btlBytes);
                    //}
                }
        }

        void ReadTiledObjects(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            int blockCount = reader.ReadInt32();
            Game.TiledObjects.Clear();
            for (int n = 0; n < count * blockCount; n++)
            {
                var column = new TColumn();
                reader.ReadBytes(260);
                var id = reader.ReadInt32();
                var magic = reader.ReadBytes(16);
                int left = reader.ReadInt32();
                int top = reader.ReadInt32();
                int right = reader.ReadInt32();
                int bottom = reader.ReadInt32();
                int x_ = reader.ReadInt32();
                int y_ = reader.ReadInt32();
                int x = reader.ReadInt32();
                int y = reader.ReadInt32();
                int c1 = reader.ReadInt32();
                int c2 = reader.ReadInt32();
                int c3 = reader.ReadInt32();
                column.Order = Game.TiledObjects.Count;
                column.X = x_;
                column.Y = y_;
                Game.TiledObjects.Add(column);
                for (int i = 0; i < c3; i++)
                {
                    var cell = new TCell();
                    cell.X = x_;
                    cell.Y = y_ + i * TGame.TileHeight;
                    //Game.Cells[y + i, x - i];
                    //var cell = Game.Cells[(int)pos.Y + i, (int)pos.X - i];
                    var tile = new TTile();
                    tile.ImageIndex = reader.ReadInt16();
                    cell.Piece = tile;
                    column.Cells.Add(cell);
                }
                reader.ReadBytes(84);
                reader.ReadBytes((c1 + c2 + c3) * 4);
            }
        }

        void ReadInternalSprites(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int sprId = reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                int sprBottomRightX = reader.ReadInt32();
                int sprBottomRightY = reader.ReadInt32();
                int sprX = reader.ReadInt32();
                int sprY = reader.ReadInt32();
                if (sprId >= 0 && sprId < Game.Sprites.Count)
                {
                    var sprite = Game.Sprites[sprId];
                    sprite.X = sprX; 
                    sprite.Y = sprY;
                    sprite.Width = sprBottomRightX - sprX;
                    sprite.Height = sprBottomRightY - sprY;
                    int restOfFramesByteCount = (sprite.Frames.Count - 1) * 6 * 4;
                    reader.ReadBytes(restOfFramesByteCount);
                    //map.AddSriteInfo(sprId, sprX, sprY, sprBottomRightX, sprBottomRightY);
                }
                //else
                //{
                //    Metrics.Count(MetricFile.MapReadMetric, filename, "missedInternalSprite");
                //}
            }
        }

        private TPixmap LoadImageFromFile(BinaryReader reader, int width, int height)
        {
            var tile = new TPixmap(width, height);
            for (int y = 0; y < tile.Height; y++)
                for (int x = 0; x < tile.Width; x++)
                {
                    var byte0 = reader.ReadByte();
                    var byte1 = reader.ReadByte();
                    int red = byte1 & 0xF8;
                    int green = byte1 << 5 & 0xFF | byte0 >> 3 & 0xFC;
                    int blue = byte0 << 3 & 0xFF;
                    int alpha = 0xFF;
                    tile[x, y] = alpha << 24 | red << 16 | green << 8 | blue;
                }
            return tile;
        }

        public TSprite LoadSprite(BinaryReader reader)
        {
            //var info = GetSequenceInfo(reader);
            //var sequence = new SpriteSequence(info, true);
            int framesCount = 0;
            int stamp = reader.ReadInt32();
            if (stamp == 8)
            {
                stamp = reader.ReadInt32();
            }
            if (stamp == 0)
            {
                framesCount = reader.ReadInt32();
                int stamp0_2 = reader.ReadInt32();
                //info.FrameInfos = new ImageInfo[framesCount];
            }

            var sprite = new TSprite();
            for (int i = 0; i < framesCount; i++)
            {
                reader.ReadBytes(6 * 4);//some data
                var originX = reader.ReadInt32();
                var originY = reader.ReadInt32();
                var width = reader.ReadInt32();
                var height = reader.ReadInt32();
                var sizeBytes = reader.ReadUInt32() * 2;
                //info.ImageStartPosition = reader.BaseStream.Position;
                if (width < 1 || height < 1)
                {
                    throw new Exception();//fix for soulnet.spr missing one frame
                }

                //var frameInfo = info.FrameInfos[i];
                //reader.BaseStream.Seek(frameInfo.ImageStartPosition, SeekOrigin.Begin);
                var image = LoadImageFromFile(reader, width, height);
                //var frame = new SpriteFrame(frameInfo.OriginX, frameInfo.OriginY, image);
                //sequence.SetFrame(frame, i);
                sprite.Frames.Add(image);
            }
            return sprite;
        }

        private void ReadSprites(BinaryReader file)
        {
            Game.Sprites.Clear();
            int spritesCount = file.ReadInt32();
            for (int i = 0; i < spritesCount; i++)
            {
                int imageStamp = file.ReadInt32();
                file.ReadBytes(264);
                Game.Sprites.Add(LoadSprite(file));
                int imageOffset = imageStamp * 364 - 280;
                file.ReadBytes(imageOffset);
            }
        }



    }
}
