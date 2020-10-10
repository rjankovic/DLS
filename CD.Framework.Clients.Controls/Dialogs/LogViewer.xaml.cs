using CD.DLS.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Threading;

namespace CD.DLS.Clients.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for ListPicker.xaml
    /// </summary>
    public partial class LogViewer : UserControl, ILogger
    {

        DispatcherOperation _op = null;
        private DataTable _dt;
        private DateTime _lastUIUpdate;


        public LogViewer()
        {
            InitializeComponent();

            _dt = new DataTable();
            _dt.Columns.Add("Timestamp");
            _dt.Columns.Add("Message");
            _dt.Columns.Add("MessageType");

            grid.DataContext = _dt;
            grid.ItemsSource = _dt.DefaultView;
            _lastUIUpdate = DateTime.Now;

        }
        
        public void Error(string message, params object[] args)
        {
            Write(message, args, LogTypeEnum.Error);
        }

        public void Important(string message, params object[] args)
        {
            Write(message, args, LogTypeEnum.Important);
        }

        public void Info(string message, params object[] args)
        {
            Write(message, args, LogTypeEnum.Info);
        }

        public void FlushMessages()
        {
            //throw new NotImplementedException();
        }

        public void Warning(string message, params object[] args)
        {
            Write(message, args, LogTypeEnum.Warning);
        }

        public void Write(string message, LogTypeEnum messageType)
        {
            Write(message, new string[0], messageType);
        }

        private void Write(string message, object[] args, LogTypeEnum messageType)
        {
            var nr = _dt.NewRow();
            nr[0] = DateTime.Now.ToString("u");
            nr[1] = message;
            try
            {
                if (args.Length > 0)
                {
                    nr[1] = string.Format(message, args);
                }
            }
            catch
            {
                nr[1] = message;
            }
            nr[2] = messageType.ToString();
            _dt.Rows.Add(nr);

            //if (DateTime.Compare(_lastUIUpdate.AddSeconds(2), DateTime.Now) < 0)
            //{
            //    _lastUIUpdate = DateTime.Now;
            //    if (_op == null || _op.Status == DispatcherOperationStatus.Completed)
            //    {
            //        //_op = Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action)(() => { grid.Items.Refresh(); }));
            //        _op = Dispatcher.BeginInvoke((Action)(grid.Items.Refresh), DispatcherPriority.ApplicationIdle);
            //    }
            //}
            //_dispatcherStatus = x.Status;
            
        }

        public void LogUserAction(UserActionEventType eventType, string frameworkElement, string dataContext, string extendedProperties)
        {
            //throw new NotImplementedException();
        }
    }
}
