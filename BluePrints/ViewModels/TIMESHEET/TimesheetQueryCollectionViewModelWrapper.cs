using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.UndoRedo;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6Data;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using DevExpress.XtraEditors.DXErrorProvider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROGRESS object view model.
    /// </summary>
    public partial class TimesheetQueryCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<X_JOB_TIMESHEETS, X_JOB_TIMESHEETS, Guid, IPrimeroEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static TimesheetQueryCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new TimesheetQueryCollectionViewModelWrapper());
        }

        protected override string readOnlyMessage => "Cells are read only because you do not have authority to edit transactions";
        protected TimesheetQueryCollectionViewModelWrapper()
        {
        }

        #region Database Operation
        private Data.PROJECT loadPROJECT;
        private JOBCOST_HDR loadJOBCOST_HDR;
        private readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo);
        }

        public FilterTreeViewModel<BASELINE_ITEMProgress, Guid> FilterTreeViewModel { get; set; }
        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<JOBCOST_RESOURCE, JOBCOST_RESOURCE, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOBCOST_RESOURCE);
            loaderCollection.AddLoaderDescription<JOB_COSTGROUPS, JOB_COSTGROUPS, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTGROUPS);
            loaderCollection.AddLoaderDescription<JOB_COSTTYPES, JOB_COSTTYPES, int, IPrimeroEntitiesUnitOfWork>(primeroUnitOfWorkFactory, x => x.JOB_COSTTYPES);
            loaderCollection.AddLoaderDescription(primeroUnitOfWorkFactory, x => x.JOBCOST_HDR, JOBCOST_HDRProjectionFunc, x => loadJOBCOST_HDR = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
        }

        private Func<IRepositoryQuery<JOBCOST_HDR>, IQueryable<JOBCOST_HDR>> JOBCOST_HDRProjectionFunc()
        {
            return query => query.Where(x => x.JOBCODE.Contains(loadPROJECT.NUMBER.ToString()));
        }

        protected virtual Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Timesheet_Report.ToString());
        }

        public ObservableCollection<JOB_TRANSACTIONS> JOB_TRANSACTIONS = new ObservableCollection<JOB_TRANSACTIONS>();
        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.X_JOB_TIMESHEETS);
        }

        protected override Func<IRepositoryQuery<X_JOB_TIMESHEETS>, IQueryable<X_JOB_TIMESHEETS>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.MASTER_JOBCODE == loadPROJECT.NUMBER.ToString());
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<X_JOB_TIMESHEETS> entities)
        {
            MainViewModel.AlwaysSkipMessage = true;
            MainViewModel.IsPersistentView = true;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region View Properties
        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, senderKey, isBulkRefresh);
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "OffsiteDirectProgressViewModelWrapper" + view_project_specific_affix; }
            get { return "TimesheetQueryCollectionViewModelWrapper_v1" + view_project_specific_affix; }
        }

        private DevExpress.Mvvm.IDialogService DateFromToDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("DateFromToDialogService"); }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
        }

        public bool CanEditReport()
        {
            if (IsLoading || MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public bool CanViewReport()
        {
            if (IsLoading || MainViewModel == null || MainViewModel.Entities.Count == 0 || SelectedEntities.Count == 0)
                return false;

            return true;
        }

        public bool CanExportSelected()
        {
            return CanViewReport();
        }

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Timesheet_Report);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public void ExportSelected()
        {
            if (FolderBrowserDialogService.ShowDialog())
            {
                string resultPath = FolderBrowserDialogService.ResultPath;
                var groupByVariationTimesheets = SelectedEntities.GroupBy(x => x.X_VARIATIONCODE).Select(group => new { VariationCode = group.Key, Group = group.ToList() });

                XtraReportTimesheet timesheetReport = new XtraReportTimesheet();
                var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
                if (dbProjectReport != null)
                {
                    var reportString = dbProjectReport.REPORT.ToString();
                    using (var sw = new StreamWriter(new MemoryStream()))
                    {
                        sw.Write(reportString);
                        sw.Flush();
                        timesheetReport.LoadLayout(sw.BaseStream);
                    }
                }

                LoadingScreenManager.ShowLoadingScreen(groupByVariationTimesheets.Count());
                LoadingScreenManager.SetMessage("Exporting...");
                string exportPath;
                foreach (var groupByVariationTimesheet in groupByVariationTimesheets)
                {
                    string exportDirectoryPath = resultPath + "\\" + groupByVariationTimesheet.VariationCode;
                    if (!Directory.Exists(exportDirectoryPath))
                        Directory.CreateDirectory(exportDirectoryPath);

                    var groupByWeekTimesheets = groupByVariationTimesheet.Group.GroupBy(x => x.Day1Date).Select(group => new { Day1Date = group.Key, Group = group.ToList() });
                    foreach(var groupByWeekTimesheet in groupByWeekTimesheets)
                    {
                        var groupByResourceTimesheets = groupByWeekTimesheet.Group.GroupBy(x => x.RESOURCENAME).Select(group => new { ResourceName = group.Key, Group = group.ToList() });
                        foreach(var groupByResourceTimesheet in groupByResourceTimesheets)
                        {
                            exportPath = exportDirectoryPath + "\\TS" + groupByWeekTimesheet.Day1Date.ToString("yyyyMMdd") + "-" + groupByResourceTimesheet.ResourceName + "-08708" + groupByVariationTimesheet.VariationCode + ".pdf";
                            exportTimesheet(timesheetReport, groupByResourceTimesheet.Group, exportPath);
                        }
                    }

                    exportPath = exportDirectoryPath + "\\" + groupByVariationTimesheet.VariationCode + ".pdf";
                    exportTimesheet(timesheetReport, groupByVariationTimesheet.Group, exportPath);
                    LoadingScreenManager.Progress();
                }

                LoadingScreenManager.CloseLoadingScreen();
            }
        }

        private void exportTimesheet(XtraReportTimesheet xtraReportTimesheet, IEnumerable<X_JOB_TIMESHEETS> timesheets, string exportPath)
        {
            xtraReportTimesheet.AssignProperties(timesheets);
            xtraReportTimesheet.RequestParameters = false;
            xtraReportTimesheet.CreateDocument(true);

            xtraReportTimesheet.ExportToPdf(exportPath);
        }

        public void ViewReport()
        {
            LoadingScreenManager.ShowLoadingScreen(1);
            XtraReportTimesheet timesheetReport = new XtraReportTimesheet();
            var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    timesheetReport.LoadLayout(sw.BaseStream);
                }
            }

            //make sure disciplines are all populated
            IEnumerable<object> gridVisibleRows = GridControlService.GetVisibleRowObjects();
            timesheetReport.AssignProperties(SelectedEntities);
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = timesheetReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            timesheetReport.RequestParameters = false;
            timesheetReport.CreateDocument(true);
            LoadingScreenManager.CloseLoadingScreen();
            previewWindow.Show();
        }
        #endregion

        public override string UnifiedValueValidation(X_JOB_TIMESHEETS projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(X_JOB_TIMESHEETS projection)
        {
            return string.Empty;
        }

        public IEnumerable<JOBCOST_HDR> JOBCOST_HDRCollection
        {
            get
            {
                var collection = GetEntities<JOBCOST_HDR>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.JOBCODE);
                return collection;
            }
        }

        public IEnumerable<JOBCOST_RESOURCE> JOBCOST_RESOURCECollection
        {
            get
            {
                var collection = GetEntities<JOBCOST_RESOURCE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.RESOURCENAME);
                return collection;
            }
        }

        public IEnumerable<JOB_COSTGROUPS> JOB_COSTGROUPSCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTGROUPS>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SHORTCODE);
                return collection;
            }
        }

        public IEnumerable<X_JOB_TIMESHEETS> X_JOB_TRANSACTIONS_DETAILCollection
        {
            get
            {
                return GetEntities<X_JOB_TIMESHEETS>();
            }
        }

        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPESCollection
        {
            get
            {
                var collection = GetEntities<JOB_COSTTYPES>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.SHORTCODE);
                return collection;
            }
        }
    }
}

