using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.UI.WebControls;
using Microsoft.Live;
using Newtonsoft.Json;
using OneDriveIntegration.JsonData;

namespace OneDriveIntegration
{
    public class OneDriveClient
    {
        private readonly string _accessToken;
        private string _ownerId;
        private readonly string _rootFolderId;
        private const string ResourceTypeFolder = "folder";
        private const string ResourceTypeFile = "file";

        public OneDriveClient(string accessToken, string ownerId)
        {
            _accessToken = accessToken;
            _ownerId = ownerId;
            if (string.IsNullOrEmpty(ownerId))
            {
                _rootFolderId = "/me/skydrive";
            }
            else
            {
                _rootFolderId = "folder." + ownerId;
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

            string stringUri = string.Format("https://apis.live.net/v5.0/{0}", fileId);
            string jsonData = string.Format(" {{ \"destination\" : \"{0}\" }}", HttpUtility.HtmlEncode(destinationFolderId));

            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri(stringUri),
                Method = method,
                Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var httpClient = new HttpClient();
            var response = await httpClient.SendAsync(message);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResourceResult>(data);
            return BuildFileName(result.Owner.Id, sourceFile, destinationFolder);
        }

        private string BuildFileName(string ownerId, string fileName, string destinationFolder)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            var builder = new UriBuilder("https://d.docs.live.net")
            {
                Path = Path.Combine(ownerId, destinationFolder, Path.GetFileName(fileName))
            };

            return builder.Uri.ToString();
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

        private async Task<string> GetResourceIdAsync(string parentId, string folderName, string resourceType)
        {
            var httpClient = new HttpClient();
            var requestString = String.Format("https://apis.live.net/v5.0/{0}/files?access_token={1}", parentId, _accessToken);
            var response = await httpClient.GetAsync(requestString);

            var data = await response.Content.ReadAsStringAsync();
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
