using Xls = Microsoft.Office.Interop.Excel;
using NRWH_Tools_Addin.ApplicationState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRWH_Tools_Addin.ExcelManager
{
    static class SheetDetector
    {
        public static SheetState Detect(Xls.Worksheet sheet)
        {
            Xls.Range topCorner = sheet.Cells[1, 1];
            var topCornerValue = (string)topCorner.Value2;
            SheetState state = null;
            if (topCornerValue == "NewFormat")
            {
                state = ReadNewFormatColumns(sheet);
            }
            else
            {
                state = ReadOldFormatColumns(sheet);
            }
            state.SheetName = sheet.Name;
            return state;
/*
            Popis atributů/ Description of attributes                    Popis atributů VW Status      Přiřazení atributů              Description of attributes VW/ EN         GDPR classification             Attributes assignment/ EN                Status Systémová analýza CD

            ID Atributu Návrh názvu atributu CD Popis / Význam   Návrh názvu atributu Anglický CD Výskyt v reportu ID ISSUES  Název atributu VWFS Popis/ Význam   Obchodní pravidlo   Proces(nepovinný)  Další názvy/ Aliasy(Měrná) jednotka Příklad Status procesu schvalování Životní cyklus atributu Entita(plní BI)    Předmětná oblast (plní BI)	Vlastník Business Datový Stevard Atribut name    Description Business rule Kategorie OÚ Množina OÚ Možnost smazání Poznámka    Entity Entity area Owner   Business data steward Aproval process status  Atribut life cycle Zdrojový systém Zdrojová tabulka Zdrojový sloupec Datový typ Fyzický / Kalkulovaný    Pravidlo / výpočet  Ostatní systémy Poznámka VWFS   Poznámka CD Priorita pro CD Návrh na zařazení k entitě  Atribut dim / Ukazatel / Klíč  Jméno dimenze / Skupina Ukazatelů Technický název TBL (SQL)Technický název CLMN(SQL)  Datový Typ  Poznámka    Stav CD Název Skupiny Faktů Kdo dodal atribut(CD / VW)   Datum poslední změny
            */
        }

        private static Dictionary<Tuple<string, string>, Tuple<string, string>> _oldFormatToEaMapping =
            new Dictionary<Tuple<string, string>, Tuple<string, string>>()
        {
            { new Tuple<string, string>("", "ID Atributu"), new Tuple<string, string>("Basic", "ID")  },
            { new Tuple<string, string>("Popis atributů/Description of attributes", "Návrh názvu atributu CD"), new Tuple<string, string>("Basic", "Name CZ")  },
            { new Tuple<string, string>("Popis atributů/Description of attributes", "Popis/ Význam"), new Tuple<string, string>("Basic", "Description CZ")  },
            { new Tuple<string, string>("Popis atributů/Description of attributes", "Návrh názvu atributu Anglický CD"), new Tuple<string, string>("Basic", "Name EN")  },
            { new Tuple<string, string>("", "ISSUES"), new Tuple<string, string>("Notes", "Issues")  },

        };

        private static ClassListSheetState ReadOldFormatColumns(Xls.Worksheet sheet)
        {
            ClassListSheetState state = new ClassListSheetState();
            state.EaPackagePath = "Sandbox/Radovan Jankovic/XLS_Imports/Attribute_List";
            int topRowPosition = 2;

            state.FirstRowOffset = 3;
            string columnGroup = string.Empty;
            string columnName = null;

            int topCellPosition = 0;
            int topCellSpan = 1;

            for (int i = 1; i <= sheet.Columns.Count; i++)
            {
                if (topCellPosition + topCellSpan <= i)
                {
                    Xls.Range topRng = sheet.Cells[topRowPosition, i];
                    topCellSpan = topRng.MergeArea.Columns.Count;
                    topCellPosition = i;
                    columnGroup = (string)topRng.Value;
                    if (columnGroup == null)
                    {
                        columnGroup = string.Empty;
                    }
                    columnGroup = columnGroup.Trim();
                }

                Xls.Range bottomRng = sheet.Cells[topRowPosition + 1, i];
                columnName = (string)bottomRng.Value;
                if (columnName == null)
                {
                    columnName = string.Empty;
                }
                columnName = columnName.Trim();

                Tuple<string, string> headerTuple = new Tuple<string, string>(columnGroup, columnName);
                if (_oldFormatToEaMapping.ContainsKey(headerTuple))
                {
                    var eaTuple = _oldFormatToEaMapping[headerTuple];
                    if (eaTuple.Item2 == "ID")
                    {
                        state.IdColumnIndex = i;
                    }
                    state.ColumnMappings.Add(new ColumnMapping()
                    {
                        ColumnIndex = i,
                        EaAttributeGroup = eaTuple.Item1,
                        EaAttribute = eaTuple.Item2
                    });
                }
            }
            return state;
        }

        private static ClassListSheetState ReadNewFormatColumns(Xls.Worksheet sheet)
        {
            throw new NotImplementedException();
        }
    }
}
