using CD.DLS.Serialization;
using CD.DLS.API;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Parse.Mssql.Ssas;
using System.Data.SqlClient;
using System.Net;
using Microsoft.SqlServer.ReportExecution2005;
using System.Web.Services.Protocols;
using CD.DLS.Parse.Mssql;
using CD.DLS.Model.Mssql.Ssrs;
using CD.DLS.Interfaces;
using CD.DLS.Model.Mssql;
using System.Xml;
using CD.DLS.Parse.Mssql.Ssrs;
using CD.DLS.API.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;

namespace CD.DLS.Operations
{
    internal class RenderReportRequestProcessor : BIDocRequestProcessor<RenderReportRequest>
    {
        private Dictionary<string, int> _nodeDictionary = new Dictionary<string, int>();
        private Dictionary<string, int> _textBoxNodeIdDictionary = new Dictionary<string, int>();
        private HashSet<string> _tablixRowCoverage = new HashSet<string>();
        private Dictionary<string, TextBoxElement> _tablixTextBoxNameMap = new Dictionary<string, TextBoxElement>();
        private Dictionary<string, HierarchyMemberElement> _rowGroups;
        private Dictionary<string, HierarchyMemberElement> _columnGroups;
        private TablixElement _currentTablix = null;
        
        public RenderReportRequestProcessor(BIDocCore core) : base(core)
        {
        }

        /*
            exports
         	http://localhost/reportserver/Pages/ReportViewer.aspx?%2fReports%2f07+-+Perm+Dashboard&rs:Format=csv
	
	        http://localhost/reportserver/Pages/ReportViewer.aspx?%2fReports%2f07+-+Perm+Dashboard&rs:Format=excel
	
            http://localhost/ReportServer?%2fReports%2f08+-+Bonuses&DateYearMonth=%5BDate%5D.%5BYear%20-%20Month%5D.%5BMonth%5D.%26%5B2017%5D%26%5B3%5D&BranchBranchLevel1Name=%5BEmployee%20Internal%5D.%5BEmployee%20Home%20Branch%20Level1%20Name%5D.%26%5BBrno%20Staffing%5D&OrderConsultantEmployeeRegionName=%5BOrder%20Consultant%5D.%5BEmployee%20Region%20Name%5D.%5BAll%5D&OrderConsultantEmployeeCOPositionName=%5BOrder%20Consultant%5D.%5BEmployee%20CO%20Position%20Detail%20Name%5D.%5BAll%5D&rs:Format=csv

            ssrs service
            http://localhost/ReportServer/reportservice2010.asmx

            // nested folders
            %2fReports%2fVetrotech%2fPortal+page
         */


        public override ProcessingResult ProcessRequest(RenderReportRequest request, ProjectConfig projectConfig)
        {
            var attachments = new List<Attachment>();
            var es = new ReportExecutionService();
            es.Credentials = CredentialCache.DefaultCredentials;
            // Set the base Web service URL of the source server  
            es.Url = projectConfig.SsrsComponents[0].SsrsExecutionServiceUrl; //.SsrsExecutionServiceUrl;

            // Render arguments
            byte[] result = null;
            string reportPath = request.ReportPath;
            string format = null;
            AttachmentTypeEnum attachmentType = AttachmentTypeEnum.Dataset;
            switch (request.ReportFormat)
            {
                case RenderReportRequest.ReportFormatEnum.Excel:
                    //format = "EXCELOPENXML"; // .xlsx; "EXCEL" for .xls
                    format = "EXCELOPENXMLCDF"; // .xlsx; "EXCEL" for .xls
                    attachmentType = AttachmentTypeEnum.Excel;
                    break;
                case RenderReportRequest.ReportFormatEnum.Nhtml:
                    format = "NTHML";
                    attachmentType = AttachmentTypeEnum.Html;
                    break;
                case RenderReportRequest.ReportFormatEnum.Xml:
                    format = "XML";
                    attachmentType = AttachmentTypeEnum.XML;
                    break;
                case RenderReportRequest.ReportFormatEnum.ReportDataMap:
                    format = "XML";
                    attachmentType = AttachmentTypeEnum.JSON;
                    break;

            }

            string historyID = null;
            string devInfo = @"<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>";

            // Prepare report parameter.
            ParameterValue[] parameters = new ParameterValue[request.ParameterValues.Count];
            for (int i = 0; i < request.ParameterValues.Count; i++)
            {
                var requestParam = request.ParameterValues[i];
                parameters[i] = new ParameterValue() { Name = requestParam.ParameterName, Value = requestParam.ParameterValue };
            }
            
            DataSourceCredentials[] credentials = null;
            string showHideToggle = null;
            string encoding;
            string mimeType;
            string resourceMimeType;
            string extension;
            Warning[] warnings = null;
            ParameterValue[] reportHistoryParameters = null;
            string[] streamIDs = null;

            ExecutionInfo execInfo = new ExecutionInfo();
            ExecutionHeader execHeader = new ExecutionHeader();

            es.ExecutionHeaderValue = execHeader;

            execInfo = es.LoadReport(reportPath, historyID);
            
            //foreach (var parameter in execInfo.Parameters)
            //{
            //    var ser = parameter.Serialize();

            //    var deser = ReportParameter.Deserialize(ser);
            //}

            //var execInfo2 = es.GetExecutionInfo();
            
            //execInfo.Parameters.First().

            es.SetExecutionParameters(parameters, "en-us");
            String SessionId = es.ExecutionHeaderValue.ExecutionID;

            _core.Log.Info("Report Execution ({1}) SessionID: {0}", es.ExecutionHeaderValue.ExecutionID, reportPath);


            try
            {
                var secureMethods = es.ListSecureMethods();

                result = es.Render(format, devInfo, out extension, out encoding, out mimeType, out warnings, out streamIDs);

                execInfo = es.GetExecutionInfo();
                //var execInfo2 = es.GetExecutionInfo2();
                //var renderResource = es.GetRenderResource(format, devInfo, out resourceMimeType);
                var docMap = es.GetDocumentMap();

                _core.Log.Info("Report execution date and time: {0}", execInfo.ExecutionDateTime);
            }
            catch (SoapException e)
            {
                _core.Log.Error(e.Detail.OuterXml);
                throw;
            }
            Uri attachmentUri;
            // Write the contents of the report to an MHTML file.


            if (request.ReportFormat != RenderReportRequest.ReportFormatEnum.ReportDataMap)
            {
                try
                {

                    //using (MemoryStream ms = new MemoryStream(result))
                    //{
                    //    attachmentUri = _core.StorageProvider.Save(ms);
                    //}

                }
                catch (Exception e)
                {
                    _core.Log.Error(e.Message);
                    throw;
                }
            }
            else
            {
                var reportDataMap = GetReportDataMap(result, request, projectConfig);
                var mapSerialized = reportDataMap.Serialize();
                //using (var ms = Tools.Tools.GenerateStreamFromString(mapSerialized))
                //{
                //    attachmentUri = _core.StorageProvider.Save(ms);
                //}
            }

            //attachments.Add(new Attachment() { Uri = attachmentUri, Type = attachmentType, AttachmentId = Guid.NewGuid() });

            var requestResult = new RenderReportResponse();
            var stringResult = requestResult.Serialize();

            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = attachments
            };

