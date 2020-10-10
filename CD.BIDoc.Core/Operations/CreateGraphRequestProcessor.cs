using CD.DLS.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Common.Structures;
using System.Threading;
using CD.DLS.API;
using Newtonsoft.Json;
using CD.DLS.Export;
using System.IO;
using System.IO.Compression;
using CD.DLS.Serialization;
using CD.DLS.DependencyGraph;
using CD.DLS.Interfaces.DependencyGraph;
using CD.DLS.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Export.Html.Mssql;
using CD.DLS.Export.Html;
using CD.DLS.DependencyGraph.Mssql.KnowledgeBase;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;

namespace CD.DLS.Operations
{


    internal class CreateGraphRequestProcessor : BIDocRequestProcessor<CreateGraphRequest>
    {
        private BIDocCore _core;
        public CreateGraphRequestProcessor(BIDocCore core) :
            base(core)
        {
            _core = core;
        }

        public override ProcessingResult ProcessRequest(CreateGraphRequest request, ProjectConfig projectConfig)
        {
            try
            {
                _core.IsBusy = true;
                // read model from database
                BIDocModelStored modelStored = new BIDocModelStored(projectConfig.ProjectConfigId);

                FromBIDocModelConverter converterFrom = new FromBIDocModelConverter(modelStored);
                IReflectionHelper reflection = new JsonReflectionHelper(new Model.Mssql.ModelActivator());
                Model.Mssql.ModelConverter converterTo = new Model.Mssql.ModelConverter(reflection);
                MssqlModelElement convertedModel = converterFrom.Convert(converterTo, modelStored.RootId);

                // init knowledge base
                GeneralKnowledgeBase kb;
                switch (request.KnowledgeBase)
                {
                    case CreateGraphRequest.KnowledgeBaseEnum.DataFlow:
                        kb = new DataFlowKnowledgeBase();
                        break;
                    default:
                        throw new Exception();
                }

                // build graph
                _core.Log.Important("Building dependency graph");
                var graph = kb.BuildGraph(convertedModel);
                graph.BuildNodeIndex();

                // save graph
                BIDocGraphBulk graphBulk = new BIDocGraphBulk();
                graphBulk.AddGraph(graph);
                graphBulk.UpdateGraph(projectConfig.ProjectConfigId, DependencyGraphKind.DataFlow);

                if (request.KnowledgeBase == CreateGraphRequest.KnowledgeBaseEnum.DataFlow)
                {

                    _core.Log.Important("Propagating dataflow vertically");
                    GraphManager.PropagateDataFlowVertically(projectConfig.ProjectConfigId);
                    _core.Log.Important("Building transitive dependency graph");
                    GraphManager.BuildTransitiveDataFlowGraph(projectConfig.ProjectConfigId);
                    _core.Log.Important("Building dataflow sequences");
                    GraphManager.BuildDataFlowSequences(projectConfig.ProjectConfigId);
                    _core.Log.Important("Propagating dataflow sequences to higher level nodes");
                    GraphManager.BuildHigherDataFlowSequences(projectConfig.ProjectConfigId);
                    _core.Log.Important("Clensing dataflow sequences");
                    GraphManager.ClenseDataFlowSequences(projectConfig.ProjectConfigId);

                    _core.Log.Important("Building high level dataflow graph");
                    GraphManager.BuildHighLevelGraph(projectConfig.ProjectConfigId);
                    _core.Log.Important("Setting descriptive element paths");
                    GraphManager.SetModelElementDescriptivePaths(projectConfig.ProjectConfigId);

                    _core.Log.Important("Finding and saving errors in dataflow");
                    GraphManager.FillDataMessages(projectConfig.ProjectConfigId);

                    _core.Log.Important("Building fulltext indexes");
                    SearchManager.IndexFulltext(projectConfig.ProjectConfigId);

                    _core.Log.Important("Creating meduim level dataflow graph");
                    GraphManager.CreateDataFlowMediumDetailGraph(projectConfig.ProjectConfigId);
                    _core.Log.Important("Creating low level dataflow graph");
                    GraphManager.CreateDataFlowLowDetailGraph(projectConfig.ProjectConfigId);

                    _core.Log.Important("Creating links betweeen Elements and Annotations");
                    AnnotationManager.CreateLinksAnnotationsAndModelElements(projectConfig.ProjectConfigId);
                }
                /**/

                //_core.Log.Important("Persisting graph");

                BIDocDocumentBulk documentBulk = new BIDocDocumentBulk();
                /*
                    _core.Log.Important("SQL export");
                    DbDocumentExporter mssqlDbFactory = new DbDocumentExporter(new MssqlDbHtmlGenerator(LinkModeEnum.Href));
                    var mssqlDbDocs = mssqlDbFactory.ExportDocuments(graph);
                    documentBulk.AddDocuments(mssqlDbDocs);
                    */
                /*
                    _core.Log.Important("MDX export");
                    MdxDocumentExporter mdxFactory = new MdxDocumentExporter(new MdxHtmlGenerator(LinkModeEnum.Href));
                    var mdxDocs = mdxFactory.ExportDocuments(graph);
                    documentBulk.AddDocuments(mdxDocs);
                    */
                documentBulk.UpdateDocuments(projectConfig.ProjectConfigId, DependencyGraphKind.DataFlow);

                //}
                _core.IsBusy = false;
                return new ProcessingResult()
                {
                    Content = string.Empty,
                    Attachments = null
                };

            }
            catch (ModelNotAvailableException mnae)
            {
                // The model is not available, because ExtractMetadataRequest has not been processed
                // TODO: VD: how it should be handled? Return message, or write to log?
                _core.IsBusy = false;
                return new ProcessingResult()
                {
                    Content = mnae.Message
                };
            }
        }

