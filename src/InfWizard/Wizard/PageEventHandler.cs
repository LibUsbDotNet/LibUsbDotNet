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

namespace Gui.Wizard
{
    /// <summary>
    /// Delegate definition for handling NextPageEvents
    /// </summary>
    public delegate void PageEventHandler(object sender, PageEventArgs e);

    /// <summary>
    /// Arguments passed to an application when Page is closed in a wizard. The Next page to be displayed 
    /// can be changed, by the application, by setting the NextPage to a wizardPage which is part of the 
    /// wizard that generated this event.
    /// </summary>
    public class PageEventArgs : EventArgs
    {
        private readonly PageCollection vPages;
        private bool closeInFinsh = true;
        private int vPage;

        /// <summary>
        /// Constructs a new event
        /// </summary>
        /// <param name="index">The index of the next page in the collection</param>
        /// <param name="pages">Pages in the wizard that are acceptable to be </param>
        public PageEventArgs(int index, PageCollection pages)
        {
            vPage = index;
            vPages = pages;
        }

        /// <summary>
        /// Gets/Sets the wizard page that will be displayed next. If you set this it must be to a wizardPage from the wizard.
        /// </summary>
        public WizardPage Page
        {
            get
            {
                //Is this a valid page
                if (vPage >= 0 && vPage < vPages.Count)
                    return vPages[vPage];
                return null;
            }
            set
            {
                if (vPages.Contains(value))
                {
                    //If this is a valid page then set it
                    vPage = vPages.IndexOf(value);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("NextPage", value, "The page you tried to set was not found in the wizard.");
                }
            }
        }


        /// <summary>
        /// Gets the index of the page 
        /// </summary>
        public int PageIndex
        {
            get
            {
                return vPage;
            }
        }

        /// <summary>
        /// Allow the wizrd to close the form when finshed is clicked.
        /// </summary>
        public bool CloseInFinsh
        {
            get
            {
                return closeInFinsh;
            }
            set
            {
                closeInFinsh = value;
            }
        }
    }
}