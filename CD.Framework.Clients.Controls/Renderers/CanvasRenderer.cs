using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CD.DLS.Clients.Controls.Renderers
{
    public class CanvasRenderer
    {
        public void SetCanvasSize(Canvas canvas)
        {
            double maxWidth = 0;
            double maxHeight = 0;
            foreach (System.Windows.FrameworkElement child in canvas.Children)
            {
                if ((double.IsNaN(child.Width) ? 1000 : child.Width) + Canvas.GetLeft(child) > maxWidth)
                {
                    maxWidth = (double.IsNaN(child.Width) ? 1000 : child.Width) + Canvas.GetLeft(child);
                }
                if ((double.IsNaN(child.Height) ? 1000 : child.Height) + Canvas.GetTop(child) > maxHeight)
                {
                    maxHeight = (double.IsNaN(child.Height) ? 1000 : child.Height) + Canvas.GetTop(child);
                }
            }

            canvas.Width = maxWidth;
            canvas.Height = maxHeight;
        }
    }
}
