using CD.DLS.API;
using CD.DLS.API.Structures;
using CD.DLS.DAL.Objects.SsisDiagram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CD.DLS.Clients.Web.Models.TechView
{
    public class ElementTechViewRenderer
    {
        public ElementTechViewVisual Render(VisualPartNodeDescription visualPartNodeDesc, VisualNodeDescription visualNode)
        {
            switch (visualNode.VisualisationType)
            {
                case VisualisationEnum.Text:

                    var visual = new TechViewCodeVisual()
                    {
                        VisualType = VisualTypeEnum.Code.ToString(),
                        Text = visualNode.Definition
                    };

                    if (visualPartNodeDesc.DefinitionLength > 0
                        && visualNode.TextDefinitionOffset <= visualPartNodeDesc.DefinitionOffset
                        && visualNode.TextDefinitionOffset + visualNode.TextDefinitionLength >= visualPartNodeDesc.DefinitionOffset + visualPartNodeDesc.DefinitionLength)
                    {
                        var start = visualPartNodeDesc.DefinitionOffset - visualNode.TextDefinitionOffset;
                        var len = visualPartNodeDesc.DefinitionLength;
                        visual.HighlightFrom = start;
                        visual.HighlightLength = len;
                    }
                    return visual;
                case VisualisationEnum.Xaml:
                    if (visualNode.NodeType == "PackageElement")
                    {
                        var selectedRefPath = visualPartNodeDesc.RefPath;
                        var packageVisualisation = (DesignBlock)visualNode.VisualDefinition;
                        var techViewVisual = new TechViewSsisVisual() { VisualType = VisualTypeEnum.Ssis.ToString() };
                        SsisFlowCanvas cfCanvas = new SsisFlowCanvas()
                        {
                            Blocks = new List<SsisVisualBlock>(),
                            Arrows = new List<SsisVisualArrow>()
                        };
                        RenderSsisBlock(packageVisualisation, cfCanvas, 0, 0, selectedRefPath);
                        cfCanvas.Width = cfCanvas.Blocks.Max(x => x.Width + x.Position.X);
                        cfCanvas.Height = cfCanvas.Blocks.Max(x => x.Height + x.Position.Y);
                        techViewVisual.ControlFlow = cfCanvas;
                        
                        if (selectedRefPath.Contains("/Input") || selectedRefPath.Contains("/Output"))
                        {
                            var dfWrapBlock = packageVisualisation;
                            while (dfWrapBlock.ElementType != "CD.DLS.Model.Mssql.Ssis.DfInnerElement")
                            {
                                dfWrapBlock = dfWrapBlock.ChildBlocks.First(x => selectedRefPath.StartsWith(x.RefPath) || x.RefPath.EndsWith("/DataFlow"));
                            }

                            SsisFlowCanvas dfCanvas = new SsisFlowCanvas()
                            {
                                Blocks = new List<SsisVisualBlock>(),
                                Arrows = new List<SsisVisualArrow>()
                            };
                            RenderSsisBlock(dfWrapBlock, dfCanvas, 0, 0, selectedRefPath);
                            dfCanvas.Width = dfCanvas.Blocks.Max(x => x.Width + x.Position.X);
                            dfCanvas.Height = dfCanvas.Blocks.Max(x => x.Height + x.Position.Y);
                            techViewVisual.DataFlow = dfCanvas;
                        }
                        
                        return techViewVisual;
                    }
                    else if (visualNode.NodeType == "ReportElement")
                    {
                        var reportVisualisation = (ReportElementAbsolutePosition)visualNode.VisualDefinition;
                        var selectedRefPath = visualPartNodeDesc.RefPath;
                        var gridStructure = new ReportGridStructure(reportVisualisation, selectedRefPath);
                        var reportVisual = new TechViewReportVisual()
                        {
                            VisualType = VisualTypeEnum.Report.ToString(),
                            Structure = gridStructure
                        };

                        return reportVisual;
                    }
                    else
                    {
                        throw new Exception();
                    }
                default:
                    return new ElementTechViewVisual() { VisualType = VisualTypeEnum.None.ToString() };
                    //throw new NotImplementedException();
            }
        }

        private void RenderSsisBlock(DesignBlock designBlock, SsisFlowCanvas canvas, double offsetX, double offsetY, string selectedRefPath)
        {
            foreach (var childBlock in designBlock.ChildBlocks)
            {
                var block = new SsisVisualBlock()
                {
                    Height = childBlock.Size.Y,
                    Width = childBlock.Size.X,
                    Name = childBlock.Name,
                    Position = new VisualPoint() { X = childBlock.Position.X + offsetX, Y = childBlock.Position.Y + offsetY },
                    RefPath = childBlock.RefPath,
                    ModelElementId = childBlock.ElementId,
                    Highlighted = selectedRefPath.StartsWith(childBlock.RefPath)
                };
                if (block.Highlighted)
                {
                    foreach (var childBlock2 in canvas.Blocks)
                    {
                        childBlock2.Highlighted = false;
                    }
                }
                canvas.Blocks.Add(block);
                
                bool isDfTask = childBlock.ElementType.Contains("DfTaskElement");
                if (!isDfTask)
                {
                    RenderSsisBlock(childBlock, canvas, offsetX + childBlock.Position.X, offsetY + childBlock.Position.Y + 20, selectedRefPath);
                }
            }

            foreach (var arrow in designBlock.Arrows)
            {
                var visualArrow = new SsisVisualArrow()
                {
                    Position = new VisualPoint()
                    {
                        X = arrow.TopLeft.X + offsetX,
                        Y = arrow.TopLeft.Y + offsetY
                    },
                    Path = new List<VisualPoint>() { new VisualPoint() { X = 0, Y = 0 } }
                };

                var linePos = new VisualPoint { X = 0, Y = 0 };
                for (int i = 0; i < arrow.Shifts.Count - 1; i++)
                {
                    var shift = arrow.Shifts[i];
                    var newPoint = new VisualPoint { X = linePos.X + shift.X, Y = linePos.Y + shift.Y };
                    visualArrow.Path.Add(newPoint);
                    linePos.X = (float)(newPoint.X);
                    linePos.Y = (float)(newPoint.Y);
                }

                canvas.Arrows.Add(visualArrow);
                
            }
        }
    }
}