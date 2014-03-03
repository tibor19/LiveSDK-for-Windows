using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Live;

namespace OneDriveIntegration
{
    public class OneDriveClient
    {
        private readonly LiveConnectClient _lcClient;
        private string _rootFolderId;
        private const string folderPrefix = "folder.";
        private const string filesSuffix = "/files";
        private const string resourceTypeFolder = "folder";
        private const string resourceTypeFile = "file";

        public OneDriveClient(Microsoft.Live.LiveConnectClient lcClient) : this(lcClient, null)
        {
        }

        public OneDriveClient(Microsoft.Live.LiveConnectClient lcClient, string ownerId)
        {
            this._lcClient = lcClient;
            if (string.IsNullOrEmpty(ownerId))
            {
                _rootFolderId = "/me/skydrive";
            }
            else
            {
                _rootFolderId = folderPrefix + ownerId;
            }
        }

        public async Task MoveFileAsync(string sourceFile, string destinationFolder)
        {
            var fileId = await GetFileIdAsync(sourceFile);
            var destinationFolderId = await GetFolderIdAsync(destinationFolder);
            var result = await _lcClient.MoveAsync(fileId, destinationFolderId);
            if(result.Result.ContainsKey("error"))
            {
                throw new FileNotFoundException((string)result.Result["error"]);
            }
        }

        public async Task CopyFileAsync(string sourceFile, string destinationFolder)
        {
            var fileId = await GetFileIdAsync(sourceFile);
            var destinationFolderId = await GetFolderIdAsync(destinationFolder);
            var result = await _lcClient.CopyAsync(fileId, destinationFolderId);
            if (result.Result.ContainsKey("error"))
            {
                throw new FileNotFoundException((string)result.Result["error"]);
            }
        }

        public async Task<string> GetFileIdAsync(string absolutePath)
        {
            string folderPath = absolutePath;

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

                folderId = await GetResourceIdAsync(folderId, filePath, resourceTypeFile);
            }

            return folderId;
        }

        public async Task<string> GetFolderIdAsync(string absolutePath)
        {
            var names = absolutePath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var currentFolderId = _rootFolderId;
            foreach (var name in names)
            {
                currentFolderId = await GetResourceIdAsync(currentFolderId, name, resourceTypeFolder);
                if (string.IsNullOrEmpty(currentFolderId))
                    break;
            }

            return currentFolderId;
        }

        private async Task<string> GetResourceIdAsync(string parentId, string folderName, string resourceType)
        {
            var result = await _lcClient.GetAsync(parentId + filesSuffix);
            var data = (IList<object>)result.Result["data"];
            foreach (IDictionary<string, object> file in data)
            {
                try
                {
                    var name = (string)file["name"];
                    var type = (string)file["type"];
                    if (name.Equals(folderName, StringComparison.InvariantCultureIgnoreCase) && type.Equals(resourceType, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var id = (string)file["id"];
                        return id;
                    }
                }
                catch
                {

                }
            }
            return string.Empty;
        }
    }
}
