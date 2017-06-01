using BluePrints.Common;
using BluePrints.ViewModels;
using DevExpress.Xpf.Grid;
using System.Windows.Controls;

namespace BluePrints.Views
{
    public partial class PROJECTView : UserControl
    {
        public PROJECTView()
        {
            InitializeComponent();
            ((PROJECTViewModelWrapper) DataContext).ChangeViewMemberFieldNames = ChangeViewMemberFieldNames;
            ((PROJECTViewModelWrapper) DataContext).Redraw = Redraw;

            ((PROJECTViewModelWrapper)DataContext).AssignBASELINEDelegates = this.AssignBASELINEDelegates;
            ((PROJECTViewModelWrapper)DataContext).AssignPROGRESSDelegates = this.AssignPROGRESSDelegates;
            //((PROJECTViewModelWrapper)DataContext).AssignAREADelegates = this.AssignAREADelegates;
            ((PROJECTViewModelWrapper)DataContext).AssignRATEDelegates = this.AssignRATEDelegates;
        }

        int focusedRowHandleBASELINE;
        ColumnBase currentColumnBASELINE;
        private void AssignBASELINEDelegates(BASELINECollectionViewModelWrapper viewModelWrapper)
        {
            viewModelWrapper.StoreActiveCell = this.StoreBASELINEFocusedCell;
            viewModelWrapper.RestoreActiveCell = this.RestoreBASELINEFocusedCell;
        }

        protected void StoreBASELINEFocusedCell()
        {
            focusedRowHandleBASELINE = tableViewBASELINE.FocusedRowHandle;
            currentColumnBASELINE = gridControlBASELINE.CurrentColumn;
        }

        protected void RestoreBASELINEFocusedCell()
        {
            gridControlBASELINE.CurrentColumn = currentColumnBASELINE;
            tableViewBASELINE.FocusedRowHandle = focusedRowHandleBASELINE;
            gridControlBASELINE.Focus();
            tableViewBASELINE.ShowEditor();
        }

        int focusedRowHandlePROGRESS;
        ColumnBase currentColumnPROGRESS;
        private void AssignPROGRESSDelegates(PROGRESSCollectionViewModelWrapper viewModelWrapper)
        {
            viewModelWrapper.StoreActiveCell = this.StorePROGRESSFocusedCell;
            viewModelWrapper.RestoreActiveCell = this.RestorePROGRESSFocusedCell;
        }

        protected void StorePROGRESSFocusedCell()
        {
            focusedRowHandlePROGRESS = tableViewPROGRESS.FocusedRowHandle;
            currentColumnPROGRESS = gridControlPROGRESS.CurrentColumn;
        }

        protected void RestorePROGRESSFocusedCell()
        {
            gridControlPROGRESS.CurrentColumn = currentColumnPROGRESS;
            tableViewPROGRESS.FocusedRowHandle = focusedRowHandlePROGRESS;
            gridControlPROGRESS.Focus();
            tableViewPROGRESS.ShowEditor();
        }

        //int focusedRowHandleAREA;
        //ColumnBase currentColumnAREA;
        //private void AssignAREADelegates(AREACollectionViewModelWrapper viewModelWrapper)
        //{
        //    viewModelWrapper.StoreActiveCell = this.StoreAREAFocusedCell;
        //    viewModelWrapper.RestoreActiveCell = this.RestoreAREAFocusedCell;
        //}

        //protected void StoreAREAFocusedCell()
        //{
        //    focusedRowHandleAREA = tableViewAREA.FocusedRowHandle;
        //    currentColumnAREA = gridControlAREA.CurrentColumn;
        //}

        //protected void RestoreAREAFocusedCell()
        //{
        //    gridControlAREA.CurrentColumn = currentColumnAREA;
        //    tableViewAREA.FocusedRowHandle = focusedRowHandleAREA;
        //    gridControlAREA.Focus();
        //    tableViewAREA.ShowEditor();
        //}

        int focusedRowHandleRATE;
        ColumnBase currentColumnRATE;
        private void AssignRATEDelegates(RATECollectionViewModelWrapper viewModelWrapper)
        {
            viewModelWrapper.StoreActiveCell = this.StoreRATEFocusedCell;
            viewModelWrapper.RestoreActiveCell = this.RestoreRATEFocusedCell;
        }

