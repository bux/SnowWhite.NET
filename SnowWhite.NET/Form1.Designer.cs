namespace SnowWhite.NET
{
    partial class Form1
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
            this.m_btnStart = new System.Windows.Forms.Button();
            this.m_btnStop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // m_btnStart
            // 
            this.m_btnStart.BackColor = System.Drawing.SystemColors.Control;
            this.m_btnStart.Location = new System.Drawing.Point(12, 12);
            this.m_btnStart.Name = "m_btnStart";
            this.m_btnStart.Size = new System.Drawing.Size(112, 23);
            this.m_btnStart.TabIndex = 0;
            this.m_btnStart.Text = "Start everything!";
            this.m_btnStart.UseVisualStyleBackColor = false;
            this.m_btnStart.Click += new System.EventHandler(this.button1_Click);
            // 
            // m_btnStop
            // 
            this.m_btnStop.BackColor = System.Drawing.SystemColors.Control;
            this.m_btnStop.Enabled = false;
            this.m_btnStop.Location = new System.Drawing.Point(147, 12);
            this.m_btnStop.Name = "m_btnStop";
            this.m_btnStop.Size = new System.Drawing.Size(112, 23);
            this.m_btnStop.TabIndex = 1;
            this.m_btnStop.Text = "Stop everything!";
            this.m_btnStop.UseVisualStyleBackColor = false;
            this.m_btnStop.Click += new System.EventHandler(this.m_btnStop_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(311, 89);
            this.Controls.Add(this.m_btnStop);
            this.Controls.Add(this.m_btnStart);
            this.Name = "Form1";
            this.Text = "SnowWhite.NET";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button m_btnStart;
        private System.Windows.Forms.Button m_btnStop;
    }
}

