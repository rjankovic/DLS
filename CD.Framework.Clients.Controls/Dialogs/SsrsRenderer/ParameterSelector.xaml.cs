using CD.DLS.Clients.Controls.Dialogs.Misc;
using Microsoft.SqlServer.ReportExecution2005;
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
//using Xceed.Wpf.Toolkit;

namespace CD.DLS.Clients.Controls.Dialogs.SsrsRenderer
{
    /// <summary>
    /// Interaction logic for ParameterSelector.xaml
    /// </summary>
    public partial class ParameterSelector : UserControl
    {
        public ParameterSelector()
        {
            InitializeComponent();
        }

        public ParameterSelector(string reportItemPath, ReportExecutionService reportExecutionService)
        {
            _reportItemPath = reportItemPath;
            _reportExecutionService = reportExecutionService;
            //_ssrsComponentId = ssrsComponentId;
            InitializeComponent();
            string historyID = null;
            _executionInfo = _reportExecutionService.LoadReport(reportItemPath, historyID);

            btnRefresh_Click(this, new RoutedEventArgs());
        }

        public event EventHandler RenderReportClick;

        private List<ReportParameter> _parametersState { get; set; }
        
        private string _reportItemPath;
        //private int _ssrsComponentId;
        private Dictionary<string, List<string>> _selectedParameterValues = new Dictionary<string, List<string>>();
        private Dictionary<string, System.Windows.UIElement> _paramControls = new Dictionary<string, System.Windows.UIElement>();
        private ReportExecutionService _reportExecutionService;
        private ExecutionInfo _executionInfo;

        public Dictionary<string, List<string>> SelectedParameterValues
        {
            get
            {
                return _selectedParameterValues;
            }
        }
        
        public bool MapLineage
        {
            get { return lineageCheckBox.IsChecked.Value; }
        }
        
        private void LoadCurrentState()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            
            /*
            Dictionary<string, bool> paramsSpecified = new Dictionary<string, bool>();
            if (_parametersState != null)
            {
                foreach (var param in _parametersState)
                {
                    if (SelectedParameterValues.ContainsKey(param.Name) && SelectedParameterValues[param.Name].Count > 0)
                    {
                        paramsSpecified[param.Name] = true;
                    }
                    else
                    {
                        paramsSpecified[param.Name] = false;
                    }
                }
            }
            */

            UpdateParamsState();

            foreach (var param in _parametersState)
            {
                
                // TODO: does this correctly exclude hidden parameters
                if (string.IsNullOrEmpty(param.Prompt) && param.State == ParameterStateEnum.HasValidValue)
                {
                    continue;                  
                }                 
                RefreshParameterControl(param);
                 
                /*
                if (!paramsSpecified.ContainsKey(param.Name) || paramsSpecified[param.Name] == false || param.State != ParameterStateEnum.HasValidValue)
                {
                    RefreshParameterControl(param);
                }
                */                          
            }

            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void RefreshParameterControl(ReportParameter param)
        {
            System.Windows.UIElement paramControl = null;
            if (_paramControls.ContainsKey(param.Name))
            {
                paramControl = _paramControls[param.Name];
            }

            var dependenciesSpecified = CheckDependenciesSet(param);

            if (paramControl == null)
            {
                GridLengthConverter glc = new GridLengthConverter();

                paramsGrid.RowDefinitions.Add(new RowDefinition() { Height = (GridLength)glc.ConvertFrom("Auto") });

                var promptLabel = new Label() { Content = param.Prompt };
                paramsGrid.Children.Add(promptLabel);
                Grid.SetColumn(promptLabel, 0);
                Grid.SetRow(promptLabel, paramsGrid.RowDefinitions.Count - 1);

                var valueLabel = new Label() { Content = string.Empty };
                paramsGrid.Children.Add(valueLabel);
                Grid.SetColumn(valueLabel, 1);
                Grid.SetRow(valueLabel, paramsGrid.RowDefinitions.Count - 1);
                _paramControls[param.Name] = valueLabel;
            }


            paramControl = _paramControls[param.Name];

            if (!dependenciesSpecified)
            {
                var row = Grid.GetRow(paramControl);
                var column = Grid.GetColumn(paramControl);
                paramsGrid.Children.Remove(paramControl);
                TextBlock tBlock = new TextBlock() { TextWrapping = TextWrapping.Wrap };
                StringBuilder message = new StringBuilder();
                message.AppendLine("Depends on:");
                foreach (var dep in param.Dependencies)
                {
                    var depPar = _parametersState.First(x => x.Name == dep);
                    message.AppendLine(depPar.Prompt);
                }
                tBlock.Text = message.ToString();

                _paramControls[param.Name] = tBlock;
                paramsGrid.Children.Add(tBlock);
                Grid.SetRow(tBlock, row);
                Grid.SetColumn(tBlock, column);

                return;
            }

            if (param.ValidValuesQueryBased)
            {
                if (param.MultiValue)
                {
                    DisplayParamCheckComboBox(param, paramControl);
                }
                else
                {
                    DisplayParamComboBox(param, paramControl);
                }
            }
            //else if (!param.MultiValue && param.TypeSpecified && param.Type == ParameterTypeEnum.DateTime)
            //{
            //    DisplayParamDatePicker(param, paramControl);
            //}
            else
            {
                DisplayParamTextBox(param, paramControl);
            }
        }

