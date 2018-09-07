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
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TagNReplace
{
    partial class TagNReplaceString
    {
        #region Nested Types

        public class FormatterCollection : IEnumerable<TagValueFormatterBase>, IEnumerable
        {
            private readonly TagValueFormatterComparer mComparer;
            private readonly List<TagValueFormatterBase> mList;
            private readonly BlankTagValueFormatter tempItem = new BlankTagValueFormatter();

            internal int IdxTagValue;
            internal Boolean IsModifed = true;
            private Regex mRegCurrent;
            private String msRegExpCurrent;
            internal List<RegExGrpItem> RegExGroup;

            internal FormatterCollection(TagValueFormatterComparer comparer)
            {
                mList = new List<TagValueFormatterBase>();
                mComparer = comparer;
            }

            internal FormatterCollection(TagValueFormatterComparer comparer, int capacity)
            {
                mList = new List<TagValueFormatterBase>(capacity);
                mComparer = comparer;
            }

            public TagValueFormatterBase this[int index]
            {
                get
                {
                    return mList[index];
                }
            }

            public TagValueFormatterBase this[string name]
            {
                get
                {
                    tempItem.Name = name;

                    int index = mList.BinarySearch(tempItem, mComparer);
                    if (index >= 0) return mList[index];

                    return null;
                }
            }

            public int Count
            {
                get
                {
                    return mList.Count;
                }
            }

            #region IEnumerable<TagValueFormatterBase> Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return mList.GetEnumerator();
            }

            public IEnumerator<TagValueFormatterBase> GetEnumerator()
            {
                return mList.GetEnumerator();
            }

            #endregion

            public void Add(TagValueFormatterBase item)
            {
                Replace(item);
            }

            public void CopyTo(TagValueFormatterBase[] array, int arrayIndex)
            {
                mList.CopyTo(array, arrayIndex);
            }

            public bool Remove(TagValueFormatterBase item)
            {
                int index = mList.BinarySearch(item, mComparer);
                if (index >= 0)
                {
                    mList.RemoveAt(index);
                    IsModifed = true;
                    return true;
                }
                return false;
            }

            public void Replace(TagValueFormatterBase item)
            {
                int index = mList.BinarySearch(item, mComparer);
                if (index >= 0)
                {
                    mList[index] = item;
                }
                else
                {
                    mList.Add(item);
                    sort();
                }

                IsModifed = true;
            }

            public void Reset()
            {
                mList.Clear();
                IsModifed = true;
            }


            internal Regex NewRegExp(Boolean ignoreCase)
            {
                if (IsModifed)
                {
                    string sMatch = "";
                    RegexOptions eRegOptions = TagNReplaceGlobals.REGEX_OPTIONS;
                    if (ignoreCase) eRegOptions |= RegexOptions.IgnoreCase;

                    if (Count <= 0) Replace(new StandardTagValueFormatter());

                    foreach (TagValueFormatterBase tagValueFormatter in mList)
                    {
                        sMatch +=
                            String.Format("(?<{0}>{1}(?<" + TagNReplaceGlobals.REGEX_TAGVALUE + ">{2}){3})",
                                          /*0*/ tagValueFormatter.GroupName,
                                          /*1*/ Regex.Escape(tagValueFormatter.TagStart),
                                          /*2*/ tagValueFormatter.ValueMatchExpr,
                                          /*3*/ Regex.Escape(tagValueFormatter.TagEnd)) + "|";
                    }
                    sMatch = sMatch.TrimEnd(new char[] {'|'});

                    Debug.Print(sMatch);

                    msRegExpCurrent = sMatch;
                    mRegCurrent = new Regex(msRegExpCurrent, eRegOptions);
                    BuildGroupList(mRegCurrent.GroupNumberFromName(TagNReplaceGlobals.REGEX_TAGVALUE),
                                   mRegCurrent.GetGroupNames(),
                                   mRegCurrent.GetGroupNumbers());

                    IsModifed = false;
                }
                return mRegCurrent;
            }


            private void BuildGroupList(int idxTagValue, string[] groupNames, int[] groupNumbers)
            {
                IdxTagValue = idxTagValue;
                RegExGroup = new List<RegExGrpItem>(Count);
                foreach (TagValueFormatterBase tagValueFormatter in mList)
                {
                    for (int iGroup = 0; iGroup < groupNames.Length; iGroup++)
                    {
                        string groupName = groupNames[iGroup];
                        int groupNumber = groupNumbers[iGroup];
                        if (tagValueFormatter.GroupName == groupName)
                        {
                            RegExGroup.Add(new RegExGrpItem(tagValueFormatter, groupName, groupNumber));
                            break;
                        }
                    }
                }
            }

            private void sort()
            {
                mList.Sort(mComparer);
            }

            #region Nested Types

            private class BlankTagValueFormatter : TagValueFormatterBase
            {
                private String mName;

                public String Name
                {
                    get
                    {
                        return mName;
                    }
                    set
                    {
                        mName = value;
                    }
                }


                public override string GroupName
                {
                    get
                    {
                        return mName;
                    }
                }

                public override string TagStart
                {
                    get
                    {
                        throw new Exception("The method or operation is not implemented.");
                    }
                }

                public override string TagEnd
                {
                    get
                    {
                        throw new Exception("The method or operation is not implemented.");
                    }
                }

                public override string ValueMatchExpr
                {
                    get
                    {
                        throw new Exception("The method or operation is not implemented.");
                    }
                }


                public override void Format(TagValueFormatterArgs args)
                {
                    throw new Exception("The method or operation is not implemented.");
                }
            }

            internal class RegExGrpItem : IComparable<RegExGrpItem>
            {
                private readonly TagValueFormatterBase mFormatter;
                private readonly String mRegExGroupName;
                private readonly int mRegExGroupNum;

                public RegExGrpItem(TagValueFormatterBase formatter, String regExGroupName, int regExGroupNum)
                {
                    mFormatter = formatter;
                    mRegExGroupName = regExGroupName;
                    mRegExGroupNum = regExGroupNum;
                }

                public TagValueFormatterBase Formatter
                {
                    get
                    {
                        return mFormatter;
                    }
                }

                public String GroupName
                {
                    get
                    {
                        return mRegExGroupName;
                    }
                }

                public int GroupNumber
                {
                    get
                    {
                        return mRegExGroupNum;
                    }
                }

                #region IComparable<RegExGrpItem> Members

                public int CompareTo(RegExGrpItem other)
                {
                    return String.Compare(GroupName, other.GroupName, true);
                }

                #endregion
            }

            internal class TagValueFormatterComparer : IComparer<TagValueFormatterBase>
            {
                #region IComparer<TagValueFormatterBase> Members

                public int Compare(TagValueFormatterBase x, TagValueFormatterBase y)
                {
                    return String.Compare(x.GroupName, y.GroupName, true);
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}