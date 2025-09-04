using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;

namespace Strategy
{
    public class TNpc : TMonster
    {
        public string Name;
        public string Description;
        public int ScriptId;
        public int OnShowEvent;
        public List<TCell> Path = new List<TCell>();
        public List<TCell> DefaultPath;
        public int ActPathCellIdx;
        public int DialogId;
        public Vector2 ActPos;
        public bool ShowPath;
        public TGame Game;
        public bool Enabled;

        TCell cell;
        public TCell Cell
        {
            get { return cell; }
            set
            {
                if (cell != null)
                    cell.Npc = null;
                cell = value;
                if (cell != null)
                    cell.Npc = this;
            }
        }
        public void CalcPath(List<Vector2> pathPts)
        {
            Path = new List<TCell>();
            var worldRect = new Rectangle(0, 0, Game.Map.Width, Game.Map.Height);
            if (worldRect.Contains((int)pathPts[0].X, (int)pathPts[0].Y))
                Cell = Game.Cells[(int)pathPts[0].Y, (int)pathPts[0].X];
            var startCell = Cell;
            for (int i = 1; i < pathPts.Count; i++)
            {
                if (!worldRect.Contains((int)pathPts[i].X, (int)pathPts[i].Y)) continue;
                var stopCell = Game.Cells[(int)pathPts[i].Y, (int)pathPts[i].X];
                Path.AddRange(GetPathTo(stopCell));
                Cell = stopCell;
            }
            //Path.AddRange(GetPathTo(startCell));
            Cell = startCell;
            ActPos = Path[0].Position;
            X = (int)ActPos.X;
            Y = (int)ActPos.Y;
        }

        public List<TCell> GetPathTo(TCell stopCell)
        {
            var pathTree = new List<TCell>();
            var cell = Cell;
            if (stopCell != cell)
            {
                pathTree.Add(cell);
                cell.Parent = cell;
            }
            for (int j = 0; j < pathTree.Count; j++)
            {
                cell = pathTree[j];
                if (cell == stopCell) break;
                var nHood = cell.Neighbors;
                var neighbors = new List<TCell>();
                //collision correction + order change (diagonal last)
                for (int i = 0; i < nHood.Count / 2; i++)
                {
                    var neigh1 = nHood[(2 * i + 7) % nHood.Count];
                    var neigh2 = nHood[2 * i + 1];
                    if (neigh1 != null && neigh2 != null && neigh1.Collision && neigh2.Collision)
                        nHood[2 * i] = null;
                    neighbors.Insert(0, neigh2);
                    neighbors.Add(nHood[2 * i]);
                }

                //if (neighbors[7].Collision && neighbors[1].Collision) neighbors[0] = null;
                //if (neighbors[1].Collision && neighbors[3].Collision) neighbors[2] = null;
                //if (neighbors[3].Collision && neighbors[5].Collision) neighbors[4] = null;
                //if (neighbors[5].Collision && neighbors[7].Collision) neighbors[6] = null;
                for (int i = 0; i < neighbors.Count; i++)
                {
                    var neigh = neighbors[i];
                    //if (!neigh.IsVisible) continue;
                    if (neigh == null || neigh.Collision && neigh != stopCell) continue;
                    if (neigh.Parent == null)
                    {
                        neigh.Parent = cell;
                        pathTree.Add(neigh);
                    }
                }
            }
            var path = new List<TCell>();
            cell = stopCell;
            Cell.Parent = null;
            while (cell != null)
            {
                path.Add(cell);
                cell = cell.Parent;
            }
            path.Reverse();
            for (int i = 0; i < pathTree.Count; i++)
                pathTree[i].Parent = null;
            return path;
        }

        int WaitFrameNo;
        int WaitFramesCount = 5;

        public override void NextFrame()
        {
            base.NextFrame();
            if (WaitFrameNo > 0)
            {
                WaitFrameNo--;
                if (WaitFrameNo == 0)
                    Sequence = (Sequence + 1) % Animation.Sequences.Count;
            }
            else if (Path.Count > 1) {
                Sequence = 1;
                var nextCell = Path[ActPathCellIdx];
                var moveVector = nextCell.Position - Cell.Position;
                ActPos += moveVector * 0.04f;
                X = (int)(ActPos.X);// + TCell.Scale.X);
                Y = (int)(ActPos.Y);// + TCell.Scale.Y);
                if ((ActPos - nextCell.Position).Length() < 1)
                {
                    Cell = nextCell;
                    ActPathCellIdx = (ActPathCellIdx + 1) % Path.Count;
                    nextCell = Path[ActPathCellIdx];
                    if (nextCell == Cell)
                    {
                        nextCell = Path[(ActPathCellIdx + 1) % Path.Count];
                        Sequence = 0;
                        WaitFrameNo = TGame.FPS * WaitFramesCount;
                    }
                    var viewAngle = Cell.Neighbors.IndexOf(nextCell);
                    if (viewAngle >= 0)
                        ViewAngle = viewAngle;
                }
            }
            if (this == Game.ActiveNpc && ActPathCellIdx == Path.Count - 1)
            {
                ActPathCellIdx = 0;
                Sequence = 0;
                Path = new List<TCell>();
                //Path = DefaultPath;
                Game.ActiveNpc = null;
                Game.ActiveScript.IsWaiting = false;
            }

        }
    }
}
