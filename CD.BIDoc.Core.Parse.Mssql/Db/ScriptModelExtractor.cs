using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;
using CD.DLS.Model.Mssql.Db;
using CD.DLS.Model.Mssql;
using CD.DLS.DAL.Configuration;
using CD.DLS.Model.Interfaces;

namespace CD.DLS.Parse.Mssql.Db
{
    /// <summary>
    /// Creates models from SQL scripts.
    /// </summary>
    public class ScriptModelParser : ISqlScriptModelParser
    {
        private readonly TSqlParser _parser;
        private string _constextServerName;


        public TSqlParser Parser { get { return _parser; } }

        public string ContextServerName
        {
            get
            {
                return _constextServerName;
            }

            set
            {
                _constextServerName = value;
            }
        }

        /// <summary>
        /// Creates a extractor using a specified SQL parser.
        /// </summary>
        /// <param name="parser">Parser used to parse the provided scripts.</param>
        public ScriptModelParser()
        {
            _parser = new TSql140Parser(false); // sqlParser;
        }


        public SqlScriptElement ParseAndResolveOverTableSource(string expression, TableSourceColumnList tableSource, MssqlModelElement parent)
        {
            var expressionAsSelect = "SELECT " + expression + " FROM " + ((Identifier)tableSource.Identifier).Value;
            var origContextServerName = _constextServerName;
            this._constextServerName = "temp";

            ReferrableIndexBuilder srib = new ReferrableIndexBuilder();
            var environment = srib.CreateExplicitDatabaseIndex(new List<TableSourceColumnList>() { tableSource });
            var emptyDbId = new Identifier() { Value = string.Empty };
            
            var res = ExtractScriptModel(expressionAsSelect, parent, environment, emptyDbId);
            _constextServerName = origContextServerName;
            return res;
        }


        /// <summary>
        /// Creates a model for the provided script.
        /// </summary>
        public SqlScriptElement ExtractScriptModel(string script, MssqlModelElement parent, IReferrableIndex environment, Identifier dbIdentifier, 
            out Dictionary<string, MssqlModelElement> resultColumns, out List<Tuple<string, MssqlModelElement>> resultColumnsOrdinal, out Dictionary<string, TableSourceColumnList> createdTempTables)
        {
            IList<ParseError> errors = new List<ParseError>();
            TSqlScript parsed = null;
            using (TextReader sr = new StringReader(script))
            {
                TSqlFragment midParsed = null;
                try
                {
                    midParsed = _parser.Parse(sr, out errors);
                }
                catch
                {
                }
                parsed = midParsed as TSqlScript;
                //statementBatch =  as TSqlBatch;
            }
            if (errors.Any())
            {

                //throw new Exception(string.Join("\n", errors.Select(e => String.Format("{0}: {1},{2}", e.Message, e.Line, e.Column))));
                ConfigManager.Log.Error("Parse error in {0}: {1}", parent.RefPath.Path, string.Join(";", errors.Select(x => string.Format("[{0}:{1}] - {2}", x.Line, x.Column, x.Message))));

                string refpathstring = parent == null ? String.Empty : (parent.RefPath.Path + "/STATEMENT_PARSE_ ERROR");
                var refpath = new RefPath(refpathstring);
                var sn = new SqlScriptElement(/*SqlFragmentElement(*/refpath, "STATEMENT_PARSE_ERROR");
                sn.Definition = script;
                resultColumns = new Dictionary<string, MssqlModelElement>();
                createdTempTables = new Dictionary<string, TableSourceColumnList>();
                resultColumnsOrdinal = new List<Tuple<string, MssqlModelElement>>();
                return sn;
            }
            
            //var statement = parsed.Batches[0].Statements[0];// as SelectStatement;
            //environment.ContextServerName = _constextServerName;
            var res = ExtractScriptModel(parsed, parent, environment, dbIdentifier, out resultColumns, out resultColumnsOrdinal, out createdTempTables);
            return res;
        }

        public SqlScriptElement ExtractScriptModel(string script, MssqlModelElement parent, IReferrableIndex environment, Identifier dbIdentifier,
            out Dictionary<string, MssqlModelElement> resultColumns)
        {
            Dictionary<string, TableSourceColumnList> createdTempTables;
            List<Tuple<string, MssqlModelElement>> resultColumnsOrdinal;
            return ExtractScriptModel(script, parent, environment, dbIdentifier, out resultColumns, out resultColumnsOrdinal, out createdTempTables);
        }

