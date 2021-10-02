using EA_DB_Tools;
using NRWH_Tools_Addin.ApplicationState;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRWH_Tools_Addin.EaManager
{
    class EaOperations
    {
        private string _connectionString;
        private string _repoName;
        private Repository _repo;
        private Lookup _lookup;
        private ElementManager _elementManager;
        private PackageManager _packageManager;
        private DiagramManager _diagramManager;

        private const string META_MODIFIED_DATE_ATTR = "_meta_modified_date";
        private const string META_MODIFIED_BY_ATTR = "_meta_modified_by";
        private const string META_DELETED_ATTR = "_meta_deleted";
        public const string META_ID_ATTR = "ID";
        HashSet<int> _deletedItemsInEa = new HashSet<int>();

        public EaOperations(string connectionString, string repoName)
        {
            _connectionString = connectionString;
            _repoName = repoName;
            _repo = new Repository(_connectionString, _repoName);
            _lookup = new Lookup(_repo);
            _elementManager = new ElementManager(_repo);
            _packageManager = new PackageManager(_repo);
            _diagramManager = new DiagramManager(_repo);
        }

        private List<EditConflict> FindConflicts(ClassListSheetState sheet)
        {
            return new List<EditConflict>();
        }

        /// <summary>
        /// Reflects changes made in EA into the sheet state.
        /// </summary>
        /// <param name="sheetState"></param>
        private void UpdateListState(ClassListSheetState sheetState, List<ClassWithAttributes> eaData, out List<EditConflict> conflicts)
        {
            conflicts = new List<EditConflict>();

            HashSet<int> eaIdSet = new HashSet<int>();

            //handle new / modified from EA
            foreach (var eaItem in eaData)
            {
                eaIdSet.Add(eaItem.ClassId);

                // ! each row created in XLS must be added to the state immediately, 
                // otherwise an update from EA can overwrite the XLS byt its own new rows
                int nextRowOffset = sheetState.FirstRowOffset + sheetState.RowStates.Count;
                // new in EA
                if (sheetState.GetRowById(eaItem.BusinessId) == null)
                {
                    var newRowState = EaItemToRowState(eaItem, sheetState);
                    newRowState.RowOffset = nextRowOffset++;
                    sheetState.RowStates.Add(newRowState);
                }
                else
                {
                    var sheetItem = sheetState.GetRowById(eaItem.BusinessId);
                    var rowValues = ClassAttrsToRowValues(eaItem, sheetState);
                    // modified in EA
                    if (sheetItem.ModifiedDate < eaItem.ModifiedDate)
                    {
                        // EA has a newer version, but XLS has a local modification - declare conflict
                        if (sheetItem.ChangedInXls)
                        {
                            var eaConflictRow = EaItemToRowState(eaItem, sheetState);
                            conflicts.Add(new EditConflict { EaRow = eaConflictRow, LocalRow = sheetItem });
                            // do not update, conflict resolver will do that
                            continue;
                        }
                        sheetItem.ChangedInEa = true;
                        sheetItem.ModifiedBy = eaItem.ModifiedBy;
                        sheetItem.ModifiedDate = eaItem.ModifiedDate;
                        sheetItem.Values = rowValues;
                    }
                }
            }

            // handle deletes
            for (int i = 0; i < sheetState.RowStates.Count; i++)
            {
                var sheetItem = sheetState.RowStates[i];
                // ! when Excel finds out, it has to decrease RowOffsets and delete the item from the list
                if (!eaIdSet.Contains(sheetItem.Id) && _deletedItemsInEa.Contains(sheetItem.Id))
                {
                    // deleted in EA but changed in XLS - conflict
                    if (sheetItem.ChangedInXls)
                    {
                        conflicts.Add(new EditConflict { EaRow = new RowState { Deleted = true, ChangedInEa = true }, LocalRow = sheetItem });
                        // do not update, conflict resolver will do that
                        continue;
                    }
                    sheetItem.ChangedInEa = true;
                    sheetItem.Deleted = true;
                }
            }
        }

        private RowState EaItemToRowState(ClassWithAttributes eaItem, ClassListSheetState sheetState)
        {
            var values = ClassAttrsToRowValues(eaItem, sheetState);
            var res = new RowState()
            {
                ChangedInEa = true,
                Id = eaItem.BusinessId,
                ModifiedBy = eaItem.ModifiedBy,
                ModifiedDate = eaItem.ModifiedDate,
                Values = values
            };
            return res;
        }

        private List<string> ClassAttrsToRowValues(ClassWithAttributes clAtt, ClassListSheetState sheetState)
        {
            var colCount = sheetState.ColumnMappings.Max(x => x.ColumnIndex);
            List<string> res = new List<string>(colCount);
            for (int i = 0; i < colCount; i++)
            {
                res[i] = null;
            }
            foreach (var colMap in sheetState.ColumnMappings)
            {
                var eaTuple = new Tuple<string, string>(colMap.EaAttributeGroup, colMap.EaAttribute);
                if (clAtt.Attributes.ContainsKey(eaTuple))
                {
                    res[colMap.ColumnIndex] = clAtt.Attributes[eaTuple];
                }
            }
            return res;
        }

        /// <summary>
        /// Reflects changes made in XLS into EA
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="originalEaData"></param>
        /// <param name="conflicts"></param>
        private void UpdateEaList(ClassListSheetState sheet, List<ClassWithAttributes> originalEaData, out List<EditConflict> conflicts)
        {
            HashSet<int> eaItemsHashSet = new HashSet<int>();
            conflicts = new List<EditConflict>();
            Dictionary<int, ClassWithAttributes> eaItemDictionary = originalEaData.ToDictionary(x => x.BusinessId, x => x);
            foreach (var eaOrigItem in originalEaData)
            {
                eaItemsHashSet.Add(eaOrigItem.BusinessId);
            }
            List<RowState> rowsToDeleteFromState = new List<RowState>();
            for (int i = 0; i < sheet.RowStates.Count; i++)
            {
                var xlsItem = sheet.RowStates[i];
                if (!xlsItem.ChangedInXls)
                {
                    continue;
                }
                var eaName = "Element " + xlsItem.Id.ToString();
                // new in XLS
                if (!eaItemsHashSet.Contains(xlsItem.Id) && !xlsItem.Deleted)
                {
                    var packageElement = _lookup.GetPackage(sheet.EaPackagePath);

                    var newClass = _elementManager.CreateClass(packageElement, eaName, new List<ElementManager.AttributeSpecifiaction>()
                        {
                            new ElementManager.AttributeSpecifiaction() { Name = META_MODIFIED_BY_ATTR, Value = xlsItem.ModifiedBy },
                            new ElementManager.AttributeSpecifiaction() { Name = META_MODIFIED_DATE_ATTR, Value = xlsItem.ModifiedDate.ToString() },
                            new ElementManager.AttributeSpecifiaction() { Name = META_DELETED_ATTR, Value = "0" },
                        });

                    foreach (var category in sheet.ColumnMappings.Select(x => x.EaAttributeGroup).Distinct())
                    {
                        _elementManager.CreateClass(newClass, category, GetAttrSpecifications(xlsItem, sheet, category));
                    }

                    // TODO ! create composite diagram
                }
                else
                {
                    var origItem = eaItemDictionary[xlsItem.Id];
                    // modified in EA - conflict
                    if (xlsItem.ModifiedDate < origItem.ModifiedDate)
                    {
                        conflicts.Add(new EditConflict() { LocalRow = xlsItem, EaRow = EaItemToRowState(origItem, sheet) });
                        continue;
                    }

                    var origItemElement = _lookup.GetElementById(origItem.ClassId);
                    // deleted in XLS
                    if (xlsItem.Deleted)
                    {
                        // evade deletes by marking the element as deleted
                        //_elementManager.DeleteElement(origItemElement);
                        _elementManager.UpdateElement(origItemElement, eaName, new List<ElementManager.AttributeSpecifiaction>()
                        {
                            new ElementManager.AttributeSpecifiaction() { Name = META_MODIFIED_BY_ATTR, Value = xlsItem.ModifiedBy },
                            new ElementManager.AttributeSpecifiaction() { Name = META_MODIFIED_DATE_ATTR, Value = xlsItem.ModifiedDate.ToString() },
                            new ElementManager.AttributeSpecifiaction() { Name = META_DELETED_ATTR, Value = "1" },
                        });

                        sheet.RowStates.RemoveAt(i);
                        // move all subsequent rows up
                        for (int j = i; j < sheet.RowStates.Count; j++)
                        {
                            sheet.RowStates[j].RowOffset--;
                        }

                        // the current item was removed
                        i--;
                    }
                    // modified in XLS
                    else
                    {
                        foreach (var category in sheet.ColumnMappings.Select(x => x.EaAttributeGroup).Distinct())
                        {
                            var categoryElement = (EA.Element)origItemElement.Elements.GetByName(category);
                            _elementManager.UpdateElement(categoryElement, categoryElement.Name, GetAttrSpecifications(xlsItem, sheet, category));
                        }
                        _elementManager.UpdateElement(origItemElement, eaName, new List<ElementManager.AttributeSpecifiaction>()
                        {
                            new ElementManager.AttributeSpecifiaction() { Name = META_MODIFIED_BY_ATTR, Value = xlsItem.ModifiedBy },
                            new ElementManager.AttributeSpecifiaction() { Name = META_MODIFIED_DATE_ATTR, Value = xlsItem.ModifiedDate.ToString() },
                            new ElementManager.AttributeSpecifiaction() { Name = META_DELETED_ATTR, Value = "0" },
                        });
                    }

                    // remove the flag after synchronization
                    xlsItem.ChangedInXls = false;
                }
            }
        }

        private List<ElementManager.AttributeSpecifiaction> GetAttrSpecifications(RowState xlsRow, ClassListSheetState sheetState, string eaGroup)
        {
            List<ElementManager.AttributeSpecifiaction> res = new List<ElementManager.AttributeSpecifiaction>();
            foreach (var colMap in sheetState.ColumnMappings)
            {
                // colmapping is indexed from 1
                var value = xlsRow.Values[colMap.ColumnIndex - 1];
                if (colMap.EaAttributeGroup != eaGroup)
                {
                    continue;
                }
                res.Add(new ElementManager.AttributeSpecifiaction()
                {
                    Name = colMap.EaAttribute,
                    Type = ElementManager.AttributeSpecifiaction.AttributeTypeEnum.Char,
                    Value = value
                });
            }
            return res;
        }

        /// <summary>
        /// Returns false if update was not successfull
        /// </summary>
        /// <param name="workbook"></param>
        /// <returns></returns>
        public bool SynchronizeWorkbookState(WorkbookState workbook, out List<EditConflict> conflicts)
        {
            conflicts = new List<EditConflict>();
            foreach (var sheet in workbook.Sheets)
            {
                var listSh = sheet as ClassListSheetState;
                if (listSh == null)
                {
                    continue;
                }

                /**/
                var pkg = _lookup.GetPackage(sheet.EaPackagePath);
                _packageManager.ClearPackage(pkg);
                /**/


                // ! acquire lock on EA package and keep it until conflicts are resolved

                List<ClassWithAttributes> eaData = LoadListWithAttributes(listSh);
                listSh.RebuildRowIndex();
                UpdateListState(listSh, eaData, out conflicts);
                if (conflicts.Any())
                {
                    break;
                }

                
                UpdateEaList(listSh, eaData, out conflicts);
                if (conflicts.Any())
                {
                    break;
                }
            }

            if (conflicts.Any())
            {
                return false;
            }

            return true;
        }

        private struct ClassWithAttributes
        {
            public int ClassId;
            public int BusinessId { get; set; }
            public string ClassName;
            public string ModifiedBy;
            public bool Deleted;
            public DateTimeOffset ModifiedDate;
            /// <summary>
            /// two-level attribute names (category, attrName)
            /// </summary>
            public Dictionary<Tuple<string, string>, string> Attributes;
        }

        private List<ClassWithAttributes> LoadListWithAttributes(ClassListSheetState sheetState)
        {

            var packageId = _lookup.GetPackageId(sheetState.EaPackagePath);

            var resTable = _lookup.ExecuteSelectStatement(@"
SELECT
c.Name ClassName
,c.Object_ID ClassId
,sc.Name AttributeCategory 
,att.Name AttributeName
,att.[Default] AttributeValue
FROM t_object c 
INNER JOIN t_object sc ON sc.ParentID = c.Object_ID AND sc.Object_Type = N'Class'
INNER JOIN t_attribute att ON att.Object_ID = sc.Object_ID
WHERE c.Package_ID = @packageId AND c.Object_Type = N'Class' AND c.ParentID = 0

UNION ALL

SELECT
c.Name ClassName
,c.Object_ID ClassId
,NULL AttributeCategory 
,att.Name AttributeName
,att.[Default] AttributeValue
FROM t_object c 
INNER JOIN t_attribute att ON att.Object_ID = c.Object_ID
WHERE c.Package_ID = @packageId AND c.Object_Type = N'Class' AND c.ParentID = 0
", new Dictionary<string, object>() { { "packageId", packageId } });

            var tableByClassId = resTable.AsEnumerable().GroupBy(x => (int)x["ClassId"], x => x);
            List<ClassWithAttributes> res = new List<ClassWithAttributes>();
            _deletedItemsInEa = new HashSet<int>();

            foreach (var grp in tableByClassId)
            {
                Dictionary<Tuple<string, string>, string> attrDictionary = grp.ToDictionary(
                    x =>
                {
                    var categ = x["AttributeCategory"];
                    string categStd = null;
                    if (categ != DBNull.Value)
                    {
                        categStd = (string)categ;
                    }
                    var name = (string)x["AttributeName"];
                    return new Tuple<string, string>(categStd, name);
                },
                    x => x["AttributeValue"].ToString()
                    );

                var className = (string)grp.First()["ClassName"];
                var classId = (int)grp.First()["ClassId"];

                DateTimeOffset modifiedDate = DateTimeOffset.MinValue;
                var modifiedDateKey = new Tuple<string, string>(null, META_MODIFIED_DATE_ATTR);
                if (attrDictionary.ContainsKey(modifiedDateKey))
                {
                    modifiedDate = DateTimeOffset.Parse(attrDictionary[modifiedDateKey]);
                    attrDictionary.Remove(modifiedDateKey);
                }

                string modifiedBy = null;
                var modifiedByKey = new Tuple<string, string>(null, META_MODIFIED_BY_ATTR);
                if (attrDictionary.ContainsKey(modifiedByKey))
                {
                    modifiedBy = attrDictionary[modifiedByKey];
                    attrDictionary.Remove(modifiedByKey);
                }

                var idAttrKey = new Tuple<string, string>(null, META_ID_ATTR);
                var businessId = int.Parse(attrDictionary[idAttrKey]);
                var deletedKey = new Tuple<string, string>(null, META_DELETED_ATTR);
                bool deleted = Boolean.Parse(attrDictionary[deletedKey]);


                ClassWithAttributes cwa = new ClassWithAttributes()
                {
                    ClassId = classId,
                    ClassName = className,
                    ModifiedBy = modifiedBy,
                    ModifiedDate = modifiedDate,
                    Attributes = attrDictionary,
                    BusinessId = businessId,
                    Deleted = deleted
                };
                if (deleted)
                {
                    _deletedItemsInEa.Add(cwa.BusinessId);
                }
                else
                {
                    res.Add(cwa);
                }
            }

            return res;
        }
    }
}
