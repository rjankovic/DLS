using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace CD.DLS.Operations
{
    public static class StorageProviderExtensions
    {
        private static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static Attachment CreateDocumentZipAttachment(this IStorageProvider storageProvider, string name, AttachmentTypeEnum type, IEnumerable<Document> documents)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (Document doc in documents)
                    {
                        var entry = zip.CreateEntry(doc.Path);
                        using (var entryStream = entry.Open())
                        using (var streamWriter = new StreamWriter(entryStream))
                        {
                            streamWriter.Write(doc.Content);
                        }
                    }
                }

                ms.Seek(0, SeekOrigin.Begin);
                var uri = storageProvider.Save(ms);
                return new Attachment()
                {
                    Uri = uri,
                    Type = type,
                    Name = name,
                    AttachmentId = Guid.NewGuid()
                };
            }
        }

        public static Attachment CreateJsonAttachment(this IStorageProvider storageProvider, object obj, string name)
        {
            var bgiSer = JsonConvert.SerializeObject(obj);
            var stream = GenerateStreamFromString(bgiSer);
            var savedDestination = storageProvider.Save(stream);

            return new Attachment()
            {
                Uri = savedDestination,
                Type = AttachmentTypeEnum.JSON,
                Name = name,
                AttachmentId = Guid.NewGuid()
            };
        }

    }
}
