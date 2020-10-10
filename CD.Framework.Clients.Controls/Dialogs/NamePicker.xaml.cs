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
using static CD.DLS.DAL.Managers.SecurityManager;

namespace CD.DLS.Clients.Controls.Dialogs
{
    public class PickerItem
    {
        public object Id { get; set; }
        public string Label { get; set; }
    }

    /// <summary>
    /// Interaction logic for ListPicker.xaml
    /// </summary>
    public partial class NamePicker : Window
    {

        public event EventHandler Submitted;
        public event EventHandler Cancelled;


        private List<PickerItem> _takenItems = new List<PickerItem>();
        private List<PickerItem> _comboBoxNames = new List<PickerItem>();
        public bool IsSubmitted = false;

        public PickerItem SelectedItem
        {
            get
            { 
                return nameComboBox.SelectedItem == null ? (PickerItem)null : (PickerItem)(nameComboBox.SelectedItem);              
            }
        }
        
        public List<PickerItem> TakenItems { get { return _takenItems; } set { _takenItems = value; } }
        public List<PickerItem> ComboBoxNames { get { return _comboBoxNames; } set { _comboBoxNames = value; } }

        public NamePicker(List<PickerItem> takenItems = null, List<PickerItem> comboBoxNames = null)
        {
            InitializeComponent();
            ComboBoxNames = comboBoxNames;
            TakenItems = takenItems;
            nameComboBox.DisplayMemberPath = "Label";
            nameComboBox.ItemsSource = ComboBoxNames;
            errorLabel.Visibility = Visibility.Hidden;
        }

        public void SetCaption(string caption)
        {
            captionLabel.Content = caption;
        }

        public NamePicker()
            :this(null)
        {

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

        private bool Validate()
        {
            var res = true;
            if(TakenItems != null)
            {
                if (TakenItems.Any(x => x.Id.Equals(SelectedItem.Id)))
                {
                    res = false;
                }
            }
            errorLabel.Visibility = res ? Visibility.Hidden : Visibility.Visible;
            return res;
        }

        private void nameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