            //var webClient = new WebClient();
            //var excelUrl = GetDownloadUrl(projectConfig.SsrsServiceUrl, request, ExportFormat.Excel);
            //var xmlUrl = GetDownloadUrl(projectConfig.SsrsServiceUrl, request, ExportFormat.Xml);

            //using (var excelStream = webClient.OpenRead(excelUrl))
            //{
            //    var excelAttachmentUri = _core.StorageProvider.Save(excelStream);
            //    attachments.Add(new Attachment() { Uri = excelAttachmentUri, Type = AttachmentTypeEnum.Excel, AttachmentId = Guid.NewGuid() });
            //}

            //using (var xmlStream = webClient.OpenRead(xmlUrl))
            //{
            //    var xmlAttachmentUri = _core.StorageProvider.Save(xmlStream);
            //    attachments.Add(new Attachment() { Uri = xmlAttachmentUri, Type = AttachmentTypeEnum.XML, AttachmentId = Guid.NewGuid() });
            //}

        }

        private ReportDataMap GetReportDataMap(byte[] renderedXml, RenderReportRequest request, ProjectConfig projectConfig)
        {


            //throw new NotImplementedException();


            //string reportElementUrn = MssqlModelExtractor.GetReportElementUrnPath(request.ReportPath, new ModelSettings() { Config = projectConfig, Log = _core.Log });
            var modelElement = GraphManager.GetModelElementById(request.ModelElementId);
            ReportElement reportElement = null;

            _core.Log.Important("Converting from DB format");
            //using (var dbContext = new CDFrameworkContext())
            //{
            BIDocModelStored modelStored = new BIDocModelStored(projectConfig.ProjectConfigId, modelElement.RefPath);
            int reportElementId = modelElement.Id; // GraphManager.GetModelElementIdByRefPath(projectConfig.ProjectConfigId, reportElementUrn); // dbContext.ModelElements.First(x => x.RefPath == reportElementUrn).Id;

            FromBIDocModelConverter converterFrom = new FromBIDocModelConverter(modelStored);
            IReflectionHelper reflection = new JsonReflectionHelper(new Model.Mssql.ModelActivator());
            Model.Mssql.ModelConverter converterTo = new Model.Mssql.ModelConverter(reflection);
            MssqlModelElement convertedModel = converterFrom.Convert(converterTo, reportElementId);

            reportElement = (ReportElement)(convertedModel);
            //_nodeDictionary = new Dictionary<string, int>();
            _textBoxNodeIdDictionary = new Dictionary<string, int>();
            List<BIDocGraphInfoLink> links;
            foreach (var graphNode in GraphManager.GetGraphExtended(out links, projectConfig.ProjectConfigId, DependencyGraphKind.DataFlow, modelElement.RefPath))
            {
                _nodeDictionary.Add(graphNode.RefPath, graphNode.Id);
                if (graphNode.NodeType == "TextBoxElement")
                {
                    _textBoxNodeIdDictionary.Add(graphNode.Name, graphNode.Id);
                }
            }
        //}

        var rootElementPosition = GetReportItemPositionsRequestProcessor.GetReportItemPositions(reportElement);

            ReportDataMap result = new ReportDataMap()
            {
                RootElementPosition = rootElementPosition,
                ReportItemDataTables = new List<ReportItemDataTable>()
            };

            XmlDocument doc = new XmlDocument();
            using (MemoryStream ms = new MemoryStream(renderedXml))
            {
                doc.Load(ms);
            }

            var reportItems = rootElementPosition.GetDisplayableItemsTopDown().Where(x => x.IsReportItem).ToList();
            var tablices = reportElement.DescendantsOfType<TablixElement>().ToList();
            foreach (var tablix in tablices)
            {
                var tablixDataElement = (XmlElement)doc.GetElementsByTagName(tablix.Caption)[0];

                if (tablixDataElement == null)
                {
                    continue;
                }

                ReportItemDataTable itemDataTable = new ReportItemDataTable()
                {
                    NodeIds = new List<List<int>>(),
                    ReportItemName = tablix.Caption,
                    ReportItemRefPath = tablix.RefPath.Path,
                    Values = new List<List<string>>(),
                    Width = 0,
                    Height = 0
                };

                // count column levels - vertical offset of the first row
                int colLevelCount = 0;
                var columnLevelMembers = tablix.ColumnHierarchy.Members;
                while (columnLevelMembers.Any())
                {
                    colLevelCount++;
                    columnLevelMembers = columnLevelMembers.SelectMany(x => x.Members);
                }

                ItemDataPosition pos = new ItemDataPosition { X = 0, Y = colLevelCount };

                _tablixRowCoverage = new HashSet<string>();
                _tablixTextBoxNameMap = new Dictionary<string, TextBoxElement>();
                _rowGroups = GetGroupMembers(tablix.RowHierarchy.Members).ToDictionary(x => x.Group.Caption, x => x);
                _columnGroups = GetGroupMembers(tablix.ColumnHierarchy.Members).ToDictionary(x => x.Group.Caption, x => x);
                _currentTablix = tablix;
                foreach (var row in tablix.Rows)
                {
                    foreach (var cell in row.Cells)
                    {
                        var childTextBox = cell.Children.FirstOrDefault() as TextBoxElement;
                        if (childTextBox != null)
                        {
                            _tablixTextBoxNameMap.Add(childTextBox.Caption, childTextBox);
                        }
                    }
                }

                int tablixBodRowIndex = 0;
                var origX = pos.X;
                //FillTableFromNode(tablixDataElement, itemDataTable, pos, ParentMemberTypeEnum.RowMember, tablix.RowHierarchy.Members);

                foreach (var rowMember in tablix.RowHierarchy.Members)
                {
                    FillRowMemberData(rowMember, itemDataTable, pos, tablixDataElement, tablixBodRowIndex, true);
                    tablixBodRowIndex++;
                    pos.X = origX;
                }
                result.ReportItemDataTables.Add(itemDataTable);
            }


            var ds = result.GetDataSet();

            return result;
        }

