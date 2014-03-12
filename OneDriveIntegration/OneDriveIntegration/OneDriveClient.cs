using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using OneDriveIntegration.JsonData;

namespace OneDriveIntegration
{
    public class OneDriveClient
    {
        private readonly LiveRestClient _liveRestClient;
        private readonly string _rootFolderId;
        private const string ResourceTypeFolder = "folder";
        private const string ResourceTypeFile = "file";

        public OneDriveClient(LiveRestClient liveRestClient, string rootFolderId)
        {
            _liveRestClient = liveRestClient;
            if (string.IsNullOrEmpty(rootFolderId))
            {
                _rootFolderId = "/me/skydrive";
            }
            else
            {
                _rootFolderId = rootFolderId;
            }
        }

        public async Task<string> MoveFileAsync(string sourceFile, string destinationFolder)
        {
            return await ExecuteMethodAsync(new HttpMethod("MOVE"), sourceFile, destinationFolder);
        }

        public async Task<string> CopyFileAsync(string sourceFile, string destinationFolder)
        {
            return await ExecuteMethodAsync(new HttpMethod("COPY"), sourceFile, destinationFolder);
        }

        public async Task<string> UploadFileAsync(string localFile, string destinationFolder)
        {
            var destinationFolderId = await GetFolderIdAsync(destinationFolder);
            var fileName = Path.GetFileName(localFile);

            string stringUri = string.Format("https://apis.live.net/v5.0/{0}/files/{1}?access_token={2}", destinationFolderId, HttpUtility.UrlEncode(fileName), _liveRestClient.AccessToken );

            var httpClient = new HttpClient();
            var content = new StreamContent(new FileStream(localFile, FileMode.Open, FileAccess.Read, FileShare.Read));

            // for If we want to use POST follow this link:
            // http://msdn.microsoft.com/en-us/library/live/hh826531.aspx#uploading_files
            //content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data; name=\"file\"; filename=\"HelloWorld.txt@\"");
            //content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            //var outerContent = new MultipartFormDataContent("A300x");
            //outerContent.Add(content);

            var result = await httpClient.PutAsync(stringUri, content);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsStringAsync();
        }

        public async Task<string> GetFileOrFolderIdAsync(string absolutePath)
        {
            string folderPath = absolutePath;

            // This definition needs refining
            bool isFolder = absolutePath.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture))
                            || absolutePath.EndsWith(Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture));

            if (!isFolder)
            {
                folderPath = Path.GetDirectoryName(absolutePath);
            }

            var folderId = await GetFolderIdAsync(folderPath);

            if (!string.IsNullOrEmpty(folderId) && !isFolder)
            {
                var filePath = Path.GetFileName(absolutePath);

                folderId = await GetResourceIdAsync(folderId, filePath, ResourceTypeFile);
            }

            return folderId;
        }

        public async Task<string> GetFolderIdAsync(string absolutePath)
        {
            var names = absolutePath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var currentFolderId = _rootFolderId;
            foreach (var name in names)
            {
                currentFolderId = await GetResourceIdAsync(currentFolderId, name, ResourceTypeFolder);
                if (string.IsNullOrEmpty(currentFolderId))
                    break;
            }

            return currentFolderId;
        }

        private async Task<string> ExecuteMethodAsync(HttpMethod method, string sourceFile, string destinationFolder)
        {
            if (sourceFile == null) throw new ArgumentNullException("sourceFile");
            if (destinationFolder == null) throw new ArgumentNullException("destinationFolder");

            var fileId = await GetFileOrFolderIdAsync(sourceFile);
            if (string.IsNullOrEmpty(fileId))
            {
                throw new FileNotFoundException("The source file cannot be found or you don't have access!", sourceFile);
            }
            var destinationFolderId = await GetFolderIdAsync(destinationFolder);
            if (string.IsNullOrEmpty(destinationFolderId))
            {
                throw new FileNotFoundException("The destination folder cannot be found or you don't have access!", destinationFolder);
            }

            var message = BuildMessage(method, fileId, destinationFolderId);
            var result = await SendMessage(message);

            return BuildFileName(result.Owner.Id, destinationFolder, sourceFile);
        }

        private HttpRequestMessage BuildMessage(HttpMethod method, string fileId, string destinationFolderId)
        {
            string stringUri = string.Format("https://apis.live.net/v5.0/{0}", fileId);
            string jsonData = string.Format(" {{ \"destination\" : \"{0}\" }}", HttpUtility.HtmlEncode(destinationFolderId));

            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri(stringUri),
                Method = method,
                Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _liveRestClient.AccessToken);
            return message;
        }

        private async Task<ResourceResult> SendMessage(HttpRequestMessage message)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.SendAsync(message);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                await _liveRestClient.RefreshTokensAsync();
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _liveRestClient.AccessToken);
                response = await httpClient.SendAsync(message);
            }
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();
            response.Dispose();

            var result = JsonConvert.DeserializeObject<ResourceResult>(data);
            return result;
        }

        private static string BuildFileName(string ownerId, string destinationFolder, string fileName)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            var builder = new UriBuilder("https://d.docs.live.net")
            {
                Path = Path.Combine(ownerId, "Rifm", destinationFolder, Path.GetFileName(fileName))
            };

            return builder.Uri.ToString();
        }

        private async Task<string> GetResourceIdAsync(string parentId, string folderName, string resourceType)
        {
            var httpClient = new HttpClient();
            var requestString = String.Format("https://apis.live.net/v5.0/{0}/files?access_token={1}", parentId, _liveRestClient.AccessToken);
            var response = await httpClient.GetAsync(requestString);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();

                await _liveRestClient.RefreshTokensAsync();
                requestString = String.Format("https://apis.live.net/v5.0/{0}/files?access_token={1}", parentId, _liveRestClient.AccessToken);
                response = await httpClient.GetAsync(requestString);
            }

            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();
            response.Dispose();

            var files = JsonConvert.DeserializeObject<ResourceListResult>(data);
            var file =
                files.ResourceResults.SingleOrDefault(
                    f =>
                        f.Name.Equals(folderName, StringComparison.InvariantCultureIgnoreCase) &&
                        f.Type.Equals(resourceType, StringComparison.InvariantCultureIgnoreCase));

            return (file != null) ? file.Id : string.Empty;
        }
    }
}
