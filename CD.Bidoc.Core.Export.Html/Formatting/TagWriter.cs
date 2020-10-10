using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;

namespace CD.DLS.Export.Html.Formatting
{
    /// <summary>
    /// Writes text containing tags.
    /// </summary>
    public interface ITagWriter
    {
        void Text(TextWriter wr, string text);
        void Tag(TextWriter wr, Tag tag, bool start);

        IComparer<Tag> TagComparer { get; }
    }
    /// <summary>
    /// Writes text containing tags as HTML.
    /// </summary>
    public class HtmlTagWriter : ITagWriter
    {
        private LinkModeEnum _linkMode;

        public HtmlTagWriter(LinkModeEnum linkMode)
        {
            _linkMode = linkMode;
        }

        private class HTMLTagComparer : IComparer<Tag>
        {
            public int Compare(Tag x, Tag y)
            {
                if (x is LinkTag && y is StyleTag)
                    return -1;
                if (x is StyleTag && y is LinkTag)
                    return 1;
                return 0;
            }
        }

        public IComparer<Tag> TagComparer
        {
            get {
                return new HTMLTagComparer();
            }
        }

        public void Text(TextWriter wr, string text)
        {
            wr.Write(HttpUtility.HtmlEncode(text));
        }

        public virtual void WriteStyleTag(TextWriter writer, StyleTag styleTag, bool start)
        {
            if (start)
                writer.Write("<span class=\"{0}\">", styleTag.StyleClass);
            else
                writer.Write("</span>");
        }


        public virtual void WriteLinkTag(TextWriter writer, LinkTag linkTag, bool start)
        {
            if (start)
                writer.Write("<a href=\"{0}\">", linkTag.Target);
            else
                writer.Write("</a>");
        }

        public void Tag(TextWriter writer, Tag tag, bool start)
        {
            if (tag is StyleTag)
            {
                WriteStyleTag(writer, (StyleTag)tag, start);
            }
            else if (tag is LinkTag)
            {
                WriteLinkTag(writer, (LinkTag)tag, start);  
            }
            else
            {
                writer.Write("<!--Unsupported tag-->");
            }
        }
    }

    public class DirectStyleHtmlTagWriter : HtmlTagWriter
    {
        private Dictionary<string, string> _styleForClass = new Dictionary<string, string>();

        public DirectStyleHtmlTagWriter(LinkModeEnum linkMode) : base(linkMode)
        {
        }
        
        public void AddStyleForClass(string styleClass, string style)
        {
            _styleForClass.Add(styleClass, style);
        }

        public void AddColorForClass(string styleClass, string color, bool bold = false)
        {
            string style = string.Format("color:{0};", color);
            if (bold)
                style += "font-weight:bold;";
            _styleForClass.Add(styleClass, style);
        }

        public override void WriteStyleTag(TextWriter writer, StyleTag styleTag, bool start)
        {
            string style;
            if (_styleForClass.TryGetValue(styleTag.StyleClass, out style)) {

                if (start)
                {
                    writer.Write("<span style=\"{0}\">", HttpUtility.HtmlAttributeEncode(style));
                }
                else
                    base.WriteStyleTag(writer, styleTag, start);
            }
        }
    }
}
