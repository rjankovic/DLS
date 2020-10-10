using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using CD.DLS.DAL.Objects.SsisDiagram;

namespace CD.DLS.Clients.Controls.Renderers
{
    class SsisPackageRenderer : CanvasRenderer
    {

        public TabControl DrawSsisPackage(DesignBlock packageVisualisation, string selectedRefPath)
        {
            TabControl tc = new TabControl();

            TabItem cfTab = new TabItem() { Header = "Control Flow" };
            tc.Items.Add(cfTab);
            Canvas cfCanvas = new Canvas();
            DrawDesignBlock(cfCanvas, new DesignPoint() { X = 0, Y = 0 }, packageVisualisation, selectedRefPath, SsisDiagramColorScheme.ControlFlow);
            SetCanvasSize(cfCanvas);

            //cfTab.Content = cfScroll;
            ScrollViewer cfScroll = new ScrollViewer();
            //cfScroll.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            //cfScroll.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
            cfScroll.Content = cfCanvas;
            //cfScroll.Height = 500;
            cfScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            cfScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            //cfTab.Content = cfCanvas;
            cfTab.Content = cfScroll;

            if (selectedRefPath.Contains("/Input") || selectedRefPath.Contains("/Output"))
            {
                var dfWrapBlock = packageVisualisation;
                while (dfWrapBlock.ElementType != "CD.DLS.Model.Mssql.Ssis.DfInnerElement")
                {
                    dfWrapBlock = dfWrapBlock.ChildBlocks.First(x => selectedRefPath.StartsWith(x.RefPath) || x.RefPath.EndsWith("/DataFlow"));
                }

                TabItem dfTab = new TabItem() { Header = "Data Flow" };
                tc.Items.Add(dfTab);
                tc.SelectedItem = dfTab;

                Canvas dfCanvas = new Canvas();
                DrawDesignBlock(dfCanvas, new DesignPoint() { X = 0, Y = 0 }, dfWrapBlock, selectedRefPath, SsisDiagramColorScheme.DataFlow);
                SetCanvasSize(dfCanvas);

                ScrollViewer dfScroll = new ScrollViewer();
                //dfScroll.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                //dfScroll.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
                dfScroll.Content = dfCanvas;
                //dfScroll.Height = 500;
                dfScroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                dfScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;

                dfTab.Content = dfScroll;
                //dfTab.Content = dfCanvas;
            }
            
            return tc;
        }



        private enum SsisDiagramColorScheme { ControlFlow, DataFlow }

        private void DrawDesignBlock(Canvas canvas, DesignPoint offset, DesignBlock designBlock, string selectedrefpath, SsisDiagramColorScheme colorScheme)
        {
            foreach (var childBlock in designBlock.ChildBlocks)
            {
                var childRect = new Rectangle();
                Canvas.SetLeft(childRect, offset.X + childBlock.Position.X);
                Canvas.SetTop(childRect, offset.Y + childBlock.Position.Y);
                childRect.Width = childBlock.Size.X;
                childRect.Height = childBlock.Size.Y;
                childRect.Fill = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { R = 225, G = 225, B = 225, A = 255 });// System.Windows.Media.Brushes.LightGray;
                childRect.Stroke = System.Windows.Media.Brushes.LightSlateGray;
                childRect.StrokeThickness = 1;
                bool isContainer = childBlock.ElementType.Contains("Container");
                if (!isContainer && selectedrefpath.StartsWith(childBlock.RefPath))
                {
                    childRect.Fill = System.Windows.Media.Brushes.LightYellow;
                }
                canvas.Children.Add(childRect);

                TextBlock chBlockText = new TextBlock();
                Canvas.SetLeft(chBlockText, offset.X + childBlock.Position.X + 10);
                Canvas.SetTop(chBlockText, offset.Y + childBlock.Position.Y + 10);
                chBlockText.Text = childBlock.Name;
                chBlockText.TextWrapping = System.Windows.TextWrapping.Wrap;
                chBlockText.Width = childBlock.Size.X - 20;
                canvas.Children.Add(chBlockText);

                bool isDfTask = childBlock.ElementType.Contains("DfTaskElement");
                if (!isDfTask)
                {
                    DrawDesignBlock(canvas, new DesignPoint() { X = offset.X + childBlock.Position.X, Y = offset.Y + childBlock.Position.Y + 20 }, childBlock, selectedrefpath, colorScheme);
                }
            }

            foreach (var arrow in designBlock.Arrows)
            {
                Polyline line = new Polyline();
                System.Windows.Media.PointCollection pointCollection = new System.Windows.Media.PointCollection();

                var startPoint = new System.Windows.Point() { X = 0, Y = 0 };
                pointCollection.Add(startPoint);


                DesignPoint linePos = new DesignPoint() { X = 0, Y = 0 };
                for (int i = 0; i < arrow.Shifts.Count - 1; i++)
                {
                    var shift = arrow.Shifts[i];
                    var newPoint = new System.Windows.Point() { X = linePos.X + shift.X, Y = linePos.Y + shift.Y };
                    pointCollection.Add(newPoint);
                    linePos.X = (float)(newPoint.X);
                    linePos.Y = (float)(newPoint.Y);
                }
                line.Points = pointCollection;

                var endPoint = new Polygon();
                endPoint.Points = new System.Windows.Media.PointCollection();
                var arrowSize = arrow.Shifts.Last().X;
                if (arrowSize == 0)
                {
                    arrowSize = arrow.Shifts.Last().Y;
                }
                switch (arrow.PointOrientation)
                {
                    case DesignArrow.PointOrientationEnum.Down:
                    case DesignArrow.PointOrientationEnum.Up:
                        endPoint.Points.Add(new System.Windows.Point() { X = linePos.X - arrowSize / 2, Y = linePos.Y });
                        endPoint.Points.Add(new System.Windows.Point() { X = linePos.X + arrowSize / 2, Y = linePos.Y });
                        endPoint.Points.Add(new System.Windows.Point() { X = linePos.X, Y = linePos.Y + arrowSize });
                        break;
                    case DesignArrow.PointOrientationEnum.Left:
                    case DesignArrow.PointOrientationEnum.Right:
                        endPoint.Points.Add(new System.Windows.Point() { Y = linePos.Y - arrowSize / 2, X = linePos.X });
                        endPoint.Points.Add(new System.Windows.Point() { Y = linePos.Y + arrowSize / 2, X = linePos.X });
                        endPoint.Points.Add(new System.Windows.Point() { Y = linePos.Y, X = linePos.X + arrowSize });
                        break;

                }

                if (colorScheme == SsisDiagramColorScheme.ControlFlow)
                {
                    line.Stroke = System.Windows.Media.Brushes.DarkGreen;
                    endPoint.Fill = System.Windows.Media.Brushes.DarkGreen;
                }
                else if (colorScheme == SsisDiagramColorScheme.DataFlow)
                {
                    line.Stroke = System.Windows.Media.Brushes.SteelBlue;
                    endPoint.Fill = System.Windows.Media.Brushes.SteelBlue;
                }
                line.StrokeThickness = 3;
                //line.StrokeEndLineCap = System.Windows.Media.PenLineCap.Triangle;
                Canvas.SetLeft(line, offset.X + arrow.TopLeft.X);
                Canvas.SetTop(line, offset.Y + arrow.TopLeft.Y);
                Canvas.SetLeft(endPoint, offset.X + arrow.TopLeft.X);
                Canvas.SetTop(endPoint, offset.Y + arrow.TopLeft.Y);
                canvas.Children.Add(line);
                canvas.Children.Add(endPoint);
            }
        }

    }
}