        public SqlScriptElement ExtractScriptModel(string script, MssqlModelElement parent, IReferrableIndex environment, Identifier dbIdentifier,
            out Dictionary<string, MssqlModelElement> resultColumns, out Dictionary<string, TableSourceColumnList> createdTempTables)
        {
            List<Tuple<string, MssqlModelElement>> resultColumnsOrdinal;
            return ExtractScriptModel(script, parent, environment, dbIdentifier, out resultColumns, out resultColumnsOrdinal, out createdTempTables);
        }

        public SqlScriptElement ExtractScriptModel(string script, MssqlModelElement parent, IReferrableIndex environment, Identifier dbIdentifier,
            out Dictionary<string, MssqlModelElement> resultColumns, out List<Tuple<string, MssqlModelElement>> resultColumnsOrdinal)
        {
            Dictionary<string, TableSourceColumnList> createdTempTables;
            return ExtractScriptModel(script, parent, environment, dbIdentifier, out resultColumns, out resultColumnsOrdinal, out createdTempTables);
        }

        public SqlScriptElement ExtractScriptModel(string script, MssqlModelElement parent, IReferrableIndex environment, Identifier dbIdentifier)
        {
            Dictionary<string, MssqlModelElement> resultColumnsDummy;
            return ExtractScriptModel(script, parent, environment, dbIdentifier, out resultColumnsDummy);
        }


        private string MakeScriptRefId(MssqlModelElement parent)
        {
            if (parent != null)
            {
                int ordNum = parent.Children.Where(child => child is SqlScriptElement).Count() + 1;
                var nodeName = "Script " + ordNum.ToString();
                string refId = parent.GetRefId() + string.Format("/[Script{0}]", ordNum);
                return refId;
            }
            else
            {
                return string.Empty;
            }
        }


        public SqlScriptElement ExtractScriptModel(TSqlFragment script, MssqlModelElement parent, IReferrableIndex environment, Identifier dbIdentifier)
        {
            Dictionary<string, MssqlModelElement> resultColumnsDummy = new Dictionary<string, MssqlModelElement>();
            return ExtractScriptModel(script, parent, environment, dbIdentifier, out resultColumnsDummy);
        }

        public SqlScriptElement ExtractScriptModel(TSqlFragment script, MssqlModelElement parent, IReferrableIndex environment, Identifier dbIdentifier,
            out Dictionary<string, MssqlModelElement> resultColumns)


        {
            Dictionary<string, TableSourceColumnList> createdTempTables;
            List<Tuple<string, MssqlModelElement>> resultColumnsOrdinal;
            return ExtractScriptModel(script, parent, environment, dbIdentifier, out resultColumns, out resultColumnsOrdinal, out createdTempTables);
        }


        public SqlScriptElement ExtractScriptModel(TSqlFragment script, MssqlModelElement parent, IReferrableIndex environment, Identifier dbIdentifier,
            out Dictionary<string, MssqlModelElement> resultColumns, out Dictionary<string, TableSourceColumnList> createdTempTables)


        {
            List<Tuple<string, MssqlModelElement>> resultColumnsOrdinal;
            return ExtractScriptModel(script, parent, environment, dbIdentifier, out resultColumns, out resultColumnsOrdinal, out createdTempTables);
        }


