using CD.DLS.API;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.SsisDiagram;
using CD.DLS.Model.Mssql.Ssis;
using System;
using System.Collections.Generic;
using CD.DLS.Model.Interfaces;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.API.Query;
using CD.DLS.DAL.Objects.BIDoc;
using CD.DLS.Model.Mssql;
using CD.DLS.DAL.Configuration;

namespace CD.DLS.RequestProcessor.Query
{
    public class ElementTechViewRequestProcessor : RequestProcessorBase, IRequestProcessor<ElementTechViewRequest, ElementTechViewResponse>
    {
        public ElementTechViewResponse Process(ElementTechViewRequest request, ProjectConfig projectConfig)
        {
            try
            {
                var nodeId = GraphManager.GetGraphNodeId(request.ElementId, DependencyGraphKind.DataFlow);
                var nodeExtended = InspectManager.GetGraphNodeExtended(nodeId);
                BIDocGraphInfoNodeExtended visualAncestorExtended = null;

                var visualAncestor = InspectManager.GetVisualNodeAncestor(nodeExtended.Id);
                if (visualAncestor.HasValue)
                {
                    visualAncestorExtended = InspectManager.GetGraphNodeExtended(visualAncestor.Value);
                }

                var requestResult = new ElementTechViewResponse();
                requestResult.RequestedElementId = request.ElementId;


                var nodeDescription = new NodeDescription()
                {
                    Definition = nodeExtended.Description,
                    //DenseTopologicalOrder = denseTopol,
                    ModelElementId = nodeExtended.SourceElementId,
                    Name = nodeExtended.Name,
                    NodeId = nodeExtended.Id,
                    NodeType = nodeExtended.NodeType,
                    TypeDescription = nodeExtended.TypeDescription,
                    //TopologicalOrder = topol,
                    RefPath = nodeExtended.RefPath,
                    //TypeClass = _elementTypeClasses[nodeExtended.ElementType],
                    DescriptivePath = nodeExtended.DescriptivePath
                };

                requestResult.NodeDescription = nodeDescription;

                if (visualAncestorExtended == null)
                {
                    return requestResult;
                }

                //Dictionary<int, MssqlModelElement> modelElementsById = new Dictionary<int, MssqlModelElement>();
                var elementIds = new List<int>() { nodeDescription.ModelElementId, visualAncestorExtended.SourceElementId }.Distinct().ToList();
                var modelElementsById = DeserializeElementsForDisplay(projectConfig.ProjectConfigId, elementIds);

                MssqlModelElement convertedModel = modelElementsById[nodeExtended.SourceElementId];
                MssqlModelElement visualConvertedModel = modelElementsById[visualAncestorExtended.SourceElementId];

                VisualNodeDescription visualNodeDescription = new VisualNodeDescription()
                {
                    Definition = visualAncestorExtended.Description,
                    //DenseTopologicalOrder = denseTopol,
                    ModelElementId = visualAncestorExtended.SourceElementId,
                    Name = visualAncestorExtended.Name,
                    NodeId = visualAncestorExtended.Id,
                    NodeType = visualAncestorExtended.NodeType,
                    RefPath = visualAncestorExtended.RefPath,
                    //TopologicalOrder = topol,
                    TypeDescription = visualAncestorExtended.TypeDescription,
                    TextDefinition = null,
                    VisualisationType = VisualisationEnum.Text,
                    DescriptivePath = visualAncestorExtended.DescriptivePath
                };

                if (visualConvertedModel is Model.Mssql.Db.SqlFragmentElement)
                {
                    var refSqlFragment = (Model.Mssql.Db.SqlFragmentElement)convertedModel;
                    var visSqlFragment = (Model.Mssql.Db.SqlFragmentElement)visualConvertedModel;

                    if (refSqlFragment.Length == 0 || refSqlFragment.OffsetFrom < visSqlFragment.OffsetFrom)
                    {
                        requestResult.NodeDescription = nodeDescription;
                    }
                    else
                    {
                        var visualPartNode = new VisualPartNodeDescription(nodeDescription);
                        visualPartNode.DefinitionOffset = refSqlFragment.OffsetFrom - visSqlFragment.OffsetFrom;
                        visualPartNode.DefinitionLength = refSqlFragment.Length - visSqlFragment.OffsetFrom;
                        visualPartNode.VisualNodeId = visualAncestorExtended.Id;
                        requestResult.NodeDescription = visualPartNode;

                        visualNodeDescription.TextDefinitionOffset = 0;
                        visualNodeDescription.TextDefinitionLength = visSqlFragment.Length;
                        requestResult.VisualAncestor = visualNodeDescription;

                    }
                }
                else if (visualConvertedModel is Model.Mssql.Ssas.MdxFragmentElement)
                {
                    var refMdxFragment = (Model.Mssql.Ssas.MdxFragmentElement)convertedModel;
                    var visMdxFragment = (Model.Mssql.Ssas.MdxFragmentElement)visualConvertedModel;

                    if (refMdxFragment.Length == 0 || refMdxFragment.OffsetFrom < visMdxFragment.OffsetFrom)
                    {
                        requestResult.NodeDescription = nodeDescription;
                    }
                    else
                    {
                        var visualPartNode = new VisualPartNodeDescription(nodeDescription);
                        visualPartNode.DefinitionOffset = refMdxFragment.OffsetFrom - visMdxFragment.OffsetFrom;
                        if (refMdxFragment.Length < visMdxFragment.Length)
                        {
                            visualPartNode.DefinitionLength = refMdxFragment.Length/* - visMdxFragment.OffsetFrom*/;
                        }
                        visualPartNode.VisualNodeId = visualAncestorExtended.Id;
                        requestResult.NodeDescription = visualPartNode;

                        visualNodeDescription.TextDefinitionOffset = 0;
                        visualNodeDescription.TextDefinitionLength = visMdxFragment.Length;
                        requestResult.VisualAncestor = visualNodeDescription;

                    }
                }
                else if (visualConvertedModel is Model.Mssql.Ssas.DaxFragmentElement)
                {
                    var refDaxFragment = (Model.Mssql.Ssas.DaxFragmentElement)convertedModel;
                    var visDaxFragment = (Model.Mssql.Ssas.DaxFragmentElement)visualConvertedModel;

                    if (refDaxFragment.Length == 0 || refDaxFragment.OffsetFrom < visDaxFragment.OffsetFrom)
                    {
                        requestResult.NodeDescription = nodeDescription;
                    }
                    else
                    {
                        var visualPartNode = new VisualPartNodeDescription(nodeDescription);
                        visualPartNode.DefinitionOffset = refDaxFragment.OffsetFrom - visDaxFragment.OffsetFrom;
                        if (refDaxFragment.Length < visDaxFragment.Length)
                        {
                            visualPartNode.DefinitionLength = refDaxFragment.Length/* - visMdxFragment.OffsetFrom*/;
                        }
                        visualPartNode.VisualNodeId = visualAncestorExtended.Id;
                        requestResult.NodeDescription = visualPartNode;

                        visualNodeDescription.TextDefinitionOffset = 0;
                        visualNodeDescription.TextDefinitionLength = visDaxFragment.Length;
                        requestResult.VisualAncestor = visualNodeDescription;

                    }
                }
                else if (visualConvertedModel is Model.Mssql.PowerQuery.MFragmentElement)
                {
                    var refPowerQueryFragment = (Model.Mssql.PowerQuery.MFragmentElement)convertedModel;
                    var visPowerQueryFragment = (Model.Mssql.PowerQuery.MFragmentElement)visualConvertedModel;

                    var deepestFragment = visPowerQueryFragment;
                    while (true)
                    {
                        var fragmentChild = deepestFragment.Children.FirstOrDefault(x => refPowerQueryFragment.RefPath.Path.StartsWith(x.RefPath.Path)
                            && x is Model.Mssql.PowerQuery.MFragmentElement
                            && ((Model.Mssql.PowerQuery.MFragmentElement)x).Length > 0);
                        if (fragmentChild == null)
                        {
                            break;
                        }
                        deepestFragment = (Model.Mssql.PowerQuery.MFragmentElement)fragmentChild;
                    }

                    visualNodeDescription.TextDefinitionOffset = 0;
                    visualNodeDescription.TextDefinitionLength = visPowerQueryFragment.Length;
                    requestResult.VisualAncestor = visualNodeDescription;

                    var visualPartNode = new VisualPartNodeDescription(nodeDescription);
                    if (deepestFragment != visPowerQueryFragment)
                    {
                        visualPartNode.DefinitionOffset = deepestFragment.OffsetFrom - visPowerQueryFragment.OffsetFrom;
                        if (deepestFragment.Length < visPowerQueryFragment.Length)
                        {
                            visualPartNode.DefinitionLength = deepestFragment.Length/* - visMdxFragment.OffsetFrom*/;
                        }
                    }
                    visualPartNode.VisualNodeId = visualAncestorExtended.Id;
                    requestResult.NodeDescription = visualPartNode;

                }
                else if (visualConvertedModel is Model.Mssql.Ssrs.ReportElement)
                {
                    //var refSsrsFragment = (Model.Mssql.Ssrs.SsrsExpressionFragmentElement)convertedModel;
                    var visReport = (Model.Mssql.Ssrs.ReportElement)visualConvertedModel;
                    ReportItemPositionsRequestProcessor positionsProcessor = new ReportItemPositionsRequestProcessor();
                    var positions = positionsProcessor.GetReportItemPositions(visReport);

                    var visualPartNode = new VisualPartNodeDescription(nodeDescription);
                    visualPartNode.DefinitionOffset = 0;
                    visualPartNode.DefinitionLength = 0;
                    visualPartNode.VisualNodeId = visualAncestorExtended.Id;

                    requestResult.NodeDescription = visualPartNode;

                    visualNodeDescription.VisualDefinition = positions;
                    visualNodeDescription.VisualisationType = VisualisationEnum.Xaml;
                    requestResult.VisualAncestor = visualNodeDescription;

                }
                else if (visualConvertedModel is Model.Mssql.Ssis.SsisExpressionFragmentElement)
                {
                    var refSsisFragment = (Model.Mssql.Ssis.SsisExpressionFragmentElement)convertedModel;
                    var visSsisFragment = (Model.Mssql.Ssis.SsisExpressionFragmentElement)visualConvertedModel;

                    if (refSsisFragment.Length == 0 || refSsisFragment.OffsetFrom < visSsisFragment.OffsetFrom)
                    {
                        requestResult.NodeDescription = nodeDescription;
                    }
                    else
                    {
                        var visualPartNode = new VisualPartNodeDescription(nodeDescription);
                        visualPartNode.DefinitionOffset = refSsisFragment.OffsetFrom - visSsisFragment.OffsetFrom;
                        visualPartNode.DefinitionLength = refSsisFragment.Length - visSsisFragment.OffsetFrom;
                        visualPartNode.VisualNodeId = visualAncestorExtended.Id;
                        requestResult.NodeDescription = visualPartNode;

                        visualNodeDescription.TextDefinitionOffset = 0;
                        visualNodeDescription.TextDefinitionLength = visSsisFragment.Length;
                        requestResult.VisualAncestor = visualNodeDescription;

                    }
                }
                else if (convertedModel is Model.Mssql.Db.ColumnElement)
                {

                    var visualPartNode = new VisualPartNodeDescription(nodeDescription);
                    visualPartNode.DefinitionOffset = 0;
                    visualPartNode.DefinitionLength = 0;
                    visualPartNode.VisualNodeId = visualAncestorExtended.Id;
                    requestResult.NodeDescription = visualPartNode;

                    visualNodeDescription.TextDefinitionOffset = 0;
                    visualNodeDescription.TextDefinitionLength = visualConvertedModel.Definition.Length;
                    requestResult.VisualAncestor = visualNodeDescription;


                    var refColumnElement = (Model.Mssql.Db.ColumnElement)convertedModel;
                    var visSqlFragment = (Model.Mssql.Db.DbScriptedElement)visualConvertedModel;

                    bool sqlDefAvailable = refColumnElement.SqlDefinition != null && visSqlFragment.SqlDefinition != null;

                    if (sqlDefAvailable)
                    {
                        var coverSqlDefinition = visSqlFragment.SqlDefinition;
                        var refSqlDefinition = refColumnElement.SqlDefinition;

                        visualPartNode.DefinitionOffset = refSqlDefinition.OffsetFrom - coverSqlDefinition.OffsetFrom;
                        visualPartNode.DefinitionLength = refSqlDefinition.Length - coverSqlDefinition.OffsetFrom;
                        visualNodeDescription.TextDefinitionOffset = 0;
                        visualNodeDescription.TextDefinitionLength = visSqlFragment.Definition.Length;

                    }

                }
                else if (convertedModel is Model.Mssql.Db.SchemaTableElement || convertedModel is Model.Mssql.Db.ViewElement || convertedModel is Model.Mssql.Db.ProcedureElement)
                {

                    var visualPartNode = new VisualPartNodeDescription(nodeDescription);
                    visualPartNode.DefinitionOffset = 0;
                    visualPartNode.DefinitionLength = 0;
                    visualPartNode.VisualNodeId = visualAncestorExtended.Id;
                    requestResult.NodeDescription = visualPartNode;

                    visualNodeDescription.TextDefinitionOffset = 0;
                    visualNodeDescription.TextDefinitionLength = visualConvertedModel.Definition.Length;
                    requestResult.VisualAncestor = visualNodeDescription;

                    var visSqlFragment = (Model.Mssql.Db.DbScriptedElement)visualConvertedModel;

                    nodeDescription.Name = nodeExtended.DescriptivePath;

                }
                else if (convertedModel is Model.Mssql.Ssis.DfColumnElement || convertedModel is Model.Mssql.Ssis.PackageElement)
                {
                    var package = (Model.Mssql.Ssis.PackageElement)visualConvertedModel;
                    var designBlock = GeneratePackageDesignBlockHierarchy(package);

                    var visualPartNode = new VisualPartNodeDescription(nodeDescription);
                    visualPartNode.DefinitionOffset = 0;
                    visualPartNode.DefinitionLength = 0;
                    visualPartNode.VisualNodeId = visualAncestorExtended.Id;

                    requestResult.NodeDescription = visualPartNode;

                    visualNodeDescription.VisualDefinition = designBlock;
                    visualNodeDescription.VisualisationType = VisualisationEnum.Xaml;
                    requestResult.VisualAncestor = visualNodeDescription;


                }
                else
                {
                    requestResult.NodeDescription = nodeDescription;
                }

                requestResult.ElementId = request.ElementId;
                
                return requestResult;
            }
            catch(Exception e)
            {
                LogException(e);
                throw;
            }
        }



