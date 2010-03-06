namespace InfWizard
{
    partial class fUsbInfWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fUsbInfWizard));
            this.wizMain = new Gui.Wizard.Wizard();
            this.wizPageMain = new Gui.Wizard.WizardPage();
            this.infoMain = new Gui.Wizard.InfoContainer();
            this.lMainCautionTest = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lMainText = new System.Windows.Forms.Label();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.cmdLoadProfile = new System.Windows.Forms.Button();
            this.wizDeviceConfiguration = new Gui.Wizard.WizardPage();
            this.cmdSaveProfile = new System.Windows.Forms.Button();
            this.cmdFindFrivers = new System.Windows.Forms.Button();
            this.gbDeviceConfiguration_DriverType = new System.Windows.Forms.GroupBox();
            this.rbDriverType_WinUsb = new System.Windows.Forms.RadioButton();
            this.rbDriverType_LibUsb = new System.Windows.Forms.RadioButton();
            this.pgDeviceConfiguration = new System.Windows.Forms.PropertyGrid();
            this.headerDeviceConfiguration = new Gui.Wizard.Header();
            this.wizDeviceSelection = new Gui.Wizard.WizardPage();
            this.gbDeviceSelection = new System.Windows.Forms.GroupBox();
            this.cmdRemoveDevice = new System.Windows.Forms.Button();
            this.rbDeviceSelection_AllDevices = new System.Windows.Forms.RadioButton();
            this.rbDeviceSelection_OnlyConnected = new System.Windows.Forms.RadioButton();
            this.lvDeviceSelection = new System.Windows.Forms.ListView();
            this.colVid = new System.Windows.Forms.ColumnHeader();
            this.colPid = new System.Windows.Forms.ColumnHeader();
            this.colDescr = new System.Windows.Forms.ColumnHeader();
            this.colMfu = new System.Windows.Forms.ColumnHeader();
            this.headerDeviceSelection = new Gui.Wizard.Header();
            this.infoPageMain = new Gui.Wizard.InfoPage();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.wizMain.SuspendLayout();
            this.wizPageMain.SuspendLayout();
            this.infoMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.wizDeviceConfiguration.SuspendLayout();
            this.gbDeviceConfiguration_DriverType.SuspendLayout();
            this.wizDeviceSelection.SuspendLayout();
            this.gbDeviceSelection.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizMain
            // 
            this.wizMain.Controls.Add(this.wizDeviceConfiguration);
            this.wizMain.Controls.Add(this.wizDeviceSelection);
            this.wizMain.Controls.Add(this.wizPageMain);
            this.wizMain.Controls.Add(this.infoPageMain);
            this.wizMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizMain.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wizMain.Location = new System.Drawing.Point(0, 0);
            this.wizMain.Name = "wizMain";
            this.wizMain.Pages.AddRange(new Gui.Wizard.WizardPage[] {
            this.wizPageMain,
            this.wizDeviceSelection,
            this.wizDeviceConfiguration});
            this.wizMain.Size = new System.Drawing.Size(637, 460);
            this.wizMain.TabIndex = 1;
            // 
            // wizPageMain
            // 
            this.wizPageMain.Controls.Add(this.infoMain);
            this.wizPageMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizPageMain.IsFinishPage = false;
            this.wizPageMain.Location = new System.Drawing.Point(0, 0);
            this.wizPageMain.Name = "wizPageMain";
            this.wizPageMain.Size = new System.Drawing.Size(637, 412);
            this.wizPageMain.TabIndex = 2;
            // 
            // infoMain
            // 
            this.infoMain.BackColor = System.Drawing.Color.White;
            this.infoMain.Controls.Add(this.lMainCautionTest);
            this.infoMain.Controls.Add(this.pictureBox1);
            this.infoMain.Controls.Add(this.lMainText);
            this.infoMain.Controls.Add(this.pictureBox4);
            this.infoMain.Controls.Add(this.cmdLoadProfile);
            this.infoMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoMain.Image = ((System.Drawing.Image)(resources.GetObject("infoMain.Image")));
            this.infoMain.Location = new System.Drawing.Point(0, 0);
            this.infoMain.Name = "infoMain";
            this.infoMain.PageTitle = "Welcome to the LibUsbDotNet USB Inf Creation Wizard";
            this.infoMain.Size = new System.Drawing.Size(637, 412);
            this.infoMain.TabIndex = 0;
            // 
            // lMainCautionTest
            // 
            this.lMainCautionTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lMainCautionTest.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lMainCautionTest.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lMainCautionTest.Location = new System.Drawing.Point(194, 202);
            this.lMainCautionTest.Name = "lMainCautionTest";
            this.lMainCautionTest.Size = new System.Drawing.Size(431, 149);
            this.lMainCautionTest.TabIndex = 17;
            this.lMainCautionTest.Text = resources.GetString("lMainCautionTest.Text");
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(172, 202);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(11, 11);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 16;
            this.pictureBox1.TabStop = false;
            // 
            // lMainText
            // 
            this.lMainText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lMainText.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lMainText.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lMainText.Location = new System.Drawing.Point(194, 61);
            this.lMainText.Name = "lMainText";
            this.lMainText.Size = new System.Drawing.Size(431, 127);
            this.lMainText.TabIndex = 14;
            this.lMainText.Text = "$MAIN_TEXT$";
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox4.Image")));
            this.pictureBox4.Location = new System.Drawing.Point(172, 61);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(16, 16);
            this.pictureBox4.TabIndex = 13;
            this.pictureBox4.TabStop = false;
            // 
            // cmdLoadProfile
            // 
            this.cmdLoadProfile.Location = new System.Drawing.Point(172, 365);
            this.cmdLoadProfile.Name = "cmdLoadProfile";
            this.cmdLoadProfile.Size = new System.Drawing.Size(114, 23);
            this.cmdLoadProfile.TabIndex = 15;
            this.cmdLoadProfile.Text = "&Load Profile";
            this.toolTip1.SetToolTip(this.cmdLoadProfile, "Loads a previously saved usb device profile and skips to the configuration panel." +
                    "");
            this.cmdLoadProfile.UseVisualStyleBackColor = true;
            this.cmdLoadProfile.Click += new System.EventHandler(this.cmdLoadProfile_Click);
            // 
            // wizDeviceConfiguration
            // 
            this.wizDeviceConfiguration.Controls.Add(this.cmdSaveProfile);
            this.wizDeviceConfiguration.Controls.Add(this.cmdFindFrivers);
            this.wizDeviceConfiguration.Controls.Add(this.gbDeviceConfiguration_DriverType);
            this.wizDeviceConfiguration.Controls.Add(this.pgDeviceConfiguration);
            this.wizDeviceConfiguration.Controls.Add(this.headerDeviceConfiguration);
            this.wizDeviceConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizDeviceConfiguration.IsFinishPage = false;
            this.wizDeviceConfiguration.Location = new System.Drawing.Point(0, 0);
            this.wizDeviceConfiguration.Name = "wizDeviceConfiguration";
            this.wizDeviceConfiguration.Size = new System.Drawing.Size(637, 412);
            this.wizDeviceConfiguration.TabIndex = 4;
            this.wizDeviceConfiguration.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wizDeviceConfiguration_CloseFromNext);
            this.wizDeviceConfiguration.CloseFromBack += new Gui.Wizard.PageEventHandler(this.wizDeviceConfiguration_CloseFromBack);
            this.wizDeviceConfiguration.ShowFromNext += new System.EventHandler(this.wizDeviceConfiguration_ShowFromNext);
            // 
            // cmdSaveProfile
            // 
            this.cmdSaveProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSaveProfile.Location = new System.Drawing.Point(476, 96);
            this.cmdSaveProfile.Name = "cmdSaveProfile";
            this.cmdSaveProfile.Size = new System.Drawing.Size(158, 23);
            this.cmdSaveProfile.TabIndex = 5;
            this.cmdSaveProfile.Text = "Save Profile..";
            this.toolTip1.SetToolTip(this.cmdSaveProfile, "Locate the parent directory where the source drivers for the selected driver type" +
                    " can be found.");
            this.cmdSaveProfile.UseVisualStyleBackColor = true;
            this.cmdSaveProfile.Click += new System.EventHandler(this.cmdSaveProfile_Click);
            // 
            // cmdFindFrivers
            // 
            this.cmdFindFrivers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdFindFrivers.Location = new System.Drawing.Point(476, 67);
            this.cmdFindFrivers.Name = "cmdFindFrivers";
            this.cmdFindFrivers.Size = new System.Drawing.Size(158, 23);
            this.cmdFindFrivers.TabIndex = 4;
            this.cmdFindFrivers.Text = "Find Required Driver(s)..";
            this.toolTip1.SetToolTip(this.cmdFindFrivers, "Locate the parent directory where the source drivers for the selected driver type" +
                    " can be found.");
            this.cmdFindFrivers.UseVisualStyleBackColor = true;
            this.cmdFindFrivers.Click += new System.EventHandler(this.cmdFindFrivers_Click);
            // 
            // gbDeviceConfiguration_DriverType
            // 
            this.gbDeviceConfiguration_DriverType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDeviceConfiguration_DriverType.Controls.Add(this.rbDriverType_WinUsb);
            this.gbDeviceConfiguration_DriverType.Controls.Add(this.rbDriverType_LibUsb);
            this.gbDeviceConfiguration_DriverType.Location = new System.Drawing.Point(3, 67);
            this.gbDeviceConfiguration_DriverType.Name = "gbDeviceConfiguration_DriverType";
            this.gbDeviceConfiguration_DriverType.Size = new System.Drawing.Size(467, 46);
            this.gbDeviceConfiguration_DriverType.TabIndex = 3;
            this.gbDeviceConfiguration_DriverType.TabStop = false;
            this.gbDeviceConfiguration_DriverType.Text = "Driver Type";
            // 
            // rbDriverType_WinUsb
            // 
            this.rbDriverType_WinUsb.AutoSize = true;
            this.rbDriverType_WinUsb.Location = new System.Drawing.Point(102, 20);
            this.rbDriverType_WinUsb.Name = "rbDriverType_WinUsb";
            this.rbDriverType_WinUsb.Size = new System.Drawing.Size(62, 17);
            this.rbDriverType_WinUsb.TabIndex = 1;
            this.rbDriverType_WinUsb.Text = "WinUSB";
            this.rbDriverType_WinUsb.UseVisualStyleBackColor = true;
            this.rbDriverType_WinUsb.CheckedChanged += new System.EventHandler(this.rbDriverType_WinUsb_CheckedChanged);
            // 
            // rbDriverType_LibUsb
            // 
            this.rbDriverType_LibUsb.AutoSize = true;
            this.rbDriverType_LibUsb.Checked = true;
            this.rbDriverType_LibUsb.Location = new System.Drawing.Point(9, 20);
            this.rbDriverType_LibUsb.Name = "rbDriverType_LibUsb";
            this.rbDriverType_LibUsb.Size = new System.Drawing.Size(56, 17);
            this.rbDriverType_LibUsb.TabIndex = 0;
            this.rbDriverType_LibUsb.TabStop = true;
            this.rbDriverType_LibUsb.Text = "LibUsb";
            this.rbDriverType_LibUsb.UseVisualStyleBackColor = true;
            this.rbDriverType_LibUsb.CheckedChanged += new System.EventHandler(this.rbDriverType_LibUsb_CheckedChanged);
            // 
            // pgDeviceConfiguration
            // 
            this.pgDeviceConfiguration.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pgDeviceConfiguration.Location = new System.Drawing.Point(3, 119);
            this.pgDeviceConfiguration.Name = "pgDeviceConfiguration";
            this.pgDeviceConfiguration.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.pgDeviceConfiguration.Size = new System.Drawing.Size(631, 290);
            this.pgDeviceConfiguration.TabIndex = 2;
            this.pgDeviceConfiguration.PropertySortChanged += new System.EventHandler(this.pgDeviceConfiguration_PropertySortChanged);
            this.pgDeviceConfiguration.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgDeviceConfiguration_PropertyValueChanged);
            // 
            // headerDeviceConfiguration
            // 
            this.headerDeviceConfiguration.BackColor = System.Drawing.SystemColors.Control;
            this.headerDeviceConfiguration.CausesValidation = false;
            this.headerDeviceConfiguration.Description = "DEVICE_CONFIGURATION_DESCRIPTION$";
            this.headerDeviceConfiguration.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerDeviceConfiguration.Image = ((System.Drawing.Image)(resources.GetObject("headerDeviceConfiguration.Image")));
            this.headerDeviceConfiguration.Location = new System.Drawing.Point(0, 0);
            this.headerDeviceConfiguration.Name = "headerDeviceConfiguration";
            this.headerDeviceConfiguration.Size = new System.Drawing.Size(637, 61);
            this.headerDeviceConfiguration.TabIndex = 1;
            this.headerDeviceConfiguration.Title = "$DEVICE_CONFIGURATION_TITLE$";
            // 
            // wizDeviceSelection
            // 
            this.wizDeviceSelection.Controls.Add(this.gbDeviceSelection);
            this.wizDeviceSelection.Controls.Add(this.lvDeviceSelection);
            this.wizDeviceSelection.Controls.Add(this.headerDeviceSelection);
            this.wizDeviceSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizDeviceSelection.IsFinishPage = false;
            this.wizDeviceSelection.Location = new System.Drawing.Point(0, 0);
            this.wizDeviceSelection.Name = "wizDeviceSelection";
            this.wizDeviceSelection.Size = new System.Drawing.Size(637, 412);
            this.wizDeviceSelection.TabIndex = 3;
            this.wizDeviceSelection.Visible = false;
            this.wizDeviceSelection.ShowFromNext += new System.EventHandler(this.wizDeviceSelection_ShowFromNext);
            // 
            // gbDeviceSelection
            // 
            this.gbDeviceSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDeviceSelection.Controls.Add(this.cmdRemoveDevice);
            this.gbDeviceSelection.Controls.Add(this.rbDeviceSelection_AllDevices);
            this.gbDeviceSelection.Controls.Add(this.rbDeviceSelection_OnlyConnected);
            this.gbDeviceSelection.Location = new System.Drawing.Point(3, 67);
            this.gbDeviceSelection.Name = "gbDeviceSelection";
            this.gbDeviceSelection.Size = new System.Drawing.Size(630, 49);
            this.gbDeviceSelection.TabIndex = 3;
            this.gbDeviceSelection.TabStop = false;
            this.gbDeviceSelection.Text = "Show Devices";
            this.gbDeviceSelection.Enter += new System.EventHandler(this.gbDeviceSelection_Enter);
            // 
            // cmdRemoveDevice
            // 
            this.cmdRemoveDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdRemoveDevice.Enabled = false;
            this.cmdRemoveDevice.Location = new System.Drawing.Point(497, 17);
            this.cmdRemoveDevice.Name = "cmdRemoveDevice";
            this.cmdRemoveDevice.Size = new System.Drawing.Size(127, 23);
            this.cmdRemoveDevice.TabIndex = 2;
            this.cmdRemoveDevice.Text = "Remove Device";
            this.cmdRemoveDevice.UseVisualStyleBackColor = true;
            this.cmdRemoveDevice.Click += new System.EventHandler(this.cmdRemoveDevice_Click);
            // 
            // rbDeviceSelection_AllDevices
            // 
            this.rbDeviceSelection_AllDevices.AutoSize = true;
            this.rbDeviceSelection_AllDevices.Location = new System.Drawing.Point(157, 20);
            this.rbDeviceSelection_AllDevices.Name = "rbDeviceSelection_AllDevices";
            this.rbDeviceSelection_AllDevices.Size = new System.Drawing.Size(111, 17);
            this.rbDeviceSelection_AllDevices.TabIndex = 1;
            this.rbDeviceSelection_AllDevices.Text = "All Known Devices";
            this.rbDeviceSelection_AllDevices.UseVisualStyleBackColor = true;
            this.rbDeviceSelection_AllDevices.CheckedChanged += new System.EventHandler(this.rbDeviceSelection_AllDevices_CheckedChanged);
            // 
            // rbDeviceSelection_OnlyConnected
            // 
            this.rbDeviceSelection_OnlyConnected.AutoSize = true;
            this.rbDeviceSelection_OnlyConnected.Checked = true;
            this.rbDeviceSelection_OnlyConnected.Location = new System.Drawing.Point(9, 20);
            this.rbDeviceSelection_OnlyConnected.Name = "rbDeviceSelection_OnlyConnected";
            this.rbDeviceSelection_OnlyConnected.Size = new System.Drawing.Size(142, 17);
            this.rbDeviceSelection_OnlyConnected.TabIndex = 0;
            this.rbDeviceSelection_OnlyConnected.TabStop = true;
            this.rbDeviceSelection_OnlyConnected.Text = "Only Connected Devices";
            this.rbDeviceSelection_OnlyConnected.UseVisualStyleBackColor = true;
            this.rbDeviceSelection_OnlyConnected.CheckedChanged += new System.EventHandler(this.rbDeviceSelection_OnlyConnected_CheckedChanged);
            // 
            // lvDeviceSelection
            // 
            this.lvDeviceSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lvDeviceSelection.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colVid,
            this.colPid,
            this.colDescr,
            this.colMfu});
            this.lvDeviceSelection.FullRowSelect = true;
            this.lvDeviceSelection.GridLines = true;
            this.lvDeviceSelection.HideSelection = false;
            this.lvDeviceSelection.Location = new System.Drawing.Point(0, 122);
            this.lvDeviceSelection.Name = "lvDeviceSelection";
            this.lvDeviceSelection.Size = new System.Drawing.Size(636, 287);
            this.lvDeviceSelection.TabIndex = 1;
            this.lvDeviceSelection.UseCompatibleStateImageBehavior = false;
            this.lvDeviceSelection.View = System.Windows.Forms.View.Details;
            this.lvDeviceSelection.SelectedIndexChanged += new System.EventHandler(this.lvDeviceSelection_SelectedIndexChanged);
            // 
            // colVid
            // 
            this.colVid.Text = "Vendor ID";
            this.colVid.Width = 74;
            // 
            // colPid
            // 
            this.colPid.Text = "Product ID";
            this.colPid.Width = 77;
            // 
            // colDescr
            // 
            this.colDescr.Text = "Description";
            this.colDescr.Width = 177;
            // 
            // colMfu
            // 
            this.colMfu.Text = "Manufacturer";
            this.colMfu.Width = 150;
            // 
            // headerDeviceSelection
            // 
            this.headerDeviceSelection.BackColor = System.Drawing.SystemColors.Control;
            this.headerDeviceSelection.CausesValidation = false;
            this.headerDeviceSelection.Description = "$DEVICE_SELECTION_DESCRIPTION$";
            this.headerDeviceSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerDeviceSelection.Image = ((System.Drawing.Image)(resources.GetObject("headerDeviceSelection.Image")));
            this.headerDeviceSelection.Location = new System.Drawing.Point(0, 0);
            this.headerDeviceSelection.Name = "headerDeviceSelection";
            this.headerDeviceSelection.Size = new System.Drawing.Size(637, 61);
            this.headerDeviceSelection.TabIndex = 0;
            this.headerDeviceSelection.Title = "$DEVICE_SELECTION_TITLE$";
            // 
            // infoPageMain
            // 
            this.infoPageMain.BackColor = System.Drawing.Color.White;
            this.infoPageMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoPageMain.Image = ((System.Drawing.Image)(resources.GetObject("infoPageMain.Image")));
            this.infoPageMain.Location = new System.Drawing.Point(0, 0);
            this.infoPageMain.Name = "infoPageMain";
            this.infoPageMain.PageText = "This wizard enables you to...";
            this.infoPageMain.PageTitle = "Welcome to the / Completing the <Title> Wizard";
            this.infoPageMain.Size = new System.Drawing.Size(637, 460);
            this.infoPageMain.TabIndex = 1;
            // 
            // fUsbInfWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(637, 460);
            this.Controls.Add(this.wizMain);
            this.Name = "fUsbInfWizard";
            this.Text = "USB Inf Wizard";
            this.Load += new System.EventHandler(this.fUsbInfWizard_Load);
            this.wizMain.ResumeLayout(false);
            this.wizPageMain.ResumeLayout(false);
            this.infoMain.ResumeLayout(false);
            this.infoMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.wizDeviceConfiguration.ResumeLayout(false);
            this.gbDeviceConfiguration_DriverType.ResumeLayout(false);
            this.gbDeviceConfiguration_DriverType.PerformLayout();
            this.wizDeviceSelection.ResumeLayout(false);
            this.gbDeviceSelection.ResumeLayout(false);
            this.gbDeviceSelection.PerformLayout();
            this.ResumeLayout(false);

        }



        private Gui.Wizard.Wizard wizMain;
        private Gui.Wizard.WizardPage wizPageMain;
        private Gui.Wizard.InfoContainer infoMain;
        private System.Windows.Forms.Label lMainText;
        private System.Windows.Forms.PictureBox pictureBox4;
        private Gui.Wizard.WizardPage wizDeviceSelection;
        private Gui.Wizard.InfoPage infoPageMain;
        private Gui.Wizard.Header headerDeviceSelection;
        private System.Windows.Forms.ListView lvDeviceSelection;
        private System.Windows.Forms.ColumnHeader colVid;
        private System.Windows.Forms.ColumnHeader colPid;
        private System.Windows.Forms.ColumnHeader colDescr;
        private System.Windows.Forms.ColumnHeader colMfu;
        private Gui.Wizard.WizardPage wizDeviceConfiguration;
        private Gui.Wizard.Header headerDeviceConfiguration;
        private System.Windows.Forms.GroupBox gbDeviceSelection;
        private System.Windows.Forms.RadioButton rbDeviceSelection_AllDevices;
        private System.Windows.Forms.RadioButton rbDeviceSelection_OnlyConnected;
        private System.Windows.Forms.PropertyGrid pgDeviceConfiguration;
        private System.Windows.Forms.GroupBox gbDeviceConfiguration_DriverType;
        private System.Windows.Forms.RadioButton rbDriverType_WinUsb;
        private System.Windows.Forms.RadioButton rbDriverType_LibUsb;
        private System.Windows.Forms.Button cmdRemoveDevice;
        private System.Windows.Forms.Button cmdFindFrivers;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button cmdSaveProfile;
        private System.Windows.Forms.Button cmdLoadProfile;
        private System.Windows.Forms.Label lMainCautionTest;
        private System.Windows.Forms.PictureBox pictureBox1;

    }
}

