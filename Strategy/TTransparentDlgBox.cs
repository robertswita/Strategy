using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Strategy
{
    public partial class TTransparentDlgBox : UserControl
    {
        const int GWL_EXSTYLE = -20;
        const int WS_EX_LAYERED = 0x80000;
        const int WS_EX_TRANSPARENT = 0x20;

        public TTransparentDlgBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, true);
            //SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.Opaque , true);
            //ControlStyles.AllPaintingInWmPaint |
            //ControlStyles.ResizeRedraw |
            //ControlStyles.UserPaint, true);
            //TransparencyKey
            BackColor = Color.Transparent;
            //Multiline = true;
            Text = "Hello";
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TRANSPARENT;
                return cp;
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawRectangle(Pens.Red, new Rectangle(10, 10, Width - 20, Height - 20));
        }
    }
}