        public SqlScriptElement ExtractScriptModel(TSqlFragment script, MssqlModelElement parent, IReferrableIndex environment, Identifier dbIdentifier, 
            out Dictionary<string, MssqlModelElement> resultColumns, out List<Tuple<string, MssqlModelElement>> resultColumnsOrdinal, 
            out Dictionary<string, TableSourceColumnList> createdTempTables)
        {
            //if (parent.RefPath.Path.Contains("Merge DimEmployee"))
            //{

            //}
            ReferenceResolver resolver = new ReferenceResolver();
            environment.SetContextServer(_constextServerName);
            //environment.ContextServerName = _constextServerName;
            DependencySeekingTSqlFragmentVisitor _visitor = new DependencySeekingTSqlFragmentVisitor(dbIdentifier, environment, resolver);
            FragmentTreeNode fragmentTree = _visitor.CleanupAndVisit(script);
            resolver.TryResolveReferences(_visitor);

            string refId = MakeScriptRefId(parent);

            if (fragmentTree.Fragment != script)
            {
                throw new Exception();
            }
            Dictionary<TSqlFragment, SqlFragmentElement> fragmentsToElements = new Dictionary<TSqlFragment, SqlFragmentElement>();

            int nodeCounter = 0;

            var fragmentNode = BuildModelFromFragmentTree(fragmentTree, fragmentsToElements, parent, true, ref nodeCounter, resolver);
            var scriptNode = (SqlScriptElement)fragmentNode;

            foreach (var reference in resolver.ResolvedReferences.Where(x => x.ScriptReferenceType != ScriptReferenceTypeEnum.InsertTarget
                && x.ScriptReferenceType != ScriptReferenceTypeEnum.AssignmentTarget && x.ScriptReferenceType != ScriptReferenceTypeEnum.NAryOperationOutputColumn))
            {
                var nodeFrom = reference.FromFragment == null ? reference.FromObject : (reference.FromObject != null ? reference.FromObject : (fragmentsToElements.ContainsKey(reference.FromFragment) ?
                    fragmentsToElements[reference.FromFragment] : null));
                var nodeTo = reference.ToFragment == null ? reference.ToObject : (reference.ToObject != null ? reference.ToObject :  (fragmentsToElements.ContainsKey(reference.ToFragment) ?
                    fragmentsToElements[reference.ToFragment] : reference.ToObject)); //environment.GetNode(reference.ToUrn);

                if (nodeFrom == null)
                {
                    ConfigManager.Log.Error(string.Format("Could not resolve reference {0} in {1}, {2}", reference.FromFragment.GetText(), script.GetText(), parent.RefPath.Path));
                }
                else if (nodeTo == null)
                {
                    ConfigManager.Log.Error(string.Format("Could not resolve reference {0} in {1}, {2}", reference.ToFragment.GetText(), script.GetText(), parent.RefPath.Path));
                }
                else
                {
                    //TODO: VD: when multiple references??
                    //if (nodeFrom.Reference != null)
                    //throw new Exception("Multiple references for fragment node");
                    nodeFrom.Reference = (MssqlModelElement)nodeTo;
                    /*var link = new BasicNodeLink() { NodeFrom = nodeFrom, NodeTo = nodeTo };
                    scriptNode.Links.Add(link);*/
                }
            }

            foreach (var reference in resolver.ResolvedReferences.Where(x => x.ScriptReferenceType == ScriptReferenceTypeEnum.NAryOperationOutputColumn))
            {
                var nodeFrom = reference.FromFragment == null ? reference.FromObject : (fragmentsToElements.ContainsKey(reference.FromFragment) ?
                    fragmentsToElements[reference.FromFragment] : reference.FromObject); // null;
                var nodeTo = reference.ToFragment == null ? reference.ToObject : (fragmentsToElements.ContainsKey(reference.ToFragment) ?
                    fragmentsToElements[reference.ToFragment] : reference.ToObject); //environment.GetNode(reference.ToUrn);

                if (reference.FromFragment == reference.ToFragment)
                {
                    // x is a part and output column of the nary operation => output takes priority
                    continue;
                }

                if (nodeFrom == null)
                {
                    ConfigManager.Log.Error(string.Format("Could not resolve reference {0} in {1}, {2}", reference.FromFragment.GetText(), script.GetText(), parent.RefPath.Path));
                }
                else if (nodeTo == null)
                {
                    ConfigManager.Log.Error(string.Format("Could not resolve reference {0} in {1}, {2}", reference.ToFragment.GetText(), script.GetText(), parent.RefPath.Path));
                }
                else
                {
                    //TODO: VD: when multiple references??
                    //if (nodeFrom.Reference != null)
                    //throw new Exception("Multiple references for fragment node");
                    var nodeFromTyped = (SqlNAryOperationOperandColumnElement)nodeFrom;
                    var nodeToTyped = (SqlNAryOperationOutputColumnElement)nodeTo;

                    nodeFromTyped.OperationOutputColumn = nodeToTyped;
                    if (nodeToTyped.OperandSources_DEBUG == null)
                    {
                        nodeToTyped.OperandSources_DEBUG = new List<SqlNAryOperationOperandColumnElement>();
                    }
                    nodeToTyped.OperandSources_DEBUG.Add(nodeFromTyped);

                    /*var link = new BasicNodeLink() { NodeFrom = nodeFrom, NodeTo = nodeTo };
                    scriptNode.Links.Add(link);*/
                }
            }

            /*
                public class SqlNAryOperationOutputColumnElement : SqlElement
    {
        public SqlNAryOperationOutputColumnElement(RefPath refPath, string caption)
            : base(refPath, caption) { }
        
    }

    public class SqlNAryOperationOperandColumnElement : SqlFragmentElement
    {
        public SqlNAryOperationOperandColumnElement(RefPath refPath, string caption)
            : base(refPath, caption) { }

        [ModelLink]
        SqlNAryOperationOutputColumnElement OperationOutputColumn { get; set; }
    }

             */

            int dmlReferenceCounter = 0;
            foreach (var dmlReference in resolver.ResolvedReferences.Where(x => x.ScriptReferenceType == ScriptReferenceTypeEnum.InsertTarget || x.ScriptReferenceType == ScriptReferenceTypeEnum.AssignmentTarget))
            {
                if (dmlReference.ScriptReferenceType == ScriptReferenceTypeEnum.AssignmentTarget)
                {

                }
                MssqlModelElement nodeFrom = null;
                if (dmlReference.FromObject != null)
                {
                    nodeFrom = dmlReference.FromObject;
                }
                else if(dmlReference.FromFragment != null)

                {
                    nodeFrom = fragmentsToElements.ContainsKey(dmlReference.FromFragment) ?
                    fragmentsToElements[dmlReference.FromFragment] : null;
                }
                
                MssqlModelElement nodeTo = dmlReference.ToObject as MssqlModelElement;
                if (nodeTo == null && dmlReference.ToFragment != null)
                {
                    nodeTo = fragmentsToElements.ContainsKey(dmlReference.ToFragment) ?
                        fragmentsToElements[dmlReference.ToFragment] : null; /* (SqlFragmentElement)insertReference.ToObject;*///environment.GetNode(reference.ToUrn);
                }

                if (nodeFrom == null || nodeTo == null)
                {
                    if (nodeFrom == null)
                    {
                        try
                        {
                            ConfigManager.Log.Error(string.Format("Could not resolve reference {0} in {1}, {2}", dmlReference.FromFragment.GetText(), script.GetText(), parent.RefPath.Path));
                        }
                        catch
                        {
                            if (dmlReference.FromFragment == null)
                            {
                                ConfigManager.Log.Error("DML reference from fragment is null in " + script.GetText());
                            }
                        }
                    }
                    else if (nodeTo == null)
                    {
                        try
                        {
                            ConfigManager.Log.Error(string.Format("Could not resolve reference {0} in {1}, {2}", dmlReference.ToFragment.GetText(), script.GetText(), parent.RefPath.Path));
                        }
                        catch
                        {
                            if (dmlReference.ToFragment == null)
                            {
                                ConfigManager.Log.Error("DML reference to fragment is null in " + script.GetText());
                            }
                        }
                    }

                    continue;

                    //var fragments = fragmentsToElements.ToArray().Select(x => new Tuple<string, TSqlFragment>(x.Key.ToString(), x.Key)).ToList();

                    //throw new Exception();
                }

                if (nodeFrom.RefPath.Path == "Server[@Name='PC12\\SQL2012']/Database[@Name='ESS']/StoredProcedure[@Name='ADM_510' and @Schema='Report']/[CREATE_0]/[_s_679]/[SELECT_680]/[SELECT_681]/[u_682]")
                {

                }
                if (nodeTo.RefPath.Path == "Server[@Name='PC12\\SQL2012']/Database[@Name='ESS']/StoredProcedure[@Name='ADM_510' and @Schema='Report']/[CREATE_0]/[_s_679]/[SELECT_680]/[SELECT_681]/[u_682]")
                {

                }


                SqlDmlSourceElement sourceTyped = null;

                
                if (nodeFrom is DbModelElement && nodeFrom.Reference == null)
                {
                    sourceTyped = new SqlDmlSourceElement(scriptNode.RefPath.NamedChild("DmlSource", string.Format("No_{0}", dmlReferenceCounter)), nodeFrom.Caption);
                    
                    
                    if (nodeFrom is SqlFragmentElement)
                    {
                        sourceTyped.OffsetFrom = ((SqlFragmentElement)nodeFrom).OffsetFrom;
                        sourceTyped.Length = ((SqlFragmentElement)nodeFrom).Length;
                    }
                    sourceTyped.Definition = nodeFrom.Definition;
                    sourceTyped.Reference = nodeFrom;

                    sourceTyped.Parent = scriptNode;
                    scriptNode.AddChild(sourceTyped);
                }
                else
                {

                    sourceTyped = new SqlDmlSourceElement(nodeFrom.Parent.RefPath.NamedChild("DmlSource", string.Format("No_{0}", dmlReferenceCounter)), nodeFrom.Caption);
                    if (nodeFrom is SqlFragmentElement)
                    {
                        sourceTyped.OffsetFrom = ((SqlFragmentElement)nodeFrom).OffsetFrom;
                        sourceTyped.Length = ((SqlFragmentElement)nodeFrom).Length;
                    }
                    sourceTyped.Definition = nodeFrom.Definition;
                    sourceTyped.Reference = nodeFrom.Reference;
                    if (sourceTyped.Reference == null)
                    {
                        sourceTyped.Reference = nodeFrom;
                    }
                    //foreach (var sourceChild in nodeFrom.Children)
                    //{
                    //    sourceChild.Parent = sourceTyped;
                    //    sourceTyped.AddChild(sourceChild);
                    //}

                    sourceTyped.Parent = nodeFrom.Parent;
                    nodeFrom.Parent.RemoveChild(nodeFrom);
                    nodeFrom.Parent.AddChild(sourceTyped);
                    // x2
                    sourceTyped.AddChild(nodeFrom);
                    nodeFrom.Parent = sourceTyped;
                    nodeFrom.AdjustRefPathUnderNewParentOneLevel(sourceTyped);
                }

                if (nodeTo is SqlFragmentElement)
                {
                    var targetTyped = new SqlDmlTargetReferenceElement(nodeTo.Parent.RefPath.NamedChild("DmlTarget", string.Format("No_{0}", dmlReferenceCounter)), nodeTo.Caption);
                    targetTyped.OffsetFrom = nodeTo is SqlFragmentElement ? ((SqlFragmentElement)nodeTo).OffsetFrom : 0;
                    targetTyped.Length = nodeTo is SqlFragmentElement ? ((SqlFragmentElement)nodeTo).Length : 0;
                    targetTyped.Definition = nodeTo.Definition;
                    targetTyped.Reference = nodeTo.Reference;
                    //foreach (var targetChild in nodeTo.Children)
                    //{
                    //    targetChild.Parent = targetTyped;
                    //    targetTyped.AddChild(targetChild);
                    //}
                    targetTyped.Parent = nodeTo.Parent;
                    nodeTo.Parent.RemoveChild(nodeTo);
                    nodeTo.Parent.AddChild(targetTyped);
                    // x2
                    targetTyped.AddChild(nodeTo);
                    nodeTo.Parent = targetTyped;
                    nodeTo.AdjustRefPathUnderNewParentOneLevel(targetTyped);
                    nodeTo = targetTyped;

                    sourceTyped.TargetReference = targetTyped;
                }
                else
                {
                    sourceTyped.TargetReference = nodeTo;
                }
                // replace original SQL fragments with specialized subtypes
                nodeFrom = sourceTyped;
                
                dmlReferenceCounter++;
            }

            resultColumns = null;
            resultColumnsOrdinal = null;

            if (_visitor.FirstTopLevelTableSource != null || _visitor.FirstProcedureCall != null)
            {
                // sql does not forbid one from duplicating column names
                if (_visitor.FirstProcedureCall == null && _visitor.FirstTopLevelTableSource.Columns.Count == _visitor.FirstTopLevelTableSource.Columns
                    // there can be one column without explicit name and the columns can still be unique
                    .Select(x => ((Identifier)(x.Identifier == null ? new Identifier() { Value = "" } : x.Identifier)).Value).Distinct().Count())
                {
                    resultColumns = new Dictionary<string, MssqlModelElement>(StringComparer.InvariantCultureIgnoreCase);
                    resultColumnsOrdinal = new List<Tuple<string, MssqlModelElement>>();

                    SqlScriptResultElement scriptResultElement = new SqlScriptResultElement(UrnBuilder.GetScriptResultRefPath(scriptNode.RefPath, 0), "Script result");
                    scriptResultElement.Parent = scriptNode;
                    scriptNode.AddChild(scriptResultElement);

                    if (_visitor.FirstTopLevelTableSource.Columns.Select(x => ((Identifier)(x.Identifier == null ? new Identifier() { Value = "" } : x.Identifier)).Value)
                        .Distinct(StringComparer.OrdinalIgnoreCase).Count() == _visitor.FirstTopLevelTableSource.Columns.Count)
                    {
                        int colCount = 0;
                        foreach (var outCol in _visitor.FirstTopLevelTableSource.Columns)
                        {
                            MssqlModelElement columnElement = null;
                            if (outCol.ObjectContent != null && fragmentsToElements.ContainsKey(outCol.ObjectContent))
                            {
                                columnElement = fragmentsToElements[outCol.ObjectContent];
                            }
                            else if (outCol.ModelElement != null)   // view columns in SELECT * FROM view
                            {
                                columnElement = (MssqlModelElement)(outCol.ModelElement);
                            }
                            else
                            {
                                columnElement = fragmentsToElements[_visitor.FirstTopLevelTableSource.ObjectContent];
                            }

                            var identifier = ((Identifier)(outCol.Identifier == null ? new Identifier() { Value = "" } : outCol.Identifier)).Value;
                            resultColumns.Add(identifier, columnElement);
                            resultColumnsOrdinal.Add(new Tuple<string, MssqlModelElement>(identifier, columnElement));

                            SqlScriptResultColumnElement resultColumnElement = new SqlScriptResultColumnElement(UrnBuilder.GetScriptResultColumnRefPath(scriptResultElement.RefPath, colCount++), identifier);
                            resultColumnElement.Parent = scriptResultElement;
                            scriptResultElement.AddChild(resultColumnElement);
                            resultColumnElement.ColumnSource = columnElement;
                            resultColumnElement.Ordinal = colCount;
                        }
                    }
                }
                else if (_visitor.FirstProcedureCall != null)
                {
                    var spNode = (ProcedureElement)(environment.FindStoredProcedure((MultiPartIdentifier)(_visitor.FirstProcedureCall.Identifier), dbIdentifier).ModelElement);
                    //ConfigManager.Log.Info("First procedure call: " + (_visitor.FirstProcedureCall.ModelElement == null ? "" : _visitor.FirstProcedureCall.ModelElement.RefPath.Path) + " " + _visitor.FirstProcedureCall.Identifier.GetText());
                    
                    if (spNode.OutputColumns != null)
                    {
                        //ConfigManager.Log.Info("SP output columns: " + string.Join(", ", spNode.OutputColumns.Select(x => x.Key)));

                        SqlScriptResultElement scriptResultElement = new SqlScriptResultElement(UrnBuilder.GetScriptResultRefPath(scriptNode.RefPath, 0), "Script result");
                        scriptResultElement.Parent = scriptNode;
                        scriptNode.AddChild(scriptResultElement);
                        
                        if (_visitor.FirstProcedureCallResultSet != null)
                        {
                            resultColumns = new Dictionary<string, MssqlModelElement>();
                            resultColumnsOrdinal = new List<Tuple<string, MssqlModelElement>>();
                            var resultSetColumns = _visitor.FirstProcedureCallResultSet.Columns;
                            //ConfigManager.Log.Info("SP result output columns: " + string.Join(", ", resultColumns.Select(x => x.Key)));

                            for (int i = 0; i < resultSetColumns.Count; i++)
                            {
                                var resultSetColumnElement = fragmentsToElements[resultSetColumns[i].ObjectContent];
                                resultSetColumnElement.Reference = spNode.OutputColumnsOrdinal[i].Item2;
                                var idf = resultSetColumns[i].Identifier;
                                var columnIdentifier = ((Identifier)(idf == null ? new Identifier() { Value = "" } : idf)).Value; // = resultSetColumns[i].Identifier.GetText();
                                resultColumnsOrdinal.Add(new Tuple<string, MssqlModelElement>(columnIdentifier, resultSetColumnElement));
                                resultColumns.Add(columnIdentifier, resultSetColumnElement);

                                SqlScriptResultColumnElement resultColumnElement = new SqlScriptResultColumnElement(UrnBuilder.GetScriptResultColumnRefPath(scriptResultElement.RefPath, i), columnIdentifier);
                                resultColumnElement.Parent = scriptResultElement;
                                scriptResultElement.AddChild(resultColumnElement);
                                resultColumnElement.ColumnSource = resultSetColumnElement;
                                resultColumnElement.Ordinal = i;
                            }
                        }
                        else
                        {
                            resultColumns = spNode.OutputColumns;
                            resultColumnsOrdinal = spNode.OutputColumnsOrdinal;
                        }
                    }
                }
            }

            createdTempTables = new Dictionary<string, TableSourceColumnList>();
            foreach (var tempTable in _visitor.TempTablesDefined)
            {
                if (tempTable.ObjectContent == null)
                {
                    try
                    {
                        ConfigManager.Log.Error(string.Format(
                            "Object content of temp table {0} in {1} cannot be null!", tempTable.Identifier.GetText(), parent.RefPath.Path));
                    }
                    catch
                    {
                    }
                    continue;
                }
                tempTable.ModelElement = fragmentsToElements[tempTable.ObjectContent];
                foreach (var column in tempTable.Columns)
                {
                    if (column.ObjectContent == null)
                    {
                        try
                        {
                            ConfigManager.Log.Error(string.Format(
                                "Object content of temp table column {0} in {1} cannot be null!", column.Identifier.GetText(), parent.RefPath.Path));
                        }
                        catch
                        {
                        }
                            continue;

                    }
                    column.ModelElement = fragmentsToElements[column.ObjectContent];
                }

                string identifierValue = null;
                if (tempTable.Identifier is Identifier)
                {
                    identifierValue = ((Identifier)(tempTable.Identifier)).Value;
                }
                else if (tempTable.Identifier is SchemaObjectName)
                {
                    identifierValue = ((SchemaObjectName)(tempTable.Identifier)).BaseIdentifier.Value;
                }
                else
                {
                    identifierValue = tempTable.Identifier.GetText().TrimStart('[').TrimEnd(']');
                }
                createdTempTables.Add(identifierValue, tempTable);
            }


            //TODO: resultColumns
#if vdfalse
            Dictionary<string, MssqlModelElement> resultColumns = null;

            if (_visitor.LastTopLevelTableSource != null || _visitor.LastProcedureCall != null)
            {
                if (_visitor.LastProcedureCall == null)
                {
                    resultColumns = new Dictionary<string, MssqlModelElement>();
                    foreach (var outCol in _visitor.LastTopLevelTableSource.Columns)
                    {
                        resultColumns.Add(((Identifier)outCol.Identifier).Value, fragmentsToNodes[(outCol.ObjectContent) ?? (_visitor.LastTopLevelTableSource.ObjectContent)]);
                    }
                }
                else
                {
                    var spNode = (ProcedureNode)(environment.GetNode(_visitor.LastProcedureCall.Urn));
                    if (spNode.OutputColumns != null)
                    {
                        resultColumns = spNode.OutputColumns;
                    }
                }
            }
#endif
            //selectNode.Nodes.Add(selectNode);
            return scriptNode;

        }

