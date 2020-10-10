using CD.DLS.Model.Mssql;
using CD.DLS.Model.Interfaces;

namespace CD.DLS.Parse.Mssql.Ssrs
{
    public class UrnBuilder
    {
        public RefPath GetServerUrn(string serverName)
        {
            return new RefPath().NamedChild("SSRSServer", serverName);
        }
        //public RefPath GetServerUrn()
        //{
        //    return new RefPath().NamedChild("SSRSServer", "SSRS");
        //}

        public RefPath GetFolderUrn(string name, RefPath parent)
        {
            return parent.NamedChild("Folder", name);
        }

        public RefPath GetDataSourceUrn(string name, RefPath parent)
        {
            return parent.NamedChild("DataSource", name);
        }

        public RefPath DataSetUrn(string name, RefPath parent)
        {
            return parent.NamedChild("DataSet", name);
        }

        public RefPath GetDataSetFieldUrn(string name, RefPath parent)
        {
            return parent.NamedChild("Field", name);
        }

        public RefPath GetReportUrn(string name, RefPath parent)
        {
            return parent.NamedChild("Report", name);
        }

        public RefPath GetExpressionUrn(RefPath parent)
        {
            return parent.Child("Expression");
        }

        public RefPath GetExpressionUrn(RefPath parent, int ordinal)
        {
            return parent.NamedChild("Expression", string.Format("No_{0}", ordinal));
        }

        public RefPath GetExpressionUrn(RefPath parent, string name)
        {
            return parent.NamedChild("Expression", name);
        }

        public RefPath GetExpressionFragmentUrn(int ordinal, RefPath parent)
        {
            return parent.NamedChild("Fragment", "No_" + ordinal);
        }

        public RefPath GetQueryParameterUrn(string name, RefPath parent)
        {
            return parent.NamedChild("QueryParameter", name);
        }

        public RefPath GetReportParameterUrn(string name, RefPath parent)
        {
            return parent.NamedChild("ReportParameter", name);
        }
        
        public RefPath GetReportParameterValidValuesStaticUrn(RefPath parent)
        {
            return parent.Child("ParameterValidValuesStatic");
        }

        public RefPath GetReportParameterValidValuesDataSetUrn(RefPath parent)
        {
            return parent.Child("ReportParameterValidValuesDataSet");
        }

        public RefPath GetReportParameterDefaultValuesDataSetUrn(RefPath parent)
        {
            return parent.Child("ReportParameterDefaultValuesDataSet");
        }

        public RefPath GetReportParameterDefaultValuesStaticUrn(RefPath parent)
        {
            return parent.Child("ReportParameterDefaultValuesStatic");
        }

        public RefPath GetParameterValueUrn(RefPath parent, int ordinal)
        {
            return parent.NamedChild("ParameterValue", string.Format("No_{0}", ordinal));
        }

        public RefPath GetReportSectionUrn(RefPath parent, int ordinal)
        {
            return parent.NamedChild("ReportSection", string.Format("No_{0}", ordinal));
        }

        public RefPath GetReportSectionBodyUrn(RefPath parent)
        {
            return parent.Child("Body");
        }

        public RefPath GetTablixUrn(RefPath parent, string name)
        {
            return parent.NamedChild("Tablix", name);
        }

        public RefPath GetTablixRowUrn(RefPath parent, int ordinal)
        {
            return parent.NamedChild("TablixRow", string.Format("No_{0}", ordinal));
        }

        public RefPath GetCellUrn(RefPath parent, int ordinal)
        {
            return parent.NamedChild("Cell", string.Format("No_{0}", ordinal));
        }

        public RefPath GetTextBoxUrn(RefPath parent, string name)
        {
            return parent.NamedChild("TextBox", name);
        }

        public RefPath GetSsrsExpressionUrn(RefPath parent, int ordinal)
        {
            return parent.NamedChild("SsrsExpression", string.Format("No_{0}", ordinal));
        }

        public RefPath GetSsrsExpressionFragmentUrn(RefPath parent, int ordinal)
        {
            return parent.NamedChild("Fragment", string.Format("No_{0}", ordinal));
        }

        public RefPath GetRectangleUrn(RefPath parent, string name)
        {
            return parent.NamedChild("Rectangle", name);
        }

        public RefPath GetChartUrn(RefPath parent, string name)
        {
            return parent.NamedChild("Chart", name);
        }

        public RefPath GetChartDataUrn(RefPath parent)
        {
            return parent.Child("ChartData");
        }

        public RefPath GetChartSeriesUrn(RefPath parent, string name)
        {
            return parent.NamedChild("ChartSeries", name);
        }

        public RefPath GetChartDataPointUrn(RefPath parent, int ordinal)
        {
            return parent.NamedChild("DataPoint", string.Format("No_{0}", ordinal));
        }

        public RefPath GetMapUrn(RefPath parent, string name)
        {
            return parent.NamedChild("Map", name);
        }

        public RefPath GetMapRegionUrn(RefPath parent, string name)
        {
            return parent.NamedChild("Region", name);
        }

        public RefPath GetMapGroupUrn(RefPath parent, string name)
        {
            return parent.NamedChild("Group", name);
        }

        public RefPath GetGaugePanelUrn(RefPath parent, string name)
        {
            return parent.NamedChild("GaugePanel", name);
        }

        public RefPath GetGaugeUrn(RefPath parent, string name)
        {
            return parent.NamedChild("Gauge", name);
        }

        public RefPath GetGaugeScaleUrn(RefPath parent, string name)
        {
            return parent.NamedChild("Scale", name);
        }

        public RefPath GetGaugePointerUrn(RefPath parent, string name)
        {
            return parent.NamedChild("Pointer", name);
        }

        public RefPath GetRowHierarchyUrn(RefPath parent)
        {
            return parent.Child("RowHierarchy");
        }

        public RefPath GetColumnHierarchyUrn(RefPath parent)
        {
            return parent.Child("ColumnHierarchy");
        }

        public RefPath GetHierarchyMemberUrn(RefPath parent, int counter)
        {
            return parent.NamedChild("HierarchyMember", string.Format("No_{0}", counter));
        }

        public RefPath GetHierarchyGroupUrn(RefPath parent, string name)
        {
            return parent.NamedChild("HierarchyMember", name);
        }

    }
}
