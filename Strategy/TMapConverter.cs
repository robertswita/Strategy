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
        public int DuplicatesCount;
        public bool IsFloorIso;
        public bool[] HasEvent = new bool[64];
        public List<TDiabloWall> Vises = new List<TDiabloWall>();

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
                    var size = (Game.Map.Width + 2 * Game.Map.Height) / 5;
                    DiabloMap.WorldWidth = size;
                    DiabloMap.WorldHeight = size;
                    DiabloMap.GridOffset = new Vector2(-2 * size, 2 * size);
                    DiabloMap.Height = 2 * Game.Map.Height / 5;
                    DiabloMap.Width = 2 * Game.Map.Width / 5;
                    DiabloMap.Cells = new TCell[DiabloMap.Height, DiabloMap.Width];
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
                                foreach (var wall in cell.Walls)
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

        Bitmap GetDispelBitmap()
        {
            var mapViewSize = Game.Map.Map2ViewTransform(Game.Map.Width, Game.Map.Height);
            var dispelBmp = new Bitmap((int)mapViewSize.X, (int)mapViewSize.Y);
            var gc = Graphics.FromImage(dispelBmp);
            foreach (var cell in Game.Map.Cells)
            {
                if (cell == null || cell.Floor == null) continue;
                var bounds = cell.Bounds;
                gc.DrawImage(cell.Floor.Image, bounds.X, bounds.Y);
                //gc.DrawPolygon(Pens.Yellow, TBoard.GetRhomb(bounds));
            }
            return dispelBmp;
        }

        List<TCell>[] GetFloorRefs()
        {
            var floorRefs = new List<TCell>[Game.Map.Floors.Count];
            for (int i = 0; i < floorRefs.Length; i++)
                floorRefs[i] = new List<TCell>();
            foreach (var cell in Game.Map.Cells)
                if (cell != null && cell.Floor != null)
                    floorRefs[cell.Floor.Index].Add(cell);
            return floorRefs;
        }

