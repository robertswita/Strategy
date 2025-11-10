namespace Strategy
{
    partial class StrategyForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StrategyForm));
            this.PropPanel = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.MapNameLbl = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.ResourceView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.CitiesList = new System.Windows.Forms.ListBox();
            this.HerosList = new System.Windows.Forms.ListBox();
            this.MapView = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.floorsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.wallsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.spritesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.roofsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.eventsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.boundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collisionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pathsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showFloorDuplicatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpConversionGridsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PlayTimer = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.Board = new Strategy.TBoard();
            this.PropPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MapView)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PropPanel
            // 
            this.PropPanel.BackColor = System.Drawing.Color.Maroon;
            this.PropPanel.Controls.Add(this.button3);
            this.PropPanel.Controls.Add(this.MapNameLbl);
            this.PropPanel.Controls.Add(this.button2);
            this.PropPanel.Controls.Add(this.ResourceView);
            this.PropPanel.Controls.Add(this.button1);
            this.PropPanel.Controls.Add(this.CitiesList);
            this.PropPanel.Controls.Add(this.HerosList);
            this.PropPanel.Controls.Add(this.MapView);
            this.PropPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.PropPanel.Location = new System.Drawing.Point(906, 0);
            this.PropPanel.Name = "PropPanel";
            this.PropPanel.Size = new System.Drawing.Size(279, 800);
            this.PropPanel.TabIndex = 2;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(58, 756);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(165, 32);
            this.button3.TabIndex = 7;
            this.button3.Text = "Save map";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // MapNameLbl
            // 
            this.MapNameLbl.AutoSize = true;
            this.MapNameLbl.BackColor = System.Drawing.Color.White;
            this.MapNameLbl.Location = new System.Drawing.Point(25, 247);
            this.MapNameLbl.Name = "MapNameLbl";
            this.MapNameLbl.Size = new System.Drawing.Size(44, 16);
            this.MapNameLbl.TabIndex = 6;
            this.MapNameLbl.Text = "label1";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(58, 712);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(165, 32);
            this.button2.TabIndex = 5;
            this.button2.Text = "Load map";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ResourceView
            // 
            this.ResourceView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.ResourceView.HideSelection = false;
            this.ResourceView.Location = new System.Drawing.Point(18, 501);
            this.ResourceView.Name = "ResourceView";
            this.ResourceView.Size = new System.Drawing.Size(243, 205);
            this.ResourceView.TabIndex = 4;
            this.ResourceView.UseCompatibleStateImageBehavior = false;
            this.ResourceView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 123;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Width = 113;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(58, 463);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(165, 32);
            this.button1.TabIndex = 3;
            this.button1.Text = "End Turn";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // CitiesList
            // 
            this.CitiesList.FormattingEnabled = true;
            this.CitiesList.ItemHeight = 16;
            this.CitiesList.Location = new System.Drawing.Point(151, 277);
            this.CitiesList.Name = "CitiesList";
            this.CitiesList.Size = new System.Drawing.Size(111, 180);
            this.CitiesList.TabIndex = 2;
            // 
            // HerosList
            // 
            this.HerosList.FormattingEnabled = true;
            this.HerosList.ItemHeight = 16;
            this.HerosList.Location = new System.Drawing.Point(18, 277);
            this.HerosList.Name = "HerosList";
            this.HerosList.Size = new System.Drawing.Size(123, 180);
            this.HerosList.TabIndex = 1;
            this.HerosList.SelectedIndexChanged += new System.EventHandler(this.HerosList_SelectedIndexChanged);
            // 
            // MapView
            // 
            this.MapView.ContextMenuStrip = this.contextMenuStrip1;
            this.MapView.Location = new System.Drawing.Point(18, 22);
            this.MapView.Name = "MapView";
            this.MapView.Size = new System.Drawing.Size(244, 211);
            this.MapView.TabIndex = 0;
            this.MapView.TabStop = false;
            this.MapView.Paint += new System.Windows.Forms.PaintEventHandler(this.MapView_Paint);
            this.MapView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapView_MouseMove);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewToolStripMenuItem1,
            this.debugToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(124, 52);
            // 
            // viewToolStripMenuItem1
            // 
            this.viewToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.floorsToolStripMenuItem1,
            this.wallsToolStripMenuItem1,
            this.spritesToolStripMenuItem1,
            this.roofsToolStripMenuItem1,
            this.eventsToolStripMenuItem,
            this.boundsToolStripMenuItem,
            this.gridToolStripMenuItem,
            this.collisionsToolStripMenuItem,
            this.pathsToolStripMenuItem});
            this.viewToolStripMenuItem1.Name = "viewToolStripMenuItem1";
            this.viewToolStripMenuItem1.Size = new System.Drawing.Size(123, 24);
            this.viewToolStripMenuItem1.Text = "View";
            // 
            // floorsToolStripMenuItem1
            // 
            this.floorsToolStripMenuItem1.Checked = true;
            this.floorsToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.floorsToolStripMenuItem1.Name = "floorsToolStripMenuItem1";
            this.floorsToolStripMenuItem1.Size = new System.Drawing.Size(155, 26);
            this.floorsToolStripMenuItem1.Text = "Floors";
            this.floorsToolStripMenuItem1.Click += new System.EventHandler(this.floorsToolStripMenuItem_Click);
            // 
            // wallsToolStripMenuItem1
            // 
            this.wallsToolStripMenuItem1.Checked = true;
            this.wallsToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.wallsToolStripMenuItem1.Name = "wallsToolStripMenuItem1";
            this.wallsToolStripMenuItem1.Size = new System.Drawing.Size(155, 26);
            this.wallsToolStripMenuItem1.Text = "Walls";
            this.wallsToolStripMenuItem1.Click += new System.EventHandler(this.floorsToolStripMenuItem_Click);
            // 
            // spritesToolStripMenuItem1
            // 
            this.spritesToolStripMenuItem1.Checked = true;
            this.spritesToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.spritesToolStripMenuItem1.Name = "spritesToolStripMenuItem1";
            this.spritesToolStripMenuItem1.Size = new System.Drawing.Size(155, 26);
            this.spritesToolStripMenuItem1.Text = "Sprites";
            this.spritesToolStripMenuItem1.Click += new System.EventHandler(this.floorsToolStripMenuItem_Click);
            // 
            // roofsToolStripMenuItem1
            // 
            this.roofsToolStripMenuItem1.Checked = true;
            this.roofsToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.roofsToolStripMenuItem1.Name = "roofsToolStripMenuItem1";
            this.roofsToolStripMenuItem1.Size = new System.Drawing.Size(155, 26);
            this.roofsToolStripMenuItem1.Text = "Roofs";
            this.roofsToolStripMenuItem1.Click += new System.EventHandler(this.floorsToolStripMenuItem_Click);
            // 
            // eventsToolStripMenuItem
            // 
            this.eventsToolStripMenuItem.Checked = true;
            this.eventsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.eventsToolStripMenuItem.Name = "eventsToolStripMenuItem";
            this.eventsToolStripMenuItem.Size = new System.Drawing.Size(155, 26);
            this.eventsToolStripMenuItem.Text = "Events";
            this.eventsToolStripMenuItem.Click += new System.EventHandler(this.floorsToolStripMenuItem_Click);
            // 
            // boundsToolStripMenuItem
            // 
            this.boundsToolStripMenuItem.Name = "boundsToolStripMenuItem";
            this.boundsToolStripMenuItem.Size = new System.Drawing.Size(155, 26);
            this.boundsToolStripMenuItem.Text = "Bounds";
            this.boundsToolStripMenuItem.Click += new System.EventHandler(this.floorsToolStripMenuItem_Click);
            // 
            // gridToolStripMenuItem
            // 
            this.gridToolStripMenuItem.Name = "gridToolStripMenuItem";
            this.gridToolStripMenuItem.Size = new System.Drawing.Size(155, 26);
            this.gridToolStripMenuItem.Text = "Grid";
            this.gridToolStripMenuItem.Click += new System.EventHandler(this.floorsToolStripMenuItem_Click);
            // 
            // collisionsToolStripMenuItem
            // 
            this.collisionsToolStripMenuItem.Name = "collisionsToolStripMenuItem";
            this.collisionsToolStripMenuItem.Size = new System.Drawing.Size(155, 26);
            this.collisionsToolStripMenuItem.Text = "Collisions";
            this.collisionsToolStripMenuItem.Click += new System.EventHandler(this.floorsToolStripMenuItem_Click);
            // 
            // pathsToolStripMenuItem
            // 
            this.pathsToolStripMenuItem.Checked = true;
            this.pathsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.pathsToolStripMenuItem.Name = "pathsToolStripMenuItem";
            this.pathsToolStripMenuItem.Size = new System.Drawing.Size(155, 26);
            this.pathsToolStripMenuItem.Text = "Paths";
            this.pathsToolStripMenuItem.Click += new System.EventHandler(this.floorsToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showFloorDuplicatesToolStripMenuItem,
            this.dumpConversionGridsToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(123, 24);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // showFloorDuplicatesToolStripMenuItem
            // 
            this.showFloorDuplicatesToolStripMenuItem.Name = "showFloorDuplicatesToolStripMenuItem";
            this.showFloorDuplicatesToolStripMenuItem.Size = new System.Drawing.Size(245, 26);
            this.showFloorDuplicatesToolStripMenuItem.Text = "Dump floor duplicates";
            this.showFloorDuplicatesToolStripMenuItem.Click += new System.EventHandler(this.dumpFloorDuplicatesToolStripMenuItem_Click);
            // 
            // dumpConversionGridsToolStripMenuItem
            // 
            this.dumpConversionGridsToolStripMenuItem.Name = "dumpConversionGridsToolStripMenuItem";
            this.dumpConversionGridsToolStripMenuItem.Size = new System.Drawing.Size(245, 26);
            this.dumpConversionGridsToolStripMenuItem.Text = "Dump conversion grids";
            this.dumpConversionGridsToolStripMenuItem.Click += new System.EventHandler(this.dumpConversionGridsToolStripMenuItem_Click);
            // 
            // PlayTimer
            // 
            this.PlayTimer.Enabled = true;
            this.PlayTimer.Interval = 1000;
            this.PlayTimer.Tick += new System.EventHandler(this.PlayTimer_Tick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Diablo Map|*.ds1|Dispel Map|*.map";
            // 
            // Board
            // 
            this.Board.BackColor = System.Drawing.Color.Black;
            this.Board.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Board.Game = null;
            this.Board.Location = new System.Drawing.Point(0, 0);
            this.Board.Name = "Board";
            this.Board.ScrollPos = ((System.Drawing.PointF)(resources.GetObject("Board.ScrollPos")));
            this.Board.Size = new System.Drawing.Size(906, 800);
            this.Board.TabIndex = 1;
            this.Board.Zoom = 0.5F;
            // 
            // StrategyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1185, 800);
            this.Controls.Add(this.Board);
            this.Controls.Add(this.PropPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "StrategyForm";
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StrategyForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.StrategyForm_KeyDown);
            this.PropPanel.ResumeLayout(false);
            this.PropPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MapView)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private TBoard Board;
        private System.Windows.Forms.Panel PropPanel;
        private System.Windows.Forms.PictureBox MapView;
        private System.Windows.Forms.Timer PlayTimer;
        private System.Windows.Forms.ListBox CitiesList;
        private System.Windows.Forms.ListBox HerosList;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListView ResourceView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label MapNameLbl;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem floorsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem wallsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem spritesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem roofsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showFloorDuplicatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eventsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem boundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gridToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collisionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pathsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dumpConversionGridsToolStripMenuItem;
    }
}

