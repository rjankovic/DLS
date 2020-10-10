using CD.DLS.DAL.Objects.Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Extract.PowerBi.PowerBiAPI
{
    class VisualContainerConfig
    {
        public class Position
        {
            private double x;
            private double y;
            private double z;
            private double width;
            private double height;

            public Position(double x, double y, double z, double width, double height)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.width = width;
                this.height = height;
            }
        }
        public class Layout
        {           
            private int id;
            private Position position;

            public Layout(int id, Position position)
            {
                this.id = id;
                this.position = position;
            }
        }
       
        
        public class SingleVisual
        {
            private bool drillFilterOtherVisuals;
            private bool hasDefaultSort;

            public SingleVisual(string visualType, Projections projections, bool drillFilterOtherVisuals, bool hasDefaultSort)
            {
                this.VisualType = visualType;
                this.Projections = projections;
                this.drillFilterOtherVisuals = drillFilterOtherVisuals;
                this.hasDefaultSort = hasDefaultSort;
            }

            public string VisualType { get; set; }
            internal Projections Projections { get; set; }
        }

        public class Projections
        {           
            public Projections(Projection[] Category, Projection[] y, Projection[] y2, Projection[] x, Projection[] size, Projection[] tooltips, Projection[] series, Projection[] targetValue, Projection[] minValue, Projection[] maxValue, Projection[] values, Projection[] indicator, Projection[] trendLine, Projection[] goal, Projection[] details, Projection[] group, Projection[] play, Projection[] breakdown)
            {
                this.Category = Category;
                this.Y = y;
                this.Y2 = y2;
                this.X = x;
                this.Size = size;
                Tooltips = tooltips;
                Series = series;
                TargetValue = targetValue;
                MinValue = minValue;
                MaxValue = maxValue;
                Values = values;
                Indicator = indicator;
                TrendLine = trendLine;
                Goal = goal;
                Details = details;
                Group = group;
                Play = play;
                Breakdown = breakdown;
            }

            public Projection[] Y { get; set; }
            public Projection[] Y2 { get; set; }
            public Projection[] X { get; set; }
            public Projection[] Size { get; set; }
            public Projection[] Tooltips { get; set; }
            public Projection[] Series { get; set; }
            public Projection[] TargetValue { get; set; }
            public Projection[] MinValue { get; set; }
            public Projection[] MaxValue { get; set; }
            public Projection[] Values { get; set; }
            public Projection[] Indicator { get; set; }
            public Projection[] TrendLine { get; set; }
            public Projection[] Goal { get; set; }
            public Projection[] Details { get; set; }
            public Projection[] Group { get; set; }
            public Projection[] Play { get; set; }
            public Projection[] Breakdown { get; set; }
            public Projection[] Category { get; set; }
        }

        private string name;
        private Layout[] layouts;
        private SingleVisual singleVisual;

        public VisualContainerConfig(string name, Layout[] layouts,SingleVisual singleVisual)
        {
            this.SingleVisual1 = singleVisual;
            this.name = name;
            this.layouts = layouts;
        }

        internal SingleVisual SingleVisual1 { get; set; }
    }
}
