using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.Structures
{

    public class ReportElementAbsolutePosition
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string RefPath { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public string Expression { get; set; }
        public int ModelElementId { get; set; }
        public List<ReportElementAbsolutePosition> Children { get; set; }
        public bool IsReportItem { get; set; }
        public TablixGroupHierarchy RowHierarchy { get; set; }
        public TablixGroupHierarchy ColumnHierarchy { get; set; }
        public bool Hidden { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public List<ReportElementAbsolutePosition> GetDisplayableItemsTopDown()
        {
            List<ReportElementAbsolutePosition> res = new List<ReportElementAbsolutePosition>();
            foreach (var item in Children.OrderBy(x => x.Top).ThenBy(x => x.Left))
            {
                if (item.Text != null || item.IsReportItem)
                {
                    res.Add(item);
                }
                else
                {
                    res.AddRange(item.GetDisplayableItemsTopDown());
                }
            }
            return res;
        }

        public List<ReportElementAbsolutePosition> GetAllDisplayableItems()
        {
            List<ReportElementAbsolutePosition> res = new List<ReportElementAbsolutePosition>() { this };
            foreach (var item in Children)
            {
                    res.AddRange(item.GetAllDisplayableItems());
            }
            return res;
        }

        public List<ReportElementAbsolutePosition> GetDisplayableItemsLeftRight()
        {
            List<ReportElementAbsolutePosition> res = new List<ReportElementAbsolutePosition>();
            foreach (var item in Children.OrderBy(x => x.Left).ThenBy(x => x.Top))
            {
                // separate this to another method?
                //if (item.Hidden)
                //{
                //    continue;
                //}

                if (item.Text != null || item.IsReportItem)
                {
                    res.Add(item);
                }
                else
                {
                    res.AddRange(item.GetDisplayableItemsLeftRight());
                }
            }
            return res;
        }
    }

    public class TablixGroupHierarchy
    {
        public List<TablixGroupHierarchyMember> Members { get; set; }
    }

    public class TablixGroupHierarchyMember
    {
        public string HeaderTextBox { get; set; } 
        public string GroupName { get; set; }
        public string DataElementName { get; set; }
        public List<TablixGroupHierarchyMember> ChildMembers { get; set; }
    }
}