public void ExportFloors()
{
    var dispelBmp = GetDispelBitmap();
    var floorRefs = GetFloorRefs();
    IsFloorIso = DiabloMap.Height < 100;
    AddFloors(dispelBmp);
    var commonFloor = new TDiabloWall();
    DiabloMap.Floors.Add(commonFloor);
    DiabloMap.Walls.Add(commonFloor);
    for (int y = 0; y < DiabloMap.Height; y++)
        for (int x = 0; x < DiabloMap.Width; x++)
        {
            var viewPos = DiabloMap.Map2ViewTransform(x, y);
            var diabloCell = DiabloMap.Cells[y, x];
            var floor = (TDiabloWall)diabloCell.Floor;
            diabloCell.Floor = commonFloor;
            if (floor == null) continue;
            var dispelCellPos = Game.Map.View2MapTransform(viewPos.X, viewPos.Y);
            var dispelCell = Game.Map.Cells[(int)dispelCellPos.Y, (int)dispelCellPos.X];
            AddVis(diabloCell, dispelCell.EventIdx);
            SetCollisionFlags(diabloCell, floor);
            var duplicate = FindFloorDuplicate(diabloCell, floor, floorRefs[dispelCell.Floor.Index]);
            if (duplicate == null)
                SetFloor(diabloCell, floor);
        }
}

        void AddVis(TCell diabloCell, int eventIdx)
        {
            if (eventIdx > 0 && eventIdx <= 61 && !HasEvent[eventIdx])
            {
                var vis = new TDiabloWall();
                vis.Type = (int)TWallType.Special1;
                //vis.Hidden = true;
                vis.Style = Vises.Count;
                Vises.Add(vis);
                //vis.Tiles = floor.Tiles;
                diabloCell.Walls.Add(vis);
                DiabloMap.Walls.Add(vis);
                HasEvent[eventIdx] = true;
            }
        }

        void AddFloors(Bitmap dispelBmp)
        {
            var dispelBmpRect = new Rectangle(0, 0, dispelBmp.Width, dispelBmp.Height);
            dispelBmpRect.Inflate(-TDispelTile.Width / 2, -TDispelTile.Height);
            //dispelBmpRect.Offset(TDispelTile.Width / 2, 0);
            var tileOffset = new Point(TDiabloTile.Width, TDiabloTile.Height / 2);
            for (int y = 0; y < DiabloMap.Height; y++)
                for (int x = 0; x < DiabloMap.Width; x++)
                {
                    var diabloCell = DiabloMap.Cells[y, x];
                    if (diabloCell == null)
                    {
                        diabloCell = new TCell();
                        diabloCell.X = x;
                        diabloCell.Y = y;
                        DiabloMap.Cells[y, x] = diabloCell;
                    }
                    var viewPos = DiabloMap.Map2ViewTransform(x, y);
                    if (IsFloorIso)
                    {
                        if (!dispelBmpRect.Contains((int)viewPos.X, (int)viewPos.Y)) continue;
                        var floor = new TDiabloWall();
                        floor.Bounds = new Rectangle((int)viewPos.X, (int)viewPos.Y + floor.Height, floor.Width, -floor.Height);
                        var worldPos = DiabloMap.Map2WorldTransform(x, y);
                        var isEmpty = true;
                        for (int u = 0; u < 5; u++)
                            for (int v = 0; v < 5; v++)
                            {
                                var tile = new TDiabloTile();
                                var tilePos = DiabloMap.World2ViewTransform(worldPos.X + 4 - v, worldPos.Y + 4 - u);
                                tilePos.X += 2 * TDiabloTile.Width;
                                tile.X = (int)(tilePos.X - viewPos.X);
                                tile.Y = (int)(tilePos.Y - viewPos.Y);
                                var tileBounds = new Rectangle(0, 0, TDiabloTile.Width, TDiabloTile.Height);
                                tileBounds.X = (int)tilePos.X - tileOffset.X;
                                tileBounds.Y = (int)tilePos.Y - tileOffset.Y;
                                var tileBmp = new Bitmap(tileBounds.Width, tileBounds.Height);
                                var tileGc = Graphics.FromImage(tileBmp);
                                tileGc.DrawImage(dispelBmp, 0, 0, tileBounds, GraphicsUnit.Pixel);
                                tile.Image = tileBmp;
                                tile.Encode();
                                isEmpty &= tile.IsEmpty;
                                floor.Tiles.Add(tile);
                            }
                        if (isEmpty) continue;
                        diabloCell.Floor = floor;
                    }
                    else
                    {
                        if ((x & 1) != 0 || (y & 1) != 0) continue;
                        //if (!dispelBmpRect.Contains((int)viewPos.X, (int)viewPos.Y)) continue;
                        for (int segY = 0; segY < 2; segY++)
                            for (int segX = 0; segX < 2; segX++)
                            {
                                var floor = new TDiabloWall();
                                floor.Width = (3 - segX) * TDiabloTile.Width;
                                floor.Height = -(2 + segY) * TDiabloTile.Width;
                                var bounds = new Rectangle((int)viewPos.X, (int)viewPos.Y - 64, floor.Width, -floor.Height);         
                                bounds.Offset(segX * 96, segY * 64);
                                var origin = new Point(bounds.Left, bounds.Bottom - 80);
                                bounds.Offset(-tileOffset.X, -tileOffset.Y);
                                floor.Bounds = bounds;
                                var diabloCellPos = DiabloMap.View2MapTransform(origin.X, origin.Y);
                                if (diabloCellPos.X < 0 || diabloCellPos.X >= DiabloMap.Width || diabloCellPos.Y < 0 || diabloCellPos.Y >= DiabloMap.Height) continue;
                                diabloCell = new TCell();
                                diabloCell.X = (int)diabloCellPos.X;
                                diabloCell.Y = (int)diabloCellPos.Y;
                                DiabloMap.Cells[diabloCell.Y, diabloCell.X] = diabloCell;
                                if (!dispelBmpRect.Contains(origin.X, origin.Y)) continue;
                                var floorViewPos = DiabloMap.Map2ViewTransform(diabloCell.X, diabloCell.Y);
                                var offset = new Point(origin.X - (int)floorViewPos.X, origin.Y - (int)floorViewPos.Y);
                                var isEmpty = true;
                                for (int u = 0; u < 2 + segY; u++)
                                    for (int v = 0; v < 3 - segX; v++)
                                    {
                                        var tile = new TDiabloTile();
                                        var tileBmp = new Bitmap(TDiabloTile.Width, 2 * TDiabloTile.Height);
                                        tile.X = v * tileBmp.Width;
                                        tile.Y = u * tileBmp.Height;
                                        var tileBounds = new Rectangle(0, 0, tileBmp.Width, tileBmp.Height);
                                        tileBounds.X = floor.Bounds.X + tile.X;
                                        tileBounds.Y = floor.Bounds.Y + tile.Y;
                                        tile.X += offset.X;
                                        tile.Y += offset.Y + floor.Height;
                                        var tileGc = Graphics.FromImage(tileBmp);
                                        tileGc.DrawImage(dispelBmp, 0, 0, tileBounds, GraphicsUnit.Pixel);
                                        tile.HasRleFormat = true;
                                        tile.Image = tileBmp;
                                        tile.Encode();
                                        isEmpty &= tile.IsEmpty;
                                        floor.Tiles.Add(tile);
                                    }
                                if (isEmpty) continue;
                                floor.Width += offset.X;
                                floor.Height -= offset.Y + 32;
                                diabloCell.Floor = floor;
                            }
                    }
                }
        }

