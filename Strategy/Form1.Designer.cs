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
            this.PlayTimer = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.Board = new Strategy.TBoard();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.PropPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MapView)).BeginInit();
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
            this.MapView.Location = new System.Drawing.Point(18, 22);
            this.MapView.Name = "MapView";
            this.MapView.Size = new System.Drawing.Size(244, 211);
            this.MapView.TabIndex = 0;
            this.MapView.TabStop = false;
            this.MapView.Paint += new System.Windows.Forms.PaintEventHandler(this.MapView_Paint);
            this.MapView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapView_MouseMove);
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
    }
}

