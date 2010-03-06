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
using System.Windows.Forms.Design;

//If you are misssing ParentControlDesigner, then don't forget that you need a reference in
//this project to System.Design

namespace Gui.Wizard
{
    /// <summary>
    /// 
    /// </summary>
    public class InfoContainerDesigner : ParentControlDesigner
    {
//		/// <summary>
//		/// Prevents the grid from being drawn on the Wizard
//		/// </summary>
//		protected override bool DrawGrid
//		{
//			get { return false; }
//		}

        /// <summary>
        /// Drops the BackgroundImage property
        /// </summary>
        /// <param name="properties">properties to remove BackGroundImage from</param>
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (properties.Contains("BackgroundImage"))
                properties.Remove("BackgroundImage");
        }
    }
}