void SetCollisionFlags(TCell diabloCell, TDiabloWall floor)
{
    var isEmpty = true;
    var viewPos = DiabloMap.Map2ViewTransform(diabloCell.X, diabloCell.Y);
    var worldPos = DiabloMap.Map2WorldTransform(diabloCell.X, diabloCell.Y);
    for (int u = 0; u < 5; u++)
        for (int v = 0; v < 5; v++)
        {
            //var tileIdx = 5 * u + v;
            var tilePos = DiabloMap.World2ViewTransform(worldPos.X + 4 - v, worldPos.Y + 4 - u);
            tilePos.X += 2 * TDiabloTile.Width;
            var cellPos = Game.Map.View2WorldTransform(tilePos.X, tilePos.Y);
            cellPos = Game.Map.World2MapTransform((int)(cellPos.X), (int)(cellPos.Y));
            //var cellPos = Game.Map.View2MapTransform(tilePos.X, tilePos.Y);
            if (cellPos.X >= 0 && cellPos.X < Game.Map.Width && cellPos.Y >= 0 && cellPos.Y < Game.Map.Height)
            {
                var cell = Game.Map.Cells[(int)cellPos.Y, (int)cellPos.X];
                if (cell.Collision)
                {
                    var flagIdx = 5 * u + 4 - v;
                    var cellBounds = cell.Bounds;
                    var tileBounds = new Rectangle((int)tilePos.X, (int)tilePos.Y, 32, 32);
                    tileBounds.Offset(-32, -8);
                    //cellBounds.Inflate(-1, -1);
                    if (Rectangle.Union(cellBounds, tileBounds).Equals(cellBounds))
                    {
                        floor.TilesFlags[flagIdx] |= 1;
                        isEmpty = false;
                    }
                }
            }
        }
    if (isEmpty)
        floor.TilesFlags = null;
}

        TDiabloWall GetFloor(TCell diabloCell)
        {
            if (!IsFloorIso && diabloCell.Walls.Count == 0) return null;
            return IsFloorIso ? (TDiabloWall)diabloCell.Floor : (TDiabloWall)diabloCell.Walls[0]; 
        }
        void SetFloor(TCell diabloCell, TDiabloWall floor)
        {
            if (IsFloorIso)
            {
                floor.Style = DiabloMap.Floors.Count >> 6 & 63;
                floor.Seq = DiabloMap.Floors.Count & 63;
                floor.Direction = 3;
                diabloCell.Floor = floor;
                DiabloMap.Floors.Add(floor);
            }
            else
            {
                floor.Type = (int)TWallType.LowerWall + (DiabloMap.Walls.Count >> 12);
                floor.Style = DiabloMap.Walls.Count >> 6 & 63;
                floor.Seq = DiabloMap.Walls.Count & 63;
                floor.Direction = floor.Type % 10;
                diabloCell.Walls.Add(floor);
            }
            DiabloMap.Walls.Add(floor);
        }

