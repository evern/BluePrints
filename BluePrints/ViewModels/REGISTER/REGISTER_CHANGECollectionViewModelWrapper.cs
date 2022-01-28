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
using BluePrints.Common.Utils;
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
using DevExpress.Mvvm.UI;
using BaseModel.ViewModel.Dialogs;

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
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.REGISTER_CHANGE_ATTACHMENTS, REGISTER_CHANGE_ATTACHMENTProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<USER>, IQueryable<USER>> USERProjectionFunc()
        {
            return query => query.Where(x => x.LEAVE_DATE == null || x.LEAVE_DATE > DateTime.Now);
        }

        protected virtual Func<IRepositoryQuery<REGISTER_CHANGE_ATTACHMENT>, IQueryable<REGISTER_CHANGE_ATTACHMENT>> REGISTER_CHANGE_ATTACHMENTProjectionFunc()
        {
            return query => query.Where(x => x.REGISTER_CHANGE.GUID_PROJECT == loadPROJECT.GUID);
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
            return query => populateCHANGEReferences(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.NUMBER));
        }

        private IQueryable<REGISTER_CHANGE> populateCHANGEReferences(IQueryable<REGISTER_CHANGE> query)
        {
            List<REGISTER_CHANGE> registerCHANGE = query.ToList();
            //need to call ToList for tokenComboBoxEditSettings to work
            registerCHANGE.ForEach(x => x.Documents = REGISTER_CHANGE_ATTACHMENTCollection.Where(y => y.GUID_REGISTER_CHANGE == x.GUID).ToList());

            return registerCHANGE.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<REGISTER_CHANGE> entities)
        {
            MainViewModel.OnAfterProjectionSavedCallBack = onAfterEntitySaved;
            MainViewModel.BeforeShownEditor = beforeShownEditor;
            MainViewModel.SetParentViewModel(this);
            if (showReport)
                mainThreadDispatcher.BeginInvoke(new Action(() => previewReport(entities)));
            else if(showDesignChangeNotice)
                mainThreadDispatcher.BeginInvoke(new Action(() => exportDesignChangeNotices(entities)));

            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        private void onAfterEntitySaved(REGISTER_CHANGE projection, REGISTER_CHANGE entity, bool isNewEntity)
        {
            saveCHANGEDocument(projection);
        }

        private void saveCHANGEDocument(REGISTER_CHANGE entity)
        {
            if (entity.DocumentAssignments != null)
            {
                List<REGISTER_CHANGE_ATTACHMENT> removeDocuments = new List<REGISTER_CHANGE_ATTACHMENT>();
                foreach (REGISTER_CHANGE_ATTACHMENT document in REGISTER_CHANGE_ATTACHMENTCollection.Where(x => x.GUID_REGISTER_CHANGE == entity.GUID))
                {
                    if (!entity.DocumentAssignments.Any(x => x.GUID == document.GUID))
                        removeDocuments.Add(document);
                }

                REGISTER_CHANGE_ATTACHMENTCollectionViewModel.BaseBulkDelete(removeDocuments);

                List<REGISTER_CHANGE_ATTACHMENT> addDocuments = new List<REGISTER_CHANGE_ATTACHMENT>();
                foreach (REGISTER_CHANGE_ATTACHMENT document in entity.DocumentAssignments)
                {
                    if (document.GUID == Guid.Empty || !entity.REGISTER_CHANGE_ATTACHMENT.Any(x => x.GUID == document.GUID))
                        addDocuments.Add(new REGISTER_CHANGE_ATTACHMENT() { GUID_REGISTER_CHANGE = entity.GUID, ATTACHMENT_PATH = document.ATTACHMENT_PATH, ATTACHMENT_NAME = document.ATTACHMENT_NAME });
                }

                REGISTER_CHANGE_ATTACHMENTCollectionViewModel.BaseBulkSave(addDocuments);
            }
            else
            {
                List<REGISTER_CHANGE_ATTACHMENT> removeDocuments = new List<REGISTER_CHANGE_ATTACHMENT>();
                foreach (REGISTER_CHANGE_ATTACHMENT assignment in REGISTER_CHANGE_ATTACHMENTCollection.Where(x => x.GUID_REGISTER_CHANGE == entity.GUID))
                {
                    removeDocuments.Add(assignment);
                }

                REGISTER_CHANGE_ATTACHMENTCollectionViewModel.BaseBulkDelete(removeDocuments);
            }

        }

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

        private bool beforeShownEditor(EditorEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new REGISTER_CHANGE().CHANGE_PATH))
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

        protected IOpenFileDialogService OpenFileDialogService
        {
            get { return this.GetService<IOpenFileDialogService>(); }
        }

        public void SpecifyPath()
        {
            if (SelectedEntity == null)
            {
                MessageBoxService.ShowMessage("Please select an entry that has already been added");
                return;
            }

            OpenFileDialogService.Filter = "PDF (*.PDF)|*.PDF";
            bool DialogResult;

            DialogResult = OpenFileDialogService.ShowDialog();
            if (DialogResult)
            {
                string fullPath = OpenFileDialogService.File.GetFullName();
                SelectedEntity.CHANGE_PATH = fullPath;

                MainViewModel.Save(SelectedEntity);
                TableViewService.CommitEditing();
                GridControlService.RefreshData();
            }
        }

        public void ClearPath(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
                return;

            if (SelectedEntity == null)
            {
                MessageBoxService.ShowMessage("Please select an added record to specify reference");
                return;
            }

            if (MessageBoxService.ShowMessage("Are you sure you want to clear path for " + SelectedEntity.NUMBER + "?", "Clear Path", MessageButton.OKCancel) == MessageResult.OK)
            {
                SelectedEntity.CHANGE_PATH = null;

                MainViewModel.Save(SelectedEntity);
                TableViewService.CommitEditing();
                GridControlService.RefreshData();
            }
        }

        private DevExpress.Mvvm.IDialogService ImportDocumentsDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("ImportDocumentsDialog"); }
        }

        public void SpecifyReferences()
        {
            if (SelectedEntity == null)
            {
                MessageBoxService.ShowMessage("Please select an entry");
                return;
            }

            ListImportDocumentsViewModel<REGISTER_CHANGE_ATTACHMENT> viewModel = ListImportDocumentsViewModel<REGISTER_CHANGE_ATTACHMENT>.Create(SelectedEntity.DocumentAssignments);
            if (ImportDocumentsDialogService.ShowDialog(MessageButton.OKCancel, string.Empty, "ListImportDocuments", viewModel) == MessageResult.OK)
            {
                List<REGISTER_CHANGE_ATTACHMENT> entityDocuments = (List<REGISTER_CHANGE_ATTACHMENT>)SelectedEntity.Documents;
                entityDocuments.Clear();
                entityDocuments.AddRange(viewModel.GetSelectedDocuments());
                MainViewModel.Save(SelectedEntity);
                TableViewService.CommitEditing();
            }
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
        XtraReportChangeNotice rptChangeNotice;
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
            foreach (REGISTER_CHANGE registerChange in registerChanges)
            {
                if (selectedDesignChangeNoticeGuids.Any(x => x == registerChange.GUID))
                {
                    //argument is passed into the report as collection that contains a single element, because each report is only showing a single record
                    List<REGISTER_CHANGE> exportRegisterChange = new List<REGISTER_CHANGE>();
                    exportRegisterChange.Add(registerChange);
                    rptChangeNotice.AssignProperties(loadPROJECT, exportRegisterChange);
                    //DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
                    //previewWindow.PreviewControl.DocumentSource = rptChangeNotice;
                    //previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    //previewWindow.WindowState = WindowState.Maximized;
                    rptChangeNotice.RequestParameters = false;
                    //rptChangeNotice.CreateDocument(true);
                    //previewWindow.Show();

                    SaveFileDialogService.DefaultExt = "docx";
                    SaveFileDialogService.Filter = "Word Files (.docx)|*.docx|All Files (*.*)|*.*";
                    string fileName = string.Concat(loadPROJECT.NUMBER, BluePrintsResources.Register_Change_Suffix, registerChange.NUMBER);
                    SaveFileDialogService.Title = "Save Change Notice " + registerChange.NUMBER;
                    SaveFileDialogService.DefaultFileName = fileName;

                    if (SaveFileDialogService.ShowDialog())
                    {
                        //export to desktop routine
                        fileInfo = SaveFileDialogService.File;
                        rptChangeNotice.AssignProperties(loadPROJECT, exportRegisterChange);
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

        protected override string GetEntityNumberFieldName()
        {
            throw new NotImplementedException();
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

        public IEnumerable<REGISTER_CHANGE_ATTACHMENT> REGISTER_CHANGE_ATTACHMENTCollection
        {
            get
            {
                return GetEntities<REGISTER_CHANGE_ATTACHMENT>();
            }
        }

        public CollectionViewModel<REGISTER_CHANGE_ATTACHMENT, REGISTER_CHANGE_ATTACHMENT, Guid, IBluePrintsEntitiesUnitOfWork> REGISTER_CHANGE_ATTACHMENTCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return (CollectionViewModel<REGISTER_CHANGE_ATTACHMENT, REGISTER_CHANGE_ATTACHMENT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<REGISTER_CHANGE_ATTACHMENT>();
            }
        }

        #endregion
    }
}