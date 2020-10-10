using CD.DLS.API;
using CD.DLS.API.Structures;
using CD.DLS.DAL.Objects.SsisDiagram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CD.DLS.Clients.Controls.Renderers
{
    public static class VisualPartNodeRenderer
    {
        public static Control Render(VisualPartNodeDescription visualPartNodeDesc, VisualNodeDescription visualNode)
        {
            
            switch (visualNode.VisualisationType)
            {
                case VisualisationEnum.Text:
                    //var codeBlock = new RichTextBox();

                    FlowDocument fd = new FlowDocument();

                    fd.PageWidth = 1000;
                    //codeBlock.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

                    //codeBlock.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    //codeBlock.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

                    TextRange tr = null;

                    fd.Blocks.Add(new Paragraph(new Run(visualNode.Definition)));
                    if (visualPartNodeDesc.DefinitionLength > 0
                        && visualNode.TextDefinitionOffset <= visualPartNodeDesc.DefinitionOffset
                        && visualNode.TextDefinitionOffset + visualNode.TextDefinitionLength >= visualPartNodeDesc.DefinitionOffset + visualPartNodeDesc.DefinitionLength)
                    {
                        var start = visualPartNodeDesc.DefinitionOffset - visualNode.TextDefinitionOffset;
                        var len = visualPartNodeDesc.DefinitionLength;
                        var textPrefix = visualNode.Definition.Substring(0, start + len);
                        var lineCount = textPrefix.Split('\n').Length;
                        var richTextOffset = 2; // lineCount * 2;
                        var range = new TextRange(fd.ContentStart.GetPositionAtOffset(start + richTextOffset),
                            fd.ContentStart.GetPositionAtOffset(start + len + richTextOffset));
                        //var prefixRange = new TextRange(codeBlock.Document.ContentStart, codeBlock.Document.ContentStart.GetPositionAtOffset(start + len));
                        range.ApplyPropertyValue(TextElement.BackgroundProperty, System.Windows.Media.Brushes.Yellow);
                        tr = range;
                    }
                    fd.PagePadding = new System.Windows.Thickness(5);
                    //codeBlock.FontFamily = new System.Windows.Media.FontFamily("Lucida Console");
                    fd.FontFamily = new System.Windows.Media.FontFamily("Lucida Console");
                    fd.FontSize = 12;
                    FlowDocumentScrollViewer textViewer = new FlowDocumentScrollViewer();
                    textViewer.Document = fd;
                    //var scrollViewer = textViewer.FindScrollViewer();
                    
                    return textViewer;

                    
                    break;
                case VisualisationEnum.Xaml:
                    if (visualNode.NodeType == "PackageElement")
                    {
                        var packageVisualisation = (DesignBlock)visualNode.VisualDefinition;
                        var selectedRefPath = visualPartNodeDesc.RefPath;
                        SsisPackageRenderer ssisRenderer = new SsisPackageRenderer();
                        Control packageVisualizationControl = ssisRenderer.DrawSsisPackage(packageVisualisation, selectedRefPath);
                        //packageVisualizationControl.MaxWidth = 1000; // innerStack.Width;
                        packageVisualizationControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;// .Stretch;

                        return packageVisualizationControl;
                        //ScrollViewer pkgViewer = new ScrollViewer();
                        //pkgViewer.Content = packageVisualizationControl;
                        //return pkgViewer;

                        //targetScrollViewer.Content = packageVisualizationControl;

                    }
                    else if (visualNode.NodeType == "ReportElement")
                    {
                        var reprtVisualisation = (ReportElementAbsolutePosition)visualNode.VisualDefinition;
                        var selectedRefPath = visualPartNodeDesc.RefPath;
                        //Grid packageVisualizationControl = DrawReport(reprtVisualisation, selectedRefPath);
                        ReportLayoutRenderer ssrsRenderer = new ReportLayoutRenderer();
                        //if (_selectableTargets != null)
                        //{
                        //    ssrsRenderer = new ReportLayoutRenderer(_selectableTargets);
                        //}

                        ReportElementAbsolutePosition activePosition;
                        var reportVisualizationControl = ssrsRenderer.DrawReportCanvas(reprtVisualisation, selectedRefPath, out activePosition);
                        //reportVisualizationControl.MaxWidth = 1000; // innerStack.Width;
                        reportVisualizationControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; //.Stretch;
                        ScrollViewer reportViewer = new ScrollViewer();
                        reportViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                        reportViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                        reportViewer.Content = reportVisualizationControl;
                        if (activePosition != null)
                        {
                            reportViewer.ScrollToHorizontalOffset(Math.Max(activePosition.Left - 100, 0));
                            reportViewer.ScrollToVerticalOffset(Math.Max(activePosition.Top - 100, 0));
                        }

                        return reportViewer;


                        //targetScrollViewer.Content = reportVisualizationControl;

                    }
                    else
                    {
                        return null;
                    }
                    break;
                default:
                    throw new Exception();
            }
            
        }

    }
}
