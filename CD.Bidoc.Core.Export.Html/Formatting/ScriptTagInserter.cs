using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CD.DLS.Export.Html.Formatting
{
    class ScriptTagInserter
    {
        private string _script;
        private List<TagOccurence> _tags;
        private ITagWriter _tagWriter;
        private TagOccurenceComparer _comparer;

        private struct TagOccurence
        {
            internal readonly Tag tag;
            internal readonly bool isStart;
            internal readonly int index;

            internal TagOccurence(Tag tag, bool isStart)
            {
                this.tag = tag;
                this.isStart = isStart;
                index = isStart ? tag.StartIndex : tag.EndIndex;
            }

        }
        private class TagOccurenceComparer : IComparer<TagOccurence>
        {
            private IComparer<Tag> _tagComparer;

            public TagOccurenceComparer(IComparer<Tag> tagComparer)
            {
                _tagComparer = tagComparer;
            }

            public int Compare(TagOccurence a, TagOccurence b)
            {
                int cmp = a.index.CompareTo(b.index);
                if (cmp != 0)
                    return cmp;

                // End tags go before start tags.
                if (a.isStart && !b.isStart)
                    return -1;
                if (b.isStart && !a.isStart)
                    return 1;

                // index is the same and both tags are the same end
                // longer tags start first and end last

                cmp = a.tag.Length.CompareTo(b.tag.Length);
                if (cmp != 0)
                    return FlipComparison(cmp, a.isStart);

                // both tags have the same extent, use tag comparer
                // lower priority tasks start first and end last
                cmp = _tagComparer.Compare(a.tag, b.tag);
                return FlipComparison(cmp, !a.isStart);                
            }

            private static int FlipComparison(int result, bool flip)
            {
                if (flip)
                {
                    if (result < 0)
                        return 1;
                    if (result > 0)
                        return -1;
                }

                return result;
            }
        }

        public ScriptTagInserter(string script, ITagWriter tagWriter)
        {
            _script = script;
            _tags = new List<TagOccurence>();
            _tagWriter = tagWriter;
            _comparer = new TagOccurenceComparer(tagWriter.TagComparer);
        }

        public void AddTag(Tag tag)
        {
            TagOccurence start = new TagOccurence(tag, isStart: true);
            _tags.Add(start);
            TagOccurence end = new TagOccurence(tag, isStart: false);
            _tags.Add(end);
        }

        public void Export(TextWriter writer)
        {
            _tags.Sort(_comparer);

            int current = 0;

            foreach (var tagOccurence in _tags)
            {
                _tagWriter.Text(writer, _script.Substring(current, tagOccurence.index));
                current = tagOccurence.index;
                _tagWriter.Tag(writer, tagOccurence.tag, tagOccurence.isStart);
            }

            _tagWriter.Text(writer, _script.Substring(current));
        }
    }
}
