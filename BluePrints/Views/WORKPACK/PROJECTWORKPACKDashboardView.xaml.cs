using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using BluePrints.ViewModels;
using BluePrints.Common;

namespace BluePrints.Views
{
    public partial class PROJECTWORKPACKDashboardView : UserControl
    {
        public PROJECTWORKPACKDashboardView()
        {
            InitializeComponent();
            ((PROJECTWORKPACKDashboardViewModelWrapper)this.DataContext).ChangeViewMemberFieldNames = this.ChangeViewMemberFieldNames;
        }
        public void ChangeViewMemberFieldNames(DashboardViewType DashboardViewType)
        {
            string headerTextVar;
            string maskVar;
            string displayFormatVar;
            string fieldNameReplaceFrom;
            string fieldNameReplaceTo;

            if (DashboardViewType == DashboardViewType.Costs)
            {
                headerTextVar = "$";
                maskVar = "c";
                fieldNameReplaceFrom = "Units";
                fieldNameReplaceTo = "Costs";
                displayFormatVar = "{0:c}";
            }
            else
            {
                headerTextVar = "Units";
                maskVar = "n";
                fieldNameReplaceFrom = "Costs";
                fieldNameReplaceTo = "Units";
                displayFormatVar = "{0:n}";
            }

            colCumulativeBudget.Header = "Budgeted " + headerTextVar;
            colCumulativeBudget.FieldName = colCumulativeBudget.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colCumulativeBudgetText.Mask = maskVar;
            colCumulativeEarnedPercentage.FieldName = colCumulativeEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);

            colCumulativePlanned.Header = "Planned " + headerTextVar;
            colCumulativePlanned.FieldName = colCumulativePlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colCumulativePlannedText.Mask = maskVar;
            colCumulativeEarned.Header = "Earned " + headerTextVar;
            colCumulativeEarned.FieldName = colCumulativeEarned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colCumulativeEarnedText.Mask = maskVar;
            colCumulativeBurned.Header = "Burned " + headerTextVar;
            colCumulativeBurned.FieldName = colCumulativeBurned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colCumulativeBurnedText.Mask = maskVar;
            colCumulativeActual.Header = "Actual " + headerTextVar;
            colCumulativeActual.FieldName = colCumulativeActual.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colCumulativeActualText.Mask = maskVar;

            colPeriodEarnedPercentage.FieldName = colPeriodEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colPeriodPlanned.Header = "Planned " + headerTextVar;
            colPeriodPlanned.FieldName = colPeriodPlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colPeriodPlannedText.Mask = maskVar;
            colPeriodEarned.Header = "Earned " + headerTextVar;
            colPeriodEarned.FieldName = colPeriodEarned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colPeriodEarnedText.Mask = maskVar;
            colPeriodBurned.Header = "Burned " + headerTextVar;
            colPeriodBurned.FieldName = colPeriodBurned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colPeriodBurnedText.Mask = maskVar;
            colPeriodActual.Header = "Actual " + headerTextVar;
            colPeriodActual.FieldName = colPeriodActual.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colPeriodActualText.Mask = maskVar;

            lineSeriesOriginal.DisplayName = lineSeriesPlanned.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesPlanned.DisplayName = lineSeriesPlanned.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesEarned.DisplayName = lineSeriesEarned.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesBurned.DisplayName = lineSeriesBurned.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesActual.DisplayName = lineSeriesActual.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesOriginal.ValueDataMember = lineSeriesOriginal.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesPlanned.ValueDataMember = lineSeriesPlanned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesEarned.ValueDataMember = lineSeriesEarned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesBurned.ValueDataMember = lineSeriesBurned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesActual.ValueDataMember = lineSeriesActual.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesOriginal.DisplayName = barSeriesOriginal.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesPlanned.DisplayName = barSeriesPlanned.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesEarned.DisplayName = barSeriesEarned.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesBurned.DisplayName = barSeriesBurned.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesActual.DisplayName = barSeriesActual.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesOriginal.ValueDataMember = barSeriesOriginal.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesPlanned.ValueDataMember = barSeriesPlanned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesEarned.ValueDataMember = barSeriesEarned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesBurned.ValueDataMember = barSeriesBurned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesActual.ValueDataMember = barSeriesActual.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            secondaryAxisY.Title.Content = secondaryAxisY.Title.Content.ToString().Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            primaryAxisY.Title.Content = primaryAxisY.Title.Content.ToString().Replace(fieldNameReplaceFrom, fieldNameReplaceTo);

