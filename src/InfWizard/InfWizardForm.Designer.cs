namespace InfWizard
{
    partial class InfWizardForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InfWizardForm));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.folderBrowserDriverResources = new System.Windows.Forms.FolderBrowserDialog();
            this.wizMain = new Gui.Wizard.Wizard();
            this.wizardPageSelectDevice = new Gui.Wizard.WizardPage();
            this.deviceSelection_RefreshButton = new System.Windows.Forms.Button();
            this.deviceSelectionGroupBox = new System.Windows.Forms.GroupBox();
            this.deviceSelection_DriverlessOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.deviceSelection_ConnectedOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.deviceSelection_CreateNewRadio = new System.Windows.Forms.RadioButton();
            this.gridDeviceSelection = new System.Windows.Forms.DataGridView();
            this.colVid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colManufacturer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDriverless = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeviceId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cmdRemoveDevice = new System.Windows.Forms.Button();
            this.headerDeviceSelection = new Gui.Wizard.Header();
            this.wizardPageGetDrivers = new Gui.Wizard.WizardPage();
            this.linkToLudnDriverResources = new System.Windows.Forms.LinkLabel();
            this.groupBoxDownloadStatus = new System.Windows.Forms.GroupBox();
            this.rtfDownloadSatus = new InfWizard.RtfSatusControl();
            this.buttonCancelDownload = new System.Windows.Forms.Button();
            this.progressBarDownloadDriverResources = new System.Windows.Forms.ProgressBar();
            this.groupBoxDriverList = new System.Windows.Forms.GroupBox();
            this.dataGridViewDriverDownloadList = new System.Windows.Forms.DataGridView();
            this.coDriverResourceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDriverResourceDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonDownloadDriverResources = new System.Windows.Forms.Button();
            this.buttonSelectNoneDriverResources = new System.Windows.Forms.Button();
            this.buttonSelectAllDriverResources = new System.Windows.Forms.Button();
            this.radioButtonDownloadDrivers = new System.Windows.Forms.RadioButton();
            this.headerGetDrivers = new Gui.Wizard.Header();
            this.wizardPageWelcome = new Gui.Wizard.WizardPage();
            this.infoMain = new Gui.Wizard.InfoContainer();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.lMainText = new System.Windows.Forms.Label();
            this.rtfWelcomeStatus = new InfWizard.RtfSatusControl();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lMainCautionTest = new System.Windows.Forms.Label();
            this.linkLoadProfile = new System.Windows.Forms.LinkLabel();
            this.linkJumpToDownloadDriverResources = new System.Windows.Forms.LinkLabel();
            this.wizardPageFinished = new Gui.Wizard.WizardPage();
            this.groupBoxInstallDriver = new System.Windows.Forms.GroupBox();
            this.pictureBoxInstallDriver = new System.Windows.Forms.PictureBox();
            this.labelInstallDriver = new System.Windows.Forms.Label();
            this.buttonInstallDriver = new System.Windows.Forms.Button();
            this.groupBoxStatus = new System.Windows.Forms.GroupBox();
            this.rtfFinishedSatus = new InfWizard.RtfSatusControl();
            this.headerFinished = new Gui.Wizard.Header();
            this.wizardPageConfigDevice = new Gui.Wizard.WizardPage();
            this.buttonSaveProfile = new System.Windows.Forms.Button();
            this.gbDeviceConfiguration_DriverType = new System.Windows.Forms.GroupBox();
            this.comboBoxDriverResource = new System.Windows.Forms.ComboBox();
            this.DeviceConfigGrid = new System.Windows.Forms.PropertyGrid();
            this.headerDeviceConfiguration = new Gui.Wizard.Header();
            this.infoPageMain = new Gui.Wizard.InfoPage();
            this.wizMain.SuspendLayout();
            this.wizardPageSelectDevice.SuspendLayout();
            this.deviceSelectionGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridDeviceSelection)).BeginInit();
            this.wizardPageGetDrivers.SuspendLayout();
            this.groupBoxDownloadStatus.SuspendLayout();
            this.groupBoxDriverList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDriverDownloadList)).BeginInit();
            this.wizardPageWelcome.SuspendLayout();
            this.infoMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.wizardPageFinished.SuspendLayout();
            this.groupBoxInstallDriver.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxInstallDriver)).BeginInit();
            this.groupBoxStatus.SuspendLayout();
            this.wizardPageConfigDevice.SuspendLayout();
            this.gbDeviceConfiguration_DriverType.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizMain
            // 
            this.wizMain.Controls.Add(this.wizardPageFinished);
            this.wizMain.Controls.Add(this.wizardPageConfigDevice);
            this.wizMain.Controls.Add(this.wizardPageSelectDevice);
            this.wizMain.Controls.Add(this.wizardPageGetDrivers);
            this.wizMain.Controls.Add(this.wizardPageWelcome);
            this.wizMain.Controls.Add(this.infoPageMain);
            this.wizMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizMain.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wizMain.Location = new System.Drawing.Point(0, 0);
            this.wizMain.Name = "wizMain";
            this.wizMain.Pages.AddRange(new Gui.Wizard.WizardPage[] {
            this.wizardPageWelcome,
            this.wizardPageGetDrivers,
            this.wizardPageSelectDevice,
            this.wizardPageConfigDevice,
            this.wizardPageFinished});
            this.wizMain.Size = new System.Drawing.Size(504, 350);
            this.wizMain.TabIndex = 1;
            // 
            // wizardPageSelectDevice
            // 
            this.wizardPageSelectDevice.Controls.Add(this.deviceSelection_RefreshButton);
            this.wizardPageSelectDevice.Controls.Add(this.deviceSelectionGroupBox);
            this.wizardPageSelectDevice.Controls.Add(this.gridDeviceSelection);
            this.wizardPageSelectDevice.Controls.Add(this.cmdRemoveDevice);
            this.wizardPageSelectDevice.Controls.Add(this.headerDeviceSelection);
            this.wizardPageSelectDevice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageSelectDevice.IsFinishPage = false;
            this.wizardPageSelectDevice.Location = new System.Drawing.Point(0, 0);
            this.wizardPageSelectDevice.Name = "wizardPageSelectDevice";
            this.wizardPageSelectDevice.Size = new System.Drawing.Size(504, 302);
            this.wizardPageSelectDevice.TabIndex = 3;
            this.wizardPageSelectDevice.Visible = false;
            this.wizardPageSelectDevice.CloseFromBack += new Gui.Wizard.PageEventHandler(this.wizardPageSelectDevice_CloseFromBack);
            this.wizardPageSelectDevice.ShowFromNext += new System.EventHandler(this.wizardPageSelectDevice_ShowFromNext);
            // 
            // deviceSelection_RefreshButton
            // 
            this.deviceSelection_RefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceSelection_RefreshButton.Image = global::InfWizard.resInfWizard.RefreshImage;
            this.deviceSelection_RefreshButton.Location = new System.Drawing.Point(387, 93);
            this.deviceSelection_RefreshButton.Name = "deviceSelection_RefreshButton";
            this.deviceSelection_RefreshButton.Size = new System.Drawing.Size(113, 23);
            this.deviceSelection_RefreshButton.TabIndex = 8;
            this.deviceSelection_RefreshButton.Text = "Refresh List";
            this.deviceSelection_RefreshButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.deviceSelection_RefreshButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.deviceSelection_RefreshButton, "Refresh the device list view.");
            this.deviceSelection_RefreshButton.UseVisualStyleBackColor = true;
            this.deviceSelection_RefreshButton.Click += new System.EventHandler(this.deviceSelection_RefreshButton_Click);
            // 
            // deviceSelectionGroupBox
            // 
            this.deviceSelectionGroupBox.Controls.Add(this.deviceSelection_DriverlessOnlyCheckBox);
            this.deviceSelectionGroupBox.Controls.Add(this.deviceSelection_ConnectedOnlyCheckBox);
            this.deviceSelectionGroupBox.Controls.Add(this.deviceSelection_CreateNewRadio);
            this.deviceSelectionGroupBox.Location = new System.Drawing.Point(3, 67);
            this.deviceSelectionGroupBox.Name = "deviceSelectionGroupBox";
            this.deviceSelectionGroupBox.Size = new System.Drawing.Size(263, 49);
            this.deviceSelectionGroupBox.TabIndex = 7;
            this.deviceSelectionGroupBox.TabStop = false;
            this.deviceSelectionGroupBox.Text = "Selection Filter";
            // 
            // deviceSelection_DriverlessOnlyCheckBox
            // 
            this.deviceSelection_DriverlessOnlyCheckBox.AutoSize = true;
            this.deviceSelection_DriverlessOnlyCheckBox.Checked = true;
            this.deviceSelection_DriverlessOnlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.deviceSelection_DriverlessOnlyCheckBox.Location = new System.Drawing.Point(93, 21);
            this.deviceSelection_DriverlessOnlyCheckBox.Name = "deviceSelection_DriverlessOnlyCheckBox";
            this.deviceSelection_DriverlessOnlyCheckBox.Size = new System.Drawing.Size(73, 17);
            this.deviceSelection_DriverlessOnlyCheckBox.TabIndex = 7;
            this.deviceSelection_DriverlessOnlyCheckBox.Text = "Driverless";
            this.toolTip1.SetToolTip(this.deviceSelection_DriverlessOnlyCheckBox, "Show only usb devices that do not have a driver.");
            this.deviceSelection_DriverlessOnlyCheckBox.UseVisualStyleBackColor = true;
            this.deviceSelection_DriverlessOnlyCheckBox.CheckedChanged += new System.EventHandler(this.deviceSelection_DriverlessOnlyCheckBox_CheckedChanged);
            // 
            // deviceSelection_ConnectedOnlyCheckBox
            // 
            this.deviceSelection_ConnectedOnlyCheckBox.AutoSize = true;
            this.deviceSelection_ConnectedOnlyCheckBox.Checked = true;
            this.deviceSelection_ConnectedOnlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.deviceSelection_ConnectedOnlyCheckBox.Location = new System.Drawing.Point(9, 21);
            this.deviceSelection_ConnectedOnlyCheckBox.Name = "deviceSelection_ConnectedOnlyCheckBox";
            this.deviceSelection_ConnectedOnlyCheckBox.Size = new System.Drawing.Size(78, 17);
            this.deviceSelection_ConnectedOnlyCheckBox.TabIndex = 6;
            this.deviceSelection_ConnectedOnlyCheckBox.Text = "Connected";
            this.toolTip1.SetToolTip(this.deviceSelection_ConnectedOnlyCheckBox, "Show only usb devices that are currently connected to the PC.");
            this.deviceSelection_ConnectedOnlyCheckBox.UseVisualStyleBackColor = true;
            this.deviceSelection_ConnectedOnlyCheckBox.CheckedChanged += new System.EventHandler(this.deviceSelection_ConnectedOnlyCheckBox_CheckedChanged);
            // 
            // deviceSelection_CreateNewRadio
            // 
            this.deviceSelection_CreateNewRadio.AutoSize = true;
            this.deviceSelection_CreateNewRadio.Location = new System.Drawing.Point(172, 21);
            this.deviceSelection_CreateNewRadio.Name = "deviceSelection_CreateNewRadio";
            this.deviceSelection_CreateNewRadio.Size = new System.Drawing.Size(82, 17);
            this.deviceSelection_CreateNewRadio.TabIndex = 5;
            this.deviceSelection_CreateNewRadio.Text = "Create New";
            this.toolTip1.SetToolTip(this.deviceSelection_CreateNewRadio, "Create a new setup package from scratch.");
            this.deviceSelection_CreateNewRadio.UseVisualStyleBackColor = true;
            this.deviceSelection_CreateNewRadio.CheckedChanged += new System.EventHandler(this.deviceSelection_CreateNewRadio_CheckedChanged);
            // 
            // gridDeviceSelection
            // 
            this.gridDeviceSelection.AllowUserToAddRows = false;
            this.gridDeviceSelection.AllowUserToDeleteRows = false;
            this.gridDeviceSelection.AllowUserToResizeRows = false;
            this.gridDeviceSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gridDeviceSelection.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridDeviceSelection.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colVid,
            this.colPid,
            this.colDescription,
            this.colManufacturer,
            this.colDriverless,
            this.colDeviceId});
            this.gridDeviceSelection.Location = new System.Drawing.Point(3, 122);
            this.gridDeviceSelection.MultiSelect = false;
            this.gridDeviceSelection.Name = "gridDeviceSelection";
            this.gridDeviceSelection.RowHeadersVisible = false;
            this.gridDeviceSelection.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridDeviceSelection.Size = new System.Drawing.Size(497, 148);
            this.gridDeviceSelection.TabIndex = 4;
            this.gridDeviceSelection.SelectionChanged += new System.EventHandler(this.gridDeviceSelection_SelectionChanged);
            // 
            // colVid
            // 
            this.colVid.HeaderText = "Vendor ID";
            this.colVid.MinimumWidth = 90;
            this.colVid.Name = "colVid";
            this.colVid.ReadOnly = true;
            this.colVid.Width = 90;
            // 
            // colPid
            // 
            this.colPid.HeaderText = "Product ID";
            this.colPid.MinimumWidth = 90;
            this.colPid.Name = "colPid";
            this.colPid.ReadOnly = true;
            this.colPid.Width = 90;
            // 
            // colDescription
            // 
            this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDescription.HeaderText = "Description";
            this.colDescription.MinimumWidth = 100;
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            // 
            // colManufacturer
            // 
            this.colManufacturer.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colManufacturer.HeaderText = "Manufacturer";
            this.colManufacturer.MinimumWidth = 100;
            this.colManufacturer.Name = "colManufacturer";
            this.colManufacturer.ReadOnly = true;
            // 
            // colDriverless
            // 
            this.colDriverless.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colDriverless.HeaderText = "Driverless";
            this.colDriverless.MinimumWidth = 76;
            this.colDriverless.Name = "colDriverless";
            this.colDriverless.ReadOnly = true;
            this.colDriverless.Width = 77;
            // 
            // colDeviceId
            // 
            this.colDeviceId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDeviceId.HeaderText = "Device ID";
            this.colDeviceId.MinimumWidth = 100;
            this.colDeviceId.Name = "colDeviceId";
            this.colDeviceId.ReadOnly = true;
            // 
            // cmdRemoveDevice
            // 
            this.cmdRemoveDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdRemoveDevice.Enabled = false;
            this.cmdRemoveDevice.Image = global::InfWizard.resInfWizard.StopImage;
            this.cmdRemoveDevice.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.cmdRemoveDevice.Location = new System.Drawing.Point(3, 276);
            this.cmdRemoveDevice.Name = "cmdRemoveDevice";
            this.cmdRemoveDevice.Size = new System.Drawing.Size(117, 23);
            this.cmdRemoveDevice.TabIndex = 2;
            this.cmdRemoveDevice.Text = "Remove Device..";
            this.cmdRemoveDevice.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.cmdRemoveDevice, "Show the remove device window. ");
            this.cmdRemoveDevice.UseVisualStyleBackColor = true;
            this.cmdRemoveDevice.Click += new System.EventHandler(this.cmdRemoveDevice_Click);
            // 
            // headerDeviceSelection
            // 
            this.headerDeviceSelection.BackColor = System.Drawing.SystemColors.Control;
            this.headerDeviceSelection.CausesValidation = false;
            this.headerDeviceSelection.Description = "Select a usb device. This device will be used as a template for the setup package" +
                ".  Alternatively, select the \'Create New\' option to start from scratch.";
            this.headerDeviceSelection.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerDeviceSelection.Image = global::InfWizard.resInfWizard.LudnImage;
            this.headerDeviceSelection.Location = new System.Drawing.Point(0, 0);
            this.headerDeviceSelection.Name = "headerDeviceSelection";
            this.headerDeviceSelection.Size = new System.Drawing.Size(504, 61);
            this.headerDeviceSelection.TabIndex = 0;
            this.headerDeviceSelection.Title = "Device Selection";
            // 
            // wizardPageGetDrivers
            // 
            this.wizardPageGetDrivers.Controls.Add(this.linkToLudnDriverResources);
            this.wizardPageGetDrivers.Controls.Add(this.groupBoxDownloadStatus);
            this.wizardPageGetDrivers.Controls.Add(this.groupBoxDriverList);
            this.wizardPageGetDrivers.Controls.Add(this.radioButtonDownloadDrivers);
            this.wizardPageGetDrivers.Controls.Add(this.headerGetDrivers);
            this.wizardPageGetDrivers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageGetDrivers.IsFinishPage = false;
            this.wizardPageGetDrivers.Location = new System.Drawing.Point(0, 0);
            this.wizardPageGetDrivers.Name = "wizardPageGetDrivers";
            this.wizardPageGetDrivers.Size = new System.Drawing.Size(504, 302);
            this.wizardPageGetDrivers.TabIndex = 6;
            this.wizardPageGetDrivers.ShowFromNext += new System.EventHandler(this.wizardPageGetDrivers_ShowFromNext);
            // 
            // linkToLudnDriverResources
            // 
            this.linkToLudnDriverResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkToLudnDriverResources.AutoSize = true;
            this.linkToLudnDriverResources.Location = new System.Drawing.Point(289, 74);
            this.linkToLudnDriverResources.Name = "linkToLudnDriverResources";
            this.linkToLudnDriverResources.Size = new System.Drawing.Size(203, 13);
            this.linkToLudnDriverResources.TabIndex = 7;
            this.linkToLudnDriverResources.TabStop = true;
            this.linkToLudnDriverResources.Text = "Alternative driver resource download link";
            this.toolTip1.SetToolTip(this.linkToLudnDriverResources, resources.GetString("linkToLudnDriverResources.ToolTip"));
            this.linkToLudnDriverResources.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkToLudnDriverResources_LinkClicked);
            // 
            // groupBoxDownloadStatus
            // 
            this.groupBoxDownloadStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDownloadStatus.Controls.Add(this.rtfDownloadSatus);
            this.groupBoxDownloadStatus.Controls.Add(this.buttonCancelDownload);
            this.groupBoxDownloadStatus.Controls.Add(this.progressBarDownloadDriverResources);
            this.groupBoxDownloadStatus.Location = new System.Drawing.Point(12, 210);
            this.groupBoxDownloadStatus.Name = "groupBoxDownloadStatus";
            this.groupBoxDownloadStatus.Size = new System.Drawing.Size(480, 79);
            this.groupBoxDownloadStatus.TabIndex = 6;
            this.groupBoxDownloadStatus.TabStop = false;
            this.groupBoxDownloadStatus.Text = "Download status";
            // 
            // rtfDownloadSatus
            // 
            this.rtfDownloadSatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtfDownloadSatus.Location = new System.Drawing.Point(6, 20);
            this.rtfDownloadSatus.LoggingEnabled = false;
            this.rtfDownloadSatus.Name = "rtfDownloadSatus";
            this.rtfDownloadSatus.Size = new System.Drawing.Size(468, 37);
            this.rtfDownloadSatus.StatusFilter = null;
            this.rtfDownloadSatus.TabIndex = 6;
            // 
            // buttonCancelDownload
            // 
            this.buttonCancelDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancelDownload.Enabled = false;
            this.buttonCancelDownload.Image = ((System.Drawing.Image)(resources.GetObject("buttonCancelDownload.Image")));
            this.buttonCancelDownload.Location = new System.Drawing.Point(457, 59);
            this.buttonCancelDownload.Name = "buttonCancelDownload";
            this.buttonCancelDownload.Size = new System.Drawing.Size(17, 17);
            this.buttonCancelDownload.TabIndex = 5;
            this.toolTip1.SetToolTip(this.buttonCancelDownload, "Abort downloading");
            this.buttonCancelDownload.UseVisualStyleBackColor = true;
            this.buttonCancelDownload.Click += new System.EventHandler(this.buttonCancelDownload_Click);
            // 
            // progressBarDownloadDriverResources
            // 
            this.progressBarDownloadDriverResources.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarDownloadDriverResources.Location = new System.Drawing.Point(6, 63);
            this.progressBarDownloadDriverResources.Name = "progressBarDownloadDriverResources";
            this.progressBarDownloadDriverResources.Size = new System.Drawing.Size(445, 10);
            this.progressBarDownloadDriverResources.TabIndex = 4;
            // 
            // groupBoxDriverList
            // 
            this.groupBoxDriverList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDriverList.Controls.Add(this.dataGridViewDriverDownloadList);
            this.groupBoxDriverList.Controls.Add(this.buttonDownloadDriverResources);
            this.groupBoxDriverList.Controls.Add(this.buttonSelectNoneDriverResources);
            this.groupBoxDriverList.Controls.Add(this.buttonSelectAllDriverResources);
            this.groupBoxDriverList.Enabled = false;
            this.groupBoxDriverList.Location = new System.Drawing.Point(12, 100);
            this.groupBoxDriverList.Name = "groupBoxDriverList";
            this.groupBoxDriverList.Size = new System.Drawing.Size(480, 104);
            this.groupBoxDriverList.TabIndex = 4;
            this.groupBoxDriverList.TabStop = false;
            this.groupBoxDriverList.Text = "Driver resources available for download";
            // 
            // dataGridViewDriverDownloadList
            // 
            this.dataGridViewDriverDownloadList.AllowUserToAddRows = false;
            this.dataGridViewDriverDownloadList.AllowUserToDeleteRows = false;
            this.dataGridViewDriverDownloadList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewDriverDownloadList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDriverDownloadList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.coDriverResourceName,
            this.colDriverResourceDescription});
            this.dataGridViewDriverDownloadList.Location = new System.Drawing.Point(6, 20);
            this.dataGridViewDriverDownloadList.Name = "dataGridViewDriverDownloadList";
            this.dataGridViewDriverDownloadList.RowHeadersVisible = false;
            this.dataGridViewDriverDownloadList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewDriverDownloadList.Size = new System.Drawing.Size(387, 75);
            this.dataGridViewDriverDownloadList.TabIndex = 6;
            this.dataGridViewDriverDownloadList.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridViewDriverDownloadList_CellFormatting);
            this.dataGridViewDriverDownloadList.SelectionChanged += new System.EventHandler(this.dataGridViewDriverDownloadList_SelectionChanged);
            // 
            // coDriverResourceName
            // 
            this.coDriverResourceName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.coDriverResourceName.HeaderText = "Name";
            this.coDriverResourceName.Name = "coDriverResourceName";
            this.coDriverResourceName.ReadOnly = true;
            // 
            // colDriverResourceDescription
            // 
            this.colDriverResourceDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDriverResourceDescription.FillWeight = 400F;
            this.colDriverResourceDescription.HeaderText = "Description";
            this.colDriverResourceDescription.Name = "colDriverResourceDescription";
            this.colDriverResourceDescription.ReadOnly = true;
            // 
            // buttonDownloadDriverResources
            // 
            this.buttonDownloadDriverResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownloadDriverResources.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDownloadDriverResources.Location = new System.Drawing.Point(399, 72);
            this.buttonDownloadDriverResources.Name = "buttonDownloadDriverResources";
            this.buttonDownloadDriverResources.Size = new System.Drawing.Size(75, 23);
            this.buttonDownloadDriverResources.TabIndex = 3;
            this.buttonDownloadDriverResources.Text = "Download";
            this.toolTip1.SetToolTip(this.buttonDownloadDriverResources, "Download selected driver resources");
            this.buttonDownloadDriverResources.UseVisualStyleBackColor = true;
            this.buttonDownloadDriverResources.Click += new System.EventHandler(this.buttonDownloadDriverResources_Click);
            // 
            // buttonSelectNoneDriverResources
            // 
            this.buttonSelectNoneDriverResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectNoneDriverResources.Location = new System.Drawing.Point(399, 46);
            this.buttonSelectNoneDriverResources.Name = "buttonSelectNoneDriverResources";
            this.buttonSelectNoneDriverResources.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectNoneDriverResources.TabIndex = 2;
            this.buttonSelectNoneDriverResources.Text = "Select None";
            this.buttonSelectNoneDriverResources.UseVisualStyleBackColor = true;
            this.buttonSelectNoneDriverResources.Click += new System.EventHandler(this.buttonSelectNoneDriverResources_Click);
            // 
            // buttonSelectAllDriverResources
            // 
            this.buttonSelectAllDriverResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectAllDriverResources.Location = new System.Drawing.Point(399, 20);
            this.buttonSelectAllDriverResources.Name = "buttonSelectAllDriverResources";
            this.buttonSelectAllDriverResources.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectAllDriverResources.TabIndex = 1;
            this.buttonSelectAllDriverResources.Text = "Select All";
            this.buttonSelectAllDriverResources.UseVisualStyleBackColor = true;
            this.buttonSelectAllDriverResources.Click += new System.EventHandler(this.buttonSelectAllDriverResources_Click);
            // 
            // radioButtonDownloadDrivers
            // 
            this.radioButtonDownloadDrivers.Image = global::InfWizard.resInfWizard.RefreshImage;
            this.radioButtonDownloadDrivers.Location = new System.Drawing.Point(12, 62);
            this.radioButtonDownloadDrivers.Name = "radioButtonDownloadDrivers";
            this.radioButtonDownloadDrivers.Size = new System.Drawing.Size(183, 36);
            this.radioButtonDownloadDrivers.TabIndex = 3;
            this.radioButtonDownloadDrivers.TabStop = true;
            this.radioButtonDownloadDrivers.Text = " Use InfWizard Downloader";
            this.radioButtonDownloadDrivers.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.radioButtonDownloadDrivers, "Click this option to get a list of available driver resources from the internet.");
            this.radioButtonDownloadDrivers.UseVisualStyleBackColor = true;
            this.radioButtonDownloadDrivers.Click += new System.EventHandler(this.radioButtonDownloadDrivers_Click);
            // 
            // headerGetDrivers
            // 
            this.headerGetDrivers.BackColor = System.Drawing.SystemColors.Control;
            this.headerGetDrivers.CausesValidation = false;
            this.headerGetDrivers.Description = "InfWizard requires driver resource(s) to build a setup package.  They can be down" +
                "loaded from the internet now. Alternatively, a manual download link is provided." +
                "";
            this.headerGetDrivers.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerGetDrivers.Image = global::InfWizard.resInfWizard.LudnImage;
            this.headerGetDrivers.Location = new System.Drawing.Point(0, 0);
            this.headerGetDrivers.Name = "headerGetDrivers";
            this.headerGetDrivers.Size = new System.Drawing.Size(504, 61);
            this.headerGetDrivers.TabIndex = 1;
            this.headerGetDrivers.Title = "Download Driver Packages";
            // 
            // wizardPageWelcome
            // 
            this.wizardPageWelcome.Controls.Add(this.infoMain);
            this.wizardPageWelcome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageWelcome.IsFinishPage = false;
            this.wizardPageWelcome.Location = new System.Drawing.Point(0, 0);
            this.wizardPageWelcome.Name = "wizardPageWelcome";
            this.wizardPageWelcome.Size = new System.Drawing.Size(504, 302);
            this.wizardPageWelcome.TabIndex = 2;
            this.wizardPageWelcome.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wizardPageWelcome_CloseFromNext);
            this.wizardPageWelcome.ShowFromBack += new System.EventHandler(this.wizardPageWelcome_ShowFromBack);
            // 
            // infoMain
            // 
            this.infoMain.BackColor = System.Drawing.Color.White;
            this.infoMain.Controls.Add(this.pictureBox4);
            this.infoMain.Controls.Add(this.lMainText);
            this.infoMain.Controls.Add(this.rtfWelcomeStatus);
            this.infoMain.Controls.Add(this.pictureBox1);
            this.infoMain.Controls.Add(this.lMainCautionTest);
            this.infoMain.Controls.Add(this.linkLoadProfile);
            this.infoMain.Controls.Add(this.linkJumpToDownloadDriverResources);
            this.infoMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoMain.Image = ((System.Drawing.Image)(resources.GetObject("infoMain.Image")));
            this.infoMain.Location = new System.Drawing.Point(0, 0);
            this.infoMain.Name = "infoMain";
            this.infoMain.PageTitle = "Welcome to the LibUsbDotNet USB/INF Wizard";
            this.infoMain.Size = new System.Drawing.Size(504, 302);
            this.infoMain.TabIndex = 0;
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = global::InfWizard.resInfWizard.LudnImage;
            this.pictureBox4.Location = new System.Drawing.Point(172, 61);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(16, 16);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox4.TabIndex = 13;
            this.pictureBox4.TabStop = false;
            // 
            // lMainText
            // 
            this.lMainText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lMainText.Location = new System.Drawing.Point(194, 61);
            this.lMainText.Name = "lMainText";
            this.lMainText.Size = new System.Drawing.Size(307, 80);
            this.lMainText.TabIndex = 14;
            this.lMainText.Text = resources.GetString("lMainText.Text");
            // 
            // rtfWelcomeStatus
            // 
            this.rtfWelcomeStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtfWelcomeStatus.BackColor = System.Drawing.SystemColors.Window;
            this.rtfWelcomeStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtfWelcomeStatus.Location = new System.Drawing.Point(172, 211);
            this.rtfWelcomeStatus.LoggingEnabled = true;
            this.rtfWelcomeStatus.Name = "rtfWelcomeStatus";
            this.rtfWelcomeStatus.Size = new System.Drawing.Size(329, 79);
            this.rtfWelcomeStatus.StatusFilter = null;
            this.rtfWelcomeStatus.TabIndex = 19;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::InfWizard.resInfWizard.YieldImage;
            this.pictureBox1.Location = new System.Drawing.Point(172, 141);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(16, 16);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 16;
            this.pictureBox1.TabStop = false;
            // 
            // lMainCautionTest
            // 
            this.lMainCautionTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lMainCautionTest.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lMainCautionTest.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lMainCautionTest.Location = new System.Drawing.Point(194, 141);
            this.lMainCautionTest.Name = "lMainCautionTest";
            this.lMainCautionTest.Size = new System.Drawing.Size(307, 33);
            this.lMainCautionTest.TabIndex = 17;
            this.lMainCautionTest.Text = "Use this wizard with caution! It\'s features include usb device removal and forced" +
                " driver installation.";
            // 
            // linkLoadProfile
            // 
            this.linkLoadProfile.AutoSize = true;
            this.linkLoadProfile.Image = global::InfWizard.resInfWizard.OpenFolderImage;
            this.linkLoadProfile.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.linkLoadProfile.Location = new System.Drawing.Point(169, 176);
            this.linkLoadProfile.Margin = new System.Windows.Forms.Padding(2);
            this.linkLoadProfile.Name = "linkLoadProfile";
            this.linkLoadProfile.Size = new System.Drawing.Size(92, 13);
            this.linkLoadProfile.TabIndex = 20;
            this.linkLoadProfile.TabStop = true;
            this.linkLoadProfile.Text = "       Load Profile..";
            this.toolTip1.SetToolTip(this.linkLoadProfile, "Loads a previously saved usb device profile and jumps  to the configuration page." +
                    "");
            this.linkLoadProfile.Click += new System.EventHandler(this.linkLoadProfile_Click);
            // 
            // linkJumpToDownloadDriverResources
            // 
            this.linkJumpToDownloadDriverResources.AutoSize = true;
            this.linkJumpToDownloadDriverResources.Image = global::InfWizard.resInfWizard.RightArrowImage;
            this.linkJumpToDownloadDriverResources.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.linkJumpToDownloadDriverResources.Location = new System.Drawing.Point(169, 193);
            this.linkJumpToDownloadDriverResources.Margin = new System.Windows.Forms.Padding(2);
            this.linkJumpToDownloadDriverResources.Name = "linkJumpToDownloadDriverResources";
            this.linkJumpToDownloadDriverResources.Size = new System.Drawing.Size(173, 13);
            this.linkJumpToDownloadDriverResources.TabIndex = 21;
            this.linkJumpToDownloadDriverResources.TabStop = true;
            this.linkJumpToDownloadDriverResources.Text = "       Driver Resource Downloader..";
            this.toolTip1.SetToolTip(this.linkJumpToDownloadDriverResources, "Jump to the driver resource download wizard page.");
            this.linkJumpToDownloadDriverResources.Click += new System.EventHandler(this.linkJumpToDownloadDriverResources_Click);
            // 
            // wizardPageFinished
            // 
            this.wizardPageFinished.Controls.Add(this.groupBoxInstallDriver);
            this.wizardPageFinished.Controls.Add(this.groupBoxStatus);
            this.wizardPageFinished.Controls.Add(this.headerFinished);
            this.wizardPageFinished.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageFinished.IsFinishPage = false;
            this.wizardPageFinished.Location = new System.Drawing.Point(0, 0);
            this.wizardPageFinished.Name = "wizardPageFinished";
            this.wizardPageFinished.Size = new System.Drawing.Size(504, 302);
            this.wizardPageFinished.TabIndex = 5;
            this.wizardPageFinished.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wizardPageFinished_CloseFromNext);
            this.wizardPageFinished.CloseFromBack += new Gui.Wizard.PageEventHandler(this.wizardPageFinished_CloseFromBack);
            this.wizardPageFinished.ShowFromNext += new System.EventHandler(this.wizardPageFinished_ShowFromNext);
            // 
            // groupBoxInstallDriver
            // 
            this.groupBoxInstallDriver.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxInstallDriver.Controls.Add(this.pictureBoxInstallDriver);
            this.groupBoxInstallDriver.Controls.Add(this.labelInstallDriver);
            this.groupBoxInstallDriver.Controls.Add(this.buttonInstallDriver);
            this.groupBoxInstallDriver.Location = new System.Drawing.Point(3, 67);
            this.groupBoxInstallDriver.Name = "groupBoxInstallDriver";
            this.groupBoxInstallDriver.Size = new System.Drawing.Size(498, 58);
            this.groupBoxInstallDriver.TabIndex = 6;
            this.groupBoxInstallDriver.TabStop = false;
            this.groupBoxInstallDriver.Text = "Driver Installer";
            // 
            // pictureBoxInstallDriver
            // 
            this.pictureBoxInstallDriver.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxInstallDriver.Image")));
            this.pictureBoxInstallDriver.Location = new System.Drawing.Point(85, 30);
            this.pictureBoxInstallDriver.Name = "pictureBoxInstallDriver";
            this.pictureBoxInstallDriver.Size = new System.Drawing.Size(15, 15);
            this.pictureBoxInstallDriver.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxInstallDriver.TabIndex = 7;
            this.pictureBoxInstallDriver.TabStop = false;
            // 
            // labelInstallDriver
            // 
            this.labelInstallDriver.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInstallDriver.Location = new System.Drawing.Point(106, 17);
            this.labelInstallDriver.Name = "labelInstallDriver";
            this.labelInstallDriver.Size = new System.Drawing.Size(376, 38);
            this.labelInstallDriver.TabIndex = 6;
            this.labelInstallDriver.Text = "Please wait..";
            this.labelInstallDriver.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonInstallDriver
            // 
            this.buttonInstallDriver.Location = new System.Drawing.Point(6, 25);
            this.buttonInstallDriver.Name = "buttonInstallDriver";
            this.buttonInstallDriver.Size = new System.Drawing.Size(75, 23);
            this.buttonInstallDriver.TabIndex = 5;
            this.buttonInstallDriver.Text = "Install Now";
            this.toolTip1.SetToolTip(this.buttonInstallDriver, resources.GetString("buttonInstallDriver.ToolTip"));
            this.buttonInstallDriver.UseVisualStyleBackColor = true;
            this.buttonInstallDriver.Click += new System.EventHandler(this.buttonInstallDriver_Click);
            // 
            // groupBoxStatus
            // 
            this.groupBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxStatus.Controls.Add(this.rtfFinishedSatus);
            this.groupBoxStatus.Location = new System.Drawing.Point(3, 131);
            this.groupBoxStatus.Name = "groupBoxStatus";
            this.groupBoxStatus.Size = new System.Drawing.Size(498, 168);
            this.groupBoxStatus.TabIndex = 4;
            this.groupBoxStatus.TabStop = false;
            this.groupBoxStatus.Text = "Status";
            // 
            // rtfFinishedSatus
            // 
            this.rtfFinishedSatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfFinishedSatus.Location = new System.Drawing.Point(3, 17);
            this.rtfFinishedSatus.LoggingEnabled = false;
            this.rtfFinishedSatus.Name = "rtfFinishedSatus";
            this.rtfFinishedSatus.Size = new System.Drawing.Size(492, 148);
            this.rtfFinishedSatus.StatusFilter = null;
            this.rtfFinishedSatus.TabIndex = 0;
            // 
            // headerFinished
            // 
            this.headerFinished.BackColor = System.Drawing.SystemColors.Control;
            this.headerFinished.CausesValidation = false;
            this.headerFinished.Description = "Optionally, install this driver now, or skip this step and install the setup pack" +
                "age normally. ";
            this.headerFinished.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerFinished.Image = global::InfWizard.resInfWizard.LudnImage;
            this.headerFinished.Location = new System.Drawing.Point(0, 0);
            this.headerFinished.Name = "headerFinished";
            this.headerFinished.Size = new System.Drawing.Size(504, 61);
            this.headerFinished.TabIndex = 2;
            this.headerFinished.Title = "Driver Package Finished";
            // 
            // wizardPageConfigDevice
            // 
            this.wizardPageConfigDevice.Controls.Add(this.buttonSaveProfile);
            this.wizardPageConfigDevice.Controls.Add(this.gbDeviceConfiguration_DriverType);
            this.wizardPageConfigDevice.Controls.Add(this.DeviceConfigGrid);
            this.wizardPageConfigDevice.Controls.Add(this.headerDeviceConfiguration);
            this.wizardPageConfigDevice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPageConfigDevice.IsFinishPage = false;
            this.wizardPageConfigDevice.Location = new System.Drawing.Point(0, 0);
            this.wizardPageConfigDevice.Name = "wizardPageConfigDevice";
            this.wizardPageConfigDevice.Size = new System.Drawing.Size(504, 302);
            this.wizardPageConfigDevice.TabIndex = 4;
            this.wizardPageConfigDevice.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wizardPageConfigDevice_CloseFromNext);
            this.wizardPageConfigDevice.CloseFromBack += new Gui.Wizard.PageEventHandler(this.wizardPageConfigDevice_CloseFromBack);
            this.wizardPageConfigDevice.ShowFromNext += new System.EventHandler(this.wizardPageConfigDevice_ShowFromNext);
            // 
            // buttonSaveProfile
            // 
            this.buttonSaveProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveProfile.Image = global::InfWizard.resInfWizard.SaveImage;
            this.buttonSaveProfile.Location = new System.Drawing.Point(416, 78);
            this.buttonSaveProfile.Name = "buttonSaveProfile";
            this.buttonSaveProfile.Size = new System.Drawing.Size(85, 34);
            this.buttonSaveProfile.TabIndex = 5;
            this.buttonSaveProfile.Text = "Save Profile..";
            this.buttonSaveProfile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.toolTip1.SetToolTip(this.buttonSaveProfile, "Locate the parent directory where the source drivers for the selected driver type" +
                    " can be found.");
            this.buttonSaveProfile.UseVisualStyleBackColor = true;
            this.buttonSaveProfile.Click += new System.EventHandler(this.buttonSaveProfile_Click);
            // 
            // gbDeviceConfiguration_DriverType
            // 
            this.gbDeviceConfiguration_DriverType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDeviceConfiguration_DriverType.Controls.Add(this.comboBoxDriverResource);
            this.gbDeviceConfiguration_DriverType.Location = new System.Drawing.Point(3, 67);
            this.gbDeviceConfiguration_DriverType.Name = "gbDeviceConfiguration_DriverType";
            this.gbDeviceConfiguration_DriverType.Size = new System.Drawing.Size(407, 46);
            this.gbDeviceConfiguration_DriverType.TabIndex = 3;
            this.gbDeviceConfiguration_DriverType.TabStop = false;
            this.gbDeviceConfiguration_DriverType.Text = "Select Driver Resource";
            // 
            // comboBoxDriverResource
            // 
            this.comboBoxDriverResource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDriverResource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDriverResource.FormattingEnabled = true;
            this.comboBoxDriverResource.Location = new System.Drawing.Point(9, 19);
            this.comboBoxDriverResource.Name = "comboBoxDriverResource";
            this.comboBoxDriverResource.Size = new System.Drawing.Size(392, 21);
            this.comboBoxDriverResource.TabIndex = 0;
            this.comboBoxDriverResource.SelectedIndexChanged += new System.EventHandler(this.comboBoxDriverResource_SelectedIndexChanged);
            // 
            // DeviceConfigGrid
            // 
            this.DeviceConfigGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.DeviceConfigGrid.Location = new System.Drawing.Point(3, 119);
            this.DeviceConfigGrid.Name = "DeviceConfigGrid";
            this.DeviceConfigGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.DeviceConfigGrid.Size = new System.Drawing.Size(498, 180);
            this.DeviceConfigGrid.TabIndex = 2;
            this.DeviceConfigGrid.PropertySortChanged += new System.EventHandler(this.DeviceConfigGrid_PropertySortChanged);
            this.DeviceConfigGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.DeviceConfigGrid_PropertyValueChanged);
            // 
            // headerDeviceConfiguration
            // 
            this.headerDeviceConfiguration.BackColor = System.Drawing.SystemColors.Control;
            this.headerDeviceConfiguration.CausesValidation = false;
            this.headerDeviceConfiguration.Description = "Choose the driver type && save location. Fine tune the device parameters.  The se" +
                "tup package will be written when Next is clicked.";
            this.headerDeviceConfiguration.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerDeviceConfiguration.Image = global::InfWizard.resInfWizard.LudnImage;
            this.headerDeviceConfiguration.Location = new System.Drawing.Point(0, 0);
            this.headerDeviceConfiguration.Name = "headerDeviceConfiguration";
            this.headerDeviceConfiguration.Size = new System.Drawing.Size(504, 61);
            this.headerDeviceConfiguration.TabIndex = 1;
            this.headerDeviceConfiguration.Title = "Device Configuration";
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
            this.infoPageMain.Size = new System.Drawing.Size(504, 350);
            this.infoPageMain.TabIndex = 1;
            // 
            // InfWizardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 350);
            this.Controls.Add(this.wizMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(512, 384);
            this.Name = "InfWizardForm";
            this.Text = "LibUsbDotNet USB/INF Wizard";
            this.Load += new System.EventHandler(this.InfWizardForm_Load);
            this.wizMain.ResumeLayout(false);
            this.wizardPageSelectDevice.ResumeLayout(false);
            this.deviceSelectionGroupBox.ResumeLayout(false);
            this.deviceSelectionGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridDeviceSelection)).EndInit();
            this.wizardPageGetDrivers.ResumeLayout(false);
            this.wizardPageGetDrivers.PerformLayout();
            this.groupBoxDownloadStatus.ResumeLayout(false);
            this.groupBoxDriverList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDriverDownloadList)).EndInit();
            this.wizardPageWelcome.ResumeLayout(false);
            this.infoMain.ResumeLayout(false);
            this.infoMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.wizardPageFinished.ResumeLayout(false);
            this.groupBoxInstallDriver.ResumeLayout(false);
            this.groupBoxInstallDriver.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxInstallDriver)).EndInit();
            this.groupBoxStatus.ResumeLayout(false);
            this.wizardPageConfigDevice.ResumeLayout(false);
            this.gbDeviceConfiguration_DriverType.ResumeLayout(false);
            this.ResumeLayout(false);

        }



        private Gui.Wizard.Wizard wizMain;
        private Gui.Wizard.WizardPage wizardPageWelcome;
        private Gui.Wizard.InfoContainer infoMain;
        private System.Windows.Forms.Label lMainText;
        private System.Windows.Forms.PictureBox pictureBox4;
        private Gui.Wizard.WizardPage wizardPageSelectDevice;
        private Gui.Wizard.InfoPage infoPageMain;
        private Gui.Wizard.Header headerDeviceSelection;
        private Gui.Wizard.WizardPage wizardPageConfigDevice;
        private Gui.Wizard.Header headerDeviceConfiguration;
        private System.Windows.Forms.PropertyGrid DeviceConfigGrid;
        private System.Windows.Forms.GroupBox gbDeviceConfiguration_DriverType;
        private System.Windows.Forms.Button cmdRemoveDevice;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button buttonSaveProfile;
        private System.Windows.Forms.Label lMainCautionTest;
        private System.Windows.Forms.PictureBox pictureBox1;
        private Gui.Wizard.WizardPage wizardPageFinished;
        private Gui.Wizard.Header headerFinished;
        private System.Windows.Forms.DataGridView gridDeviceSelection;
        private System.Windows.Forms.GroupBox groupBoxInstallDriver;
        private System.Windows.Forms.Button buttonInstallDriver;
        private System.Windows.Forms.GroupBox groupBoxStatus;
        private System.Windows.Forms.Label labelInstallDriver;
        private System.Windows.Forms.ComboBox comboBoxDriverResource;
        private Gui.Wizard.WizardPage wizardPageGetDrivers;
        private Gui.Wizard.Header headerGetDrivers;
        private System.Windows.Forms.GroupBox groupBoxDriverList;
        private System.Windows.Forms.Button buttonDownloadDriverResources;
        private System.Windows.Forms.Button buttonSelectNoneDriverResources;
        private System.Windows.Forms.Button buttonSelectAllDriverResources;
        private System.Windows.Forms.ProgressBar progressBarDownloadDriverResources;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDriverResources;
        private System.Windows.Forms.DataGridView dataGridViewDriverDownloadList;
        private System.Windows.Forms.DataGridViewTextBoxColumn coDriverResourceName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDriverResourceDescription;
        private System.Windows.Forms.GroupBox groupBoxDownloadStatus;
        private System.Windows.Forms.Button buttonCancelDownload;
        private System.Windows.Forms.PictureBox pictureBoxInstallDriver;
        private RtfSatusControl rtfDownloadSatus;
        private RtfSatusControl rtfFinishedSatus;
        private System.Windows.Forms.LinkLabel linkToLudnDriverResources;
        private System.Windows.Forms.RadioButton deviceSelection_CreateNewRadio;
        private System.Windows.Forms.RadioButton radioButtonDownloadDrivers;
        private RtfSatusControl rtfWelcomeStatus;
        private System.Windows.Forms.GroupBox deviceSelectionGroupBox;
        private System.Windows.Forms.CheckBox deviceSelection_ConnectedOnlyCheckBox;
        private System.Windows.Forms.CheckBox deviceSelection_DriverlessOnlyCheckBox;
        private System.Windows.Forms.Button deviceSelection_RefreshButton;
        private System.Windows.Forms.LinkLabel linkLoadProfile;
        private System.Windows.Forms.LinkLabel linkJumpToDownloadDriverResources;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colManufacturer;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDriverless;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeviceId;

    }
}

