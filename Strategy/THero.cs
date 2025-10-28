using Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class THero: TPiece
    {
        public TPlayer Player;
        public string Name { get; set; }
        TCell FCell;
        public TCell Cell
        {
            get { return FCell; }
            set
            {
                if (FCell != null)
                    FCell.Piece = null;
                FCell = value;
                if (FCell != null)
                {
                    if (FCell.Piece is TResource)
                    {
                        var res = (TResource)FCell.Piece;
                        Player.Resources[(int)res.Type] += res.Value;
                        Player.Game.OnResourceChanged(this, null);
                    }
                    FCell.Piece = this;
                    int w = Player.Game.Map.Width;
                    int h = Player.Game.Map.Height;
                    for (int y = FCell.Y - SightRange; y < FCell.Y + SightRange; y++)
                        for (int x = FCell.X - SightRange; x < FCell.X + SightRange; x++)
                            if (x >= 0 && x < w && y >= 0 && y < h)
                            {
                                var selCell = Player.Game.Map.Cells[y, x];
                                if (!selCell.IsVisible)
                                {
                                    selCell.IsVisible = true;
                                    Player.VisibleCells.Add(selCell);
                                }
                            }
                }
            }
        }

        TCell stopCell;
        public TCell StopCell
        {
            get { return stopCell; }
            set
            {
                stopCell = value;
                FindPath();
            }
        }

        public List<TCell> Path;
        void FindPath()
        {
            var pathTree = new List<TCell>();
            var cell = Cell;
            if (StopCell != cell)
                pathTree.Add(cell);
            for (int j = 0; j < pathTree.Count; j++)
            {
                cell = pathTree[j];
                if (cell == StopCell) break;
                for (int i = 0; i < cell.Neighbors.Count; i++)
                {
                    var neigh = cell.Neighbors[i];
                    if (!neigh.IsVisible) continue;
                    if (neigh.Piece != null && neigh != StopCell) continue;
                    if (neigh.Parent == null)
                    {
                        neigh.Parent = cell;
                        pathTree.Add(neigh);
                    }
                }
            }
            Path = new List<TCell>();
            cell = StopCell;
            while (cell != null)
            {
                Path.Add(cell);
                cell = cell.Parent;
            }
            Path.Reverse();
            for (int i = 0; i < pathTree.Count; i++)
                pathTree[i].Parent = null;
        }

        public int MovesCount;
        public int MovesCountMax = 10;
        public int SightRange = 5;
    }
}
