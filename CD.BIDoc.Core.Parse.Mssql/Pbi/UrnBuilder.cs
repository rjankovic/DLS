using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;

namespace CD.DLS.Parse.Mssql.Pbi
{
    public class UrnBuilder
    {
        public RefPath GetTenantUrn(string tenantId)
        {
            return new RefPath().NamedChild("PowerBI", tenantId);
        }

        public RefPath GetTenantUrn(Tenant tenant)
        {
            return new RefPath().NamedChild("PowerBI", tenant.TenantID.ToString());
        }

        public RefPath GetWorkspaceUrn(string workspaceName, RefPath parent)
        {
            return parent.NamedChild("Workspace", workspaceName);
        }

        public RefPath GetReportUrn(Report report, MssqlModelElement parent)
        {
            return parent.RefPath.NamedChild("PowerBI Report", report.ReportName + "_" + (parent.Children.Count() + 1).ToString());
        }

        public RefPath GetColumnUrn(PbiColumn column, RefPath parent)
        {
            return parent.NamedChild("Column", column.ColumnName);
        }

        public RefPath GetColumnUrn(string columnName, RefPath parent)
        {
            return parent.NamedChild("Column", columnName);
        }

        public RefPath GetTableUrn(PbiTable table, RefPath parent)
        {
            return parent.NamedChild("Table",table.TableName);
        }

        public RefPath GetConnectionUrn(Connection connection, RefPath parent)
        {
            return parent.NamedChild("Connection", connection.Type);
        }

        public RefPath GetVisualUrn(Visual visual, MssqlModelElement parent)
        {
            if (!string.IsNullOrEmpty(visual.Id))
            {
                return parent.RefPath.NamedChild("Visual", visual.Id + "_" + (parent.Children.Count() + 1).ToString());
            }
            else
            {
                return parent.RefPath.NamedChild("Visual", "No_" + (parent.Children.Count() + 1).ToString());
            }
        }

        public RefPath GetFilterUrn(Filter filter, MssqlModelElement parent)
        {
            return parent.RefPath.NamedChild("Filter", $"No_{parent.Children.Count() + 1}");
        }

        public RefPath GetProjectionUrn(Projection projection, MssqlModelElement parent)
        {
            return parent.RefPath.NamedChild("Projection", projection.Name + $"_{parent.Children.Count() + 1}");
        }

        public RefPath GetReportSectionUrn(ReportSection reportSection, MssqlModelElement parent)
        {
            return parent.RefPath.NamedChild("Report section", reportSection.Displayname + $"_{parent.Children.Count() + 1}");
        }

        public RefPath GetMeasureExtensionUrn(string tableName, string measureName, MssqlModelElement parent)
        {
            return parent.RefPath.NamedChild("ExtensionMeasure", $"{tableName}_{measureName}");
        }



    }
}
