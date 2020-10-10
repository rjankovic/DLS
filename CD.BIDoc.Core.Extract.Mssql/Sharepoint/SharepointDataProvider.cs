using CD.DLS.DAL.Configuration;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Extract.Mssql.Sharepoint
{
    public class SharepointDataProvider
    {
        private ClientContext _context;
        private Folder _rootFolder;

        public List<SharepointLibraryItem> ExtractFolderContents(string basePath, string folderPath, string[] fileExtensions)
        {
            //var basePath = "https://cleverdecision.sharepoint.com/sites/Descent";
            //var folderPath = "/Sdilene%20dokumenty";
            //var basePath = "https://intranet-test.cpi.cz/bi/";

            
            //_context = new ClientContext("https://intranet-test.cpi.cz/bi/");
            _context = new ClientContext(basePath);
            Web web = _context.Web;

            
            //var password = new SecureString();
            //foreach (var c in "password".ToCharArray())
            //    password.AppendChar(c);
            //_context.Credentials =
            //    new SharePointOnlineCredentials("Radovan.Jankovic@cleverdecision.com", password);

            var folderPathParts = folderPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            
            _rootFolder = web.RootFolder;
            var folder = _rootFolder;
            var foundFolders = "/";
            bool firstSubFolder = true;
            foreach (var folderPart in folderPathParts)
            {
                var subfolder = FindSubFolder(folder, folderPart);
                if (subfolder == null && firstSubFolder)
                {
                    ConfigManager.Log.Important(string.Format("Folder {0} could not be found, trying to find it as a list", folderPart));
                    var list = FindList(web, folderPart);
                    if (list != null)
                    {
                        ConfigManager.Log.Important(string.Format("List {0} found", folderPart));
                        subfolder = list.RootFolder;
                        firstSubFolder = false;
                        //_rootFolder = list.RootFolder;
                    }
                }
                foundFolders += folderPart + "/";
                if (subfolder == null)
                {
                    ConfigManager.Log.Important(folder.ServerRelativeUrl);
                    throw new Exception("Coluld not find folder " + foundFolders);
                }
                folder = subfolder;
            }

            List<SharepointLibraryItem> res = ListFolder(folder, fileExtensions);
            return res;
        }

        private List FindList(Web web, string name)
        {
            var list = web.Lists.GetByTitle(name);

            if (list == null)
            {
                return null;
            }

            _context.Load(list);
            _context.ExecuteQuery();

            _context.Load(list.RootFolder);
            _context.ExecuteQuery();

            return list;
        }

        private Folder FindSubFolder(Folder folder, string subFolderName)
        {
            _context.Load(folder);
            _context.Load(folder.Folders, folders => folders.Include(
                x => x.Name));
            _context.ExecuteQuery();
            
            foreach (var subFolder in folder.Folders)
            {
                if (subFolder.Name == subFolderName)
                {
                    return subFolder;
                }
            }
            return null;
        }

        private void ListFolders(Folder folder)
        {
            _context.Load(folder);
            _context.Load(folder.Folders, folders => folders.Include(
                x => x.Name, x => x.ServerRelativeUrl));
            _context.ExecuteQuery();

            foreach (var subFolder in folder.Folders)
            {
                ConfigManager.Log.Important(string.Format("\t{0}", subFolder.ServerRelativeUrl));
            }
        }


        private List<SharepointLibraryItem> ListFolder(Folder folder, string[] extensions)
        {
            //return;
            _context.Load(folder);
            _context.Load(folder.Folders, folders => folders.Include(
                x => x.Name));
            _context.ExecuteQuery();

            _context.Load(folder.Files, files => files.Include(
                f => f.Name,
                f => f.Title,
                f => f.ServerRelativeUrl
                ));
            _context.ExecuteQuery();

            List<SharepointLibraryItem> res = new List<SharepointLibraryItem>();

            foreach (var file in folder.Files)
            {
                var extension = Path.GetExtension(file.Name);
                // new string[] { ".rdl", ".rsd", ".rrds", ".rsds" }
                if (!extensions.Contains(extension))
                {
                    continue;
                }
                
                //ConfigManager.Log.Important("{2}{0} : {1}", file.Name, file.ServerRelativeUrl, indent);
                
                ClientResult<Stream> stream = file.OpenBinaryStream();
                
                _context.ExecuteQuery();
                var content = StreamToString(stream.Value);
                

                //ConfigManager.Log.Important("\t{1}{0}", res.Substring(0, 100), indent);
                res.Add(new SharepointLibraryItem()
                {
                    Type = SharepointLibraryItemTypeEnum.File,
                    Title = file.Title,
                    Name = file.Name,
                    Content = content,
                    RelativePath = file.ServerRelativeUrl.Substring(_rootFolder.ServerRelativeUrl.Length)
                });

                ConfigManager.Log.Important(string.Format("File {0} : {1}", file.Name, file.ServerRelativeUrl));
            }

            foreach (var subFolder in folder.Folders)
            {
                //ConfigManager.Log.Important("{1}{0}", subFolder.Name, indent);
                var subFolderContent = ListFolder(subFolder, extensions);
                res.Add(new SharepointLibraryItem()
                {
                    Type = SharepointLibraryItemTypeEnum.Folder,
                    Title = subFolder.Name,
                    Name = subFolder.Name,
                    Content = null,
                    RelativePath = subFolder.ServerRelativeUrl.Substring(_rootFolder.ServerRelativeUrl.Length)
                });

                ConfigManager.Log.Important("Folder {0} : {1}", subFolder.Name, subFolder.ServerRelativeUrl);

                res.AddRange(subFolderContent);
            }

            return res;
        }

        public static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
