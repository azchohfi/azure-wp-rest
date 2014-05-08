using System;

namespace WindowsAzure.Rest
{
    public static class AzureSettings
    {
        public static string KeyString { get; private set; }
        public static string KeySecondString { get; private set; }
        public static string Account { get; private set; }
        public static string BlobEndPoint { get; private set; }
        
        internal static readonly string SharedKeyAuthorizationScheme = "SharedKey";
        internal static byte[] Key;

        public static void Initialize(string account, string keyString, string keySecondString, string blobEndPoint)
        {
            Account = account;
            KeyString = keyString;
            KeySecondString = keySecondString;
            BlobEndPoint = blobEndPoint;

            Key = Convert.FromBase64String(KeySecondString);
        }
    }
}