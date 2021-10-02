using Excel = Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Runtime.InteropServices;
using System.Linq;

namespace ExcelTools
{
    public static class ExcelTools
    {
        public static DataTable ReadSheet(string filePath, string sheetName, bool headingsInFirstRow = true, int rowsToSkip = 0, int columnsToSkip = 0)
        {
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(filePath);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[sheetName];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            object[,] allValues = xlRange.Value2;
            var origRows = allValues.GetLength(0);
            var origCols = allValues.GetLength(1);

            object[,] croppedValues = new object[origRows - rowsToSkip, origCols - columnsToSkip];
            for (int i = rowsToSkip; i < origRows; i++)
            {
                for (int j = columnsToSkip; j < origCols; j++)
                {
                    croppedValues[i - rowsToSkip, j - columnsToSkip] = allValues[i + 1, j + 1];
                }
            }
      
            int rowIdx = 0;
            var columnCount = origCols - columnsToSkip;
            DataTable resTable = new DataTable();
            HashSet<string> usedNames = new HashSet<string>();
            for (int i = 0; i < columnCount; i++)
            {
                if (!headingsInFirstRow)
                {
                    resTable.Columns.Add();
                }
                else
                {
                    var columnName = (croppedValues[rowIdx, i] == null) ? string.Empty : croppedValues[rowIdx, i].ToString();
                    var replacementColumnName = columnName;
                    int duplicateColumnCounter = 1;
                    while (usedNames.Contains(replacementColumnName))
                    {
                        duplicateColumnCounter++;
                        replacementColumnName = columnName + "_" + duplicateColumnCounter.ToString();
                    }
                    usedNames.Add(replacementColumnName);
                    resTable.Columns.Add(replacementColumnName);
                }
            }

            if (headingsInFirstRow)
            {
                rowIdx++;
            }

            for (int i = rowIdx; i < origRows - rowsToSkip; i++)
            {
                var nr = resTable.NewRow();
                for (int j = 0; j < columnCount; j++)
                {
                    nr[j] = (croppedValues[i, j] == null) ? null : croppedValues[i, j].ToString();
                }
                resTable.Rows.Add(nr);
            }

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            return resTable;
            
        }
        

    }
}
