using CD.DLS.Serialization;
using CD.DLS.API;
using CD.DLS.Common.Structures;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;
using System;

namespace CD.DLS.Operations
{
    internal class ElementTechViewRequestProcessor : BIDocRequestProcessor<ElementTechViewRequest>
    {

        
        public ElementTechViewRequestProcessor(BIDocCore core) : base(core)
        {
        }
        
        private DLS.DAL.Objects.SsisDiagram.DesignBlock GeneratePackageDesignBlockHierarchy(Model.Mssql.Ssis.DesignBlockElement designBlock)
        {
            var res = CreateDesignBlock(designBlock);
            List<Model.Mssql.Ssis.DesignBlockElement> childBlocks = new List<Model.Mssql.Ssis.DesignBlockElement>();
            var childLevelElements = designBlock.Children;
            while (childLevelElements.Any() && !childLevelElements.Any(x => x is Model.Mssql.Ssis.DesignBlockElement))
            {
                childLevelElements = childLevelElements.SelectMany(x => x.Children);
            }

            childBlocks = childLevelElements.Where(x => x is Model.Mssql.Ssis.DesignBlockElement).Select(x => x as Model.Mssql.Ssis.DesignBlockElement).ToList();
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

        private DLS.DAL.Objects.SsisDiagram.DesignBlock CreateDesignBlock(Model.Mssql.Ssis.DesignBlockElement element)
        {
            var res = new DLS.DAL.Objects.SsisDiagram.DesignBlock()
            {
                Arrows = new List<DLS.DAL.Objects.SsisDiagram.DesignArrow>(),
                ChildBlocks = new List<DLS.DAL.Objects.SsisDiagram.DesignBlock>(),
                ElementId = element.Id,
                ElementType = element.GetType().FullName,
                Name = element.Caption,
                Position = new DLS.DAL.Objects.SsisDiagram.DesignPoint()
                {
                    X = element.Position.X,
                    Y = element.Position.Y
                },
                RefPath = element.RefPath.Path,
                Size = new DLS.DAL.Objects.SsisDiagram.DesignPoint()
                {
                    X = element.Size.X,
                    Y = element.Size.Y
                }
            };

            var precedenceConstraints = element.Children.OfType<Model.Mssql.Ssis.PrecedenceConstraintElement>();
            var dfPaths = element.Children.OfType<Model.Mssql.Ssis.DfPathElement>();

            foreach (var pc in precedenceConstraints)
            {
                res.Arrows.Add(new DLS.DAL.Objects.SsisDiagram.DesignArrow()
                {
                    TopLeft = new DLS.DAL.Objects.SsisDiagram.DesignPoint()
                    {
                        X = pc.Arrow.TopLeft.X,
                        Y = pc.Arrow.TopLeft.Y
                    },
                    Shifts = pc.Arrow.Shifts.Select(x => new DLS.DAL.Objects.SsisDiagram.DesignPoint()
                    {
                        X = x.X,
                        Y = x.Y
                    }).ToList()
                });
            }

            foreach (var path in dfPaths)
            {
                res.Arrows.Add(new DLS.DAL.Objects.SsisDiagram.DesignArrow()
                {
                    TopLeft = new DLS.DAL.Objects.SsisDiagram.DesignPoint()
                    {
                        X = path.Arrow.TopLeft.X,
                        Y = path.Arrow.TopLeft.Y
                    },
                    Shifts = path.Arrow.Shifts.Select(x => new DLS.DAL.Objects.SsisDiagram.DesignPoint()
                    {
                        X = x.X,
                        Y = x.Y
                    }).ToList()
                });
            }

            return res;
        }

        public override ProcessingResult ProcessRequest(ElementTechViewRequest request, ProjectConfig projectConfig)
        {
            var nodeId = GraphManager.GetGraphNodeId(request.ElementId, DependencyGraphKind.DataFlow);
            var nodeExtended = InspectManager.GetGraphNodeExtended(nodeId);
            BIDocGraphInfoNodeExtended visualAncestorExtended = null;

            var visualAncestor = InspectManager.GetVisualNodeAncestor(nodeExtended.Id);
            if (visualAncestor.HasValue)
            {
                visualAncestorExtended = InspectManager.GetGraphNodeExtended(visualAncestor.Value);
            }

            var requestResult = new ElementTechViewRequestResponse();


            
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
                var stringResult1 = requestResult.Serialize();

                return new ProcessingResult()
                {
                    Content = stringResult1,
                    Attachments = new List<Attachment>()
                };
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

            else if (visualConvertedModel is Model.Mssql.Ssrs.ReportElement)
            {
                var refSsrsFragment = (Model.Mssql.Ssrs.SsrsExpressionFragmentElement)convertedModel;
                var visReport = (Model.Mssql.Ssrs.ReportElement)visualConvertedModel;
                var positions = GetReportItemPositionsRequestProcessor.GetReportItemPositions(visReport);

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


            var stringResult = requestResult.Serialize();

            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = new List<Attachment>()
            };
        }
    }
}
