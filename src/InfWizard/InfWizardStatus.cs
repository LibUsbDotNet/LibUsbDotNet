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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WinApiNet;

namespace InfWizard
{
    internal delegate void StatusEventDelegate(object sender, StatusArgs e);
    internal delegate void RawStatusEventDelegate(object sender, RawStatusArgs rawData);

    internal static class InfWizardStatus
    {
        public static StatusType DebugStatusOnlyMask=0;// = StatusType.UserDefined;

        private static object mLogLock = new object();
        public static event EventHandler<StatusArgs> StatusEvent;
        public static event EventHandler<RawStatusArgs> RawStatusEvent;

        internal static StatusArgs Log(StatusArgs statusArgs)
        {
            EventHandler<StatusArgs> temp = StatusEvent;
            if (!ReferenceEquals(temp, null))
                temp(null, statusArgs);

            if (DebugStatusOnlyMask==0)
                Debug.WriteLine("[" + statusArgs.Type + "] " + statusArgs.Text, statusArgs.Category.ToString());
            else
            {
               if ((statusArgs.Type & DebugStatusOnlyMask) > 0)
               {
                   Debug.WriteLine("[" + statusArgs.Type + "] " + statusArgs.Text, statusArgs.Category.ToString());
               }
            }
            return statusArgs;
        }

        internal static StatusArgs Log(CategoryType category, StatusType statusType, string statusText)
        {
            StatusArgs statusArgs = new StatusArgs(category, statusType, statusText);
            return Log(statusArgs);
        }

        internal static StatusArgs Log(CategoryType category, StatusType statusType, string fmt, params object[] args)
        {
            StatusArgs statusArgs = new StatusArgs(category, statusType, fmt, args);
            lock (mLogLock)
            {
                return Log(statusArgs);
            }
        }
        internal static void LogRaw(object[] rawData)
        {
            lock (mLogLock)
            {
                EventHandler<RawStatusArgs> temp = RawStatusEvent;
                if (!ReferenceEquals(temp, null))
                    temp(null, new RawStatusArgs(rawData));
                return;
            }
        }

        private static void writeStatus(RichTextBox rtfStatus, Color statusColor, StatusArgs statusArgs)
        {
            rtfStatus.SelectionColor = statusColor;
            rtfStatus.AppendText(statusArgs.Category.ToString());
            rtfStatus.SelectionColor = Color.Black;
            rtfStatus.AppendText(": " + statusArgs.Text + "\r\n");
        }

        /// <summary>
        /// Supported Types:
        /// * String = AppendText
        /// * StatusType = AppendText
        /// * CategoryType = AppendText
        /// * Color  = SelectionColor
        /// * Object[] = WriteRtfRawStatus
        /// </summary>
        /// <param name="rawData"></param>
        public static void WriteRtfRawStatus(RichTextBox rtfStatus, object[] rawData)
        {
            foreach (object o in rawData)
            {
                if (o is string)
                    rtfStatus.AppendText((string)o);
                else if (o is Color)
                    rtfStatus.SelectionColor = (Color)o;
                else if (o is StatusType)
                    rtfStatus.AppendText(((StatusType)o).ToString());
                else if (o is CategoryType)
                    rtfStatus.AppendText(((CategoryType)o).ToString());
                else if (o is FontStyle)
                    rtfStatus.SelectionFont = new Font(rtfStatus.SelectionFont, (FontStyle)o);
                else if (o is object[])
                    WriteRtfRawStatus(rtfStatus, (object[]) o);
                else
                    throw new Exception("WriteRtfRawStatus: unsupported type");
 
            }
        }
        public static void WriteRtfStatusArgsLine(RichTextBox rtfStatus, StatusArgs statusArgs)
        {
            switch (statusArgs.Type & StatusType._MASK)
            {
                case StatusType.Info:
                    writeStatus(rtfStatus, Color.MediumBlue, statusArgs);
                    break;
                case StatusType.Success:
                    writeStatus(rtfStatus, Color.DarkGreen, statusArgs);
                    break;
                case StatusType.Warning:
                    writeStatus(rtfStatus, Color.DarkOrange, statusArgs);
                    break;
                case StatusType.Error:
                    writeStatus(rtfStatus, Color.DarkRed, statusArgs);
                    break;
                default:
                    writeStatus(rtfStatus, Color.DeepPink, statusArgs);
                    break;
            }
        }
    }

    [Flags]
    public enum StatusType
    {
        Info = 0x1,
        Success = 0x2,
        Warning = 0x4,
        Error = 0x8,
        UserDefined=0x10,
        _MASK = 0xff,
        Win32Error = 0x8000,
    }

    public enum CategoryType
    {
        EnumerateDevices,
        RefreshDriver,
        CheckRemoved,
        InstallSetupPackage,
        RemoveDevice,
        DriverResource,
        InfWriter,
        DriverDownloader
    }
    public class RawStatusArgs : EventArgs
    {
        private readonly object[] mRawData;
        public RawStatusArgs(object[] rawData) { mRawData = rawData; }

        public object[] RawData
        {
            get { return mRawData; }
        }
    }
    public class StatusArgs : EventArgs
    {
        internal CategoryType mCategory;
        internal string mStatusText = String.Empty;
        internal StatusType mStatusType = StatusType.Info;


        public StatusArgs(CategoryType category, StatusType statusType, string statusText)
        {
            mStatusType = statusType;
            mStatusText = statusText;
            mCategory = category;
            if ((mStatusType & StatusType.Win32Error) != StatusType.Win32Error) return;

            string newErrorString;
            SetupApi.GetLastWin32ErrorDetails(mStatusText, out newErrorString);
            mStatusText = newErrorString;
            mStatusType ^= StatusType.Win32Error;
            if (mStatusType == 0) mStatusType = StatusType.Error;
        }

        public StatusArgs(CategoryType category, StatusType statusType, string fmt, params object[] args)
            : this(category, statusType, string.Format(fmt, args)) { }

        public string Text
        {
            get { return mStatusText; }
        }

        public CategoryType Category
        {
            get { return mCategory; }
        }

        public StatusType Type
        {
            get { return mStatusType; }
        }

        public override string ToString()
        {
            return string.Format("{0}: [{1}] {2}", mCategory, mStatusType, mStatusText);
        }
    }
}