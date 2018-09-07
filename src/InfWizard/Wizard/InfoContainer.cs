// Copyright © 2009 Travis Robinson. All rights reserved.
// 
// website: http://sourceforge.net/projects/libusbdotnet
// e-mail:  trobinso@users.sourceforge.net
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or 
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
// for more details.
// 
// You should have received a copy of the GNU General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA. or 
// visit www.gnu.org.
// 
// 
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace Gui.Wizard
{
    /// <summary>
    /// Summary description for UserControl1.
    /// </summary>
    [Designer(typeof (InfoContainerDesigner))]
    public class InfoContainer : UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        protected readonly Container components;

        private Label lblTitle;
        private PictureBox picImage;

        /// <summary>
        /// 
        /// </summary>
        public InfoContainer()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        /// <summary>
        /// Get/Set the title for the info page
        /// </summary>
        [Category("Appearance")]
        public string PageTitle
        {
            get
            {
                return lblTitle.Text;
            }
            set
            {
                lblTitle.Text = value;
            }
        }


        /// <summary>
        /// Gets/Sets the Icon
        /// </summary>
        [Category("Appearance")]
        public Image Image
        {
            get
            {
                return picImage.Image;
            }
            set
            {
                picImage.Image = value;
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }


        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ResourceManager resources = new ResourceManager(typeof (InfoContainer));
            picImage = new PictureBox();
            lblTitle = new Label();
            SuspendLayout();
            // 
            // picImage
            // 
            picImage.Dock = DockStyle.Left;
            picImage.Image = ((Image) (resources.GetObject("picImage.Image")));
            picImage.Location = new Point(0, 0);
            picImage.Name = "picImage";
            picImage.Size = new Size(164, 388);
            picImage.TabIndex = 0;
            picImage.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.Anchor = ((((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
            lblTitle.FlatStyle = FlatStyle.System;
            lblTitle.Font = new Font("Tahoma", 12F, FontStyle.Bold, GraphicsUnit.Point, ((0)));
            lblTitle.Location = new Point(172, 4);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(304, 48);
            lblTitle.TabIndex = 7;
            lblTitle.Text = "Welcome to the / Completing the <Title> Wizard";
            // 
            // InfoContainer
            // 
            BackColor = Color.White;
            Controls.Add(lblTitle);
            Controls.Add(picImage);
            Name = "InfoContainer";
            Size = new Size(480, 388);
            Load += InfoContainer_Load;
            ResumeLayout(false);
        }


        private void InfoContainer_Load(object sender, EventArgs e)
        {
            //Handle really irating resize that doesn't take account of Anchor
            lblTitle.Left = picImage.Width + 8;
            lblTitle.Width = (Width - 4) - lblTitle.Left;
        }
    }
}