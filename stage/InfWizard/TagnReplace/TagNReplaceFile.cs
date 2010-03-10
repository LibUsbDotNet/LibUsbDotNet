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

namespace TagNReplace
{
    public class TagFileEventArgs : EventArgs
    {
        private readonly FileInfo mSrcFile;
        public Boolean Handled;
        private FileInfo mDstFile;

        public TagFileEventArgs(FileInfo srcFile, FileInfo dstFile)
        {
            mSrcFile = srcFile;
            mDstFile = dstFile;
        }

        public FileInfo SrcFile
        {
            get
            {
                return mSrcFile;
            }
        }

        public FileInfo DstFile
        {
            get
            {
                return mDstFile;
            }
            set
            {
                mDstFile = value;
                Handled = true;
            }
        }
    }

    /// <summary>
    /// Main class for tagging files and directories
    /// </summary>
    public class TagNReplaceFile : TagNReplaceString
    {
        #region Delegates

        public delegate void TagFileEventHandler(TagFileEventArgs tagFileEventArgs);

        #endregion

        private Boolean mbAllowOverwrite = true;
        private Boolean mbAllowSourceEqualDest;

        public TagNReplaceFile()
        {
        }

        public TagNReplaceFile(bool ignoreCase)
            : base(ignoreCase)
        {
        }

        /// <summary>
        /// If true, allows the source file(s) (files containing the tags) to be overwritten.
        /// </summary>
        public Boolean AllowlSourceEqualDest
        {
            get
            {
                return mbAllowSourceEqualDest;
            }
            set
            {
                mbAllowSourceEqualDest = value;
            }
        }

        /// <summary>
        /// If true, allows overwriting of destination file(s), unless destination=source; see <see cref="AllowlSourceEqualDest"/>.
        /// </summary>
        public Boolean AllowOverwrite
        {
            get
            {
                return mbAllowOverwrite;
            }
            set
            {
                mbAllowOverwrite = value;
            }
        }

        public event TagFileEventHandler OnPostTagFile;
        public event TagFileEventHandler OnPreTagFile;

        // Overloaded TagFile functions, these all eventually call the main function above.


        /// <summary>
        /// TagString files in the directory specified by diSource, save them in the directory specified by diDest with the same filename
        /// </summary>
        public void TagDir(DirectoryInfo diSource, DirectoryInfo diDest, string searchPattern, SearchOption searchOption)
        {
            FileInfo[] filesToTag = diSource.GetFiles(searchPattern, searchOption);
            if (!diDest.Exists) diDest.Create();
            foreach (FileInfo fileToTag in filesToTag)
            {
                string sDestFile = fileToTag.FullName.Replace(diSource.FullName, diDest.FullName);
                FileInfo fiDest = new FileInfo(sDestFile);
                TagFile(fileToTag, fiDest);
            }
        }

        /// <summary>
        /// Tags the single file fiSource and writes it to fiDest.
        /// </summary>
        /// <param name="fiSource"></param>
        /// <param name="fiDest"></param>
        public void TagFile(FileInfo fiSource, FileInfo fiDest)
        {
            if (OnPreTagFile != null)
            {
                TagFileEventHandler tempOnPreTagFile = OnPreTagFile;
                TagFileEventArgs args = new TagFileEventArgs(fiSource, fiDest);
                tempOnPreTagFile(args);
                if (args.Handled)
                    fiDest = args.DstFile;
            }
            if (fiDest.FullName == fiSource.FullName && !mbAllowSourceEqualDest)
                throw new NotSupportedException("Source and destination is equal. See the AllowlSourceEqualDest property.\r\nSource:" +
                                                fiSource.FullName);

            if (fiDest.Exists && !mbAllowOverwrite)
                throw new NotSupportedException("Destination file already exists. See the AllowOverwrite property.\r\nDest:" + fiDest.FullName);

            StreamReader sr = new StreamReader(fiSource.FullName);
            string sTaggedData = sr.ReadToEnd();
            sr.Close();

            if (fiDest.Exists) fiDest.Delete();

            StreamWriter sw = new StreamWriter(fiDest.FullName);
            sw.Write(TagString(ref sTaggedData));
            sw.Flush();
            sw.Close();

            if (OnPostTagFile != null)
            {
                TagFileEventHandler tempOnPostTagFile = OnPostTagFile;
                TagFileEventArgs args = new TagFileEventArgs(fiSource, fiDest);
                tempOnPostTagFile(args);
            }
        }


        public void TagDir(string srcDir, string destDir)
        {
            TagDir(srcDir, destDir, SearchOption.TopDirectoryOnly);
        }

        public void TagDir(string srcDir, string destDir, SearchOption searchOption)
        {
            TagDir(srcDir, destDir, "*.*", searchOption);
        }

        public void TagDir(string srcDir, string destDir, string searchPattern, SearchOption searchOption)
        {
            TagDir(new DirectoryInfo(srcDir), new DirectoryInfo(destDir), searchPattern, searchOption);
        }


        public void TagFile(string sourceFile, string destFile)
        {
            TagFile(new FileInfo(sourceFile), new FileInfo(destFile));
        }


        // Overloaded TagDir functions, these all eventually call the main function above.
    }
}