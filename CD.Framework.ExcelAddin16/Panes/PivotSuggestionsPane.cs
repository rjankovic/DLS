using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CD.DLS.Clients.Controls.Dialogs.ExcelPanes;
using CD.DLS.DAL.Managers;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.Learning;

namespace CD.Framework.ExcelAddin16.Panels
{
    public partial class WinFormsPivotSuggestionsPane : UserControl
    {
        private PivotSuggestionsPane _userControl;

        public event PivotSuggestionsHandler SuggestionDoubleClicked;
        public event PivotSuggestionsHandler SuggestionsChanged;


        public WinFormsPivotSuggestionsPane()
        {
            InitializeComponent();
            _userControl = (PivotSuggestionsPane)(elementHost1.Child);
            _userControl.SuggestionDoubleClicked += UserControl_SuggestionDoubleClicked;
            _userControl.SuggestionsChanged += UserControl_SuggestionsChanged;
        }

        public List<OlapField> CurrentSuggestions
        {
            get { return _userControl.CurrentSuggestions; }
        }
        
        public void Init(LearningManager learningManager, ProjectConfig projectConfig, OlapCubeRuleFilter cubeRuleFilter)
        {
            _userControl.Init(learningManager, projectConfig, cubeRuleFilter);
        }

        public void FieldsChanged(List<string> fieldSourceReferences)
        {
            _userControl.FieldsChanged(fieldSourceReferences);
        }

        private void UserControl_SuggestionsChanged(object sender, PivotSuggestionsEventArgs e)
        {
            if (SuggestionsChanged != null)
            {
                SuggestionsChanged(this, e);
            }
        }

        private void UserControl_SuggestionDoubleClicked(object sender, PivotSuggestionsEventArgs e)
        {
            if (SuggestionDoubleClicked != null)
            {
                SuggestionDoubleClicked(this, e);
            }
        }

        private void elementHost1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }
    }
}