        public static SqlFragmentElement CreateElementForFragment(TSqlFragment fg, MssqlModelElement parent, bool wholeScript, ref int nodeCounter, ReferenceResolver resolver)
        {

            var text = fg.GetText();
            if (wholeScript)
            {
                text = fg.ScriptTokenStream.GetText(0, fg.ScriptTokenStream.Count - 1);
            }
            string refpathstring = parent == null ? String.Empty : (parent.RefPath + String.Format("/[{0}_{1}]", fg.ScriptTokenStream[fg.FirstTokenIndex].Text, nodeCounter));
            var refpath = new RefPath(refpathstring);
            string caption = fg.ScriptTokenStream[fg.FirstTokenIndex].Text;

            SqlFragmentElement sn = null;

            if (fg is Microsoft.SqlServer.TransactSql.ScriptDom.DeclareVariableElement)
            {
                var declVar = new Model.Mssql.Db.DeclareVariableElement(refpath, caption);
                
                sn = declVar;
            }
            else
            {


                var nAryReferenceToFragment = resolver.ResolvedReferences.FirstOrDefault(x =>
                x.ToFragment.FirstTokenIndex == fg.FirstTokenIndex
                && x.ToFragment.LastTokenIndex == fg.LastTokenIndex && x.ScriptReferenceType == ScriptReferenceTypeEnum.NAryOperationOutputColumn);
                if (nAryReferenceToFragment != null)
                {
                    var naryOutputCol = new Model.Mssql.Db.SqlNAryOperationOutputColumnElement(refpath, caption);

                    sn = naryOutputCol;

                }
                else
                {
                    var nAryReferenceFromFragment = resolver.ResolvedReferences.Where(x => x.FromFragment != null).FirstOrDefault(x =>
                    x.FromFragment.FirstTokenIndex == fg.FirstTokenIndex
                    && x.FromFragment.LastTokenIndex == fg.LastTokenIndex && x.ScriptReferenceType == ScriptReferenceTypeEnum.NAryOperationOutputColumn);
                    if (nAryReferenceFromFragment != null)
                    {
                            var operandCol = new Model.Mssql.Db.SqlNAryOperationOperandColumnElement(refpath, caption);

                            sn = operandCol;
                    }
                }
                if (sn == null)
                {
                    // TODO: modified temporarily so that model building tests using scripts pass
                    sn = new SqlScriptElement(/*SqlFragmentElement(*/refpath, caption);
                    //sn.Definition = fg.GetText();
                }
                if (sn.RefPath.Path == "Server[@Name='FSCZPRCT0013']/Database[@Name='MIS_DB_TEST']/StoredProcedure[@Name='rep_brands_mngmt_brand' and @Schema='rs']/[CREATE_0]/[SELECT_125]/[SELECT_126]/[IIf_211]")
                {
                }
            }

            sn.Definition = text;
            sn.Parent = parent;
            //parent.AddChild(sn);
            sn.OffsetFrom = fg.StartOffset;
            sn.Length = fg.FragmentLength;

            nodeCounter++;
            if (sn.RefPath.Path == "Server[@Name='PC12\\SQL2012']/Database[@Name='ESS']/View[@Name='v_Dispatch_Info' and @Schema='Admin']/[CREATE_0]/[SELECT_1]/[Q_2]/[Q_3]")
            {

            }
            return sn;
        }


