using Strategy.Diablo;
using Strategy.Dispel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Strategy
{
    class TMapConverter
    {
        public TGame Game;
        public TDiabloMap DiabloMap;

        public void Export(string filename)
        {
            //var subMaps = SplitDispelMap();
            var subMaps = new TDispelMap[1, 1] { { (TDispelMap)Game.Map } };
            var baseName = filename.Substring(0, filename.Length - 4);
            for (int y = 0; y < subMaps.GetLength(0); y++)
                for (int x = 0; x < subMaps.GetLength(1); x++)
                {
                    var subName = subMaps.Length == 1 ? filename : $"{baseName}{y}{x}.ds1";
                    Game.Map = subMaps[y, x];
                    DiabloMap = new TDiabloMap();
                    DiabloMap.ActNo = 0;
                    DiabloMap.ReadPalette(subName);
                    var size = (Game.Map.Width + 2 * Game.Map.Height) / 5;// + 2;
                    DiabloMap.WorldWidth = size;
                    DiabloMap.WorldHeight = size;
                    DiabloMap.GridOffset = new Vector2(-2f * size + 2f, 2f * size - 2);
                    ExportFloors();
                    ExportWalls();
                    DiabloMap.WriteTileSet(subName, ".dt1");
                    DiabloMap.WriteMap(subName);
                }
        }

        public TDispelMap[,] SplitDispelMap()
        {
            var maxSize = 250;
            var colsCount = (Game.Map.Width + maxSize - 1) / maxSize;
            var subMaps = new TDispelMap[colsCount, colsCount];
            var subMapSizeW = (Game.Map.Width + colsCount - 1) / colsCount;
            var subMapSizeH = subMapSizeW * 3 / 4;
            for (int rowIdx = 0; rowIdx < colsCount; rowIdx++)
                for (int colIdx = 0; colIdx < colsCount; colIdx++)
                {
                    var subMap = new TDispelMap();
                    subMap.Width = subMapSizeW;
                    subMap.Height = subMapSizeH;
                    subMap.Floors = Game.Map.Floors;
                    subMaps[rowIdx, colIdx] = subMap;
                    subMap.Cells = new TCell[subMapSizeH, subMapSizeW];
                    for (int y = 0; y < subMapSizeH; y++)
                        for (int x = 0; x < subMapSizeW; x++)
                        {
                            var cell = new TCell();
                            var posY = rowIdx * subMapSizeH + y;
                            var posX = colIdx * subMapSizeW + x;
                            if (posY < Game.Map.Height && posX < Game.Map.Width)
                            {
                                var offsetX = -TDispelTile.Width / 2 * colIdx * subMapSizeW;
                                var offsetY = -TDispelTile.Height * rowIdx * subMapSizeH;
                                cell = Game.Map.Cells[posY, posX];
                                cell.Bounds.Offset(offsetX, offsetY);
                                var wall = cell.Wall;
                                if (wall != null)
                                {
                                    wall.X += offsetX;
                                    wall.Y += offsetY;
                                    subMap.Walls.Add(wall);
                                }
                            }  
                            subMap.Cells[y, x] = cell;
                        }
                }
            return subMaps;
        }

        public void ExportFloors2()
        {
            var mapViewSize = Game.Map.Map2ViewTransform(Game.Map.Width, Game.Map.Height);
            var dispelBmp = new Bitmap((int)mapViewSize.X, (int)mapViewSize.Y);
            var gc = Graphics.FromImage(dispelBmp);
            var floorRefs = new List<TCell>[Game.Map.Floors.Count];
            for (int y = 0; y < Game.Map.Height; y++)
                for (int x = 0; x < Game.Map.Width; x++)
                {
                    var cell = Game.Map.Cells[y, x];
                    var bounds = cell.Bounds;
                    if (cell.Floor != null)
                    {
                        var refCells = floorRefs[cell.Floor.Index];
                        if (refCells == null)
                        {
                            refCells = new List<TCell>();
                            floorRefs[cell.Floor.Index] = refCells;
                        }
                        refCells.Add(cell);
                        gc.DrawImage(cell.Floor.Image, bounds.X, bounds.Y);
                    }
                }
            var size = DiabloMap.WorldWidth;
            var dispelBmpRect = new Rectangle(0, 0, dispelBmp.Width, dispelBmp.Height);
            dispelBmpRect.Inflate(-TDispelTile.Width / 2, -TDispelTile.Height);
            //dispelBmpRect.Offset(TDispelTile.Width / 2, 0);
            var duplicatesCount = 0;
            var visAdded = false;
            DiabloMap.Cells = new TCell[5 * size, 5 * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    var diabloCell = new TCell();
                    diabloCell.X = 5 * x;
                    diabloCell.Y = 5 * y;
                    var diabloCellIdx = y * size + x;
                    DiabloMap.Cells[diabloCell.Y, diabloCell.X] = diabloCell;
                    var pos = DiabloMap.World2ViewTransform(diabloCell.X, diabloCell.Y);
                    if (!dispelBmpRect.Contains((int)pos.X, (int)pos.Y)) continue;
                    var floor = new TDiabloWall();
                    floor.Width = 5 * TDiabloTile.Width;
                    floor.Height = -8 * TDiabloTile.Height;
                    floor.Bounds = new Rectangle((int)pos.X - floor.Width / 2, (int)pos.Y - 8, floor.Width, floor.Width / 2);
                    var isEmpty = true;
                    for (int u = 0; u < 5; u++)
                        for (int v = 0; v < 5; v++)
                        {
                            var tile = new TDiabloTile();
                            var tilePos = DiabloMap.World2ViewTransform(diabloCell.X + 4 - v, diabloCell.Y + 4 - u);
                            tilePos.X += 2 * TDiabloTile.Width;
                            tile.X = (int)(tilePos.X - pos.X);
                            tile.Y = (int)(tilePos.Y - pos.Y);
                            var tileBounds = new Rectangle(0, 0, TDiabloTile.Width, TDiabloTile.Height);
                            tileBounds.X = (int)tilePos.X - TDiabloTile.Width / 2;
                            tileBounds.Y = (int)tilePos.Y - TDiabloTile.Height / 2;
                            var cellPos = Game.Map.View2WorldTransform(tilePos.X, tilePos.Y);
                            cellPos = Game.Map.World2MapTransform((int)(cellPos.X), (int)(cellPos.Y));
                            //var cellPos = Game.Map.View2MapTransform(tilePos.X, tilePos.Y);
                            if (cellPos.X < Game.Map.Width && cellPos.Y < Game.Map.Height)
                            {
                                var cell = Game.Map.Cells[(int)cellPos.Y, (int)cellPos.X];
                                if (cell.Collision)
                                {
                                    var cellBounds = cell.Bounds;
                                    cellBounds.Inflate(-1, -1);
                                    if (Rectangle.Union(cellBounds, tileBounds).Equals(cellBounds))
                                    //if (diabloCell.Wall == null) diabloCell.Wall = new TDiabloWall();
                                    //(diabloCell.Wall as TDiabloWall).TilesFlags[floor.Tiles.Count] |= 1;
                                        floor.TilesFlags[5 * u + 4 - v] |= 1;
                                }
                            }
                            var tileBmp = new Bitmap(tileBounds.Width, tileBounds.Height);
                            var tileGc = Graphics.FromImage(tileBmp);
                            tileGc.DrawImage(dispelBmp, 0, 0, tileBounds, GraphicsUnit.Pixel);
                            tile.Image = tileBmp;
                            tile.Encode();
                            isEmpty &= tile.IsEmpty;
                            floor.Tiles.Add(tile);
                        }
                    if (isEmpty) continue;  
                    var duplicatePos = Game.Map.View2MapTransform(pos.X, pos.Y);
                    {
                        var cell = Game.Map.Cells[(int)duplicatePos.Y, (int)duplicatePos.X];
                        if (cell.Floor != null)
                        {
                            if (cell.EventIdx > 0 && cell.EventIdx <= 61 && !visAdded)
                            {
                                visAdded = true;
                                var vis = new TDiabloWall();
                                vis.Type = (int)TWallType.Special1;
                                vis.Hidden = true;
                                vis.Style = 0;
                                vis.Tiles = floor.Tiles;
                                //wall.Style = cell.EventIdx >> 6;
                                //wall.Seq = cell.EventIdx & 63;
                                vis.Width = 5 * TDiabloTile.Width;
                                vis.Height = -8 * TDiabloTile.Height;
                                diabloCell.Wall = new TDiabloWall();
                                diabloCell.Wall.Tiles.Add(vis);
                                DiabloMap.Walls.Add(vis);
                            }
                            var floorRef = floorRefs[cell.Floor.Index];
                            for (var i = 0; i < floorRef.Count; i++)
                            {
                                var prevCell = floorRef[i];
                                //if (prevCell == cell) continue;
                                var cellPos = Game.Map.Map2ViewTransform(prevCell.X, prevCell.Y);
                                cellPos = DiabloMap.View2WorldTransform(cellPos.X, cellPos.Y);
                                cellPos.X = (int)((cellPos.X + 2.5f) / 5);
                                cellPos.Y = (int)((cellPos.Y + 2.5f) / 5);
                                if (cellPos.Y < 0 || cellPos.Y >= size || cellPos.X < 0 || cellPos.X >= size) continue;
                                var prevDiabloCell = DiabloMap.Cells[(int)cellPos.Y * 5, (int)cellPos.X * 5];
                                if (prevDiabloCell != null && prevDiabloCell.Floor != null && prevDiabloCell != diabloCell)
                                {
                                    var prevFloor = (TDiabloWall)prevDiabloCell.Floor;
                                    var j = 0;
                                    for (; j < floor.TilesFlags.Length; j++)
                                        if (prevFloor.TilesFlags[j] != floor.TilesFlags[j])
                                            break;
                                    //if (j < floor.TilesFlags.Length)
                                    //    continue;
                                    if (floor.IsEqual(prevFloor))
                                    {
                                        diabloCell.Floor = prevFloor;
                                        duplicatesCount++;
                                        floorRef.RemoveAt(i);

                                        //diabloCell.Wall = new TDiabloWall();
                                        //diabloCell.Wall.Tiles.Add(prevFloor);
                                        break;
                                    }
                                }
                            }
                            //if (diabloCell.Floor != null)
                            //    continue;
                        }
                    }
                    if (diabloCell.Floor == null)
                    {
                        diabloCell.Floor = floor;
                        DiabloMap.Walls.Add(floor);
                    }
                    floor = (TDiabloWall)diabloCell.Floor;
                    //diabloCell.Floor.Index = diabloCellIdx;
                    floor.Style = floor.Index >> 6 & 63;
                    floor.Seq = floor.Index & 63;
                    floor.Type = (int)TWallType.LowerWall;
                    floor.Height = -96;
                    if (diabloCell.Wall == null)
                    {
                        diabloCell.Wall = new TDiabloWall();
                        diabloCell.Wall.Tiles.Add(floor);
                    }
                }
            dispelBmp.Dispose();

        }
        public void ExportFloors()
        {
            var mapViewSize = Game.Map.Map2ViewTransform(Game.Map.Width, Game.Map.Height);
            var dispelBmp = new Bitmap((int)mapViewSize.X, (int)mapViewSize.Y);
            var gc = Graphics.FromImage(dispelBmp);
            var floorRefs = new List<TCell>[Game.Map.Floors.Count];
            for (int y = 0; y < Game.Map.Height; y++)
                for (int x = 0; x < Game.Map.Width; x++)
                {
                    var cell = Game.Map.Cells[y, x];
                    var bounds = cell.Bounds;
                    if (cell.Floor != null)
                    {
                        var refCells = floorRefs[cell.Floor.Index];
                        if (refCells == null)
                        {
                            refCells = new List<TCell>();
                            floorRefs[cell.Floor.Index] = refCells;
                        }
                        refCells.Add(cell);
                        gc.DrawImage(cell.Floor.Image, bounds.X, bounds.Y);
                    }
                }
            var size = DiabloMap.WorldWidth;
            var dispelBmpRect = new Rectangle(0, 0, dispelBmp.Width, dispelBmp.Height);
            dispelBmpRect.Inflate(-TDispelTile.Width / 2, -TDispelTile.Height);
            //dispelBmpRect.Offset(TDispelTile.Width / 2, 0);
            var duplicatesCount = 0;
            var visAdded = false;
            var commonFloor = new TDiabloWall();
            commonFloor.Width = 5 * TDiabloTile.Width;
            commonFloor.Height = -8 * TDiabloTile.Height;
            DiabloMap.Floors.Add(commonFloor);
            DiabloMap.Walls.Add(commonFloor);
            //var floorIdx = 0;
            DiabloMap.Cells = new TCell[5 * size, 5 * size];
            var offsetX = -16;
            var offsetY = 64;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    var diabloCell = new TCell();
                    diabloCell.X = 5 * x;
                    diabloCell.Y = 5 * y;
                    diabloCell.Floor = commonFloor;
                    diabloCell.Wall = new TDiabloWall();
                    var diabloCellIdx = y * size + x;
                    DiabloMap.Cells[diabloCell.Y, diabloCell.X] = diabloCell;
                    var pos = DiabloMap.World2ViewTransform(diabloCell.X, diabloCell.Y);
                    //pos.Y -= 5 * TDiabloTile.Height;
                    if (!dispelBmpRect.Contains((int)pos.X, (int)pos.Y)) continue;
                    //if ((x & 1) != 0 || (y & 1) != 0) continue;
                    var floor = new TDiabloWall();
                    floor.Width = 5 * TDiabloTile.Width / 2;
                    floor.Height = -10 * TDiabloTile.Height / 2;
                    //floor.Width = 160;
                    //floor.Height = -80;
                    floor.Bounds = new Rectangle((int)pos.X + offsetX, (int)pos.Y - 96 + offsetY, floor.Width, -floor.Height);
                    //var segX = x & 1;
                    //var segY = y & 1;
                    //if (segX == 0 && segY == 1)
                    //{
                    //    floor.Width = 3 * TDiabloTile.Width;
                    //    floor.Height = -3 * TDiabloTile.Width;
                    //}
                    //else if (segX == 0 && segY == 1)
                    //{
                    //    floor.Width = 3 * TDiabloTile.Width;
                    //    floor.Height = -2 * TDiabloTile.Width;
                    //}
                    //else if (segX == 1 && segY == 0)
                    //{
                    //    floor.Width = 2 * TDiabloTile.Width;
                    //    floor.Height = -3 * TDiabloTile.Width;
                    //}
                    //else
                    //{
                    //    floor.Width = 2 * TDiabloTile.Width;
                    //    floor.Height = -2 * TDiabloTile.Width;
                    //}
                    //floor.Bounds = new Rectangle((int)pos.X + segX * 16, (int)pos.Y - 96 + segX * 40 + segY * 16, floor.Width, -floor.Height);
                    for (int u = 0; u < 5; u++)
                        for (int v = 0; v < 5; v++)
                        {
                            var tilePos = DiabloMap.World2ViewTransform(diabloCell.X + 4 - v, diabloCell.Y + 4 - u);
                            tilePos.X += 2 * TDiabloTile.Width;
                            //tilePos.Y -= 5 * TDiabloTile.Height;
                            var cellPos = Game.Map.View2WorldTransform(tilePos.X, tilePos.Y);
                            cellPos = Game.Map.World2MapTransform((int)(cellPos.X), (int)(cellPos.Y));
                            //var cellPos = Game.Map.View2MapTransform(tilePos.X, tilePos.Y);
                            if (cellPos.X >= 0 && cellPos.X < Game.Map.Width && cellPos.Y >= 0 && cellPos.Y < Game.Map.Height)
                            {
                                var cell = Game.Map.Cells[(int)cellPos.Y, (int)cellPos.X];
                                if (cell.Collision)
                                {
                                    var cellBounds = cell.Bounds;
                                    var tileBounds = new Rectangle((int)tilePos.X - 16, (int)tilePos.Y - 16, 32, 32);
                                    //cellBounds.Inflate(-1, -1);
                                    if (Rectangle.Union(cellBounds, tileBounds).Equals(cellBounds))
                                        floor.TilesFlags[5 * u + 4 - v] |= 1;
                                }
                            }

                        }
                    var isEmpty = true;
                    for (int u = 0; u < 3; u++)
                        for (int v = 0; v < 3; v++)
                        {
                            var tile = new TDiabloTile();
                            var tileBmp = new Bitmap(TDiabloTile.Width, 2 * TDiabloTile.Height);
                            var tilePos = new Vector2(pos.X + v * tileBmp.Width, pos.Y + u * tileBmp.Height);
                            tile.X = v * tileBmp.Width;
                            tile.Y = u * tileBmp.Height - 96;
                            var tileBounds = new Rectangle(0, 0, tileBmp.Width, tileBmp.Height);
                            tileBounds.X = (int)pos.X + tile.X + offsetX;
                            tileBounds.Y = (int)pos.Y + tile.Y + offsetY;
                            tileBounds = Rectangle.Intersect(tileBounds, floor.Bounds);
                            var tileGc = Graphics.FromImage(tileBmp);
                            tileGc.DrawImage(dispelBmp, 0, 0, tileBounds, GraphicsUnit.Pixel);
                            tile.HasRleFormat = true;
                            tile.Image = tileBmp;
                            tile.Encode();
                            isEmpty &= tile.IsEmpty;
                            floor.Tiles.Add(tile);
                        }
                    if (isEmpty) continue;
                    var duplicatePos = Game.Map.View2MapTransform(pos.X, pos.Y);
                    {
                        var cell = Game.Map.Cells[(int)duplicatePos.Y, (int)duplicatePos.X];
                        if (cell.Floor != null)
                        {
                            if (cell.EventIdx > 0 && cell.EventIdx <= 61 && !visAdded)
                            {
                                visAdded = true;
                                var vis = new TDiabloWall();
                                vis.Type = (int)TWallType.Special1;
                                //vis.Hidden = true;
                                vis.Style = 0;
                                vis.Tiles = floor.Tiles;
                                //wall.Style = cell.EventIdx >> 6;
                                //wall.Seq = cell.EventIdx & 63;
                                vis.Width = 5 * TDiabloTile.Width;
                                vis.Height = -10 * TDiabloTile.Height;
                                diabloCell.Wall.Tiles.Add(vis);
                                DiabloMap.Walls.Add(vis);
                                continue;
                            }
                            var floorRef = floorRefs[cell.Floor.Index];
                            for (var i = 0; i < floorRef.Count; i++)
                            {
                                var prevCell = floorRef[i];
                                //if (prevCell == cell) continue;
                                var cellPos = Game.Map.Map2ViewTransform(prevCell.X, prevCell.Y);
                                cellPos = DiabloMap.View2WorldTransform(cellPos.X, cellPos.Y);
                                cellPos.X = (int)((cellPos.X + 2.5f) / 5);
                                cellPos.Y = (int)((cellPos.Y + 2.5f) / 5);
                                if (cellPos.Y < 0 || cellPos.Y >= size || cellPos.X < 0 || cellPos.X >= size) continue;
                                var prevDiabloCell = DiabloMap.Cells[(int)cellPos.Y * 5, (int)cellPos.X * 5];
                                if (prevDiabloCell != null && prevDiabloCell.Wall != null && prevDiabloCell != diabloCell)
                                {
                                    if (prevDiabloCell.Wall.Tiles.Count == 0) continue;
                                    var prevFloor = (TDiabloWall)prevDiabloCell.Wall.Tiles[0];
                                    if (prevFloor.Type == (int)TWallType.Special1) continue;
                                    if (floor.IsEqual(prevFloor))
                                    {
                                        diabloCell.Wall.Tiles.Add(floor);
                                        AddCollisionFloor(diabloCell);
                                        diabloCell.Wall.Tiles[0] = prevFloor;
                                        if (prevDiabloCell.Floor == commonFloor)
                                            AddCollisionFloor(prevDiabloCell);
                                        duplicatesCount++;
                                        floorRef.RemoveAt(i);
                                        break;
                                    }
                                }
                            }
                            //if (diabloCell.Floor != null)
                            //    continue;
                        }
                    }
                    if (diabloCell.Wall.Tiles.Count == 0)
                    {
                        diabloCell.Wall.Tiles.Add(floor);
                        DiabloMap.Walls.Add(floor);
                    }
                    floor.Type = (int)TWallType.LowerWall + (floor.Index >> 12);
                    floor.Style = floor.Index >> 6 & 63;
                    floor.Seq = floor.Index & 63;
                    floor.Direction = floor.Type % 10;
                }
            dispelBmp.Dispose();
        }

        void AddCollisionFloor(TCell diabloCell)
        {
            var floor = (TDiabloWall)diabloCell.Wall.Tiles[0];
            var collisionFloor = new TDiabloWall();
            collisionFloor.Width = 5 * TDiabloTile.Width;
            collisionFloor.Height = -8 * TDiabloTile.Height;
            collisionFloor.TilesFlags = floor.TilesFlags;
            floor.TilesFlags = new byte[floor.TilesFlags.Length];
            diabloCell.Floor = collisionFloor;
            collisionFloor.Style = DiabloMap.Floors.Count >> 6 & 63;
            collisionFloor.Seq = DiabloMap.Floors.Count & 63;
            DiabloMap.Floors.Add(collisionFloor);
            DiabloMap.Walls.Add(collisionFloor);
        }

        public void ExportWalls()
        {
            var size = DiabloMap.WorldWidth;
            var floorsCount = DiabloMap.Walls.Count;
            var walls = new List<TWall>(Game.Map.Walls);
            walls.Sort();
            var wallLayerIdx = new int[size * size];
            for (int i = 0; i < walls.Count; i++)
            {
                var wall = walls[i];
                var wallPosX = wall.X + 1 * TDiabloTile.Width / 2;
                var wallPosY = wall.Y + wall.Bounds.Height - 8 * TDiabloTile.Height / 2;
                var cellPos = DiabloMap.View2WorldTransform(wallPosX, wallPosY);
                var gridPos = new Vector2((int)cellPos.X / 5 * 5, (int)cellPos.Y / 5 * 5);
                var diabloWallPos = DiabloMap.World2ViewTransform(gridPos.X, gridPos.Y);
                var offsetX = wallPosX - (int)diabloWallPos.X;
                var offsetY = wallPosY - (int)diabloWallPos.Y;
                if (offsetY > 0)
                {
                    gridPos.X += 5;
                    gridPos.Y += 5;
                    offsetY -= 5 * TDiabloTile.Height;
                }
                if (offsetX < 0)
                {
                    gridPos.X -= 5;
                    gridPos.Y += 5;
                    offsetX += 5 * TDiabloTile.Width;
                }
                var diabloCell = DiabloMap.Cells[(int)gridPos.Y, (int)gridPos.X];
                var diabloCellIdx = diabloCell.Y / 5 * size + diabloCell.X / 5;
                var layerIdx = wallLayerIdx[diabloCellIdx];
                //if (diabloCell.Wall == null)
                //    diabloCell.Wall = new TDiabloWall();
                //else 
                if (layerIdx == 0)
                    layerIdx++;
                var diabloWall = layerIdx < diabloCell.Wall.Tiles.Count ? (TDiabloWall)diabloCell.Wall.Tiles[layerIdx] : null;
                if (layerIdx > DiabloMap.WallsLayersCount - 1)
                    DiabloMap.WallsLayersCount++;
                wallLayerIdx[diabloCellIdx] = (layerIdx + 1) & 3;
                if (diabloWall == null)
                {
                    diabloWall = new TDiabloWall();
                    DiabloMap.Walls.Add(diabloWall);
                    var wallIdx = DiabloMap.Walls.Count - floorsCount;
                    diabloWall.Type = 1 + (wallIdx >> 12);
                    diabloWall.Style = wallIdx >> 6 & 63;
                    diabloWall.Seq = wallIdx & 63;
                    diabloWall.Direction = diabloWall.Type % 10;
                    diabloCell.Wall.Tiles.Add(diabloWall);
                }
                for (int n = 0; n < wall.Tiles.Count; n++)
                {
                    var tile = wall.Tiles[n];
                    var y = n * TDispelTile.Height;
                    for (int m = 0; m < 2; m++)
                    {
                        var tileBmp = new Bitmap(TDiabloTile.Width, 2 * TDiabloTile.Height);
                        var tileGc = Graphics.FromImage(tileBmp);
                        var tileBounds = new Rectangle(m * tileBmp.Width, 0, tileBmp.Width, tileBmp.Height);
                        tileGc.DrawImage(tile.Image, 0, 0, tileBounds, GraphicsUnit.Pixel);
                        var diabloTile = new TDiabloTile();
                        diabloTile.X = offsetX + tileBounds.X;
                        diabloTile.Y = offsetY + y - wall.Bounds.Height;
                        diabloTile.HasRleFormat = true;
                        diabloTile.Image = tileBmp;
                        diabloWall.Tiles.Add(diabloTile);
                    }
                }
                var dispelWallBox = new Rectangle(offsetX, offsetY - wall.Bounds.Height, wall.Bounds.Width, wall.Bounds.Height);
                diabloWall.Bounds = Rectangle.Union(diabloWall.Bounds, dispelWallBox);
                diabloWall.Width = diabloWall.Bounds.Width;
                diabloWall.Height = -diabloWall.Bounds.Height - 2 * TDiabloTile.Height;
            }
        }

        public Bitmap GetDuplicatesBmp()
        {
            var pen = new Pen(Color.Black);
            pen.Width = 4;
            var font = new Font("Arial", 10);
            var pal = TPalette.CreateRGB332();
            var mapViewSize = Game.Map.Map2ViewTransform(Game.Map.Width, Game.Map.Height);
            var debugBmp = new Bitmap((int)mapViewSize.X, (int)mapViewSize.Y);
            var debugGc = Graphics.FromImage(debugBmp);
            var floorRefs = new List<TCell>[Game.Map.Floors.Count];
            for (int y = 0; y < Game.Map.Height; y++)
                for (int x = 0; x < Game.Map.Width; x++)
                {
                    var cell = Game.Map.Cells[y, x];
                    if (cell == null) continue;
                    var bounds = cell.Bounds;
                    if (cell.Floor != null)
                    {
                        var refCells = floorRefs[cell.Floor.Index];
                        if (refCells == null)
                        {
                            refCells = new List<TCell>();
                            floorRefs[cell.Floor.Index] = refCells;
                        }
                        refCells.Add(cell);
                        debugGc.DrawImage(cell.Floor.Image, bounds.X, bounds.Y);
                    }
                }
            for (int i = 0; i < floorRefs.Count(); i++)
            {
                var floorRef = floorRefs[i];
                pen.Color = Color.FromArgb(pal[i & 0xFF]);
                if (floorRef != null && floorRef.Count > 1)
                    foreach (var cell in floorRef)
                    {
                        var fBounds = cell.Bounds; fBounds.Inflate(-3, -3);
                        var rhomb = TBoard.GetRhomb(fBounds);
                        debugGc.DrawPolygon(pen, rhomb);
                        debugGc.DrawString(i.ToString(), font, Brushes.White, rhomb[1].X, rhomb[0].Y);
                    }
            }
            return debugBmp;

        }

        public Bitmap GetGridsBmp()
        {
            var pen = new Pen(Color.Black);
            var mapViewSize = Game.Map.Map2ViewTransform(Game.Map.Width, Game.Map.Height);
            var debugBmp = new Bitmap((int)mapViewSize.X, (int)mapViewSize.Y);
            var debugGc = Graphics.FromImage(debugBmp);
            for (int y = 0; y < Game.Map.Height; y++)
                for (int x = 0; x < Game.Map.Width; x++)
                {
                    var cell = Game.Map.Cells[y, x];
                    if (cell == null) continue;
                    var bounds = cell.Bounds;
                    if (cell.Floor != null)
                    {
                        pen.Color = Color.Yellow;
                        debugGc.DrawImage(cell.Floor.Image, bounds.X, bounds.Y);
                        debugGc.DrawPolygon(pen, TBoard.GetRhomb(bounds));
                    }
                }
            var size = (Game.Map.Width + 2 * Game.Map.Height) / 5;
            var diabloMap = new TDiabloMap();
            diabloMap.GridOffset = new Vector2(-2f * size + 2, 2f * size + 6);
            var dispelBmpRect = new Rectangle(0, 0, debugBmp.Width, debugBmp.Height);
            dispelBmpRect.Inflate(-TDispelTile.Width / 2, -TDispelTile.Height);
            diabloMap.Cells = new TCell[5 * size, 5 * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    var pos = diabloMap.World2ViewTransform(5 * x, 5 * y);
                    if (!dispelBmpRect.Contains((int)pos.X, (int)pos.Y)) continue;
                    var floor = new TDiabloWall();
                    floor.Width = 5 * TDiabloTile.Width;
                    floor.Height = 5 * TDiabloTile.Height;
                    floor.Bounds = new Rectangle((int)pos.X - 80, (int)pos.Y - 8, floor.Width, floor.Height);
                    pen.Color = Color.Magenta;
                    debugGc.DrawPolygon(pen, TBoard.GetRhomb(floor.Bounds));
                }
            return debugBmp;
        }

    }
}
