using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Benchmark
{
    partial class fBenchmark
    {
        private readonly BenchmarkDisplayUpdate mDisplayInfo=new BenchmarkDisplayUpdate();

        private class BenchmarkDisplayUpdate
        {
            public readonly object UpdateUILock = new object();

            // The following fields must be locked before use!
            public string NewStatusText = String.Empty;
            public bool IsStatusError=false;
            public StringBuilder NewInfoText =new StringBuilder();
            public string DataRateText = String.Empty;
        }


        private void timerUpdateUI_Tick(object sender, EventArgs e)
        {
            lock (mDisplayInfo.UpdateUILock)
            {
                SetStatusFromDisplayInfo();
                SetDataRateFromDisplayInfo();
            }
        }

        private void  SetDataRateFromDisplayInfo()
        {
            if (mDisplayInfo.DataRateText != String.Empty)
            {
                lDataRateEP1.Text = mDisplayInfo.DataRateText;
                mDisplayInfo.DataRateText = string.Empty;
            }
        }
        private void SetStatusFromDisplayInfo()
        {
            if (mDisplayInfo.NewStatusText != String.Empty)
            {
                string[] status = mDisplayInfo.NewStatusText.Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                tslStatus.Text = status[0];
                tslStatus.ForeColor = mDisplayInfo.IsStatusError ? Color.FromKnownColor(KnownColor.Red) : Color.FromKnownColor(KnownColor.ControlText);

                if (mDisplayInfo.NewInfoText.Length!=0)
                {
                    tInfo.AppendText(mDisplayInfo.NewInfoText.ToString());
                    mDisplayInfo.NewInfoText=new StringBuilder();
                }

                mDisplayInfo.IsStatusError = false;
                mDisplayInfo.NewStatusText = String.Empty;
            }
        }

        private void SetStatus(string statusText, bool bIsError)
        {
            lock (mDisplayInfo.UpdateUILock)
            {
                mDisplayInfo.IsStatusError = bIsError;
                if (bIsError) mDisplayInfo.NewInfoText.AppendLine(string.Format("[ERROR]{0}", statusText));
                mDisplayInfo.NewStatusText = statusText;

                if (!InvokeRequired && !timerUpdateUI.Enabled) 
                    SetStatusFromDisplayInfo();
            }

        }

        private void UpdateDataRate(int count)
        {
            mEndPointStopWatch.AddPacket((ulong)count);

            double dDataRate = Math.Round(mEndPointStopWatch.BytesPerSecond, 2);

            lock (mDisplayInfo.UpdateUILock)
            {
                if (mEndPointStopWatch.PacketErrorCount > 0)
                    mDisplayInfo.DataRateText = "E" + mEndPointStopWatch.PacketErrorCount + ":" + dDataRate + " (" + mEndPointStopWatch.PacketCount +
                                                ")";
                else
                    mDisplayInfo.DataRateText = dDataRate + " (" + mEndPointStopWatch.PacketCount + ")";

                if (!timerUpdateUI.Enabled && !InvokeRequired)
                {
                    SetDataRateFromDisplayInfo();
                }
            }
        }

    }
}
