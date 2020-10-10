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
    public partial class NameChooser : UserControl
    {

        public event EventHandler Submitted;
        public event EventHandler Cancelled;


        private List<string> _takenNames = new List<string>();
        private bool _dialogResult = false;

        public string SelectedName
        {
            get { return nameTextBox.Text; }
        }

        public bool DialogResult { get { return _dialogResult; } }

        public List<string> TakenNames { get { return _takenNames; } set { _takenNames = value; } }

        public NameChooser(List<string> takenNames = null)
        {
            InitializeComponent();
            errorLabel.Visibility = Visibility.Hidden;
            _takenNames = takenNames;
        }

        public NameChooser()
            :this(null)
        {

        }

        public string Label {
            get { return label.Content.ToString(); }
            set { label.Content = value; }
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
            if (Validate())
            {
                Submitted?.Invoke(this, new EventArgs());
            }
        }

        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private bool Validate()
        {
            
            var res = true;
            if(TakenNames == null)
            {
                
            }
            else
            {
                if (!(String.IsNullOrEmpty(SelectedName)))
                {
                    foreach (var str in TakenNames)
                    {                   
                        if (str == SelectedName)
                        {
                            res = false;
                        }                      
                    }
                }
                else
                {
                    res = false;
                }               
            }
            errorLabel.Visibility = res ? Visibility.Hidden : Visibility.Visible;
            return res;
        }
    }
}
