using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.SsisDiagram
{
    /// <summary>
    /// Relative or absolute (for sizes and arrow segments) position
    /// </summary>
    public class DesignPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    /// <summary>
    /// An arrow connecting two DF or CF components
    /// </summary>
    public class DesignArrow
    {
        public DesignPoint TopLeft { get; set; }
        /// <summary>
        /// Brush moves
        /// </summary>
        public List<DesignPoint> Shifts { get; set; }
        public enum PointOrientationEnum { Left, Up, Right, Down }
        public PointOrientationEnum PointOrientation
        {
            get
            {
                if (Shifts.Count == 0)
                {
                    // nasty cover-up
                    return PointOrientationEnum.Down;
                }
                var finSeg = Shifts[Shifts.Count - 1];
                if (finSeg.X < 0)
                    return PointOrientationEnum.Left;
                if (finSeg.Y < 0)
                    return PointOrientationEnum.Up;
                if (finSeg.X > 0)
                    return PointOrientationEnum.Right;
                if (finSeg.Y > 0)
                    return PointOrientationEnum.Down;

                // dtto
                return PointOrientationEnum.Down;
            }
        }

    }

    public class DesignBlock
    {
        public DesignPoint Position { get; set; }
        public DesignPoint Size { get; set; }
        public List<DesignBlock> ChildBlocks { get; set; }

        public List<DesignArrow> Arrows { get; set; }
        public string RefPath { get; set; }
        public int ElementId { get; set; }
        public string ElementType { get; set; }
        public string Name { get; set; }

    }
}
