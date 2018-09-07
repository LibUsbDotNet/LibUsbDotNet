namespace Test_DeviceNotify
{
    partial class fTestDeviceNotify
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



        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tNotify = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tNotify
            // 
            this.tNotify.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tNotify.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tNotify.Location = new System.Drawing.Point(0, 0);
            this.tNotify.Multiline = true;
            this.tNotify.Name = "tNotify";
            this.tNotify.ReadOnly = true;
            this.tNotify.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tNotify.Size = new System.Drawing.Size(740, 340);
            this.tNotify.TabIndex = 0;
            this.tNotify.WordWrap = false;
            this.tNotify.DoubleClick += new System.EventHandler(this.tNotify_DoubleClick);
            // 
            // fTestDeviceNotify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 340);
            this.Controls.Add(this.tNotify);
            this.Name = "fTestDeviceNotify";
            this.Text = "Test Device Notify";
            this.ResumeLayout(false);
            this.PerformLayout();

        }



        private System.Windows.Forms.TextBox tNotify;
    }
}