TDiabloWall FindFloorDuplicate(TCell diabloCell, TDiabloWall floor, List<TCell> floorRef)
{
    for (var i = 0; i < floorRef.Count; i++)
    {
        var prevCell = floorRef[i];
        var cellPos = Game.Map.Map2ViewTransform(prevCell.X, prevCell.Y);
        cellPos = DiabloMap.View2MapTransform(cellPos.X, cellPos.Y);
        cellPos.X = (int)(cellPos.X + 0.8f);
        cellPos.Y = (int)(cellPos.Y + 0.8f);
        if (cellPos.Y < 0 || cellPos.Y >= DiabloMap.Height || cellPos.X < 0 || cellPos.X >= DiabloMap.Width) continue;
        var prevDiabloCell = DiabloMap.Cells[(int)cellPos.Y, (int)cellPos.X];
        var prevFloor = GetFloor(prevDiabloCell);
        if (prevFloor == null || prevFloor.Index == 0) continue;
        if (prevFloor.Type == (int)TWallType.Special1) continue;
        if (prevFloor.IsEqual(floor))
        {
            AddCollision(diabloCell, floor);
            if (prevDiabloCell.Walls.Count == 0 || prevDiabloCell.Floor == DiabloMap.Floors[0])
                AddCollision(prevDiabloCell, prevFloor);
            if (IsFloorIso)
                diabloCell.Floor = prevFloor;
            else
                diabloCell.Walls.Add(prevFloor);
            DuplicatesCount++;
            floorRef.RemoveAt(i);
            return prevFloor;
        }
    }
    return null;
}

        void AddCollision(TCell diabloCell, TDiabloWall floor)
        {
            if (floor.TilesFlags == null) return;
            var collisionWall = new TDiabloWall();
            collisionWall.TilesFlags = floor.TilesFlags;
            floor.TilesFlags = new byte[floor.TilesFlags.Length];
            IsFloorIso = !IsFloorIso;
            SetFloor(diabloCell, collisionWall);
            IsFloorIso = !IsFloorIso;
        }

public void ExportWalls()
{
    var wallsCount = 0;
    var walls = new List<TWall>(Game.Map.Walls);
    walls.Sort();
    var wallLayerIndexes = new int[DiabloMap.Height, DiabloMap.Width];
    for (int i = 0; i < walls.Count; i++)
    {
        var wall = walls[i];
        var wallPosX = wall.X + TDiabloTile.Width;
        var wallPosY = wall.Y + wall.Bounds.Height - 9 * TDiabloTile.Height / 2;
        var cellPos = DiabloMap.View2MapTransform(wallPosX, wallPosY);
        var diabloWallPos = DiabloMap.Map2ViewTransform((int)cellPos.X, (int)cellPos.Y);
        var offsetX = wallPosX - (int)diabloWallPos.X;
        var offsetY = wallPosY - (int)diabloWallPos.Y;
        if (offsetY > 0)
        {
            cellPos.Y++;
            offsetY -= 5 * TDiabloTile.Height;
        }
        if (offsetX < 0)
        {
            cellPos.X--;
            offsetX += 5 * TDiabloTile.Width;
        }
        var diabloCell = DiabloMap.Cells[(int)cellPos.Y, (int)cellPos.X];
        var wallLayerIdx = wallLayerIndexes[diabloCell.Y, diabloCell.X];
        if (!IsFloorIso && wallLayerIdx == 0)
            wallLayerIdx += 4;
        var layerIdx = wallLayerIdx / 4;
        var diabloWall = layerIdx < diabloCell.Walls.Count ? (TDiabloWall)diabloCell.Walls[layerIdx] : null;
        if (layerIdx > DiabloMap.WallsLayersCount - 1)
            DiabloMap.WallsLayersCount++;
        wallLayerIndexes[diabloCell.Y, diabloCell.X] = (wallLayerIdx + 1) & 0xF;
        byte[] flags = null;
        if (diabloWall != null && diabloWall.Type >= (int)TWallType.LowerWall)
        {
            flags = diabloWall.TilesFlags;
            diabloCell.Walls.Clear();
            diabloWall = null;
        }
        if (diabloWall == null)
        {
            diabloWall = new TDiabloWall();
            DiabloMap.Walls.Add(diabloWall);
            diabloWall.Type = 1 + (wallsCount >> 12);
            diabloWall.Style = wallsCount >> 6 & 63;
            diabloWall.Seq = wallsCount & 63;
            diabloWall.Direction = diabloWall.Type % 10;
            if (flags != null)
                diabloWall.TilesFlags = flags;
            diabloCell.Walls.Add(diabloWall);
            wallsCount++;
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
            var debugBmp = GetDispelBitmap();
            var debugGc = Graphics.FromImage(debugBmp);
            var floorRefs = GetFloorRefs();
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
            diabloMap.GridOffset = new Vector2(-2f * size + 2, 2f * size - 2);
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
