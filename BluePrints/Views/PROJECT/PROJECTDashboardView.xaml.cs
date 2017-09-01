using BluePrints.Common;
using BluePrints.ViewModels;
using System.Windows.Controls;
using System.Windows.Interop;

namespace BluePrints.Views
{
    public partial class PROJECTDashboardView : UserControl
    {
        public PROJECTDashboardView()
        {
            InitializeComponent();

            HwndSource source = (HwndSource)HwndSource.FromVisual(this);
            HwndTarget target = source?.CompositionTarget;
            if (target != null)
                target.RenderMode = RenderMode.SoftwareOnly;

            ((PROJECTDashboardViewModelWrapper)DataContext).ChangeViewMemberFieldNames = ChangeViewMemberFieldNames;
        }

        public void ChangeViewMemberFieldNames(DashboardViewType DashboardViewType)
        {
            string displayFormatVar;
            string fieldNameReplaceFrom;
            string fieldNameReplaceTo;

            if (DashboardViewType == DashboardViewType.Costs)
            {
                fieldNameReplaceFrom = "Units";
                fieldNameReplaceTo = "Costs";
                displayFormatVar = "{0:c}";
            }
            else
            {
                fieldNameReplaceFrom = "Costs";
                fieldNameReplaceTo = "Units";
                displayFormatVar = "{0:n}";
            }

            secondaryAxisY.Title.Content = secondaryAxisY.Title.Content.ToString().Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            primaryAxisY.Title.Content = primaryAxisY.Title.Content.ToString().Replace(fieldNameReplaceFrom, fieldNameReplaceTo);

            totalSummaryCumulativeEarned.FieldName = totalSummaryCumulativeEarned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeBurned.FieldName = totalSummaryCumulativeBurned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeActual.FieldName = totalSummaryCumulativeActual.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodPlanned.FieldName = totalSummaryPeriodPlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodEarned.FieldName = totalSummaryPeriodEarned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodBurned.FieldName = totalSummaryPeriodBurned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodActual.FieldName = totalSummaryPeriodActual.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryBudgeted.ShowInColumn = totalSummaryBudgeted.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeEarned.ShowInColumn = totalSummaryCumulativeEarned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeBurned.ShowInColumn = totalSummaryCumulativeBurned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeActual.ShowInColumn = totalSummaryCumulativeActual.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodPlanned.ShowInColumn = totalSummaryPeriodPlanned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodEarned.ShowInColumn = totalSummaryPeriodEarned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodBurned.ShowInColumn = totalSummaryPeriodBurned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodActual.ShowInColumn = totalSummaryPeriodActual.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryBudgeted.DisplayFormat = displayFormatVar;
            totalSummaryCumulativeEarned.DisplayFormat = displayFormatVar;
            totalSummaryCumulativeBurned.DisplayFormat = displayFormatVar;
            totalSummaryCumulativeActual.DisplayFormat = displayFormatVar;
            totalSummaryPeriodPlanned.DisplayFormat = displayFormatVar;
            totalSummaryPeriodEarned.DisplayFormat = displayFormatVar;
            totalSummaryPeriodBurned.DisplayFormat = displayFormatVar;
            totalSummaryPeriodActual.DisplayFormat = displayFormatVar;

            foreach (var formatCondition in tableView.FormatConditions)
                if (formatCondition.Expression != null)
                {
                    formatCondition.Expression = formatCondition.Expression.Replace(fieldNameReplaceFrom,
                        fieldNameReplaceTo);
                    formatCondition.FieldName = formatCondition.FieldName.Replace(fieldNameReplaceFrom,
                        fieldNameReplaceTo);
                }

            totalSummaryBudgeted.FieldName = totalSummaryBudgeted.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            totalSummaryBudgeted.DisplayFormat = displayFormatVar;
        }
    }
}