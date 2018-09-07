namespace Benchmark
{
    partial class fBenchmark
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
            this.components = new System.ComponentModel.Container();
            this.cboDevice = new System.Windows.Forms.ComboBox();
            this.cmdOpenClose = new System.Windows.Forms.Button();
            this.panTest = new System.Windows.Forms.Panel();
            this.cmdGetDeviceInfo = new System.Windows.Forms.Button();
            this.GetConfigValue = new System.Windows.Forms.Button();
            this.tInfo = new System.Windows.Forms.TextBox();
            this.cmdGetTestType = new System.Windows.Forms.Button();
            this.lDataRateEP1 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panDevice = new System.Windows.Forms.Panel();
            this.cboTestType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.benchParamsPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.timerUpdateUI = new System.Windows.Forms.Timer(this.components);
            this.panTest.SuspendLayout();
            this.panDevice.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboDevice
            // 
            this.cboDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDevice.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboDevice.FormattingEnabled = true;
            this.cboDevice.Items.AddRange(new object[] {
            "b35924d6-0000-4a9e-9782-5524a4b79bac"});
            this.cboDevice.Location = new System.Drawing.Point(6, 10);
            this.cboDevice.Name = "cboDevice";
            this.cboDevice.Size = new System.Drawing.Size(513, 22);
            this.cboDevice.TabIndex = 21;
            this.cboDevice.DropDown += new System.EventHandler(this.cboDevice_DropDown);
            // 
            // cmdOpenClose
            // 
            this.cmdOpenClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOpenClose.Location = new System.Drawing.Point(525, 10);
            this.cmdOpenClose.Name = "cmdOpenClose";
            this.cmdOpenClose.Size = new System.Drawing.Size(75, 23);
            this.cmdOpenClose.TabIndex = 20;
            this.cmdOpenClose.Text = "Open";
            this.cmdOpenClose.UseVisualStyleBackColor = true;
            this.cmdOpenClose.Click += new System.EventHandler(this.cmdOpenClose_Click);
            // 
            // panTest
            // 
            this.panTest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panTest.Controls.Add(this.cmdGetDeviceInfo);
            this.panTest.Controls.Add(this.GetConfigValue);
            this.panTest.Controls.Add(this.tInfo);
            this.panTest.Controls.Add(this.cmdGetTestType);
            this.panTest.Controls.Add(this.lDataRateEP1);
            this.panTest.Controls.Add(this.label1);
            this.panTest.Enabled = false;
            this.panTest.Location = new System.Drawing.Point(6, 27);
            this.panTest.Name = "panTest";
            this.panTest.Size = new System.Drawing.Size(330, 257);
            this.panTest.TabIndex = 22;
            // 
            // cmdGetDeviceInfo
            // 
            this.cmdGetDeviceInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdGetDeviceInfo.Location = new System.Drawing.Point(243, 231);
            this.cmdGetDeviceInfo.Name = "cmdGetDeviceInfo";
            this.cmdGetDeviceInfo.Size = new System.Drawing.Size(83, 23);
            this.cmdGetDeviceInfo.TabIndex = 27;
            this.cmdGetDeviceInfo.Text = "Device Info";
            this.cmdGetDeviceInfo.UseVisualStyleBackColor = true;
            this.cmdGetDeviceInfo.Click += new System.EventHandler(this.cmdGetDeviceInfo_Click);
            // 
            // GetConfigValue
            // 
            this.GetConfigValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.GetConfigValue.Location = new System.Drawing.Point(154, 231);
            this.GetConfigValue.Name = "GetConfigValue";
            this.GetConfigValue.Size = new System.Drawing.Size(83, 23);
            this.GetConfigValue.TabIndex = 26;
            this.GetConfigValue.Text = "Config Info";
            this.GetConfigValue.UseVisualStyleBackColor = true;
            this.GetConfigValue.Click += new System.EventHandler(this.GetConfigValue_Click);
            // 
            // tInfo
            // 
            this.tInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tInfo.BackColor = System.Drawing.SystemColors.Info;
            this.tInfo.ForeColor = System.Drawing.Color.Black;
            this.tInfo.Location = new System.Drawing.Point(6, 34);
            this.tInfo.Multiline = true;
            this.tInfo.Name = "tInfo";
            this.tInfo.ReadOnly = true;
            this.tInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tInfo.Size = new System.Drawing.Size(320, 191);
            this.tInfo.TabIndex = 21;
            // 
            // cmdGetTestType
            // 
            this.cmdGetTestType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdGetTestType.Location = new System.Drawing.Point(6, 231);
            this.cmdGetTestType.Name = "cmdGetTestType";
            this.cmdGetTestType.Size = new System.Drawing.Size(75, 23);
            this.cmdGetTestType.TabIndex = 25;
            this.cmdGetTestType.Text = "Get Test";
            this.cmdGetTestType.UseVisualStyleBackColor = true;
            this.cmdGetTestType.Click += new System.EventHandler(this.cmdGetTestType_Click);
            // 
            // lDataRateEP1
            // 
            this.lDataRateEP1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lDataRateEP1.Location = new System.Drawing.Point(139, 9);
            this.lDataRateEP1.Name = "lDataRateEP1";
            this.lDataRateEP1.Size = new System.Drawing.Size(187, 13);
            this.lDataRateEP1.TabIndex = 20;
            this.toolTip1.SetToolTip(this.lDataRateEP1, "Bytes per/second (Number of packets transferred)");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Bytes/Sec (Usb Packets):";
            // 
            // panDevice
            // 
            this.panDevice.Controls.Add(this.cboTestType);
            this.panDevice.Controls.Add(this.label2);
            this.panDevice.Controls.Add(this.panTest);
            this.panDevice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panDevice.Enabled = false;
            this.panDevice.Location = new System.Drawing.Point(0, 0);
            this.panDevice.Name = "panDevice";
            this.panDevice.Size = new System.Drawing.Size(336, 284);
            this.panDevice.TabIndex = 23;
            // 
            // cboTestType
            // 
            this.cboTestType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cboTestType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTestType.FormattingEnabled = true;
            this.cboTestType.Location = new System.Drawing.Point(79, 3);
            this.cboTestType.Name = "cboTestType";
            this.cboTestType.Size = new System.Drawing.Size(253, 21);
            this.cboTestType.TabIndex = 24;
            this.cboTestType.SelectedIndexChanged += new System.EventHandler(this.cboTestType_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "Select Test:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tslStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 325);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(604, 22);
            this.statusStrip1.TabIndex = 24;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(42, 17);
            this.toolStripStatusLabel1.Text = "Status:";
            // 
            // tslStatus
            // 
            this.tslStatus.Name = "tslStatus";
            this.tslStatus.Size = new System.Drawing.Size(547, 17);
            this.tslStatus.Spring = true;
            this.tslStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // benchParamsPropertyGrid
            // 
            this.benchParamsPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.benchParamsPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.benchParamsPropertyGrid.Name = "benchParamsPropertyGrid";
            this.benchParamsPropertyGrid.Size = new System.Drawing.Size(254, 284);
            this.benchParamsPropertyGrid.TabIndex = 25;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(6, 39);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.benchParamsPropertyGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panDevice);
            this.splitContainer1.Size = new System.Drawing.Size(594, 284);
            this.splitContainer1.SplitterDistance = 254;
            this.splitContainer1.TabIndex = 26;
            // 
            // timerUpdateUI
            // 
            this.timerUpdateUI.Interval = 500;
            this.timerUpdateUI.Tick += new System.EventHandler(this.timerUpdateUI_Tick);
            // 
            // fBenchmark
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 347);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.cboDevice);
            this.Controls.Add(this.cmdOpenClose);
            this.MinimumSize = new System.Drawing.Size(410, 350);
            this.Name = "fBenchmark";
            this.Text = "PIC Endpoint Benchmark";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.panTest.ResumeLayout(false);
            this.panTest.PerformLayout();
            this.panDevice.ResumeLayout(false);
            this.panDevice.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }



        private System.Windows.Forms.ComboBox cboDevice;
        private System.Windows.Forms.Button cmdOpenClose;
        private System.Windows.Forms.Panel panTest;
        private System.Windows.Forms.Label lDataRateEP1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panDevice;
        private System.Windows.Forms.ComboBox cboTestType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tInfo;
        private System.Windows.Forms.Button cmdGetTestType;
        private System.Windows.Forms.Button GetConfigValue;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel tslStatus;
        private System.Windows.Forms.PropertyGrid benchParamsPropertyGrid;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button cmdGetDeviceInfo;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer timerUpdateUI;
    }
}

