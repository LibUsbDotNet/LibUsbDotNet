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
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace InfWizard
{
    public partial class InfWizardForm
    {
        private readonly DriverResourceDownloader driverResourceDownloader = new DriverResourceDownloader();


        private void downloadDriverResources()
        {
            bool success = false;
            groupBoxDriverList.Enabled = false;
            buttonCancelDownload.Enabled = false;
            groupBoxDownloadStatus.Enabled = true;
            rtfDownloadSatus.LoggingEnabled = true;
            driverResourceDownloader.Reset();

            try
            {
                DriverResourceDownloads resourceDownloads = new DriverResourceDownloads();
                foreach (DataGridViewRow selectedRow in dataGridViewDriverDownloadList.SelectedRows)
                {
                    resourceDownloads.Add(selectedRow.Tag as DriverResourceDownload);
                }

                int waitRet = -1;
                foreach (DriverResourceDownload resourceDownload in resourceDownloads)
                {
                    if (driverResourceDownloader.IsCancelled)
                    {
                        waitRet = 1;
                        break;
                    }
                    InfWizardStatus.Log(CategoryType.DriverDownloader,
                                        StatusType.Info,
                                        "requesting driver resource {0}..",
                                        resourceDownload.DisplayName);

                    progressBarDownloadDriverResources.Value = 0;
                    driverResourceDownloader.DownloadDataAsync(progressBarDownloadDriverResources, resourceDownload.Url, null);
                    buttonCancelDownload.Enabled = true;

                    WaitHandle[] waitHandles = new WaitHandle[]
                                                   {driverResourceDownloader.CompleteWaitHandle, driverResourceDownloader.CancelWaitHandle};
                    do
                    {
                        waitRet = WaitHandle.WaitAny(waitHandles, 10);
                        Application.DoEvents();
                        Thread.Sleep(0);
                    } while (waitRet == WaitHandle.WaitTimeout);

                    switch (waitRet)
                    {
                        case 0:
                            InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Info, "driver resource downloaded.");
                            break;
                        case 1:
                            InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Warning, "failed getting driver resource; operation cancelled.");
                            break;
                        case WaitHandle.WaitTimeout:
                            InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Error, "connection timed out.");
                            break;
                    }
                    if (waitRet == 0 && driverResourceDownloader.Result != null)
                    {
                        string downloadFilename = driverResourceDownloader.GetDownloadFilename();
                        
                        // TODO: Add main driver resource path for saving downloaded resources
                        string path = null;
                        if (String.IsNullOrEmpty(path))
                            path = Environment.CurrentDirectory;

                        string resourceFilename = Path.Combine(path, downloadFilename);
                        if (File.Exists(resourceFilename))
                        {
                            InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Warning, "deleting driver resource file {0}", resourceFilename);
                            File.Delete(resourceFilename);
                        }
                        InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Success, "saving driver resource {0}", resourceFilename);
                        FileStream resourceStreamOut = File.Create(resourceFilename);
                        resourceStreamOut.Write(driverResourceDownloader.Result, 0, driverResourceDownloader.Result.Length);
                        resourceStreamOut.Flush();
                        resourceStreamOut.Close();
                    }
                    else
                    {
                        break;
                    }
                }

                if (waitRet == 0)
                {
                    if (DriverResManager.LoadResources())
                    {
                        success = true;
                    }
                    else
                    {
                        InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Error, "failed loading driver resources!");
                    }
                }
            }
            catch (Exception ex)
            {
                InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Error, ex.ToString());
                buttonDownloadDriverResources.Enabled = false;
                success = false;
            }
            finally
            {
                buttonCancelDownload.Enabled = false;
                rtfDownloadSatus.LoggingEnabled = false;
                if (success)
                {
                    buttonSelectAllDriverResources_Click(this, new EventArgs());
                    wizMain.NextEnabled = DriverResManager.Check();
                }
                groupBoxDriverList.Enabled = true;
            }
        }

        private void downloadDriverResourceList()
        {
            bool success = false;
            groupBoxDriverList.Enabled = false;
            buttonCancelDownload.Enabled = false;
            groupBoxDownloadStatus.Enabled = true;
            rtfDownloadSatus.LoggingEnabled = true;
            driverResourceDownloader.Reset();

            try
            {
                InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Info, "requesting driver resource list..");

                dataGridViewDriverDownloadList.Rows.Clear();

                driverResourceDownloader.DownloadDataAsync(progressBarDownloadDriverResources, mSettings.DriverResourceUrl, null);
                buttonCancelDownload.Enabled = true;
                int waitRet;
                WaitHandle[] waitHandles = new WaitHandle[] {driverResourceDownloader.CompleteWaitHandle, driverResourceDownloader.CancelWaitHandle};
                do
                {
                    waitRet = WaitHandle.WaitAny(waitHandles, 10);
                    Application.DoEvents();
                    Thread.Sleep(0);
                } while (waitRet == WaitHandle.WaitTimeout);


                switch (waitRet)
                {
                    case 0:
                        break;
                    case 1:
                        InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Warning, "get driver resource list cancelled.");
                        break;
                    case WaitHandle.WaitTimeout:
                        InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Error, "get driver resource list connection timed out.");
                        break;
                }
                if (waitRet == 0 && driverResourceDownloader.Result!=null)
                {
                    MemoryStream resultStream = new MemoryStream(driverResourceDownloader.Result);
                    DriverResourceDownloads driverResourceDownloads = DriverResourceDownloads.Load(resultStream);

                    if (driverResourceDownloads != null)
                    {
                            foreach (DriverResourceDownload download in driverResourceDownloads)
                            {
                                int row = dataGridViewDriverDownloadList.Rows.Add(new object[] { download.DisplayName, download.Description });
                                dataGridViewDriverDownloadList.Rows[row].Tag = download;
                            }

                        success = true;
                        InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Success, "get driver resource list complete.");
                        resultStream.Seek(0, SeekOrigin.Begin);
                        try
                        {
                            string fileName = driverResourceDownloader.GetDownloadFilename();
                            File.WriteAllBytes(fileName, driverResourceDownloader.Result);
                        }
                        catch (Exception)
                        {}
                    }

                    if (!success)
                        InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Error, "invalid driver resource list.");
                }
            }
            catch (Exception ex)
            {
                InfWizardStatus.Log(CategoryType.DriverDownloader, StatusType.Error, ex.ToString());
                success = false;
            }
            finally
            {
                buttonCancelDownload.Enabled = false;
                rtfDownloadSatus.LoggingEnabled = false;
                if (success)
                {
                    groupBoxDriverList.Enabled = true;
                    SynchronizationContext.Current.Post(selectDriverResources, true);
                }
            }
        }

        private void selectDriverResources(object state)
        {
            bool selected = (bool) state;
            for (int i = 0; i < dataGridViewDriverDownloadList.Rows.Count; i++)
                dataGridViewDriverDownloadList.Rows[i].Selected = selected;
        }
    }
}