        private SqlFragmentElement BuildModelFromFragmentTree(FragmentTreeNode treeNode, Dictionary<TSqlFragment, SqlFragmentElement> fragmentsToNodes, MssqlModelElement parent, bool wholeScript, ref int nodeCounter, ReferenceResolver resolver)
        {
            var fg = treeNode.Fragment;

            SqlFragmentElement sn = CreateElementForFragment(fg, parent, wholeScript, ref nodeCounter, resolver);

            if (sn is Model.Mssql.Db.DeclareVariableElement)
            {
                var declareModelElement = (Model.Mssql.Db.DeclareVariableElement)sn;

                var declElement = fg as Microsoft.SqlServer.TransactSql.ScriptDom.DeclareVariableElement;
                if (declElement.DataType.Name != null)  // is not a CURSOR or some other typeless variable
                {
                    var resolvedTypeReference = resolver.ResolvedReferences.Where(x => x.FromFragment != null).FirstOrDefault(x => x.FromFragment.FirstTokenIndex == declElement.DataType.Name.FirstTokenIndex
                    && x.FromFragment.LastTokenIndex == declElement.DataType.Name.LastTokenIndex);
                    // even base types (e.g. NVARCHAR) are data types - these will not be resolved
                    if (resolvedTypeReference != null)
                    {
                        if (resolvedTypeReference.ToObject is DbScriptedElement)
                        {
                            var dbScriptedTypeDefinition = (DbScriptedElement)resolvedTypeReference.ToObject;
                            declareModelElement.SqlTypeDefinition = dbScriptedTypeDefinition.SqlDefinition;
                        }
                    }
                }

            }
            
            fragmentsToNodes[fg] = sn;
            foreach (var child in treeNode.Children)
            {
                var childElement = BuildModelFromFragmentTree(child, fragmentsToNodes, sn, false, ref nodeCounter, resolver);
                sn.AddChild(childElement);
            }
            return sn;
        }

