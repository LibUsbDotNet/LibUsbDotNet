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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace TagNReplace
{
    internal class TagNReplaceGlobals
    {
        public const RegexOptions REGEX_OPTIONS =
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;

        public const String REGEX_TAGVALUE = "TagValue";
        private static Process mAStyleProcess;
        private static XmlSerializer mTagDefSerializer;

        internal static XmlSerializer TagDefSerializer
        {
            get
            {
                if (ReferenceEquals(mTagDefSerializer, null))
                {
                    XmlSerializer temp = new XmlSerializer(typeof (TagDefinitionContainer));
                    mTagDefSerializer = temp;
                }
                return mTagDefSerializer;
            }
        }

        internal static Process AStyleProcess
        {
            get
            {
                if (ReferenceEquals(mAStyleProcess, null))
                {
                    mAStyleProcess = new Process();
                    mAStyleProcess.StartInfo.CreateNoWindow = true;
                    mAStyleProcess.StartInfo.FileName = "AStyle.exe";
                    mAStyleProcess.StartInfo.RedirectStandardInput = true;
                    mAStyleProcess.StartInfo.RedirectStandardOutput = true;
                    mAStyleProcess.StartInfo.RedirectStandardError = true;
                    mAStyleProcess.StartInfo.UseShellExecute = false;
                    mAStyleProcess.StartInfo.Arguments = "--style=ansi --mode=c";
                }
                return mAStyleProcess;
            }
        }
    }
}