            totalSummaryCumulativeEarnedPercentage.FieldName = totalSummaryCumulativeEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativePlanned.FieldName = totalSummaryCumulativePlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeEarned.FieldName = totalSummaryCumulativeEarned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeBurned.FieldName = totalSummaryCumulativeBurned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeActual.FieldName = totalSummaryCumulativeActual.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodEarnedPercentage.FieldName = totalSummaryPeriodEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodPlanned.FieldName = totalSummaryPeriodPlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodEarned.FieldName = totalSummaryPeriodEarned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodBurned.FieldName = totalSummaryPeriodBurned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodActual.FieldName = totalSummaryPeriodActual.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);

            groupSummaryBudgeted.FieldName = groupSummaryBudgeted.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeEarnedPercentage.FieldName = groupSummaryCumulativeEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativePlanned.FieldName = groupSummaryCumulativePlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeEarned.FieldName = groupSummaryCumulativeEarned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeBurned.FieldName = groupSummaryCumulativeBurned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeActual.FieldName = groupSummaryCumulativeActual.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodEarnedPercentage.FieldName = groupSummaryPeriodEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodPlanned.FieldName = groupSummaryPeriodPlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodEarned.FieldName = groupSummaryPeriodEarned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodBurned.FieldName = groupSummaryPeriodBurned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodActual.FieldName = groupSummaryPeriodActual.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);

            totalSummaryBudgeted.ShowInColumn = totalSummaryBudgeted.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeEarnedPercentage.ShowInColumn = totalSummaryCumulativeEarnedPercentage.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativePlanned.ShowInColumn = totalSummaryCumulativePlanned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeEarned.ShowInColumn = totalSummaryCumulativeEarned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeBurned.ShowInColumn = totalSummaryCumulativeBurned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeActual.ShowInColumn = totalSummaryCumulativeActual.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodEarnedPercentage.ShowInColumn = totalSummaryPeriodEarnedPercentage.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodPlanned.ShowInColumn = totalSummaryPeriodPlanned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodEarned.ShowInColumn = totalSummaryPeriodEarned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodBurned.ShowInColumn = totalSummaryPeriodBurned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodActual.ShowInColumn = totalSummaryPeriodActual.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);

            groupSummaryBudgeted.ShowInColumn = groupSummaryBudgeted.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeEarnedPercentage.ShowInColumn = groupSummaryCumulativeEarnedPercentage.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativePlanned.ShowInColumn = groupSummaryCumulativePlanned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeEarned.ShowInColumn = groupSummaryCumulativeEarned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeBurned.ShowInColumn = groupSummaryCumulativeBurned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeActual.ShowInColumn = groupSummaryCumulativeActual.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodEarnedPercentage.ShowInColumn = groupSummaryPeriodEarnedPercentage.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodPlanned.ShowInColumn = groupSummaryPeriodPlanned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodEarned.ShowInColumn = groupSummaryPeriodEarned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodBurned.ShowInColumn = groupSummaryPeriodBurned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodActual.ShowInColumn = groupSummaryPeriodActual.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);

            totalSummaryBudgeted.DisplayFormat = displayFormatVar;
            totalSummaryCumulativePlanned.DisplayFormat = displayFormatVar;
            totalSummaryCumulativeEarned.DisplayFormat = displayFormatVar;
            totalSummaryCumulativeBurned.DisplayFormat = displayFormatVar;
            totalSummaryCumulativeActual.DisplayFormat = displayFormatVar;
            totalSummaryPeriodPlanned.DisplayFormat = displayFormatVar;
            totalSummaryPeriodEarned.DisplayFormat = displayFormatVar;
            totalSummaryPeriodBurned.DisplayFormat = displayFormatVar;
            totalSummaryPeriodActual.DisplayFormat = displayFormatVar;

            groupSummaryBudgeted.DisplayFormat = displayFormatVar;
            groupSummaryCumulativePlanned.DisplayFormat = displayFormatVar;
            groupSummaryCumulativeEarned.DisplayFormat = displayFormatVar;
            groupSummaryCumulativeBurned.DisplayFormat = displayFormatVar;
            groupSummaryCumulativeActual.DisplayFormat = displayFormatVar;
            groupSummaryPeriodPlanned.DisplayFormat = displayFormatVar;
            groupSummaryPeriodEarned.DisplayFormat = displayFormatVar;
            groupSummaryPeriodBurned.DisplayFormat = displayFormatVar;
            groupSummaryPeriodActual.DisplayFormat = displayFormatVar;

            foreach (var formatCondition in tableView.FormatConditions)
            {
                if (formatCondition.Expression != null)
                {
                    formatCondition.Expression = formatCondition.Expression.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
                    formatCondition.FieldName = formatCondition.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
                }
            }

            groupSummaryBudgeted.FieldName = groupSummaryBudgeted.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryBudgeted.DisplayFormat = displayFormatVar;
            totalSummaryBudgeted.FieldName = totalSummaryBudgeted.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryBudgeted.DisplayFormat = displayFormatVar;
        }
    }
}
