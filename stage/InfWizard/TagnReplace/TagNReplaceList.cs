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
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace TagNReplace
{
    public class TagDefinitionValueComparer : IComparer<TagDefinitionValue>
    {
        private readonly bool mbIgnoreCase;

        public TagDefinitionValueComparer(bool ignoreCase)
        {
            mbIgnoreCase = ignoreCase;
        }

        #region IComparer<TagDefinitionValue> Members

        public int Compare(TagDefinitionValue x, TagDefinitionValue y)
        {
            return String.Compare(x.Key, y.Key, mbIgnoreCase);
        }

        #endregion
    }

    public class TagNReplaceList : IEnumerable<TagDefinitionValue>
    {
        private readonly IComparer<TagDefinitionValue> mComparer;
        private readonly List<TagDefinitionValue> mList;

        private readonly TagDefinitionValue tempItem = new TagDefinitionValue();

        public TagNReplaceList(IComparer<TagDefinitionValue> comparer)
        {
            mList = new List<TagDefinitionValue>();
            mComparer = comparer;
        }

        public TagNReplaceList(IComparer<TagDefinitionValue> comparer, TagNReplaceList list)
            : this(comparer)
        {
            foreach (TagDefinitionValue item in list)
                Add(item);
        }

        public string this[string key]
        {
            get
            {
                string value = null;
                TryGetValue(key, out value);
                return value;
            }
            set
            {
                tempItem.Key = key;
                int index = IndexOf(tempItem);
                if (index < 0)
                    Add(key, value);
                else
                    mList[index].Value = value;
            }
        }

        //public string this[object keyToString]
        //{
        //    get
        //    {
        //        return this[keyToString.ToString()];
        //    }
        //    set
        //    {
        //        this[keyToString.ToString()] = value;
        //    }
        //}

        public TagDefinitionValue this[int index]
        {
            get
            {
                return mList[index];
            }
            set
            {
                mList[index] = value;
                Sort();
            }
        }

        public int Count
        {
            get
            {
                return mList.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public List<TagDefinitionValue> InternalList
        {
            get
            {
                return mList;
            }
        }

        #region IEnumerable<TagDefinitionValue> Members

        public IEnumerator<TagDefinitionValue> GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        #endregion

        public void Add(string key, string value)
        {
            Add(new TagDefinitionValue(ref key, ref value));
        }

        public void Add(TagDefinitionValue item)
        {
            int index = IndexOf(item);
            if (index < 0)
            {
                mList.Add(item);
                Sort();
            }
        }

        public void Clear()
        {
            mList.Clear();
        }

        public bool Contains(TagDefinitionValue item)
        {
            if (IndexOf(item) < 0)
                return false;
            else
                return true;
        }

        public bool ContainsKey(string key)
        {
            tempItem.Key = key;
            if (IndexOf(tempItem) < 0)
                return false;
            else
                return true;
        }

        public void CopyTo(TagDefinitionValue[] array, int arrayIndex)
        {
            mList.CopyTo(array);
        }

        public int IndexOf(TagDefinitionValue item)
        {
            return mList.BinarySearch(item, mComparer);
        }

        public void LoadXmlTagList(Stream tagListFile)
        {
            TagDefinitionContainer tdc = (TagDefinitionContainer) TagNReplaceGlobals.TagDefSerializer.Deserialize(tagListFile);
            tdc.SaveTo(mList);
            Sort();
        }

        public bool Remove(string key)
        {
            tempItem.Key = key;
            return Remove(tempItem);
        }

        public bool Remove(TagDefinitionValue item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;
            else
            {
                mList.RemoveAt(index);
                return true;
            }
        }

        public void SaveXmlTagList(Stream tagListFile)
        {
            TagDefinitionContainer tdc = new TagDefinitionContainer();
            tdc.FillFrom(mList);
            TagNReplaceGlobals.TagDefSerializer.Serialize(tagListFile, tdc);
        }

        public void Sort()
        {
            mList.Sort(mComparer);
        }

        public bool TryGetValue(string key, out string value)
        {
            tempItem.Key = key;
            int index = IndexOf(tempItem);
            if (index < 0)
            {
                value = null;
                return false;
            }
            else
            {
                value = mList[index].Value;
                return true;
            }
        }
    }
}