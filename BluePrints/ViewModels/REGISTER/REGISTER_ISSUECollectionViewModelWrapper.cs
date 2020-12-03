using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace BluePrints.ViewModels
{
    public class REGISTER_ISSUECollectionViewModelWrapper :
        BluePrintsEntitiesAutoNumberCollectionWrapper
        <REGISTER_ISSUE, REGISTER_ISSUE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of REGISTERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static REGISTER_ISSUECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new REGISTER_ISSUECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the REGISTERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the REGISTERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected REGISTER_ISSUECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;
        DispatcherTimer delayedRefreshTimer;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();

            delayedRefreshTimer = new DispatcherTimer();
            delayedRefreshTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            delayedRefreshTimer.Tick += DelayedRefreshTimer_Tick;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.REGISTER_CHANGE, REGISTER_CHANGEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.REGISTER_HOLD, REGISTER_HOLDProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.USERS, USERProjectionFunc);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<USER>, IQueryable<USER>> USERProjectionFunc()
        {
            return query => query.Where(x => x.LEAVE_DATE == null);
        }

        private Func<IRepositoryQuery<REGISTER_CHANGE>, IQueryable<REGISTER_CHANGE>> REGISTER_CHANGEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<REGISTER_HOLD>, IQueryable<REGISTER_HOLD>> REGISTER_HOLDProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.REGISTER_ISSUE);
        }

        protected override Func<IRepositoryQuery<REGISTER_ISSUE>, IQueryable<REGISTER_ISSUE>> specifyMainViewModelProjection()
        {
            return query => constructMainViewModelProjection(query);
        }

        private IQueryable<REGISTER_ISSUE> constructMainViewModelProjection(IQueryable<REGISTER_ISSUE> query)
        {
            List<REGISTER_ISSUE> registerIssue = query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).OrderBy(x => x.NUMBER).ToList();
            registerIssue.ForEach(x => x.SetRegisterChange(REGISTER_CHANGECollection));
            registerIssue.ForEach(x => x.SetRegisterHold(REGISTER_HOLDCollection));
            return registerIssue.AsQueryable();
        }

        protected virtual Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Issues_Register.ToString());
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<REGISTER_ISSUE> entities)
        {
            MainViewModel.SetParentViewModel(this);
            if (showReport)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => previewReport(entities)));
                showReport = false;
            }

            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override void OnAfterAuxiliaryEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            if (changedType == typeof(REGISTER_CHANGE) || changedType == typeof(REGISTER_HOLD))
            {
                FullRefreshWithoutClearingUndoRedo();
                return;
            }

            base.OnAfterAuxiliaryEntitiesChanged(key, changedType, messageType, sender, senderKey, isBulkRefresh);
        }

        #region Collection Call Backs
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(REGISTER_ISSUE projection, out bool isNew)
        {
            if (projection.GUID == Guid.Empty && projection.DATE_RAISED == null)
                projection.DATE_RAISED = DateTime.Now;

            projection.GUID_PROJECT = loadPROJECT.GUID;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        public override string UnifiedRowValidation(REGISTER_ISSUE projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(REGISTER_ISSUE projection, string field_name, object new_value, bool isPaste)
        {
            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_ISSUE().DATE_CLOSED))
            {
                DateTime? dateClosed = (DateTime?)new_value;
                if (projection.DATE_RAISED != null && dateClosed != null && ((DateTime)projection.DATE_RAISED).Date > ((DateTime)dateClosed).Date)
                    return "Date closed cannot be earlier than date raised";
            }

            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_ISSUE().DATE_RAISED))
            {
                DateTime? dateRaised = (DateTime?)new_value;
                if (projection.DATE_CLOSED != null && dateRaised != null && ((DateTime)dateRaised).Date > ((DateTime)projection.DATE_CLOSED).Date)
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
        protected IDocumentManagerService DocumentManagerService
        {
            get { return this.GetService<IDocumentManagerService>(); }
        }

        public void HoldRegister()
        {
            DocumentInfo DocumentInfo = new DocumentInfo("View_HoldRegister" + loadPROJECT.GUID.ToString(),
            new EntitiesParameter<PROJECT>(loadPROJECT),
                "REGISTER_HOLDCollectionView",
                "[" + loadPROJECT.NUMBER + "] Hold Register");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
        }

        public bool CanSendToHoldRegister()
        {
            if (SelectedEntity == null)
                return false;

            return SelectedEntity.RegisterChange == null && SelectedEntity.RegisterHold == null;
        }

        public bool CanSendToChangeRegister()
        {
            if (SelectedEntity == null)
                return false;

            return SelectedEntity.RegisterChange == null && SelectedEntity.RegisterHold == null;
        }

        public void SendToChangeRegister()
        {
            if (SelectedEntity == null)
                return;

            var editingEntity = SelectedEntity;
            REGISTER_CHANGE newRegister = new REGISTER_CHANGE();
            newRegister.GUID_PROJECT = loadPROJECT.GUID;
            newRegister.GUID_AREA = SelectedEntity.GUID_AREA;
            newRegister.NUMBER = getChangeRegisterNewNumber();
            newRegister.TITLE = SelectedEntity.TITLE;
            newRegister.DESCRIPTION = SelectedEntity.DESCRIPTION;
            newRegister.SCHEDULE_IMPACT = SelectedEntity.SCHEDULE_IMPACT;
            newRegister.COST_IMPACT = SelectedEntity.COST_IMPACT;
            newRegister.IMPACT_TYPE = Register_ImpactType.Internal;
            newRegister.INTERDISC_CHECK_COMPLETE = false;
            newRegister.APPROVED = false;
            newRegister.DATE_RAISED = DateTime.Now;
            newRegister.CREATED = DateTime.Now;
            newRegister.CREATEDBY = LoginCredentials.CurrentUserGuid;
            REGISTER_CHANGEViewModel.Save(newRegister);

            editingEntity.GUID_CHANGE = newRegister.GUID;
            MainViewModel.Save(editingEntity);

            DocumentInfo DocumentInfo = new DocumentInfo("View_ChangeRegister" + loadPROJECT.GUID.ToString(),
                new EntitiesParameter<PROJECT>(loadPROJECT),
                    "REGISTER_CHANGECollectionView",
                    "[" + loadPROJECT.NUMBER + "] Change Register");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
            delayedRefreshTimer.Start();
        }

        public void SendToHoldRegister()
        {
            if (SelectedEntity == null)
                return;

            var editingEntity = SelectedEntity;
            REGISTER_HOLD newRegister = new REGISTER_HOLD();
            newRegister.GUID_PROJECT = loadPROJECT.GUID;
            newRegister.GUID_AREA = SelectedEntity.GUID_AREA;
            newRegister.NUMBER = getHoldRegisterNewNumber();
            newRegister.DESCRIPTION = SelectedEntity.DESCRIPTION;
            newRegister.DATE_RAISED = DateTime.Now;
            newRegister.CREATED = DateTime.Now;
            newRegister.CREATEDBY = LoginCredentials.CurrentUserGuid;
            REGISTER_HOLDViewModel.Save(newRegister);

            editingEntity.GUID_HOLD = newRegister.GUID;
            MainViewModel.Save(editingEntity);

            DocumentInfo DocumentInfo = new DocumentInfo("View_HoldRegister" + loadPROJECT.GUID.ToString(),
                new EntitiesParameter<PROJECT>(loadPROJECT),
                    "REGISTER_HOLDCollectionView",
                    "[" + loadPROJECT.NUMBER + "] Hold Register");

            DocumentManagerService.ShowExistingEntityDocumentWithLogging(DocumentInfo, this);
            delayedRefreshTimer.Start();
        }


        private void DelayedRefreshTimer_Tick(object sender, EventArgs e)
        {
            delayedRefreshTimer.Stop();
            FullRefresh();
        }


        private string getChangeRegisterNewNumber()
        {
            IEnumerable<REGISTER_CHANGE> entitiesInOrder = REGISTER_CHANGEViewModel.Entities.OrderBy(x => x.EntityNumber);
            if (entitiesInOrder.Count() == 0)
                return StringFormatUtils.AppendStringWithEnumerator(string.Empty, 0, DefaultNumericFieldLength());

            REGISTER_CHANGE largestNumberEntity = entitiesInOrder.Last();
            string largestNumberString = largestNumberEntity.EntityNumber;
            int numericFieldLength = 0;
            long largestNumberValueOnly = 0;
            string largestNumberStringOnly = StringFormatUtils.ParseStringIntoComponents(largestNumberString, out numericFieldLength, out largestNumberValueOnly);
            long newRowNumber = largestNumberValueOnly + 1;
            return StringFormatUtils.AppendStringWithEnumerator(string.Empty, newRowNumber, DefaultNumericFieldLength());
        }

        private string getHoldRegisterNewNumber()
        {
            IEnumerable<REGISTER_HOLD> entitiesInOrder = REGISTER_HOLDViewModel.Entities.OrderBy(x => x.EntityNumber);
            if (entitiesInOrder.Count() == 0)
                return StringFormatUtils.AppendStringWithEnumerator(string.Empty, 0, DefaultNumericFieldLength());

            REGISTER_HOLD largestNumberEntity = entitiesInOrder.Last();
            string largestNumberString = largestNumberEntity.EntityNumber;
            int numericFieldLength = 0;
            long largestNumberValueOnly = 0;
            string largestNumberStringOnly = StringFormatUtils.ParseStringIntoComponents(largestNumberString, out numericFieldLength, out largestNumberValueOnly);
            long newRowNumber = largestNumberValueOnly + 1;
            return StringFormatUtils.AppendStringWithEnumerator(string.Empty, newRowNumber, DefaultNumericFieldLength());
        }

        public bool IsActionedOnDrawingVisibility { get; set; }
        public override void ExportToExcel()
        {
            IsActionedOnDrawingVisibility = true;
            this.RaisePropertyChanged(x => x.IsActionedOnDrawingVisibility);
            base.ExportToExcel();
            IsActionedOnDrawingVisibility = false;
            this.RaisePropertyChanged(x => x.IsActionedOnDrawingVisibility);
        }



        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT,
                (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)
                loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Issues_Register);
            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        bool showReport = false;
        XtraReportIssuesRegister issuesRegisterReport;
        public void ViewReport()
        {
            showReport = true;
            //to make sure all navigational properties are loaded
            FullRefresh();
        }

        private void previewReport(IEnumerable<REGISTER_ISSUE> issues)
        {
            LoadingScreenManager.ShowLoadingScreen(1);
            showReport = false;
            issuesRegisterReport = new XtraReportIssuesRegister();
            PROJECT_REPORT dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    issuesRegisterReport.LoadLayout(sw.BaseStream);
                }
            }

            //set paperkind depending on project location
            if (loadPROJECT.OFFICE.NAME.ToUpper().Contains("PERTH"))
                issuesRegisterReport.PaperKind = System.Drawing.Printing.PaperKind.A3;
            else
                issuesRegisterReport.PaperKind = System.Drawing.Printing.PaperKind.Tabloid;

            issuesRegisterReport.AssignProperties(issues);
            DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = issuesRegisterReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            issuesRegisterReport.RequestParameters = false;
            issuesRegisterReport.CreateDocument(true);
            LoadingScreenManager.CloseLoadingScreen();
            previewWindow.Show();
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            //get { return "REGISTER_ISSUECollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "REGISTER_ISSUECollectionViewModelWrapper_v2"; }
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
            return loadPROJECT.NUMBER + "_Register_Issue";
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

        public IEnumerable<REGISTER_CHANGE> REGISTER_CHANGECollection
        {
            get
            {
                var collection = GetEntities<REGISTER_CHANGE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NUMBER);
                return collection;
            }
        }

        public IEnumerable<REGISTER_HOLD> REGISTER_HOLDCollection
        {
            get
            {
                var collection = GetEntities<REGISTER_HOLD>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NUMBER);
                return collection;
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

        public CollectionViewModel<REGISTER_CHANGE, REGISTER_CHANGE, Guid, IBluePrintsEntitiesUnitOfWork> REGISTER_CHANGEViewModel
        {
            get
            {
                return (CollectionViewModel<REGISTER_CHANGE, REGISTER_CHANGE, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<REGISTER_CHANGE>();
            }
        }

        public CollectionViewModel<REGISTER_HOLD, REGISTER_HOLD, Guid, IBluePrintsEntitiesUnitOfWork> REGISTER_HOLDViewModel
        {
            get
            {
                return (CollectionViewModel<REGISTER_HOLD, REGISTER_HOLD, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<REGISTER_HOLD>();
            }
        }
        #endregion
    }
}