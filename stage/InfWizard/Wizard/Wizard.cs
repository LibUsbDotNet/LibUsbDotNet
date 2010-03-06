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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Gui.Wizard
{
    /// <summary>
    /// A wizard is the control added to a form to provide a step by step functionality.
    /// It contains <see cref="WizardPage"/>s in the <see cref="Pages"/> collection, which
    /// are containers for other controls. Only one wizard page is shown at a time in the client
    /// are of the wizard.
    /// </summary>
    [Designer(typeof (WizardDesigner))]
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof (Wizard))]
    public class Wizard : UserControl
    {
        /// <summary>
        /// Called when the cancel button is pressed, before the form is closed. Set e.Cancel to true if 
        /// you do not wish the cancel to close the wizard.
        /// </summary>
        public event CancelEventHandler CloseFromCancel;

        protected internal Panel pnlButtons;
        private Panel pnlButtonBright3d;
        private Panel pnlButtonDark3d;
        protected internal Button btnBack;
        protected internal Button btnNext;
        private Button btnCancel;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        protected readonly Container components;


        private readonly PageCollection vPages;

        private WizardPage vActivePage;

        /// <summary>
        /// Wizard control with designer support
        /// </summary>
        public Wizard()
        {
            //Empty collection of Pages
            vPages = new PageCollection(this);

            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        /// <summary>
        /// Returns the collection of Pages in the wizard
        /// </summary>
        [Category("Wizard")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public PageCollection Pages
        {
            get
            {
                return vPages;
            }
        }

        /// <summary>
        /// Gets/Sets the activePage in the wizard
        /// </summary>
        [Category("Wizard")]
        internal int PageIndex
        {
            get
            {
                return vPages.IndexOf(vActivePage);
            }
            set
            {
                //Do I have any pages?
                if (vPages.Count == 0)
                {
                    //No then show nothing
                    ActivatePage(-1);
                    return;
                }
                // Validate the page asked for
                if (value < -1 || value >= vPages.Count)
                {
                    throw new ArgumentOutOfRangeException("PageIndex",
                                                          value,
                                                          "The page index must be between 0 and " + Convert.ToString(vPages.Count - 1));
                }
                //Select the new page
                ActivatePage(value);
            }
        }

        /// <summary>
        /// Alternative way of getting/Setiing  the current page by using wizardPage objects
        /// </summary>
        public WizardPage Page
        {
            get
            {
                return vActivePage;
            } //Dont use this anymore, see Next, Back, NextTo and BackTo
//			set
//			{
//				ActivatePage(value);
//			}
        }


        /// <summary>
        /// Gets/Sets the enabled state of the Next button. 
        /// </summary>
        [Category("Wizard")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool NextEnabled
        {
            get
            {
                return btnNext.Enabled;
            }
            set
            {
                btnNext.Enabled = value;
            }
        }

        /// <summary>
        /// Gets/Sets the enabled state of the back button. 
        /// </summary>
        [Category("Wizard")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool BackEnabled
        {
            get
            {
                return btnBack.Enabled;
            }
            set
            {
                btnBack.Enabled = value;
            }
        }

        /// <summary>
        /// Gets/Sets the enabled state of the cancel button. 
        /// </summary>
        [Category("Wizard")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CancelEnabled
        {
            get
            {
                return btnCancel.Enabled;
            }
            set
            {
                btnCancel.Enabled = value;
            }
        }


        /// <summary>
        /// Closes the current page after a <see cref="WizardPage.CloseFromBack"/>, then moves to 
        /// the previous page and calls <see cref="WizardPage.ShowFromBack"/>
        /// </summary>
        public void Back()
        {
            Debug.Assert(PageIndex < vPages.Count, "Page Index was beyond Maximum pages");
            //Can I press back
            Debug.Assert(PageIndex > 0 && PageIndex < vPages.Count, "Attempted to go back to a page that doesn't exist");
            //Tell the application that I closed a page
            int newPage = vActivePage.OnCloseFromBack(this);

            ActivatePage(newPage);
            //Tell the application I have shown a page
            vActivePage.OnShowFromBack(this);
        }

        /// <summary>
        /// Moves to the page given and calls <see cref="WizardPage.ShowFromBack"/>
        /// </summary>
        /// <remarks>Does NOT call <see cref="WizardPage.CloseFromBack"/> on the current page</remarks>
        /// <param name="page"></param>
        public void BackTo(WizardPage page)
        {
            //Since we have a page to go to, then there is no need to validate most of it
            ActivatePage(page);
            //Tell the application, I have just shown a page
            vActivePage.OnShowFromNext(this);
        }

        /// <summary>
        /// Closes the current page after a <see cref="WizardPage.CloseFromNext"/>, then moves to 
        /// the Next page and calls <see cref="WizardPage.ShowFromNext"/>
        /// </summary>
        public void Next()
        {
            Debug.Assert(PageIndex >= 0, "Page Index was below 0");

            PageEventArgs pageEventArgs = vActivePage.OnCloseFromNext(this);
            //Tell the Application I just closed a Page
            int newPageIndex = pageEventArgs.PageIndex;

            //Did I just press Finish instead of Next
            if (newPageIndex < vPages.Count && (vActivePage.IsFinishPage == false || DesignMode))
            {
                ActivatePage(newPageIndex);
                //No still going
                //Tell the application, I have just shown a page
                vActivePage.OnShowFromNext(this);
            }
            else
            {
                Debug.Assert(PageIndex < vPages.Count,
                             "Error I've just gone past the finish",
                             "btnNext_Click tried to go to page " + Convert.ToString(PageIndex + 1) + ", but I only have " +
                             Convert.ToString(vPages.Count));
                //yep Finish was pressed
                if (DesignMode == false && pageEventArgs.CloseInFinsh)
                    ParentForm.Close();
            }
        }

        /// <summary>
        /// Moves to the page given and calls <see cref="WizardPage.ShowFromNext"/>
        /// </summary>
        /// <remarks>Does NOT call <see cref="WizardPage.CloseFromNext"/> on the current page</remarks>
        /// <param name="page"></param>
        public void NextTo(WizardPage page)
        {
            //Since we have a page to go to, then there is no need to validate most of it
            ActivatePage(page);
            //Tell the application, I have just shown a page
            vActivePage.OnShowFromNext(this);
        }


#if DEBUG
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (DesignMode)
            {
                const string noPagesText = "No wizard pages inside the wizard.";


                SizeF textSize = e.Graphics.MeasureString(noPagesText, Font);
                RectangleF layout = new RectangleF((Width - textSize.Width)/2,
                                                   (pnlButtons.Top - textSize.Height)/2,
                                                   textSize.Width,
                                                   textSize.Height);

                Pen dashPen = (Pen) SystemPens.GrayText.Clone();
                dashPen.DashStyle = DashStyle.Dash;

                e.Graphics.DrawRectangle(dashPen,
                                         Left + 8,
                                         Top + 8,
                                         Width - 17,
                                         pnlButtons.Top - 17);

                e.Graphics.DrawString(noPagesText, Font, new SolidBrush(SystemColors.GrayText), layout);
            }
        }
#endif


#if DEBUG
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (DesignMode)
            {
                Invalidate();
            }
        }

#endif


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
            pnlButtons = new Panel();
            btnCancel = new Button();
            btnNext = new Button();
            btnBack = new Button();
            pnlButtonBright3d = new Panel();
            pnlButtonDark3d = new Panel();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnNext);
            pnlButtons.Controls.Add(btnBack);
            pnlButtons.Controls.Add(pnlButtonBright3d);
            pnlButtons.Controls.Add(pnlButtonDark3d);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 224);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(444, 48);
            pnlButtons.TabIndex = 0;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = (((AnchorStyles.Top | AnchorStyles.Right)));
            btnCancel.FlatStyle = FlatStyle.System;
            btnCancel.Location = new Point(356, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click;
            // 
            // btnNext
            // 
            btnNext.Anchor = (((AnchorStyles.Top | AnchorStyles.Right)));
            btnNext.FlatStyle = FlatStyle.System;
            btnNext.Location = new Point(272, 12);
            btnNext.Name = "btnNext";
            btnNext.TabIndex = 4;
            btnNext.Text = "&Next >";
            btnNext.Click += btnNext_Click;
            btnNext.MouseDown += btnNext_MouseDown;
            // 
            // btnBack
            // 
            btnBack.Anchor = (((AnchorStyles.Top | AnchorStyles.Right)));
            btnBack.FlatStyle = FlatStyle.System;
            btnBack.Location = new Point(196, 12);
            btnBack.Name = "btnBack";
            btnBack.TabIndex = 3;
            btnBack.Text = "< &Back";
            btnBack.Click += btnBack_Click;
            btnBack.MouseDown += btnBack_MouseDown;
            // 
            // pnlButtonBright3d
            // 
            pnlButtonBright3d.BackColor = SystemColors.ControlLightLight;
            pnlButtonBright3d.Dock = DockStyle.Top;
            pnlButtonBright3d.Location = new Point(0, 1);
            pnlButtonBright3d.Name = "pnlButtonBright3d";
            pnlButtonBright3d.Size = new Size(444, 1);
            pnlButtonBright3d.TabIndex = 1;
            // 
            // pnlButtonDark3d
            // 
            pnlButtonDark3d.BackColor = SystemColors.ControlDark;
            pnlButtonDark3d.Dock = DockStyle.Top;
            pnlButtonDark3d.Location = new Point(0, 0);
            pnlButtonDark3d.Name = "pnlButtonDark3d";
            pnlButtonDark3d.Size = new Size(444, 1);
            pnlButtonDark3d.TabIndex = 2;
            // 
            // Wizard
            // 
            Controls.Add(pnlButtons);
            Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, ((0)));
            Name = "Wizard";
            Size = new Size(444, 272);
            Load += Wizard_Load;
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }


        private void btnBack_Click(object sender, EventArgs e)
        {
            Back();
        }

        private void btnBack_MouseDown(object sender, MouseEventArgs e)
        {
            if (DesignMode)
                Back();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CancelEventArgs arg = new CancelEventArgs();

            //Throw the event out to subscribers
            if (CloseFromCancel != null)
            {
                CloseFromCancel(this, arg);
            }
            //If nobody told me to cancel
            if (arg.Cancel == false)
            {
                //Then we close the form
                FindForm().Close();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            Next();
        }

        private void btnNext_MouseDown(object sender, MouseEventArgs e)
        {
            if (DesignMode)
                Next();
        }

        private void Wizard_Load(object sender, EventArgs e)
        {
            //Attempt to activate a page
            ActivatePage(0);

            //Can I add my self as default cancel button
            Form form = FindForm();
            if (form != null && DesignMode == false)
                form.CancelButton = btnCancel;
        }


        protected internal void ActivatePage(int index)
        {
            //If the new page is invalid
            if (index < 0 || index >= vPages.Count)
            {
                btnNext.Enabled = false;
                btnBack.Enabled = false;

                return;
            }


            //Change to the new Page
            WizardPage tWizPage = (vPages[index]);

            //Really activate the page
            ActivatePage(tWizPage);
        }

        protected internal void ActivatePage(WizardPage page)
        {
            //Deactivate the current
            if (vActivePage != null)
            {
                vActivePage.Visible = false;
            }


            //Activate the new page
            vActivePage = page;

            if (vActivePage != null)
            {
                //Ensure that this panel displays inside the wizard
                vActivePage.Parent = this;
                if (Contains(vActivePage) == false)
                {
                    Container.Add(vActivePage);
                }
                //Make it fill the space
                vActivePage.Dock = DockStyle.Fill;
                vActivePage.Visible = true;
                vActivePage.BringToFront();
                vActivePage.FocusFirstTabIndex();
            }

            //What should the back button say
            if (PageIndex > 0)
            {
                btnBack.Enabled = true;
            }
            else
            {
                btnBack.Enabled = false;
            }

            //What should the Next button say
            if (vPages.IndexOf(vActivePage) < vPages.Count - 1 && vActivePage.IsFinishPage == false)
            {
                btnNext.Text = "&Next >";
                btnNext.Enabled = true;
                //Don't close the wizard :)
                btnNext.DialogResult = DialogResult.None;
            }
            else
            {
                btnNext.Text = "Fi&nish";
                //Dont allow a finish in designer
                if (DesignMode && vPages.IndexOf(vActivePage) == vPages.Count - 1)
                {
                    btnNext.Enabled = false;
                }
                else
                {
                    btnNext.Enabled = true;
                    //If Not in design mode then allow a close
                    btnNext.DialogResult = DialogResult.OK;
                }
            }

            //Cause a refresh
            if (vActivePage != null)
                vActivePage.Invalidate();
            else
                Invalidate();
        }
    }
}