        public RefPath GetDbRefPath(string connectionString)
        {
            return new RefPath(UrnBuilder.GetDbRefPath(connectionString));
        }

        public string GetDbNameFromConnectionString(string connectionString)
        {
            return UrnBuilder.GetDbName(connectionString);
        }

        public string GetServerNameFromConnectionString(string connectionString)
        {
            return UrnBuilder.GetServerName(connectionString, null);
        }
        public bool CompareServerNames(string server1, string server2)
        {
            return UrnBuilder.AreServersNamesEqual(server1, server2);
        }

        public ForeignProviderSqlScriptElement ExtractForeignScriptModel(string script, MssqlModelElement parent, out Dictionary<string, MssqlModelElement> resultColumns)
        {
            IList<ParseError> errors = new List<ParseError>();
            TSqlScript parsed = null;


            var sqlNodePath = parent.RefPath.NamedChild("ForeignSqlCommand", script.GetHashCode().ToString());

            var sqlNode = new ForeignProviderSqlScriptElement(sqlNodePath, "Foreign SQL script");
            sqlNode.Parent = parent;

            //componentNode.AddChild(sqlNode);
            sqlNode.Definition = script;
            //foreignSource = sqlNode;
            
            using (TextReader sr = new StringReader(script))
            {
                TSqlFragment midParsed = null;
                try
                {
                    midParsed = _parser.Parse(sr, out errors);
                }
                catch
                {
                }
                parsed = midParsed as TSqlScript;
                //statementBatch =  as TSqlBatch;
            }
            if (errors.Any())
            {
                resultColumns = null;
                return sqlNode;
            }
            //var statement = parsed.Batches[0].Statements[0];// as SelectStatement;
            SchemaTableSeekingTsqlFragmentVisitor schemaVisitor = new Db.SchemaTableSeekingTsqlFragmentVisitor();
            schemaVisitor.CleanupAndVisit(parsed);
            foreach (var table in schemaVisitor.Tables)
            {
                var tablePath = sqlNode.RefPath.NamedChild("TableReference", table.BaseIdentifier.Value);
                ForeignDbTableElement foreignTable = new ForeignDbTableElement(tablePath, table.BaseIdentifier.Value);
                foreignTable.Parent = sqlNode;
                sqlNode.AddChild(foreignTable);
                foreignTable.Definition = table.GetText();
            }
            if (schemaVisitor.Tables.Count == 0)
            {

            }
            resultColumns = null;
            return sqlNode;
        }

        public string GetServerNameFromConnectionString(string connectionString, string localhostInterpretation)
        {
            return UrnBuilder.GetServerName(connectionString, localhostInterpretation);
        }
    }
}
