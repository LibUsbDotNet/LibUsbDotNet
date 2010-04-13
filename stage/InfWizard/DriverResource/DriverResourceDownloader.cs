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
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace InfWizard
{
    internal class DriverResourceDownloader
    {
        private static object mDownloadLock = new object();
        private ManualResetEvent mDownloadCancelEvent = new ManualResetEvent(true);
        private ManualResetEvent mDownloadCompleteEvent = new ManualResetEvent(true);
        private ProgressBar mProgressBar;
        private byte[] mResult;
        private WebClient mWebClient = new WebClient();

        public DriverResourceDownloader()
        {
            mWebClient.DownloadDataCompleted += new DownloadDataCompletedEventHandler(WebClient_DownloadDataCompleted);
            mWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(WebClient_DownloadProgressChanged);
        }

        public WaitHandle CompleteWaitHandle
        {
            get { return mDownloadCompleteEvent; }
        }

        public WaitHandle CancelWaitHandle
        {
            get { return mDownloadCancelEvent; }
        }

        public byte[] Result
        {
            get { return mResult; }
        }

        public bool IsCancelled
        {
            get { return mDownloadCancelEvent.WaitOne(0, false); }
        }

        public bool IsCompleted
        {
            get { return mDownloadCompleteEvent.WaitOne(0, false); }
        }

        public string GetDownloadFilename()
        {
            string content = mWebClient.ResponseHeaders["content-disposition"];
            Match match = Regex.Match(content,
                                      "filename=\"(?<filename>.+?)\"",
                                      RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

            return match.Groups["filename"].Value;
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!ReferenceEquals(mProgressBar, null))
            {
                if (e.TotalBytesToReceive >= 0 && e.BytesReceived >= 0 && e.TotalBytesToReceive >= e.BytesReceived)
                {
                    mProgressBar.Minimum = 0;
                    mProgressBar.Maximum = (int) e.TotalBytesToReceive;
                    mProgressBar.Value = (int) e.BytesReceived;
                }
            }
        }

        private void WebClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (IsCancelled)
            {
                mResult = null;
                return;
            }

            try
            {
                mResult = e.Result;
            }
            catch (WebException ex)
            {
                InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Error, "failed getting driver resource list: {0}", ex.Message);
                return;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is WebException)
                    {
                        InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Error, "download failed: {0}", ex.InnerException.Message);
                        return;
                    }
                }
                InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Error, "download failed: {0}", ex);
            }
            finally
            {
                mDownloadCompleteEvent.Set();
            }
        }

        public event DownloadProgressChangedEventHandler ProgressChangedEvent
        {
            add { mWebClient.DownloadProgressChanged += value; }
            remove { mWebClient.DownloadProgressChanged -= value; }
        }

        public event DownloadDataCompletedEventHandler DownloadDataCompleted
        {
            add { mWebClient.DownloadDataCompleted += value; }
            remove { mWebClient.DownloadDataCompleted -= value; }
        }

        public void Abort()
        {
            lock (mDownloadLock)
            {
                if (!mDownloadCompleteEvent.WaitOne(0, false) && !mDownloadCancelEvent.WaitOne(0, false))
                {
                    mDownloadCancelEvent.Set();
                    mWebClient.CancelAsync();
                }
            }
        }

        public void Reset() { mDownloadCancelEvent.Reset(); }

        public void DownloadDataAsync(ProgressBar progressBar, string uri, object userObject)
        {
            try
            {
                lock (mDownloadLock)
                {
                    mDownloadCompleteEvent.Reset();
                    if (!ReferenceEquals(progressBar, null))
                    {
                        progressBar.Style = ProgressBarStyle.Continuous;
                        progressBar.Minimum = 0;
                        progressBar.Maximum = 100;
                        progressBar.Value = 0;
                    }
                    mProgressBar = progressBar;
                    mWebClient.DownloadDataAsync(new Uri(uri), userObject);
                }
            }
            catch (WebException ex)
            {
                InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Error, "failed getting driver resource list: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is WebException)
                    {
                        InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Error, "failed getting driver resource list: {0}", ex.InnerException.Message);
                        return;
                    }
                }
                InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Error, "failed getting driver resource list: {0}", ex);
            }
        }
    }
}