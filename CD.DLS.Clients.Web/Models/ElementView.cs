using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models
{
    public enum ElementTechViewTypeEnum
    {
        None = 0, SsrsReport = 1, SsisPackage = 2, SqlCode = 3, MdxCode = 4, DaxCode = 5
    }

    public class ElementView
    {
        public string ElementViewId { get; set; }
        public int ElementId { get; set; }
        public string ElementName { get; set; }
        public string RefPath { get; set; }
        public List<string> RefPathParts { get; set; }
        public List<ElementBusinessDictionaryEntry> BusinessDictionary { get; set; }
        public ElementTechViewTypeEnum TechViewType { get; set; }
    }

    public class ElementBusinessDictionaryEntry
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
    }

    public class ElementDictionaryEntryCollection
    {
        public int ElementId { get; set; }
        public ElementBusinessDictionaryEntry[] FieldValues { get; set; }
    }
}