        private List<HierarchyMemberElement> GetGroupMembers(IEnumerable<HierarchyMemberElement> members)
        {
            return members.Where(x => x.Group != null).Union(members.SelectMany(x => GetGroupMembers(x.Members))).ToList();
        }

        public enum ParentMemberTypeEnum { RowMember, ColumnMember, RowGroupMember, ColumnGroupMember }


        private void FillTableFromNode(XmlNode node, ReportItemDataTable itemDataTable, ItemDataPosition fromPosition, ParentMemberTypeEnum parentMemberType, IEnumerable<HierarchyMemberElement> availableMembers)
        {
            var origX = fromPosition.X;

            var localName = node.LocalName;
            var value = node.Value;
            bool hasValue = !string.IsNullOrEmpty(node.Value);

            // is body cell
            if (_tablixTextBoxNameMap.ContainsKey(localName))
            {
                var textBox = _tablixTextBoxNameMap[localName];
                var row = textBox.Parent.Parent as TablixRowElement;
                if (row != null)
                {
                    // fill previous rows if no records have been written
                    foreach (var previousRow in (_currentTablix.Rows))
                    {
                        if (previousRow.RefPath.Path == row.RefPath.Path)
                        {
                            break;
                        }
                        if (_tablixRowCoverage.Contains(previousRow.RefPath.Path))
                        {
                            continue;
                        }
                        
                        foreach (var cell in previousRow.Cells)
                        {
                            var prevRowCellExpression = cell.DescendantsOfType<SsrsExpressionElement>().FirstOrDefault();
                            if (prevRowCellExpression != null)
                            {
                                var val = prevRowCellExpression.Definition;
                                if (!val.StartsWith("="))
                                {
                                    WriteValueToTable(itemDataTable, fromPosition, val, prevRowCellExpression.Parent.RefPath.Path);
                                }
                            }
                            fromPosition.X++;
                        }
                        fromPosition.X = origX;
                        fromPosition.Y++;

                        _tablixRowCoverage.Add(previousRow.RefPath.Path);
                    }

                    // fill previous empty cells in the current row
                    foreach (var prevCell in row.Cells)
                    {
                        if (prevCell.RefPath.Path == textBox.Parent.RefPath.Path)
                        {
                            break;
                        }
                        var prevCellExpression = prevCell.DescendantsOfType<SsrsExpressionElement>().FirstOrDefault();
                        if (prevCellExpression.Definition.StartsWith("="))
                        {
                            continue;
                        }
                        if (prevCellExpression != null)
                        {
                            var val = prevCellExpression.Definition;
                            WriteValueToTable(itemDataTable, fromPosition, val, prevCellExpression.Parent.RefPath.Path);
                        }
                        fromPosition.X++;
                    }
                    if (hasValue)
                    {
                        WriteValueToTable(itemDataTable, fromPosition, value, textBox.RefPath.Path);
                        fromPosition.X++;
                    }

                    //// end of a new body row
                    //if (textBox.Parent == row.Cells.Last())
                    //{
                    //    if (!_tablixRowRecordCounts.ContainsKey(row.RefPath.Path))
                    //    {
                    //        _tablixRowRecordCounts.Add(row.RefPath.Path, 0);
                    //    }
                    //    _tablixRowRecordCounts[row.RefPath.Path]++;
                    //}
                }
                else
                {
                    throw new Exception("The grandparent of tablix body textbox must be a tablix row");
                }
            }

            // not a body textbox, try filling row or column group headers

            // if no rows have finished yet, fill column headers if available

            if (
                (
                    (parentMemberType == ParentMemberTypeEnum.RowGroupMember || parentMemberType == ParentMemberTypeEnum.RowMember)
                    ||
                    (_tablixRowCoverage.Count == 0 && (parentMemberType == ParentMemberTypeEnum.ColumnMember || parentMemberType == ParentMemberTypeEnum.ColumnGroupMember))
                )
                &&
                (
                    availableMembers.Any(x => GetMemberName(x) == localName)
                )
                )
            {
                //var origX3 = fromPosition.X;
                //bool writtenMember = false;
                foreach (var prevMember in availableMembers)
                {
                    if (GetMemberName(prevMember) == localName)
                    {
                        if (node is XmlElement)
                        {
                            var attrVal = ((XmlElement)node).GetAttribute(localName);
                            if (!string.IsNullOrEmpty(attrVal))
                            {
                                string nodeRefPath = null;
                                if (prevMember.HeaderTextBox != null)
                                {
                                    nodeRefPath = prevMember.HeaderTextBox.RefPath.Path;
                                }
                                WriteValueToTable(itemDataTable, fromPosition, attrVal, nodeRefPath);
                            }
                        }
                        fromPosition.X++;
                        //writtenMember = true;
                        break;
                    }
                    fromPosition.X++;
                }
                /*
                if (!writtenMember)
                {
                    fromPosition.X = origX3;
                }
                */
            }


            // done filling values, go to next element(s)
            // end of row group item, CR LF
            if (parentMemberType == ParentMemberTypeEnum.RowGroupMember)
            {
                fromPosition.X = origX;
                fromPosition.Y++;
            }

            HierarchyMemberElement subMember = null;
            var subMemberMembers = availableMembers;
            var subMemberType = parentMemberType;

            if (localName.EndsWith("_Collection"))
            {
                var preCollection = localName.Substring(0, localName.Length - "_Collection".Length);
                //var membNames = availableMembers.Select(x => GetMemberName(x)).ToList();
                if (_columnGroups.ContainsKey(preCollection))
                {
                    subMemberType = ParentMemberTypeEnum.ColumnGroupMember;
                    subMember = _columnGroups[preCollection];
                }
                else
                {
                    subMemberType = ParentMemberTypeEnum.RowGroupMember;
                    subMember = _rowGroups[preCollection];
                }
                /*
                subMember = availableMembers.FirstOrDefault(x => GetMemberName(x) == preCollection);
                //while (subMember == null)
                //{
                //    availableMembers = 
                //}
                if (subMember.Group != null)
                {
                    if (parentMemberType == ParentMemberTypeEnum.RowGroupMember || parentMemberType == ParentMemberTypeEnum.RowMember)
                    {
                        subMemberType = ParentMemberTypeEnum.RowGroupMember;
                    }
                    else
                    {
                        subMemberType = ParentMemberTypeEnum.ColumnGroupMember;
                    }
                }
                else
                {
                    if (parentMemberType == ParentMemberTypeEnum.RowGroupMember || parentMemberType == ParentMemberTypeEnum.RowMember)
                    {
                        subMemberType = ParentMemberTypeEnum.RowMember;
                    }
                    else
                    {
                        subMemberType = ParentMemberTypeEnum.ColumnMember;
                    }
                }
                */
            }

            if (subMember != null)
            {
                subMemberMembers = subMember.Members;
            }

            // no more row submembers, go to columns
            if (!subMemberMembers.Any() && (parentMemberType == ParentMemberTypeEnum.RowMember || parentMemberType == ParentMemberTypeEnum.RowGroupMember))
            {
                subMemberMembers = _currentTablix.ColumnHierarchy.Members;
                subMemberType = ParentMemberTypeEnum.ColumnMember;
            }

            var origX2 = fromPosition.X;

            if (node.Attributes != null)
            {
                foreach (XmlAttribute subNode in node.Attributes)
                {
                    //var origX3 = fromPosition.X;
                    FillTableFromNode(subNode, itemDataTable, fromPosition, subMemberType, subMemberMembers);
                    //if (parentMemberType == ParentMemberTypeEnum.RowMember || parentMemberType == ParentMemberTypeEnum.RowGroupMember)
                    //{
                    //    fromPosition.Y++;
                    //    fromPosition.X = origX2;
                    //}
                }
            }
            fromPosition.X = origX2;

            foreach (XmlNode subNode in node.ChildNodes)
            {
                FillTableFromNode(subNode, itemDataTable, fromPosition, subMemberType, subMemberMembers);
                if (parentMemberType == ParentMemberTypeEnum.RowMember || parentMemberType == ParentMemberTypeEnum.RowGroupMember)
                {
                    fromPosition.Y++;
                    fromPosition.X = origX2;
                }
            }
            
        }

