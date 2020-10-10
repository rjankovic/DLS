using CD.DLS.Serialization;
using CD.DLS.Parse.Mssql;
using CD.DLS.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Ssrs;
using CD.DLS.API;
using CD.DLS.API.Structures;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.DAL.Managers;

namespace CD.DLS.Operations
{
    class GetReportItemPositionsRequestProcessor : BIDocRequestProcessor<GetReportItemPositionsRequest>
    {
        public GetReportItemPositionsRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(GetReportItemPositionsRequest request, ProjectConfig projectConfig)
        {
            //throw new NotImplementedException();

            //int reportElementId

            //string reportElementUrn = MssqlModelExtractor.GetReportElementUrnPath(request.ReportPath, new ModelSettings() { Config = projectConfig, Log = _core.Log } /* new ExtractSettingsProvider(projectConfig, _core.Log)*/);
            //ReportElement reportElement = null;

            //_core.Log.Important("Converting to DB format");
            ////using (var dbContext = new CDFrameworkContext())
            ////{
            //int reportElementId = GraphManager.GetModelElementIdByRefPath(projectConfig.ProjectConfigId, reportElementUrn); // dbContext.ModelElements.First(x => x.RefPath == reportElementUrn).Id;
            
            int reportElementId = request.ReportElementId;
            
            var rootElementPosition = ExtractReportPortItemPositions(projectConfig, reportElementId);

            var requestResult = new GetReportItemPositionsRequestResponse()
            {
                RootElement = rootElementPosition
            };

            var stringResult = requestResult.Serialize();
            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = new List<Attachment>()
            };
        }

        public static ReportElementAbsolutePosition ExtractReportPortItemPositions(ProjectConfig projectConfig, int reportElementId)
        {
            //int reportElementId = request.ReportElementId;
            var element = GraphManager.GetModelElementById(reportElementId);
            BIDocModelStored modelStored = new BIDocModelStored(projectConfig.ProjectConfigId, element.RefPath);

            FromBIDocModelConverter converterFrom = new FromBIDocModelConverter(modelStored);
            IReflectionHelper reflection = new JsonReflectionHelper(new Model.Mssql.ModelActivator());
            Model.Mssql.ModelConverter converterTo = new Model.Mssql.ModelConverter(reflection);
            MssqlModelElement convertedModel = converterFrom.Convert(converterTo, reportElementId);

            var reportElement = (ReportElement)(convertedModel);
            //}

            var rootElementPosition = GetReportItemPositions(reportElement);
            return rootElementPosition;
        }

        public static ReportElementAbsolutePosition GetReportItemPositions(ReportElement reportElement)
        {
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
            return positionWrap;
        }

        private static ReportElementAbsolutePosition GetReportItemPosition(double offsetLeft, double offsetTop, ReportDesignElement reportDesignElement)
        {
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
                Type = reportDesignElement.GetType().Name
            };
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

            foreach (var designChild in reportDesignElement.Children.OfType<ReportDesignElement>())
            {
                if (horizontalStack)
                {
                    var childPosition = GetReportItemPosition(position.Left + position.Width, position.Top, designChild);
                    if (reportDesignElement.Position.Width == 0)
                    {
                        position.Width = childPosition.Width + childPosition.Width;
                    }
                    //if (reportDesignElement.Position.Height == 0)
                    //{
                    //    position.Height = Math.Max(position.Height, childPosition.Height);
                    //}
                    position.Children.Add(childPosition);
                }
                else
                {
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
            }
            //else if (reportDesignElement is TablixElement)
            //{
            //    position.Text = reportDesignElement.Caption;
            //}
            

            return position;
        }
    }
}
