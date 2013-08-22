using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace WindowsAzure.Rest
{
    public class BlobManager
    {
        public event EventHandler<DownloadProgressEventArgs> UploadStatusChangedHandler;
        public event EventHandler<DownloadFinishedEventArgs> UploadFinishedHandler;

        public async Task PutBlob(string containerName, string blobName, string contentType, byte[] blobContent, object userState)
        {
            const string requestMethod = "PUT";

            String urlPath = String.Format("{0}/{1}", containerName, blobName);

            const string storageServiceVersion = "2009-09-19";

            String dateInRfc1123Format = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);

            Int32 blobLength = blobContent.Length;

            const String blobType = "BlockBlob";

            String canonicalizedHeaders = String.Format(
                  "x-ms-blob-content-type:{0}\nx-ms-blob-type:{1}\nx-ms-date:{2}\nx-ms-version:{3}",
                  contentType,
                  blobType,
                  dateInRfc1123Format,
                  storageServiceVersion);
            String canonicalizedResource = String.Format("/{0}/{1}", AzureSettings.Account, urlPath);
            String stringToSign = String.Format(
                  "{0}\n\n\n{1}\n\n\n\n\n\n\n\n\n{2}\n{3}",
                  requestMethod,
                  blobLength,
                  canonicalizedHeaders,
                  canonicalizedResource);
            String authorizationHeader = CreateAuthorizationHeader(stringToSign);

            var uri = new Uri(AzureSettings.BlobEndPoint + urlPath);

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = requestMethod;
            request.AllowWriteStreamBuffering = false;
            request.Headers["x-ms-blob-type"] = blobType;
            request.Headers["x-ms-date"] = dateInRfc1123Format;
            request.Headers["x-ms-version"] = storageServiceVersion;
            request.Headers["x-ms-blob-content-type"] = contentType;
            request.Headers["Authorization"] = authorizationHeader;
            request.ContentLength = blobLength;

            using (var requestStream = await Task<Stream>.Factory.FromAsync(request.BeginGetRequestStream, request.EndGetRequestStream, null).ConfigureAwait(false))
            {
                // Read chunks of this file
                var buffer = new Byte[1024 * 32];
                int bytesRead;

                var stream = new MemoryStream(blobContent.Length);
                await stream.WriteAsync(blobContent, 0, blobContent.Length).ConfigureAwait(false);
                stream.Seek(0, SeekOrigin.Begin);

                int bytesUploaded = 0;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                    requestStream.Flush(); // Will block until data is sent

                    bytesUploaded += bytesRead;

                    if (UploadStatusChangedHandler != null)
                    {
                        UploadStatusChangedHandler(this, new DownloadProgressEventArgs(bytesUploaded, blobLength, bytesUploaded * 100.0 / blobLength, userState));
                    }
                }
                //requestStream.Write(blobContent, 0, blobLength);
            }

            using (var response = (HttpWebResponse)await Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null).ConfigureAwait(false))
            {
                if (UploadFinishedHandler != null)
                {
                    UploadFinishedHandler(this, new DownloadFinishedEventArgs(response, userState));
                }
            }
        }

        private String CreateAuthorizationHeader(String canonicalizedString)
        {
            String signature;
            using (var hmacSha256 = new HMACSHA256(AzureSettings.Key))
            {
                Byte[] dataToHmac = System.Text.Encoding.UTF8.GetBytes(canonicalizedString);
                signature = Convert.ToBase64String(hmacSha256.ComputeHash(dataToHmac));
            }

            String authorizationHeader = String.Format(
                  CultureInfo.InvariantCulture,
                  "{0} {1}:{2}",
                  AzureSettings.SharedKeyAuthorizationScheme,
                  AzureSettings.Account,
                  signature);

            return authorizationHeader;
        }
    }
}