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
using CD.DLS.Model.Business.Excel;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParsePivotTableTemplatesRequestProcessor : RequestProcessorBase, IRequestProcessor<ParsePivotTableTemplatesRequest, DLSApiMessage>
    {
        public DLSApiMessage Process(ParsePivotTableTemplatesRequest request, ProjectConfig projectConfig)
        {
            try
            {
                var itemIdx = request.ItemIndex;

                if (itemIdx >= request.Items.Count)
                {
                    return new DLSApiMessage();
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();

                SerializationHelper sh = new SerializationHelper(projectConfig, GraphManager);
                
                AvailableDatabaseModelIndex adbix = new AvailableDatabaseModelIndex(projectConfig, GraphManager);
                ConfigManager.Log.Important("Creating SSAS index");
                SsasServerIndex ssasIndex = new SsasServerIndex(projectConfig, GraphManager);
                
                do
                {
                    var item = request.Items[itemIdx];
                    var tableElement = (PivotTableTemplateElement)sh.LoadElementModel(item.PivotTableRefPath);

                    var sqlExtractor = new ScriptModelParser();

                    var databaseName = sqlExtractor.GetDbNameFromConnectionString(tableElement.ConnectionString);
                    var serverName = sqlExtractor.GetServerNameFromConnectionString(tableElement.ConnectionString);
                    var ssasEnvironment = ssasIndex.GetDatabase(serverName, databaseName);

                    if (ssasEnvironment == null)
                    {
                        ConfigManager.Log.Warning(string.Format("Could not find SSAS database {0} on {1} for table {2} - template will not be parsed", 
                            databaseName, serverName, tableElement.RefPath.Path));
                        itemIdx++;
                        continue;
                    }

                    // this should not be neccessary - the specific index is created with it's default query mode
                    /*
                    if (tableElement.PivotTableStructure.ConnectionType == API.Structures.PivotTableConnectionType.Multidimensional)
                    {
                        ssasEnvironment.QueryMode = SsasQueryMode.MDX;
                    }
                    else
                    {
                        ssasEnvironment.QueryMode = SsasQueryMode.DAX;
                    }
                    */

                    var multidimensionalSsasEnvironment = ssasEnvironment as SsasMultidimensionalDatabaseIndex;
                    var tabularSsasEnvironment = ssasEnvironment as SsasTabularDatabaseIndex;

                    if (ssasEnvironment.SsasType == SsasTypeEnum.Multidimensional)
                    {
                        multidimensionalSsasEnvironment.CubeInUse = tableElement.CubeName;
                    }

                    foreach (var pivotField in tableElement.VisibleFields)
                    {
                        Model.Mssql.Ssas.SsasModelElement ssasElement = null;
                        if (ssasEnvironment.SsasType == SsasTypeEnum.Multidimensional)
                        {
                            ssasElement = multidimensionalSsasEnvironment.TryResolveIdentifier(pivotField.OlapFieldName);
                        }
                        else
                        {
                            tabularSsasEnvironment.QueryMode = SsasQueryMode.MDX;
                            ssasElement = tabularSsasEnvironment.TryResolveIdentifier(pivotField.OlapFieldName, TabularReferenceType.Column);
                        }


                        if (ssasElement != null)
                        {
                            pivotField.SourceField = ssasElement;
                            //ConfigManager.Log.Info(string.Format("PivotTableParser: Found reference from {0} to {1}", pivotField.RefPath.Path, ssasElement));
                        }

                        foreach (var valueFilter in pivotField.Filters)
                        {
                            var filterMeasureIdentifier = valueFilter.MeasureName;
                            Model.Mssql.Ssas.SsasModelElement filterMeasureElement = null;
                            if (ssasEnvironment.SsasType == SsasTypeEnum.Multidimensional)
                            {
                                filterMeasureElement = multidimensionalSsasEnvironment.TryResolveIdentifier(filterMeasureIdentifier);
                            }
                            else
                            {
                                filterMeasureElement = tabularSsasEnvironment.TryResolveIdentifier(filterMeasureIdentifier, TabularReferenceType.Column);
                            }

                            if (filterMeasureElement != null)
                            {
                                valueFilter.SourceMeasure = filterMeasureElement;
                                //ConfigManager.Log.Info(string.Format("PivotTableParser: Found reference from {0} to {1}", valueFilter.RefPath.Path, filterMeasureElement));
                            }
                        }
                    }
                    
                    var premappedModel = ssasEnvironment.GetPremappedIds();
                    var premappedTableModel = sh.CreatePremappedModel(tableElement);

                    foreach (var tableMapItem in premappedTableModel)
                    {
                        if (!premappedModel.ContainsKey(tableMapItem.Key))
                        {
                            premappedModel.Add(tableMapItem.Key, tableMapItem.Value);
                        }
                    }
                    
                    sh.SaveModelPart(tableElement, premappedModel, true);

                    itemIdx++;

                } while (sw.ElapsedMilliseconds / 1000 < ConfigManager.ServiceTimeout / 2 && itemIdx < request.Items.Count);

                //if (request.ItemIndex == request.Reports.Count - 1)
                if (itemIdx >= request.Items.Count)
                {
                    return new DLSApiMessage();
                }
                else
                {
                    return new DLSApiProgressResponse()
                    {
                        ContinueWith = new ParsePivotTableTemplatesRequest()
                        {
                            ItemIndex =  itemIdx,
                            Items = request.Items
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw ex;
            }
        }

    }
}
