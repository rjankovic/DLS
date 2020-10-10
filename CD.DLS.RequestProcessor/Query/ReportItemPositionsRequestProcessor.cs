using CD.DLS.API;
using CD.DLS.API.Query;
using CD.DLS.API.Structures;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Ssrs;
using CD.DLS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.Query
{
    public class ReportItemPositionsRequestProcessor : RequestProcessorBase, IRequestProcessor<ReportItemPositionsRequest, ReportItemPositionsResponse>
    {
        public ReportItemPositionsResponse Process(ReportItemPositionsRequest request, ProjectConfig projectConfig)
        {
            try
            {
                int reportElementId = request.ReportElementId;

                var rootElementPosition = ExtractReportPortItemPositions(projectConfig, reportElementId);

                var requestResult = new ReportItemPositionsResponse()
                {
                    RootElement = rootElementPosition
                };
                return requestResult;
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw;
            }
        }


        public ReportElementAbsolutePosition ExtractReportPortItemPositions(ProjectConfig projectConfig, int reportElementId)
        {
            //int reportElementId = request.ReportElementId;
            //var element = GraphManager.GetModelElementById(reportElementId);
            BIDocModelStored modelStored = new BIDocModelStored(projectConfig.ProjectConfigId, reportElementId, BIDocModelStored.LoadMethodEnum.SecondLevelAncestor, GraphManager);

            FromBIDocModelConverter converterFrom = new FromBIDocModelConverter(modelStored);
            IReflectionHelper reflection = new JsonReflectionHelper(new Model.Mssql.ModelActivator());
            Model.Mssql.ModelConverter converterTo = new Model.Mssql.ModelConverter(reflection);
            MssqlModelElement convertedModel = converterFrom.Convert(converterTo, reportElementId);

            var reportElement = (ReportElement)(convertedModel);
            //}

            var rootElementPosition = GetReportItemPositions(reportElement);
            return rootElementPosition;
        }

        public ReportElementAbsolutePosition GetReportItemPositions(ReportElement reportElement)
        {
            ConfigManager.Log.Important(string.Format("Getting report item positions of {0}", reportElement));

            ReportElementAbsolutePosition positionWrap = new ReportElementAbsolutePosition()
            {
                Left = 0,
                Top = 0,
                Width = 0,
                Height = 0,
                RefPath = reportElement.RefPath.Path,
                IsReportItem = false,
                Name = reportElement.Caption,
                ModelElementId = reportElement.Id,
                Type = reportElement.GetType().Name
            };
            positionWrap.Children = new List<ReportElementAbsolutePosition>();

            foreach (var section in reportElement.Sections)
            {
                var sectionPosition = GetReportItemPosition(0, positionWrap.Height, section);
                positionWrap.Width = Math.Max(positionWrap.Width, sectionPosition.Width);
                positionWrap.Height = positionWrap.Height + sectionPosition.Height;
                positionWrap.Children.Add(sectionPosition);
            }
            if (reportElement.Sections.Count() == 0)
            {
                if (reportElement.SectionBody != null)
                {
                    var sectionBody = reportElement.SectionBody;
                    var sectionPosition = GetReportItemPosition(0, positionWrap.Height, sectionBody);
                    positionWrap.Width = Math.Max(positionWrap.Width, sectionPosition.Width);
                    positionWrap.Height = positionWrap.Height + sectionPosition.Height;
                    positionWrap.Children.Add(sectionPosition);
                }
            }

            MaximizeTablixWidths(positionWrap);

            return positionWrap;
        }

        /**/
        private void MaximizeTablixWidths(ReportElementAbsolutePosition position)
        {
            if (//position.Type.Contains("TablixElement") 
                //|| 
                position.Type.Contains("TextBoxElement") 
                || position.Type.Contains("CellElement"))
            {
                if (position.Children.Count == 1)
                {
                    if (position.Width > 0)
                    {
                        position.Children[0].Width = position.Width;
                    }
                    if (position.Height > 0)
                    {
                        position.Children[0].Height = position.Height;
                    }
                }
            }
            foreach (var child in position.Children)
            {
                MaximizeTablixWidths(child);
            }
        }
        /**/

        private ReportElementAbsolutePosition GetReportItemPosition(double offsetLeft, double offsetTop, ReportDesignElement reportDesignElement, bool firstChild = false)
        {
            //if (reportDesignElement.RefPath.Path == "SSRSServer[@Name='DWH-SERVER']/Folder[@Name='/']/Folder[@Name='/Reports']/Report[@Name='12 - Perm ROI by Consultant Seniority']/ReportSection[@Name='No_1']/Body/Rectangle[@Name='Rectangle6']/Tablix[@Name='Tablix1']/TablixRow[@Name='No_3']/Cell[@Name='No_2']/TextBox[@Name='Textbox55']")
            //{
            //    ConfigManager.Log.Info(string.Format("Positioning {0}: left {1}, top {2}", reportDesignElement.RefPath.Path, offsetLeft, offsetTop));
            //}
            offsetLeft = Math.Round(offsetLeft, 3);
            offsetTop = Math.Round(offsetTop, 3);
            reportDesignElement.Position.Left = Math.Round(reportDesignElement.Position.Left, 3);
            reportDesignElement.Position.Top = Math.Round(reportDesignElement.Position.Top, 3);
            reportDesignElement.Position.Width = Math.Round(reportDesignElement.Position.Width, 3);
            reportDesignElement.Position.Height = Math.Round(reportDesignElement.Position.Height, 3);

            ReportElementAbsolutePosition position = new ReportElementAbsolutePosition()
            {
                Left = offsetLeft + reportDesignElement.Position.Left,
                Top = offsetTop + reportDesignElement.Position.Top,
                Width = reportDesignElement.Position.Width,
                Height = reportDesignElement.Position.Height,
                RefPath = reportDesignElement.RefPath.Path,
                IsReportItem = reportDesignElement is ReportItemElement,
                Name = reportDesignElement.Caption,
                ModelElementId = reportDesignElement.Id,
                Type = reportDesignElement.GetType().Name,
                Hidden = false
            };

            if (reportDesignElement is TablixRowElement)
            {
                position.Hidden = ((TablixRowElement)reportDesignElement).Hidden;
            }

            if (reportDesignElement is CellElement && firstChild)
            {
                //position.Width = 0;
                //if (firstChild)
                //{

                    //var parentDesignElement = reportDesignElement.Parent as ReportDesignElement;
                    //if (parentDesignElement != null)
                    //{
                    //    position.Left = offsetLeft;
                    //}

                //}
            }
            position.Children = new List<ReportElementAbsolutePosition>();

            //if (reportDesignElement is TablixElement)
            //{
            //    position.Width = reportDesignElement.Position.Width;
            //    position.Height = reportDesignElement.Position.Height;
            //    position.Left = reportDesignElement.Position.Left;
            //    position.Top = reportDesignElement.Position.Top;
            //}


            // position children in one row left to right or in a column top to bottom?
            bool horizontalStack = reportDesignElement is TablixRowElement;

            if (horizontalStack)
            {
                position.Width = 0;
            }

            ReportElementAbsolutePosition prevHorizontalChildPosition = null;

            bool first = true;

            //ConfigManager.Log.Info("Design item: " + reportDesignElement.Caption);
            foreach (var designChild in reportDesignElement.Children.OfType<ReportDesignElement>())
            {
                //ConfigManager.Log.Info("Design item child: " + designChild.Caption);
                if (horizontalStack)
                {
                    var offsetLeft1 = position.Left + position.Width;

                    /*
                    if (position.Type == "TablixRowElement" && first)
                    {
                        ConfigManager.Log.Important(string.Format("First row {0} cell: {1}, offset {2}, parent offset {3}",
                            position.RefPath, designChild.Caption, offsetLeft, position.Left));
                        offsetLeft1 = offsetLeft;
                    }
                    */
                    var childPosition = GetReportItemPosition(offsetLeft1, position.Top, designChild, first);
                    first = false;
                    if (reportDesignElement.Position.Width == 0)
                    {
                        position.Width = childPosition.Width + childPosition.Width;
                    }
                    if (prevHorizontalChildPosition != null)
                    {
                        var prevChildRight = prevHorizontalChildPosition.Left + prevHorizontalChildPosition.Width;
                        if (childPosition.Left > prevChildRight)
                        {
                            //if (prevHorizontalChildPosition.Children.Any(x => x.Left >= prevChildRight))
                            //{
                            //    var rightExpansion = childPosition.Left - prevChildRight;
                            //    prevHorizontalChildPosition.Width = prevHorizontalChildPosition.Width + rightExpansion;
                            //}
                            //else
                            //{
                                //var leftExpansion = childPosition.Left - prevChildRight;
                                //childPosition.Width = childPosition.Width + leftExpansion;
                                //childPosition.Left = childPosition.Left - leftExpansion;

                                //var currentChild = childPosition;

                                //while (currentChild.Children.Count == 1)
                                //{
                                //    currentChild = currentChild.Children.First();
                                //    currentChild.Width = currentChild.Width + leftExpansion;
                                //    currentChild.Left = currentChild.Left - leftExpansion;
                                //}
                            //}
                        }
                    }
                    prevHorizontalChildPosition = childPosition;
                    //if (reportDesignElement.Position.Height == 0)
                    //{
                    //    position.Height = Math.Max(position.Height, childPosition.Height);
                    //}
                    position.Children.Add(childPosition);
                }
                else
                {
                    if (designChild is TablixRowElement)
                    {
                        if (((TablixRowElement)designChild).Hidden)
                        {
                            continue;
                        }
                    }
                    var childPosition = GetReportItemPosition(position.Left, position.Top, designChild);
                    //if (reportDesignElement.Position.Width == 0 && !(reportDesignElement is ReportSectionElement) && !(reportDesignElement is ReportSectionBodyElement))
                    //{
                    //    position.Width = Math.Max(position.Width, childPosition.Width);
                    //}
                    //if (reportDesignElement.Position.Height == 0 && !(reportDesignElement is ReportSectionElement) && !(reportDesignElement is ReportSectionBodyElement))
                    //{
                    //    position.Height = position.Height + childPosition.Height;
                    //}
                    position.Children.Add(childPosition);
                }
            }

            var textBox = reportDesignElement as TextBoxElement;
            if (textBox != null)
            {
                string text = null;
                string posExpression = null;
                foreach (var expression in textBox.Children.OfType<SsrsExpressionElement>())
                {
                    if (expression.Definition == null)
                    {
                        continue;
                    }
                    if (text == null)
                    {
                        text = expression.Definition;
                    }
                    else
                    {
                        text = text + " " + expression.Definition;
                    }
                    if (expression.Definition.StartsWith("="))
                    {
                        //if (text != null)
                        //{
                        //    text = text + " ... ";
                        //}
                        if (posExpression == null)
                        {
                            posExpression = expression.Definition;
                        }
                        continue;
                    }
                    if (text == null)
                    {
                        text = string.Empty;
                    }
                    //text = text + " " + expression.Definition;
                }

                position.Text = text;
                position.Expression = posExpression;
                if (position.Expression == null && position.Text == null)
                {
                    position.Text = string.Empty;
                }
            }
            //else if (reportDesignElement is TablixElement)
            //{
            //    position.Text = reportDesignElement.Caption;
            //}

            if (reportDesignElement is TablixElement)
            {
                var tablix = reportDesignElement as TablixElement;
                position.RowHierarchy = new TablixGroupHierarchy();
                position.ColumnHierarchy = new TablixGroupHierarchy();
                position.RowHierarchy.Members = new List<TablixGroupHierarchyMember>();
                position.ColumnHierarchy.Members = new List<TablixGroupHierarchyMember>();
                foreach (HierarchyMemberElement rowMember in tablix.RowHierarchy.Members)
                {
                    position.RowHierarchy.Members.Add(ExtractTablixGroupHierarchyMember(rowMember));
                }
                foreach (HierarchyMemberElement columnMember in tablix.ColumnHierarchy.Members)
                {
                    position.ColumnHierarchy.Members.Add(ExtractTablixGroupHierarchyMember(columnMember));
                }
            }

            return position;
        }

        private TablixGroupHierarchyMember ExtractTablixGroupHierarchyMember(HierarchyMemberElement memberElement)
        {
            var member = new TablixGroupHierarchyMember();
            member.ChildMembers = new List<TablixGroupHierarchyMember>();
            if (memberElement.HeaderTextBox != null)
            {
                member.HeaderTextBox = memberElement.HeaderTextBox.Caption;
            }
            if (memberElement.Group == null)
            {
                foreach (var childMember in memberElement.Members)
                {
                    member.ChildMembers.Add(ExtractTablixGroupHierarchyMember(childMember));
                }
            }
            else
            {
                member.GroupName = memberElement.Group.Caption;
                member.DataElementName = memberElement.Group.Caption;
                if (!string.IsNullOrWhiteSpace(memberElement.Group.DataElementName))
                {
                    member.DataElementName = memberElement.Group.DataElementName;
                }
                foreach (var groupMember in memberElement.Members)
                {
                    member.ChildMembers.Add(ExtractTablixGroupHierarchyMember(groupMember));
                }
            }

            if (member.HeaderTextBox == null && member.ChildMembers.Count == 1)
            {
                member.HeaderTextBox = member.ChildMembers[0].HeaderTextBox;
            }

            return member;
        }
    }
}
