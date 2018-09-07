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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Gui.Wizard
{
    /// <summary>
    /// An inherited <see cref="InfoContainer"/> that contains a <see cref="Label"/> 
    /// with the description of the page.
    /// </summary>
    public class InfoPage : InfoContainer
    {
        private Label lblDescription;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public InfoPage()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
        }

        /// <summary>
        /// Gets/Sets the text on the info page
        /// </summary>
        [Category("Appearance")]
        public string PageText
        {
            get
            {
                return lblDescription.Text;
            }
            set
            {
                lblDescription.Text = value;
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
            lblDescription = new Label();
            SuspendLayout();
            // 
            // lblDescription
            // 
            lblDescription.Anchor = (((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
            lblDescription.FlatStyle = FlatStyle.System;
            lblDescription.Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            lblDescription.Location = new Point(172, 56);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(304, 328);
            lblDescription.TabIndex = 8;
            lblDescription.Text = "This wizard enables you to...";
            // 
            // InfoPage
            // 
            Controls.Add(lblDescription);
            Name = "InfoPage";
            Controls.SetChildIndex(lblDescription, 0);
            ResumeLayout(false);
        }
    }
}