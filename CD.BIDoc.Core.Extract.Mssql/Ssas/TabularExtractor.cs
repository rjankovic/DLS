using System;

using CD.DLS.Common.Structures;

using System.Collections.Generic;
using CD.DLS.DAL.Objects.Extract;
using System.Diagnostics;
using Microsoft.AnalysisServices.Tabular;
using CD.DLS.DAL.Managers;
using CD.DLS.Common.Tools;
using System.IO;
using System.Reflection;
using System.Linq;
using CD.DLS.DAL.Configuration;

namespace CD.DLS.Extract.Mssql.Ssas
{
    class TabularExtractor
    {
      


        private Server _server;
        private SsasDbProjectComponent _tabularProject;
        List<TabularDB> databases;
        List<TabularModel> models;
        
        private string _outputDirPath;
        private string _relativePathBase;
        private Manifest _manifest;
        
        public TabularExtractor(SsasDbProjectComponent tabularProjectComponent, string outputDirPath, string relativePathBase, Manifest manifest)
        {
            _outputDirPath = outputDirPath;
            _relativePathBase = relativePathBase;
            _manifest = manifest;
            _tabularProject = tabularProjectComponent;
            databases = new List<TabularDB>();
            models = new List<TabularModel>();
            
       /*     foreach (KeyValuePair<string, string> entry in dbNames)
            {
                Debug.WriteLine("Tabular Extractor, predana reldb " + entry.Key + " " + entry.Value);
                // do something with entry.Value or entry.Key
            } */
        }

     /*   public Boolean isTabular()
        {
            var serverName = _tabularProject.ServerName;
            Microsoft.AnalysisServices.Server _server = new Microsoft.AnalysisServices.Server();
            _server.Connect(string.Format("Provider=MSOLAP.8;Integrated Security=SSPI;DataSource={0}", _tabularProject.ServerName));

                 if (_server.ServerMode == Microsoft.AnalysisServices.ServerMode.Tabular)
                 {
                     return true;
                 }

                 else { return false; }  
           
        } */

