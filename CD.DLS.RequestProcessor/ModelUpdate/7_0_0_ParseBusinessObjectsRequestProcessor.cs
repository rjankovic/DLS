using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql.Ssas;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Model.Mssql.Ssrs;
using CD.DLS.Parse.Mssql.Ssrs;
using CD.DLS.Parse.Mssql.Db;
using System;
using CD.DLS.DAL.Configuration;
using CD.DLS.Model;
using CD.DLS.Model.Business.Excel;
using CD.DLS.Model.Business.Organization;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParseBusinessObjectsRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseBusinessObjectsRequest, DLSApiProgressResponse>
    {
        public DLSApiProgressResponse Process(ParseBusinessObjectsRequest request, ProjectConfig projectConfig)
        {
            try
            {
                
                SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);
                var modelWithTables = (BusinessRootElement)serializationHelper.LoadElementModelToChildrenOfType("Business", typeof(PivotTableTemplateElement));
                var tables = modelWithTables.DescendantsOfType<PivotTableTemplateElement>().ToList();
                var tableItems = tables.Select(x => new PivotTableParserReference() { PivotTableRefPath = x.RefPath.Path }).ToList();

                return new DLSApiProgressResponse()
                {
                    ParallelRequests = new List<DLSApiMessage>(){ new ParsePivotTableTemplatesRequest()
                    {
                        ItemIndex = 0,
                        Items = tableItems
                    }},
                    ContinueWith = new //FindAssociationRulesRequest() // 
                    BuildAggregationsRequest()
                    {
                    }
                };
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw ex;
            }
        }
    }
}
