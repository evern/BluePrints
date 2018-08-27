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
        protected override void resolveParameters(object parameter)
        {
            base.resolveParameters(parameter);
            IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            jobLines = ExoQueries.GetProjectLines(primeroUnitOfWork, loadPROJECT.NUMBER);
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
                    PROGRESS earliestProgress;
                    if (constructDataDate < designDataDate)
                        earliestProgress = liveConstructProgress;
                    else
                        earliestProgress = liveDesignProgress;

                    dataPointsTable = new DataTable();
                    TimeSpan interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(earliestProgress);
                    DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(earliestProgress);
                    DateTime lastDataDate = earliestProgress.DATA_DATE.AddDays(-1 * interval.Days);
                    IEnumerable<DateTime> alignedDataDateCollection = ChronologicalHelpers.GenerateAlignedDatesCollection(firstAlignedDataDate, lastDataDate, interval);

                    dataPointsTable.Columns.Add(columnEntity, typeof(DashboardFlatStructure));

                    //bool conditionalFormattingAdded = false;
                    foreach (DateTime alignedDataDate in alignedDataDateCollection)
                    {
                        ColorScaleFormatCondition colorScaleFormatCondition = new ColorScaleFormatCondition();
                        string columnFieldName = alignedDataDate.Date.ToShortDateString();
                        //if (!conditionalFormattingAdded)
                        //{
                        colorScaleFormatCondition.FieldName = columnFieldName;
                        colorScaleFormatCondition.Format = new ColorScaleFormat() { ColorMin = Colors.LightSalmon, ColorMiddle = Colors.LemonChiffon, ColorMax = Colors.Lime };
                        colorScaleFormatCondition.MinValue = 0;
                        colorScaleFormatCondition.MaxValue = 1;
                        TableViewService.AddFormatCondition(colorScaleFormatCondition);
                        //    conditionalFormattingAdded = true;
                        //}

                        if (alignedDataDate == earliestProgress.DATA_DATE)
                        {
                            DataColumn lastColumn = new DataColumn();
                            lastColumn.ColumnName = columnFieldName;
                            lastColumn.DataType = typeof(decimal);
                            dataPointsTable.Columns.Add(lastColumn);
                        }
                        else
                            dataPointsTable.Columns.Add(columnFieldName, typeof(decimal));
                    }

                    foreach (DashboardFlatStructure entity in SingleProjectDashboards)
                    {
                        BuildRowStats(entity, false);
                    }

                    TableViewService.ScrollToLast();
                }

                return dataPointsTable;
            }
        }

        private void BuildRowStats(DashboardFlatStructure entity, bool isUpdate)
        {
            if (dataPointsTable == null)
                return;

            DataRow newDataRow;
            if (!isUpdate)
                newDataRow = dataPointsTable.NewRow();
            else
            {
                newDataRow = (from DataRow dr in dataPointsTable.Rows
                              where ((DashboardFlatStructure)dr[columnEntity]).SubjobCode == entity.SubjobCode
                              select dr).FirstOrDefault();
            }

            if (newDataRow == null)
                return;

            ExoTimeAuthorisation jobLine = jobLines.FirstOrDefault(x => x.SubJobCode == entity.SubjobCode);
            if(jobLine != null)
            {
                entity.Stats.ExoBudgetQty = jobLine.BudgetQty;
                entity.Stats.ExoBudgetCosts = jobLine.BudgetCosts;
            }

            newDataRow[columnEntity] = entity;

            //for (int i = 0; i < newDataRow.ItemArray.Count(); i++)
            //{
            //    string columnName = dataPointsTable.Columns[i].ColumnName;
            //    if (!defaultColumnFieldNames.Any(x => x == columnName))
            //        newDataRow[columnName] = 0.00m;
            //}

            if (entity.Stats.Earned != null && entity.Stats.Earned.CumulativeDataPoints != null)
                foreach (Common.ViewModel.Reporting.DataPoint progress in entity.Stats.Remaining.DataPoints)
                {
                    string dateField = progress.ProgressDate.Date.ToShortDateString();
                    if (dataPointsTable.Columns.Contains(dateField))
                    {
                        newDataRow[dateField] = progress.Units;
                    }
                }

            if (!isUpdate)
                dataPointsTable.Rows.Add(newDataRow);
        }
        #endregion
    }
}