        public void Extract()
        {
            
            var serverName = _tabularProject.ServerName;
            _server = new Server();

            _server.Connect(string.Format("Provider=MSOLAP.8;Integrated Security=SSPI;DataSource={0}", _tabularProject.ServerName));
            
           

            foreach (Microsoft.AnalysisServices.Tabular.Database db in _server.Databases)
            {
               
                if (db.Name.Equals(_tabularProject.DbName))
                {
                    



                    TabularDB tdb = new TabularDB();

                    tdb.DBName = db.Name;

                    tdb.Collation = db.Collation;
                    tdb.Description = db.Description;


                    foreach (Microsoft.AnalysisServices.Annotation an in db.Annotations)
                    {
                        TabularAnnotation ta = new TabularAnnotation();
                        ta.Name = an.Name;
                        ta.Value = an.Value.ToString();
                        tdb.Annotations.Add(ta);
                    }



                    Microsoft.AnalysisServices.Tabular.Model tblModel = db.Model;
                    TabularModel tbm = new TabularModel();


                    tbm.modelName = tblModel.Name;

                    tbm.DBName = tblModel.Database.Name;

                    tbm.Description = tblModel.Description;

                    foreach (Microsoft.AnalysisServices.Tabular.Annotation an in tblModel.Annotations)
                    {
                        TabularAnnotation ta = new TabularAnnotation();
                        ta.Name = an.Name;
                        ta.Value = an.Value;
                        tbm.Annotations.Add(ta);
                    }



                    foreach (Microsoft.AnalysisServices.Tabular.DataSource ds in db.Model.DataSources)
                    {
                        //   var allProp= ds.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).OrderBy(pi => pi.Name).ToList();
                        //   var allFields = ds.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).OrderBy(pi => pi.Name).ToList();
                        //String ServerName = dbNames[ds.Name];
                        //   Debug.WriteLine("DS Server Name "+ServerName);

                        //var connectionString = ds.
                        //var serverName = 


                        //if (providerDs == null)
                        //{
                        //    continue;
                        //}

                        if (ds is ProviderDataSource)
                        {
                            var providerDs = ds as ProviderDataSource;

                            var connectionString = providerDs.ConnectionString;
                            var dsServerName = ConnectionStringTools.GetServerName(connectionString);
                            var dsDbName = ConnectionStringTools.GetDbName(connectionString);

                            TabularDataSource tbs = new TabularDataSource();

                            tbs.DSname = ds.Name;
                            tbs.Description = ds.Description;
                            tbs.ServerName = dsServerName;
                            tbs.DatabaseName = dsDbName;

                            //tbs.ServerName = ServerName;


                            foreach (Microsoft.AnalysisServices.Tabular.Annotation an in ds.Annotations)
                            {
                                TabularAnnotation ta = new TabularAnnotation();
                                ta.Name = an.Name;
                                ta.Value = an.Value;
                                tbs.Annotations.Add(ta);
                            }

                            tbm.TabularDataSources.Add(tbs);
                        }
                        else if (ds is StructuredDataSource)
                        {
                            var structuredDs = ds as StructuredDataSource;

                            TabularDataSource tbs = new TabularDataSource();

                            tbs.DSname = structuredDs.Name;
                            tbs.ServerName = structuredDs.ConnectionDetails.Address.Server;
                            tbs.DatabaseName = structuredDs.ConnectionDetails.Address.Database;
                            tbs.Description = structuredDs.Description;

                            foreach (Microsoft.AnalysisServices.Tabular.Annotation an in ds.Annotations)
                            {
                                TabularAnnotation ta = new TabularAnnotation();
                                ta.Name = an.Name;
                                ta.Value = an.Value;
                                tbs.Annotations.Add(ta);
                            }

                            tbm.TabularDataSources.Add(tbs);
                        }
                        else
                        {
                            ConfigManager.Log.Warning(string.Format("Unsupported tabular datasource type: {0}", ds.GetType().FullName));

                        }

                    }



                    foreach (Table t in tblModel.Tables)
                    {
                        TabularTable tt = new TabularTable();
                        tt.Name = t.Name;
                        //tblModel.DataSources

                        

                        foreach (Column cl in t.Columns)
                        {
                            TabularTableColumn ttc = new TabularTableColumn();
                            ttc.Name = cl.Name;
                            //cl.Type == ColumnType.
                            ttc.DataType = cl.DataType.ToString();
                            ttc.ColumnType = (TabularTableColumnTypeEnum)Enum.Parse(typeof(TabularTableColumnTypeEnum), cl.Type.ToString());

                            if (cl is DataColumn)
                            {
                                ttc.SourceColumn = ((DataColumn)cl).SourceColumn;
                            }

                            if (cl is CalculatedColumn)
                            {
                                ttc.Expression = ((CalculatedColumn)cl).Expression;
                            }

                            foreach (Microsoft.AnalysisServices.Tabular.Annotation an in cl.Annotations)
                            {
                                TabularAnnotation ta = new TabularAnnotation();
                                ta.Name = an.Name;
                                ta.Value = an.Value;

                                ttc.Annotations.Add(ta);
                            }

                            AttributeHierarchy ah = cl.AttributeHierarchy;



                            TabularTableColumnAttributeHierarchy hierarchy = new TabularTableColumnAttributeHierarchy();
                            hierarchy.Name = ah.ToString();


                            foreach (Microsoft.AnalysisServices.Tabular.Annotation an in ah.Annotations)
                            {
                                TabularAnnotation ta = new TabularAnnotation();
                                ta.Name = an.Name;
                                ta.Value = an.Value;
                                hierarchy.Annotations.Add(ta);
                            }

                            ttc.AttributeHierarchy = hierarchy;
                            tt.Columns.Add(ttc);
                        }

                        foreach (Microsoft.AnalysisServices.Tabular.Partition p in t.Partitions)
                        {

                            TabularTablePartition ttp = new TabularTablePartition();
                            ttp.Name = p.Name;

                            if (p.SourceType == PartitionSourceType.Query)
                            {
                                var queryPartitionSource = (QueryPartitionSource)p.Source;
                                ttp.PartitionSourceType = TabularPartitionSourceTypeEnum.QueryPartitionSource;
                                ttp.Query = queryPartitionSource.Query;
                                ttp.DataSourceName = queryPartitionSource.DataSource.Name;
                            }
                            else if (p.SourceType == PartitionSourceType.Calculated)
                            {
                                var calculatedPartitionSource = (CalculatedPartitionSource)p.Source;
                                ttp.PartitionSourceType = TabularPartitionSourceTypeEnum.CalculatedPartitionSource;
                                ttp.Expression = calculatedPartitionSource.Expression;
                            }
                            else if (p.SourceType == PartitionSourceType.M)
                            {
                                var mPartitionSource = (MPartitionSource)p.Source;
                                ttp.PartitionSourceType = TabularPartitionSourceTypeEnum.MLanguagePartitionSource;
                                ttp.Expression = mPartitionSource.Expression;
                            }
                            else
                            {
                                // skip the partition
                                ConfigManager.Log.Warning("Unsupported partition source type: " + p.GetType().FullName);
                                continue;
                            }

                            foreach (Microsoft.AnalysisServices.Tabular.Annotation an in p.Annotations)
                            {
                                TabularAnnotation ta = new TabularAnnotation();
                                ta.Name = an.Name;
                                ta.Value = an.Value;
                                ttp.Annotations.Add(ta);
                            }

                            tt.Partitions.Add(ttp);
                        }

                        foreach (Microsoft.AnalysisServices.Tabular.Measure m in t.Measures)
                        {
                            
                            TabularTableMeasure ttm = new TabularTableMeasure();
                            ttm.Name = m.Name;
                            ttm.Expression = m.Expression;
                            ttm.ErrorMessage = m.ErrorMessage;




                            foreach (Microsoft.AnalysisServices.Tabular.Annotation an in m.Annotations)
                            {
                                TabularAnnotation ta = new TabularAnnotation();
                                ta.Name = an.Name;
                                ta.Value = an.Value;
                                ttm.Annotations.Add(ta);
                            }

                            tt.Measures.Add(ttm);
                        }

                        foreach (Microsoft.AnalysisServices.Tabular.Hierarchy h in t.Hierarchies)
                        {
                            TabularTableHierarchy tth = new TabularTableHierarchy();
                            tth.Name = h.Name;

                            foreach (Microsoft.AnalysisServices.Tabular.Annotation an in h.Annotations)
                            {
                                TabularAnnotation ta = new TabularAnnotation();
                                ta.Name = an.Name;
                                ta.Value = an.Value;
                                tth.Annotations.Add(ta);
                            }

                            tt.Hierarchies.Add(tth);
                        }

                        foreach (Microsoft.AnalysisServices.Tabular.Annotation an in t.Annotations)
                        {
                            TabularAnnotation ta = new TabularAnnotation();
                            ta.Name = an.Name;
                            ta.Value = an.Value;
                            tt.Annotations.Add(ta);
                        }

                        tbm.TabularTables.Add(tt);
                    }


                    foreach (Microsoft.AnalysisServices.Tabular.Relationship r in db.Model.Relationships)
                    {
                        TabularRelationship rs = new TabularRelationship();
                        rs.Name = r.Name;
                        rs.FromTable = r.FromTable.Name;
                        rs.ToTable = r.ToTable.Name;


                        foreach (Microsoft.AnalysisServices.Tabular.Annotation an in r.Annotations)
                        {
                            TabularAnnotation ta = new TabularAnnotation();
                            ta.Name = an.Name;
                            ta.Value = an.Value;
                            rs.Annotations.Add(ta);
                        }

                        tbm.relationships.Add(rs);
                    }

                    foreach (Microsoft.AnalysisServices.Tabular.Perspective p in db.Model.Perspectives)
                    {
                        TabularPerspective tp = new TabularPerspective();
                        tp.Name = p.Name;

                        foreach (PerspectiveTable pt in p.PerspectiveTables)
                        {
                            TabularPerspectiveTable tpt = new TabularPerspectiveTable();
                            tpt.Name = pt.Name;

                            foreach (PerspectiveColumn pc in pt.PerspectiveColumns)
                            {
                                TabularPerspectiveTableColumn tptc = new TabularPerspectiveTableColumn();
                                tptc.Name = pc.Name;

                                foreach (Microsoft.AnalysisServices.Tabular.Annotation an in pc.Annotations)
                                {
                                    TabularAnnotation ta = new TabularAnnotation();
                                    ta.Name = an.Name;
                                    ta.Value = an.Value;
                                    tptc.Annotations.Add(ta);
                                }

                                tpt.Columns.Add(tptc);
                            }

                            foreach (Microsoft.AnalysisServices.Tabular.PerspectiveMeasure m in pt.PerspectiveMeasures)
                            {
                                TabularPerspectiveMeasure tpm = new TabularPerspectiveMeasure();
                                tpm.Name = m.Name;
                                tpm.Measure = m.Measure.Expression;

                                foreach (Microsoft.AnalysisServices.Tabular.Annotation an in m.Annotations)
                                {
                                    TabularAnnotation ta = new TabularAnnotation();
                                    ta.Name = an.Name;
                                    ta.Value = an.Value;
                                    tpm.Annotations.Add(ta);
                                }

                                tpt.Measures.Add(tpm);
                            }

                            foreach (Microsoft.AnalysisServices.Tabular.PerspectiveHierarchy ph in pt.PerspectiveHierarchies)
                            {
                                TabularPerspectiveHierarchy tph = new TabularPerspectiveHierarchy();
                                tph.Name = ph.Name;
                                foreach (var lev in ph.Hierarchy.Levels)
                                {
                                    TabularPerspectiveHierarchyLevel tphl = new TabularPerspectiveHierarchyLevel();
                                    tphl.Name = lev.Name;
                                    tph.HierarchyLevels.Add(tphl);
                                }

                                foreach (Microsoft.AnalysisServices.Tabular.Annotation an in ph.Annotations)
                                {
                                    TabularAnnotation ta = new TabularAnnotation();
                                    ta.Name = an.Name;
                                    ta.Value = an.Value;
                                    tph.Annotations.Add(ta);
                                }
                                tpt.Hierarchies.Add(tph);
                            }
                            foreach (Microsoft.AnalysisServices.Tabular.Annotation an in pt.Annotations)
                            {
                                TabularAnnotation ta = new TabularAnnotation();
                                ta.Name = an.Name;
                                ta.Value = an.Value;
                                tpt.Annotations.Add(ta);
                            }

                            tp.PerspectiveTables.Add(tpt);


                        }

                        tbm.Perspectives.Add(tp);
                    }



                    databases.Add(tdb);
                    models.Add(tbm);
                }
            }

              MakeJsons();
        }