        private string GetMemberName(HierarchyMemberElement memberElement)
        {
            if (memberElement.Group != null)
            {
                return memberElement.Group.Caption;
            }
            if (memberElement.HeaderTextBox != null)
            {
                return memberElement.HeaderTextBox.Caption;
            }
            //throw new Exception();
            return null;
        }

        /// <summary>
        /// Also moves the position to the beginning of the next row
        /// </summary>
        /// <param name="itemDataTable"></param>
        /// <param name="fromPosition"></param>
        private void FillRowMemberData(HierarchyMemberElement member, ReportItemDataTable itemDataTable, ItemDataPosition fromPosition, XmlElement memberParentXml, int tablixBodyRowIndex, bool isFirstRow)
        {
            var tablix = (TablixElement)member.ReportItem;
            var tablixRow = tablix.Rows.ToList()[tablixBodyRowIndex];

            if (member.Group != null)
            {
                var groupTagName = member.Group.Caption + "_Collection";
                var groupXml = (XmlElement)memberParentXml.GetElementsByTagName(groupTagName)[0];

                if (groupXml == null)
                {
                    return;
                }

                var originalX3 = fromPosition.X;

                foreach (XmlNode groupItemXmlNode in groupXml.ChildNodes /*.GetElementsByTagName(member.Group.Caption)*/)
                {
                    XmlElement groupItemXml = groupItemXmlNode as XmlElement;
                    if (groupItemXml == null)
                    {
                        continue;
                    }
                    if (member.HeaderTextBox != null)
                    {
                        var headerTextBoxName = member.HeaderTextBox.Caption;
                        var textBoxVal = groupItemXml.GetAttribute(headerTextBoxName);
                        WriteValueToTable(itemDataTable, fromPosition, textBoxVal, member.HeaderTextBox.RefPath.Path);
                        fromPosition.X++;
                    }
                    if (member.Members.Any())
                    {
                        var tempTablixRowIndex = tablixBodyRowIndex;
                        foreach (var subMember in member.Members)
                        {
                            var origX2 = fromPosition.X;
                            FillRowMemberData(subMember, itemDataTable, fromPosition, groupItemXml, tempTablixRowIndex, isFirstRow);
                            fromPosition.X = origX2;
                            tempTablixRowIndex++;
                        }
                    }
                    else
                    {
                        var colHierarcy = ((TablixElement)member.ReportItem).ColumnHierarchy;
                        if (colHierarcy.Members.Any(x => CheckGroupedHierarchyExists(x)))
                        {
                            var origX = fromPosition.X;

                            foreach (var columnMember in colHierarcy.Members)
                            {
                                FillGroupedColumnData(columnMember, itemDataTable, fromPosition, groupItemXml, tablixRow, 0);
                            }
                            fromPosition.Y++;
                            fromPosition.X = origX;
                        }
                        else
                        {
                            var origX = fromPosition.X;



                            // fill headers from first row if any
                            if (isFirstRow)
                            {
                                if (tablix.Rows.Count() > 1 && tablixRow != tablix.Rows.First())
                                {
                                    var firstRowCells = tablix.Rows.OrderBy(x => x.Caption).First().Children.OfType<CellElement>();

                                    foreach (var cell in firstRowCells)
                                    {
                                        var cellExpressions = cell.DescendantsOfType<SsrsExpressionElement>();
                                        var cellConstText = string.Join(" ", cellExpressions.Where(x => x.Definition != null).Select(x => x.Definition));
                                        WriteValueToTable(itemDataTable, fromPosition, cellConstText, cell.RefPath.Path);
                                        fromPosition.X++;
                                    }
                                    fromPosition.Y++;
                                    fromPosition.X = origX;
                                }
                            }

                            FillUngroupedColumnData(groupItemXml, tablixRow, itemDataTable, fromPosition);
                            fromPosition.Y++;
                        }
                    }

                    fromPosition.X = originalX3;
                    isFirstRow = false;
                }
                return;
            }
            // end group

            //var membersXml = memberParentXml.GetElementsByTagName(member.HeaderTextBox.Caption);
            //var memberXml = (XmlElement)membersXml[0];

            //if (member.HeaderTextBox == null)
            //{
            //    throw new Exception();
            //}

            XmlElement memberXml = null;


            if (member.HeaderTextBox != null)
            {
                var membersXml = memberParentXml.GetElementsByTagName(member.HeaderTextBox.Caption);
                //if (membersXml[0] != null)
                //{
                memberXml = (XmlElement)membersXml[0];

                var headerTextBoxName = member.HeaderTextBox.Caption;
                var textBoxVal = memberXml.GetAttribute(headerTextBoxName);
                if (string.IsNullOrEmpty(textBoxVal))
                {
                    var expressions = member.HeaderTextBox.DescendantsOfType<SsrsExpressionElement>();
                    var expressionConcat = string.Join(" ", expressions.Select(x => x.Definition));
                    textBoxVal = expressionConcat;
                }
                WriteValueToTable(itemDataTable, fromPosition, textBoxVal, member.HeaderTextBox.RefPath.Path);
                fromPosition.X++;
                //}
            }
            //else //if (memberXml == null)
            //{
            //    // no header row, but the parent xml belongs to this tablix row
            //    if (tablixRow.CellItemNames.Any(x => memberParentXml.LocalName == x))
            //    {
            //        memberXml = memberParentXml;
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}
            //else
            //{
            //    if (member.Members.Count() > 1)
            //    {
            //        return;
            //    }
            //    else
            //    {
            //        memberXml = memberParentXml;
            //    }

