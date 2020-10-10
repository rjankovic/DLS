using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.Structures
{
    public class ReportGridCell
    {
        public string Content { get; set; }
        public string RefPath { get; set; }
        public int ModelElementId { get; set; }
        public bool Highlighted { get; set; }
        public bool InTablix { get; set; }
    }
    
    public class ReportGridStructure
    {
        // [row, column]
        public ReportGridCell[,] Cells { get; set; }
        public string HighlightedRefPath { get; set; }
        public int RowCount { get { return Cells.GetLength(0); } }
        public int ColCount { get { return Cells.GetLength(1); } }

        public ReportGridStructure(ReportElementAbsolutePosition absolutePosition, string activeRefPath = null)
        {
            var topBottom = absolutePosition.GetAllDisplayableItems();
            var distinctTops = topBottom.Select(x => x.Top).Distinct().OrderBy(x => x).ToList();
            var distinctLefts = topBottom.Select(x => x.Left).Distinct().OrderBy(x => x).ToList();
            Cells = new ReportGridCell[distinctTops.Count, distinctLefts.Count];
            HighlightedRefPath = activeRefPath;

            var refPathDictionary = topBottom.ToDictionary(x => x.RefPath, x => x);

            for (int row = 0; row < distinctTops.Count; row++)
            {
                for (int col = 0; col < distinctLefts.Count; col++)
                {
                    var bottomItemAtPos = topBottom.Where(x => x.Top == distinctTops[row] && x.Left == distinctLefts[col]).OrderBy(x => x.RefPath).LastOrDefault();
                    if (bottomItemAtPos == null)
                    {
                        Cells[row, col] = null; // new ReportGridCell() { Content = null };
                        continue;
                    }

                    var parentRefPath = bottomItemAtPos.RefPath.Substring(0, bottomItemAtPos.RefPath.LastIndexOf("]/") + 1);

                    var cell = new ReportGridCell()
                    {
                        RefPath = bottomItemAtPos.RefPath,
                        ModelElementId = bottomItemAtPos.ModelElementId,
                        Content = bottomItemAtPos.Expression == null ? bottomItemAtPos.Text : bottomItemAtPos.Expression,
                        Highlighted = activeRefPath != null && activeRefPath.StartsWith(bottomItemAtPos.RefPath),
                        InTablix = bottomItemAtPos.RefPath.Contains("/Tablix[")
                    };

                    if (string.IsNullOrWhiteSpace(cell.Content))
                    {
                        Cells[row, col] = null;
                        continue;
                    }

                    if (refPathDictionary.ContainsKey(parentRefPath))
                    {
                        var parentItem = refPathDictionary[parentRefPath];
                    }

                    Cells[row, col] = cell;
                }
            }
        }
    }
}
