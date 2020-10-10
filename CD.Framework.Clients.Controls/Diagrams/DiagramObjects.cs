using CD.DLS.Clients.Controls.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CD.DLS.Clients.Controls.Diagrams
{
    public enum ConnectorPosition
    {
        Left, Top, Right, Bottom
    }
    
    public class DiagramNodeEventArgs : EventArgs
    {
        public DiagramNode Node { get; set; }
    }

    public delegate void DiagramNodeEventHander(object sender, DiagramNodeEventArgs e);

    public class Diagram
    {
        public List<DiagramNode> Nodes { get; set; }
        public List<DiagramLink> Links { get; set; }
        public int MaxLinkStrength { get; set; }
        public Canvas Canvas { get; set; }
        public bool SelectableNodes { get; set; }

        private DiagramNode _draggedObject = null;
        private DiagramNode _selectedNode = null;

        private double _dragMouseFromX = 0;
        private double _dragMouseFromY = 0;
        private double _dragObjectFromX = 0;
        private double _dragObjectFromY = 0;
        
        public Diagram()
        {
            Nodes = new List<DiagramNode>();
            Links = new List<DiagramLink>();
            MaxLinkStrength = 0;
            Canvas = new Canvas()
            {
                Background = Brushes.Transparent
            };
        }
    
        public event DiagramNodeEventHander NodeSelected;
        public event DiagramNodeEventHander NodeRightClick;
        public event DiagramNodeEventHander NodeDoubleClick; 

        public DiagramNode SelectedNode
        {
            get { return _selectedNode; }
        }

        public DiagramNode AddNode(int id, string name, string typeDescription)
        {
            var n = new DiagramNode(id, this, name, typeDescription);
            n.NodeSelected += OnNodeSelected;
            n.NodeRightClick += OnNodeRightClick;
            Nodes.Add(n);
            n.NodeDoubleClick += N_NodeDoubleClick;
            return n;
        }

        private void N_NodeDoubleClick(object sender, DiagramNodeEventArgs e)
        {
            if (NodeDoubleClick != null)
            {
                NodeDoubleClick(sender, e);
            }
        }

        public DiagramLink AddLink(int id, DiagramNode source, DiagramNode target, int strength)
        {
            if (strength > MaxLinkStrength)
            {
                MaxLinkStrength = strength;
            }
            var l = new DiagramLink(id, source, target, this);
            l.Strength = strength;
            Links.Add(l);
            return l;
        }

        public void RemoveLink(int id)
        {
            var linkToRemove = Links.First(x => x.Id == id);
            RemoveLink(linkToRemove);
        }

        public void RemoveLink(DiagramLink linkToRemove)
        {
            linkToRemove.RemoveShape();
            var nodeFrom = linkToRemove.FromNode;
            var nodeTo = linkToRemove.ToNode;
            nodeFrom.DetachLink(linkToRemove);
            nodeTo.DetachLink(linkToRemove);
            Links.Remove(linkToRemove);
        }

        public void RemoveNode(int id, out List<DiagramLink> removedLinks)
        {
            var nodeToRemove = Nodes.First(x => x.Id == id);
            RemoveNode(nodeToRemove, out removedLinks);
        }
        
        public void RemoveNode(DiagramNode nodeToRemove, out List<DiagramLink> removedLinks)
        {
            removedLinks = new List<DiagramLink>();
            while (nodeToRemove.Links.Any())
            {
                var lnk = nodeToRemove.Links.First();
                removedLinks.Add(lnk);
                RemoveLink(lnk);
            }
            nodeToRemove.RemoveShape();
            Nodes.Remove(nodeToRemove);
        }

        public void RemoveNode(DiagramNode nodeToRemove)
        {
            List<DiagramLink> dummy;
            if (_selectedNode == nodeToRemove)
            {
                _selectedNode = null;
            }
            RemoveNode(nodeToRemove, out dummy);
        }

        public void RemoveNode(int id)
        {
            var nodeToRemove = Nodes.First(x => x.Id == id);
            List<DiagramLink> dummy;
            RemoveNode(nodeToRemove, out dummy);
        }
        
        public DiagramNode GetNode(int id)
        {
            return Nodes.FirstOrDefault(x => x.Id == id);
        }
        public DiagramLink GetLink(int id)
        {
            return Links.FirstOrDefault(x => x.Id == id);
        }

        public enum DiagramArrangementDirection { Horizontal, Vertical };




        public void ResetArrangement()
        {
            foreach (var node in Nodes)
            {
                node.IsArranged = false;
            }
        }

        public void ArrangeDiagram(DiagramArrangementDirection direction, bool clickHide = false)
        {
            //// in vertical order, an uniform width is desirable
            //if (direction == DiagramArrangementDirection.Vertical)
            //{
            //    double maxWidth = 0;
            //    foreach (var node in Nodes)
            //    {
            //        if (node.Width > maxWidth)
            //        {
            //            maxWidth = node.Width;
            //        }
            //    }

            //    foreach (var node in Nodes)
            //    {
            //        node.Width = maxWidth;
            //    }
            //}

            // reduce the links to an acyclic graph with minimal losses so that topological ordering is possible
            var topologyLinks = new List<DiagramLink>(Links);
            DiagramLink cyclicLink = FindWeakCyclicLink(topologyLinks);
            while (cyclicLink != null)
            {
                topologyLinks.Remove(cyclicLink);
                cyclicLink = FindWeakCyclicLink(topologyLinks);
            }

            var topolOrder = DenseTopol(topologyLinks);
            
            var topolGroups = topolOrder.GroupBy(x => x.Value).OrderBy(x => x.Key);

            double insertedGroupsOffset = 0;
            double horizontalOffset = 50;
            double verticalOffset = 50;
            bool firstRun = true;

            // groups of nodes that do not have links between them - if the flow arrangement is vertical, 
            // these will be horizontally besides each other and vice versa
            foreach (var topolGroup in topolGroups)
            {
                double groupStretch = 0;

                bool someNodesArranged = topolGroup.Any(x => x.Key.IsArranged);
                double arrangedHorizontalOffset = 0;
                double arrangedVerticalOffset = 0;
                DiagramNode arrangedRightNode = null;
                DiagramNode arrangedBottomNode = null;

                // new nodes in the group will have to follow after the already arranged ones
                if (someNodesArranged)
                {
                    foreach (var arrangedNode in topolGroup.Where(x => x.Key.IsArranged))
                    {
                        if (direction == DiagramArrangementDirection.Horizontal)
                        {
                            arrangedNode.Key.Left += insertedGroupsOffset;
                        }
                        else if (direction == DiagramArrangementDirection.Vertical)
                        {
                            arrangedNode.Key.Top += insertedGroupsOffset;

                            if (insertedGroupsOffset == 0 && clickHide == true && arrangedNode.Key.Top != 50 && firstRun == true)
                            {
                                arrangedNode.Key.Top = arrangedNode.Key.Top - 150;
                            }
                            else if (insertedGroupsOffset == 0 && clickHide == true && arrangedNode.Key.Top == 50)
                            {
                                arrangedNode.Key.Top = arrangedNode.Key.Top;
                            }
                        }
                        firstRun = false;
                    }

                    arrangedRightNode = topolGroup.Where(x => x.Key.IsArranged).OrderByDescending(y => y.Key.Left + y.Key.Width).First().Key;
                    arrangedBottomNode = topolGroup.Where(x => x.Key.IsArranged).OrderByDescending(y => y.Key.Top + y.Key.Height).First().Key;
                    
                    arrangedHorizontalOffset = arrangedRightNode.Left + arrangedRightNode.Width;
                    arrangedVerticalOffset = arrangedBottomNode.Top + arrangedBottomNode.Height;

                }

                // horizontal arrangement - start from left, alongside the rightmost arranged node, if any
                if (direction == DiagramArrangementDirection.Horizontal)
                {
                    verticalOffset = 50 + arrangedVerticalOffset;
                    if (arrangedRightNode != null)
                    {
                        horizontalOffset = arrangedRightNode.Left;
                    }
                }
                // vertical arrangement - start from top
                else if(direction == DiagramArrangementDirection.Vertical)
                {
                    horizontalOffset = 50 + arrangedHorizontalOffset;
                    if (arrangedBottomNode != null)
                    {
                        if (verticalOffset == arrangedBottomNode.Top)
                        {
                            verticalOffset = arrangedBottomNode.Top;
                        }
                        else if(verticalOffset > arrangedBottomNode.Top)
                        {

                        }
                    }
                    
                }
                
                foreach (var node in topolGroup)
                {
                    if (!(direction == DiagramArrangementDirection.Vertical && node.Key.IsArranged))
                    {
                        node.Key.Left = horizontalOffset;
                    }
                    // if the diagram is arranged horizontally, don't change the vertical position of the node 
                    // (keep its place within group, the entire group may be shifted, though)
                    if (!(direction == DiagramArrangementDirection.Horizontal && node.Key.IsArranged))
                    {
                        node.Key.Top = verticalOffset;
                    }
                    if (direction == DiagramArrangementDirection.Horizontal)
                    {
                        verticalOffset = verticalOffset + node.Key.Height + 50;
                        groupStretch = Math.Max(groupStretch, node.Key.Width);
                    }
                    else
                    {
                        horizontalOffset = horizontalOffset + node.Key.Width + 50;
                        groupStretch = Math.Max(groupStretch, node.Key.Height);
                    }
                    //groupMaxWidth = Math.Max(groupMaxWidth, node.Key.Width);

                    node.Key.IsArranged = true;
                }
                
                // make space for the next group
                if (direction == DiagramArrangementDirection.Horizontal)
                {
                    horizontalOffset = horizontalOffset + groupStretch + 100;
                    // if this group nad no prearranged nodes (it was inserted now), all the subsequent groups that have prearranged nodes will have to be shifted
                    if (!someNodesArranged)
                    {
                        insertedGroupsOffset += groupStretch + 100;
                    }
                }
                else
                {
                    verticalOffset = verticalOffset + groupStretch + 100;
                    if (!someNodesArranged)
                    {
                        insertedGroupsOffset += groupStretch + 100;
                    }
                }
            }

            // arrange nodes in a vertical group to the center of the group
            if (direction == DiagramArrangementDirection.Vertical)
            {
                var verticalGroups = topolOrder.GroupBy(x => x.Key.Left);
                foreach (var verticalGroup in verticalGroups)
                {
                    double maxWidth = verticalGroup.Max(x => x.Key.Width);
                    foreach (var node in verticalGroup)
                    {
                        node.Key.Left = node.Key.Left + (maxWidth - node.Key.Width) / 2;
                    }
                }
            }
        }

        public void Render()
        {
            foreach (var node in Nodes)
            {
                node.UpdateShape();
            }
            foreach (var link in Links)
            {
                link.UpdateShape();
            }

            CanvasRenderer cr = new CanvasRenderer();
            cr.SetCanvasSize(Canvas);
            Canvas.MouseLeftButtonUp += Canvas_MouseUp;
            Canvas.MouseMove += Canvas_MouseMove;
        }


        private void OnNodeSelected(object sender, DiagramNodeEventArgs e)
        {
            if (SelectableNodes == true)
            {
                if (_selectedNode != null)
                {
                    _selectedNode.SetUnselectedColour();
                }
                _selectedNode = e.Node;

                _selectedNode.SetSelectedColour();

                if (NodeSelected != null)
                {
                    NodeSelected(this, e);
                }
            }
        }

        private void OnNodeRightClick(object sender, DiagramNodeEventArgs e)
        {
            if (NodeRightClick != null)
            {
                NodeRightClick(sender, e);
            }
        }


        private DiagramLink FindWeakCyclicLink(List<DiagramLink> links)
        {
            var seenLinks = new List<DiagramLink>();
            foreach (var node in Nodes)
            {
                var res = DfsForCycle(node, seenLinks, links);
                if (res != null)
                {
                    return res;
                }
            }
            return null;
        }

        private DiagramLink DfsForCycle(DiagramNode startNode, List<DiagramLink> seenLinks, List<DiagramLink> links)
        {
            var nxtLinks = links.Where(x => x.FromNode == startNode).ToList();
            foreach (var nl in nxtLinks)
            {
                var nlIdx = seenLinks.IndexOf(nl);
                if (nlIdx > -1)
                {
                    var cycleRange = seenLinks.GetRange(nlIdx, seenLinks.Count - nlIdx);
                    return cycleRange.OrderBy(x => x.Strength).First();
                }
                seenLinks.Add(nl);
                var res = DfsForCycle(nl.ToNode, seenLinks, links);
                if (res != null)
                {
                    return res;
                }
                seenLinks.Remove(nl);
            }
            return null;
        }

        private Dictionary<DiagramNode, int> DenseTopol(List<DiagramLink> topologyLinks)
        {
            int dt = 1;
            Dictionary<DiagramNode, int> res = new Dictionary<DiagramNode, int>();
            while (res.Count < Nodes.Count)
            {
                // don;t have a topology rank assigned yet and all incoming links lead from nodes whose topology is already set
                var freeNodes = Nodes.Where(x => !res.ContainsKey(x) && topologyLinks.Where(y => y.ToNode == x).All(z => res.ContainsKey(z.FromNode))).ToList();
                foreach (var fn in freeNodes)
                {
                    res[fn] = dt;
                }
                dt++;
            }

            return res;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedObject == null)
            {
                return;
            }

            var newPos = e.GetPosition(Canvas);
            var newLeft = _dragObjectFromX + newPos.X - _dragMouseFromX;
            var newTop = _dragObjectFromY + newPos.Y - _dragMouseFromY;

            Canvas.SetLeft(_draggedObject.Shape, newLeft);
            Canvas.SetTop(_draggedObject.Shape, newTop);
            _draggedObject.Left = newLeft;
            _draggedObject.Top = newTop;
            var allLinks = _draggedObject.Links.ToList();
            foreach (var l in allLinks)
            {
                l.UpdateShape();
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _draggedObject = null;

        }

        public void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_draggedObject != null)
            {
                return;
            }
            _draggedObject = (DiagramNode)sender;
            var pos = e.GetPosition(Canvas);

            _dragMouseFromX = pos.X;
            _dragMouseFromY = pos.Y;
            _dragObjectFromX = Canvas.GetLeft(_draggedObject.Shape);
            _dragObjectFromY = Canvas.GetTop(_draggedObject.Shape);

            //tb.Text = string.Format("Drag from [{0}, {1}], click [{2}, {3}]", _dragObjectFromX, _dragObjectFromY, _dragMouseFromX, _dragMouseFromY);
        }
    }

    public class DiagramNode
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DiagramLink> LinksTop { get; set; }
        public List<DiagramLink> LinksBottom { get; set; }
        public List<DiagramLink> LinksLeft { get; set; }
        public List<DiagramLink> LinksRight { get; set; }
        public IEnumerable<DiagramLink> Links { get { return LinksLeft.Union(LinksTop).Union(LinksRight).Union(LinksBottom); } }
        public Diagram Diagram { get; set; }
        public int Id { get; set; }
        public bool IsArranged { get; set; }

        public System.Windows.FrameworkElement Shape { get; set; }

        public DiagramNode(int id, Diagram diagram, string name, string description)
        {
            Id = id;
            Left = 0;
            Top = 0;
            Width = 0;
            Height = 0;
            Name = name;
            Description = description;
            LinksTop = new List<DiagramLink>();
            LinksBottom = new List<DiagramLink>();
            LinksLeft = new List<DiagramLink>();
            LinksRight = new List<DiagramLink>();
            Diagram = diagram;
            var maxLen = Math.Max(name.Length, description.Length + 2);

            Width = maxLen * 8 + 30;
            Height = 50;

            var border = new Border()
            {
                BorderThickness = new System.Windows.Thickness(1),
                BorderBrush = Brushes.DarkGray
            };

            var tb = new TextBlock()
            {
                Width = this.Width,
                Height = this.Height,
                Background = new LinearGradientBrush(
                    new Color() { R = 240, G = 240, B = 240, A = 255 },
                    new Color() { R = 220, G = 220, B = 220, A = 255 },
                    90
                    ),
                Padding = new System.Windows.Thickness(10, 5, 10, 5),
                //Text = Name + Environment.NewLine + description,
            };
            border.Child = tb;

            tb.Inlines.Add(new System.Windows.Documents.Run(Name) { FontWeight = System.Windows.FontWeights.Bold });
            tb.Inlines.Add(new System.Windows.Documents.Run(Environment.NewLine + Description));
            
            Shape = border;
            Diagram.Canvas.Children.Add(Shape);

            Shape.MouseLeftButtonDown += Shape_MouseDown;
            Shape.MouseRightButtonDown += Shape_MouseRightButtonDown;
            
            Shape.MouseEnter += Shape_MouseEnter;
            Shape.MouseLeave += Shape_MouseLeave;
            
        }

        public void RemoveShape()
        {
            Diagram.Canvas.Children.Remove(Shape);
            Shape = null;
        }

        public void DetachLink(DiagramLink link)
        {
            if (LinksTop.Contains(link))
            {
                LinksTop.Remove(link);
            }
            if (LinksRight.Contains(link))
            {
                LinksRight.Remove(link);
            }
            if (LinksBottom.Contains(link))
            {
                LinksBottom.Remove(link);
            }
            if (LinksLeft.Contains(link))
            {
                LinksLeft.Remove(link);
            }
        }

        private void Shape_MouseLeave(object sender, MouseEventArgs e)
        {
            if (NodeSelected != null)
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private void Shape_MouseEnter(object sender, MouseEventArgs e)
        {
            if (NodeSelected != null)
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
        }

        public event DiagramNodeEventHander NodeSelected;
        public event DiagramNodeEventHander NodeDoubleClick;

        private void Shape_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Diagram.Rect_MouseDown(this, e);
            if (NodeSelected != null)
            {
                NodeSelected(sender, new DiagramNodeEventArgs() { Node = this });
            }
            if (e.ClickCount == 2)
            {
                if (NodeDoubleClick != null)
                {
                    NodeDoubleClick(sender, new DiagramNodeEventArgs()
                    {
                        Node = this
                    });
                }
            }
        }

        public event DiagramNodeEventHander NodeRightClick;

        private void Shape_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (NodeRightClick != null)
            {
                NodeRightClick(sender, new DiagramNodeEventArgs() { Node = this });
            }
        }


        public void SetSelectedColour()
        {
            var border = (Border)Shape;
            var tb = (TextBlock)(border.Child);
            tb.Background = new LinearGradientBrush(
                    new Color() { R = 240, G = 240, B = 240, A = 255 },
                    new Color() { R = 240, G = 240, B = 220, A = 255 },
                    90
                    );
        }

        public void SetUnselectedColour()
        {
            var border = (Border)Shape;
            var tb = (TextBlock)(border.Child);
            tb.Background = new LinearGradientBrush(
                    new Color() { R = 240, G = 240, B = 240, A = 255 },
                    new Color() { R = 220, G = 220, B = 220, A = 255 },
                    90
                    );
        }

        public void UpdateShape()
        {
            Canvas.SetLeft(Shape, Left);
            Canvas.SetTop(Shape, Top);
        }

        public List<DiagramLink> LinksFromSide(ConnectorPosition side)
        {
            switch (side)
            {
                case ConnectorPosition.Left:
                    return LinksLeft;

                case ConnectorPosition.Top:
                    return LinksTop;

                case ConnectorPosition.Right:
                    return LinksRight;

                case ConnectorPosition.Bottom:
                    return LinksBottom;
                default:
                    throw new NotImplementedException();
            }
        }


        public Point ConnectorCenterPosition(DiagramLink link, ConnectorPosition connectorSide)
        {
            var sideCount = LinksFromSide(connectorSide).Count;
            var sideIdx = LinksFromSide(connectorSide).IndexOf(link);

            var sideStretch = connectorSide == ConnectorPosition.Left || connectorSide == ConnectorPosition.Right ? Height : Width;
            var sideOffset = sideStretch / (sideCount + 1) * (sideIdx + 1);

            switch (connectorSide)
            {
                case ConnectorPosition.Left:
                    return new Point()
                    {
                        X = Left,
                        Y = Top + sideOffset
                    };
                case ConnectorPosition.Top:
                    return new Point()
                    {
                        X = Left + sideOffset,
                        Y = Top
                    };
                case ConnectorPosition.Right:
                    return new Point()
                    {
                        X = Left + Width,
                        Y = Top + sideOffset
                    };
                case ConnectorPosition.Bottom:
                    return new Point()
                    {
                        X = Left + sideOffset,
                        Y = Top + Height
                    };
                default:
                    throw new NotImplementedException();
            }
        }

    }

    public class DiagramLink
    {
        public int Id { get; set; }
        public DiagramNode FromNode { get; set; }
        public DiagramNode ToNode { get; set; }
        public ConnectorPosition ConnectorFromSide { get; set; }
        public ConnectorPosition ConnectorToSide { get; set; }

        public Diagram Diagram { get; set; }
        public int Thickness { get; set; }
        public int Strength { get; set; }

        public Polygon Shape { get; set; }

        public DiagramLink(int id, DiagramNode fromNode, DiagramNode toNode, Diagram diagram)
        {
            FromNode = fromNode;
            ToNode = toNode;
            Diagram = diagram;
            Id = id;
        }

        public void UpdateShape()
        {
            bool isAdded = false;
            if (Shape == null)
            {
                Shape = new Polygon()
                {
                    Fill = new SolidColorBrush(new Color() { R = 200, G = 200, B = 200, A = 255 }), // Brushes.SteelBlue,
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(Colors.DarkGray)
                };
                Thickness = 5; // (int)Math.Floor(Math.Log(Strength + 3) / Math.Log(Diagram.MaxLinkStrength + 3)) * 5 + 2;
                isAdded = true;
            }


            var sourceConnectorSide = GeometryHelper.RecommendedTargetConnectorPosition(ToNode, FromNode);
            var targetConnectorSide = GeometryHelper.RecommendedTargetConnectorPosition(FromNode, ToNode);

            bool sourceSideChanged = isAdded || !FromNode.LinksFromSide(sourceConnectorSide).Contains(this);
            bool targetSideChanged = isAdded || !ToNode.LinksFromSide(targetConnectorSide).Contains(this);

            // remove the (now wrong) side to which the link was originally connected
            if (sourceSideChanged && !isAdded)
            {
                FromNode.LinksFromSide(ConnectorFromSide).Remove(this);
            }
            if (targetSideChanged && !isAdded)
            {
                ToNode.LinksFromSide(ConnectorToSide).Remove(this);
            }

            // add to the new side
            if (sourceSideChanged)
            {
                var linksFromSide = FromNode.LinksFromSide(sourceConnectorSide);
                linksFromSide.Add(this);
                // update the position of the other links attached to this side of the node (shrink them a bit)
                foreach (var link in linksFromSide.ToList())
                {
                    if (link == this)
                    {
                        break;
                    }
                    link.UpdateShape();
                }
            }
            if (targetSideChanged)
            {
                var linksFromSide = ToNode.LinksFromSide(targetConnectorSide);
                linksFromSide.Add(this);
                foreach (var link in linksFromSide.ToList())
                {
                    if (link == this)
                    {
                        break;
                    }
                    link.UpdateShape();
                }
            }

            Point sourceConnectorPos1 = null;
            Point sourceConnectorPos2 = null;
            Point targetConnectorPos = null;
            var sourceConnectionCenter = FromNode.ConnectorCenterPosition(this, sourceConnectorSide);
            targetConnectorPos = ToNode.ConnectorCenterPosition(this, targetConnectorSide);

            if (sourceConnectorSide == ConnectorPosition.Left || sourceConnectorSide == ConnectorPosition.Right)
            {
                sourceConnectorPos1 = new Point() { X = sourceConnectionCenter.X, Y = sourceConnectionCenter.Y - Thickness / 2 };
                sourceConnectorPos2 = new Point() { X = sourceConnectionCenter.X, Y = sourceConnectionCenter.Y + Thickness / 2 };
            }
            else
            {
                sourceConnectorPos1 = new Point() { X = sourceConnectionCenter.X - Thickness / 2, Y = sourceConnectionCenter.Y };
                sourceConnectorPos2 = new Point() { X = sourceConnectionCenter.X + Thickness / 2, Y = sourceConnectionCenter.Y };
            }

            // if these two are not inverted, the arrow will be skewed
            if (sourceConnectorSide == ConnectorPosition.Left || sourceConnectorSide == ConnectorPosition.Bottom)
            {
                var t = sourceConnectorPos1;
                sourceConnectorPos1 = sourceConnectorPos2;
                sourceConnectorPos2 = t;
            }

            SetPolygonArrowPoints(sourceConnectorPos1, sourceConnectorPos2, targetConnectorPos, Shape);

            if (isAdded)
            {
                Diagram.Canvas.Children.Add(Shape);
            }
            ConnectorFromSide = sourceConnectorSide;
            ConnectorToSide = targetConnectorSide;

        }

        public void RemoveShape()
        {
            Diagram.Canvas.Children.Remove(Shape);
            Shape = null;
        }


        public void SetPolygonArrowPoints(Point sourceConnector1, Point sourceConnector2, Point targetConnector, Polygon polygon)
        {
            //polygon.Points.Clear();

            var p1 = sourceConnector1;
            var p2 = sourceConnector2;
            var tp = targetConnector;
            // between p1 and p2
            var ptd = GeometryHelper.MultiplyVector(GeometryHelper.AddVectors(p1, p2), 0.5);
            var dst = GeometryHelper.Distance(ptd, tp);
            var mainArch = GeometryHelper.SubtractVectors(tp, ptd);
            var d = GeometryHelper.Distance(p1, p2);
            //percentage used for the arrow line, the rest is for the head
            var leftSpaceFactor = (dst - 3 * d) / dst;
            if (leftSpaceFactor < 0)
            {
                leftSpaceFactor = 0;
            }
            // between d and tp, base of the head
            var sd = GeometryHelper.MultiplyVector(mainArch, leftSpaceFactor);

            var triangleBaseShrinkFactor = (/*2 * */d) / dst;
            var triangleMidBaseShrinkFactor = (d) / dst;
            var triangleBase1 = GeometryHelper.MultiplyVector(GeometryHelper.RotateCounterClockwise(mainArch), triangleBaseShrinkFactor);
            var triangleBase2 = GeometryHelper.MultiplyVector(GeometryHelper.RotateClockwise(mainArch), triangleBaseShrinkFactor);
            var triangleMidBase = GeometryHelper.MultiplyVector(GeometryHelper.RotateClockwise(mainArch), triangleMidBaseShrinkFactor);

            var p1s = GeometryHelper.AddVectors(p1, sd);
            //var p2s = GeometryHelper.AddVectors(p2, sd);
            var p2s = GeometryHelper.AddVectors(p1s, triangleMidBase);
            var p1so = GeometryHelper.AddVectors(p1s, triangleBase1);
            var p2so = GeometryHelper.AddVectors(p2s, triangleBase2);

            PointCollection pc = new PointCollection();

            //A1*B2 - A2*B1
            if (GeometryHelper.DoIntersect(p1, p1s, p2, p2s))
            {
                var p1t = p1s;
                p1s = p2s;
                p2s = p1t;
            }

            pc.Add(new System.Windows.Point() { X = p1.X, Y = p1.Y });
            pc.Add(new System.Windows.Point() { X = p1s.X, Y = p1s.Y });
            pc.Add(new System.Windows.Point() { X = p1so.X, Y = p1so.Y });
            pc.Add(new System.Windows.Point() { X = tp.X, Y = tp.Y });
            pc.Add(new System.Windows.Point() { X = p2so.X, Y = p2so.Y });
            pc.Add(new System.Windows.Point() { X = p2s.X, Y = p2s.Y });
            pc.Add(new System.Windows.Point() { X = p2.X, Y = p2.Y });

            polygon.Points = pc;
        }
    }
}
