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
using System.Windows.Forms;

namespace Gui.Wizard
{
    /// <summary>
    /// 
    /// </summary>
    [Designer(typeof (WizardPageDesigner))]
    public class WizardPage : Panel
    {
        private bool _IsFinishPage;

        [Category("Wizard")]
        public bool IsFinishPage
        {
            get
            {
                return _IsFinishPage;
            }
            set
            {
                _IsFinishPage = value;
            }
        }

        /// <summary>
        /// Event called before this page is closed when the back button is pressed. If you don't want to show pageIndex -1 then
        /// set page to be the new page that you wish to show
        /// </summary>
        public event PageEventHandler CloseFromBack;

        /// <summary>
        /// Event called before this page is closed when the next button is pressed. If you don't want to show pageIndex -1 then
        /// set page to be the new page that you wish to show 
        /// </summary>
        public event PageEventHandler CloseFromNext;

        /// <summary>
        /// Event called after this page is shown when the back button is pressed.
        /// </summary>
        public event EventHandler ShowFromBack;

        /// <summary>
        /// Event called after this page is shown when the next button is pressed. 
        /// </summary>
        public event EventHandler ShowFromNext;


        /// <summary>
        /// Set the focus to the contained control with a lowest tabIndex
        /// </summary>
        public void FocusFirstTabIndex()
        {
            //Activate the first control in the Panel
            Control found = null;
            //find the control with the lowest 
            foreach (Control control in Controls)
            {
                if (control.CanFocus && (found == null || control.TabIndex < found.TabIndex))
                {
                    found = control;
                }
            }
            //Have we actually found anything
            if (found != null)
            {
                //Focus the found control
                found.Focus();
            }
            else
            {
                //Just focus the wizard Page
                Focus();
            }
        }

        /// <summary>
        /// Fires the CloseFromBack Event
        /// </summary>
        /// <param name="wiz">Wizard to pass into the sender argument</param>
        /// <returns>Index of Page that the event handlers would like to see next</returns>
        public int OnCloseFromBack(Wizard wiz)
        {
            //Event args thinks that the next pgae will be the one before this one
            PageEventArgs e = new PageEventArgs(wiz.PageIndex - 1, wiz.Pages);
            //Tell anybody who listens
            if (CloseFromBack != null)
                CloseFromBack(wiz, e);
            //And then tell whomever called me what the prefered page is
            return e.PageIndex;
        }

        /// <summary>
        /// Fires the CloseFromNextEvent
        /// </summary>
        /// <param name="wiz">Sender</param>
        public PageEventArgs OnCloseFromNext(Wizard wiz)
        {
            //Event args thinks that the next pgae will be the one before this one
            PageEventArgs e = new PageEventArgs(wiz.PageIndex + 1, wiz.Pages);
            //Tell anybody who listens
            if (CloseFromNext != null)
                CloseFromNext(wiz, e);
            //And then tell whomever called me what the prefered page is
            return e;
        }

        /// <summary>
        /// Fires the showFromBack event
        /// </summary>
        /// <param name="wiz">sender</param>
        public void OnShowFromBack(Wizard wiz)
        {
            if (ShowFromBack != null)
                ShowFromBack(wiz, EventArgs.Empty);
        }

        /// <summary>
        /// Fires the showFromNext event
        /// </summary>
        /// <param name="wiz">Sender</param>
        public void OnShowFromNext(Wizard wiz)
        {
            if (ShowFromNext != null)
                ShowFromNext(wiz, EventArgs.Empty);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
//				//Unregister callbacks
//				ClearChangeNotifications();      
            }
            base.Dispose(disposing);
        }
    }
}