using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveConnectDesktopSample
{
    public class OneDriveClient
    {
        private Microsoft.Live.LiveConnectClient _lcClient;
        private readonly string _ownerId;
        private const string folderPrefix = "folder.";
        private const string filesSuffix = "/files";
        private const string resourceTypeFolder = "folder";
        private const string resourceTypeFile = "file";


        public OneDriveClient(Microsoft.Live.LiveConnectClient lcClient, string ownerId)
        {
            this._lcClient = lcClient;
            this._ownerId = ownerId;
        }

        public async Task<string> GetFileId(string absolutePath)
        {
            var filePath = Path.GetFileName(absolutePath);
            var folderPath = Path.GetDirectoryName(absolutePath);

            var folderId = await GetFolderId(folderPath);
            if (!string.IsNullOrEmpty(folderId))
            {
                return await GetResourceId(folderId, filePath, resourceTypeFile);
            }
            return string.Empty;
        }

        public async Task<string> GetFolderId(string absolutePath)
        {
            var names = absolutePath.Split(new[] { '/', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var currentFolderId = folderPrefix + _ownerId;
            foreach (var name in names)
            {
                currentFolderId = await GetResourceId(currentFolderId, name, resourceTypeFolder);
                if (string.IsNullOrEmpty(currentFolderId))
                    break;
            }

            return currentFolderId;
        }

        private async Task<string> GetResourceId(string parentId, string folderName, string resourceType)
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
