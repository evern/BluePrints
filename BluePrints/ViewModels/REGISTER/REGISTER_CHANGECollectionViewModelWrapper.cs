using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    public class REGISTER_CHANGECollectionViewModelWrapper :
        BluePrintsEntitiesAutoNumberCollectionWrapper
        <REGISTER_CHANGE, REGISTER_CHANGE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of REGISTERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static REGISTER_CHANGECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new REGISTER_CHANGECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the REGISTERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the REGISTERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected REGISTER_CHANGECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;
        int defaultNumericFieldLengthForRegisters;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            defaultNumericFieldLengthForRegisters = Int32.Parse(BluePrintsResources.Default_Register_Numeric_Length);
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.USERS, USERProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<USER>, IQueryable<USER>> USERProjectionFunc()
        {
            return query => query.Where(x => x.LEAVE_DATE == null || x.LEAVE_DATE > DateTime.Now);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && (x.REPORT_TYPE == ReportType.Change_Register.ToString() || x.REPORT_TYPE == ReportType.Change_Notice.ToString()));
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.REGISTER_CHANGE);
        }

        protected override Func<IRepositoryQuery<REGISTER_CHANGE>, IQueryable<REGISTER_CHANGE>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.NUMBER);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<REGISTER_CHANGE> entities)
        {
            MainViewModel.SetParentViewModel(this);
            if (showReport)
                mainThreadDispatcher.BeginInvoke(new Action(() => previewReport(entities)));
            else if(showDesignChangeNotice)
                mainThreadDispatcher.BeginInvoke(new Action(() => exportDesignChangeNotices(entities)));

            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        public override void UnifiedNewRowInitializationFromView(REGISTER_CHANGE projection)
        {
            if(LoginCredentials.CurrentUser.GUID != Guid.Empty)
                projection.GUID_RAISEDBY = LoginCredentials.CurrentUser.GUID;

            base.UnifiedNewRowInitializationFromView(projection);
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(REGISTER_CHANGE projection, out bool isNew)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
            if (projection.GUID == Guid.Empty && projection.DATE_RAISED == null)
                projection.DATE_RAISED = DateTime.Now.Date;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        public override string UnifiedRowValidation(REGISTER_CHANGE projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(REGISTER_CHANGE projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_CHANGE().DATE_CLOSED))
            {
                DateTime? dateClosed = (DateTime?)new_value;
                if (projection.DATE_RAISED != null && dateClosed != null && projection.DATE_RAISED > dateClosed)
                    return "Date closed cannot be earlier than date raised";
            }

            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_CHANGE().DATE_RAISED))
            {
                DateTime? dateRaised = (DateTime?)new_value;
                if (projection.DATE_CLOSED != null && dateRaised != null && dateRaised > projection.DATE_CLOSED)
                    return "Date raised cannot be later than date closed";
            }

            return string.Empty;
        }
        #endregion

        #endregion

        #region IEntityNumber
        protected override string GetEntityNumberFieldName()
        {
            return BindableBase.GetPropertyName(() => new REGISTER_CHANGE().NUMBER);
        }

        protected override int DefaultNumericFieldLength()
        {
            return Int32.Parse(BluePrintsResources.Default_Register_Numeric_Length);
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "REGISTER_CHANGECollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "REGISTER_CHANGECollectionViewModelWrapper_v2"; }
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

        protected override string ExportFilename()
        {
            return loadPROJECT.NUMBER + "_Register_Change";
        }

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT,
                (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Change_Register);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public void EditNotice()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT,
                (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Change_Notice);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        bool showReport = false;
        XtraReportChangeRegister rptChangeRegister;
        XtraReportChangeRegisterStandalone rptChangeNotice;
        public void ViewReport()
        {
            showReport = true;
            //to make sure all navigational properties are loaded
            FullRefresh();
        }

        private void previewReport(IEnumerable<REGISTER_CHANGE> registerChanges)
        {
            LoadingScreenManager.ShowLoadingScreen(1);
            showReport = false;
            rptChangeRegister = new XtraReportChangeRegister();
            PROJECT_REPORT dbProjectReport = PROJECT_REPORTCollection.FirstOrDefault(x => x.REPORT_TYPE == ReportType.Change_Register.ToString());
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    rptChangeRegister.LoadLayout(sw.BaseStream);
                }
            }

            //set paperkind depending on project location
            if (loadPROJECT.OFFICE.NAME.ToUpper().Contains("PERTH"))
                rptChangeRegister.PaperKind = System.Drawing.Printing.PaperKind.A3;
            else
                rptChangeRegister.PaperKind = System.Drawing.Printing.PaperKind.Tabloid;

            rptChangeRegister.AssignProperties(loadPROJECT, registerChanges);
            DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = rptChangeRegister;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            rptChangeRegister.RequestParameters = false;
            rptChangeRegister.CreateDocument(true);
            LoadingScreenManager.CloseLoadingScreen();
            previewWindow.Show();
        }

        bool showDesignChangeNotice = false;
        List<Guid> selectedDesignChangeNoticeGuids = new List<Guid>();
        public void ExportDesignChangeNotice()
        {
            showDesignChangeNotice = true;
            selectedDesignChangeNoticeGuids.AddRange(SelectedEntities.Select(x => x.GUID).ToList());
            //to make sure all navigational properties are loaded
            FullRefresh();
        }

        private void exportDesignChangeNotices(IEnumerable<REGISTER_CHANGE> registerChanges)
        {
            showDesignChangeNotice = false;
            rptChangeNotice = new XtraReportChangeRegisterStandalone();
            PROJECT_REPORT dbProjectReport = PROJECT_REPORTCollection.FirstOrDefault(x => x.REPORT_TYPE == ReportType.Change_Notice.ToString());
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    rptChangeNotice.LoadLayout(sw.BaseStream);
                }
            }

            //string ResultPath = string.Empty;
            //int exportCount = 0;
            //if (FolderBrowserDialogService.ShowDialog())
            //{
                foreach(REGISTER_CHANGE registerChange in registerChanges)
                {
                    if (selectedDesignChangeNoticeGuids.Any(x => x == registerChange.GUID))
                    {
                        //argument is passed into the report as collection that contains a single element, because each report is only showing a single record
                        List<REGISTER_CHANGE> exportRegisterChange = new List<REGISTER_CHANGE>();
                        exportRegisterChange.Add(registerChange);
                        rptChangeNotice.AssignProperties(loadPROJECT, exportRegisterChange);
                        DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
                        previewWindow.PreviewControl.DocumentSource = rptChangeNotice;
                        previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        previewWindow.WindowState = WindowState.Maximized;
                        rptChangeNotice.RequestParameters = false;
                        rptChangeNotice.CreateDocument(true);
                        previewWindow.Show();

                        //export to desktop routine

                        //ResultPath = FolderBrowserDialogService.ResultPath;
                        //rptChangeNotice.AssignProperties(loadPROJECT, exportRegisterChange);
                        //rptChangeNotice.CreateDocument();
                        //string fileName = loadPROJECT.NUMBER + "_ChangeNotice_" + registerChange.NUMBER + ".pdf";
                        //try
                        //{
                        //    rptChangeNotice.ExportToPdf(ResultPath + "\\" + fileName);
                        //    exportCount += 1;
                        //}
                        //catch
                        //{
                        //    MessageBoxService.ShowMessage("Cannot export " + fileName + " because it is in use");
                        //}
                    }
                //}

                selectedDesignChangeNoticeGuids.Clear();
                //MessageBoxService.ShowMessage("Exported " + exportCount + " reports to " + ResultPath);
            }
        }

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<PROJECT_REPORT> PROJECT_REPORTCollection
        {
            get
            {
                return GetEntities<PROJECT_REPORT>();
            }
        }
        #endregion
    }
}