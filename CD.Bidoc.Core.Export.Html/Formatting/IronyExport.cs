using CD.DLS.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Export.Html.Formatting
{
    /// <summary>
    /// Exports scripts using Irony grammar.
    /// </summary>
    public class IronyScriptExport
    {
        private Irony.Parsing.Parser _parser;
        private ITagWriter _tagWriter;
        private IGrammarTagger _grammarTagger;
        
        public IronyScriptExport(Irony.Parsing.Grammar grammar, IGrammarTagger grammarTagger, ITagWriter tagWriter)
        {
            _parser = new Irony.Parsing.Parser(grammar);
            _tagWriter = tagWriter;
            _grammarTagger = grammarTagger;
        }

        private void AddTagsFromParseTree(ScriptTagInserter inserter, Irony.Parsing.ParseTreeNode parseTreeNode)
        {
            Tag tag = _grammarTagger.GetTag(parseTreeNode);
            if (tag != null)
                inserter.AddTag(tag);

            foreach(var child in parseTreeNode.ChildNodes)
            {
                AddTagsFromParseTree(inserter, child);
            }
        }

        private void AddTagsFromParseTree(ScriptTagInserter inserter, Irony.Parsing.ParseTree parseTree)
        {
            if (_grammarTagger != null)
            {
                AddTagsFromParseTree(inserter, parseTree.Root);
            }
        }

        private void AddTagsFromModel(ScriptTagInserter inserter, IScriptFragmentModelElement model)
        {
            IModelElement reference = model.Reference;

            if (reference != null)
            {
                Tag tag = new LinkTag {
                    StartIndex = model.OffsetFrom,
                    Length = model.Length,
                    Target = reference.RefPath.GetHtmlRefPath()
                };
                inserter.AddTag(tag);
            }

            foreach(var child in model.Children)
            {
                AddTagsFromModel(inserter, child);
            }
        }

        public void Export(TextWriter writer, string script, IScriptFragmentModelElement element)
        {
            var parseTree = _parser.Parse(script);

            ScriptTagInserter inserter = new ScriptTagInserter(script, _tagWriter);
            
            AddTagsFromParseTree(inserter, parseTree);
            
            AddTagsFromModel(inserter, element);

            inserter.Export(writer);   
        }

        public string Export(string script, IScriptFragmentModelElement element)
        {
            StringWriter writer = new StringWriter();
            Export(writer, script, element);
            return writer.ToString();
        }
    }
}
