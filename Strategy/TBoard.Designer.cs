namespace Strategy
{
    partial class TBoard
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.DialogBox1 = new System.Windows.Forms.Label();
            this.DialogBox2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // DialogBox1
            // 
            this.DialogBox1.BackColor = System.Drawing.Color.Transparent;
            this.DialogBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.DialogBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.DialogBox1.ForeColor = System.Drawing.Color.Lime;
            this.DialogBox1.Location = new System.Drawing.Point(0, 611);
            this.DialogBox1.Name = "DialogBox1";
            this.DialogBox1.Size = new System.Drawing.Size(1153, 98);
            this.DialogBox1.TabIndex = 0;
            this.DialogBox1.Text = "To be or not to be";
            this.DialogBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DialogBox1.Visible = false;
            this.DialogBox1.Click += new System.EventHandler(this.DialogBox1_Click);
            this.DialogBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.label1_Paint);
            this.DialogBox1.MouseEnter += new System.EventHandler(this.DialogBox1_MouseEnter);
            this.DialogBox1.MouseLeave += new System.EventHandler(this.DialogBox1_MouseLeave);
            // 
            // DialogBox2
            // 
            this.DialogBox2.BackColor = System.Drawing.Color.Transparent;
            this.DialogBox2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.DialogBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.DialogBox2.ForeColor = System.Drawing.Color.Lime;
            this.DialogBox2.Location = new System.Drawing.Point(0, 513);
            this.DialogBox2.Name = "DialogBox2";
            this.DialogBox2.Size = new System.Drawing.Size(1153, 98);
            this.DialogBox2.TabIndex = 1;
            this.DialogBox2.Text = "To be or not to be";
            this.DialogBox2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DialogBox2.Visible = false;
            this.DialogBox2.Click += new System.EventHandler(this.DialogBox1_Click);
            this.DialogBox2.Paint += new System.Windows.Forms.PaintEventHandler(this.label1_Paint);
            this.DialogBox2.MouseEnter += new System.EventHandler(this.DialogBox1_MouseEnter);
            this.DialogBox2.MouseLeave += new System.EventHandler(this.DialogBox1_MouseLeave);
            // 
            // TBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.DialogBox2);
            this.Controls.Add(this.DialogBox1);
            this.Name = "TBoard";
            this.Size = new System.Drawing.Size(1153, 709);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label DialogBox1;
        private System.Windows.Forms.Label DialogBox2;
    }
}
