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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BusinessDictionaryIndex;

namespace CD.DLS.Clients.Controls.Dialogs.ExcelBusinessDictionary
{

    public class BusinessDictionaryPaneEventArgs : EventArgs
    {
        public List<AnnotationViewFieldValue> Values { get; set; }
        public int ModelElementId { get; set; }
    }

    public delegate void BusinessDictionaryPaneHandler(object sender, BusinessDictionaryPaneEventArgs e);

    /// <summary>
    /// Interaction logic for ExcelBusinessDictionary.xaml
    /// </summary>
    public partial class ExcelBusinessDictionary : UserControl
    {
        private OlapFieldLookupResult _activeElement = null;
        private List<ProjectDictionaryFieldMappingItem> _fields;
        private List<AnnotationViewFieldValue> _values;
        private Dictionary<int, TextBox> _fieldsToTextBoxes;
        private BusinessDictionaryIndex _businessDictionaryIndex;

        public event BusinessDictionaryPaneHandler SaveClicked;
        public event BusinessDictionaryPaneHandler DetailsClicked;
        
        public ExcelBusinessDictionary()
        {
            InitializeComponent();
        }

        public void LoadContent(OlapFieldLookupResult olapField, BusinessDictionaryIndex businessDictionaryIndex, string fieldName)
        {
            if (_activeElement != null)
            {
                if (_activeElement.ModelElementId == olapField.ModelElementId)
                {
                    return;
                }
            }

            businessDictionaryFields.Children.Clear();
            _businessDictionaryIndex = businessDictionaryIndex;
            _activeElement = olapField;
            _fields = businessDictionaryIndex.FindFieldsForElementType(_activeElement.ElementType);
            _values = businessDictionaryIndex.GetFieldValues(_activeElement.ModelElementId);
            _fieldsToTextBoxes = new Dictionary<int, TextBox>();
            
            foreach (var field in _fields)
            {
                var label = new Label() { Content = field.FieldName };
                businessDictionaryFields.Children.Add(label);

                var textBox = new TextBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                    MinLines = 3,
                    MaxLines = 5,
                    AcceptsReturn = true
                };
                textBox.TextChanged += (o, e) => savedIndicatorLabel.Visibility = Visibility.Hidden;
                businessDictionaryFields.Children.Add(textBox);
                _fieldsToTextBoxes.Add(field.FieldId, textBox);

                if (!_values.Any(x => x.FieldId == field.FieldId))
                {
                    _values.Add(new AnnotationViewFieldValue()
                    {
                        AnnotationElementId = 0,
                        FieldId = field.FieldId,
                        FieldValueId = 0,
                        ModelElementId = _activeElement.ModelElementId,
                        Value = string.Empty
                    });
                }

                var value = _values.First(x => x.FieldId == field.FieldId);
                textBox.Text = value.Value;
            }

            elementNameLabel.Content = fieldName;
            savedIndicatorLabel.Visibility = Visibility.Hidden;
        }

        public void ShowSavedIndicator()
        {
            savedIndicatorLabel.Foreground = Brushes.Black;
            savedIndicatorLabel.Content = "Values saved.";
            savedIndicatorLabel.Visibility = Visibility.Visible;
        }

        public void ShowMissingPermissionsIndicator()
        {
            savedIndicatorLabel.Foreground = Brushes.DarkRed;
            savedIndicatorLabel.Content = "Insufficient permissions.";
            savedIndicatorLabel.Visibility = Visibility.Visible;
        }

        public void ReadValues()
        {
            foreach (var v in _values)
            {
                v.Value = _fieldsToTextBoxes[v.FieldId].Text;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ReadValues();
            if (SaveClicked != null)
            {
                SaveClicked(this, new BusinessDictionaryPaneEventArgs()
                {
                    ModelElementId = _activeElement.ModelElementId,
                    Values = _values
                });
            }
        }

        public void RefreshData()
        {
            _businessDictionaryIndex.Refresh();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var kv in _fieldsToTextBoxes)
            {
                kv.Value.Text = _values.First(x => x.FieldId == kv.Key).Value;
            }
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DetailsClicked != null)
            {
                DetailsClicked(this, new BusinessDictionaryPaneEventArgs()
                {
                    ModelElementId = _activeElement.ModelElementId,
                    Values = _values
                });
            }
        }
    }
}
