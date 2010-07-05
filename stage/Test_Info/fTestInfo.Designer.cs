namespace Test_Info
{
    partial class fTestInfo
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsNumDevices = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsRefresh = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsVersion = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsKernelType = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsKernelVersion = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cboDevices = new System.Windows.Forms.ComboBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyAsPlainTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabDevice = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tvInfo = new System.Windows.Forms.TreeView();
            this.tabRegistry = new System.Windows.Forms.TabPage();
            this.tRegProps = new System.Windows.Forms.TextBox();
            this.tabVersionInfo = new System.Windows.Forms.TabPage();
            this.rtfVersionInfo = new System.Windows.Forms.RichTextBox();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.tabDevice.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabRegistry.SuspendLayout();
            this.tabVersionInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.tsNumDevices,
            this.tsRefresh,
            this.tsVersion,
            this.tsKernelType,
            this.tsKernelVersion});
            this.statusStrip1.Location = new System.Drawing.Point(0, 279);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(430, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(81, 17);
            this.toolStripStatusLabel1.Text = "Devices Found:";
            // 
            // tsNumDevices
            // 
            this.tsNumDevices.Name = "tsNumDevices";
            this.tsNumDevices.Size = new System.Drawing.Size(13, 17);
            this.tsNumDevices.Text = "0";
            // 
            // tsRefresh
            // 
            this.tsRefresh.IsLink = true;
            this.tsRefresh.Name = "tsRefresh";
            this.tsRefresh.Size = new System.Drawing.Size(45, 17);
            this.tsRefresh.Text = "Refresh";
            this.tsRefresh.Click += new System.EventHandler(this.tsRefresh_Click);
            // 
            // tsVersion
            // 
            this.tsVersion.Name = "tsVersion";
            this.tsVersion.Size = new System.Drawing.Size(92, 17);
            this.tsVersion.Spring = true;
            this.tsVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tsKernelType
            // 
            this.tsKernelType.Name = "tsKernelType";
            this.tsKernelType.Size = new System.Drawing.Size(92, 17);
            this.tsKernelType.Spring = true;
            // 
            // tsKernelVersion
            // 
            this.tsKernelVersion.Name = "tsKernelVersion";
            this.tsKernelVersion.Size = new System.Drawing.Size(92, 17);
            this.tsKernelVersion.Spring = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.cboDevices);
            this.groupBox1.Location = new System.Drawing.Point(0, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(426, 39);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Usb Device";
            // 
            // cboDevices
            // 
            this.cboDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cboDevices.FormattingEnabled = true;
            this.cboDevices.Location = new System.Drawing.Point(3, 16);
            this.cboDevices.Name = "cboDevices";
            this.cboDevices.Size = new System.Drawing.Size(420, 21);
            this.cboDevices.TabIndex = 0;
            this.cboDevices.SelectedIndexChanged += new System.EventHandler(this.cboDevices_SelectedIndexChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(430, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem,
            this.copyAsPlainTextToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // copyAsPlainTextToolStripMenuItem
            // 
            this.copyAsPlainTextToolStripMenuItem.Enabled = false;
            this.copyAsPlainTextToolStripMenuItem.Name = "copyAsPlainTextToolStripMenuItem";
            this.copyAsPlainTextToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.copyAsPlainTextToolStripMenuItem.Text = "&Copy";
            this.copyAsPlainTextToolStripMenuItem.Click += new System.EventHandler(this.copyAsPlainTextToolStripMenuItem_Click);
            // 
            // tabDevice
            // 
            this.tabDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabDevice.Controls.Add(this.tabPage1);
            this.tabDevice.Controls.Add(this.tabRegistry);
            this.tabDevice.Controls.Add(this.tabVersionInfo);
            this.tabDevice.Location = new System.Drawing.Point(3, 69);
            this.tabDevice.Name = "tabDevice";
            this.tabDevice.SelectedIndex = 0;
            this.tabDevice.Size = new System.Drawing.Size(427, 207);
            this.tabDevice.TabIndex = 4;
            this.tabDevice.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabDevice_Selected);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tvInfo);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(419, 181);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Device Info";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tvInfo
            // 
            this.tvInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvInfo.Location = new System.Drawing.Point(3, 3);
            this.tvInfo.Name = "tvInfo";
            this.tvInfo.Size = new System.Drawing.Size(413, 175);
            this.tvInfo.TabIndex = 0;
            // 
            // tabRegistry
            // 
            this.tabRegistry.Controls.Add(this.tRegProps);
            this.tabRegistry.Location = new System.Drawing.Point(4, 22);
            this.tabRegistry.Name = "tabRegistry";
            this.tabRegistry.Padding = new System.Windows.Forms.Padding(3);
            this.tabRegistry.Size = new System.Drawing.Size(419, 181);
            this.tabRegistry.TabIndex = 1;
            this.tabRegistry.Text = "Registry Properties";
            this.tabRegistry.UseVisualStyleBackColor = true;
            // 
            // tRegProps
            // 
            this.tRegProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tRegProps.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tRegProps.Location = new System.Drawing.Point(3, 3);
            this.tRegProps.Multiline = true;
            this.tRegProps.Name = "tRegProps";
            this.tRegProps.ReadOnly = true;
            this.tRegProps.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tRegProps.Size = new System.Drawing.Size(413, 175);
            this.tRegProps.TabIndex = 0;
            // 
            // tabVersionInfo
            // 
            this.tabVersionInfo.Controls.Add(this.rtfVersionInfo);
            this.tabVersionInfo.Location = new System.Drawing.Point(4, 22);
            this.tabVersionInfo.Name = "tabVersionInfo";
            this.tabVersionInfo.Size = new System.Drawing.Size(419, 181);
            this.tabVersionInfo.TabIndex = 2;
            this.tabVersionInfo.Text = "Version Information";
            this.tabVersionInfo.UseVisualStyleBackColor = true;
            // 
            // rtfVersionInfo
            // 
            this.rtfVersionInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfVersionInfo.Location = new System.Drawing.Point(0, 0);
            this.rtfVersionInfo.Name = "rtfVersionInfo";
            this.rtfVersionInfo.ReadOnly = true;
            this.rtfVersionInfo.Size = new System.Drawing.Size(419, 181);
            this.rtfVersionInfo.TabIndex = 1;
            this.rtfVersionInfo.Text = "";
            // 
            // fTestInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 301);
            this.Controls.Add(this.tabDevice);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "fTestInfo";
            this.Text = "USB Cfg Interrorgator";
            this.Load += new System.EventHandler(this.fTestInfo_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.fTestInfo_FormClosing);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabDevice.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabRegistry.ResumeLayout(false);
            this.tabRegistry.PerformLayout();
            this.tabVersionInfo.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel tsNumDevices;
        private System.Windows.Forms.ToolStripStatusLabel tsRefresh;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox cboDevices;
        private System.Windows.Forms.ToolStripStatusLabel tsVersion;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.TabControl tabDevice;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabRegistry;
        private System.Windows.Forms.TextBox tRegProps;
        private System.Windows.Forms.ToolStripStatusLabel tsKernelType;
        private System.Windows.Forms.ToolStripStatusLabel tsKernelVersion;
        private System.Windows.Forms.TabPage tabVersionInfo;
        private System.Windows.Forms.RichTextBox rtfVersionInfo;
        private System.Windows.Forms.TreeView tvInfo;
        private System.Windows.Forms.ToolStripMenuItem copyAsPlainTextToolStripMenuItem;
    }
}