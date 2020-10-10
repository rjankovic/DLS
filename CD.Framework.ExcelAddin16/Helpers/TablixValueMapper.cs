using CD.DLS.DAL.Configuration;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.Framework.ExcelAddin16.Helpers
{
    class TablixValueMapper
    {
        private Workbook _workbook;
        private Dictionary<string, string[,]> _worksheetValues = null;
        private Dictionary<string, Tuple<int, int>> _worksheetUsedRangeOffsets = null;
        List<string> _sheetsList = null;

        public TablixValueMapper(Workbook workbook)
        {
            _workbook = workbook;
            ReadWorkbookValues();
        }

        private void ReadWorkbookValues()
        {
            _worksheetValues = new Dictionary<string, string[,]>();
            _worksheetUsedRangeOffsets = new Dictionary<string, Tuple<int, int>>();
            _sheetsList = new List<string>();

            foreach (Worksheet ws in _workbook.Worksheets)
            {
                _sheetsList.Add(ws.Name);
                Range usedRange = ws.UsedRange; //.Cells.Value;

                var topRow = usedRange.Row;
                var leftColumn = usedRange.Column;
                _worksheetUsedRangeOffsets.Add(ws.Name, new Tuple<int, int>(topRow - 1, leftColumn - 1));
                //if (topRow != 1 || leftColumn != 1)
                //{
                //    throw new Exception();
                //}

                var rangeValues = usedRange.Cells.Value;
                var valuesArray = ConvertToStringArray(rangeValues);
                _worksheetValues.Add(ws.Name, valuesArray);
            }
        }

        public bool FindTablix(ReportTextBoxValue[,] tablixValues)
        {
            var foundTablix = FindTablixFromCell(tablixValues, 0, 0, 0);

            if (foundTablix == null)
            {
                return false;
            }
            int topRow = int.MaxValue;
            int rightColumn = int.MinValue;
            string sheetName = null;
            for (int i = 0; i < foundTablix.GetLength(0); i++)
            {
                for (int j = 0; j < foundTablix.GetLength(1); j++)
                {
                    if (foundTablix[i, j] != null)
                    {
                        if (!string.IsNullOrWhiteSpace(foundTablix[i, j].Value))
                        {
                            if (foundTablix[i, j].RenderedRow < topRow)
                            {
                                topRow = foundTablix[i, j].RenderedRow;
                            }
                            if (foundTablix[i, j].RenderedColumn >= rightColumn)
                            {
                                rightColumn = foundTablix[i, j].RenderedColumn;
                            }
                            if (sheetName == null)
                            {
                                sheetName = foundTablix[i, j].RenderedSheet;
                            }
                        }
                    }
                }
            }

            var sheetOffset = _sheetsList.IndexOf(sheetName);

            // another tablix found after this on (to the right and bottom) - the mapping isn't unique => not found
            var anotherTablixFound = FindTablixFromCell(tablixValues, sheetOffset, topRow, rightColumn);
            if (anotherTablixFound != null)
            {
                return false;
            }

            for (int i = 0; i < foundTablix.GetLength(0); i++)
            {
                for (int j = 0; j < foundTablix.GetLength(1); j++)
                {
                    if (foundTablix[i, j] != null)
                    {
                        tablixValues[i, j].RenderedSheet = foundTablix[i, j].RenderedSheet;
                        tablixValues[i, j].RenderedRow = foundTablix[i, j].RenderedRow 
                            + _worksheetUsedRangeOffsets[foundTablix[i, j].RenderedSheet].Item1;
                        tablixValues[i, j].RenderedColumn = foundTablix[i, j].RenderedColumn
                            + _worksheetUsedRangeOffsets[foundTablix[i, j].RenderedSheet].Item2;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Adds 1-based sheet, row and column indicators to a copy of tablixValues[,] if found
        /// </summary>
        /// <param name="tablixValues"></param>
        /// <param name="sheetOffset"></param>
        /// <param name="rowOffset"></param>
        /// <param name="columnOffset"></param>
        /// <returns></returns>
        private ReportTextBoxValue[,] FindTablixFromCell(ReportTextBoxValue[,] tablixValues, int sheetOffset, int rowOffset, int columnOffset)
        {
            ReportTextBoxValue[,] valuesCopy = new ReportTextBoxValue[tablixValues.GetLength(0), tablixValues.GetLength(1)];
            for (int i = 0; i < valuesCopy.GetLength(0); i++)
            {
                for (int j = 0; j < valuesCopy.GetLength(1); j++)
                {
                    if (tablixValues[i, j] != null)
                    {
                        valuesCopy[i, j] = tablixValues[i, j].ShallowCopy();
                    }
                }
            }

            int firstNonEmptyValueRowOffset = 0;
            int firstNonEmptyValueColumnOffset = 0;

            while (valuesCopy[firstNonEmptyValueRowOffset, firstNonEmptyValueColumnOffset] == null
                || valuesCopy[firstNonEmptyValueRowOffset, firstNonEmptyValueColumnOffset].Value == string.Empty)
            {
                firstNonEmptyValueColumnOffset++;
                if (firstNonEmptyValueColumnOffset >= valuesCopy.GetLength(1))
                {
                    firstNonEmptyValueColumnOffset = 0;
                    firstNonEmptyValueRowOffset++;
                }
                if (firstNonEmptyValueRowOffset >= valuesCopy.GetLength(0))
                {
                    return null;
                }
            }

            int testRow = rowOffset;
            int testColumn = columnOffset;
            for (int sh = sheetOffset; sh < _sheetsList.Count; sh++)
            {
                var sheetName = _sheetsList[sh];

                var sheetValues = _worksheetValues[sheetName];

                // find first cell of the sheet (starting from the offset) that matches
                // the first non-empty cell of the tablix(found earlier)
                while (true)
                {
                    // end of row
                    if (testColumn >= sheetValues.GetLength(1))
                    {
                        testColumn = 0;
                        testRow++;
                    }

                    //end of sheet
                    if (testRow >= sheetValues.GetLength(0))
                    {
                        break;
                    }

                    if (sheetValues[testRow, testColumn] != null 
                        && testRow >= firstNonEmptyValueRowOffset
                        && testColumn >= firstNonEmptyValueColumnOffset)
                    {
                        if (CompareReportValues(sheetValues[testRow, testColumn],
                            valuesCopy[firstNonEmptyValueRowOffset, firstNonEmptyValueColumnOffset].Value))
                        {
                            // calculate currentColumnLM = testColumn - firstNonEmptyValueColumnOffset
                            // [while ignoring merged cells (sheetValues == null)]
                            var columnOffsetWithMergedCells = 0;
                            var leftStepsLeft = firstNonEmptyValueColumnOffset;
                            int currentColumnLM = testColumn;
                            while (leftStepsLeft > 0 && currentColumnLM > 0)
                            {
                                currentColumnLM--;
                                if (sheetValues[testRow, currentColumnLM] != null)
                                {
                                    leftStepsLeft--;
                                }
                            }

                            if (TryMapTablixFromCell(valuesCopy, sheetValues,
                                currentColumnLM,
                                //testColumn - firstNonEmptyValueColumnOffset,
                                testRow - firstNonEmptyValueRowOffset))
                            {
                                // match found, set rendered sheet name
                                for (int mappedRow = 0; mappedRow < valuesCopy.GetLength(0); mappedRow++)
                                {
                                    for (int mappedColumn = 0; mappedColumn < valuesCopy.GetLength(1); mappedColumn++)
                                    {
                                        if (valuesCopy[mappedRow, mappedColumn] != null)
                                        {
                                            valuesCopy[mappedRow, mappedColumn].RenderedSheet = sheetName;
                                        }
                                    }
                                }

                                return valuesCopy;
                            }
                        }
                    }

                    testColumn++;
                }

                testRow = 0;
                testColumn = 0;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tablixValues">zero-based</param>
        /// <param name="sheetValues">indexes zero based, rendered row and column numbers 1-based</param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        private bool TryMapTablixFromCell(ReportTextBoxValue[,] tablixValues, string[,] sheetValues, int left, int top)
        {
            int currentSheetRow = top;
            int currentSheetColumn = left;
            
            //int currentTablixRow = 0;
            //int currentTablixColumn = 0;

            for (int tablixRow = 0; tablixRow < tablixValues.GetLength(0); tablixRow++)
            {
                // the first non-empty column in this row (allow skipping sheet rows)
                bool firstColumn = true;

                for (int tablixColumn = 0; tablixColumn < tablixValues.GetLength(1); tablixColumn++)
                {
                    // do not search for empty strings
                    if (tablixValues[tablixRow, tablixColumn] == null)
                    {
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(tablixValues[tablixRow, tablixColumn].Value))
                    {
                        continue;
                    }
                    
                    // find first non-empty sheet cell to compare with
                    while (true)
                    {
                        if (currentSheetColumn >= sheetValues.GetLength(1))
                        {
                            // reached end of sheet row, but this can be acceptable if it's the first column 
                            // (empty row in sheet due to tablix positioning... tablix boundaies not handled)
                            if (firstColumn)
                            {
                                currentSheetColumn = left;
                                currentSheetRow++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        // end of tablix
                        if (currentSheetRow >= sheetValues.GetLength(0))
                        {
                            break;
                        }

                        // skip empty strings in the sheet
                        if (string.IsNullOrWhiteSpace(sheetValues[currentSheetRow, currentSheetColumn]))
                        {
                            currentSheetColumn++;
                            continue;
                        }
                        else
                        {
                            // non-empty cell found
                            break;
                        }
                    }

                    // compare values if nout out of bound
                    if (currentSheetRow >= sheetValues.GetLength(0)
                        || currentSheetColumn >= sheetValues.GetLength(1))
                    {
                        // out of bound
                        return false;
                    }

                    var match = CompareReportValues(sheetValues[currentSheetRow, currentSheetColumn], tablixValues[tablixRow, tablixColumn].Value);

                    if (match)
                    {
                        // from 0-based to 1-based, rendered sheet name will be assigned in calling method
                        tablixValues[tablixRow, tablixColumn].RenderedRow = currentSheetRow + 1;
                        tablixValues[tablixRow, tablixColumn].RenderedColumn = currentSheetColumn + 1;

                        // end of row handled in parent cycle
                        currentSheetColumn++;
                    }
                    else 
                    {
                        ConfigManager.Log.Important(string.Format("Failed to map tablix from [{0}, {1}] because of mismatch at [{2}, {3}]: XLSX '{4}' != TABLIX '{5}'",
                            left, top, tablixRow, tablixColumn, sheetValues[currentSheetRow, currentSheetColumn], tablixValues[tablixRow, tablixColumn].Value));
                        // TODO: handle tablix boundaries? Continue if empty row && reached another tablix on the right?
                        return false;
                    }

                    firstColumn = false;
                }

                // CR;LF in sheet
                currentSheetColumn = left;
                currentSheetRow++;
            }

            return true;
        }

        private string[,] ConvertToStringArray(System.Array values)
        {

            // create a new string array
            string[,] theArray = new string[values.GetLength(0), values.GetLength(1)];

            // loop through the 2-D System.Array and populate the 1-D String Array
            for (int i = 1; i <= values.GetLength(0); i++)
            {
                for (int j = 1; j <= values.GetLength(1); j++)
                {
                    if (values.GetValue(i, j) == null)
                        theArray[i - 1, j - 1] = null;
                    else
                        theArray[i - 1, j - 1] = (string)values.GetValue(i, j).ToString();
                }
            }

            return theArray;
        }

        private bool CompareReportValues(string xmlValue, string xlsValue)
        {
            xmlValue = xmlValue.Replace('\n', ' ');

            /*
            // treat NaN as an error?
            if (xmlValue == "NaN")
            {
                return true;
            }
            */

            if (xmlValue == null && xlsValue == null)
            {
                return true;
            }

            if (xmlValue == null || xlsValue == null)
            {
                return false;
            }

            if (xmlValue == xlsValue)
            {
                return true;
            }

            xmlValue = xmlValue.Trim();
            xlsValue = xlsValue.Trim();

            if (xmlValue == xlsValue)
            {
                return true;
            }

            DateTime xmlDate = DateTime.MinValue;
            DateTime xlsDate = DateTime.MinValue;
            if (xmlValue.Length * xlsValue.Length == 0 && xmlValue.Length + xlsValue.Length > 0)
            {
                return false;
            }

            if (DateTime.TryParse(xmlValue, out xmlDate))
            {
                DateTime.TryParse(xlsValue, out xlsDate);
                if (xmlDate != xlsDate)
                {
                    ConfigManager.Log.Warning(string.Format("DateTime mismatch: XML: {0} XLS: {1}", xmlDate, xlsDate));
                }
                return xmlDate == xlsDate;
            }

            if (char.IsNumber(xmlValue[0]))
            {
                //xmlValue = xmlValue.Replace(" ", "").Replace(",", "").Replace(".", "");
                //xlsValue = xlsValue.Replace(" ", "").Replace(",", "").Replace(".", "");
                var minLen = Math.Min(xmlValue.Length, xlsValue.Length);
                xmlValue = xmlValue.Substring(0, minLen);
                xlsValue = xlsValue.Substring(0, minLen);
                return xmlValue == xlsValue;
            }
            else
            {
                return false;
            }
        }
    }
}
