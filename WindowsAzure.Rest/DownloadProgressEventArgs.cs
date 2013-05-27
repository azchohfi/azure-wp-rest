using System;

namespace WindowsAzure.Rest
{
    public class DownloadProgressEventArgs : EventArgs
    {
        public long BytesReceived { get; private set; }
        public long TotalBytesToReceive { get; private set; }
        public double ProgressPercentage { get; private set; }
        public object UserState { get; set; }

        public DownloadProgressEventArgs(long bytesReceived, long totalBytesToReceive, double progressPercentage, object userState)
        {
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
            ProgressPercentage = progressPercentage;
            UserState = userState;
        }
    }
}