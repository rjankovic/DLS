using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Interfaces
{

    public struct RefPath
    {
        private string _path;
        private string _htmlRefPath;

        public string Path { get { return _path; } }
        public RefPath(string path)
        {
            _path = path;
            _htmlRefPath = null;
        }

        public override string ToString()
        {
            return _path;
        }

        public string RefId { get { return _path.Split('/').Last(); } }
        //public RefPath Parent { get; }
        

        public string GetHtmlRefPath()
        {
            if (_htmlRefPath == null)
            {
                _htmlRefPath = GetHtmlRefPathInner();
            }
            return _htmlRefPath;
        }

        private string GetHtmlRefPathInner()
        {
            var newRefPath = _path.ToArray();
            for (int i = 0; i < newRefPath.Length; i++)
            {
                if (!Char.IsLetter(newRefPath[i]) && !Char.IsDigit(newRefPath[i]))
                {
                    newRefPath[i] = '_';
                }
            }
            return new string(newRefPath) + '_' + _path.GetHashCode().ToString().Replace("-", "_");
        }
        
        /// <summary>
        /// If node has been reassigned under a different parent, the refpath must reflect this,
        /// but the common prefix of the two nodes' paths should be kept, therefore the new parent's
        /// ref path is suffixed by the suffix of the original path in which the two nodes differ.
        /// </summary>
        /// <param name="prefix"></param>
        public void SetPathPrefix(RefPath prefix)
        {
            var prefixString = prefix.Path;
            var commonPrefixLength = 0;
            while (commonPrefixLength < prefixString.Length - 1 && prefixString[commonPrefixLength] == _path[commonPrefixLength])
            {
                commonPrefixLength++;
            }
            var commonPrefix = _path.Substring(0, commonPrefixLength);

            // common prefix can be an incorrect refpath like "A[BC]/X[Y", cut to A[BC]
            bool quoted = false;
            int lastSlashPos = -1;
            for (int i = 0; i < commonPrefix.Length; i++)
            {
                if (commonPrefix[i] == '\'')
                {
                    quoted = !quoted;
                }
                if (!quoted && commonPrefix[i] == '/')
                {
                    lastSlashPos = i;
                }
            }
            var cutPrefix = commonPrefix;
            if (lastSlashPos >= 0)
            {
                cutPrefix = commonPrefix.Substring(0, lastSlashPos);
            }

            // "/X[YZ]/U[VT]"
            var originalDistinguishingSuffix = _path.Substring(lastSlashPos + 1);

            _path = cutPrefix + prefixString + originalDistinguishingSuffix;
            _htmlRefPath = null;
        }

        public RefPath AddRefIdSuffix(string suffix)
        {
            return new RefPath(_path + "/" + suffix);
        }
        
    }

    /// <summary>
    /// Object of the model.
    /// </summary>
    public interface IModelElement
    {
        /// <summary>
        /// The code defining the object (excluding the descendants), or null.
        /// </summary>
        string Definition { get; set; }

        /// <summary>
        /// Full path identifying the object in the solution.
        /// </summary>
        RefPath RefPath { get; }

        /// <summary>
        /// Readable caption of the object, or null.
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// Parent object in the hierarchy, or null.
        /// </summary>
        IModelElement Parent { get; }

        IEnumerable<IModelElement> Children { get; }

        int Id { get; set; }

    }
    
    public static class IModelObjectExtensions
    {
        public static string GetRefId(this IModelElement modelObject)
        {
            return modelObject.RefPath.RefId;
        }
    }

    public interface IScriptFragmentModelElement
    {
        int OffsetFrom { get; }
        int Length { get; }
        IModelElement Reference { get; }
        IEnumerable<IScriptFragmentModelElement> Children { get; }
    }
}
