using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Db;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CD.DLS.Export.Debug
{
    /// <summary>
    /// Exports a model to a collection of HTML files (one per element) for debugging use.
    /// </summary>
    public class MssqlDebugExport
    {
        private readonly Dictionary<MssqlModelElement, string> _exportIds = new Dictionary<MssqlModelElement, string>();
        private int _idCounter = 0;

        /// <summary>
        /// Gets a name of html file for an element.
        /// </summary>
        private string GetFileName(MssqlModelElement element)
        {
            return GetExportId(element) + ".html";
        }
        private string GetExportId(MssqlModelElement element)
        {
            string id;
            if(!_exportIds.TryGetValue(element, out id))
            {
                var fullCaption = string.Join("", element.Caption.ToLowerInvariant().Where(c => (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_'));
                if (fullCaption.Length > 50)
                {
                    fullCaption = fullCaption.Substring(0, 25) + "__" + fullCaption.Substring(fullCaption.Length - 25);
                }
                id = fullCaption + _idCounter.ToString();
                _idCounter++;
                _exportIds[element] = id;
            }
            
            return id;
        }

        /// <summary>
        /// Exports one element into a html file
        /// </summary>
        public void ExportElementFile(MssqlModelElement element, string path)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(path, GetFileName(element))))
            {
                ExportElement(element, sw);
            }
        }

        MssqlModelElement FindAncestor(MssqlModelElement element, string refpath)
        {
            while(element != null)
            {
                if (element.RefPath.ToString() == refpath)
                    return element;
                element = element.Parent;
            }
            return null;
        }

        private void ExportRefPath(MssqlModelElement element, StreamWriter sw)
        {
            string[] parts = element.RefPath.ToString().Split('/');

            string prefix = "";
            for(int i = 0; i < parts.Length - 1; ++i)
            {
                prefix += parts[i];
                MssqlModelElement ancestor = FindAncestor(element, prefix);
                if (ancestor != null)
                    sw.Write("<a href='{1}'>{0}</a>/", parts[i], GetFileName(ancestor));
                else
                    sw.Write("{0}/", parts[i]);
                prefix += '/';

            }

            sw.Write(parts[parts.Length - 1]);
        }

        /// <summary>
        /// Exports one element into a stream.
        /// </summary>
        public void ExportElement(MssqlModelElement element, StreamWriter sw)
        {
            // Header
            sw.WriteLine("<h1>{0}</h1>", element.Caption);
            ExportRefPath(element, sw);
            sw.WriteLine("<br/>");
            sw.WriteLine("{0}", element.GetType().Name);
            // Tree structure
            if (element.Parent != null)
                sw.WriteLine("Parent: <a href='{1}'>{0}</a>", element.Parent.Caption, GetFileName(element.Parent));

            bool first = true;
            foreach(var child in element.Children)
            {
                if (first)
                {
                    sw.WriteLine("Children:");
                    first = false;
                }
                sw.WriteLine(" <a href='{1}'>{0}</a>", child.Caption, GetFileName(child));
            }

            // Links
            foreach (var property in element.GetType().GetProperties().Where(p => Attribute.IsDefined(p, typeof(ModelLinkAttribute))))
            {
                //TODO: VD: Link collections
                var at = (ModelLinkAttribute)Attribute.GetCustomAttribute(property, typeof(ModelLinkAttribute));

                var target = (MssqlModelElement)property.GetValue(element);
                if(target == null)
                {
                    sw.WriteLine("{0}: null", at.LinkType ?? property.Name);
                }
                else
                sw.WriteLine("{2}: <a href='{1}'>{0}</a>", target.Caption, GetFileName(target), at.LinkType ?? property.Name);

                
            }

            // Definition
            if (element.Definition != null)
            {
                sw.WriteLine("<pre>{0}</pre>", new XText(element.Definition));
            }
        }

        /// <summary>
        /// Exports a model into a directory of files
        /// </summary>
        public void ExportModel(MssqlModelElement element, string path)
        {
            ExportElementFile(element, path);
            foreach(var child in element.Children)
            {
                ExportModel(child, path);
            }
        }
    }
}
