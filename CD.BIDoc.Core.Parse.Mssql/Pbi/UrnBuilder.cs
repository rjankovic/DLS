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

        public RefPath GetReportUrn(Report report, RefPath parent)
        {
            return parent.NamedChild("PowerBI Report", report.ReportName);
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

        public RefPath GetVisualUrn(Visual visual, RefPath parent)
        {
            return parent.NamedChild("Visual", visual.Id+"");
        }

        public RefPath GetFilterUrn(Filter filter, RefPath parent)
        {
            return parent.NamedChild("Filter", filter.FilterName);
        }

        public RefPath GetProjectionUrn(Projection projection, RefPath parent)
        {
            return parent.NamedChild("Projection", projection.Name);
        }

        public RefPath GetReportSectionUrn(ReportSection reportSection, RefPath parent)
        {
            return parent.NamedChild("Report section", reportSection.Displayname);
        }



    }
}
