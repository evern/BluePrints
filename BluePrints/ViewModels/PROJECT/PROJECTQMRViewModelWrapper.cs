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
using DevExpress.Xpf.Printing;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class PROJECTQMRViewModelWrapper :
        PROJECTViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public new static PROJECTQMRViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROJECTQMRViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected PROJECTQMRViewModelWrapper()
        {
            switchChartOnly = true;
        }


        public bool CanViewReportLookAhead()
        {
            return SummaryEntity != null && SummaryEntity.Stats != null;
        }

        public void ViewReportLookAhead()
        {
            SummaryStats displaySummary = SummaryEntity.Stats as SummaryStats;
            if (displaySummary == null)
                return;

            var progressReport = new XtraReportDashboardLookahead();
            //loadReportLayoutFromDatabase(progressReport);

            string title = string.Empty;
            PROJECT_Dashboard project_dashboard = DisplaySelectedEntity as PROJECT_Dashboard;
            if (project_dashboard != null)
                title = project_dashboard.Entity.NAME;

            var dateFromToViewModel = DateFromToDialogViewModel.Create();
            var collection = GetEntities<PROGRESS>();


            DateTime endDate;
            if (designDataDate == null && constructDataDate == null)
                endDate = DateTime.Now;
            else if (designDataDate == null)
                endDate = (DateTime)constructDataDate;
            else if (constructDataDate == null)
                endDate = (DateTime)designDataDate;
            else
            {
                if (designDataDate > constructDataDate)
                    endDate = (DateTime)designDataDate;
                else
                    endDate = (DateTime)constructDataDate;
            }

            //start showing after data date
            endDate = endDate.Date.AddDays(22);

            SummaryStats filteredSummary = new SummaryStats(displaySummary, endDate);
            progressReport.AssignProperties(filteredSummary, filteredSummary.ReportingDataDate, title);
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = progressReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            progressReport.RequestParameters = false;
            progressReport.CreateDocument(true);
            previewWindow.Show();
        }
    }
}