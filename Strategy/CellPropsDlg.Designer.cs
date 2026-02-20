namespace Strategy
{
    partial class CellPropsDlg
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
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.FloorBox = new System.Windows.Forms.NumericUpDown();
            this.RoofBox = new System.Windows.Forms.NumericUpDown();
            this.WallBox = new System.Windows.Forms.NumericUpDown();
            this.EventBox = new System.Windows.Forms.NumericUpDown();
            this.FloorView = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.WallLayerBox = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.RoofView = new System.Windows.Forms.PictureBox();
            this.WallView = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.FloorBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RoofBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WallBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EventBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloorView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WallLayerBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RoofView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WallView)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(353, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Event";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(320, 618);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(101, 45);
            this.button1.TabIndex = 5;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button2.Location = new System.Drawing.Point(437, 618);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(101, 45);
            this.button2.TabIndex = 6;
            this.button2.Text = "OK";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // FloorBox
            // 
            this.FloorBox.Location = new System.Drawing.Point(153, 559);
            this.FloorBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.FloorBox.Name = "FloorBox";
            this.FloorBox.Size = new System.Drawing.Size(118, 22);
            this.FloorBox.TabIndex = 7;
            this.FloorBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.FloorBox.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // RoofBox
            // 
            this.RoofBox.Location = new System.Drawing.Point(153, 47);
            this.RoofBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.RoofBox.Name = "RoofBox";
            this.RoofBox.Size = new System.Drawing.Size(118, 22);
            this.RoofBox.TabIndex = 8;
            this.RoofBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.RoofBox.ValueChanged += new System.EventHandler(this.RoofBox_ValueChanged);
            // 
            // WallBox
            // 
            this.WallBox.Location = new System.Drawing.Point(288, 243);
            this.WallBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.WallBox.Name = "WallBox";
            this.WallBox.Size = new System.Drawing.Size(118, 22);
            this.WallBox.TabIndex = 9;
            this.WallBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.WallBox.ValueChanged += new System.EventHandler(this.WallBox_ValueChanged);
            // 
            // EventBox
            // 
            this.EventBox.Location = new System.Drawing.Point(356, 47);
            this.EventBox.Name = "EventBox";
            this.EventBox.Size = new System.Drawing.Size(108, 22);
            this.EventBox.TabIndex = 10;
            // 
            // FloorView
            // 
            this.FloorView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FloorView.Location = new System.Drawing.Point(36, 540);
            this.FloorView.Name = "FloorView";
            this.FloorView.Size = new System.Drawing.Size(108, 95);
            this.FloorView.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.FloorView.TabIndex = 11;
            this.FloorView.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(150, 540);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 16);
            this.label2.TabIndex = 12;
            this.label2.Text = "Floor:";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(153, 594);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(118, 41);
            this.button3.TabIndex = 13;
            this.button3.Text = "Clear";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(150, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 16);
            this.label3.TabIndex = 14;
            this.label3.Text = "Roof:";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(153, 82);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(118, 41);
            this.button4.TabIndex = 15;
            this.button4.Text = "Clear";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(288, 283);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(118, 41);
            this.button5.TabIndex = 16;
            this.button5.Text = "Clear";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // WallLayerBox
            // 
            this.WallLayerBox.Location = new System.Drawing.Point(288, 194);
            this.WallLayerBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.WallLayerBox.Name = "WallLayerBox";
            this.WallLayerBox.Size = new System.Drawing.Size(118, 22);
            this.WallLayerBox.TabIndex = 17;
            this.WallLayerBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.WallLayerBox.ValueChanged += new System.EventHandler(this.WallLayerBox_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(285, 175);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 16);
            this.label4.TabIndex = 18;
            this.label4.Text = "Wall Layer:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(285, 224);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 16);
            this.label5.TabIndex = 19;
            this.label5.Text = "Wall:";
            // 
            // RoofView
            // 
            this.RoofView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.RoofView.Location = new System.Drawing.Point(36, 28);
            this.RoofView.Name = "RoofView";
            this.RoofView.Size = new System.Drawing.Size(108, 95);
            this.RoofView.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.RoofView.TabIndex = 20;
            this.RoofView.TabStop = false;
            // 
            // WallView
            // 
            this.WallView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WallView.Location = new System.Drawing.Point(36, 175);
            this.WallView.Name = "WallView";
            this.WallView.Size = new System.Drawing.Size(235, 331);
            this.WallView.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.WallView.TabIndex = 21;
            this.WallView.TabStop = false;
            // 
            // CellPropsDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 690);
            this.Controls.Add(this.WallView);
            this.Controls.Add(this.RoofView);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.WallLayerBox);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FloorView);
            this.Controls.Add(this.EventBox);
            this.Controls.Add(this.WallBox);
            this.Controls.Add(this.RoofBox);
            this.Controls.Add(this.FloorBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Name = "CellPropsDlg";
            this.Text = "CellPropsDlg";
            this.Load += new System.EventHandler(this.CellPropsDlg_Load);
            ((System.ComponentModel.ISupportInitialize)(this.FloorBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RoofBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WallBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EventBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FloorView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WallLayerBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RoofView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WallView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.NumericUpDown FloorBox;
        private System.Windows.Forms.NumericUpDown RoofBox;
        private System.Windows.Forms.NumericUpDown WallBox;
        private System.Windows.Forms.NumericUpDown EventBox;
        private System.Windows.Forms.PictureBox FloorView;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.NumericUpDown WallLayerBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox RoofView;
        private System.Windows.Forms.PictureBox WallView;
    }
}