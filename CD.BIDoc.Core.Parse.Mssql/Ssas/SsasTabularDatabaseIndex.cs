using System;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Model.Mssql.Ssas;
using System.Text.RegularExpressions;
using CD.DLS.DAL.Configuration;
using CD.DLS.Model.Serialization;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.Model.Mssql;
using CD.DLS.Model;
using CD.DLS.Model.Mssql.Tabular;

namespace CD.DLS.Parse.Mssql.Ssas
{
    /// <summary>
    /// Full ID ~ column; measure ~ column
    /// </summary>
    public enum TabularReferenceType { Table, Column, General }

    public class SsasTabularDatabaseIndex : SsasDatabaseIndex
    {
        private Dictionary<string, SsasModelElement> _referrablesDictionary = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, SsasModelElement> _tempReferrablesDictionary = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, List<SsasModelElement>> _tempReferrablesByTable = new Dictionary<string, List<SsasModelElement>>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, SsasModelElement> _localReferrablesDictionary = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);
        private List<SsasTabularRelationshipElement> _relationships = new List<SsasTabularRelationshipElement>();
        Dictionary<string, List<SsasTabularRelationshipElement>> _relationshipsFromTable = new Dictionary<string, List<SsasTabularRelationshipElement>>();
        Dictionary<string, List<SsasTabularRelationshipElement>> _relationshipsToTable = new Dictionary<string, List<SsasTabularRelationshipElement>>();
        // these don't matter when not resolved
        HashSet<string> _keywords = new HashSet<string>(new string[] { "second", "minute", "hour", "day", "today", "week", "month", "year", "now", "true", "false" });

        private ProjectConfig _projectConfig;
        private GraphManager _graphManager;

        private Dictionary<MssqlModelElement, int> _premappedElements = new Dictionary<MssqlModelElement, int>();
        public Dictionary<MssqlModelElement, int> PremappedElements { get { return _premappedElements; } }

        private SsasTabularDatabaseElement _tabularDb = null;

        private void AddMappedElement(MssqlModelElement e)
        {
            if (!_premappedElements.ContainsKey(e))
            {
                _premappedElements.Add(e, e.Id);
            }
        }

        public void AddTempMeasure(string tableName, string measureName, SsasModelElement measureElement)
        {
            if (!_tempReferrablesByTable.ContainsKey(tableName))
            {
                _tempReferrablesByTable.Add(tableName, new List<SsasModelElement>());
            }
            _tempReferrablesByTable[tableName].Add(measureElement);

            var measureFullId = string.Format("'{0}'[{1}]", tableName, measureName);
            _tempReferrablesDictionary.Add(measureFullId, measureElement);
        }

        public void ClearTempMeasures()
        {
            _tempReferrablesByTable.Clear();
            _tempReferrablesDictionary.Clear();
        }

        public override Dictionary<MssqlModelElement, int> GetPremappedIds()
        {
            return PremappedElements;
        }

        public SsasModelElement TryResolveIdentifier(string identifier, TabularReferenceType referenceType)
        {
            var translatedIdentifier = identifier;
            if (QueryMode == SsasQueryMode.MDX)
            {
                translatedIdentifier = TranslateMdxIdentifier(identifier);
                ConfigManager.Log.Info(string.Format("DAX resolver translated MDX {0} to DAX {1}", identifier, translatedIdentifier));
            }

            var normalizedIdentifier = NormalizeIdentifier(translatedIdentifier, referenceType);

            if (_localReferrablesDictionary.ContainsKey(normalizedIdentifier))
            {
                return _localReferrablesDictionary[normalizedIdentifier];
            }

            if (_tempReferrablesDictionary.ContainsKey(normalizedIdentifier))
            {
                return _tempReferrablesDictionary[normalizedIdentifier];
            }

            if (_referrablesDictionary.ContainsKey(normalizedIdentifier))
            {
                return _referrablesDictionary[normalizedIdentifier];
            }

            // 1- part identifier; assumed column, but column not found
            if (normalizedIdentifier.StartsWith("["))
            {
                var normalizedTableIdentifier = "'" + normalizedIdentifier.Substring(1, normalizedIdentifier.Length - 2) + "'";

                if (_localReferrablesDictionary.ContainsKey(normalizedTableIdentifier))
                {
                    return _localReferrablesDictionary[normalizedTableIdentifier];
                }

                if (_tempReferrablesDictionary.ContainsKey(normalizedTableIdentifier))
                {
                    return _tempReferrablesDictionary[normalizedTableIdentifier];
                }

                if (_referrablesDictionary.ContainsKey(normalizedTableIdentifier))
                {
                    return _referrablesDictionary[normalizedTableIdentifier];
                }
            }

            if(!_keywords.Contains(identifier.ToLower()))
            {
                ConfigManager.Log.Warning(string.Format("DAX resolver could not resolve {0}", normalizedIdentifier));
            }

            return null;
        }

        public SsasTabularTableColumnElement FindColumn(string table, string column)
        { 
            var id = $"\'{table}\'[{column}]";
            return (SsasTabularTableColumnElement)TryResolveIdentifier(id, TabularReferenceType.Column);
        }
        
