namespace NRWH_Tools_Addin
{
    partial class Ribbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Ribbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl1 = this.Factory.CreateRibbonDropDownItem();
            Microsoft.Office.Tools.Ribbon.RibbonDropDownItem ribbonDropDownItemImpl2 = this.Factory.CreateRibbonDropDownItem();
            this.tab1 = this.Factory.CreateRibbonTab();
            this.group1 = this.Factory.CreateRibbonGroup();
            this.nrwhToolsTab = this.Factory.CreateRibbonTab();
            this.displayGrp = this.Factory.CreateRibbonGroup();
            this.viewDropDown = this.Factory.CreateRibbonDropDown();
            this.group2 = this.Factory.CreateRibbonGroup();
            this.btnSynchronize = this.Factory.CreateRibbonButton();
            this.tab1.SuspendLayout();
            this.nrwhToolsTab.SuspendLayout();
            this.displayGrp.SuspendLayout();
            this.group2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Groups.Add(this.group1);
            this.tab1.Label = "TabAddIns";
            this.tab1.Name = "tab1";
            // 
            // group1
            // 
            this.group1.Label = "group1";
            this.group1.Name = "group1";
            // 
            // nrwhToolsTab
            // 
            this.nrwhToolsTab.Groups.Add(this.displayGrp);
            this.nrwhToolsTab.Groups.Add(this.group2);
            this.nrwhToolsTab.Label = "NRWH Tools";
            this.nrwhToolsTab.Name = "nrwhToolsTab";
            // 
            // displayGrp
            // 
            this.displayGrp.Items.Add(this.viewDropDown);
            this.displayGrp.Label = "Display";
            this.displayGrp.Name = "displayGrp";
            // 
            // viewDropDown
            // 
            ribbonDropDownItemImpl1.Label = "Old";
            ribbonDropDownItemImpl2.Label = "New";
            this.viewDropDown.Items.Add(ribbonDropDownItemImpl1);
            this.viewDropDown.Items.Add(ribbonDropDownItemImpl2);
            this.viewDropDown.Label = "View";
            this.viewDropDown.Name = "viewDropDown";
            // 
            // group2
            // 
            this.group2.Items.Add(this.btnSynchronize);
            this.group2.Label = "Data";
            this.group2.Name = "group2";
            // 
            // btnSynchronize
            // 
            this.btnSynchronize.Label = "Synchronize";
            this.btnSynchronize.Name = "btnSynchronize";
            this.btnSynchronize.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnSynchronize_Click);
            // 
            // Ribbon
            // 
            this.Name = "Ribbon";
            this.RibbonType = "Microsoft.Excel.Workbook";
            this.Tabs.Add(this.tab1);
            this.Tabs.Add(this.nrwhToolsTab);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Ribbon_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.nrwhToolsTab.ResumeLayout(false);
            this.nrwhToolsTab.PerformLayout();
            this.displayGrp.ResumeLayout(false);
            this.displayGrp.PerformLayout();
            this.group2.ResumeLayout(false);
            this.group2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group1;
        private Microsoft.Office.Tools.Ribbon.RibbonTab nrwhToolsTab;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup displayGrp;
        internal Microsoft.Office.Tools.Ribbon.RibbonDropDown viewDropDown;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup group2;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSynchronize;
    }

    partial class ThisRibbonCollection
    {
        internal Ribbon Ribbon
        {
            get { return this.GetRibbon<Ribbon>(); }
        }
    }
}
