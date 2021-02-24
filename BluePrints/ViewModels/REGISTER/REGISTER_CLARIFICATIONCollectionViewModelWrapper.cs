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
using DevExpress.XtraPrinting;
using System.Diagnostics;

namespace BluePrints.ViewModels
{
    public class REGISTER_CLARIFICATIONCollectionViewModelWrapper :
        BluePrintsEntitiesAutoNumberCollectionWrapper
        <REGISTER_TQ, REGISTER_TQ, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of REGISTERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static REGISTER_CLARIFICATIONCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new REGISTER_CLARIFICATIONCollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the REGISTERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the REGISTERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected REGISTER_CLARIFICATIONCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<USER>, IQueryable<USER>> USERProjectionFunc()
        {
            return query => query.Where(x => x.LEAVE_DATE == null || x.LEAVE_DATE > DateTime.Now);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && (x.REPORT_TYPE == ReportType.Clarification_Register.ToString() || x.REPORT_TYPE == ReportType.Clarification_Register.ToString()));
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
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
            MainViewModel.SetParentViewModel(this);
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

        private void previewReport(IEnumerable<REGISTER_TQ> registerChanges)
        {
            LoadingScreenManager.ShowLoadingScreen(1);
            showReport = false;
            rptChangeRegister = new XtraReportChangeRegister();
            PROJECT_REPORT dbProjectReport = PROJECT_REPORTCollection.FirstOrDefault(x => x.REPORT_TYPE == ReportType.TQ_Register.ToString());
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

            //rptChangeRegister.AssignProperties(loadPROJECT, registerChanges);
            DocumentPreviewWindow previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = rptChangeRegister;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            rptChangeRegister.RequestParameters = false;
            rptChangeRegister.CreateDocument(true);
            LoadingScreenManager.CloseLoadingScreen();
            previewWindow.Show();
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