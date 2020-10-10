using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Serialization;
using System.Collections.Generic;
using CD.DLS.Parse.Mssql.Ssis;
using CD.DLS.Model.Mssql.Ssrs;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Parse.Mssql.Ssrs;
using CD.DLS.Parse.Mssql.Ssas;
using System.Linq;
using CD.DLS.DAL.Configuration;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParseSsrsComponentRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSsrsComponentRequest, DLSApiProgressResponse>
    {
        public DLSApiProgressResponse Process(ParseSsrsComponentRequest request, ProjectConfig projectConfig)
        {
            List<DLSApiMessage> parseReportRequests = new List<DLSApiMessage>();

            var reports = StageManager.GetExtractItems(request.ExtractId, request.SsrsComponentId, ExtractTypeEnum.SsrsReport);

            List<ParseSsrsReportItem> reportItems = new List<ParseSsrsReportItem>();

            foreach (SsrsReport report in reports)
            {
                reportItems.Add(new ParseSsrsReportItem() { ExtractItemId = report.ExtractItemId });
            }

            return new DLSApiProgressResponse()
            {
                ContinueWith = new ParseSsrsReportRequest()
                {
                    ExtractId = request.ExtractId,
                    ItemIndex = 0,
                    Reports = reportItems,
                    ServerRefPath = request.ServerRefPath,
                    SsrsComponentId = request.SsrsComponentId
                }
            };

            /*

            SerializationHelper sh = new SerializationHelper(projectConfig, GraphManager);
            var serverFolders = (ServerElement)sh.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(FolderElement));
            var serverDataSources = (ServerElement)sh.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(SharedDataSourceElement));
            var serverDataSets = (ServerElement)sh.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(SharedDataSourceElement));

            AvailableDatabaseModelIndex adbix = new AvailableDatabaseModelIndex(projectConfig, GraphManager);
            Parse.Mssql.Ssas.SsasServerIndex ssasIndex = new SsasServerIndex(projectConfig, GraphManager);
            
            var rootDataSources = serverDataSources.Children.OfType<SharedDataSourceElement>();
            var rootDataSets = serverDataSets.Children.OfType<SharedDataSetElement>();
            var allDataSources = rootDataSources.Union(CollectElements<SharedDataSourceElement>(serverDataSources.Folders)).ToList();
            var allDataSets = rootDataSets.Union(CollectElements<SharedDataSetElement>(serverDataSets.Folders)).ToList();

            
            var premappedModel = sh.CreatePremappedModel(serverFolders);
            foreach (var dataSource in allDataSources)
            {
                premappedModel.Add(dataSource, dataSource.Id);
            }
            foreach (var dataSet in allDataSets)
            {
                premappedModel.Add(dataSet, dataSet.Id);
            }

            
            foreach (SsrsReport report in reports)
            {
                //parseReportRequests.Add(new ParseSsrsReportRequest()
                //{
                //    ExtractId = request.ExtractId,
                //    ExtractItemId = report.ExtractItemId,
                //    ServerRefPath = request.ServerRefPath
                //});

                //var reportExtract = (SsrsReport)StageManager.GetExtractItem(request.ExtractItemId);

                SsrsModelExtractor extractor = new SsrsModelExtractor(adbix, ssasIndex, new MdxScriptModelExtractor(),
                projectConfig, request.ExtractId, StageManager);
                extractor.SetFolderIndex(serverFolders.Folders.ToList());
                extractor.SetSharedDataSourceIndex(allDataSources);
                extractor.SetSharedDataSetIndex(allDataSets);


                ConfigManager.Log.Important("Parsing " + report.FullPath);
                var reportElement = extractor.LoadReport(report);

                foreach (var dbItem in adbix.GetAllPremappedIds())
                {
                    if (!premappedModel.ContainsKey(dbItem.Key))
                    {
                        premappedModel.Add(dbItem.Key, dbItem.Value);
                    }
                }

                var ssasItems = ssasIndex.GetPremappedIds();
                foreach (var ssasItem in ssasItems)
                {
                    if (!premappedModel.ContainsKey(ssasItem.Key))
                    {
                        premappedModel.Add(ssasItem.Key, ssasItem.Value);
                    }
                }

                sh.SaveModelPart(reportElement, premappedModel);
            }
            

            //return new DLSApiProgressResponse()
            //{
            //    ParallelRequests = parseReportRequests
            //};

    */
            
        }

        //private IEnumerable<T> CollectElements<T>(IEnumerable<FolderElement> folders)
        //{
        //    var init = folders.SelectMany(x => x.Children.OfType<T>());
        //    return init.Union(folders.SelectMany(x => CollectElements<T>(x.Folders)));
        //}
    }
}
