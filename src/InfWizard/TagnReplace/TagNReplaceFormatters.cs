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
using System.Text.RegularExpressions;

namespace TagNReplace
{
    public class TagValueFormatterArgs
    {
        public readonly Match Match;
        public readonly TagNReplaceList Tags;
        public readonly String Value;

        public Boolean Handled = false;
        public Boolean PostFormatReturnValue = true;
        public String ReturnValue;

        internal TagValueFormatterArgs(Match Match, TagNReplaceList Tags, ref String Value)
        {
            this.Match = Match;
            this.Tags = Tags;
            this.Value = Value;
        }
    }

    public abstract class TagValueFormatterBase
    {
        public abstract String GroupName
        {
            get;
        }

        public abstract String TagStart
        {
            get;
        }

        public abstract String TagEnd
        {
            get;
        }

        public abstract String ValueMatchExpr
        {
            get;
        }


        public abstract void Format(TagValueFormatterArgs args);
    }


    public class StandardTagValueFormatter : TagValueFormatterBase
    {
        public static readonly String DEF_TAGEND = ")";
        public static readonly String DEF_TAGSTART = "$(";
        public static readonly String MATCH_VALUE_CHARS = "a-zA-Z0-9_.";
        public static readonly String MATCH_VALUE_EXPR = "[" + Regex.Escape(MATCH_VALUE_CHARS) + "]+?";

        private Boolean mbFormatReturnValue;
        private String mTagEnd;
        private String mTagStart;

        public StandardTagValueFormatter()
            : this(DEF_TAGSTART, DEF_TAGEND, true)
        {
        }

        public StandardTagValueFormatter(Boolean formatReturnValue)
            : this(DEF_TAGSTART, DEF_TAGEND, formatReturnValue)
        {
        }

        public StandardTagValueFormatter(string tagStart, string tagEnd, Boolean formatReturnValue)
        {
            mTagStart = tagStart;
            mTagEnd = tagEnd;
            mbFormatReturnValue = formatReturnValue;
        }

        public override String GroupName
        {
            get
            {
                return "Tag";
            }
        }

        public override String TagStart
        {
            get
            {
                return mTagStart;
            }
        }

        public override String TagEnd
        {
            get
            {
                return mTagEnd;
            }
        }

        public override String ValueMatchExpr
        {
            get
            {
                return MATCH_VALUE_EXPR;
            }
        }


        public override void Format(TagValueFormatterArgs args)
        {
            args.PostFormatReturnValue = mbFormatReturnValue;

            if (args.Tags.TryGetValue(args.Value, out args.ReturnValue))
                args.Handled = true;
            else
                args.Handled = false;
        }
    }

    public class TagDelimChangeFormatter : StandardTagValueFormatter
    {
        private readonly string mNewDelimEnd;
        private readonly string mNewDelimStart;

        public TagDelimChangeFormatter(string oldStart, string oldEnd, string newStart, string newEnd)
            : base(oldStart, oldEnd, false)
        {
            mNewDelimStart = newStart;
            mNewDelimEnd = newEnd;
        }

        //public TagDelimChangeFormatter() : base(DEF_TAGSTART, DEF_TAGEND, true) { }
        //public TagDelimChangeFormatter(Boolean formatReturnValue) : base(DEF_TAGSTART, DEF_TAGEND, formatReturnValue) { }

        //public TagDelimChangeFormatter(string tagStart, string tagEnd, Boolean formatReturnValue):base(mTagStart,
        //{
        //    mTagStart = tagStart;
        //    mTagEnd = tagEnd;
        //    mbFormatReturnValue = formatReturnValue;
        //}


        public override void Format(TagValueFormatterArgs args)
        {
            args.ReturnValue = mNewDelimStart + args.Value + mNewDelimEnd;
            args.PostFormatReturnValue = false;
            args.Handled = true;
        }
    }


    public class TagBreakFormatter : TagValueFormatterBase
    {
        private readonly string mBreakChars;
        private readonly string mBreakString;

        public TagBreakFormatter(string breakString, string breakChars)
        {
            mBreakString = breakString;
            mBreakChars = breakChars;
        }

        public override string GroupName
        {
            get
            {
                return "TagBreak";
            }
        }

        public override string TagStart
        {
            get
            {
                return "";
            }
        }

        public override string TagEnd
        {
            get
            {
                return "";
            }
        }

        public override string ValueMatchExpr
        {
            get
            {
                return "(" + Regex.Escape(mBreakString) + "){1}";
            }
        }


        public override void Format(TagValueFormatterArgs args)
        {
            args.ReturnValue = args.Value + mBreakChars;
            args.Handled = true;
            args.PostFormatReturnValue = false;
        }
    }
}