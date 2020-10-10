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
    /// Interaction logic for NameChooserWindow.xaml
    /// </summary>
    public partial class NameChooserWindow : Window
    {
        private List<string> _takenNames = new List<string>();
        public List<string> TakenNames { get { return _takenNames; } set { _takenNames = value; } }
        public string SelectedName { get { return NameChooser.SelectedName; } }
        public bool Submittted = false;

        public NameChooserWindow()
        {
            InitializeComponent();
        }

        public string Label
        {
            get { return NameChooser.Label; }
            set { NameChooser.Label = value; }
        }
        
        public NameChooserWindow(List<string> takenNames = null)
        {
            InitializeComponent();
            NameChooser.TakenNames = takenNames;
            NameChooser.Submitted += (s, e1) => { DialogResult = true; Close(); Submittted = true; };
            NameChooser.Cancelled += (s, e2) => { DialogResult = false; Close(); };
        }
    }
}
