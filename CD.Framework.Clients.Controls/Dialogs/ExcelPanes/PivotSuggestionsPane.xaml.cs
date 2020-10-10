using CD.DLS.Common.Structures;
using CD.DLS.DAL.Lookup;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Learning;
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

namespace CD.DLS.Clients.Controls.Dialogs.ExcelPanes
{
    public class PivotSuggestionsEventArgs : EventArgs
    {
        public List<OlapField> SuggestedFields { get; set; }
    }

    public delegate void PivotSuggestionsHandler(object sender, PivotSuggestionsEventArgs e);
    
    /// <summary>
    /// Interaction logic for PivotSuggestionsPane.xaml
    /// </summary>
    public partial class PivotSuggestionsPane : UserControl
    {
        public PivotSuggestionsPane()
        {
            InitializeComponent();
        }

        private ProjectConfig _projectConfig = null;
        private LearningManager _learningManager = null;
        private OlapRuleSet _knowledgeBase = null;
        private OlapRuleLookup _olapRuleLookup = null;

        // selected fields that are mentioned in some rule
        private List<OlapField> _currentFields = new List<OlapField>();

        private List<OlapField> _currentSuggestions = new List<OlapField>();
        public List<OlapField> CurrentSuggestions { get { return _currentSuggestions; } }

        private Dictionary<string, OlapField> _ruleFieldsByReference = null;
        private Dictionary<int, OlapField> _ruleFieldsById = null;

        public event PivotSuggestionsHandler SuggestionsChanged;
        public event PivotSuggestionsHandler SuggestionDoubleClicked;
        private List<string> _previousFieldSourceReferences = null;
        private OlapCubeRuleFilter _currentFilter = null;

        public void Init(LearningManager learningManager, ProjectConfig projectConfig, OlapCubeRuleFilter cubeRuleFilter)
        {
            _learningManager = learningManager;
            _projectConfig = projectConfig;
            _currentSuggestions = new List<OlapField>();
            _currentFields = new List<OlapField>();
            
            if (!cubeRuleFilter.FiltersEqual(_currentFilter))
            {
                _knowledgeBase = _learningManager.LoadOlapRules(_projectConfig.ProjectConfigId, cubeRuleFilter);
                _currentFilter = cubeRuleFilter;
                _olapRuleLookup = new OlapRuleLookup(_knowledgeBase);
                _ruleFieldsByReference = _knowledgeBase.Fields.ToDictionary(x => x.FieldReference, x => x);
                _ruleFieldsById = _knowledgeBase.Fields.ToDictionary(x => x.OlapFieldId, x => x);
                
                listPicker.Selected -= ListPicker_Selected;
                listPicker.Selected += ListPicker_Selected;
            }

            RefreshSuggestionsView();
        }

        private void ListPicker_Selected(object sender, EventArgs e)
        {
            if (SuggestionDoubleClicked == null)
            {
                return;
            }

            SuggestionDoubleClicked(this, new PivotSuggestionsEventArgs()
            {
                SuggestedFields =
                new List<OlapField>() { _ruleFieldsById[int.Parse(listPicker.SelectedItem.Value)] }
            });
        }

        public void FieldAdded(string fieldSourceReference)
        {
            // impossible?
            var ruleAltreadyIncluded = _currentFields.FirstOrDefault(x => x.FieldReference == fieldSourceReference);
            if (ruleAltreadyIncluded != null)
            {
                return;
            }

            if (!_ruleFieldsByReference.ContainsKey(fieldSourceReference))
            {
                return;
            }

            _currentFields.Add(_ruleFieldsByReference[fieldSourceReference]);
            RefreshSuggestions();
        }

        public void FieldRemoved(string fieldSourceReference)
        {
            var ruleFieldRemoved = _currentFields.FirstOrDefault(x => x.FieldReference == fieldSourceReference);
            if (ruleFieldRemoved == null)
            {
                return;
            }

            _currentFields.Remove(ruleFieldRemoved);
            RefreshSuggestions();
        }

        public void FieldsChanged(List<string> fieldSourceReferences)
        {
            // return if the lists are identical
            /*
            if (_previousFieldSourceReferences != null)
            {
                if (_previousFieldSourceReferences.Count == fieldSourceReferences.Count)
                {
                    if (_previousFieldSourceReferences.Join(fieldSourceReferences, x => x, x => x, (a, b) => a).Count() == _previousFieldSourceReferences.Count)
                    {
                        return;
                    }
                }
            }
            */

            _previousFieldSourceReferences = fieldSourceReferences;
            _currentFields = new List<OlapField>();
            foreach (var reference in fieldSourceReferences)
            {
                if (!_ruleFieldsByReference.ContainsKey(reference))
                {
                    var lastDot = reference.LastIndexOf("].[");
                    if (lastDot > 0)
                    {
                        var preDot = reference.Substring(0, lastDot + 1);
                        if (!_ruleFieldsByReference.ContainsKey(preDot))
                        {
                            continue;
                        }
                        else
                        {
                            _currentFields.Add(_ruleFieldsByReference[preDot]);
                        }
                    }
                    continue;
                }
                _currentFields.Add(_ruleFieldsByReference[reference]);
            }

            RefreshSuggestions();
        }

        private void RefreshSuggestions()
        {
            var oldSuggestions = _currentSuggestions;
            var newSuggestions = _olapRuleLookup.SuggestFields(_currentFields);
            if (newSuggestions.Count != oldSuggestions.Count
                || newSuggestions.Any(x => !oldSuggestions.Any(y => y.OlapFieldId == x.Field.OlapFieldId)))
            {
                _currentSuggestions = newSuggestions.OrderByDescending(x => x.Weight).Select(x => x.Field).ToList();
                RefreshSuggestionsView();
                if (SuggestionsChanged != null)
                {
                    SuggestionsChanged(this, new PivotSuggestionsEventArgs() { SuggestedFields = _currentSuggestions });
                }
            }
        }

        private void RefreshSuggestionsView()
        {
            listPicker.Init(_currentSuggestions.Select(x => new ListPicker.ListPickerItem()
            {
                Name = x.FieldName,
                Tooltip = string.Format("{0} {1}", x.FieldType.ToString(), x.FieldReference),
                Value = x.OlapFieldId.ToString()
            }).ToList(), false);
        }
    }
}
