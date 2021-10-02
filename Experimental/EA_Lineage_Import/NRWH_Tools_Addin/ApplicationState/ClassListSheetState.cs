using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRWH_Tools_Addin.ApplicationState
{
    class ClassListSheetState : SheetState
    {
        public ClassListSheetState()
        {
            ColumnMappings = new List<ColumnMapping>();
            RowStates = new List<RowState>();
        }
        public List<ColumnMapping> ColumnMappings { get; set; }
        public int IdColumnIndex { get; set; }
        public List<RowState> RowStates { get; set; }
        public int FirstRowOffset { get; set; }

        public int GetColumnCount()
        {
            return ColumnMappings.Max(x => x.ColumnIndex);
        }

        private Dictionary<int, RowState> _rowIndex = null;

        public void RebuildRowIndex()
        {
            _rowIndex = RowStates.ToDictionary(x => x.Id, x => x);
        }

        public RowState GetRowById(int id)
        {
            if (_rowIndex == null)
            {
                RebuildRowIndex();
            }
            if (!_rowIndex.ContainsKey(id))
            {
                return null;
            }
            return _rowIndex[id];
        }
    }
}
