using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using CD.DLS.API.Structures;

namespace CD.DLS.Clients.Controls.Renderers
{
    class ReportLayoutRenderer : CanvasRenderer
    {

        private HashSet<int> _selectableElements = new HashSet<int>();
        private Dictionary<TextBlock, ReportElementAbsolutePosition> _positionMap = new Dictionary<TextBlock, ReportElementAbsolutePosition>();
        private TextBlock _currentlySelectedTb = null;

        public ReportLayoutRenderer()
            :base()
        {
        }
        public ReportLayoutRenderer(IEnumerable<int> selectableElements)
            :base()
        {
            _selectableElements = new HashSet<int>(selectableElements);
        }


        public ReportElementAbsolutePosition SelectedElement
        {
            get
            {
                if (_currentlySelectedTb == null)
                {
                    return null;
                }
                if (!_positionMap.ContainsKey(_currentlySelectedTb))
                {
                    return null;
                }
                return _positionMap[_currentlySelectedTb];
            }
        }

        public Canvas DrawReportCanvas(ReportElementAbsolutePosition positions)
        {
            ReportElementAbsolutePosition activePosition;
            return DrawReportCanvas(positions, null, out activePosition);
        }

        public Canvas DrawReportCanvas(ReportElementAbsolutePosition positions, string selectedRefPath, out ReportElementAbsolutePosition activePosition)
        {
            _positionMap = new Dictionary<TextBlock, ReportElementAbsolutePosition>();

            if (selectedRefPath == null)
            {
                selectedRefPath = "__NONE__";
            }

            //ScrollViewer scroll = new ScrollViewer();
            Canvas canvas = new Canvas();
            //scroll.Height = 500;
            //scroll.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            //scroll.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
            //scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            //scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            activePosition = DrawReportItemCanvas(canvas, positions, selectedRefPath);
            SetCanvasSize(canvas);
            return canvas;
            
            //scroll.Content = canvas;

            //return scroll;

        }
        

        private ReportElementAbsolutePosition DrawReportItemCanvas(Canvas canvas, ReportElementAbsolutePosition item, string selectedRefPath)
        {
            ReportElementAbsolutePosition activePosition = null;

            if (!string.IsNullOrEmpty(item.Expression) || !string.IsNullOrEmpty(item.Text))
            {
                TextBlock tb = new TextBlock();

                tb.Background = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { R = 225, G = 225, B = 225, A = 255 });// System.Windows.Media.Brushes.LightGray;

                if (!string.IsNullOrEmpty(item.Text) && item.Text != item.Expression)
                {
                    tb.Text = item.Text;
                    tb.SetValue(TextBlock.FontWeightProperty, System.Windows.FontWeights.Bold);
                }
                else if (item.Expression != null)
                {
                    tb.Text = item.Expression;
                }

                

                if (_selectableElements.Contains(item.ModelElementId))
                {
                    tb.Background = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { R = 200, G = 225, B = 200, A = 255 });
                    tb.PreviewMouseDown += TextBlck_PreviewMouseDown;
                }

                var positionLeft = item.Left * 100;
                var positionTop = item.Top * 40;

                if (item.Expression != null)
                {
                    //tb.Text = item.Expression;
                    if (selectedRefPath.StartsWith(item.RefPath))
                    {
                        tb.Background = System.Windows.Media.Brushes.Yellow;
                        Canvas.SetZIndex(tb, 100);
                        activePosition = new ReportElementAbsolutePosition()
                        {
                            Left = positionLeft,
                            Top = positionTop
                        };
                    }
                }
                Canvas.SetLeft(tb, positionLeft);
                Canvas.SetTop(tb, positionTop);
                //tb.Width = item.Width * 100;
                //tb.Height = item.Height * 100;
                //tb.Stroke = System.Windows.Media.Brushes.LightSlateGray;
                //tb.StrokeThickness = 1;

                _positionMap[tb] = item;

                canvas.Children.Add(tb);
            }
            else if (!double.IsNaN(item.Width) && !double.IsNaN(item.Height))
            {
                if (item.Width > 0 && item.Height > 0 && item.Type.Contains("TablixElement"))
                {
                    Rectangle rct = new Rectangle();
                    Canvas.SetLeft(rct, item.Left * 100);
                    Canvas.SetTop(rct, item.Top * 40);
                    rct.Width = item.Width * 100;
                    rct.Height = item.Height * 40;
                    rct.Stroke = System.Windows.Media.Brushes.Gray;
                    rct.StrokeDashArray = new System.Windows.Media.DoubleCollection(new double[] { 2, 4 });
                    rct.Fill = System.Windows.Media.Brushes.Transparent;
                    rct.Fill = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { R = 70, G = 130, B = 180, A = 64 });// System.Windows.Media.Brushes.SteelBlue;
                                                                                                                                               //rct.Fill.Opacity = 0.7;
                    rct.StrokeThickness = 1;
                    rct.StrokeLineJoin = System.Windows.Media.PenLineJoin.Miter;

                    canvas.Children.Add(rct);
                }
            }

            foreach (var child in item.Children)
            {
                var childActivePosition = DrawReportItemCanvas(canvas, child, selectedRefPath);
                if (activePosition == null)
                {
                    activePosition = childActivePosition;
                }
            }

            return activePosition;
        }

        private void TextBlck_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var tb = (TextBlock)sender;

            var selectedItem = _positionMap[tb];

            if (_currentlySelectedTb != null)
            {
                _currentlySelectedTb.Background = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color() { R = 200, G = 225, B = 200, A = 255 });
            }
            tb.Background = System.Windows.Media.Brushes.Yellow;
            _currentlySelectedTb = tb;

            if (CanvasItemSelected != null)
            {
                CanvasItemSelected(this, new CanvasItemArgs() { ElementId = selectedItem.ModelElementId, RefPath = selectedItem.RefPath, Type = selectedItem.Type });
            }
        }

        public class CanvasItemArgs : EventArgs
        {
            public string RefPath { get; set; }
            public int ElementId { get; set; }
            public string Type { get; set; }

        }

        public delegate void CanvasEventHandler(object sender, CanvasItemArgs e);

        public event CanvasEventHandler CanvasItemSelected;
    }
}
