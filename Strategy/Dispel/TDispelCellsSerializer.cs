using System;
using System.Drawing;
using System.IO;

namespace Strategy.Dispel
{
    class TDispelCellsSerializer
    {
        public TDispelMap Map;
        TCell[,] TransformToHexMapping()
        {
            var cells = new TCell[Map.WorldHeight, Map.WorldWidth];
            for (int y = 0; y < Map.Height; y++)
                for (int x = 0; x < Map.Width; x++)
                {
                    var pos = Map.Map2WorldTransform(x, y);
                    if (pos.Y >= 0 && pos.Y < Map.WorldHeight && pos.X >= 0 && pos.X < Map.WorldWidth)
                    {
                        var cell = Map.Cells[y, x];
                        //cell.X = (int)pos.X;
                        //cell.Y = (int)pos.Y;
                        cells[(int)pos.Y, (int)pos.X] = cell;
                    }
                }
            return cells;
        }

        public void Write(BinaryWriter writer)
        {
            var cells = TransformToHexMapping();
            foreach (var cell in cells)
            {
                var eventIdx = cell == null ? 0 : cell.EventIdx;
                writer.Write((short)eventIdx);
                writer.Write((short)0);
            }
            foreach (var cell in cells)
            {
                var idx = 0;
                if (cell != null)
                {
                    idx = cell.Floor.Index << 10;
                    if (cell.Collision) idx |= 1;
                }
                writer.Write(idx);
            }
            var bytes = new byte[4 * Map.WorldHeight * Map.WorldWidth];
            foreach (var roofTile in Map.Roofs)
            {
                var tile = roofTile.Tiles[0];
                var pos = Map.Map2WorldTransform(tile.X, tile.Y);
                var linPos = (int)pos.Y * Map.WorldWidth + (int)pos.X;
                var num = tile.Index;
                bytes[4 * linPos + 0] = (byte)num;
                bytes[4 * linPos + 1] = (byte)(num >> 8);
                bytes[4 * linPos + 2] = (byte)(num >> 16);
                bytes[4 * linPos + 3] = (byte)(num >> 24);
            }
            writer.Write(bytes);
        }
        TCell[,] UntransformFromHexMapping()
        {
            var cells = new TCell[Map.Height, Map.Width];
            for (int y = 0; y < Map.Height; y++)
                for (int x = 0; x < Map.Width; x++)
                {
                    var pos = Map.Map2WorldTransform(x, y);
                    if (pos.Y >= 0 && pos.Y < Map.WorldHeight && pos.X >= 0 && pos.X < Map.WorldWidth)
                    {
                        var cell = Map.Cells[(int)pos.Y, (int)pos.X];
                        cell.X = x;
                        cell.Y = y;
                        var cellPos = cell.Position;
                        cell.Bounds = new Rectangle((int)cellPos.X, (int)cellPos.Y, TDispelTile.Width, TDispelTile.Height);
                        cell.Bounds.Offset(-TDispelTile.Width / 2, -TDispelTile.Height / 2);
                        cells[y, x] = cell;
                    }
                    else
                        cells[y, x] = Map.Cells[0, 0];
                }
            return cells;
        }

        public void Read(BinaryReader reader)
        {
            Map.Cells = new TCell[Map.WorldHeight, Map.WorldWidth];
            for (int y = 0; y < Map.WorldHeight; y++)
                for (int x = 0; x < Map.WorldWidth; x++)
                {
                    var cell = new TCell();
                    cell.Map = Map;
                    cell.X = x;
                    cell.Y = y;
                    Map.Cells[y, x] = cell;
                }
            foreach (var cell in Map.Cells)
            {
                short eventId = reader.ReadInt16();
                short id = reader.ReadInt16();
                cell.EventIdx = eventId;
            }
            TCell firstCell = null;
            TCell lastCell = null;
            //foreach (var cell in Game.Cells)
            for (int y = 0; y < Map.WorldHeight; y++)
                for (int x = 0; x < Map.WorldWidth; x++)
                {
                    var cell = Map.Cells[y, x];
                    int idx = reader.ReadInt32();
                    if (idx > 0)
                    {
                        if (firstCell == null)
                            firstCell = cell;
                        lastCell = cell;
                    }
                    //var tile = new TTile();
                    cell.Floor = Map.Floors[idx >> 10];
                    cell.Collision = (idx & 0x3FF) != 0;
                }
            //GridOffset.X = firstCell.X + firstCell.Y;
            //GridOffset.Y = firstCell.Y - firstCell.X;
            //Width = 2 * (WorldWidth - (int)GridOffset.X);
            //Height = -(int)GridOffset.Y;

            //offset = (hexMapSize - mapSize) / 2;
            //var fl = new Point(lastCell.X - firstCell.X, lastCell.Y - firstCell.Y);
            //Width = fl.X + fl.Y + 1;
            //Height = (fl.Y - fl.X + 1) / 2;
            ReadRoofs(reader);
            Map.Cells = UntransformFromHexMapping();
        }

        private void ReadRoofs(BinaryReader reader)
        {
            Map.Roofs.Clear();
            for (int y = 0; y < Map.WorldHeight; y++)
                for (int x = 0; x < Map.WorldWidth; x++)
                {
                    int idx = reader.ReadInt32();
                    if (idx > 0 && idx < Map.WallTiles.Count)
                    {
                        var roofTile = new TWall();
                        var tile = Map.WallTiles[idx];
                        var pos = Map.World2ViewTransform(x, y);
                        roofTile.X = (int)(pos.X);
                        roofTile.Y = (int)(pos.Y);
                        roofTile.Tiles.Add(tile);
                        roofTile.Bounds = new Rectangle(roofTile.X, roofTile.Y, TDispelTile.Width, TDispelTile.Height);
                        Map.Roofs.Add(roofTile);
                    }
                }
        }

    }
}
