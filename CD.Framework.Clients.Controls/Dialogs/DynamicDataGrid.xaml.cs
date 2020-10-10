using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace CD.DLS.Clients.Controls.Dialogs
{

    public class DataGridField
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public bool Freeze { get; set; }
        public bool Editable { get; set; }
        public bool Visible { get; set; }
        public int Order { get; set; }
        public string TableColumnName { get { return "FLD_" + Name; } }
    }

    public class DataGridFieldValue
    {
        public int RowId { get; set; }
        public int FieldId { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Interaction logic for ListPicker.xaml
    /// </summary>
    public partial class DynamicDataGrid : UserControl
    {
        private List<DataGridField> _fields;
        private List<DataGridFieldValue> _values;
        private Dictionary<int, DataGridField> _fieldsById;
        private Dictionary<string, DataGridField> _fieldsByTableColumn;

        public class DynamicDataGridEditArgs : EventArgs
        {
            public List<DataGridFieldValue> Values { get; set; }
        }

        public delegate void DynamicGridEditHandler(object sender, DynamicDataGridEditArgs e);

        public event DynamicGridEditHandler DynamicGridEdit;

        private DataTable _dt;

        public DataTable Dt
        {
            get
            {
                return _dt;
            }

            set
            {
                _dt = value;
            }
        }

        public DynamicDataGrid()
        {
            InitializeComponent();
        }
        
        public void SetData(List<DataGridField> fields, List<DataGridFieldValue> values)
        {
            _fields = fields;
            _values = values;

            var visibleFields = _fields.Where(x => x.Visible).OrderBy(x => !x.Freeze).ThenBy(x => x.Order).ToList();

            grid.FrozenColumnCount = visibleFields.Where(x => x.Freeze).Count();
            grid.Columns.Clear();

            foreach (var field in visibleFields)
            {
                DataGridTextColumn textCol = new DataGridTextColumn();
                textCol.Header = field.Name;
                textCol.Binding = new Binding(field.TableColumnName);
                textCol.IsReadOnly = !field.Editable;
                grid.Columns.Add(textCol);
            }

            Dt = new DataTable();
            Dt.Columns.Add("ID");
            foreach (var fld in _fields)
            {
                Dt.Columns.Add(fld.TableColumnName);
            }

            _fieldsById = _fields.ToDictionary(x => x.Id, x => x);
            _fieldsByTableColumn = _fields.ToDictionary(x => x.TableColumnName, x => x);

            var itemGroups = _values.GroupBy(x => x.RowId);
            foreach (var itemGroup in itemGroups)
            {
                var nr = Dt.NewRow();
                foreach (DataGridFieldValue fieldValue in itemGroup)
                {
                    nr[_fieldsById[fieldValue.FieldId].TableColumnName] = fieldValue.Value;
                }
                nr["ID"] = itemGroup.Key;
                Dt.Rows.Add(nr);
            }

            Dt.RowChanged += Dt_RowChanged;
            grid.DataContext = Dt;
            grid.ItemsSource = _dt.DefaultView;
            //var b = new Binding("Dt");
            //b.Mode = BindingMode.TwoWay;
            //b.up
            //grid.ItemsSource = new Binding("Dt"); Dt.DefaultView;
            //grid.mode
        }

        private void Dt_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (DynamicGridEdit == null)
            {
                return;
            }
            List<DataGridFieldValue> values = new List<DataGridFieldValue>();
            var rowId = int.Parse(e.Row["ID"].ToString());
            foreach (DataColumn col in e.Row.Table.Columns)
            {
                if (col.ColumnName == "ID")
                {
                    continue;
                }
                var field = _fieldsByTableColumn[col.ColumnName];
                if (!field.Editable)
                {
                    continue;
                }
                if (e.Row[col] == DBNull.Value)
                {
                    continue;
                }
                values.Add(new DataGridFieldValue() { FieldId = field.Id, RowId = rowId, Value = (string)e.Row[col] });
            }
            DynamicGridEdit(this, new DynamicDataGridEditArgs() { Values = values });
        }

        private void grid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {

            }

        }
    }
}
