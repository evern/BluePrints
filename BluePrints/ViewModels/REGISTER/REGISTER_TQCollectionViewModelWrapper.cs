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
using BaseModel.ViewModel.Document;
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
using DevExpress.XtraPrinting;
using System.Diagnostics;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class REGISTER_TQCollectionViewModelWrapper :
        BluePrintsEntitiesAutoNumberCollectionWrapper
        <REGISTER_TQ, REGISTER_TQ, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of REGISTERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static REGISTER_TQCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new REGISTER_TQCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the REGISTERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the REGISTERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected REGISTER_TQCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;
        int defaultNumericFieldLengthForRegisters;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        DispatcherTimer delayedPathSelectionTimer;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            defaultNumericFieldLengthForRegisters = Int32.Parse(BluePrintsResources.Default_Register_Numeric_Length);
            delayedPathSelectionTimer = new DispatcherTimer();
            delayedPathSelectionTimer.Interval = new TimeSpan(0, 0, 0, 1);
            delayedPathSelectionTimer.Tick += DelayedPathSelectionTimer_Tick;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.USERS, USERProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.REGISTER_CLARIFICATIONS, REGISTER_CLARIFICATIONProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
        }

        protected virtual Func<IRepositoryQuery<USER>, IQueryable<USER>> USERProjectionFunc()
        {
            return query => query.Where(x => x.LEAVE_DATE == null || x.LEAVE_DATE > DateTime.Now);
        }

        protected virtual Func<IRepositoryQuery<REGISTER_CLARIFICATION>, IQueryable<REGISTER_CLARIFICATION>> REGISTER_CLARIFICATIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
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
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.REGISTER_TQ);
        }

        protected override Func<IRepositoryQuery<REGISTER_TQ>, IQueryable<REGISTER_TQ>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.NUMBER);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<REGISTER_TQ> entities)
        {
            MainViewModel.BeforeShownEditor = beforeShownEditor;
            MainViewModel.SetParentViewModel(this);
            if (showReport)
                mainThreadDispatcher.BeginInvoke(new Action(() => previewReport(entities)));

            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        public override void UnifiedNewRowInitializationFromView(REGISTER_TQ projection)
        {
            if(LoginCredentials.CurrentUser.GUID != Guid.Empty)
                projection.RAISEDBY = LoginCredentials.CurrentUser.Full_Name;

            base.UnifiedNewRowInitializationFromView(projection);
        }

        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(REGISTER_TQ projection, out bool isNew)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
            if (projection.GUID == Guid.Empty && projection.DATE_RAISED == null)
                projection.DATE_RAISED = DateTime.Now.Date;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        public override string UnifiedRowValidation(REGISTER_TQ projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(REGISTER_TQ projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_TQ().DATE_RESPONDED))
            {
                DateTime? dateClosed = (DateTime?)new_value;
                if (projection.DATE_RAISED != null && dateClosed != null && projection.DATE_RAISED > dateClosed)
                    return "Date responded cannot be earlier than date raised";
            }

            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_TQ().DATE_RAISED))
            {
                DateTime? dateRaised = (DateTime?)new_value;
                if (projection.DATE_RESPONDED != null && dateRaised != null && dateRaised > projection.DATE_RESPONDED)
                    return "Date responded cannot be later than date closed";
            }

            return string.Empty;
        }
        #endregion

        #endregion

        #region IEntityNumber
        protected override string GetEntityNumberFieldName()
        {
            return BindableBase.GetPropertyName(() => new REGISTER_TQ().NUMBER);
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
            //get { return "REGISTER_TQCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "REGISTER_TQCollectionViewModelWrapper_v2"; }
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
            return loadPROJECT.NUMBER + "_REGISTER_TQ";
        }

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.TQ_Register);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public void EditNotice()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Change_Notice);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void SendToClarificationRegister()
        {
            if (SelectedEntity == null)
                return;

            if(!REGISTER_CLARIFICATIONCollection.Any(x => x.TQ_NUMBER == SelectedEntity.NUMBER))
            {
                var editingEntity = SelectedEntity;
                REGISTER_CLARIFICATION newRegister = new REGISTER_CLARIFICATION();
                newRegister.GUID_PROJECT = loadPROJECT.GUID;
                newRegister.TQ_NUMBER = editingEntity.NUMBER;
                newRegister.TQ_PATH = editingEntity.TQ_PATH;
                newRegister.CREATED = DateTime.Now;
                newRegister.CREATEDBY = LoginCredentials.CurrentUserGuid;
                REGISTER_CLARIFICATIONCollectionViewModel.Save(newRegister);
            }

            DocumentInfo DocumentInfo = new DocumentInfo("View_ClarificationRegister" + loadPROJECT.GUID.ToString(),
                new EntitiesParameter<PROJECT>(loadPROJECT),
                    "REGISTER_CLARIFICATIONCollectionView",
                    "[" + loadPROJECT.NUMBER + "] Clarification Register");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        bool showReport = false;
        XtraReportTQRegister rptTQRegister;
        XtraReportChangeNotice rptChangeNotice;
        public void ViewReport()
        {
            showReport = true;
            //to make sure all navigational properties are loaded
            FullRefresh();
        }

        private void previewReport(IEnumerable<REGISTER_TQ> registerTQ)
        {
            LoadingScreenManager.ShowLoadingScreen(1);
            showReport = false;
            rptTQRegister = new XtraReportTQRegister();
            PROJECT_REPORT dbProjectReport = PROJECT_REPORTCollection.FirstOrDefault(x => x.REPORT_TYPE == ReportType.TQ_Register.ToString());
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    rptTQRegister.LoadLayout(sw.BaseStream);
                }
            }

            //set paperkind depending on project location
            if (loadPROJECT.OFFICE.NAME.ToUpper().Contains("PERTH"))
                rptTQRegister.PaperKind = System.Drawing.Printing.PaperKind.A3;
            else
                rptTQRegister.PaperKind = System.Drawing.Printing.PaperKind.Tabloid;

            rptTQRegister.AssignProperties(loadPROJECT, registerTQ);
            DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = rptTQRegister;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            rptTQRegister.RequestParameters = false;
            rptTQRegister.CreateDocument(true);
            LoadingScreenManager.CloseLoadingScreen();
            previewWindow.Show();
        }

        protected IOpenFileDialogService OpenFileDialogService
        {
            get { return this.GetService<IOpenFileDialogService>(); }
        }

        private bool beforeShownEditor(EditorEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new REGISTER_TQ().TQ_PATH))
                delayedPathSelectionTimer.Start();

            return true;
        }

        private void DelayedPathSelectionTimer_Tick(object sender, EventArgs e)
        {
            delayedPathSelectionTimer.Stop();
            SpecifyPath();
        }

        public bool CanSpecifyPath()
        {
            return SelectedEntity != null;
        }

        public void SpecifyPath()
        {
            OpenFileDialogService.Filter = "PDF (*.PDF)|*.PDF";
            bool DialogResult;

            DialogResult = OpenFileDialogService.ShowDialog();
            if (DialogResult)
            {
                string fullPath = OpenFileDialogService.File.GetFullName();
                SelectedEntity.TQ_PATH = fullPath;
                MainViewModel.Save(SelectedEntity);
                GridControlService.RefreshData();
            }
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

        private void exportTQNotices(IEnumerable<REGISTER_TQ> registerChanges)
        {
            showDesignChangeNotice = false;
            rptChangeNotice = new XtraReportChangeNotice();
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

            IFileInfo fileInfo = null;
            foreach (REGISTER_TQ registerChange in registerChanges)
            {
                if (selectedDesignChangeNoticeGuids.Any(x => x == registerChange.GUID))
                {
                    //argument is passed into the report as collection that contains a single element, because each report is only showing a single record
                    List<REGISTER_TQ> exportRegisterChange = new List<REGISTER_TQ>();
                    exportRegisterChange.Add(registerChange);
                    //rptChangeNotice.AssignProperties(loadPROJECT, exportRegisterChange);
                    //DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
                    //previewWindow.PreviewControl.DocumentSource = rptChangeNotice;
                    //previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    //previewWindow.WindowState = WindowState.Maximized;
                    rptChangeNotice.RequestParameters = false;
                    //rptChangeNotice.CreateDocument(true);
                    //previewWindow.Show();

                    SaveFileDialogService.DefaultExt = "docx";
                    SaveFileDialogService.Filter = "Word Files (.docx)|*.docx|All Files (*.*)|*.*";
                    string fileName = string.Concat(loadPROJECT.NUMBER, BluePrintsResources.Register_TQ_Suffix, registerChange.NUMBER);
                    SaveFileDialogService.Title = "Save Change Notice " + registerChange.NUMBER;
                    SaveFileDialogService.DefaultFileName = fileName;

                    if (SaveFileDialogService.ShowDialog())
                    {
                        //export to desktop routine
                        fileInfo = SaveFileDialogService.File;
                        //rptChangeNotice.AssignProperties(loadPROJECT, exportRegisterChange);
                        rptChangeNotice.CreateDocument();
                        
                        try
                        {
                            DocxExportOptions DocxExportOptions = new DocxExportOptions();
                            DocxExportOptions.ExportMode = DocxExportMode.SingleFile;
                            DocxExportOptions.TableLayout = true;
                            string exportPath = SaveFileDialogService.GetFullFileName();
                            rptChangeNotice.ExportToDocx(exportPath, DocxExportOptions);

                            Process.Start(exportPath);
                        }
                        catch
                        {
                            MessageBoxService.ShowMessage("Cannot export " + fileName + " because it is in use");
                        }

                    }
                }
            }

            selectedDesignChangeNoticeGuids.Clear();
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

        public IEnumerable<string> USERStrCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection.Select(x => x.Full_Name);
            }
        }

        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<REGISTER_CLARIFICATION> REGISTER_CLARIFICATIONCollection
        {
            get
            {
                var collection = GetEntities<REGISTER_CLARIFICATION>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.TQ_NUMBER);
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

        public CollectionViewModel<REGISTER_CLARIFICATION, REGISTER_CLARIFICATION, Guid, IBluePrintsEntitiesUnitOfWork> REGISTER_CLARIFICATIONCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<REGISTER_CLARIFICATION, REGISTER_CLARIFICATION, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<REGISTER_CLARIFICATION>();
            }
        }
        #endregion
    }
}