        protected void StoreRATEFocusedCell()
        {
            focusedRowHandleRATE = tableViewRATE.FocusedRowHandle;
            currentColumnRATE = gridControlRATE.CurrentColumn;
        }

        protected void RestoreRATEFocusedCell()
        {
            gridControlRATE.CurrentColumn = currentColumnRATE;
            tableViewRATE.FocusedRowHandle = focusedRowHandleRATE;
            gridControlRATE.Focus();
            tableViewRATE.ShowEditor();
        }

        public void Redraw()
        {
            //GridControl.RefreshData();
        }

        public void ChangeViewMemberFieldNames(DashboardViewType DashboardViewType)
        {
            string headerTextVar;
            string maskVar;
            string fieldNameReplaceFrom;
            string fieldNameReplaceTo;

            if (DashboardViewType == DashboardViewType.Costs)
            {
                headerTextVar = "$";
                maskVar = "c";
                fieldNameReplaceFrom = "Units";
                fieldNameReplaceTo = "Costs";
            }
            else
            {
                headerTextVar = "Units";
                maskVar = "n";
                fieldNameReplaceFrom = "Costs";
                fieldNameReplaceTo = "Units";
            }

            colCumulativeBudget.Header = "Budgeted " + headerTextVar;
            colCumulativeBudget.FieldName = colCumulativeBudget.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colCumulativeBudgetText.Mask = maskVar;
            colCumulativeEarnedPercentage.FieldName =
                colCumulativeEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);

            colCumulativePlanned.Header = "Planned " + headerTextVar;
            colCumulativePlanned.FieldName = colCumulativePlanned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colCumulativePlannedText.Mask = maskVar;
            colCumulativeEarned.Header = "Earned " + headerTextVar;
            colCumulativeEarned.FieldName = colCumulativeEarned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colCumulativeEarnedText.Mask = maskVar;
            colCumulativeBurned.Header = "Burned " + headerTextVar;
            colCumulativeBurned.FieldName = colCumulativeBurned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colCumulativeBurnedText.Mask = maskVar;
            colCumulativeActual.Header = "Actual " + headerTextVar;
            colCumulativeActual.FieldName = colCumulativeActual.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colCumulativeActualText.Mask = maskVar;

            colPeriodEarnedPercentage.FieldName = colPeriodEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
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
            lineSeriesRemainingPlanned.DisplayName = lineSeriesRemainingPlanned.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesOriginal.ValueDataMember = lineSeriesOriginal.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesPlanned.ValueDataMember = lineSeriesPlanned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesEarned.ValueDataMember = lineSeriesEarned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesBurned.ValueDataMember = lineSeriesBurned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesActual.ValueDataMember = lineSeriesActual.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            lineSeriesRemainingPlanned.ValueDataMember = lineSeriesRemainingPlanned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesOriginal.DisplayName = barSeriesOriginal.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesPlanned.DisplayName = barSeriesPlanned.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesEarned.DisplayName = barSeriesEarned.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesBurned.DisplayName = barSeriesBurned.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesActual.DisplayName = barSeriesActual.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesRemainingPlanned.DisplayName = barSeriesRemainingPlanned.DisplayName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesOriginal.ValueDataMember = barSeriesOriginal.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesPlanned.ValueDataMember = barSeriesPlanned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesEarned.ValueDataMember = barSeriesEarned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesBurned.ValueDataMember = barSeriesBurned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesActual.ValueDataMember = barSeriesActual.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            barSeriesRemainingPlanned.ValueDataMember = barSeriesRemainingPlanned.ValueDataMember.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            secondaryAxisY.Title.Content = secondaryAxisY.Title.Content.ToString().Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            primaryAxisY.Title.Content = primaryAxisY.Title.Content.ToString().Replace(fieldNameReplaceFrom, fieldNameReplaceTo);


            foreach (var formatCondition in tableView.FormatConditions)
                if (formatCondition.Expression != null)
                {
                    formatCondition.Expression = formatCondition.Expression.Replace(fieldNameReplaceFrom,
                        fieldNameReplaceTo);
                    formatCondition.FieldName = formatCondition.FieldName.Replace(fieldNameReplaceFrom,
                        fieldNameReplaceTo);
                }

        }
    }
}