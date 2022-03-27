using CD.DLS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Mssql.Ssas
{
    #region COMMON

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DaxFunctionName : Attribute
    {
        public string FunctionName { get; set; }
        public DaxFunctionName(string functionName)
        {
            FunctionName = functionName;
        }
    }

    public abstract class DaxFirstTableVectorFunctionElement : DaxTableFunctionElement
    {
        public DaxFirstTableVectorFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var table = arguments[0].FragmentElement;
            var outputColumn = AddOutputColumn(DaxArgumentColumn.DEFAULT_NAME);
            AddDataFlowLink(table, outputColumn);
        }
    }

    public abstract class DaxFirstArgumentScalarFunctionElement : DaxScalarFunctionElement
    {
        public DaxFirstArgumentScalarFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var value = arguments[0].FragmentElement;
            AddDataFlowLink(value);
        }
    }

    public abstract class DaxCartesianFunctionElement : DaxTableFunctionElement
    {
        public DaxCartesianFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            foreach (var argument in arguments.Arguments)
            {
                foreach (var column in argument.Columns)
                {
                    var outputColumn = AddOutputColumn(column.Name);
                    AddDataFlowLink(column.RefereneElement, outputColumn);
                }
            }
        }
    }

    #endregion

    #region DATE_AND_TIME

    [DaxFunctionName("CALENDAR")]
    public class DaxCalendarFunctionElement : DaxTableFunctionElement
    {
        public DaxCalendarFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var startDate = arguments[0].FragmentElement;
            var endDate = arguments[1].FragmentElement;
            var outputColumn = AddOutputColumn(DaxArgumentColumn.DEFAULT_NAME);
            AddDataFlowLink(startDate, outputColumn);
            AddDataFlowLink(endDate, outputColumn);
        }
    }

    [DaxFunctionName("CALENDARAUTO")]
    public class DaxCalendarAutoFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxCalendarAutoFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("DATE")]
    public class DaxDateFunctionElement : DaxScalarFunctionElement
    {
        public DaxDateFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var year = arguments[0].FragmentElement;
            var month = arguments[1].FragmentElement;
            var day = arguments[2].FragmentElement;
            AddDataFlowLink(year);
            AddDataFlowLink(month);
            AddDataFlowLink(day);
        }
    }

    [DaxFunctionName("DATEDIFF")]
    public class DaxDateDiffFunctionElement : DaxScalarFunctionElement
    {
        public DaxDateDiffFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var start = arguments[0].FragmentElement;
            var end = arguments[1].FragmentElement;
            AddDataFlowLink(start);
            AddDataFlowLink(end);
        }
    }

    [DaxFunctionName("DATEVALUE")]
    public class DaxDateValueFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxDateValueFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("DAY")]
    public class DaxDayFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxDayFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("EDATE")]
    public class DaxEDateFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxEDateFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("EOMONTH")]
    public class DaxEOMonthFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxEOMonthFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("HOUR")]
    public class DaxHourFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxHourFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("MINUTE")]
    public class DaxMinuteFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxMinuteFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }
    
    [DaxFunctionName("MONTH")]
    public class DaxMonthFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxMonthFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("NOW")]
    public class DaxNowFunctionElement : DaxScalarFunctionElement
    {
        public DaxNowFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("SECOND")]
    public class DaxSecondFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxSecondFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("TIME")]
    public class DaxTimeFunctionElement : DaxScalarFunctionElement
    {
        public DaxTimeFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var hour = arguments[0].FragmentElement;
            var minute = arguments[1].FragmentElement;
            var second = arguments[2].FragmentElement;
            AddDataFlowLink(hour);
            AddDataFlowLink(minute);
            AddDataFlowLink(second);
        }
    }

    [DaxFunctionName("TIMEVALUE")]
    public class DaxTimeValueFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxTimeValueFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("TODAY")]
    public class DaxTodayFunctionElement : DaxScalarFunctionElement
    {
        public DaxTodayFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("UTCNOW")]
    public class DaxUtcNowFunctionElement : DaxScalarFunctionElement
    {
        public DaxUtcNowFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("UTCTODAY")]
    public class DaxUtcTodayFunctionElement : DaxScalarFunctionElement
    {
        public DaxUtcTodayFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("WEEKDAY")]
    public class DaxWeekDayFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxWeekDayFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("WEEKNUM")]
    public class DaxWeekNumFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxWeekNumFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("YEAR")]
    public class DaxYearFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxYearFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("YEARFRAC")]
    public class DaxYearFracFunctionElement : DaxScalarFunctionElement
    {
        public DaxYearFracFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var start = arguments[0].FragmentElement;
            var end = arguments[1].FragmentElement;
            AddDataFlowLink(start);
            AddDataFlowLink(end);
        }
    }

    #endregion

    #region TIME_INTELLIGENCE

    [DaxFunctionName("CLOSINGBALANCEMONTH")]
    public class DaxClosingBalanceMonthFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxClosingBalanceMonthFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("CLOSINGBALANCEQUARTER")]
    public class DaxClosingBalanceQuarterFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxClosingBalanceQuarterFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("CLOSINGBALANCEYEAR")]
    public class DaxClosingBalanceYearFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxClosingBalanceYearFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("DATEADD")]
    public class DaxDateAddFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxDateAddFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("DATESBETWEEN")]
    public class DaxDatesBetweenFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxDatesBetweenFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("DATESINPERIOD")]
    public class DaxDatesInPeriodFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxDatesInPeriodFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("DATESMTD")]
    public class DaxDatesMtdFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxDatesMtdFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("DATESQTD")]
    public class DaxDatesQtdFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxDatesQtdFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("DATESYTD")]
    public class DaxDatesYtdFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxDatesYtdFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ENDOFMONTH")]
    public class DaxEndOfMonthFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxEndOfMonthFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }
    
    [DaxFunctionName("ENDOFQUARTER")]
    public class DaxEndOfQuarterFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxEndOfQuarterFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ENDOFYEAR")]
    public class DaxEndOfYearFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxEndOfYearFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("FIRSTDATE")]
    public class DaxFirstDateFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxFirstDateFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("FIRSTNONBLANK")]
    public class DaxFirstNonBlankFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxFirstNonBlankFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("LASTDATE")]
    public class DaxLastDateFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxLastDateFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("LASTNONBLANK")]
    public class DaxLastNonBlankFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxLastNonBlankFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("NEXTDAY")]
    public class DaxNextDayFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxNextDayFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("NEXTMONTH")]
    public class DaxNextMonthFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxNextMonthFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("NEXTQUARTER")]
    public class DaxNextQuarterFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxNextQuarterFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("NEXTYEAR")]
    public class DaxNextYearFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxNextYearFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("OPENINGBALANCEMONTH")]
    public class DaxOpeningBalanceMonthFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxOpeningBalanceMonthFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("OPENINGBALANCEQUARTER")]
    public class DaxOpeningBalanceQuarterFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxOpeningBalanceQuarterFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("OPENINGBALANCEYEAR")]
    public class DaxOpeningBalanceYearFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxOpeningBalanceYearFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("PARALLELPERIOD")]
    public class DaxParallelPeriodFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxParallelPeriodFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("PREVIOUSDAY")]
    public class DaxPreviousDayFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxPreviousDayFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("PREVIOUSMONTH")]
    public class DaxPreviousMonthFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxPreviousMonthFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("PREVIOUSQUARTER")]
    public class DaxPreviousQuarterFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxPreviousQuarterFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("PREVIOUSYEAR")]
    public class DaxPreviousYearFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxPreviousYearFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("SAMEPERIODLASTYEAR")]
    public class DaxSamePeriodLastYearFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxSamePeriodLastYearFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("STARTOFMONTH")]
    public class DaxStartOfMonthFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxStartOfMonthFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("STARTOFQUARTER")]
    public class DaxStartOfQuarterFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxStartOfQuarterFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("STARTOFYEAR")]
    public class DaxStartOfYearFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxStartOfYearFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("TOTALMTD")]
    public class DaxTotalMtdFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxTotalMtdFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("TOTALQTD")]
    public class DaxTotalQtdFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxTotalQtdFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("TOTALYTD")]
    public class DaxTotalYtdFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxTotalYtdFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }
    
    #endregion

    #region FILTER

    [DaxFunctionName("ADDMISSINGITEMS")]
    public class DaxAddMissingItemsFunctionElement : DaxTableFunctionElement
    {
        public DaxAddMissingItemsFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();

            var tableReference = arguments.Arguments.FirstOrDefault(
                x => x.FragmentElement is DaxTableReferenceElement
                || (x.FragmentElement is DaxTableOperationElement)
                || (x.FragmentElement is DaxExpressionEvaluationFunctionElement));

            if (tableReference != null)
            {
                foreach (var column in tableReference.Columns)
                {
                    var outputColumn = AddOutputColumn(column.Name);
                    AddDataFlowLink(column.RefereneElement, outputColumn);
                }

            }
        }
    }

    [DaxFunctionName("ALL")]
    public class DaxAllFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxAllFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ALLEXCEPT")]
    public class DaxAllExceptFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxAllExceptFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ALLNOBLANKROW")]
    public class DaxAllNoBlankRowFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxAllNoBlankRowFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ALLSELECTED")]
    public class DaxAllSelectedFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxAllSelectedFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }
    
    [DaxFunctionName("CALCULATE")]
    public class DaxCalculateFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxCalculateFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("CALCULATETABLE")]
    public class DaxCalculateTableFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxCalculateTableFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("CROSSFILTER")]
    public class DaxCrossFilterFunctionElement : DaxScalarFunctionElement
    {
        public DaxCrossFilterFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }
    
    [DaxFunctionName("DISTINCT")]
    public class DaxDistinctFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxDistinctFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("EARLIER")]
    public class DaxEarlierFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxEarlierFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("EARLIEST")]
    public class DaxEarliestFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxEarliestFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("FILTER")]
    public class DaxFilterFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxFilterFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("FILTERS")]
    public class DaxFiltersFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxFiltersFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("HASONEFILTER")]
    public class DaxHasOneFilterFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxHasOneFilterFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("HASONEVALUE")]
    public class DaxHasOneValueFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxHasOneValueFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ISCROSSFILTERED")]
    public class DaxIsCrossFilteredFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxIsCrossFilteredFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ISFILTERED")]
    public class DaxIsFilteredFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxIsFilteredFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("KEEPFILTERS")]
    public class DaxKeepFiltersFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxKeepFiltersFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("RELATED")]
    public class DaxRelatedFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxRelatedFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("RELATEDTABLE")]
    public class DaxRelatedTableFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxRelatedTableFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("SELECTEDVALUE")]
    public class DaxSelectedValueFunctionElement : DaxScalarFunctionElement
    {
        public DaxSelectedValueFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var defaultValue = arguments[0].FragmentElement;
            var errorValue = arguments[1].FragmentElement;
            AddDataFlowLink(defaultValue);
            AddDataFlowLink(errorValue);
        }
    }

    [DaxFunctionName("SUBSTITUTEWITHINDEX")]
    public class DaxSubstituteWithIndexFunctionElement : DaxTableFunctionElement
    {
        public DaxSubstituteWithIndexFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();

            var tableReference = arguments.Arguments[0];
            var columnNameArgument = arguments.Arguments[1];
            var secondTable = arguments.Arguments[2];

            var indexColumnName = columnNameArgument.FragmentElement.Definition.Trim('"');

            HashSet<string> secondTableColumns = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var column in secondTable.Columns)
            {
                secondTableColumns.Add(column.Name);
            }

            var indexOutputColumn = AddOutputColumn(indexColumnName);

            foreach (var column in tableReference.Columns)
            {
                if (secondTableColumns.Contains(column.Name))
                {
                    AddDataFlowLink(column.RefereneElement, indexOutputColumn);
                }
                else
                {
                    var outputColumn = AddOutputColumn(column.Name);
                    AddDataFlowLink(column.RefereneElement, outputColumn);
                }
            }

            foreach (var rightColumn in secondTable.Columns)
            {
                AddDataFlowLink(rightColumn.RefereneElement, indexOutputColumn);
            }
        }
    }

    [DaxFunctionName("USERELATIONSHIP")]
    public class DaxUseRelationshipFunctionElement : DaxScalarFunctionElement
    {
        public DaxUseRelationshipFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("VALUES")]
    public class DaxValuesFunctionElement : DaxFirstTableVectorFunctionElement
    {
        public DaxValuesFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    #endregion

    #region INFORMATION

    [DaxFunctionName("CONTAINS")]
    public class DaxContainsFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxContainsFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("CUSTOMDATA")]
    public class DaxCustomDataFunctionElement : DaxScalarFunctionElement
    {
        public DaxCustomDataFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ISBLANK")]
    public class DaxIsBlankFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxIsBlankFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ISERROR")]
    public class DaxIsErrorFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxIsErrorFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ISEVEN")]
    public class DaxIsEvenFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxIsEvenFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ISINSCOPE")]
    public class DaxIsInScopeFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxIsInScopeFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ISLOGICAL")]
    public class DaxIsLogicalFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxIsLogicalFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ISNONTEXT")]
    public class DaxIsNonTextFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxIsNonTextFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ISNUMBER")]
    public class DaxIsNumberFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxIsNumberFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ISODD")]
    public class DaxIsOddFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxIsOddFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ISONORAFTER")]
    public class DaxIsOnOrAfterFunctionElement : DaxScalarFunctionElement
    {
        public DaxIsOnOrAfterFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var defaultValue = arguments[0].FragmentElement;
            var errorValue = arguments[1].FragmentElement;
            AddDataFlowLink(defaultValue);
            AddDataFlowLink(errorValue);
        }
    }

    [DaxFunctionName("ISTEXT")]
    public class DaxIsTextFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxIsTextFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("LOOKUPVALUE")]
    public class DaxLookupValueFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxLookupValueFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("USERNAME")]
    public class DaxUserNameFunctionElement : DaxScalarFunctionElement
    {
        public DaxUserNameFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    #endregion

    #region

    [DaxFunctionName("AND")]
    public class DaxAndFunctionElement : DaxScalarFunctionElement
    {
        public DaxAndFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var defaultValue = arguments[0].FragmentElement;
            var errorValue = arguments[1].FragmentElement;
            AddDataFlowLink(defaultValue);
            AddDataFlowLink(errorValue);
        }
    }

    [DaxFunctionName("FALSE")]
    public class DaxFalseFunctionElement : DaxScalarFunctionElement
    {
        public DaxFalseFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("IF")]
    public class DaxIfFunctionElement : DaxScalarFunctionElement
    {
        public DaxIfFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var condition = arguments[0].FragmentElement;
            var trueValue = arguments[1].FragmentElement;
            AddDataFlowLink(condition);
            AddDataFlowLink(trueValue);
            if (arguments.Count > 2)
            {
                var falseValue = arguments[2].FragmentElement;
                AddDataFlowLink(falseValue);
            }
        }
    }

    [DaxFunctionName("IFERROR")]
    public class DaxIfErrorFunctionElement : DaxScalarFunctionElement
    {
        public DaxIfErrorFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();          
            var defaultValue = arguments[0].FragmentElement;
            var errorValue = arguments[1].FragmentElement;
            AddDataFlowLink(defaultValue);
            AddDataFlowLink(errorValue);
        }
    }

    [DaxFunctionName("NOT")]
    public class DaxNotFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxNotFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("OR")]
    public class DaxOrFunctionElement : DaxScalarFunctionElement
    {
        public DaxOrFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var defaultValue = arguments[0].FragmentElement;
            var errorValue = arguments[1].FragmentElement;
            AddDataFlowLink(defaultValue);
            AddDataFlowLink(errorValue);
        }
    }

    [DaxFunctionName("SWITCH")]
    public class DaxSwitchFunctionElement : DaxScalarFunctionElement
    {
        public DaxSwitchFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            foreach (var argument in arguments.Arguments)
            {
                AddDataFlowLink(argument.FragmentElement);
            }
        }
    }

    [DaxFunctionName("TRUE")]
    public class DaxTrueFunctionElement : DaxScalarFunctionElement
    {
        public DaxTrueFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    #endregion

    #region MATH_AND_TRIG
    
    // handled as general scalar functions (all arguments -> value)

    #endregion

    #region OTHER

    [DaxFunctionName("DATATABLE")]
    public class DaxDataTableFunctionElement : DaxTableFunctionElement
    {
        public DaxDataTableFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            List<DaxTableOperationOutputColumnElement> outputColumnList = new List<DaxTableOperationOutputColumnElement>();

            for (int i = 0; i < arguments.Count; i += 2)
            {
                var nameArguemnt = arguments.Arguments[i].FragmentElement as DaxLiteralElement;
                if (nameArguemnt == null)
                {
                    continue;
                }

                var outputColumnName = nameArguemnt.Definition;
                var outputColumn = AddOutputColumn(outputColumnName);
                outputColumnList.Add(outputColumn);
            }

            var lastArgument = arguments.Arguments.Last();

            int idx = 0;
            while (idx < lastArgument.Columns.Count && idx < outputColumnList.Count)
            {
                AddDataFlowLink(lastArgument.Columns[idx].RefereneElement, outputColumnList[idx]);
            }
        }
    }

    [DaxFunctionName("ERROR")]
    public class DaxErrorFunctionElement : DaxScalarFunctionElement
    {
        public DaxErrorFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("EXCEPT")]
    public class DaxExceptFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxExceptFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("GENERATESERIES")]
    public class DaxGenerateSeriesFunctionElement : DaxTableFunctionElement
    {
        public DaxGenerateSeriesFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var outputColumn = AddOutputColumn(DaxArgumentColumn.DEFAULT_NAME);

            var startValue = arguments[0].FragmentElement;
            var endValue = arguments[1].FragmentElement;
            AddDataFlowLink(startValue, outputColumn);
            AddDataFlowLink(endValue, outputColumn);
        }
    }

    [DaxFunctionName("GROUPBY")]
    public class DaxGroupByFunctionElement : DaxTableFunctionElement
    {
        public DaxGroupByFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();

            int argumentIndex = 1;

            // group by column part
            while (argumentIndex < arguments.Count)
            {
                var argument = arguments[argumentIndex];
                if (!(argument.FragmentElement is DaxColumnReferenceElement))
                {
                    break;
                }

                var columnReference = argument.FragmentElement as DaxColumnReferenceElement;
                var outputColumn = AddOutputColumn(columnReference.ColumnName);
                AddDataFlowLink(argument.FragmentElement, outputColumn);

                argumentIndex++;
            }
            

            // aggregation columns - couples [name, expression]
            while (argumentIndex < arguments.Count - 1)
            {
                var columnNameArgument = arguments[argumentIndex];
                if (!(columnNameArgument.FragmentElement is DaxLiteralElement))
                {
                    break;
                }

                var columnName = columnNameArgument.FragmentElement.Definition.Trim('"');
                var columnExpression = arguments[argumentIndex + 1];

                if (columnExpression.FragmentElement == null)
                {
                    break;
                }

                // output column is influenced by the expression and the filter function (not linking to the grouping columns)
                var outputColumn = AddOutputColumn(columnName);
                AddDataFlowLink(columnExpression.FragmentElement, outputColumn);

                argumentIndex += 2;
            }
        }
    }

    [DaxFunctionName("INTERSECT")]
    public class DaxIntersectFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxIntersectFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ISEMPTY")]
    public class DaxIsEmptyFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxIsEmptyFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("NATURALINNERJOIN")]
    public class DaxNaturalInnerJoinFunctionElement : DaxTableFunctionElement
    {
        public DaxNaturalInnerJoinFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();

            var tableReference = arguments.Arguments[0];
            var secondTable = arguments.Arguments[1];

            
            Dictionary<string, DaxTableOperationOutputColumnElement> firstTableColumns 
                = new Dictionary<string, DaxTableOperationOutputColumnElement>(StringComparer.InvariantCultureIgnoreCase);
            
            foreach (var column in tableReference.Columns)
            {
                    var outputColumn = AddOutputColumn(column.Name);
                    AddDataFlowLink(column.RefereneElement, outputColumn);
                firstTableColumns.Add(column.Name, outputColumn);
            }
            foreach (var rightColumn in secondTable.Columns)
            {
                if (firstTableColumns.ContainsKey(rightColumn.Name))
                {
                    AddDataFlowLink(rightColumn.RefereneElement, firstTableColumns[rightColumn.Name]);
                }
                else
                {

                    var outputColumn = AddOutputColumn(rightColumn.Name);
                    AddDataFlowLink(rightColumn.RefereneElement, outputColumn);
                }
            }
        }
    }

    [DaxFunctionName("NATURALLEFTOUTERJOIN")]
    public class DaxNaturalLeftOuterJoinFunctionElement : DaxTableFunctionElement
    {
        public DaxNaturalLeftOuterJoinFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();

            var tableReference = arguments.Arguments[0];
            var secondTable = arguments.Arguments[1];


            Dictionary<string, DaxTableOperationOutputColumnElement> firstTableColumns
                = new Dictionary<string, DaxTableOperationOutputColumnElement>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var column in tableReference.Columns)
            {
                var outputColumn = AddOutputColumn(column.Name);
                AddDataFlowLink(column.RefereneElement, outputColumn);
                firstTableColumns.Add(column.Name, outputColumn);
            }
            foreach (var rightColumn in secondTable.Columns)
            {
                if (firstTableColumns.ContainsKey(rightColumn.Name))
                {
                    AddDataFlowLink(rightColumn.RefereneElement, firstTableColumns[rightColumn.Name]);
                }
                else
                {

                    var outputColumn = AddOutputColumn(rightColumn.Name);
                    AddDataFlowLink(rightColumn.RefereneElement, outputColumn);
                }
            }
        }
    }

    [DaxFunctionName("SUMMARIZECOLUMNS")]
    public class DaxSummarizeColumnsFunctionElement : DaxTableFunctionElement
    {
        public DaxSummarizeColumnsFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();

            int argumentIndex = 0;

            // group by column part
            while (argumentIndex < arguments.Count)
            {
                var argument = arguments[argumentIndex];
                if (!(argument.FragmentElement is DaxColumnReferenceElement))
                {
                    break;
                }

                var columnReference = argument.FragmentElement as DaxColumnReferenceElement;
                var outputColumn = AddOutputColumn(columnReference.ColumnName);
                AddDataFlowLink(argument.FragmentElement, outputColumn);

                argumentIndex++;
            }

            DaxFragmentElement filterFunction = null;

            // filter table [optional]
            if (argumentIndex < arguments.Count)
            {
                var midArgument = arguments[argumentIndex];
                // not a literal && not column reference => filter function
                if (!(midArgument.FragmentElement is DaxLiteralElement))
                {
                    filterFunction = midArgument.FragmentElement;
                    argumentIndex++;
                }
            }

            // aggregation columns - couples [name, expression]
            while (argumentIndex < arguments.Count - 1)
            {
                var columnNameArgument = arguments[argumentIndex];
                if (!(columnNameArgument.FragmentElement is DaxLiteralElement))
                {
                    break;
                }

                var columnName = columnNameArgument.FragmentElement.Definition.Trim('"');
                var columnExpression = arguments[argumentIndex + 1];

                if (columnExpression.FragmentElement == null)
                {
                    break;
                }

                // output column is influenced by the expression and the filter function (not linking to the grouping columns)
                var outputColumn = AddOutputColumn(columnName);
                AddDataFlowLink(columnExpression.FragmentElement, outputColumn);
                if (filterFunction != null)
                {
                    AddDataFlowLink(filterFunction, outputColumn);
                }

                argumentIndex += 2;
            }
        }
    }

    [DaxFunctionName("TREATAS")]
    public class DaxTreatAsFunctionElement : DaxExpressionEvaluationFunctionElement
    {
        public DaxTreatAsFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("UNION")]
    public class DaxUnionFunctionElement : DaxTableFunctionElement
    {
        public DaxUnionFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();

            var tableReference = arguments.Arguments[0];
            var secondTable = arguments.Arguments[1];


            Dictionary<string, DaxTableOperationOutputColumnElement> firstTableColumns
                = new Dictionary<string, DaxTableOperationOutputColumnElement>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var column in tableReference.Columns)
            {
                var outputColumn = AddOutputColumn(column.Name);
                AddDataFlowLink(column.RefereneElement, outputColumn);
                firstTableColumns.Add(column.Name, outputColumn);
            }
            foreach (var rightColumn in secondTable.Columns)
            {
                if (firstTableColumns.ContainsKey(rightColumn.Name))
                {
                    AddDataFlowLink(rightColumn.RefereneElement, firstTableColumns[rightColumn.Name]);
                }
            }
        }
    }

    #endregion

    #region PARENT_CHILD

    // handled as general scalar functions (all arguments -> value)

    #endregion

    #region STATISTICAL

    [DaxFunctionName("ADDCOLUMNS")]
    public class DaxAddColumnsFunctionElement : DaxTableFunctionElement
    {
        public DaxAddColumnsFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();

            var tableReference = arguments.Arguments[0];
            

            Dictionary<string, DaxTableOperationOutputColumnElement> firstTableColumns
                = new Dictionary<string, DaxTableOperationOutputColumnElement>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var column in tableReference.Columns)
            {
                var outputColumn = AddOutputColumn(column.Name);
                AddDataFlowLink(column.RefereneElement, outputColumn);
            }

            var argumentIndex = 1;

            while (argumentIndex < arguments.Count - 1)
            {
                var columnNameArgument = arguments[argumentIndex];
                if (!(columnNameArgument.FragmentElement is DaxLiteralElement))
                {
                    break;
                }

                var columnName = columnNameArgument.FragmentElement.Definition.Trim('"');
                var columnExpression = arguments[argumentIndex + 1];

                if (columnExpression.FragmentElement == null)
                {
                    break;
                }
                
                var outputColumn = AddOutputColumn(columnName);
                AddDataFlowLink(columnExpression.FragmentElement, outputColumn);

                argumentIndex += 2;
            }
        }
    }

    [DaxFunctionName("CROSSJOIN")]
    public class DaxCrossJoinFunctionElement : DaxCartesianFunctionElement
    {
        public DaxCrossJoinFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("GENERATE")]
    public class DaxGenerateFunctionElement : DaxCartesianFunctionElement
    {
        public DaxGenerateFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("GENERATEALL")]
    public class DaxGenerateAllFunctionElement : DaxCartesianFunctionElement
    {
        public DaxGenerateAllFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("ROW")]
    public class DaxRowFunctionElement : DaxTableFunctionElement
    {
        public DaxRowFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var argumentIndex = 0;

            while (argumentIndex < arguments.Count - 1)
            {
                var columnNameArgument = arguments[argumentIndex];
                if (!(columnNameArgument.FragmentElement is DaxLiteralElement))
                {
                    break;
                }

                var columnName = columnNameArgument.FragmentElement.Definition.Trim('"');
                var columnExpression = arguments[argumentIndex + 1];

                if (columnExpression.FragmentElement == null)
                {
                    break;
                }

                var outputColumn = AddOutputColumn(columnName);
                AddDataFlowLink(columnExpression.FragmentElement, outputColumn);

                argumentIndex += 2;
            }
        }
    }

    [DaxFunctionName("SAMPLE")]
    public class DaxSampleFunctionElement : DaxTableFunctionElement
    {
        public DaxSampleFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            var tableReference = arguments.Arguments[1];
            
            foreach (var column in tableReference.Columns)
            {
                var outputColumn = AddOutputColumn(column.Name);
                AddDataFlowLink(column.RefereneElement, outputColumn);
            }
        }
    }

    [DaxFunctionName("SELECTCOLUMNS")]
    public class DaxSelectColumnsFunctionElement : DaxTableFunctionElement
    {
        public DaxSelectColumnsFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            
            var argumentIndex = 1;

            while (argumentIndex < arguments.Count - 1)
            {
                var columnNameArgument = arguments[argumentIndex];
                if (!(columnNameArgument.FragmentElement is DaxLiteralElement))
                {
                    break;
                }

                var columnName = columnNameArgument.FragmentElement.Definition.Trim('"');
                var columnExpression = arguments[argumentIndex + 1];

                if (columnExpression.FragmentElement == null)
                {
                    break;
                }

                var outputColumn = AddOutputColumn(columnName);
                AddDataFlowLink(columnExpression.FragmentElement, outputColumn);

                argumentIndex += 2;
            }
        }
    }

    [DaxFunctionName("SUMMARIZE")]
    public class DaxSummarizeFunctionElement : DaxTableFunctionElement
    {
        public DaxSummarizeFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();

            int argumentIndex = 1;

            // group by column part
            while (argumentIndex < arguments.Count)
            {
                var argument = arguments[argumentIndex];
                if (!(argument.FragmentElement is DaxColumnReferenceElement))
                {
                    break;
                }

                var columnReference = argument.FragmentElement as DaxColumnReferenceElement;
                var outputColumn = AddOutputColumn(columnReference.ColumnName);
                AddDataFlowLink(argument.FragmentElement, outputColumn);

                argumentIndex++;
            }
            

            // aggregation columns - couples [name, expression]
            while (argumentIndex < arguments.Count - 1)
            {
                var columnNameArgument = arguments[argumentIndex];
                if (!(columnNameArgument.FragmentElement is DaxLiteralElement))
                {
                    break;
                }

                var columnName = columnNameArgument.FragmentElement.Definition.Trim('"');
                var columnExpression = arguments[argumentIndex + 1];

                if (columnExpression.FragmentElement == null)
                {
                    break;
                }

                // output column is influenced by the expression (not linking to the grouping columns)
                var outputColumn = AddOutputColumn(columnName);
                AddDataFlowLink(columnExpression.FragmentElement, outputColumn);

                argumentIndex += 2;
            }
        }
    }


    [DaxFunctionName("COUNTROWS")]
    public class DaxCountRowsFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxCountRowsFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("COUNT")]
    public class DaxCountFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxCountFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("COUNTA")]
    public class DaxCountaFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxCountaFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("DISTINCTCOUNT")]
    public class DaxDistinctCountFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxDistinctCountFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("SUM")]
    public class DaxSumFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxSumFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    // rest handled as general scalar functions (all arguments -> value)
    
    #endregion

    #region TEXT

    [DaxFunctionName("BLANK")]
    public class DaxBlankFunctionElement : DaxScalarFunctionElement
    {
        public DaxBlankFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("FORMAT")]
    public class DaxFormatFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxFormatFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    [DaxFunctionName("RIGHT")]
    public class DaxRightFunctionElement : DaxFirstArgumentScalarFunctionElement
    {
        public DaxRightFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }
    }

    // rest (and potentially all) handled as general scalar functions (all arguments -> value)

    #endregion

    /// <summary>
    /// ///////////////////////////////////////////
    /// </summary>

        



}