        public override void ClearLocalIndexes()
        {
            _localReferrablesDictionary.Clear();
        }

        public void SetContextTable(string tableName)
        {
            ClearLocalIndexes();

            var table = _tabularDb.Tables.FirstOrDefault(x => x.Caption == tableName);
            if (table == null)
            {
                return;
            }

            foreach (var measure in table.Measures)
            {
                AddLocalMeasure(table.Caption, measure.Caption, measure);
            }

            foreach (var column in table.Columns)
            {
                AddLocalColumn(column.Caption, column);
            }

            if (_tempReferrablesByTable.ContainsKey(tableName))
            {
                foreach (var tempMeasure in _tempReferrablesByTable[tableName])
                {
                    AddLocalMeasure(tableName, tempMeasure.Caption, tempMeasure);
                }
            }
        }

        public SsasTabularDatabaseIndex()
        {
            QueryMode = SsasQueryMode.DAX;
            _ssasType = SsasTypeEnum.Tabular;

            _referrablesDictionary = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);
            _localReferrablesDictionary = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);
        }

        public SsasTabularDatabaseIndex(SsasTabularDatabaseElement databaseElement)
        {
            QueryMode = SsasQueryMode.DAX;
            _ssasType = SsasTypeEnum.Tabular;

            _referrablesDictionary = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);
            _localReferrablesDictionary = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);
            
            SetDatabase(databaseElement);
        }

        public SsasTabularDatabaseIndex(SsasTabularDatabaseElement databaseElement, GraphManager graphManager, ProjectConfig projectConfig)
        {
            QueryMode = SsasQueryMode.DAX;
            _ssasType = SsasTypeEnum.Tabular;
            
            _referrablesDictionary = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);
            _localReferrablesDictionary = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);

            _graphManager = graphManager;
            _projectConfig = projectConfig;

            SerializationHelper sh = new SerializationHelper(_projectConfig, _graphManager);
            var dbWithContents = (SsasTabularDatabaseElement)sh.LoadElementModel(databaseElement.RefPath.Path);


            SetDatabase(dbWithContents);
        }

        private void SetDatabase(SsasTabularDatabaseElement databaseElement)
        {
            _tabularDb = databaseElement;

            _relationships = databaseElement.Relationships.ToList();
            _relationshipsFromTable = _relationships.GroupBy(x => x.FromColumn.Table).ToDictionary(x => x.Key.Caption, x => x.ToList());
            _relationshipsToTable = _relationships.GroupBy(x => x.ToColumn.Table).ToDictionary(x => x.Key.Caption, x => x.ToList());

            foreach (var table in databaseElement.Tables)
            {
                var tableId = string.Format("'{0}'", table.Caption);
                
                _referrablesDictionary.Add(tableId, table);
                AddMappedElement(table);

                foreach (var column in table.Columns)
                {
                    var columnId = string.Format("{0}[{1}]", tableId, column.Caption);

                    _referrablesDictionary.Add(columnId, column);
                    AddMappedElement(column);
                }

                foreach (var measure in table.Measures)
                {
                    var measureFullId = string.Format("{0}[{1}]", tableId, measure.Caption);
                    var measureShortId = string.Format("[{0}]", measure.Caption);

                    _referrablesDictionary.Add(measureFullId, measure);
                    AddMappedElement(measure);

                    if (!_referrablesDictionary.ContainsKey(measureShortId))
                    {
                        _referrablesDictionary.Add(measureShortId, measure);
                    }
                }
            }
        }

        public void AddLocalVariable(string variableName, SsasModelElement expressionElement)
        {
            if (!_localReferrablesDictionary.ContainsKey(variableName))
            {
                _localReferrablesDictionary.Add(variableName, expressionElement);
            }
            var variableNameNormalized = string.Format("[{0}]", variableName);
            if (!_localReferrablesDictionary.ContainsKey(variableNameNormalized))
            {
                _localReferrablesDictionary.Add(variableNameNormalized, expressionElement);
            }


            //AddMappedElement(expressionElement);
        }

        public void AddLocalMeasure(string tableName, string measureName, SsasModelElement measureExpressionElement)
        {
            var measureFullId = string.Format("'{0}'[{1}]", tableName, measureName);
            var measureShortId = string.Format("[{0}]", measureName);

            _localReferrablesDictionary.Add(measureFullId, measureExpressionElement);
            //AddMappedElement(measureExpressionElement);

            if (!_localReferrablesDictionary.ContainsKey(measureShortId))
            {
                _localReferrablesDictionary.Add(measureShortId, measureExpressionElement);
            }
        }

        public void AddLocalColumn(string columnName, SsasModelElement columnElement)
        {
            var columnId = string.Format("[{0}]", columnName);
            if (!_localReferrablesDictionary.ContainsKey(columnId))
            {
                _localReferrablesDictionary.Add(columnId, columnElement);
                //AddMappedElement(columnElement);
            }
        }

        public void AddLocalMeasure(string fullName, SsasModelElement measureExpressionElement)
        {
            if (!_localReferrablesDictionary.ContainsKey(fullName))
            {
                _localReferrablesDictionary.Add(fullName, measureExpressionElement);
            }

            if (fullName.StartsWith("'"))
            {
                var columnNameStart = fullName.IndexOf("[");
                var columnName = fullName.Substring(columnNameStart);

                if (!_localReferrablesDictionary.ContainsKey(columnName))
                {
                    _localReferrablesDictionary.Add(columnName, measureExpressionElement);
                }
            }

            //AddMappedElement(measureExpressionElement);
        }

        public static string TranslateMdxIdentifier(string mdxId)
        {
            var mdxIdLower = mdxId.ToLower();

            // measure
            if (mdxIdLower.StartsWith("measures") || mdxIdLower.StartsWith("[measures]"))
            {
                var measureWrapped = mdxIdLower.Replace("measures.", "").Replace("[measures].", "");
                var measure = measureWrapped.TrimStart('[').TrimEnd(']');
                return "[" + measure + "]";
            }

            var splitParts = mdxId.Split(new string[] { "].[" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.TrimStart('[').TrimEnd(']')).ToList();
            if (splitParts.Count == 1)
            {
                splitParts = mdxId.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.TrimStart('[').TrimEnd(']')).ToList();
            }

            // something unsplittable
            if (splitParts.Count <= 1)
            {
                return mdxId;
            }

            // full ID
            return string.Format("'{0}'[{1}]", splitParts[0], splitParts[1]);
        }

        private string NormalizeIdentifier(string input, TabularReferenceType referenceType)
        {
            var trimmed = input.Trim();
            if (referenceType == TabularReferenceType.Column)
            {
                // 'table'~~~
                if (trimmed.StartsWith("'"))
                {
                    // 'tab'[col]
                    if (trimmed.EndsWith("]"))
                    {
                        return trimmed;
                    }
                    // 'tab'col
                    else
                    {
                        var tabIdEnd = trimmed.LastIndexOf('\'');
                        if (tabIdEnd == -1)
                        {
                            ConfigManager.Log.Important(string.Format("Unrecognized tabular column ID format {0}", trimmed));
                            return trimmed;
                        }

                        if (tabIdEnd == trimmed.Length - 1)
                        {
                            // just the table??
                            return trimmed;
                        }

                        var columnEnclosed = trimmed.Insert(tabIdEnd + 1, "[") + "]";
                        return columnEnclosed;
                    }
                }
                // [col]
                else if (trimmed.StartsWith("["))
                {
                    return trimmed;
                }
                // table[column]
                else if (trimmed.EndsWith("]"))
                {
                    var columnIdStart = trimmed.IndexOf('[');
                    if (columnIdStart == -1)
                    {
                        ConfigManager.Log.Important(string.Format("Unrecognized tabular column ID format {0}", trimmed));
                        return trimmed;
                    }

                    var tableEnclosed = "'" + trimmed.Insert(columnIdStart, "'");
                    return tableEnclosed;
                }
                else if (trimmed.Contains("."))
                {
                    var split = trimmed.Split('.');
                    if (split.Length < 2)
                    {
                        throw new Exception(string.Format("Could not normalize tabular identifier {0}", input));
                    }
                    var formatted = $"'{split[0]}'[{split[1]}]";
                    return formatted;
                }
                else
                {
                    throw new Exception(string.Format("Could not normalize tabular identifier {0}", input));
                }
            }
            else if (referenceType == TabularReferenceType.Table)
            {
                // 'tab'
                if (trimmed.StartsWith("'"))
                {
                    return trimmed;
                }
                else if (trimmed.StartsWith("["))
                {
                    ConfigManager.Log.Important(string.Format("Unrecognized tabular table ID format {0}", trimmed));
                    return trimmed;
                }
                else
                {
                    return "'" + trimmed + "'";
                }
            }
            else if (referenceType == TabularReferenceType.General)
            {
                // [col]
                if (trimmed.StartsWith("["))
                {
                    return trimmed;
                }
                // 'tab'~~~
                else if (trimmed.StartsWith("'"))
                {
                    if (trimmed.EndsWith("'") || trimmed.EndsWith("]"))
                    {
                        return trimmed;
                    }

                    // 'tab'col
                    var tableIdEnd = trimmed.IndexOf('\'', 1);
                    if (tableIdEnd == -1)
                    {
                        ConfigManager.Log.Important(string.Format("Unrecognized tabular ID format {0}", trimmed));
                        return trimmed;
                    }

                    var columnEncolsed = trimmed.Insert(tableIdEnd + 1, "[") + "]";
                    return columnEncolsed;
                }
                // measure, maybe?
                else
                {
                    var measureWrap = "[" + trimmed + "]";
                    return measureWrap;
                }
            }
            else
            {
                throw new Exception(string.Format("Unsupported tabular reference type of {0}: {1}", input, referenceType));
            }
        }
        
    }
    

}
