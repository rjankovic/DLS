using CD.DLS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Mssql
{
    public static class RefPathExtensions
    {
        /// <summary>
        /// Creates a ref path by appending a child element with a name attribute.
        /// </summary>
        public static RefPath NamedChild(this RefPath path, string childType, string childName)
        {
            if (string.IsNullOrEmpty(path.Path))
            {
                return new RefPath(string.Format("{0}[@Name='{1}']", childType, childName));
            }
            return new RefPath(string.Format("{0}/{1}[@Name='{2}']", path.Path, childType, childName));
        }
        /// <summary>
        /// Creates a ref path by appending a child element.
        /// </summary>
        public static RefPath Child(this RefPath path, string childType)
        {
            return new RefPath(string.Format("{0}/{1}", path.Path, childType));
        }
    }
}
