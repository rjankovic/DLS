using Xls = Microsoft.Office.Interop.Excel;
using NRWH_Tools_Addin.ApplicationState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRWH_Tools_Addin.ExcelManager
{
    class ExcelOperations
    {
        public WorkbookState UpdateWorkbookState(Xls.Workbook workbook, WorkbookState appState)
        {
            if (appState == null)
            {
                appState = WorkbookDetector.Detect(workbook);
            }
            foreach (var sheet in appState.Sheets)
            {
                var correspondingSheet = workbook.Sheets[sheet.SheetName];
                if (sheet is ClassListSheetState)
                {
                    // only new rows are read here - the cnaged ones are tracked by OnSheetChanged
                    ReadNewRowsFromClassListSheet((ClassListSheetState)sheet, correspondingSheet);
                }
            }
            
            return appState;
        }
        
        private void ReadNewRowsFromClassListSheet(ClassListSheetState state, Xls.Worksheet sourceSheet)
        {
            var rowIndex = state.FirstRowOffset + 1 + state.RowStates.Count;

            //int currentRowId = -1;
            //var idColumn = state.IdColumnIndex;

            var rowState = ReadRowState(state, rowIndex, sourceSheet);
            while (rowState != null)
            {
                state.RowStates.Add(rowState);
                rowIndex++;
                rowState = ReadRowState(state, rowIndex, sourceSheet);
            }
                /*
            while (int.TryParse(((string)(((Xls.Range)sourceSheet.Cells[rowIndex, idColumn]).Value2)), out currentRowId))
            {
                RowState
            }
            */
        }

        private RowState ReadRowState(ClassListSheetState listState, int rowIndex, Xls.Worksheet sheet)
        {
            var idColumn = listState.IdColumnIndex;
            var idCell = (Xls.Range)sheet.Cells[rowIndex, idColumn];
            if (idCell == null)
            {
                return null;
            }
            if (idCell.Value2 == null)
            {
                return null;
            }
            int idParsed;

            if (typeof(double).IsAssignableFrom(idCell.Value2.GetType()))
            {
                var doubleId = (double)(idCell.Value2);
                idParsed = (int)doubleId;
            }
            else if (!int.TryParse((string)idCell.Value2, out idParsed))
            {
                return null;
            }
            RowState rowState = new RowState()
            {
                ChangedInXls = true,
                ChangedInEa = false,
                Id = idParsed,
                RowOffset = rowIndex - 1,
                ModifiedDate = DateTimeOffset.UtcNow,
                ModifiedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name
            };

            Xls.Range row = sheet.Rows[rowIndex];
            var values = (System.Array)row.Cells.Value;
            rowState.Values = new List<string>();
            var maxColCount = listState.GetColumnCount();
            foreach (var val in values)
            {
                if (rowState.Values.Count > maxColCount)
                {
                    break;
                }
                rowState.Values.Add((val != null ? val : string.Empty).ToString());
            }

            return rowState;
        }

        /// <summary>
        /// Update workbook with changes from EA
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="wbState"></param>
        public void UpdateLocalWorkbook(Xls.Workbook workbook, WorkbookState wbState)
        {
            foreach (var sheetState in wbState.Sheets)
            {
                var sheetName = sheetState.SheetName;
                Xls.Worksheet sheet = workbook.Worksheets[sheetName];
                if (sheet == null)
                {
                    sheet = (Xls.Worksheet)workbook.Worksheets.Add(null, null, 1, null);
                    sheet.Name = sheetName;
                }
                if (sheetState is ClassListSheetState)
                {
                    UpdateLocalSheet(sheet, (ClassListSheetState)sheetState);
                }
            }
        }

        private void UpdateLocalSheet(Xls.Worksheet sheet, ClassListSheetState state)
        {
            // dictionary of ids already in XLS
            var rowIndex = state.FirstRowOffset + 1;
            Dictionary<int, int> idsToRows = new Dictionary<int, int>();
            int idParsed = -1;
            var cellRange = (Xls.Range)(sheet.Cells[rowIndex, state.IdColumnIndex]);
            var cellValue = cellRange.Value2;
            while (rowIndex <= sheet.Rows.Count && cellValue != null && int.TryParse(((object)cellValue).ToString(), out idParsed))
            {
                idsToRows.Add(idParsed, rowIndex++);
                cellRange = (Xls.Range)(sheet.Cells[rowIndex, state.IdColumnIndex]);
                cellValue = cellRange.Value2;
            }

            for (int i = 0; i < state.RowStates.Count; i++)
            {
                var rowState = state.RowStates[i];
                if (!rowState.ChangedInEa)
                {
                    continue;
                }

                /*
                // new in EA - covered by last case
                if (!rowState.Deleted && !idsToRows.ContainsKey(rowState.Id))
                {
                    var newRowIndex = rowState.RowOffset + 1;
                    ((Xls.Range)sheet.Rows[newRowIndex, null]).Value = rowState.Values.ToArray();
                }
                // deleted in EA
                else 
                */
                if (rowState.Deleted)
                {
                    // delete from XLS
                    ((Xls.Range)sheet.Rows[rowState.RowOffset + 1, null]).Delete(Xls.XlDeleteShiftDirection.xlShiftUp);
                    state.RowStates.RemoveAt(i);
                    for (int j = i; j < state.RowStates.Count; j++)
                    {
                        state.RowStates[j].RowOffset--;
                    }
                    // the current item was removed
                    i--;
                }
                // new or modified in EA
                else
                {
                    var newRowIndex = rowState.RowOffset + 1;
                    ((Xls.Range)sheet.Rows[newRowIndex, null]).Value = rowState.Values.ToArray();
                }
                rowState.ChangedInEa = false;
            }
        }


        public void OnSheetChanged(WorkbookState wbState, Xls.Worksheet sheet, Xls.Range range)
        {
            if (wbState == null)
            {
                return;
            }
            var sheetState = wbState.Sheets.FirstOrDefault(x => x.SheetName == sheet.Name) as ClassListSheetState;
            if (sheetState == null)
            {
                return;
            }
            for(int i = 0; i < sheetState.RowStates.Count; i++)
            //foreach (var rowState in sheetState.RowStates)
            {
                var rowState = sheetState.RowStates[i];
                if (range.Row < rowState.RowOffset && range.Row + range.Rows.Count >= rowState.RowOffset)
                {
                    rowState = ReadRowState(sheetState, rowState.RowOffset + 1, sheet);
                    
                }
            }
        }
    }
}
