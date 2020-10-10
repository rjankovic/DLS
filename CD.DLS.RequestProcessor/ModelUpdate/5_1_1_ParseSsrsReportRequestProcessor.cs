using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Serialization;
using System.Collections.Generic;
using CD.DLS.Model.Mssql.Ssrs;
using CD.DLS.Parse.Mssql.Ssrs;
using System.Linq;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Parse.Mssql.Ssas;
using CD.DLS.DAL.Configuration;
using System;
using System.Diagnostics;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParseSsrsReportRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSsrsReportRequest, DLSApiMessage>
    {
        public DLSApiMessage Process(ParseSsrsReportRequest request, ProjectConfig projectConfig)
        {
            try
            {
                var itemIdx = request.ItemIndex;

                if (itemIdx >= request.Reports.Count)
                {
                    return new DLSApiMessage();
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();

                SerializationHelper sh = new SerializationHelper(projectConfig, GraphManager);
                var serverFolders = (ServerElement)sh.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(FolderElement));
                var serverDataSources = (ServerElement)sh.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(SharedDataSourceElement));
                var serverDataSets = (ServerElement)sh.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(SharedDataSourceElement));

                AvailableDatabaseModelIndex adbix = new AvailableDatabaseModelIndex(projectConfig, GraphManager);
                ConfigManager.Log.Important("Creating SSAS index");
                Parse.Mssql.Ssas.SsasServerIndex ssasIndex = new SsasServerIndex(projectConfig, GraphManager);

                ConfigManager.Log.Important("Collecting data sources");
                var rootDataSources = serverDataSources.Children.OfType<SharedDataSourceElement>();
                ConfigManager.Log.Important("Collecting data sets");
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

                do
                {
                    var extractItemId = request.Reports[itemIdx].ExtractItemId;
                    var report = (SsrsReport)StageManager.GetExtractItem(extractItemId);
                    
                    ConfigManager.Log.Important("Parsing " + report.FullPath);

                    var ssrsComponent = projectConfig.SsrsComponents.First(x => x.SsrsProjectComponentId == request.SsrsComponentId);
                    var reportElement = extractor.LoadReport(report, ssrsComponent);
                    
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

                    sh.SaveModelPart(reportElement, premappedModel, true);
                    
                    itemIdx++;

                } while (sw.ElapsedMilliseconds / 1000 < ConfigManager.ServiceTimeout / 2 && itemIdx < request.Reports.Count);

                //if (request.ItemIndex == request.Reports.Count - 1)
                if (itemIdx == request.Reports.Count)
                {
                    return new DLSApiMessage();
                }
                else
                {
                    return new DLSApiProgressResponse()
                    {
                        ContinueWith = new ParseSsrsReportRequest()
                        {
                            ExtractId = request.ExtractId,
                            ItemIndex =  itemIdx,//request.ItemIndex + 1,
                            Reports = request.Reports,
                            ServerRefPath = request.ServerRefPath,
                            SsrsComponentId = request.SsrsComponentId
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw ex;
            }

            //return new DLSApiProgressResponse()
            //{
            //    ParallelRequests = parseReportRequests
            //};


            //List<DLSApiMessage> parseReportRequests = new List<DLSApiMessage>();

            //SerializationHelper sh = new SerializationHelper(projectConfig, GraphManager);
            //var serverFolders = (ServerElement)sh.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(FolderElement));
            //var serverDataSources = (ServerElement)sh.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(SharedDataSourceElement));
            //var serverDataSets = (ServerElement)sh.LoadElementModelToChildrenOfType(request.ServerRefPath, typeof(SharedDataSourceElement));

            //AvailableDatabaseModelIndex adbix = new AvailableDatabaseModelIndex(projectConfig, GraphManager);
            //SsasServerIndex ssasIndex = new SsasServerIndex(projectConfig, GraphManager);
            //SsrsModelExtractor extractor = new SsrsModelExtractor(adbix, ssasIndex, new MdxScriptModelExtractor(),
            //    projectConfig, request.ExtractId, StageManager);

            //extractor.SetFolderIndex(serverFolders.Folders.ToList());
            //var rootDataSources = serverDataSources.Children.OfType<SharedDataSourceElement>();
            //var rootDataSets = serverDataSets.Children.OfType<SharedDataSetElement>();
            //var allDataSources = rootDataSources.Union(CollectElements<SharedDataSourceElement>(serverDataSources.Folders)).ToList();
            //var allDataSets = rootDataSets.Union(CollectElements<SharedDataSetElement>(serverDataSets.Folders)).ToList();
            //extractor.SetSharedDataSourceIndex(allDataSources);
            //extractor.SetSharedDataSetIndex(allDataSets);

            //var premappedModel = sh.CreatePremappedModel(serverFolders);
            //foreach (var dataSource in allDataSources)
            //{
            //    premappedModel.Add(dataSource, dataSource.Id);
            //}
            //foreach (var dataSet in allDataSets)
            //{
            //    premappedModel.Add(dataSet, dataSet.Id);
            //}

            //var reportExtract = (SsrsReport)StageManager.GetExtractItem(request.ExtractItemId);

            //var reportElement = extractor.LoadReport(reportExtract);

            //sh.SaveModelPart(reportElement, premappedModel);

            //return new DLSApiMessage();
        }

        private IEnumerable<T> CollectElements<T>(IEnumerable<FolderElement> folders)
        {
            var init = folders.SelectMany(x => x.Children.OfType<T>());
            return init.Union(folders.SelectMany(x => CollectElements<T>(x.Folders)));
        }

    }
}
