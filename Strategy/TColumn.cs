using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strategy
{
    public class TColumn : IComparable<TColumn>
    {
        public int X, Y;
        public int Order;
        public Rectangle Bounds;
        public List<TCell> Cells = new List<TCell>();
        int IComparable<TColumn>.CompareTo(TColumn other)
        {
            var y1_ = Y + Cells.Count * TGame.TileHeight;
            var y2_ = other.Y + other.Cells.Count * TGame.TileHeight;
            return y1_ == y2_ ? Order.CompareTo(other.Order) : y1_.CompareTo(y2_);
            //return y1_ == y2_ ? other.Order.CompareTo(Order) : y2_.CompareTo(y1_);
        }
    }
}
