using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Common.Interfaces
{
    public interface IStorageProvider
    {
        Uri Save(Stream content);
        Stream Read(Uri path);
        void Save(Stream content, Uri uri);
        void Delete(Uri uri);
    }
}
