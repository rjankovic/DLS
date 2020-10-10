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
    
    public static class GeometryHelper
    {
        public static double Distance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        public static double Sin(Point source, Point target)
        {
            var dst = Distance(source, target);
            var vertical = target.Y - source.Y;
            return vertical / dst;
        }

        public static ConnectorPosition RecommendedTargetConnectorPosition(Point sourcePoint, Point targetPoint)
        {
            var sin = Sin(sourcePoint, targetPoint);
            var angle = Math.Asin(sin);
            if (angle > Math.PI / 4)
            {
                return ConnectorPosition.Top;
            }
            if (angle < -Math.PI / 4)
            {
                return ConnectorPosition.Bottom;
            }
            if (sourcePoint.X < targetPoint.X)
            {
                return ConnectorPosition.Left;
            }
            return ConnectorPosition.Right;
        }

        public static ConnectorPosition RecommendedTargetConnectorPosition(DiagramNode fromNode, DiagramNode toNode)
        {
            var fromCenter = new Point()
            {
                X = fromNode.Left + fromNode.Width / 2,
                Y = fromNode.Top + fromNode.Height / 2
            };
            var toCenter = new Point()
            {
                X = toNode.Left + toNode.Width / 2,
                Y = toNode.Top + toNode.Height / 2
            };

            return RecommendedTargetConnectorPosition(fromCenter, toCenter);
        }

        public static Point AddVectors(Point v1, Point v2)
        {
            return new Point()
            {
                X = v1.X + v2.X,
                Y = v1.Y + v2.Y
            };
        }

        public static Point SubtractVectors(Point v1, Point v2)
        {
            return new Point()
            {
                X = v1.X - v2.X,
                Y = v1.Y - v2.Y
            };
        }

        public static Point RotateClockwise(Point vector)
        {
            return new Point()
            {
                X = -vector.Y,
                Y = vector.X
            };
        }

        public static Point RotateCounterClockwise(Point vector)
        {
            return new Point()
            {
                X = vector.Y,
                Y = -vector.X
            };
        }

        public static Point MultiplyVector(Point vector, double factor)
        {
            return new Point()
            {
                X = vector.X * factor,
                Y = vector.Y * factor
            };
        }

        // Given three colinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        private static bool onSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        // To find orientation of ordered triplet (p, q, r).
        // The function returns following values
        // 0 --> p, q and r are colinear
        // 1 --> Clockwise
        // 2 --> Counterclockwise
        private static int Orientation(Point p, Point q, Point r)
        {
            // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
            // for details of below formula.
            double val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);

            if (val == 0) return 0;  // colinear

            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }

        // The main function that returns true if line segment 'p1q1'
        // and 'p2q2' intersect.
        public static bool DoIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            // Find the four orientations needed for general and
            // special cases
            int o1 = Orientation(p1, q1, p2);
            int o2 = Orientation(p1, q1, q2);
            int o3 = Orientation(p2, q2, p1);
            int o4 = Orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1
            if (o1 == 0 && onSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are colinear and q2 lies on segment p1q1
            if (o2 == 0 && onSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2
            if (o3 == 0 && onSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2
            if (o4 == 0 && onSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases
        }
    }

    public class Point
    {
        public double X;
        public double Y;
    }
    
}
