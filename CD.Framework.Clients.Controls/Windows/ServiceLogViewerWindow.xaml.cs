using CD.DLS.DAL.Receiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CD.DLS.Clients.Controls.Windows
{
    /// <summary>
    /// Interaction logic for ServiceLogViewerWindow.xaml
    /// </summary>
    public partial class ServiceLogViewerWindow : Window
    {
        public ServiceLogViewerWindow()
        {
            InitializeComponent();
            LogListener listener = new LogListener();
            
            listener.NewLog += (s, e1) =>
            {
                logViewer.Write(e1.LogEntry.Message, e1.LogEntry.LogType);
            };
        }
    }
}
