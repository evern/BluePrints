using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using BluePrints.Reports;
using System.IO;
using BluePrints.Common.Reports;
using BaseModel.ViewModel.Dialogs;
using BluePrints.Common.Resources;
using BaseModel.ViewModel.Services;
using DevExpress.Mvvm.DataAnnotations;
using BluePrints.P6EntitiesDataModel;
using DevExpress.Xpf.Core.ConditionalFormatting;
using System.Data;
using System.Windows.Media;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Editors;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class PROJECTForecastViewModelWrapper : PROJECTViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public new static PROJECTForecastViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROJECTForecastViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected PROJECTForecastViewModelWrapper()
        {

        }

        IEnumerable<ExoTimeAuthorisation> jobLines { get; set; }
        IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

        List<string> defaultColumnFieldNames = new List<string>();
        List<string> hiddenColumnFieldNames = new List<string>();
        protected override void resolveParameters(object parameter)
        {
            base.resolveParameters(parameter);
            defaultColumnFieldNames.Add(columnEntity);
            jobLines = ExoQueries.GetProjectLines(primeroUnitOfWork, loadPROJECT.NUMBER);
        }

        protected override void onSummaryCalculateComplete()
        {
            this.RaisePropertyChanged(x => x.DataPointsTable);
        }

        public override void FullRefresh()
        {
            dataPointsTable = null;
            base.FullRefresh();
        }

        #region Data Points Table
        string columnEntity = "Entity";
        DataTable dataPointsTable = null;
        public DataTable DataPointsTable
        {
            get
            {
                if (MainViewModel == null || SingleProjectDashboards == null)
                    return null;

                if (dataPointsTable == null)
                {
                    IQueryable<ExoSubJobProjection> exoSubJobs = ExoQueries.GetNativeExoSubJobProjection(primeroUnitOfWork, loadPROJECT);

                    dataPointsTable = new DataTable();
                    TimeSpan interval = new TimeSpan(7, 0, 0, 0);
                    DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(liveDesignProgress);
                    DateTime lastDataDate = SingleProjectDashboards.Where(x => x.Stats != null && x.Stats.Remaining != null).Max(x => x.Stats.Remaining.EndDate);
                    IEnumerable<DateTime> alignedDataDateCollection = ChronologicalHelpers.GenerateAlignedDatesCollection(firstAlignedDataDate, lastDataDate, interval);

                    dataPointsTable.Columns.Add(columnEntity, typeof(ExoSubJobProjection));

                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();

                        if (alignedDataDate == liveDesignProgress.DATA_DATE)
                        {
                            DataColumn lastColumn = new DataColumn();
                            lastColumn.ColumnName = columnFieldName;
                            lastColumn.DataType = typeof(decimal);
                            dataPointsTable.Columns.Add(lastColumn);
                        }
                        else
                            dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    foreach (ExoSubJobProjection entity in exoSubJobs)
                    {
                        BuildRowStats(entity, false);
                    }

                    TableViewService.ScrollToLast();
                }

                return dataPointsTable;
            }
        }

        private void BuildRowStats(ExoSubJobProjection entity, bool isUpdate)
        {
            if (dataPointsTable == null)
                return;

            DataRow newDataRow;
            if (!isUpdate)
                newDataRow = dataPointsTable.NewRow();
            else
            {
                newDataRow = (from DataRow dr in dataPointsTable.Rows
                              where ((ExoSubJobProjection)dr[columnEntity]).SubJob.Code == entity.SubJob.Code && ((ExoSubJobProjection)dr[columnEntity]).Discipline.Code == entity.Discipline.Code && ((ExoSubJobProjection)dr[columnEntity]).Commodity.Code == entity.Commodity.Code
                              select dr).FirstOrDefault();
            }

            if (newDataRow == null)
                return;

            ExoTimeAuthorisation jobLine = jobLines.FirstOrDefault(x => x.SubJobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code && x.CommodityCode == entity.Commodity.Code);
            if (jobLine != null)
            {
                entity.ExoBudgetQty = jobLine.BudgetQty;
                entity.ExoBudgetCosts = jobLine.BudgetCosts;
            }

            newDataRow[columnEntity] = entity;

            for (int i = 0; i < newDataRow.ItemArray.Count(); i++)
            {
                string columnName = dataPointsTable.Columns[i].ColumnName;
                if (!defaultColumnFieldNames.Any(x => x == columnName))
                    newDataRow[columnName] = 0.00m;
            }

            DashboardFlatStructure findDashboardEntity = SingleProjectDashboards.FirstOrDefault(x => x.SubjobCode == entity.SubJob.Code && x.DisciplineCode == entity.Discipline.Code && x.CommodityCode == entity.Commodity.Code);
            if(findDashboardEntity != null)
            {
                if (findDashboardEntity.Stats.Remaining != null && findDashboardEntity.Stats.Remaining.CumulativeDataPoints != null)
                    foreach (Common.ViewModel.Reporting.DataPoint progress in findDashboardEntity.Stats.Remaining.DataPoints)
                    {
                        string dateField = progress.ProgressDate.Date.ToShortDateString();
                        if (dataPointsTable.Columns.Contains(dateField))
                        {
                            newDataRow[dateField] = progress.Units;
                        }
                    }
            }

            if (!isUpdate)
                dataPointsTable.Rows.Add(newDataRow);
        }
        #endregion

        #region View Events
        public void AutoGeneratingPercentageColumns(AutoGeneratingColumnEventArgs e)
        {
            if (!defaultColumnFieldNames.Any(x => x == e.Column.FieldName))
            {
                DateTime parsedate;
                if (DateTime.TryParse(e.Column.FieldName, out parsedate))
                {
                    if(parsedate < liveDesignProgress.DATA_DATE)
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplatePast"] as DataTemplate;
                    else
                        e.Column.CellTemplate = Application.Current.Resources["forecastTemplateFuture"] as DataTemplate;

                    e.Column.FilterPopupMode = FilterPopupMode.CheckedList;
                }
                else
                {
                    e.Column.Fixed = FixedStyle.Left;
                    e.Column.ReadOnly = true;
                }
            }
            else
                e.Column.Visible = false;
        }
        #endregion
    }
}