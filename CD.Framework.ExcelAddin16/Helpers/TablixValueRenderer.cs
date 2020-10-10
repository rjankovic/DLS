using CD.DLS.API.Structures;
using CD.DLS.DAL.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CD.Framework.ExcelAddin16.Helpers
{
    public class ReportTextBoxValue
    {
        public ReportElementAbsolutePosition RdlPosition { get; set; }
        public ReportElementAbsolutePosition TablixPosition { get; set; }
        public string Value { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string RenderedSheet { get; set; }
        public int RenderedRow { get; set; }
        public int RenderedColumn { get; set; }

        public override string ToString()
        {
            return Value;
        }

        public ReportTextBoxValue ShallowCopy()
        {
            return new ReportTextBoxValue()
            {
                RdlPosition = this.RdlPosition,
                TablixPosition = this.TablixPosition,
                Value = this.Value,
                X = this.X,
                Y = this.Y,
                RenderedSheet = this.RenderedSheet,
                RenderedRow = this.RenderedRow,
                RenderedColumn = this.RenderedColumn
            };
        }
    }

    public class TablixValueRenderer
    {
        private TablixGroupHierarchy _rowHierarchy = null;
        private TablixGroupHierarchy _columnHierarchy = null;
        Dictionary<string, List<string>> _rowGroupTextBoxes = null;
        Dictionary<string, List<string>> _columnGroupTextBoxes = null;
        Dictionary<string, Tuple<int, int>> _columnGroupSpans = null;
        Dictionary<string, Tuple<int, int>> _rowGroupSpans = null;
        Dictionary<string, Tuple<int, int>> _columnGroupRdlSpans = null;
        Dictionary<string, Tuple<int, int>> _rowGroupRdlSpans = null;
        Dictionary<string, List<string>> _groupAncestorLists = null;
        Dictionary<string, int> _columnGroupRdlWidths = null;
        Dictionary<string, int> _rowGroupRdlHeights = null;
        Dictionary<string, int> _textBoxRowSpans = null;
        Dictionary<string, int> _textBoxColumnSpans = null;
        //[row, column]
        Dictionary<string, Tuple<int, int>> _textBoxDesignPositions = null;
        List<List<ReportTextBoxValue>> _values = null;
        private string _currentRowGroup = null;
        private string _currentColumnGroup = null;
        private int _tablixRow = 0;
        private int _tablixColumn = 0;
        private int _valuesTableRow = 0;
        private int _valuesTableColumn = 0;
        private ReportElementAbsolutePosition _tablixPosition = null;
        private XmlElement _tablixElement = null;
        private Dictionary<string, string> _groupElementNames = null;

        private const bool RENDERER_DEBUG = false;

        public List<ReportElementAbsolutePosition> FindTablixes(ReportElementAbsolutePosition root)
        {
            List<ReportElementAbsolutePosition> res = new List<ReportElementAbsolutePosition>();

            if (root.Type == "TablixElement")
            {
                res.Add(root);
            }
            else
            {
                res.AddRange(root.Children.SelectMany(x => FindTablixes(x)));
            }
            return res;
        }

        /// <summary>
        /// [row,column]
        /// </summary>
        /// <param name="tablixPosition"></param>
        /// <param name="reportData"></param>
        /// <returns></returns>
        public ReportTextBoxValue[,] GetTablixValuesTable(ReportElementAbsolutePosition tablixPosition, XmlDocument reportData)
        {
            var leftRightValues = tablixPosition.GetDisplayableItemsLeftRight();
            _rowHierarchy = tablixPosition.RowHierarchy;
            _columnHierarchy = tablixPosition.ColumnHierarchy;

            var tablixElement = FindDescendantElement(reportData.DocumentElement, tablixPosition.Name);

            // [row, column]
            _values = new List<List<ReportTextBoxValue>>();

            var rowGroups = CollectGroupsAndTextBoxes(tablixPosition.RowHierarchy.Members);
            var colGroups = CollectGroupsAndTextBoxes(tablixPosition.ColumnHierarchy.Members);
            _rowGroupTextBoxes = rowGroups.ToDictionary(x => x.Item1, x => x.Item2);
            _columnGroupTextBoxes = colGroups.ToDictionary(x => x.Item1, x => x.Item2);
            _tablixPosition = tablixPosition;
            _tablixElement = tablixElement;
            _rowGroupSpans = new Dictionary<string, Tuple<int, int>>();
            _columnGroupSpans = new Dictionary<string, Tuple<int, int>>();
            _rowGroupRdlSpans = new Dictionary<string, Tuple<int, int>>();
            _columnGroupRdlSpans = new Dictionary<string, Tuple<int, int>>();
            _columnGroupRdlWidths = new Dictionary<string, int>();
            _rowGroupRdlHeights = new Dictionary<string, int>();
            _textBoxRowSpans = new Dictionary<string, int>();
            _textBoxColumnSpans = new Dictionary<string, int>();
            _groupElementNames = new Dictionary<string, string>();

            _groupAncestorLists = new Dictionary<string, List<string>>();
            //var topLevelHierarchyMembers = _rowHierarchy.Members.Union(_columnHierarchy.Members).ToList();
            FindTextBoxDesignPositions();

            int cummulativeHeight = 0;
            foreach (var topLevelHierarchyMember in _rowHierarchy.Members)
            {
                cummulativeHeight += BuildGroupAncestorsDFS(topLevelHierarchyMember, false, null, cummulativeHeight, 0);
            }
            int cummulativeWidth = 0;
            foreach (var topLevelHierarchyMember in _columnHierarchy.Members)
            {
                cummulativeWidth += BuildGroupAncestorsDFS(topLevelHierarchyMember, true, null, 0, cummulativeWidth);
            }

            _tablixRow = 0;
            _tablixColumn = -1;
            _valuesTableRow = 0;
            _valuesTableColumn = 0;
            _currentColumnGroup = null;
            _currentRowGroup = null;

            bool start = true;
            ReportElementAbsolutePosition previousTextBox = null;
            while (MoveToNextTextBox())
            {
                var tb = GetCurrentTextBox();
                if (!start)
                {
                    StandardShiftInValuesTable(previousTextBox, tb);
                }
                FillCurrentTextBox(tablixElement);
                previousTextBox = tb;
                start = false;
            }

            // ommit empty rows
            _values.RemoveAll(x => x.Count == 0);
            var maxLen = _values.Max(x => x.Count);
            var res = new ReportTextBoxValue[_values.Count, maxLen];

            for (int row = 0; row < _values.Count; row++)
            {
                for (int column = 0; column < maxLen; column++)
                {
                    if (_values[row].Count > column)
                    {
                        res[row, column] = _values[row][column];
                        if (_values[row][column] != null)
                        {
                            res[row, column].TablixPosition = tablixPosition;
                        }
                    }
                }
            }

            return res;
        }

        private void StandardShiftInValuesTable(ReportElementAbsolutePosition previousTextBox, ReportElementAbsolutePosition currentTextBox)
        {
            var previousDesignPosition = _textBoxDesignPositions[previousTextBox.Name];
            var currentDesignPosition = _textBoxDesignPositions[currentTextBox.Name];

            if (previousDesignPosition.Item1 == currentDesignPosition.Item1)
            {
                // shift right
                _valuesTableColumn += currentDesignPosition.Item2 - previousDesignPosition.Item2;
            }
            else
            {
                // new row
                _valuesTableRow += Math.Max(currentDesignPosition.Item1 - previousDesignPosition.Item1, 1);
                _valuesTableColumn = currentDesignPosition.Item2;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentMember"></param>
        /// <param name="parentGroup"></param>
        /// <returns>The group widht(member count</returns>
        private int BuildGroupAncestorsDFS(TablixGroupHierarchyMember currentMember, bool isColumnGroup, TablixGroupHierarchyMember parentGroup, int defaultRowPosition, int defaultColumnPosition)
        {
            if (currentMember.GroupName != null)
            {
                _groupAncestorLists.Add(currentMember.GroupName, new List<string>() { currentMember.GroupName });
                _groupElementNames.Add(currentMember.GroupName, currentMember.DataElementName);

                // concat ancestor groups
                if (parentGroup != null)
                {
                    if (parentGroup.GroupName != null)
                    {
                        _groupAncestorLists[currentMember.GroupName].AddRange(_groupAncestorLists[parentGroup.GroupName]);
                    }
                }
            }

            var nextPosRow = defaultRowPosition;
            var nextPosColumn = defaultColumnPosition;
            //if (isColumnGroup)
            //{
            //    nextPosColumn++;
            //}
            //else
            //{
            //    nextPosRow++;
            //}

            int width = 0;
            foreach (var subMember in currentMember.ChildMembers /*.Where(x => x.GroupName != null)*/ )
            {
                if (isColumnGroup)
                {
                    nextPosColumn = defaultColumnPosition + width;
                }
                else
                {
                    nextPosRow = defaultRowPosition + width;
                }
                width += BuildGroupAncestorsDFS(subMember, isColumnGroup,
                    currentMember.GroupName != null ? currentMember : parentGroup,
                    nextPosRow, nextPosColumn);
            }

            // member with no submembers => width = 1
            var myWidth = Math.Max(width, 1);

            if (currentMember.GroupName != null)
            {
                if (isColumnGroup)
                {
                    _columnGroupRdlWidths[currentMember.GroupName] = myWidth;
                }
                else
                {
                    _rowGroupRdlHeights[currentMember.GroupName] = myWidth;
                }


                Tuple<int, int> textBoxPosition = new Tuple<int, int>(defaultRowPosition, defaultColumnPosition);
                if (currentMember.HeaderTextBox != null)
                {
                    textBoxPosition = _textBoxDesignPositions[currentMember.HeaderTextBox];
                }
                else if (currentMember.ChildMembers.Where(x => x.GroupName == null).Count() == 1)
                {
                    var firstNonGroupChild = currentMember.ChildMembers.First(x => x.GroupName == null);
                    if (firstNonGroupChild.HeaderTextBox != null)
                    {
                        textBoxPosition = _textBoxDesignPositions[firstNonGroupChild.HeaderTextBox];
                    }
                }

                if (isColumnGroup)
                {
                    // X - position
                    _columnGroupRdlSpans.Add(currentMember.GroupName, new Tuple<int, int>(textBoxPosition.Item2, textBoxPosition.Item2 + myWidth - 1));
                    _columnGroupSpans.Add(currentMember.GroupName, _columnGroupRdlSpans[currentMember.GroupName]);
                }
                else
                {
                    // Y - position
                    _rowGroupRdlSpans.Add(currentMember.GroupName, new Tuple<int, int>(textBoxPosition.Item1, textBoxPosition.Item1 + myWidth - 1));
                    _rowGroupSpans.Add(currentMember.GroupName, _rowGroupRdlSpans[currentMember.GroupName]);
                }
            }

            var leadTbName = currentMember.HeaderTextBox;


            return myWidth;
        }

        private void FindTextBoxDesignPositions()
        {
            _textBoxDesignPositions = new Dictionary<string, Tuple<int, int>>();
            var allTextBoxes = _tablixPosition.GetDisplayableItemsLeftRight();
            var xPoints = allTextBoxes.Select(x => x.Left).Distinct().OrderBy(x => x).ToList();
            var yPoints = allTextBoxes.Select(x => x.Top).Distinct().OrderBy(x => x).ToList();
            foreach (var tb in allTextBoxes)
            {
                _textBoxDesignPositions.Add(tb.Name, new Tuple<int, int>(yPoints.IndexOf(tb.Top), xPoints.IndexOf(tb.Left)));
                var midXPoints = xPoints.Where(x => x >= tb.Left && x < tb.Left + tb.Width);
                var midYPoints = yPoints.Where(y => y >= tb.Top && y <= tb.Top + tb.Height);
                _textBoxColumnSpans[tb.Name] = midXPoints.Count(); // Math.Max(midXPoints.Count(), 1);
                _textBoxRowSpans[tb.Name] = midYPoints.Count(); // Math.Max(midYPoints.Count(), 1);
            }
        }

        /// <summary>
        /// ... and other textboxes if a group is encountered
        /// TextBox shifts (RDL & value tables) are handled by the calling method
        /// </summary>
        /// <param name="currentElement"></param>
        private void FillCurrentTextBox(XmlElement currentElement)
        {
            bool isFirstInRow;
            bool isLastInRow;
            bool lastInRowGroup;
            bool lastInColumnGroup;

            var rdlTextBox = GetCurrentTextBox(out isFirstInRow, out isLastInRow, out lastInRowGroup, out lastInColumnGroup);

            // value in the current element
            List<XmlElement> childTbElements = new List<XmlElement>();
            var currentDrillElement = currentElement;
            while (currentDrillElement != null)
            {
                XmlElement firstChild = null;
                int childCount = 0;
                string firstChildName = string.Empty;
                foreach (XmlNode childNode in currentDrillElement.ChildNodes)
                {
                    if (childNode is XmlElement)
                    {
                        if (_textBoxDesignPositions.ContainsKey(childNode.LocalName))
                        {
                            // only one and not group
                            // and there can be a subsequent group after the header columns
                            if (childCount == 0)
                            {
                                firstChild = (XmlElement)childNode;
                                firstChildName = childNode.LocalName;
                            }
                            childCount++;


                            childTbElements.Add((XmlElement)childNode);
                        }
                    }
                }

                if (childCount == 1 /*&& _textBoxDesignPositions.ContainsKey(firstChildName)*/)
                {
                    currentDrillElement = firstChild;
                }
                else
                {
                    currentDrillElement = null;
                }
            }
            int childrenWithAttribute = 0;
            string childAttributeValue = null;
            foreach (XmlElement childTbElement in childTbElements)
            {
                if (childTbElement.HasAttribute(rdlTextBox.Name))
                {
                    childrenWithAttribute++;
                    childAttributeValue = childTbElement.GetAttribute(rdlTextBox.Name);
                }
            }
            if (currentElement.HasAttribute(rdlTextBox.Name))
            {
                var attributeValue = currentElement.GetAttribute(rdlTextBox.Name);
                WriteCellValuePlain(attributeValue);
            }
            else if (childrenWithAttribute == 1)
            {
                //var attributeValue = childTbElements[0].GetAttribute(rdlTextBox.Name);
                WriteCellValuePlain(childAttributeValue);
            }
            // group (beginning)
            // or nested values
            else
            {
                if (!FillGroupValues(rdlTextBox, currentElement))
                {
                    // constant
                    if (rdlTextBox.Text != null && !rdlTextBox.Text.StartsWith("="))
                    {
                        WriteCellValuePlain(rdlTextBox.Text);
                    }
                    else
                    {
                        var nestedValueElements = currentElement.GetElementsByTagName(rdlTextBox.Name);
                        if (nestedValueElements.Count > 0)
                        {
                            var nestedElement = (XmlElement)nestedValueElements[0];
                            var attributeValue = nestedElement.GetAttribute(rdlTextBox.Name);
                            WriteCellValuePlain(attributeValue);
                        }
                        /**/
                        else
                        {
                            // find child element at the currect row... last option - cannot tell if the nested FillCurrentTextBox succeeds

                            var textBoxesLeftRight = _tablixPosition.Children[_tablixRow].GetDisplayableItemsLeftRight();
                            var presentTextBox = textBoxesLeftRight.FirstOrDefault(x => currentElement.GetElementsByTagName(x.Name).Count
                            == 1);
                            if (presentTextBox != null)
                            {
                                if (_textBoxDesignPositions[presentTextBox.Name].Item2 <= _tablixColumn)
                                {
                                    var subElement = (XmlElement)(currentElement.GetElementsByTagName(presentTextBox.Name)[0]);
                                    FillCurrentTextBox(subElement);
                                }
                            }
                        }
                        /**/

                        //... else
                    }
                }

                //// still could be column group cells outside of row group
                //if (!groupFound)
                //{

                //}
            }
        }

        private bool FillGroupValues(ReportElementAbsolutePosition rdlTextBox, XmlElement currentElement)
        {


            var rowGroupsStartingHere = _rowGroupRdlSpans.Where(x => x.Value.Item1 <= _textBoxDesignPositions[rdlTextBox.Name].Item1 && x.Value.Item2 >= _textBoxDesignPositions[rdlTextBox.Name].Item1).Select(x => x.Key).ToList();
            var columnGroupsStartingHere = _columnGroupRdlSpans.Where(x => x.Value.Item1 <= _textBoxDesignPositions[rdlTextBox.Name].Item2 && x.Value.Item2 >= _textBoxDesignPositions[rdlTextBox.Name].Item2).Select(x => x.Key).ToList();

            if (RENDERER_DEBUG)
            {
                ConfigManager.Log.Info(string.Format("Row groups starting here: {0}", string.Join(", ", rowGroupsStartingHere)));
                ConfigManager.Log.Info(string.Format("Column groups starting here: {0}", string.Join(", ", columnGroupsStartingHere)));

                ConfigManager.Log.Info(string.Format("Group ancestor lists: {0}", string.Join(", ", _groupAncestorLists.Select(x => string.Format("[{0} => {1}]", x.Key, string.Join(", ", x.Value))))));
            }

            //var rowGroupsStartingHere = _rowGroupRdlSpans.Where(x => x.Value.Item1 <= _tablixRow && x.Value.Item2 >= _tablixRow).Select(x => x.Key).ToList();
            //var columnGroupsStartingHere = _columnGroupRdlSpans.Where(x => x.Value.Item1 <= _tablixColumn && x.Value.Item2 >= _tablixColumn).Select(x => x.Key).ToList();

            // order from ancestor to descendant
            List<string> potentialRowGroups = new List<string>();
            List<string> potentialColumnGroups = new List<string>();

            if (rowGroupsStartingHere.Count > 0)
            {
                if (_currentRowGroup == null)
                {
                    potentialRowGroups = rowGroupsStartingHere
                        .OrderBy(x => _groupAncestorLists[x].Count).ToList();
                }
                else
                {
                    potentialRowGroups = rowGroupsStartingHere
                        .Where(x => x != _currentRowGroup)
                        .Where(x => _groupAncestorLists[x].Contains(_currentRowGroup))
                        .OrderBy(x => _groupAncestorLists[x].Count).ToList();
                }
            }
            if (columnGroupsStartingHere.Count > 0)
            {
                if (_currentColumnGroup == null)
                {
                    potentialColumnGroups = columnGroupsStartingHere
                        .OrderBy(x => _groupAncestorLists[x].Count).ToList();
                }
                else
                {
                    potentialColumnGroups = columnGroupsStartingHere
                        .Where(x => x != _currentColumnGroup)
                        //.OrderBy(x => _groupAncestorLists[x].Count)
                        .Where(x => _groupAncestorLists[x].Contains(_currentColumnGroup))
                        .OrderBy(x => _groupAncestorLists[x].Count).ToList();
                }
            }

            string topRowGroup = potentialRowGroups.FirstOrDefault();
            string topColumnGroup = potentialColumnGroups.FirstOrDefault();

            if (RENDERER_DEBUG)
            {
                ConfigManager.Log.Info(string.Format("Top row group: {0}", topRowGroup));
                ConfigManager.Log.Info(string.Format("Top column group: {0}", topColumnGroup));

                ConfigManager.Log.Info(string.Format("Group element names: {0}", string.Join(", ", _groupElementNames.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
            }

            var rowElementName = (topRowGroup == null ? null : _groupElementNames[topRowGroup]);
            var columnElementName = (topColumnGroup == null ? null : _groupElementNames[topColumnGroup]);

            var rowGroupElements = currentElement.GetElementsByTagName(rowElementName + "_Collection");
            // takes all descending elements - so it can skip the row group and go to nested column group
            var columnGroupElements = currentElement.GetElementsByTagName(columnElementName + "_Collection");

            if (RENDERER_DEBUG)
            {
                ConfigManager.Log.Info(string.Format("Row element name: {0}, column element name: {1}", rowElementName, columnElementName));
            }

            if (rowGroupElements.Count > 0 && topRowGroup != null)
            {
                var rowGroupElement = (XmlElement)rowGroupElements[0];
                var oldRowGroup = _currentRowGroup;
                _currentRowGroup = topRowGroup;
                FillRowGroup(rowGroupElement);

                _currentRowGroup = oldRowGroup;
                return true;
            }
            else if (columnGroupElements.Count > 0 && topColumnGroup != null)
            {
                var columnGroupElement = (XmlElement)columnGroupElements[0];

                // multiple rows, the column group needs to choose the right element
                if (columnGroupElements.Count > 1)
                {
                    bool breakOut = false;
                    foreach (XmlElement potentialGroupElement in columnGroupElements)
                    {
                        var parentElement = potentialGroupElement.ParentNode as XmlElement;
                        while (parentElement != currentElement && parentElement != null)
                        {
                            if (_textBoxDesignPositions.ContainsKey(parentElement.LocalName))
                            {
                                var rowPos = _textBoxDesignPositions[parentElement.LocalName].Item1;
                                if (rowPos == _tablixRow)
                                {
                                    columnGroupElement = potentialGroupElement;
                                    breakOut = true;
                                    break;
                                }
                            }

                            parentElement = parentElement.ParentNode as XmlElement;
                        }
                        if (breakOut)
                        {
                            break;
                        }
                    }
                }

                var oldColumnGroup = _currentColumnGroup;
                _currentColumnGroup = topColumnGroup;
                FillColumnGroup(columnGroupElement);

                _currentColumnGroup = oldColumnGroup;
                return true;
            }

            return false;

        }


        private void FillRowGroup(XmlElement rowGroupElement)
        {
            var groupItems = rowGroupElement.GetElementsByTagName(_groupElementNames[_currentRowGroup]);
            var groupStartTextBoxRow = _tablixRow;
            var groupStartTextBoxColumn = _tablixColumn;
            bool first = true;

            var originalGroupColumn = _valuesTableColumn;

            foreach (XmlElement groupItem in groupItems)
            {
                if (!first)
                {
                    // return to group start in RDL
                    _tablixRow = groupStartTextBoxRow;
                    _tablixColumn = groupStartTextBoxColumn;

                    // CR;LF
                    _valuesTableRow++;
                    _valuesTableColumn = originalGroupColumn;
                }

                ReportElementAbsolutePosition previousTextBox = GetCurrentTextBox();
                FillCurrentTextBox(groupItem);

                while (MoveToNextTextBox())
                {
                    var tb = GetCurrentTextBox();
                    StandardShiftInValuesTable(previousTextBox, tb);
                    previousTextBox = tb;
                    FillCurrentTextBox(groupItem);
                }

                first = false;
            }
        }

        private void FillColumnGroup(XmlElement columnGroupElement)
        {
            var groupItems = columnGroupElement.GetElementsByTagName(_groupElementNames[_currentColumnGroup]);
            var groupStartTextBoxRow = _tablixRow;
            var groupStartTextBoxColumn = _tablixColumn;

            bool firstGroupItem = true;
            int _groupItemStartColumn = _valuesTableColumn;

            foreach (XmlElement groupItem in groupItems)
            {



                _tablixRow = groupStartTextBoxRow;
                _tablixColumn = groupStartTextBoxColumn;

                _groupItemStartColumn = _valuesTableColumn;

                var tb = GetCurrentTextBox();

                // starting next group item - right to the previous one
                if (!firstGroupItem)
                {
                    _valuesTableColumn +=
                     //_columnGroupRdlSpans[_currentColumnGroup].Item2 - _columnGroupRdlSpans[_currentColumnGroup].Item1 + 1; 
                     _textBoxColumnSpans[tb.Name];
                }


                //ExpandColumnGroups(_valuesTableColumn);
                //StandardShiftInValuesTable(previousTextBox, tb);

                FillCurrentTextBox(groupItem);
                var previousTextBox = tb;

                while (MoveToNextTextBox())
                {
                    tb = GetCurrentTextBox();
                    // don't expand if only moved down

                    // don't expand at all?
                    //if (previousTextBox.Left < tb.Left)
                    //{
                    //    ExpandColumnGroups(_valuesTableColumn);
                    //}

                    StandardShiftInValuesTable(previousTextBox, tb);

                    FillCurrentTextBox(groupItem);
                    previousTextBox = tb;
                }

                firstGroupItem = false;
            }
        }

        private void WriteCellValuePlain(string value)
        {
            ReportTextBoxValue tbValue = new ReportTextBoxValue()
            {
                RdlPosition = GetCurrentTextBox(),
                Value = value,
                X = _valuesTableRow,
                Y = _valuesTableColumn
            };

            // start of row

            try
            {

                while (_values.Count < _valuesTableRow + 1)
                {
                    _values.Add(new List<ReportTextBoxValue>());
                }

                while (_values[_valuesTableRow].Count < _valuesTableColumn + 1)
                {
                    _values[_valuesTableRow].Add(null);
                }

                _values[_valuesTableRow][_valuesTableColumn] = tbValue;
            }
            catch (Exception ex)
            {
                ConfigManager.Log.Important(string.Format("Failed to write cell value {0} to [{1}][{2}]", value, _valuesTableRow, _valuesTableColumn));
                ConfigManager.Log.Important(string.Format("Row count: {0}", _values.Count));
                ConfigManager.Log.Important(string.Format("Column count: {0}", _values[_valuesTableRow].Count));
            }
            //if (columnGroupName != null)
            //{
            //    // first column in group
            //    if (!_columnGroupSpans.ContainsKey(columnGroupName))
            //    {
            //        _columnGroupSpans.Add(columnGroupName, new Tuple<int, int>(_valuesTableColumn, _valuesTableColumn));
            //    }
            //    else
            //    {
            //        if(_values.Count > 1)
            //        { 
            //        if(_values.GetRange(0, _values.Count -1).any)
            //    }
            //}

            //_valuesTableColumn++;
        }

        /// <summary>
        /// Insert at position - shift all, starting by the current column
        /// </summary>
        /// <param name="horizontalPosition"></param>
        private void ExpandColumnGroups(int horizontalPosition)
        {
            /**/
            List<string> groupsToExpand = new List<string>();
            foreach (var groupSpan in _columnGroupSpans)
            {
                if (groupSpan.Value.Item1 <= horizontalPosition && groupSpan.Value.Item2 >= horizontalPosition)
                {
                    groupsToExpand.Add(groupSpan.Key);
                }
            }

            foreach (var groupToExpand in groupsToExpand)
            {
                _columnGroupSpans[groupToExpand] = new Tuple<int, int>(
                    _columnGroupSpans[groupToExpand].Item1, _columnGroupSpans[groupToExpand].Item2 + 1);
            }

            foreach (var row in _values)
            {
                if (row.Count >= horizontalPosition + 1)
                {
                    // shift all from the horizontal position right
                    row.Add(null);
                    for (int i = row.Count - 1; i > horizontalPosition && i > 0; i--)
                    {
                        row[i] = row[i - 1];
                    }
                }
            }
            /**/
        }



        /// <summary>
        /// When column / row group active, moves only withing the group
        /// </summary>
        /// <returns></returns>
        private bool MoveToNextTextBox()
        {
            var origTablixColumn = _tablixColumn;
            var origTablixRow = _tablixRow;

            if (_tablixRow > _tablixPosition.Children.Count - 1)
            {
                return false;
            }

            var row = _tablixPosition.Children[_tablixRow];
            _tablixColumn++;
            while (true)
            {
                // end of row
                if ((_currentColumnGroup == null && _tablixColumn > row.Children.Count - 1)
                    || (_currentColumnGroup != null && (_tablixColumn > _columnGroupRdlSpans[_currentColumnGroup].Item2 || _tablixColumn > row.Children.Count - 1)))
                {
                    // cannot move tho next row within column group
                    if (_currentColumnGroup != null)
                    {
                        _tablixRow = origTablixRow;
                        _tablixColumn = origTablixColumn;
                        return false;
                    }
                    _tablixRow++;
                    _tablixColumn = 0;
                    /*
                    if (_currentColumnGroup == null)
                    {
                        _tablixColumn = 0;
                    }
                    else
                    {
                        _tablixColumn = _columnGroupRdlSpans[_currentColumnGroup].Item1;
                    }
                    */
                    if ((_currentRowGroup == null && _tablixRow > _tablixPosition.Children.Count - 1)
                        || (_currentRowGroup != null && (_tablixRow > _rowGroupRdlSpans[_currentRowGroup].Item2 || _tablixRow > _tablixPosition.Children.Count - 1)))
                    {
                        _tablixRow = origTablixRow;
                        _tablixColumn = origTablixColumn;
                        return false;
                    }
                    row = _tablixPosition.Children[_tablixRow];
                    continue;
                }

                var cell = row.Children[_tablixColumn];

                // skip empty cells
                if (cell.Children.Count == 0)
                {
                    _tablixColumn++;
                    continue;
                }
                /**/
                // skip cells containg something else than a textbox
                if (cell.Children[0].Type != "TextBoxElement")
                {
                    _tablixColumn++;
                    continue;
                }
                /**/
                return true;
            }
        }

        private ReportElementAbsolutePosition GetCurrentTextBox()
        {
            bool firstInRow, lastInRow, lastInRowGroup, lastInColumnGroup;
            return GetCurrentTextBox(out firstInRow, out lastInRow, out lastInRowGroup, out lastInColumnGroup);
        }

        private ReportElementAbsolutePosition GetCurrentTextBox(
            out bool firstInRow, out bool lastInRow, out bool lastInRowGroup, out bool lastInColumnGroup)
        {
            lastInRowGroup = false;
            lastInColumnGroup = false;

            var row = _tablixPosition.Children[_tablixRow];
            var cell = row.Children[_tablixColumn];
            var tb = cell.Children[0];
            firstInRow = _tablixColumn == 0;
            lastInRow = _tablixColumn == row.Children.Count - 1;

            if (_currentRowGroup != null)
            {
                var rowGroupSpan = _rowGroupRdlSpans[_currentRowGroup];
                // the bottom edge of the group is the Y-axis of the current TB
                lastInRowGroup = rowGroupSpan.Item2 == _textBoxDesignPositions[tb.Name].Item2;
            }
            if (_currentColumnGroup != null)
            {
                var colGroupSpan = _columnGroupRdlSpans[_currentColumnGroup];
                // the right edge of the group is the X-axis of the current TB
                lastInColumnGroup = colGroupSpan.Item2 == _textBoxDesignPositions[tb.Name].Item1;
            }
            return tb;
        }

        private List<string> CollectGroupTextBoxes(TablixGroupHierarchyMember hierarchyMember)
        {
            List<string> res = new List<string>();
            if (hierarchyMember.HeaderTextBox != null)
            {
                res.Add(hierarchyMember.HeaderTextBox);
            }
            var subs = hierarchyMember.ChildMembers.SelectMany(x => CollectGroupTextBoxes(x)).ToList();
            res.AddRange(subs);
            return res;
        }

        private List<Tuple<string, List<string>>> CollectGroupsAndTextBoxes(List<TablixGroupHierarchyMember> members)
        {
            var groups = members.Where(x => x.GroupName != null)
                .Select(x => new Tuple<string, List<string>>(x.GroupName, CollectGroupTextBoxes(x)))
                .ToList();
            var subGroups = members.SelectMany(x => CollectGroupsAndTextBoxes(x.ChildMembers));
            return groups.Union(subGroups).ToList();
        }

        private XmlElement FindDescendantElement(XmlElement root, string elementName)
        {
            if (root.LocalName == elementName)
            {
                return root;
            }
            foreach (var child in root.ChildNodes)
            {
                if (child is XmlElement)
                {
                    var childRes = FindDescendantElement((XmlElement)child, elementName);
                    if (childRes != null)
                    {
                        return childRes;
                    }
                }
            }

            return null;
        }

        private List<string> DistinctAttributeNames(XmlElement element)
        {
            List<string> res = new List<string>();

            foreach (XmlAttribute attr in element.Attributes)
            {
                res.Add(attr.LocalName);
            }

            foreach (var childElement in element.ChildNodes)
            {
                var childElementTyped = childElement as XmlElement;
                if (childElementTyped == null)
                {
                    continue;
                }

                res.AddRange(DistinctAttributeNames(childElementTyped));
            }

            var distinct = res.Distinct().ToList();
            return distinct;
        }
    }
}
