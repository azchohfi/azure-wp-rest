using System;
using System.Net;

namespace WindowsAzure.Rest
{
    public class DownloadFinishedEventArgs : EventArgs
    {
        public HttpWebResponse Response { get; private set; }
        public object UserState { get; private set; }

        public DownloadFinishedEventArgs(HttpWebResponse response, object userState)
        {
            Response = response;
            UserState = userState;
        }
    }
}