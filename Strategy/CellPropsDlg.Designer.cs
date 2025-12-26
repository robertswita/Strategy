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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CellPropsDlg));
            this.FloorBtn = new System.Windows.Forms.Button();
            this.WallBtn = new System.Windows.Forms.Button();
            this.RoofBtn = new System.Windows.Forms.Button();
            this.EventBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // FloorBtn
            // 
            this.FloorBtn.Image = ((System.Drawing.Image)(resources.GetObject("FloorBtn.Image")));
            this.FloorBtn.Location = new System.Drawing.Point(36, 394);
            this.FloorBtn.Name = "FloorBtn";
            this.FloorBtn.Size = new System.Drawing.Size(108, 95);
            this.FloorBtn.TabIndex = 0;
            this.FloorBtn.Text = "floor";
            this.FloorBtn.UseVisualStyleBackColor = true;
            // 
            // WallBtn
            // 
            this.WallBtn.Location = new System.Drawing.Point(36, 228);
            this.WallBtn.Name = "WallBtn";
            this.WallBtn.Size = new System.Drawing.Size(108, 95);
            this.WallBtn.TabIndex = 1;
            this.WallBtn.Text = "floor";
            this.WallBtn.UseVisualStyleBackColor = true;
            // 
            // RoofBtn
            // 
            this.RoofBtn.Location = new System.Drawing.Point(36, 67);
            this.RoofBtn.Name = "RoofBtn";
            this.RoofBtn.Size = new System.Drawing.Size(108, 95);
            this.RoofBtn.TabIndex = 2;
            this.RoofBtn.Text = "floor";
            this.RoofBtn.UseVisualStyleBackColor = true;
            // 
            // EventBox
            // 
            this.EventBox.Location = new System.Drawing.Point(330, 86);
            this.EventBox.Name = "EventBox";
            this.EventBox.Size = new System.Drawing.Size(100, 22);
            this.EventBox.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(327, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Event";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(519, 481);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(101, 45);
            this.button1.TabIndex = 5;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button2.Location = new System.Drawing.Point(648, 481);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(101, 45);
            this.button2.TabIndex = 6;
            this.button2.Text = "OK";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // CellPropsDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(801, 562);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.EventBox);
            this.Controls.Add(this.RoofBtn);
            this.Controls.Add(this.WallBtn);
            this.Controls.Add(this.FloorBtn);
            this.Name = "CellPropsDlg";
            this.Text = "CellPropsDlg";
            this.Load += new System.EventHandler(this.CellPropsDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button FloorBtn;
        private System.Windows.Forms.Button WallBtn;
        private System.Windows.Forms.Button RoofBtn;
        private System.Windows.Forms.TextBox EventBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}