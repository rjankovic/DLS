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

namespace CD.DLS.Clients.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for ListPicker.xaml
    /// </summary>
    public partial class TextInput : UserControl
    {

        public event EventHandler Submitted;
        public event EventHandler Cancelled;


        private List<string> _takenNames = new List<string>();
        private bool _dialogResult = false;

        public string TextContent
        {
            get { return contentTextBox.Text; }
        }

        public bool DialogResult { get { return _dialogResult; } }

        public TextInput()
        {
            InitializeComponent();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Cancelled?.Invoke(this, new EventArgs());
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
                Submitted?.Invoke(this, new EventArgs());
        }

        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
