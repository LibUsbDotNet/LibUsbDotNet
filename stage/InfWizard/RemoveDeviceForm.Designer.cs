namespace InfWizard
{
    partial class RemoveDeviceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoveDeviceForm));
            this.removeDeviceOptionGrid = new System.Windows.Forms.PropertyGrid();
            this.warningLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.removeCommand = new System.Windows.Forms.Button();
            this.rtfRemoveDeviceStatus = new InfWizard.RtfSatusControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // removeDeviceOptionGrid
            // 
            this.removeDeviceOptionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.removeDeviceOptionGrid.Location = new System.Drawing.Point(12, 55);
            this.removeDeviceOptionGrid.Name = "removeDeviceOptionGrid";
            this.removeDeviceOptionGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.removeDeviceOptionGrid.Size = new System.Drawing.Size(409, 204);
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
            this.warningLabel.Size = new System.Drawing.Size(387, 37);
            this.warningLabel.TabIndex = 1;
            this.warningLabel.Text = "This utility makes permanent changes to the windows driver registry!";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::InfWizard.resInfWizard.YieldImage;
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
            this.removeCommand.Location = new System.Drawing.Point(346, 362);
            this.removeCommand.Name = "removeCommand";
            this.removeCommand.Size = new System.Drawing.Size(75, 23);
            this.removeCommand.TabIndex = 3;
            this.removeCommand.Text = "Remove";
            this.removeCommand.UseVisualStyleBackColor = true;
            this.removeCommand.Click += new System.EventHandler(this.removeCommand_Click);
            // 
            // rtfRemoveDeviceStatus
            // 
            this.rtfRemoveDeviceStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfRemoveDeviceStatus.Location = new System.Drawing.Point(3, 17);
            this.rtfRemoveDeviceStatus.LoggingEnabled = true;
            this.rtfRemoveDeviceStatus.Name = "rtfRemoveDeviceStatus";
            this.rtfRemoveDeviceStatus.Size = new System.Drawing.Size(403, 75);
            this.rtfRemoveDeviceStatus.StatusFilter = null;
            this.rtfRemoveDeviceStatus.TabIndex = 4;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.rtfRemoveDeviceStatus);
            this.groupBox1.Location = new System.Drawing.Point(12, 262);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(409, 95);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Remove Status";
            // 
            // RemoveDeviceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 396);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.removeCommand);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.warningLabel);
            this.Controls.Add(this.removeDeviceOptionGrid);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RemoveDeviceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remove USB Device";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RemoveDeviceForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid removeDeviceOptionGrid;
        private System.Windows.Forms.Label warningLabel;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button removeCommand;
        private RtfSatusControl rtfRemoveDeviceStatus;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}