        /*
        public ProcessingResult ProcessRequest_Old(CreateGraphRequest request, ProjectConfig projectConfig)
        {
            try
            {
                using (var dbContext = new CDFrameworkContext())
                {
                    BIDocModelStored modelStored = new BIDocModelStored(dbContext, projectConfig.ProjectConfigId);

                    FromBIDocModelConverter converterFrom = new FromBIDocModelConverter(modelStored);
                    IReflectionHelper reflection = new JsonReflectionHelper(new Model.Mssql.ModelActivator());
                    Model.Mssql.ModelConverter converterTo = new Model.Mssql.ModelConverter(reflection);
                    MssqlModelElement convertedModel = converterFrom.Convert(converterTo);

                    MssqlDependencyGraphBuilder graphBuilder = new MssqlDependencyGraphBuilder();

                    IDependencyGraph dg = graphBuilder.BuildDependencyGraph(convertedModel);

                    BIDocDocumentBulk documentBulk = new BIDocDocumentBulk();

                    _core.Log.Info("SQL export");
                    DbDocumentExporter mssqlDbFactory = new DbDocumentExporter(new MssqlDbHtmlGenerator(LinkModeEnum.Span));
                    var mssqlDbDocs = mssqlDbFactory.ExportDocuments(dg);
                    documentBulk.AddDocuments(mssqlDbDocs);

                    _core.Log.Info("SSIS export");
                    SsisDocumentExporter ssisGraphFactory = new SsisDocumentExporter();
                    var ssisDocs = ssisGraphFactory.ExportDocuments(dg);
                    documentBulk.AddDocuments(ssisDocs);

                    documentBulk.UpdateDocuments(dbContext, projectConfig.ProjectConfigId);

                    BIDocGraphBulk graphBulk = new BIDocGraphBulk();
                    graphBulk.AddGraph(dg);
                    graphBulk.UpdateGraph(dbContext, projectConfig.ProjectConfigId);
                }

                return new ProcessingResult()
                {
                    Content = string.Empty,
                    Attachments = null
                };

            }
            catch (ModelNotAvailableException mnae)
            {
                // The model is not available, because ExtractMetadataRequest has not been processed
                // TODO: VD: how it should be handled? Return message, or write to log?
                return new ProcessingResult()
                {
                    Content = mnae.Message
                };
            }
        }
        */
    }
}
