using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CD.DLS.DAL.Objects.BusinessDictionaryIndex;
using CD.DLS.Clients.Controls.Dialogs.ExcelBusinessDictionary;

namespace CD.Framework.ExcelAddin16.Panes
{
    public partial class BusinessDictionaryPane : UserControl
    {
        private ExcelBusinessDictionary _control;

        public bool HasEditPermissions { get; set; }

        public BusinessDictionaryPane()
        {
            InitializeComponent();

            this.Resize += BusinessDictionaryPane_Resize;

            _control = (ExcelBusinessDictionary)(elementHost1.Child);

            _control.SaveClicked += Control_SaveClicked;
            _control.DetailsClicked += Control_DetailsClicked;
            HasEditPermissions = false;
            //this.VerticalScroll.Enabled = true;
            //this.VerticalScroll.Visible = true;
            //elementHost1.AutoSize = true;
            //this.AutoSize = true;
        }

        public event BusinessDictionaryPaneHandler SaveClicked;
        public event BusinessDictionaryPaneHandler DetailsClicked;

        private void BusinessDictionaryPane_Resize(object sender, EventArgs e)
        {
            elementHost1.Width = this.Width;
            elementHost1.Height = this.Height;
        }

        internal void LoadContent(OlapFieldLookupResult olapField, BusinessDictionaryIndex businessDictionaryIndex, string fieldName)
        {
            _control.LoadContent(olapField, businessDictionaryIndex, fieldName);
                 //               < !--< Label Content = "Field 1 Name" />
  
                 //     < TextBox HorizontalAlignment = "Stretch" TextWrapping = "Wrap" VerticalScrollBarVisibility = "Visible" MinLines = "3" MaxLines = "5" AcceptsReturn = "True"
                 //Text = "Lorem ipsum Lorem ipsum" />
        }

        private void Control_SaveClicked(object sender, BusinessDictionaryPaneEventArgs e)
        {
            if (SaveClicked != null)
            {
                SaveClicked(sender, e);
            }
        }

        private void Control_DetailsClicked(object sender, BusinessDictionaryPaneEventArgs e)
        {
            if (DetailsClicked != null)
            {
                DetailsClicked(sender, e);
            }
        }

        public void ShowSavedIndicator()
        {
            _control.ShowSavedIndicator();
        }

        public void ShowMissingPermissionsIndicator()
        {
            _control.ShowMissingPermissionsIndicator();
        }

        public void RefreshData()
        {
            _control.RefreshData();
        }


    }
}
