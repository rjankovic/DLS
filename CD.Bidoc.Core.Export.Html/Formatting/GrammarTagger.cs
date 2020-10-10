using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Export.Html.Formatting
{
    /// <summary>
    /// Provides formatting tags for parse tree nodes.
    /// </summary>
    public interface IGrammarTagger
    {
        Tag GetTag(Irony.Parsing.ParseTreeNode node);
    }

    public class GrammarTagger : IGrammarTagger
    {
        protected Dictionary<string, string> _classesForRules = new Dictionary<string, string>();

        public void AddClass(string rule, string @class)
        {
            _classesForRules.Add(rule, @class);
        }

        public Tag GetTag(Irony.Parsing.ParseTreeNode node)
        {
            string styleClass;
            if (_classesForRules.TryGetValue(node.Term.Name, out styleClass))
            {
                return new StyleTag { StyleClass = styleClass, StartIndex = node.Span.Location.Position, Length = node.Span.Length };
            }
            else
            {
                // No tag for this node.
                return null;
            }
        }
    }

}
