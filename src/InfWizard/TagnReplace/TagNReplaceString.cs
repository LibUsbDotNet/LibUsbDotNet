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
using System.IO;
using System.Text.RegularExpressions;

namespace TagNReplace
{
    /// <summary>
    /// Main class for tagging strings
    /// </summary>
    public partial class TagNReplaceString
    {
        #region Enumerations

        public enum eListCloneType
        {
            ByReference,
            ByValue,
            NewList
        }

        #endregion

        private readonly FormatterCollection mTagValueFormatters = new FormatterCollection(new FormatterCollection.TagValueFormatterComparer());
        private Boolean mbIgnoreTagCase;

        private Regex mReg;

        protected TagDefinitionValueComparer mTagKeyComparer;
        protected TagNReplaceList mTagList;


        public TagNReplaceString()
            : this(false)
        {
        }

        public TagNReplaceString(Boolean ignoreTagCase)
        {
            mbIgnoreTagCase = ignoreTagCase;
            newTagList();
        }

        private TagNReplaceString(TagNReplaceString cloneFrom, eListCloneType listCloneType)
        {
            mbIgnoreTagCase = cloneFrom.mbIgnoreTagCase;

            mTagKeyComparer = new TagDefinitionValueComparer(mbIgnoreTagCase);

            // Same formatter instances used, but has its own list container.
            foreach (TagValueFormatterBase formatter in cloneFrom.mTagValueFormatters)
                mTagValueFormatters.Add(formatter);

            switch (listCloneType)
            {
                case eListCloneType.ByReference:
                    mTagList = cloneFrom.mTagList;
                    break;
                case eListCloneType.ByValue:
                    mTagList = new TagNReplaceList(mTagKeyComparer, cloneFrom.mTagList);
                    break;
                case eListCloneType.NewList:
                    mTagList = new TagNReplaceList(mTagKeyComparer);
                    break;
            }
        }


        /// <summary>
        /// Gets/Sets/Adds the specified key value pair.
        /// This indexer will not throw exceptions.  If a key is SET that doesn't exist it is automatically added. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                string rtn = null;
                mTagList.TryGetValue(key, out rtn);
                return rtn;
            }
            set
            {
                if (mTagList.ContainsKey(key))
                    mTagList[key] = value;
                else
                    mTagList.Add(key, value);
            }
        }

        public Boolean IgnoreTagCase
        {
            get
            {
                return mbIgnoreTagCase;
            }
            set
            {
                mbIgnoreTagCase = value;
                newTagList();
                mTagValueFormatters.IsModifed = true;
            }
        }

        public TagNReplaceList Tags
        {
            get
            {
                return mTagList;
            }
        }

        public FormatterCollection Formatters
        {
            get
            {
                return mTagValueFormatters;
            }
        }


        /// <summary>
        /// Executes the tagger on the tagged data parameter.
        /// </summary>
        /// <param name="taggedData"></param>
        /// <returns>The taggedData paramter with all of the tags replaced.</returns>
        public string TagString(string taggedData)
        {
            return TagString(ref taggedData);
        }

        /// <summary>
        /// Executes the tagger on the tagged data parameter.
        /// This function CAN be called recursively, and is called recursively if <see cref="AllowTaggingOfTagValues"/> is true.
        /// </summary>
        /// <param name="taggedData"></param>
        /// <returns>The taggedData paramter with all of the tags replaced.</returns>
        public string TagString(ref string taggedData)
        {
            validateRegexFormatter();
            return mReg.Replace(taggedData, new MatchEvaluator(defaultReplaceHandler));
        }


        public void LoadXmlTagList(FileInfo tagListFile)
        {
            FileStream fsTagListFile = tagListFile.OpenRead();
            LoadXmlTagList(fsTagListFile);
            fsTagListFile.Close();
        }

        public void LoadXmlTagList(String tagListFile)
        {
            LoadXmlTagList(new FileInfo(tagListFile));
        }

        public void SaveXmlTagList(Stream tagListFile)
        {
            mTagList.SaveXmlTagList(tagListFile);
        }

        public void SaveXmlTagList(FileInfo tagListFile)
        {
            if (tagListFile.Exists) tagListFile.Delete();
            FileStream fsTagListFile = tagListFile.Create();
            SaveXmlTagList(fsTagListFile);
            fsTagListFile.Flush();
            fsTagListFile.Close();
        }

        public void SaveXmlTagList(String tagListFile)
        {
            SaveXmlTagList(new FileInfo(tagListFile));
        }


        private void LoadXmlTagList(Stream tagListFile)
        {
            mTagList.LoadXmlTagList(tagListFile);
        }


        public TagNReplaceString Clone()
        {
            return Clone(eListCloneType.ByValue);
        }

        public TagNReplaceString Clone(eListCloneType cloneType)
        {
            return new TagNReplaceString(this, cloneType);
        }


        private void newTagList()
        {
            mTagKeyComparer = new TagDefinitionValueComparer(mbIgnoreTagCase);
            if (mTagList == null)
                mTagList = new TagNReplaceList(mTagKeyComparer);
            else
                mTagList = new TagNReplaceList(mTagKeyComparer, mTagList);
        }

        private void validateRegexFormatter()
        {
            if (mReg == null) mTagValueFormatters.IsModifed = true;
            if (mTagValueFormatters.IsModifed) mReg = mTagValueFormatters.NewRegExp(mbIgnoreTagCase);
        }


        /// <summary>
        /// Handles the replacement of tags with the actual values from the mTagList dictionary.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected virtual string defaultReplaceHandler(Match m)
        {
            FormatterCollection.RegExGrpItem customFormatter = null;
            Group grpTagValue = m.Groups[mTagValueFormatters.IdxTagValue];
            if (!grpTagValue.Success)
                throw new Exception("defaultReplaceHandler:\r\nTagValue expression does not exist in match.");

            foreach (FormatterCollection.RegExGrpItem tagFormatter in mTagValueFormatters.RegExGroup)
            {
                if (m.Groups[tagFormatter.GroupNumber].Success)
                {
                    customFormatter = tagFormatter;
                    break;
                }
            }
            if (customFormatter != null)
            {
                string sValue = m.Groups[mTagValueFormatters.IdxTagValue].Value;
                TagValueFormatterArgs args = new TagValueFormatterArgs(m, mTagList, ref sValue);
                customFormatter.Formatter.Format(args);
                if (args.Handled)
                {
                    if (args.PostFormatReturnValue)
                        return TagString(ref args.ReturnValue);
                    else
                        return args.ReturnValue;
                }
                else
                    return m.Value;
            }
            else
                throw new Exception("defaultReplaceHandler:\r\nUnhandled tagged expression.");
        }
    }
}