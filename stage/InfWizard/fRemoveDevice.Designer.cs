namespace InfWizard
{
    partial class fRemoveDevice
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fRemoveDevice));
            this.removeDeviceOptionGrid = new System.Windows.Forms.PropertyGrid();
            this.warningLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.removeCommand = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // removeDeviceOptionGrid
            // 
            this.removeDeviceOptionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.removeDeviceOptionGrid.Location = new System.Drawing.Point(12, 76);
            this.removeDeviceOptionGrid.Name = "removeDeviceOptionGrid";
            this.removeDeviceOptionGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.removeDeviceOptionGrid.Size = new System.Drawing.Size(409, 173);
            this.removeDeviceOptionGrid.TabIndex = 0;
            this.removeDeviceOptionGrid.ToolbarVisible = false;
            // 
            // warningLabel
            // 
            this.warningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.warningLabel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.warningLabel.Location = new System.Drawing.Point(34, 15);
            this.warningLabel.Name = "warningLabel";
            this.warningLabel.Size = new System.Drawing.Size(387, 58);
            this.warningLabel.TabIndex = 1;
            this.warningLabel.Text = resources.GetString("warningLabel.Text");
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 15);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // removeCommand
            // 
            this.removeCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.removeCommand.Location = new System.Drawing.Point(346, 255);
            this.removeCommand.Name = "removeCommand";
            this.removeCommand.Size = new System.Drawing.Size(75, 23);
            this.removeCommand.TabIndex = 3;
            this.removeCommand.Text = "Remove";
            this.removeCommand.UseVisualStyleBackColor = true;
            this.removeCommand.Click += new System.EventHandler(this.removeCommand_Click);
            // 
            // RemoveDeviceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 289);
            this.Controls.Add(this.removeCommand);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.warningLabel);
            this.Controls.Add(this.removeDeviceOptionGrid);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RemoveDeviceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remove USB Device";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid removeDeviceOptionGrid;
        private System.Windows.Forms.Label warningLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button removeCommand;
    }
}