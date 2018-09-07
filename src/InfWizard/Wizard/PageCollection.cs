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
using System.Collections;

namespace Gui.Wizard
{
    /// <summary>
    /// Summary description for PanelCollection.
    /// </summary>
    public class PageCollection : CollectionBase
    {
        private readonly Wizard vParent;

        /// <summary>
        /// Constructor requires the  wizard that owns this collection
        /// </summary>
        /// <param name="parent">Wizard</param>
        public PageCollection(Wizard parent)
        {
            vParent = parent;
        }

        /// <summary>
        /// Returns the wizard that owns this collection
        /// </summary>
        public Wizard Parent
        {
            get
            {
                return vParent;
            }
        }

        /// <summary>
        /// Finds the Page in the collection
        /// </summary>
        public WizardPage this[int index]
        {
            get
            {
                return ((WizardPage) List[index]);
            }
            set
            {
                List[index] = value;
            }
        }


        /// <summary>
        /// Adds a WizardPage into the Collection
        /// </summary>
        /// <param name="value">The page to add</param>
        /// <returns></returns>
        public int Add(WizardPage value)
        {
            int result = List.Add(value);
            return result;
        }


        /// <summary>
        /// Adds an array of pages into the collection. Used by the Studio Designer generated coed
        /// </summary>
        /// <param name="pages">Array of pages to add</param>
        public void AddRange(WizardPage[] pages)
        {
            // Use external to validate and add each entry
            foreach (WizardPage page in pages)
            {
                Add(page);
            }
        }

        /// <summary>
        /// Detects if a given Page is in the Collection
        /// </summary>
        /// <param name="value">Page to find</param>
        /// <returns></returns>
        public bool Contains(WizardPage value)
        {
            // If value is not of type Int16, this will return false.
            return (List.Contains(value));
        }

        /// <summary>
        /// Finds the position of the page in the colleciton
        /// </summary>
        /// <param name="value">Page to find position of</param>
        /// <returns>Index of Page in collection</returns>
        public int IndexOf(WizardPage value)
        {
            return (List.IndexOf(value));
        }

        /// <summary>
        /// Adds a new page at a particular position in the Collection
        /// </summary>
        /// <param name="index">Position</param>
        /// <param name="value">Page to be added</param>
        public void Insert(int index, WizardPage value)
        {
            List.Insert(index, value);
        }


        /// <summary>
        /// Removes the given page from the collection
        /// </summary>
        /// <param name="value">Page to remove</param>
        public void Remove(WizardPage value)
        {
            //Remove the item
            List.Remove(value);
        }


        /// <summary>
        /// Propgate when a external designer modifies the pages
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);
            //Showthe page added
            vParent.PageIndex = index;
        }

        /// <summary>
        /// Propogates when external designers remove items from page
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        protected override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);
            //If the page that was added was the one that was visible
            if (vParent.PageIndex == index)
            {
                //Can I show the one after
                if (index < InnerList.Count)
                {
                    vParent.PageIndex = index;
                }
                else
                {
                    //Can I show the end one (if not -1 makes everythign disappear
                    vParent.PageIndex = InnerList.Count - 1;
                }
            }
        }
    }
}