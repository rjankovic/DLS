using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CD.DLS.Clients.Controls.Dialogs.Misc
{
    public delegate void TreeNodeMouseButtonEventHandler(StretchingTreeViewItem sender, MouseButtonEventArgs e);

    public class StretchingTreeView : TreeView
    {
        public StretchingTreeView()
            : base()
        {
            this.PreviewMouseWheel += TreeView_PreviewMouseWheel;
            this.MouseRightButtonDown += StretchingTreeView_MouseRightButtonDown;
        }

        
        public event TreeNodeMouseButtonEventHandler TreeNodeRightClick;

        private void StretchingTreeView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            StretchingTreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            
            if (treeViewItem != null && TreeNodeRightClick != null)
            {
                treeViewItem.Focus();
                TreeNodeRightClick(treeViewItem, e);
            }
        }

        static StretchingTreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is StretchingTreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as StretchingTreeViewItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new StretchingTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StretchingTreeViewItem;
        }

        // https://serialseb.com/blog/2007/09/03/wpf-tips-6-preventing-scrollviewer-from/
        private void TreeView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TreeView && !e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }

    public class StretchingTreeViewItem : TreeViewItem
    {
        public StretchingTreeViewItem()
        {
            this.Loaded += new RoutedEventHandler(StretchingTreeViewItem_Loaded);
        }

        private void StretchingTreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            // The purpose of this code is to stretch the Header Content all the way accross the TreeView. 
            if (this.VisualChildrenCount > 0)
            {
                Grid grid = this.GetVisualChild(0) as Grid;
                if (grid != null && grid.ColumnDefinitions.Count == 3)
                {
                    // Remove the middle column which is set to Auto and let it get replaced with the 
                    // last column that is set to Star.
                    grid.ColumnDefinitions.RemoveAt(1);
                }
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new StretchingTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StretchingTreeViewItem;
        }
        
    }
}
