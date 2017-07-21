using BluePrints.Common;
using BluePrints.Common.ViewModel.Converters;
using BluePrints.ViewModels;
using DevExpress.Xpf.Charts;
using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BluePrints.Views
{
    public partial class PROJECTView : UserControl
    {
        public PROJECTView()
        {
            InitializeComponent();
            ((PROJECTViewModelWrapper)DataContext).ChangeViewMemberFieldNames = ChangeViewMemberFieldNames;
            ((PROJECTViewModelWrapper)DataContext).Redraw = Redraw;

            ((PROJECTViewModelWrapper)DataContext).AssignBASELINEDelegates = this.AssignBASELINEDelegates;
            ((PROJECTViewModelWrapper)DataContext).AssignPROGRESSDelegates = this.AssignPROGRESSDelegates;
            ((PROJECTViewModelWrapper)DataContext).AssignESTIMATION_DIRECTDelegates = this.AssignESTIMATION_DIRECTDelegates;
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

        int focusedRowHandleESTIMATE;
        ColumnBase currentColumnESTIMATE;
        private void AssignESTIMATION_DIRECTDelegates(ESTIMATION_DIRECTCollectionViewModelWrapper viewModelWrapper)
        {
            viewModelWrapper.StoreActiveCell = this.StoreESTIMATION_DIRECTFocusedCell;
            viewModelWrapper.RestoreActiveCell = this.RestoreESTIMATION_DIRECTFocusedCell;
        }

        protected void StoreESTIMATION_DIRECTFocusedCell()
        {
            focusedRowHandleESTIMATE = tableViewESTIMATION_DIRECT.FocusedRowHandle;
            currentColumnESTIMATE = gridControlESTIMATION_DIRECT.CurrentColumn;
        }

        protected void RestoreESTIMATION_DIRECTFocusedCell()
        {
            gridControlESTIMATION_DIRECT.CurrentColumn = currentColumnESTIMATE;
            tableViewESTIMATION_DIRECT.FocusedRowHandle = focusedRowHandleESTIMATE;
            gridControlESTIMATION_DIRECT.Focus();
            tableViewESTIMATION_DIRECT.ShowEditor();
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
            string displayFormatVar;
            string fieldNameReplaceFrom;
            string fieldNameReplaceTo;

            if (DashboardViewType == DashboardViewType.Costs)
            {
                colCumulativeActual.Visible = true;
                colPeriodActual.Visible = true;
                totalSummaryPeriodActual.Visible = true;
                totalSummaryCumulativeActual.Visible = true;
                groupSummaryPeriodActual.Visible = true;
                groupSummaryCumulativeActual.Visible = true;
                colDisciplinePeriodActual.Visible = true;
                colDisciplineCumulativeActual.Visible = true;
                colPhaseCumulativeActual.Visible = true;
                colPhasePeriodActual.Visible = true;
                colCommodityCumulativeActual.Visible = true;
                colCommodityPeriodActual.Visible = true;
                barSeriesActual.Visible = true;
                lineSeriesActual.Visible = true;


                headerTextVar = "$";
                maskVar = "c";
                fieldNameReplaceFrom = "Units";
                fieldNameReplaceTo = "Costs";
                displayFormatVar = "{0:c}";
            }
            else
            {
                colCumulativeActual.Visible = false;
                colPeriodActual.Visible = false;
                totalSummaryPeriodActual.Visible = false;
                totalSummaryCumulativeActual.Visible = false;
                groupSummaryPeriodActual.Visible = false;
                groupSummaryCumulativeActual.Visible = false;
                colDisciplinePeriodActual.Visible = false;
                colDisciplineCumulativeActual.Visible = false;
                colPhaseCumulativeActual.Visible = false;
                colPhasePeriodActual.Visible = false;
                barSeriesActual.Visible = false;
                lineSeriesActual.Visible = false;

                headerTextVar = "Units";
                maskVar = "n";
                fieldNameReplaceFrom = "Costs";
                fieldNameReplaceTo = "Units";
                displayFormatVar = "{0:n}";
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

            colPhaseCumulativePlanned.Header = "Planned " + headerTextVar;
            colPhaseCumulativePlanned.FieldName = colPhaseCumulativePlanned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colPhaseCumulativePlannedText.Mask = maskVar;
            colPhaseCumulativeEarned.Header = "Earned " + headerTextVar;
            colPhaseCumulativeEarned.FieldName = colPhaseCumulativeEarned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colPhaseCumulativeEarnedText.Mask = maskVar;
            colPhaseCumulativeBurned.Header = "Burned " + headerTextVar;
            colPhaseCumulativeBurned.FieldName = colPhaseCumulativeBurned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colPhaseCumulativeBurnedText.Mask = maskVar;
            colPhaseCumulativeActual.Header = "Actual " + headerTextVar;
            colPhaseCumulativeActual.FieldName = colPhaseCumulativeActual.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colPhaseCumulativeActualText.Mask = maskVar;

            colPhasePeriodEarnedPercentage.FieldName = colPhasePeriodEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colPhasePeriodPlanned.Header = "Planned " + headerTextVar;
            colPhasePeriodPlanned.FieldName = colPhasePeriodPlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colPhasePeriodPlannedText.Mask = maskVar;
            colPhasePeriodEarned.Header = "Earned " + headerTextVar;
            colPhasePeriodEarned.FieldName = colPhasePeriodEarned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colPhasePeriodEarnedText.Mask = maskVar;
            colPhasePeriodBurned.Header = "Burned " + headerTextVar;
            colPhasePeriodBurned.FieldName = colPhasePeriodBurned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colPhasePeriodBurnedText.Mask = maskVar;
            colPhasePeriodActual.Header = "Actual " + headerTextVar;
            colPhasePeriodActual.FieldName = colPhasePeriodActual.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colPhasePeriodActualText.Mask = maskVar;

            colDisciplineCumulativePlanned.Header = "Planned " + headerTextVar;
            colDisciplineCumulativePlanned.FieldName = colDisciplineCumulativePlanned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colDisciplineCumulativePlannedText.Mask = maskVar;
            colDisciplineCumulativeEarned.Header = "Earned " + headerTextVar;
            colDisciplineCumulativeEarned.FieldName = colDisciplineCumulativeEarned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colDisciplineCumulativeEarnedText.Mask = maskVar;
            colDisciplineCumulativeBurned.Header = "Burned " + headerTextVar;
            colDisciplineCumulativeBurned.FieldName = colDisciplineCumulativeBurned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colDisciplineCumulativeBurnedText.Mask = maskVar;
            colDisciplineCumulativeActual.Header = "Actual " + headerTextVar;
            colDisciplineCumulativeActual.FieldName = colDisciplineCumulativeActual.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colDisciplineCumulativeActualText.Mask = maskVar;

            colDisciplinePeriodEarnedPercentage.FieldName = colDisciplinePeriodEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colDisciplinePeriodPlanned.Header = "Planned " + headerTextVar;
            colDisciplinePeriodPlanned.FieldName = colDisciplinePeriodPlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colDisciplinePeriodPlannedText.Mask = maskVar;
            colDisciplinePeriodEarned.Header = "Earned " + headerTextVar;
            colDisciplinePeriodEarned.FieldName = colDisciplinePeriodEarned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colDisciplinePeriodEarnedText.Mask = maskVar;
            colDisciplinePeriodBurned.Header = "Burned " + headerTextVar;
            colDisciplinePeriodBurned.FieldName = colDisciplinePeriodBurned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colDisciplinePeriodBurnedText.Mask = maskVar;
            colDisciplinePeriodActual.Header = "Actual " + headerTextVar;
            colDisciplinePeriodActual.FieldName = colDisciplinePeriodActual.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colDisciplinePeriodActualText.Mask = maskVar;

            colCommodityCumulativePlanned.Header = "Planned " + headerTextVar;
            colCommodityCumulativePlanned.FieldName = colCommodityCumulativePlanned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colCommodityCumulativePlannedText.Mask = maskVar;
            colCommodityCumulativeEarned.Header = "Earned " + headerTextVar;
            colCommodityCumulativeEarned.FieldName = colCommodityCumulativeEarned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colCommodityCumulativeEarnedText.Mask = maskVar;
            colCommodityCumulativeBurned.Header = "Burned " + headerTextVar;
            colCommodityCumulativeBurned.FieldName = colCommodityCumulativeBurned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colCommodityCumulativeBurnedText.Mask = maskVar;
            colCommodityCumulativeActual.Header = "Actual " + headerTextVar;
            colCommodityCumulativeActual.FieldName = colCommodityCumulativeActual.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colCommodityCumulativeActualText.Mask = maskVar;

            colCommodityPeriodEarnedPercentage.FieldName = colCommodityPeriodEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            colCommodityPeriodPlanned.Header = "Planned " + headerTextVar;
            colCommodityPeriodPlanned.FieldName = colCommodityPeriodPlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colCommodityPeriodPlannedText.Mask = maskVar;
            colCommodityPeriodEarned.Header = "Earned " + headerTextVar;
            colCommodityPeriodEarned.FieldName = colCommodityPeriodEarned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colCommodityPeriodEarnedText.Mask = maskVar;
            colCommodityPeriodBurned.Header = "Burned " + headerTextVar;
            colCommodityPeriodBurned.FieldName = colCommodityPeriodBurned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colCommodityPeriodBurnedText.Mask = maskVar;
            colCommodityPeriodActual.Header = "Actual " + headerTextVar;
            colCommodityPeriodActual.FieldName = colCommodityPeriodActual.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            colCommodityPeriodActualText.Mask = maskVar;

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

            //totalSummaryCumulativeEarnedPercentage.FieldName =
            //    totalSummaryCumulativeEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            //totalSummaryCumulativePlanned.FieldName =
            //    totalSummaryCumulativePlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeEarned.FieldName = totalSummaryCumulativeEarned.FieldName.Replace(
                fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeBurned.FieldName = totalSummaryCumulativeBurned.FieldName.Replace(
                fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeActual.FieldName = totalSummaryCumulativeActual.FieldName.Replace(
                fieldNameReplaceFrom, fieldNameReplaceTo);
            //totalSummaryPeriodEarnedPercentage.FieldName =
            //    totalSummaryPeriodEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodPlanned.FieldName = totalSummaryPeriodPlanned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            totalSummaryPeriodEarned.FieldName = totalSummaryPeriodEarned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            totalSummaryPeriodBurned.FieldName = totalSummaryPeriodBurned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            totalSummaryPeriodActual.FieldName = totalSummaryPeriodActual.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);

            groupSummaryBudgeted.FieldName = groupSummaryBudgeted.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            //groupSummaryCumulativeEarnedPercentage.FieldName =
            //    groupSummaryCumulativeEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            //groupSummaryCumulativePlanned.FieldName =
            //    groupSummaryCumulativePlanned.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeEarned.FieldName = groupSummaryCumulativeEarned.FieldName.Replace(
                fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeBurned.FieldName = groupSummaryCumulativeBurned.FieldName.Replace(
                fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeActual.FieldName = groupSummaryCumulativeActual.FieldName.Replace(
                fieldNameReplaceFrom, fieldNameReplaceTo);
            //groupSummaryPeriodEarnedPercentage.FieldName =
            //    groupSummaryPeriodEarnedPercentage.FieldName.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodPlanned.FieldName = groupSummaryPeriodPlanned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            groupSummaryPeriodEarned.FieldName = groupSummaryPeriodEarned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            groupSummaryPeriodBurned.FieldName = groupSummaryPeriodBurned.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            groupSummaryPeriodActual.FieldName = groupSummaryPeriodActual.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);

            totalSummaryBudgeted.ShowInColumn = totalSummaryBudgeted.ShowInColumn.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            //totalSummaryCumulativeEarnedPercentage.ShowInColumn =
            //    totalSummaryCumulativeEarnedPercentage.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            //totalSummaryCumulativePlanned.ShowInColumn =
            //    totalSummaryCumulativePlanned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeEarned.ShowInColumn =
                totalSummaryCumulativeEarned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeBurned.ShowInColumn =
                totalSummaryCumulativeBurned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryCumulativeActual.ShowInColumn =
                totalSummaryCumulativeActual.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            //totalSummaryPeriodEarnedPercentage.ShowInColumn =
            //    totalSummaryPeriodEarnedPercentage.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodPlanned.ShowInColumn = totalSummaryPeriodPlanned.ShowInColumn.Replace(
                fieldNameReplaceFrom, fieldNameReplaceTo);
            totalSummaryPeriodEarned.ShowInColumn = totalSummaryPeriodEarned.ShowInColumn.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            totalSummaryPeriodBurned.ShowInColumn = totalSummaryPeriodBurned.ShowInColumn.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            totalSummaryPeriodActual.ShowInColumn = totalSummaryPeriodActual.ShowInColumn.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);

            groupSummaryBudgeted.ShowInColumn = groupSummaryBudgeted.ShowInColumn.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            //groupSummaryCumulativeEarnedPercentage.ShowInColumn =
            //    groupSummaryCumulativeEarnedPercentage.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            //groupSummaryCumulativePlanned.ShowInColumn =
            //    groupSummaryCumulativePlanned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeEarned.ShowInColumn =
                groupSummaryCumulativeEarned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeBurned.ShowInColumn =
                groupSummaryCumulativeBurned.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryCumulativeActual.ShowInColumn =
                groupSummaryCumulativeActual.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            //groupSummaryPeriodEarnedPercentage.ShowInColumn =
            //    groupSummaryPeriodEarnedPercentage.ShowInColumn.Replace(fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodPlanned.ShowInColumn = groupSummaryPeriodPlanned.ShowInColumn.Replace(
                fieldNameReplaceFrom, fieldNameReplaceTo);
            groupSummaryPeriodEarned.ShowInColumn = groupSummaryPeriodEarned.ShowInColumn.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            groupSummaryPeriodBurned.ShowInColumn = groupSummaryPeriodBurned.ShowInColumn.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            groupSummaryPeriodActual.ShowInColumn = groupSummaryPeriodActual.ShowInColumn.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);

            totalSummaryBudgeted.DisplayFormat = displayFormatVar;
            //totalSummaryCumulativePlanned.DisplayFormat = displayFormatVar;
            totalSummaryCumulativeEarned.DisplayFormat = displayFormatVar;
            totalSummaryCumulativeBurned.DisplayFormat = displayFormatVar;
            totalSummaryCumulativeActual.DisplayFormat = displayFormatVar;
            totalSummaryPeriodPlanned.DisplayFormat = displayFormatVar;
            totalSummaryPeriodEarned.DisplayFormat = displayFormatVar;
            totalSummaryPeriodBurned.DisplayFormat = displayFormatVar;
            totalSummaryPeriodActual.DisplayFormat = displayFormatVar;

            groupSummaryBudgeted.DisplayFormat = displayFormatVar;
            //groupSummaryCumulativePlanned.DisplayFormat = displayFormatVar;
            groupSummaryCumulativeEarned.DisplayFormat = displayFormatVar;
            groupSummaryCumulativeBurned.DisplayFormat = displayFormatVar;
            groupSummaryCumulativeActual.DisplayFormat = displayFormatVar;
            groupSummaryPeriodPlanned.DisplayFormat = displayFormatVar;
            groupSummaryPeriodEarned.DisplayFormat = displayFormatVar;
            groupSummaryPeriodBurned.DisplayFormat = displayFormatVar;
            groupSummaryPeriodActual.DisplayFormat = displayFormatVar;

            foreach (var formatCondition in tableView.FormatConditions)
                if (formatCondition.Expression != null)
                {
                    formatCondition.Expression = formatCondition.Expression.Replace(fieldNameReplaceFrom,
                        fieldNameReplaceTo);
                    formatCondition.FieldName = formatCondition.FieldName.Replace(fieldNameReplaceFrom,
                        fieldNameReplaceTo);
                }

            groupSummaryBudgeted.FieldName = groupSummaryBudgeted.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            groupSummaryBudgeted.DisplayFormat = displayFormatVar;
            totalSummaryBudgeted.FieldName = totalSummaryBudgeted.FieldName.Replace(fieldNameReplaceFrom,
                fieldNameReplaceTo);
            totalSummaryBudgeted.DisplayFormat = displayFormatVar;
        }
    }
}