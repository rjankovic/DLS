using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Export.Html.Formatting
{
    /// <summary>
    /// Abstract tag inserted into a script.
    /// </summary>
    public abstract class Tag
    {
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public int EndIndex { get { return StartIndex + Length; } }
    }

    /// <summary>
    /// Text style tag.
    /// </summary>
    public class StyleTag : Tag
    {
        public string StyleClass { get; set; }
    }

    /// <summary>
    /// Document link tag.
    /// </summary>
    public class LinkTag : Tag
    {
        public string Target { get; set; }
    }
}
