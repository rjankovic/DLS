using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorCode;
using HtmlAgilityPack;

using CD.DLS.Interfaces.DependencyGraph;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Db;
using System.Net;

namespace CD.DLS.Export.Html
{
    public class MssqlDbHtmlGenerator : IGraphNodeHtmlGenerator
    {
        private LinkModeEnum _linkMode;

        public MssqlDbHtmlGenerator(LinkModeEnum linkMode)
        {
            _linkMode = linkMode;
        }

        private string ExtractText(HtmlNode node)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                return node.InnerText;
            }
            return string.Join("", node.ChildNodes.Select(c => ExtractText(c)));
        }
        private List<Tuple<SqlFragmentElement, int>> GetAllDefinitionSegments(SqlFragmentElement node, int level = 0)
        {
            return node.Children.Select(x => new Tuple<SqlFragmentElement, int>((SqlFragmentElement)x, level)).Union(node.Children.SelectMany(ds => GetAllDefinitionSegments((SqlFragmentElement)ds, level + 1))).ToList();
        }


        public string GenerateHtmlDocument(IDependencyGraph graph, IDependencyGraphNode node)
        {
            //return new CodeColorizer().Colorize(node.ModelElement.Definition, Languages.Sql);

            SqlFragmentElement sqlElement = null;
            if (node.ModelElement is SqlFragmentElement)
            {
                sqlElement = (SqlFragmentElement)node.ModelElement;
            }
            else if (node.ModelElement is DbScriptedElement)
            {
                sqlElement = ((DbScriptedElement)(node.ModelElement)).SqlDefinition;
            }
            else
            {
                return string.Empty;
            }

            if (sqlElement == null)
            {
                if (node.ModelElement.Definition != null)
                {
                    return new CodeColorizer().Colorize(node.ModelElement.Definition, Languages.Sql);
                }
                return string.Empty;
            }
            var segments = GetAllDefinitionSegments(sqlElement);

            HashSet<string> nodesInScript = new HashSet<string>(segments.Select(x => x.Item1.RefPath.GetHtmlRefPath()));

            List<InsertedTag> insertionOrder;
            var taggedCode = InsertCodeSegmentPositionsIntoDefinition((MssqlModelElement)node.ModelElement, segments, out insertionOrder);
            var colorCode = new CodeColorizer().Colorize(taggedCode, Languages.Sql);

            var sb = new StringBuilder(colorCode);

            //var offset = 0;

            Dictionary<string, string> targetDictionary = new Dictionary<string, string>();
            Dictionary<string, string> targetFileDictionary = new Dictionary<string, string>();
            foreach (var l in segments)
            {
                if (l.Item1.Reference == null)
                {
                    continue;
                }
                //if (targetDictionary.ContainsKey(l.NodeFrom.GetHtmlRefPath()))
                //{
                //    //var duplList = Links.Where(x => x.NodeFrom.GetHtmlRefPath() == l.NodeFrom.GetHtmlRefPath()).ToList();
                //}

                var sourcePath = l.Item1.RefPath.GetHtmlRefPath();
                var targetPath = l.Item1.Reference.RefPath.GetHtmlRefPath();
                if (_linkMode == LinkModeEnum.Href)
                {
                    sourcePath = l.Item1.RefPath.Path;
                    targetPath = l.Item1.Reference.RefPath.Path;
                }

                if (!(targetDictionary.ContainsKey(sourcePath) && targetPath == targetDictionary[sourcePath]))
                {

                    targetDictionary.Add(sourcePath, targetPath);
                }

                if (l.Item1.Reference is SqlFragmentElement)
                {
                    if (!(targetFileDictionary.ContainsKey(sourcePath) && targetFileDictionary[sourcePath] == (/*(SqlFragmentElement)*/(l.Item1.Reference)).RefPath.GetHtmlRefPath())) // restrict to scriptroot?
                    {
                        targetFileDictionary.Add(sourcePath, (/*(SqlFragmentElement)*/(l.Item1.Reference)).RefPath.GetHtmlRefPath()); // script root?
                    }
                }
            }

            //var linkDict = Links.ToDictionary(l => l.NodeFrom.GetHtmlRefPath(), l => l.NodeFrom.Definition);
            Dictionary<string, string> replacements = new Dictionary<string, string>();

            // prepare "START_" & "END_" replacements

            foreach (var segment in segments)
            {
                if (segment.Item1.Reference == null)
                {
                    continue;
                }

                var sourcePath = segment.Item1.RefPath.GetHtmlRefPath();
                var sourceHtmlPath = segment.Item1.RefPath.GetHtmlRefPath();
                var targetPath = segment.Item1.Reference.RefPath.GetHtmlRefPath();
                if (_linkMode == LinkModeEnum.Href)
                {
                    sourcePath = segment.Item1.RefPath.Path;
                    targetPath = segment.Item1.Reference.RefPath.Path;
                }

                string replacement;
                string endReplacement;
                if (!targetDictionary.ContainsKey(sourcePath))
                {
                    if (_linkMode == LinkModeEnum.Span)
                    {
                        replacement = String.Format("<span id=\"{0}\">", sourcePath);
                        endReplacement = "</span>";
                    }
                    else
                    {
                        replacement = string.Format("<a href=\"{0}\">", sourcePath);
                        endReplacement = "</a>";
                    }

                }
                else
                {
                    if (_linkMode == LinkModeEnum.Span)
                    {
                        var isInner = nodesInScript.Contains(targetDictionary[sourcePath]);
                        replacement = String.Format("<span id=\"{0}\" target=\"{1}\" {3} class=\"nodeLink {2}\">", sourcePath, targetDictionary[sourcePath], (isInner ? "inboundLink" : "outboundLink"), (isInner ? string.Empty : string.Format("targetFile=\"{0}\"", targetFileDictionary[sourcePath])));
                        endReplacement = "</span>";
                    }
                    else
                    {
                        replacement = string.Format("<a href=\"{0}\"", sourcePath);
                        endReplacement = "</a>";
                    }
                }

                replacements.Add(WebUtility.HtmlEncode("START_" + sourceHtmlPath + "|"), replacement);
                replacements.Add(WebUtility.HtmlEncode("END_" + sourceHtmlPath + "|"), endReplacement);

                /*
                var tmpString = sb.ToString();
                
                var startPos = tmpString.IndexOf("START_" + htmlRefId);
                var startLen = ("START_" + htmlRefId).Length;
                var endPos = tmpString.IndexOf("END_" + htmlRefId);
                var endLen = ("END_" + htmlRefId).Length;
                */

                // keep
                /*
                var inTxt = tmpString.Substring(startPos, endPos - startPos);
                if (linkDict.ContainsKey(htmlRefId))
                {
                    if (inTxt.Contains("INNER JOIN"))
                    {
                        var z = 1;
                    }
                }
                */
                // endkeep

                /*
                sb.Replace("START_" + htmlRefId, replacement, startPos, startLen);
                endPos = endPos - startLen + replacement.Length;
            sb.Replace("END_" + htmlRefId, "</span>", endPos, endLen);
                */
            }

            // replace "START_" & "END_"

            int pos = 0;
            StringBuilder currentWord = new StringBuilder();
            while (pos < sb.Length)
            {
                if (sb[pos] != 'S' && sb[pos] != 'E')
                {
                    pos++;
                    continue;
                }
                if (sb[pos + 1] != 'T' && sb[pos + 1] != 'N')
                {
                    pos++;
                    continue;
                }
                if (sb[pos + 2] != 'A' && sb[pos + 2] != 'D')
                {
                    pos++;
                    continue;
                }
                if (sb[pos + 3] != 'R' && sb[pos + 3] != '_')
                {
                    pos++;
                    continue;
                }
                // found "END_"
                if (sb[pos + 3] == '_')
                {
                    int endPos = pos + 3;
                    while (endPos < sb.Length && sb[endPos] != '|')
                    {
                        endPos++;
                    }
                    var tag = sb.ToString(pos, endPos - pos + 1);
                    if (replacements.ContainsKey(tag))
                    {
                        sb.Remove(pos, endPos - pos + 1);
                        sb.Insert(pos, replacements[tag]);
                        pos += replacements[tag].Length;
                    }
                    else
                    {
                        pos++;
                    }
                }
                // found "START_"
                else if (sb[pos + 4] == 'T' && sb[pos + 5] == '_')
                {
                    int endPos = pos + 5;
                    while (endPos < sb.Length && sb[endPos] != '|')
                    {
                        endPos++;
                    }
                    var tag = sb.ToString(pos, endPos - pos + 1);
                    if (replacements.ContainsKey(tag))
                    {
                        sb.Remove(pos, endPos - pos + 1);
                        sb.Insert(pos, replacements[tag]);
                        pos += replacements[tag].Length;
                    }
                    else
                    {
                        pos++;
                    }
                }
                else
                {
                    pos++;
                }
            }

            var res = sb.ToString();
            return res;
        }


        private class InsertedTag
        {
            public int Pos { get; set; }
            public int Level { get; set; }
            public string Tag { get; set; }

            public string HtmlId { get; set; }
        }

        private string InsertCodeSegmentPositionsIntoDefinition(MssqlModelElement node, List<Tuple<SqlFragmentElement, int>> segments, out List<InsertedTag> insertionOrder)
        {
            StringBuilder sb = new StringBuilder(node.Definition);

            //HashSet<string> refNodes = new HashSet<string>(Links.Select(l => l.NodeFrom.GetHtmlRefPath()).ToList());

            var startPositions = segments.Select(x => new InsertedTag() { Pos = x.Item1.OffsetFrom, Level = x.Item2, Tag = "START_" + x.Item1.RefPath.GetHtmlRefPath() + "|", HtmlId = x.Item1.RefPath.GetHtmlRefPath() });
            var endPositions = segments.Select(x => new InsertedTag() { Pos = x.Item1.OffsetFrom + x.Item1.Length, Level = x.Item2, Tag = "END_" + x.Item1.RefPath.GetHtmlRefPath() + "|", HtmlId = x.Item1.RefPath.GetHtmlRefPath() });

            /*
            var startPositionsOrd = startPositions.OrderBy(x => refNodes.Contains(x.HtmlId) ? 1 : 0).ThenBy(x => x.Pos).ThenBy(x => x.Level).ToList();
            */
            /*
            var endPositionsOrd = endPositions.OrderBy(x=> refNodes.Contains(x.HtmlId) ? 0 : 1).ThenByDescending(x => x.Pos).ThenByDescending(x => x.Level).ToList();
            */

            /*
            var startPositionsOrd = startPositions.OrderBy(x => x.Pos).ThenBy(x => x.Level).ToList();
            var endPositionsOrd = endPositions.OrderByDescending(x => x.Pos).ThenByDescending(x => x.Level).ToList();
            */


            ////////////////
            //  Tags must be inserted from left to right for insertionOrder to work;
            //  If multiple tags start at the same position
            //  - starts of wrapping segments come before starts of child segments - nesting
            //  - ends of wrapping segments come after ends of child segments
            //  - starts come after ends - previous segment ends, following starts 
            ////////////////
            var allPositionsOrd = startPositions.Union(endPositions).OrderBy(x => x.Pos).ThenBy(x => (x.Tag.StartsWith("START") ? 1 : -1) * x.Level).ToList();

            //startPositionsOrd.AddRange(endPositionsOrd);

#if DEBUG
            /*
            var positionsDictionary = allPositionsOrd.ToDictionary(x => x.Tag, x => x);
            foreach (var st in positionsDictionary.Keys.Where(x => x.StartsWith("START_%")))
            {
                Debug.Assert(positionsDictionary.ContainsKey("END_" + st.Substring("START_".Length)));
                Debug.Assert(Definition.Substring(positionsDictionary[st].Pos).StartsWith(positionsDictionary[st].Tag));
            }
            */
#endif

            //var allPositions = startPositions.Union(endPositions).OrderBy(x => x.Pos).ThenByDescending(x => x.Level).ToList();

            insertionOrder = new List<InsertedTag>();

            var insertionOffset = 0;
            foreach (var tag in allPositionsOrd)
            {
#if DEBUG

                //var prevString = sb.ToString();
                /*
                if (!prevString.Substring(insertionOffset + tag.Pos).StartsWith(tag.Tag))
                {
                    throw new Exception();
                }
                */
#endif
                sb.Insert(tag.Pos + insertionOffset, tag.Tag);
#if DEBUG
                /*
                var search = "START_SELECT__WITH_0___t_60___SELECT_61___SELECT_62___SELECT_63___SELECT_233___CONVERT_318__759608854";
                var newString = sb.ToString();
                if (prevString.Contains(search)
    && !newString.Contains(search))
                    {
                    throw new Exception();
                }
                */
#endif
                insertionOffset += tag.Tag.Length;
                insertionOrder.Add(new InsertedTag() { Pos = tag.Pos + insertionOffset, Level = tag.Level, Tag = tag.Tag, HtmlId = tag.HtmlId });
            }

#if DEBUG
            /*
            var sbString = sb.ToString();
            foreach (var pos in allPositionsOrd)
            {
                Debug.Assert(sbString.Contains(pos.Tag));
            }
            */
#endif





            return sb.ToString();
        }
        /*
          public override List<NodeDocument> GenerateDocuments(OutputDocumentFormatEnum outputFormat, int maxDepth = -1, HashSet<string> processedNodes = null)
          {
              if (ScriptRoot != this)
              {
                  return ScriptRoot.GenerateDocuments(outputFormat, -1, processedNodes);
              }
              if (processedNodes == null)
              {
                  processedNodes = new HashSet<string>();
              }
              if (processedNodes.Contains(GetHtmlRefPath()))
              {
                  return new List<NodeDocument>();
              }
              if (outputFormat != OutputDocumentFormatEnum.Html)
              {
                  throw new NotImplementedException();
              }
              var myHtml = ScriptRoot.GenerateHtmlDocument();
              var res = new List<NodeDocument>();
              res.Add(new NodeDocument()
              {
                  Name = GetHtmlRefPath() + ".html",
                  Format = OutputDocumentFormatEnum.Html,
                  Content = myHtml
              });
              processedNodes.Add(GetHtmlRefPath());
              if (maxDepth == -1 || maxDepth > 0)
              {
                  foreach (var outLink in OutboundLinks)
                  {
                      res.AddRange(outLink.NodeTo.GenerateDocuments(outputFormat, Math.Max(maxDepth - 1, -1), processedNodes));
                  }
              }
              return res;
          }
          */

    }
}