        private void DisplayParamComboBox(ReportParameter param, UIElement gridElement)
        {
            var comboBoxControl = gridElement as ComboBox;
            if (comboBoxControl == null)
            {
                var row = Grid.GetRow(gridElement);
                var column = Grid.GetColumn(gridElement);
                paramsGrid.Children.Remove(gridElement);

                ComboBox cBox = new ComboBox();
                paramsGrid.Children.Add(cBox);
                Grid.SetRow(cBox, row);
                Grid.SetColumn(cBox, column);
                comboBoxControl = cBox;
                _paramControls[param.Name] = cBox;
            }

            comboBoxControl.ItemsSource = param.ValidValues;
            comboBoxControl.DisplayMemberPath = "Label";
            comboBoxControl.SelectedValuePath = "Value";

            var defaultValue = param.DefaultValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(defaultValue))
            {
                var selectedItem = param.ValidValues.FirstOrDefault(x => x.Value == defaultValue);
                if (selectedItem != null)
                {
                    comboBoxControl.SelectedItem = selectedItem;
                }
            }
        }

        private void DisplayParamDatePicker(ReportParameter param, UIElement gridElement)
        {
            var datePickerControl = gridElement as DatePicker;
            if (datePickerControl == null)
            {
                var row = Grid.GetRow(gridElement);
                var column = Grid.GetColumn(gridElement);
                paramsGrid.Children.Remove(gridElement);

                DatePicker picker = new DatePicker();
                paramsGrid.Children.Add(picker);
                Grid.SetRow(picker, row);
                Grid.SetColumn(picker, column);
                datePickerControl = picker;
                _paramControls[param.Name] = picker;
            }
            
            var defaultValue = param.DefaultValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(defaultValue))
            {
                DateTime dateTime = DateTime.MinValue;
                if (DateTime.TryParse(defaultValue, out dateTime))
                {
                    datePickerControl.SelectedDate = dateTime;
                }
            }
        }

        private void DisplayParamCheckComboBox(ReportParameter param, UIElement gridElement)
        {
            var comboBoxControl = gridElement as MultiSelectComboBox;
            if (comboBoxControl == null)
            {
                var row = Grid.GetRow(gridElement);
                var column = Grid.GetColumn(gridElement);
                paramsGrid.Children.Remove(gridElement);

                MultiSelectComboBox cBox = new MultiSelectComboBox();
                paramsGrid.Children.Add(cBox);
                Grid.SetRow(cBox, row);
                Grid.SetColumn(cBox, column);
                comboBoxControl = cBox;
                _paramControls[param.Name] = cBox;
            }

            //comboBoxControl.DisplayMemberPath = "Label";
            //comboBoxControl.ValueMemberPath = "Value";
            comboBoxControl.SelectedItems = new Dictionary<string, object>();

            comboBoxControl.ItemsSource = param.ValidValues.ToDictionary(x => x.Label, x => (object)x.Value); // param.ValidValues;

            if (param.DefaultValues != null)
            {
                /**/
                
                foreach (var defVal in param.DefaultValues)
                {
                    var selectedItem = param.ValidValues.FirstOrDefault(x => x.Value == defVal);
                    if (selectedItem != null)
                    {
                        comboBoxControl.SelectedItems.Add(selectedItem.Label, selectedItem.Value); // .Add(selectedItem);
                    }
                }
                /**/
            }
        }

        private void DisplayParamTextBox(ReportParameter param, UIElement gridElement)
        {
            var textBoxControl = gridElement as TextBox;
            if (textBoxControl == null)
            {
                var row = Grid.GetRow(gridElement);
                var column = Grid.GetColumn(gridElement);
                paramsGrid.Children.Remove(gridElement);
                
                TextBox tBox = new TextBox();
                paramsGrid.Children.Add(tBox);
                Grid.SetRow(tBox, row);
                Grid.SetColumn(tBox, column);
                textBoxControl = tBox;
                _paramControls[param.Name] = tBox;
            }

            if (param.DefaultValues.Any())
            {
                var tbVal = string.Join(";", param.DefaultValues);
                textBoxControl.Text = tbVal;
            }
        }

        private void RetrieveParameterValues()
        {
            if (_parametersState == null)
            {
                _parametersState = new List<ReportParameter>();
                _parametersState = new List<ReportParameter>();
            }
            foreach (var param in _parametersState)
            {
                if (!_paramControls.ContainsKey(param.Name))
                {
                    SelectedParameterValues[param.Name] = new List<string>();
                    continue;
                }

                var paramControl = _paramControls[param.Name];

                List<string> values = null;
                if (param.ValidValuesQueryBased)
                {
                    if (param.MultiValue)
                    {
                        values = GetParamValuesCheckComboBox(param, paramControl);
                    }
                    else if (param.TypeSpecified && param.Type == ParameterTypeEnum.DateTime)
                    {
                        values = GetParamValuesDateTextBox(param, paramControl);
                    }
                    //else if (!param.MultiValue && param.TypeSpecified && param.Type == ParameterTypeEnum.DateTime)
                    //{
                    //    values = GetParamValuesDatePicker(param, paramControl);
                    //}
                    else
                    {
                        values = GetParamValuesComboBox(param, paramControl);
                    }
                }
                else
                {
                    values = GetParamValuesTextBox(param, paramControl);
                }

                if (values != null)
                {
                    SelectedParameterValues[param.Name] = values;
                }
                else
                {
                    SelectedParameterValues[param.Name] = new List<string>();
                }
            }
        }

        private List<string> GetParamValuesComboBox(ReportParameter param, UIElement gridElement)
        {
            var comboBoxControl = gridElement as ComboBox;
            if (comboBoxControl == null)
            {
                return new List<string>();
            }
            var val = comboBoxControl.SelectedValue.ToString();
            if (string.IsNullOrEmpty(val))
            {
                return new List<string>();
            }
            return new List<string>() { val };
        }

        private List<string> GetParamValuesCheckComboBox(ReportParameter param, UIElement gridElement)
        {
            var comboBoxControl = gridElement as MultiSelectComboBox;
            if (comboBoxControl == null)
            {
                return new List<string>();
            }
            if (comboBoxControl.SelectedItems.Count == 0)
            {
                return new List<string>();
            }
            List<string> res = new List<string>();
            foreach (var val in comboBoxControl.SelectedItems)
            {
                res.Add((string)(val.Value));
            }
            return res;
        }

        private List<string> GetParamValuesDatePicker(ReportParameter param, UIElement gridElement)
        {
            var datePickerControl = gridElement as DatePicker;
            if (datePickerControl == null)
            {
                return new List<string>();
            }
            if (!datePickerControl.SelectedDate.HasValue)
            {
                return new List<string>();
            }
            List<string> res = new List<string>();
            res.Add(datePickerControl.SelectedDate.Value.ToString("M/d/yyyy h:mm tt"));
            return res;
        }

        private List<string> GetParamValuesTextBox(ReportParameter param, UIElement gridElement)
        {
            var textBoxControl = gridElement as TextBox;
            if (textBoxControl == null)
            {
                return new List<string>();
            }
            var res = textBoxControl.Text.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
            return res;
        }

        private List<string> GetParamValuesDateTextBox(ReportParameter param, UIElement gridElement)
        {
            var textBoxControl = gridElement as TextBox;
            if (textBoxControl == null)
            {
                return new List<string>();
            }
            var resText = textBoxControl.Text.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(x => x.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)).ToList();
            return resText;
        }

        private bool CheckDependenciesSet(ReportParameter param)
        {

            return param.State != ParameterStateEnum.HasOutstandingDependencies;

            //foreach (var dep in param.Dependencies)
            //{
            //    var depParState = _parametersState.Parameters.First(x => x.Name == dep);
            //    if(depParState.State != ParameterStateEnum.)
            //}
        }

        private void UpdateParamsState()
        {
            var parametersArray = GetParametersArray();
            
            ExecutionInfo execInfo = new ExecutionInfo();
            ExecutionHeader execHeader = new ExecutionHeader();

            _reportExecutionService.ExecutionHeaderValue = execHeader;

            execInfo = _reportExecutionService.LoadReport(_reportItemPath, null);
            
            _reportExecutionService.SetExecutionParameters(parametersArray, "en-us");
            _executionInfo = _reportExecutionService.GetExecutionInfo();
            
            var paramsState = _executionInfo.Parameters;
            _parametersState = paramsState.ToList();
          
        }

        public ParameterValue[] GetParametersArray()
        {
            List<ParameterValue> parameters = new List<ParameterValue>();
            foreach (var parameter in _selectedParameterValues)
            {
                foreach (var parameterValue in parameter.Value)
                {
                    parameters.Add(new ParameterValue() { Name = parameter.Key, Value = parameterValue });
                }
            }

            var parametersArray = parameters.ToArray();
            return parametersArray;
        }

        //private ListSsrsReportsRequestResponse GetReportList()
        //{

        //    var contentRequest = new ListSsrsReportsRequest();
        //    return (ListSsrsReportsRequestResponse)ApiClient.PostShortMessage(contentRequest);
        //}

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{
        //    //ApiClient.InitApiAuth(true);
        //    //_apiClient = new ApiClientInstance();
        //    //_apiClient.InitApiAuth();
        //    LoadCurrentState();
        //}

        private void btnViewReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RetrieveParameterValues();
                LoadCurrentState();
            }
            catch (Exception ex)
            {
                ShowException(ex);
                return;
            }

            if (_parametersState.All(x => x.State == ParameterStateEnum.HasValidValue || !x.StateSpecified))
            {
                if (RenderReportClick != null)
                {
                    RenderReportClick(this, new EventArgs());
                }
            }
            else
            {
                StringBuilder errorMessage = new StringBuilder();
                foreach (var param in _parametersState.Where(x => x.State != ParameterStateEnum.HasValidValue && x.StateSpecified))
                {
                    errorMessage.AppendLine(string.Format("Parameter {0} does not have a valid value", param.Prompt));
                    if (!string.IsNullOrEmpty(param.ErrorMessage))
                    {
                        errorMessage.AppendLine(param.ErrorMessage);
                    }
                }
                System.Windows.MessageBox.Show(errorMessage.ToString(), "Parameters not valid", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RetrieveParameterValues();
                LoadCurrentState();
            }
            catch (Exception ex)
            {
                ShowException(ex);
                return;
            }
        }

        private void ShowException(Exception ex)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            System.Windows.MessageBox.Show("Report Server Error: " + ex.Message, "Report Server Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
