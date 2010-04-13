// Copyright © 2006-2010 Travis Robinson. All rights reserved.
// 
// website: http://sourceforge.net/projects/libusbdotnet
// e-mail:  libusbdotnet@gmail.com
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace InfWizard
{
    public class FolderBrowserUITypeEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof (IWindowsFormsEditorService));
            if (edSvc != null)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.SelectedPath = (string) value;
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    return fbd.SelectedPath;
                }
                return value;
            }
            return base.EditValue(context, provider, value);
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) { return UITypeEditorEditStyle.Modal; }
    }

    public class StringDropDownUIEditor : UITypeEditor
    {
        // this is a container for strings, which can be 
        // picked-out
        public static StringCollection Strings;
        private ListBox dropListBox = new ListBox();
        private IWindowsFormsEditorService edSvc;
        // this is a string array for drop-down list

        public StringDropDownUIEditor()
        {
            dropListBox.BorderStyle = BorderStyle.None;
            dropListBox.Font=new Font("Tahoma",(float)8.5);
            // add event handler for drop-down box when item 
            // will be selected
            dropListBox.Click += (dropListBox_Click);
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) { return UITypeEditorEditStyle.DropDown; }

        // Displays the UI for value selection.
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            // Uses the IWindowsFormsEditorService to 
            // display a drop-down UI in the Properties 
            // window.
            edSvc = (IWindowsFormsEditorService) provider.GetService(typeof (IWindowsFormsEditorService));
            if (edSvc != null)
            {
                dropListBox.Items.Clear();
                if (Strings != null)
                {
                    foreach (string s in Strings)
                        dropListBox.Items.Add(s);
                }
                dropListBox.Height = dropListBox.PreferredHeight;

                if (!ReferenceEquals(value,null))
                {
                    int found = dropListBox.FindStringExact(value.ToString());
                    if (found != -1)
                        dropListBox.SelectedIndex = found;
                }

                edSvc.DropDownControl(dropListBox);
                if (dropListBox.SelectedItem != null)
                    return dropListBox.SelectedItem;
            }
            return value;
        }

        private void dropListBox_Click(object sender, EventArgs e)
        {
            if (edSvc != null) edSvc.CloseDropDown();
        }
    }
}