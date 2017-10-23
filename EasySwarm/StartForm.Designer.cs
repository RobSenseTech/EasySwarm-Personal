namespace EasySwarm
{
    partial class StartForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartForm));
            this.btn_en = new CCWin.SkinControl.SkinButton();
            this.btn_ch = new CCWin.SkinControl.SkinButton();
            this.SuspendLayout();
            // 
            // btn_en
            // 
            this.btn_en.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_en.BackColor = System.Drawing.Color.Transparent;
            this.btn_en.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(201)))), ((int)(((byte)(24)))));
            this.btn_en.BorderColor = System.Drawing.Color.Lime;
            this.btn_en.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_en.DownBack = ((System.Drawing.Image)(resources.GetObject("btn_en.DownBack")));
            this.btn_en.DrawType = CCWin.SkinControl.DrawStyle.Img;
            this.btn_en.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_en.ForeColor = System.Drawing.Color.White;
            this.btn_en.Location = new System.Drawing.Point(286, 155);
            this.btn_en.MouseBack = ((System.Drawing.Image)(resources.GetObject("btn_en.MouseBack")));
            this.btn_en.Name = "btn_en";
            this.btn_en.NormlBack = ((System.Drawing.Image)(resources.GetObject("btn_en.NormlBack")));
            this.btn_en.Palace = true;
            this.btn_en.Radius = 4;
            this.btn_en.Size = new System.Drawing.Size(168, 76);
            this.btn_en.TabIndex = 206;
            this.btn_en.Text = "ENGLISH";
            this.btn_en.UseVisualStyleBackColor = false;
            this.btn_en.Click += new System.EventHandler(this.btn_en_Click);
            // 
            // btn_ch
            // 
            this.btn_ch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_ch.BackColor = System.Drawing.Color.Transparent;
            this.btn_ch.BaseColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(201)))), ((int)(((byte)(24)))));
            this.btn_ch.BorderColor = System.Drawing.Color.Lime;
            this.btn_ch.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.btn_ch.DownBack = ((System.Drawing.Image)(resources.GetObject("btn_ch.DownBack")));
            this.btn_ch.DrawType = CCWin.SkinControl.DrawStyle.Img;
            this.btn_ch.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_ch.ForeColor = System.Drawing.Color.White;
            this.btn_ch.Location = new System.Drawing.Point(53, 155);
            this.btn_ch.MouseBack = ((System.Drawing.Image)(resources.GetObject("btn_ch.MouseBack")));
            this.btn_ch.Name = "btn_ch";
            this.btn_ch.NormlBack = ((System.Drawing.Image)(resources.GetObject("btn_ch.NormlBack")));
            this.btn_ch.Palace = true;
            this.btn_ch.Radius = 4;
            this.btn_ch.Size = new System.Drawing.Size(168, 76);
            this.btn_ch.TabIndex = 207;
            this.btn_ch.Text = "中文";
            this.btn_ch.UseVisualStyleBackColor = false;
            this.btn_ch.Click += new System.EventHandler(this.btn_ch_Click);
            // 
            // StartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Highlight;
            this.ClientSize = new System.Drawing.Size(508, 362);
            this.Controls.Add(this.btn_ch);
            this.Controls.Add(this.btn_en);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.Name = "StartForm";
            this.ShowDrawIcon = false;
            this.Text = "StartForm";
            this.Load += new System.EventHandler(this.StartForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private CCWin.SkinControl.SkinButton btn_en;
        private CCWin.SkinControl.SkinButton btn_ch;
    }
}