            private void MakeJsons()
        {
            int i = 0;
            foreach (var item in databases)
            {
                var structuresSerialized = item.Serialize();
                var structureFileName = FileTools.NormalizeFileName(item.Name) + "_" + (i++).ToString() + ".json";
                File.WriteAllText(Path.Combine(_outputDirPath, structureFileName), structuresSerialized);
                Debug.WriteLine("Extract item ID " + _tabularProject.SsaslDbProjectComponentId);
                _manifest.Items.Add(new ManifestItem()
                {
                    ComponentId = _tabularProject.SsaslDbProjectComponentId,
                    Name = structureFileName,
                    ExtractType = item.ExtractType.ToString(),
                    RelativePath = Path.Combine(_relativePathBase, structureFileName)
                });
            }

            foreach (var item in models)
            {

                var structuresSerialized = item.Serialize();
                var structureFileName = FileTools.NormalizeFileName(item.Name) + "_" + (i++).ToString() + ".json";
                File.WriteAllText(Path.Combine(_outputDirPath, structureFileName), structuresSerialized);
                Debug.WriteLine("Extract idem ID " + _tabularProject.SsaslDbProjectComponentId);
                _manifest.Items.Add(new ManifestItem()
                {
                    ComponentId = _tabularProject.SsaslDbProjectComponentId,
                    Name = structureFileName,
                    ExtractType = item.ExtractType.ToString(),
                    RelativePath = Path.Combine(_relativePathBase, structureFileName)
                });
            }

        }

    }
}
