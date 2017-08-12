using BluePrints.Common;
using BluePrints.ViewModels;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class USERDashboardView : UserControl
    {
        public USERDashboardView()
        {
            InitializeComponent();
            ((USERDashboardViewModelWrapper)DataContext).ChangeViewMemberFieldNames = ChangeViewMemberFieldNames;
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
            totalSummaryPeriodPlanned.FieldName = totalSummaryPeriodPlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodEarned.FieldName = totalSummaryPeriodEarned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryBudgeted.ShowInColumn = totalSummaryBudgeted.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeEarned.ShowInColumn = totalSummaryCumulativeEarned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodPlanned.ShowInColumn = totalSummaryPeriodPlanned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodEarned.ShowInColumn = totalSummaryPeriodEarned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryBudgeted.DisplayFormat = displayFormatVar;
            totalSummaryCumulativeEarned.DisplayFormat = displayFormatVar;
            totalSummaryPeriodPlanned.DisplayFormat = displayFormatVar;
            totalSummaryPeriodEarned.DisplayFormat = displayFormatVar;

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