        private DesignBlock GeneratePackageDesignBlockHierarchy(DesignBlockElement designBlock)
        {
            var res = CreateDesignBlock(designBlock);
            if (res == null)
            {
                return null;
            }
            List<Model.Mssql.Ssis.DesignBlockElement> childBlocks = new List<Model.Mssql.Ssis.DesignBlockElement>();
            var childLevelElements = designBlock.Children;
            while (childLevelElements.Any() && !childLevelElements.Any(x => x is DesignBlockElement))
            {
                childLevelElements = childLevelElements.SelectMany(x => x.Children);
            }

            childBlocks = childLevelElements.Where(x => x is DesignBlockElement).Select(x => x as Model.Mssql.Ssis.DesignBlockElement).ToList();
            foreach (var ch in childBlocks)
            {
                if (ch is Model.Mssql.Ssis.DfPathElement || ch is Model.Mssql.Ssis.PrecedenceConstraintElement)
                {
                    continue;
                }
                var childBlock = GeneratePackageDesignBlockHierarchy(ch);
                res.ChildBlocks.Add(childBlock);
            }

            return res;
        }

        private DesignBlock CreateDesignBlock(DesignBlockElement element)
        {
            if (element.Position == null || element.Size == null)
            {
                return null;
            }
            var res = new DesignBlock()
            {
                Arrows = new List<DAL.Objects.SsisDiagram.DesignArrow>(),
                ChildBlocks = new List<DesignBlock>(),
                ElementId = element.Id,
                ElementType = element.GetType().FullName,
                Name = element.Caption,
                Position = new DAL.Objects.SsisDiagram.DesignPoint()
                {
                    X = element.Position.X,
                    Y = element.Position.Y
                },
                RefPath = element.RefPath.Path,
                Size = new DAL.Objects.SsisDiagram.DesignPoint()
                {
                    X = element.Size.X,
                    Y = element.Size.Y
                }
            };

            var precedenceConstraints = element.Children.OfType<Model.Mssql.Ssis.PrecedenceConstraintElement>();
            var dfPaths = element.Children.OfType<Model.Mssql.Ssis.DfPathElement>();

            foreach (var pc in precedenceConstraints)
            {
                res.Arrows.Add(new DAL.Objects.SsisDiagram.DesignArrow()
                {
                    TopLeft = new DAL.Objects.SsisDiagram.DesignPoint()
                    {
                        X = pc.Arrow.TopLeft.X,
                        Y = pc.Arrow.TopLeft.Y
                    },
                    Shifts = pc.Arrow.Shifts.Select(x => new DAL.Objects.SsisDiagram.DesignPoint()
                    {
                        X = x.X,
                        Y = x.Y
                    }).ToList()
                });
            }

            foreach (var path in dfPaths)
            {
                res.Arrows.Add(new DAL.Objects.SsisDiagram.DesignArrow()
                {
                    TopLeft = new DAL.Objects.SsisDiagram.DesignPoint()
                    {
                        X = path.Arrow.TopLeft.X,
                        Y = path.Arrow.TopLeft.Y
                    },
                    Shifts = path.Arrow.Shifts.Select(x => new DAL.Objects.SsisDiagram.DesignPoint()
                    {
                        X = x.X,
                        Y = x.Y
                    }).ToList()
                });
            }

            return res;
        }
    }
}
