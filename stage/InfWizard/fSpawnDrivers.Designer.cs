namespace InfWizard
{
    partial class fSpawnDrivers
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fSpawnDrivers));
            this.panelSpawnDirectory = new System.Windows.Forms.Panel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.cmdSkip = new System.Windows.Forms.Button();
            this.cmdContinue = new System.Windows.Forms.Button();
            this.cmdDriverSourcePath = new System.Windows.Forms.Button();
            this.txtDriverSourcePath = new System.Windows.Forms.TextBox();
            this.labelSPAWNDRIVER_INFO_TEXT = new System.Windows.Forms.Label();
            this.findDriverWorker = new System.ComponentModel.BackgroundWorker();
            this.panelSpawnDirectory.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelSpawnDirectory
            // 
            this.panelSpawnDirectory.Controls.Add(this.statusStrip1);
            this.panelSpawnDirectory.Controls.Add(this.cmdSkip);
            this.panelSpawnDirectory.Controls.Add(this.cmdContinue);
            this.panelSpawnDirectory.Controls.Add(this.cmdDriverSourcePath);
            this.panelSpawnDirectory.Controls.Add(this.txtDriverSourcePath);
            this.panelSpawnDirectory.Controls.Add(this.labelSPAWNDRIVER_INFO_TEXT);
            this.panelSpawnDirectory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSpawnDirectory.Location = new System.Drawing.Point(0, 0);
            this.panelSpawnDirectory.Name = "panelSpawnDirectory";
            this.panelSpawnDirectory.Size = new System.Drawing.Size(361, 159);
            this.panelSpawnDirectory.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tslStatus,
            this.tsProgress});
            this.statusStrip1.Location = new System.Drawing.Point(0, 137);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(361, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tslStatus
            // 
            this.tslStatus.Name = "tslStatus";
            this.tslStatus.Size = new System.Drawing.Size(346, 17);
            this.tslStatus.Spring = true;
            this.tslStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tsProgress
            // 
            this.tsProgress.Name = "tsProgress";
            this.tsProgress.Size = new System.Drawing.Size(100, 16);
            this.tsProgress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.tsProgress.Visible = false;
            // 
            // cmdSkip
            // 
            this.cmdSkip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSkip.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdSkip.Location = new System.Drawing.Point(196, 109);
            this.cmdSkip.Name = "cmdSkip";
            this.cmdSkip.Size = new System.Drawing.Size(75, 23);
            this.cmdSkip.TabIndex = 5;
            this.cmdSkip.Text = "&Cancel";
            this.cmdSkip.UseVisualStyleBackColor = true;
            // 
            // cmdContinue
            // 
            this.cmdContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdContinue.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdContinue.Enabled = false;
            this.cmdContinue.Location = new System.Drawing.Point(277, 109);
            this.cmdContinue.Name = "cmdContinue";
            this.cmdContinue.Size = new System.Drawing.Size(81, 23);
            this.cmdContinue.TabIndex = 4;
            this.cmdContinue.Text = "&Ok";
            this.cmdContinue.UseVisualStyleBackColor = true;
            this.cmdContinue.Click += new System.EventHandler(this.cmdContinue_Click);
            // 
            // cmdDriverSourcePath
            // 
            this.cmdDriverSourcePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdDriverSourcePath.AutoSize = true;
            this.cmdDriverSourcePath.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cmdDriverSourcePath.Image = ((System.Drawing.Image)(resources.GetObject("cmdDriverSourcePath.Image")));
            this.cmdDriverSourcePath.Location = new System.Drawing.Point(277, 70);
            this.cmdDriverSourcePath.Name = "cmdDriverSourcePath";
            this.cmdDriverSourcePath.Size = new System.Drawing.Size(81, 23);
            this.cmdDriverSourcePath.TabIndex = 2;
            this.cmdDriverSourcePath.Text = "Directory..";
            this.cmdDriverSourcePath.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.cmdDriverSourcePath.UseVisualStyleBackColor = true;
            this.cmdDriverSourcePath.Click += new System.EventHandler(this.cmdDriverSourcePath_Click);
            // 
            // txtDriverSourcePath
            // 
            this.txtDriverSourcePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDriverSourcePath.Location = new System.Drawing.Point(3, 72);
            this.txtDriverSourcePath.Name = "txtDriverSourcePath";
            this.txtDriverSourcePath.ReadOnly = true;
            this.txtDriverSourcePath.Size = new System.Drawing.Size(268, 20);
            this.txtDriverSourcePath.TabIndex = 1;
            this.txtDriverSourcePath.TextChanged += new System.EventHandler(this.txtDriverSourcePath_TextChanged);
            // 
            // labelSPAWNDRIVER_INFO_TEXT
            // 
            this.labelSPAWNDRIVER_INFO_TEXT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSPAWNDRIVER_INFO_TEXT.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelSPAWNDRIVER_INFO_TEXT.Location = new System.Drawing.Point(3, 9);
            this.labelSPAWNDRIVER_INFO_TEXT.Name = "labelSPAWNDRIVER_INFO_TEXT";
            this.labelSPAWNDRIVER_INFO_TEXT.Size = new System.Drawing.Size(355, 58);
            this.labelSPAWNDRIVER_INFO_TEXT.TabIndex = 0;
            this.labelSPAWNDRIVER_INFO_TEXT.Text = "#SPAWNDRIVER_INFO_TEXT#";
            // 
            // findDriverWorker
            // 
            this.findDriverWorker.WorkerSupportsCancellation = true;
            this.findDriverWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.findDriverWorker_DoWork);
            this.findDriverWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.findDriverWorker_RunWorkerCompleted);
            // 
            // fSpawnDrivers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 159);
            this.Controls.Add(this.panelSpawnDirectory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximumSize = new System.Drawing.Size(800, 183);
            this.MinimumSize = new System.Drawing.Size(367, 183);
            this.Name = "fSpawnDrivers";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find Source Drivers";
            this.Load += new System.EventHandler(this.fSpawnDrivers_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.fSpawnDrivers_FormClosing);
            this.panelSpawnDirectory.ResumeLayout(false);
            this.panelSpawnDirectory.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }



        private System.Windows.Forms.Panel panelSpawnDirectory;
        private System.Windows.Forms.Label labelSPAWNDRIVER_INFO_TEXT;
        private System.Windows.Forms.Button cmdDriverSourcePath;
        private System.Windows.Forms.TextBox txtDriverSourcePath;
        private System.Windows.Forms.Button cmdSkip;
        private System.Windows.Forms.Button cmdContinue;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tslStatus;
        private System.Windows.Forms.ToolStripProgressBar tsProgress;
        private System.ComponentModel.BackgroundWorker findDriverWorker;
    }
}