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
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TagNReplace
{
    public class TagKeyComparer : IComparer<string>
    {
        private readonly Boolean mbIgoreCase;

        public TagKeyComparer(bool ignoreCase)
        {
            mbIgoreCase = ignoreCase;
        }

        public Boolean IgnoreCase
        {
            get
            {
                return mbIgoreCase;
            }
        }

        #region IComparer<string> Members

        int IComparer<string>.Compare(string x, string y)
        {
            return String.Compare(x, y, mbIgoreCase);
        }

        #endregion
    }

    [XmlType("Tag")]
    public class TagDefinitionValue
    {
        [XmlAttribute] public String Key;

        [XmlAttribute] public String Value;

        public TagDefinitionValue()
        {
        }

        public TagDefinitionValue(ref String key, ref String value)
        {
            Key = key;
            Value = value;
        }
    }

    [XmlType("TagDefinition")]
    public class TagDefinitionContainer
    {
        public List<TagDefinitionValue> Tags = new List<TagDefinitionValue>();


        public void FillFrom(List<TagDefinitionValue> sortedDict)
        {
            foreach (TagDefinitionValue tdv in sortedDict)
                Tags.Add(tdv);
        }

        public void SaveTo(List<TagDefinitionValue> sortedDict)
        {
            foreach (TagDefinitionValue tdv in Tags)
                sortedDict.Add(tdv);
        }
    }
}