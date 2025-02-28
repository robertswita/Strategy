using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    internal class TDispelMap
    {
        public TGame Game;
        public static int ChunkSize = 25;
        public int WorldWidth;
        public int WorldHeight;
        //TPixmap ReadTile()
        public void LoadTileSet(string filename)
        {
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var pixels = new byte[fStream.Length];
            fStream.Read(pixels, 0, pixels.Length);
            Game.TileHeight = 32;
            Game.TileWidth = 62;
            Game.TilesImages.Clear();
            int pos = 0;
            while (pos < pixels.Length)
            {
                var tile = new TPixmap(Game.TileWidth, Game.TileHeight);                
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
                        var color = Color.FromArgb(red, green, blue);
                        tile[x, y] = color.ToArgb();
                    }
                }
                Game.TilesImages.Add(tile.Image);
            }
            MapTileSet();
        }

        public void MapTileSet()
        {
            int mapSize = (int)Math.Sqrt(Game.TilesImages.Count);
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
                    cell.Piece = tile;
                    Game.Cells[y, x] = cell;
                }
            Game.Map = map;
        }

        public void LoadMap(string filename)
        {
            LoadTileSet(filename.Substring(0,filename.Length - 4) + ".gtl");
            var fStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var file = new BinaryReader(fStream);
            int width = file.ReadInt32();
            int height = file.ReadInt32();
            if (width <= 0 || width > 30 || height <= 0 || height > 30)
                throw new ArgumentException($"Map have incorrect size of {width}x{height}");
            WorldWidth = width * ChunkSize - 1;
            WorldHeight = height * ChunkSize - 1;
            TBoard.GridShear = new PointF(1f, -1f);
            TBoard.GridOffset.Y = (WorldWidth + WorldHeight) / 2f;
            var map = new Bitmap(WorldWidth, WorldHeight);
            Game.Cells = new TCell[map.Height, map.Width];
            for (int y = 0; y < map.Height; y++)
                for (int x = 0; x < map.Width; x++)
                {
                    var cell = new TCell();
                    cell.Game = Game;
                    cell.X = x;
                    cell.Y = y;
                    Game.Cells[y, x] = cell;
                }
            Game.Map = map;
            int count = file.ReadInt32();
            int size = file.ReadInt32();
            //file.BaseStream.Seek(8, SeekOrigin.Begin);
            file.ReadBytes(count * size * 4 - 8); //skip unknown data
            //workReporter.ReportProgress(1);
            size = file.ReadInt32();
            file.ReadBytes(size * 2);
            //workReporter.ReportProgress(2);
            ReadSpritesBlock(file);
            //workReporter.ReportProgress(3);
            ReadInternalSpriteInfo(file);
            ////workReporter.ReportProgress(4);
            ReadTiledObjectsBlock(file);
            ////workReporter.ReportProgress(5);

            //file.SetPosition(file.BaseStream.Length - map.TiledMapSize.Height * map.TiledMapSize.Width * 4 * 3);

            ReadEventBlock(file);
            ////workReporter.ReportProgress(6);

            ReadTilesAndAccessBlock(file);
            ////workReporter.ReportProgress(7);

            ReadRoofTiles(file);
            ////workReporter.ReportProgress(8);

            //map.InternalSpriteInfos.Sort(new SpriteSorter());
            //map.TiledObjectInfos.Sort(new BtlSorter());

            ////workReporter.ReportProgress(9);

        }

        public Bitmap GenerateMap()
        {
            int diagonalSize = (WorldWidth + WorldHeight) * Game.TileHeight;
            var MapSizeInPixels = new Size(diagonalSize, diagonalSize / 2);
            double xAspect = 0.3;
            double yAspect = 0.2;
            int compensateX = 0;// Game.TileHeight;
            int compensateY = 0;
            var MapNonOccludedStart = new Point(
                (int)(xAspect * MapSizeInPixels.Width - compensateX),
                (int)(yAspect * MapSizeInPixels.Height - compensateY));
            var OccludedMapSize = new Size(
                MapSizeInPixels.Width - MapNonOccludedStart.X * 2,
                MapSizeInPixels.Height - MapNonOccludedStart.Y * 2);
            var bmp = new Bitmap(OccludedMapSize.Width, OccludedMapSize.Height);
            var gc = Graphics.FromImage(bmp);
            for (int y = 0; y < WorldHeight; y++)
                for (int x = 0; x < WorldWidth; x++)
                {
                    var pos = TBoard.TransformGrid(x, y);
                    //if (pos.X < 0 || pos.X >= Game.Map.Width || pos.Y < 0 || pos.Y >= Game.Map.Height)
                    //{ continue; }
                    var cell = Game.Cells[y, x];
                    pos.X *= Game.TileHeight;
                    pos.Y *= Game.TileHeight / 2;
                    pos.X -= MapNonOccludedStart.X;
                    pos.Y -= MapNonOccludedStart.Y;
                    var piece = cell.Piece;
                    if (piece is TTile)
                    {
                        var image = Game.TilesImages[piece.ImageIndex];
                        gc.DrawImage(image, pos.X, pos.Y);
                    }
            }
            return bmp;
        }
        private void ReadEventBlock(BinaryReader reader)
        {
            for (int y = 0; y < WorldHeight; y++)
            {
                for (int x = 0; x < WorldWidth; x++)
                {
                    short eventId = reader.ReadInt16();
                    reader.ReadInt16();
                    Game.Cells[y, x].EventIdx = eventId;
                }
            }
        }

        private void ReadTilesAndAccessBlock(BinaryReader reader)
        {
            for (int y = 0; y < WorldHeight; y++)
            {
                for (int x = 0; x < WorldWidth; x++)
                {
                    int idx = reader.ReadInt32();
                    var tile = new TTile();
                    tile.ImageIndex = idx >> 10;
                    Game.Cells[y, x].Piece = tile;
                    Game.Cells[y, x].Collision = (idx & 1) != 0;
                }
            }
        }
        private void ReadRoofTiles(BinaryReader reader)
        {
            if (reader.BaseStream.Length >= reader.BaseStream.Position + Game.Map.Height * Game.Map.Width * 4)
            {
                for (int y = 0; y < WorldHeight; y++)
                {
                    for (int x = 0; x < WorldWidth; x++)
                    {
                        int bytes = reader.ReadInt32();
                        int btlBytes = bytes;
                        //map.SetRoofBtl(x, y, btlBytes);
                    }
                }
            }
        }

        private void ReadInfoChunk(BinaryReader file)
        {
            file.ReadBytes(264);

            int s8 = file.ReadInt32();
            int s0_1 = file.ReadInt32();
            int s1 = file.ReadInt32();
            int s0_2 = file.ReadInt32();

            if (s8 != 8 && s0_1 != 0 && s0_2 != 0 && s1 != 1)
            {
                ;//Metrics.Count(MetricFile.MapReadMetric, Path.GetFileName(filename), "WrongSequence");
            }

            int v1 = file.ReadInt32();
            int v2 = file.ReadInt32();
            int v3 = file.ReadInt32();
            int v4 = file.ReadInt32();
            int x = file.ReadInt32();
            int y = file.ReadInt32();
            int v7 = file.ReadInt32();
            int v8 = file.ReadInt32();

            int c1 = file.ReadInt32();
            int c2 = file.ReadInt32();
            int c3 = file.ReadInt32();

            int[] ids = new int[c3];
            for (int i = 0; i < c3; i++)
            {
                ids[i] = file.ReadInt16();
            }

            //map.AddTiledObject(x, y, ids);
            //var tile = new TTile();
            //tile.ImageIndex = ids[0];
            //Game.Cells[y, x].Piece = tile;

            file.ReadBytes(84);

            file.ReadBytes((c1 + c2 + c3) * 4);
        }

        void ReadTiledObjectsBlock(BinaryReader reader)
        {
            int bundlesCount = reader.ReadInt32();
            int number1 = reader.ReadInt32();
            for (int i = 0; i < bundlesCount; i++)
            {
                ReadInfoChunk(reader);
            }

            //int backPos = 20;
            //reader.SetPosition(reader.BaseStream.Position - backPos);
            //int lastPos = 0;
            //for (int i = 0; i < backPos; i++)
            //{
            //    byte v = reader.ReadByte();
            //    if (v == 1)
            //    {
            //        lastPos = i;
            //    }
            //}
            //int toUndo = backPos - lastPos - 4;
            ////Metrics.Count(MetricFile.MapReadMetric, Path.GetFileName(filename), "ToUndo", toUndo);
            //reader.SetPosition(reader.BaseStream.Position - toUndo);
        }

        void ReadInternalSpriteInfo(BinaryReader reader)
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


        //private SequenceInfo GetSequenceInfo(BinaryReader reader)
        //{
        //    var info = new SequenceInfo();

        //    int stamp = reader.ReadInt32();
        //    if (stamp == 8)
        //    {
        //        stamp = reader.ReadInt32();
        //    }
        //    if (stamp == 0)
        //    {
        //        int framesCount = reader.ReadInt32();
        //        int stamp0_2 = reader.ReadInt32();
        //        info.FrameInfos = new ImageInfo[framesCount];
        //    }

        //    //if (info.FrameInfos.Length == 0)
        //    //{
        //    //    Metrics.Count(MetricFile.SpriteFileMetric, filename, "zeroFrame");
        //    //    Metrics.Count(MetricFile.SpriteFileMetric, "zeroFrames");
        //    //}
        //    info.SequenceStartPosition = reader.BaseStream.Position;
        //    for (int i = 0; i < info.FrameInfos.Length; i++)
        //    {
        //        //try
        //        {
        //            info.FrameInfos[i] = GetImageInfo();
        //            reader.Skip(info.FrameInfos[i].SizeBytes);
        //            //Metrics.Gauge(MetricFile.SpriteOffsetMetric, $"file.{filename}", info.FrameInfos[i].ImageStartPosition);
        //        }
        //        //catch (FrameInfoException)
        //        //{
        //        //    var oldFrames = info.FrameInfos;
        //        //    info.FrameInfos = new ImageInfo[i];
        //        //    for (int j = 0; j < info.FrameInfos.Length; j++)
        //        //    {
        //        //        info.FrameInfos[j] = oldFrames[j];
        //        //    }
        //        //}
        //    }
        //    info.SequenceEndPosition = reader.BaseStream.Position;
        //    reader.BaseStream.Seek(info.SequenceStartPosition, SeekOrigin.Begin);


        //    //Metrics.Count(MetricFile.SpriteFileMetric, filename, "spriteFrameCount", info.FrameInfos.Length);
        //    //Metrics.Count(MetricFile.SpriteFileMetric, "allFrames", info.FrameInfos.Length);
        //    return info;
        //}


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
                    var color = Color.FromArgb(red, green, blue);
                    tile[x, y] = color.ToArgb();
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

        private void ReadSpritesBlock(BinaryReader file)
        {
            int spritesCount = file.ReadInt32();
            for (int i = 0; i < spritesCount; i++)
            {
                int imageStamp = file.ReadInt32();
                int imageOffset = imageStamp == 6 ? 1904 : (imageStamp == 9 ? 2996 : throw new NotImplementedException($"Unexpected imageStamp {imageStamp}"));
                file.ReadBytes(264);
                Game.Sprites.Add(LoadSprite(file));
                file.ReadBytes(imageOffset);
            }
        }



    }
}