            //}

            if (member.Members.Any())
            {
                foreach (var subMember in member.Members)
                {
                    var preincX = fromPosition.X;
                    FillRowMemberData(subMember, itemDataTable, fromPosition, memberXml, tablixBodyRowIndex, isFirstRow);
                    fromPosition.X = preincX;
                    tablixBodyRowIndex++;
                }
            }
            else
            {
                var colHierarcy = tablix.ColumnHierarchy;
                if (colHierarcy.Members.Any(x => CheckGroupedHierarchyExists(x)))
                {
                    var origX = fromPosition.X;

                    foreach (var columnMember in colHierarcy.Members)
                    {
                        FillGroupedColumnData(columnMember, itemDataTable, fromPosition, (memberXml == null ? memberParentXml : memberXml), tablixRow, 0);

                        fromPosition.X++;
                    }
                    fromPosition.Y++;
                    fromPosition.X = origX;
                }
                // no column groups
                else
                {
                    var origX = fromPosition.X;


                    // fill headers from first row if any
                    if (isFirstRow && colHierarcy.Members.Any())
                    {
                        fromPosition.Y = 0;
                        /*
                        if (tablix.Rows.Count() > 1)
                        {
                            var firstRowCells = tablix.Rows.OrderBy(x => x.Caption).First().Children.OfType<CellElement>();

                            foreach (var cell in firstRowCells)
                            {
                                var cellExpressions = cell.DescendantsOfType<SsrsExpressionElement>();
                                var cellConstText = string.Join(" ", cellExpressions.Select(x => x.Definition.StartsWith("=") ? "..." : x.Definition));
                                WriteValueToTable(itemDataTable, fromPosition, cellConstText);
                                fromPosition.X++;
                            }
                            fromPosition.Y++;
                            fromPosition.X = origX;
                        }
                        */
                        foreach (var headerMmber in colHierarcy.Members)
                        {
                            if (headerMmber.HeaderTextBox != null)
                            {
                                var cellExpressions = headerMmber.HeaderTextBox.DescendantsOfType<SsrsExpressionElement>();
                                var cellConstText = string.Join(" ", cellExpressions.Select(x => x.Definition));
                                WriteValueToTable(itemDataTable, fromPosition, cellConstText, headerMmber.HeaderTextBox.RefPath.Path);
                            }
                            fromPosition.X++;
                        }
                        fromPosition.Y++;
                        fromPosition.X = origX;
                    }
                    FillUngroupedColumnData(memberXml, tablixRow, itemDataTable, fromPosition);
                    fromPosition.Y++;

                    /*
                var firstRowCells = tablix.Rows.OrderBy(x => x.Caption).First().Children.OfType<CellElement>();
                var firstRowTextBoxes = firstRowCells.SelectMany(x => x.Children.OfType<TextBoxElement>());
                bool dataInFirstRow = false;
                bool firstRowNotCovered = false;
                foreach (XmlAttribute attr in memberXml.Attributes)
                {
                    if (firstRowTextBoxes.Any(x => x.Caption == attr.LocalName))
                    {
                        dataInFirstRow = true;
                    }
                    else
                    {
                        firstRowNotCovered = true;
                    }
                }
                if (dataInFirstRow && firstRowNotCovered)
                {
                    throw new Exception();
                }

                if (firstRowNotCovered)
                {
                    foreach (var cell in firstRowCells)
                    {
                        var cellExpressions = cell.DescendantsOfType<SsrsExpressionElement>();
                        var cellConstText = string.Join(" ", cellExpressions.Select(x => x.Definition.StartsWith("=") ? "..." : x.Definition));
                        WriteValueToTable(itemDataTable, fromPosition, cellConstText);
                        fromPosition.X++;
                    }
                    fromPosition.Y++;
                    fromPosition.X = origX;
                }
            }

            var tablixRow = tablix.Rows.ToList()[tablixBodyRowIndex];
            FillUngroupedColumnData()
                */

                    //
                }
            }
        }

        private bool CheckGroupedHierarchyExists(HierarchyMemberElement member)
        {
            if (member.Group != null)
            {
                return true;
            }
            if (!member.Members.Any())
            {
                return false;
            }
            return member.Members.Any(x => CheckGroupedHierarchyExists(x));
        }

        Dictionary<string, string> CollectAttributes(XmlElement element)
        {
            Dictionary<string, string> attrs = new Dictionary<string, string>();
            if (element == null)
            {
                return attrs;
            }
            foreach (XmlAttribute attr in element.Attributes)
            {
                attrs.Add(attr.LocalName, attr.Value);
            }

            foreach (var child in element.ChildNodes)
            {
                var childElem = child as XmlElement;
                if (childElem != null)
                {
                    var childDir = CollectAttributes(childElem);
                    foreach (var kv in childDir)
                    {
                        if (!attrs.ContainsKey(kv.Key))
                        {
                            attrs.Add(kv.Key, kv.Value);
                        }
                    }
                }
            }
            return attrs;
        }

        private void FillUngroupedColumnData(XmlElement rowXml, TablixRowElement tablixRow, ReportItemDataTable itemDataTable, ItemDataPosition fromPosition)
        {
            var origX = fromPosition.X;


            // all cells are constant
            //if (tablixRow.Cells.All(x => !x.DescendantsOfType<SsrsExpressionElement>().Any(y => y.Definition != null && y.Definition.StartsWith("="))))
            //{
            //    foreach (var cell in tablixRow.Cells)
            //    {
            //        var cellText = string.Join(" ", cell.DescendantsOfType<SsrsExpressionElement>().Select(x => x.Definition == null ? string.Empty : x.Definition));
            //        WriteValueToTable(itemDataTable, fromPosition, cellText);

            //        fromPosition.X++;
            //    }

            //}
            //else
            //{

            var attrDir = CollectAttributes(rowXml);

            //foreach (XmlElement rowXml in valueNodes)
            //{
            foreach (var cell in tablixRow.Cells)
            {
                var textBox = cell.Children.FirstOrDefault() as TextBoxElement;
                if (textBox == null)
                {
                    fromPosition.X++;
                    continue;
                }
                // is expression value
                if (textBox.Children.Any(x => x.Definition != null && x is SsrsExpressionElement && x.Definition.StartsWith("=")))
                {
                    if (attrDir.ContainsKey(textBox.Caption))
                    {
                        WriteValueToTable(itemDataTable, fromPosition, attrDir[textBox.Caption], textBox.RefPath.Path);
                    }
                }
                else
                {
                    var caption = string.Join(" ", textBox.Children.OfType<SsrsExpressionElement>().Where(x => x.Definition != null).Select(x => x.Definition));
                    WriteValueToTable(itemDataTable, fromPosition, caption, textBox.RefPath.Path);
                }
                fromPosition.X++;
            }
            //}
            fromPosition.X = origX;
            //}
        }

        private void FillGroupedColumnData(HierarchyMemberElement member, ReportItemDataTable itemDataTable, ItemDataPosition fromPosition, XmlElement rowXml,
            TablixRowElement tablixRow, int columnGroupLevel, int columnNumber = 0)
        {

            var origColumnNumber = columnNumber;
            if (member.Group == null)
            {
                var firstTextBox = tablixRow.Cells.Skip(columnNumber).First().Children.First() as TextBoxElement;
                if (firstTextBox == null)
                {
                    return;
                }
                var firstTextBoxName = firstTextBox.Caption;

                //if (member.Members.Count() > 0)
                //{
                //    throw new Exception();
                //}

                var firstTbAttr = rowXml.GetAttribute(firstTextBoxName);
                //XmlElement coveringElement = rowXml;
                //if (firstTbAttr == null)
                //{
                //    coveringElement = (XmlElement)rowXml.GetElementsByTagName(firstTextBoxName)[0];
                //}
                XmlElement coveringElement;
                var firstTextBoxAttr = rowXml.GetAttribute(firstTextBoxName);
                if (firstTextBoxAttr != null)
                {
                    coveringElement = rowXml;
                }
                else
                {
                    coveringElement = (XmlElement)rowXml.GetElementsByTagName(firstTextBoxName)[0];
                }
                if (member.HeaderTextBox != null)
                {
                    if (coveringElement == null)
                    {
                        coveringElement = (XmlElement)rowXml.GetElementsByTagName(member.HeaderTextBox.Caption)[0];
                    }
                    var headerPosition = new ItemDataPosition() { X = fromPosition.X, Y = columnGroupLevel };
                    var headerValue = coveringElement.GetAttribute(member.HeaderTextBox.Caption);
                    if (headerValue == null)
                    {
                        var textBox = member.HeaderTextBox;
                        headerValue = string.Join(" ", textBox.Children.OfType<SsrsExpressionElement>().Where(x => x.Definition != null).Select(x => x.Definition));
                    }
                    if (headerValue != null)
                    {
                        WriteValueToTable(itemDataTable, headerPosition, headerValue, member.HeaderTextBox.RefPath.Path);
                    }
                }
                else // column group <X_Collection><X X="AA" /></X_Collection>
                {
                    var elemName = rowXml.LocalName;
                    var headerAttr = rowXml.GetAttribute(elemName);
                    if (!string.IsNullOrEmpty(headerAttr))
                    {
                        if (coveringElement == null)
                        {
                            coveringElement = rowXml;
                        }
                        var headerPosition = new ItemDataPosition() { X = fromPosition.X, Y = columnGroupLevel };
                        var headerValue = headerAttr;
                        if (headerValue != null)
                        {
                            WriteValueToTable(itemDataTable, headerPosition, headerValue, _textBoxNodeIdDictionary[headerAttr] /* headerAttr*/);
                        }
                    }
                }

                if (member.Members.Any())
                {
                    int colIdx = 0;
                    //var origX4 = fromPosition.X;
                    foreach (var subMember in member.Members)
                    {
                        FillGroupedColumnData(subMember, itemDataTable, fromPosition, coveringElement, tablixRow, columnGroupLevel + 1, columnNumber);
                        columnNumber++;
                        fromPosition.X++;
                    }
                    //fromPosition.X = origX4;
                }
                else
                {
                    if (coveringElement == null)
                    {
                        // TODO: empty cell or something worse?
                        return;
                    }
                    //if (coveringElement.Attributes.Count > 2)
                    //{
                    //    throw new Exception("Max 2 attributes expected in a column member");
                    //}
                    //foreach (XmlAttribute attr in coveringElement.Attributes)
                    //{
                    //    if (member.HeaderTextBox == null || member.HeaderTextBox.Caption != attr.LocalName)
                    //    {
                    //        WriteValueToTable(itemDataTable, fromPosition, attr.Value);
                    //    }
                    //}
                    var colAtr = tablixRow.CellItemNames[columnNumber];
                    var attrVal = coveringElement.GetAttribute(colAtr);
                    var cell = tablixRow.Cells.ToList()[columnNumber];
                    WriteValueToTable(itemDataTable, fromPosition, attrVal, cell.RefPath.Path);
                }

                /*
                var subColNumber = 0;
                foreach (XmlAttribute attr in rowXml.Attributes)
                {
                    while (subColNumber + subColNumber < tablixRow.Cells.Count())
                    {
                        var colCell = tablixRow.Cells.Skip(columnNumber + subColNumber).First();
                        var cellItem = colCell.Children.FirstOrDefault();
                        if (cellItem == null)
                        {
                            subColNumber++;
                            fromPosition.X++;
                            continue;
                        }
                        var cellItemName = cellItem.Caption;
                        if (cellItemName != attr.LocalName)
                        {
                            subColNumber++;
                            fromPosition.X++;
                            continue;
                        }
                        else
                        {
                            WriteValueToTable(itemDataTable, fromPosition, attr.Value);
                            subColNumber++;
                            fromPosition.X++;
                            // atribute matched, go to next
                            break;
                        }
                    }
                }
                */
            }
            // group
            else
            {
                var collectionElemName = member.Group.Caption + "_Collection";
                var collectionElem = (XmlElement)rowXml.GetElementsByTagName(collectionElemName)[0];
                //var origX = fromPosition.X;
                foreach (XmlElement groupItem in collectionElem.GetElementsByTagName(member.Group.Caption))
                {
                    var origColNumber2 = columnNumber;
                    if (member.Members.Any())
                    {
                        foreach (var subMember in member.Members)
                        {
                            FillGroupedColumnData(subMember, itemDataTable, fromPosition, groupItem, tablixRow, columnGroupLevel + 1, columnNumber);
                            columnNumber++;
                            fromPosition.X++;
                        }
                    }
                    else
                    {
                        if (member.HeaderTextBox != null)
                        {
                            var headerPosition = new ItemDataPosition() { X = fromPosition.X, Y = columnGroupLevel };
                            var headerValue = groupItem.GetAttribute(member.HeaderTextBox.Caption);
                            if (headerValue == null)
                            {
                                var textBox = member.HeaderTextBox;
                                headerValue = string.Join(" ", textBox.Children.OfType<SsrsExpressionElement>().Where(x => x.Definition != null).Select(x => x.Definition));
                            }
                            if (headerValue != null)
                            {
                                WriteValueToTable(itemDataTable, headerPosition, headerValue, member.HeaderTextBox.RefPath.Path);
                            }
                        }

                        if (groupItem.Attributes.Count > 2)
                        {
                            throw new Exception("Max 2 attributes expected in a column member");
                        }
                        foreach (XmlAttribute attr in groupItem.Attributes)
                        {
                            if (member.HeaderTextBox == null || member.HeaderTextBox.Caption != attr.LocalName)
                            {
                                var textBoxNodeId = _textBoxNodeIdDictionary[attr.LocalName];
                                WriteValueToTable(itemDataTable, fromPosition, attr.Value, textBoxNodeId);
                            }
                        }

                        fromPosition.X++;
                    }
                    columnNumber = origColNumber2;

                }

                //fromPosition.X = origX;
            }

        }

        public class ItemDataPosition
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        private void WriteValueToTable(ReportItemDataTable itemDataTable, ItemDataPosition position, string value, int? graphNodeId)
        {
            // add rows if missing
            //if (position.X == 0)
            //{
            while (position.Y + 1 > itemDataTable.Values.Count)
            {
                itemDataTable.Values.Add(new List<string>());
                itemDataTable.NodeIds.Add(new List<int>());
                itemDataTable.Height++;
            }
            //}

            var row = itemDataTable.Values[position.Y];
            var nodeRow = itemDataTable.NodeIds[position.Y];
            while (position.X + 1 > row.Count)
            {
                row.Add(string.Empty);
                nodeRow.Add(-1);
            }
            row[position.X] = value;
            if (graphNodeId != null)
            {
                nodeRow[position.X] = graphNodeId.Value /*_nodeDictionary[graphNodeId]*/;
            }
            if (row.Count > itemDataTable.Width)
            {
                itemDataTable.Width = row.Count;
            }
        }


        private void WriteValueToTable(ReportItemDataTable itemDataTable, ItemDataPosition position, string value, string refPath)
        {
            // add rows if missing
            //if (position.X == 0)
            //{
            while (position.Y + 1 > itemDataTable.Values.Count)
            {
                itemDataTable.Values.Add(new List<string>());
                itemDataTable.NodeIds.Add(new List<int>());
                itemDataTable.Height++;
            }
            //}

            var row = itemDataTable.Values[position.Y];
            var nodeRow = itemDataTable.NodeIds[position.Y];
            while (position.X + 1 > row.Count)
            {
                row.Add(string.Empty);
                nodeRow.Add(-1);
            }
            row[position.X] = value;
            if (refPath != null)
            {
                nodeRow[position.X] = _nodeDictionary[refPath];
            }
            if (row.Count > itemDataTable.Width)
            {
                itemDataTable.Width = row.Count;
            }
        }


        //public enum ExportFormat
        //{
        //    Excel, Xml
        //}

        //public string GetDownloadUrl(string ssrsServiceUrl, RenderReportRequest renderRequest, ExportFormat exportFormat)
        //{
        //    ssrsServiceUrl = ssrsServiceUrl.TrimEnd('/');
        //    var serverUrl = ssrsServiceUrl.Substring(0, ssrsServiceUrl.LastIndexOf('/'));
        //    // http://localhost/ReportServer
        //    var reportPath = renderRequest.ReportPath;
        //    var reportPathEncoded = WebUtility.UrlEncode(reportPath);
        //    var parametersEncoded = new StringBuilder();
        //    foreach (var par in renderRequest.ParameterValues)
        //    {
        //        parametersEncoded.Append(string.Format("&{0}={1}", WebUtility.UrlEncode(par.ParameterName), WebUtility.UrlEncode(par.ParameterValue)));
        //    }

        //    string exportFormatParam = null;
        //    switch (exportFormat)
        //    {
        //        case ExportFormat.Excel:
        //            exportFormatParam = "excel";
        //            break;
        //        case ExportFormat.Xml:
        //            exportFormatParam = "xml";
        //            break;
        //    }

        //    var url = string.Format("{0}?{1}{2}&rs:Format={3}", serverUrl, reportPathEncoded, parametersEncoded.ToString(), exportFormatParam);
        //    return url;
        //}
    }
}
