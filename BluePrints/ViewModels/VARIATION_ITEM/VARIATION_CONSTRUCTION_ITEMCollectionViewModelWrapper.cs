using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Projections;
using BluePrints.Common.Reports;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using BluePrints.Reports;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Printing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single VARIATION object view model.
    /// </summary>
    public partial class VARIATION_CONSTRUCTION_ITEMCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<VARIATION_CONSTRUCTION_ITEM, VARIATION_CONSTRUCTION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static VARIATION_CONSTRUCTION_ITEMCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new VARIATION_CONSTRUCTION_ITEMCollectionViewModelWrapper());
        }

        protected PROJECT loadPROJECT;
        public VARIATION_CONSTRUCTION loadVARIATION { get; set; }
        protected List<JOB_COSTGROUPS> JOB_COSTGROUPS;
        protected List<JOB_COSTTYPES> JOB_COSTTYPES;
        protected List<JOBCOST_HDR> JOBCOST_HDRS;
        protected List<JOBCOST_RESOURCE> JOBCOST_RESOURCES;
        protected List<STOCK_ITEMS> STOCK_ITEMS;
        protected readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> localPrimeroUnitOfWorkFactory;
        protected IPrimeroEntitiesUnitOfWork localPrimeroUnitOfWork;
        protected IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        protected override void resolveParameters(object parameter)
        {            
            var receiveParameter = (DualEntitiesParameter<PROJECT, VARIATION_CONSTRUCTION>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            loadVARIATION = receiveParameter.GetSecondEntity();
            initializeCompulsoryViewProperties(loadPROJECT);

            string stringValueToFill = loadVARIATION.NUMBER;
            int numericFieldLength = 0;
            long valueToFillNumberOnly = 0;
            string valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(stringValueToFill, out numericFieldLength, out valueToFillNumberOnly);

            VariationNumber = valueToFillNumberOnly.ToString("D3");
            DocumentNumber = string.Concat(loadPROJECT.NUMBER, "-VAR-PM-", VariationNumber);
            Client = loadPROJECT.CLIENT;
        }
        
        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECT_REPORTS, PROJECT_REPORTProjectionFunc, null, true);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.VARIATION_CONSTRUCTIONS, VARIATION_CONSTRUCTIONProjectionFunc, null, true);
        }

        public string Client { get; set; }
        public string VariationNumber { get; set; }
        public string DocumentNumber { get; set; }
        public override string ViewName => "CONSTRUCTION_VARIATION_ITEMSViewModelWrapper_v4";

        protected override Func<IRepositoryQuery<VARIATION_CONSTRUCTION_ITEM>, IQueryable<VARIATION_CONSTRUCTION_ITEM>> specifyMainViewModelProjection()
        {
            return query => VARIATION_CONSTRUCTION_ITEMQuery(query.Where(x => x.GUID_VARIATION_CONSTRUCTION == loadVARIATION.GUID));
        }

        protected virtual Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> PROJECT_REPORTProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID && x.REPORT_TYPE == ReportType.Construction_Variation_Report.ToString());
        }

        protected virtual Func<IRepositoryQuery<VARIATION_CONSTRUCTION>, IQueryable<VARIATION_CONSTRUCTION>> VARIATION_CONSTRUCTIONProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadVARIATION.GUID);
        }

        public IQueryable<VARIATION_CONSTRUCTION_ITEM> VARIATION_CONSTRUCTION_ITEMQuery(IQueryable<VARIATION_CONSTRUCTION_ITEM> VARIATION_CONSTRUCTION_ITEMS)
        {
            foreach(VARIATION_CONSTRUCTION_ITEM VARIATION_CONSTRUCTION_ITEM in VARIATION_CONSTRUCTION_ITEMS)
            {
                VARIATION_CONSTRUCTION_ITEM.SetUnitOfWork(localPrimeroUnitOfWork);
                VARIATION_CONSTRUCTION_ITEM.SetJOBCOST_HDRS(JOBCOST_HDRS);
            }

            return VARIATION_CONSTRUCTION_ITEMS;
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.VARIATION_CONSTRUCTION_ITEMS);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<VARIATION_CONSTRUCTION_ITEM> entities)
        {
            loadVARIATION = VARIATION_CONSTRUCTIONCollectionViewModel.Entities.FirstOrDefault();
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        protected void initializeCompulsoryViewProperties(Data.PROJECT project)
        {
            localPrimeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            localPrimeroUnitOfWork = localPrimeroUnitOfWorkFactory.CreateUnitOfWork();
            JOBCOST_HDRS = ExoQueries.GetProjectSubJobs(localPrimeroUnitOfWork, loadPROJECT.NUMBER).ToList();
            JOB_COSTGROUPS = ExoQueries.GetCostGroups(localPrimeroUnitOfWork).ToList();
            JOB_COSTTYPES = localPrimeroUnitOfWork.JOB_COSTTYPES.ToList();
            JOBCOST_RESOURCES = localPrimeroUnitOfWork.JOBCOST_RESOURCE.Where(x => x.ISACTIVE == "Y").ToList();
            STOCK_ITEMS = ExoQueries.GetMiscStockItems(localPrimeroUnitOfWork).ToList();
        }

        public override void FullRefresh()
        {
            if (!CanFullRefresh())
                return;

            initializeCompulsoryViewProperties(loadPROJECT);
            base.FullRefresh();
        }

        public override void UnifiedNewRowInitializationFromView(VARIATION_CONSTRUCTION_ITEM projection)
        {
            projection.SetUnitOfWork(localPrimeroUnitOfWork);
            projection.SetJOBCOST_HDRS(JOBCOST_HDRS);
        }

        #region Tag saving behavior
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(VARIATION_CONSTRUCTION_ITEM projection, out bool isNew)
        {
            projection.GUID_VARIATION_CONSTRUCTION = loadVARIATION.GUID;
            isNew = false;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        public override string UnifiedValueValidation(VARIATION_CONSTRUCTION_ITEM projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(VARIATION_CONSTRUCTION_ITEM projection)
        {
            return string.Empty;
        }
        #endregion

        #region View Events
        public void EditValueChanged(EditValueChangedEventArgs e)
        {
            if (MainViewModel == null || loadVARIATION == null)
                return;

            string fieldName = ((BaseEdit)e.OriginalSource).Tag.ToString();
            DataUtils.TrySetNestedValue(fieldName, loadVARIATION, e.NewValue);

            VARIATION_CONSTRUCTIONCollectionViewModel.Save(loadVARIATION);
            loadVARIATION.Update();
        }

        public override void UnifiedCellValueChanged(string field_name, object old_value, object new_value, VARIATION_CONSTRUCTION_ITEM projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new VARIATION_CONSTRUCTION_ITEM().TYPEProxy))
            {
                VariationConstructionItemType type = (VariationConstructionItemType)new_value;

                string itemNumberStr;
                if (type == VariationConstructionItemType.Management)
                    itemNumberStr = "1.";
                else if (type == VariationConstructionItemType.Engineering)
                    itemNumberStr = "2.";
                else if (type == VariationConstructionItemType.TradesAndLabour)
                    itemNumberStr = "3.";
                else if (type == VariationConstructionItemType.Equipment)
                    itemNumberStr = "4.";
                else
                    itemNumberStr = "5.";

                int currentEntitiesTypeCount = Entities.Count(x => x.TYPE == type);
                itemNumberStr += currentEntitiesTypeCount.ToString();
                MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new VARIATION_CONSTRUCTION_ITEM().ITEM_ID), projection.ITEM_ID, itemNumberStr, EntityMessageType.Changed);
                projection.ITEM_ID = itemNumberStr;
            }

            GridControlService.RefreshSummary();
            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
        }

        public bool CanViewReport()
        {
            if (IsLoading || MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public bool CanEditReport()
        {
            if (IsLoading || MainViewModel == null || MainViewModel.Entities.Count == 0)
                return false;

            return true;
        }

        public void EditReport()
        {
            var reportDesigner = new UserReportDesigner(loadPROJECT, (CollectionViewModel<PROJECT_REPORT, PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>)loaderCollection.GetViewModel<PROJECT_REPORT>(), ReportType.Construction_Variation_Report);

            if (reportDesigner.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                reportDesigner.Dispose();
            else
                reportDesigner.Dispose();
        }

        public void ViewReport()
        {
            LoadingScreenManager.ShowLoadingScreen(1);
            var constructionVariationReport = new XtraReportConstructionVariation();
            var dbProjectReport = loaderCollection.GetObject<PROJECT_REPORT>();
            if (dbProjectReport != null)
            {
                var reportString = dbProjectReport.REPORT.ToString();
                using (var sw = new StreamWriter(new MemoryStream()))
                {
                    sw.Write(reportString);
                    sw.Flush();
                    constructionVariationReport.LoadLayout(sw.BaseStream);
                }
            }

            
            constructionVariationReport.AssignProperties(loadPROJECT, loadVARIATION, MainViewModel.Entities);
            var previewWindow = new DocumentPreviewWindow();
            previewWindow.PreviewControl.DocumentSource = constructionVariationReport;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            previewWindow.WindowState = WindowState.Maximized;
            constructionVariationReport.RequestParameters = false;
            constructionVariationReport.CreateDocument(true);
            LoadingScreenManager.CloseLoadingScreen();
            previewWindow.Show();
        }

        #endregion

        #region View Property
        public CollectionViewModel<VARIATION_CONSTRUCTION, VARIATION_CONSTRUCTION, Guid, IBluePrintsEntitiesUnitOfWork> VARIATION_CONSTRUCTIONCollectionViewModel
        {
            get
            {
                if (MainViewModel == null)
                    return null;

                return
                    (CollectionViewModel<VARIATION_CONSTRUCTION, VARIATION_CONSTRUCTION, Guid, IBluePrintsEntitiesUnitOfWork>)
                    loaderCollection.GetViewModel<VARIATION_CONSTRUCTION>();
            }
        }

        public IEnumerable<string> JOBCOST_HDRNameCollection
        {
            get
            {
                if (JOBCOST_HDRS == null)
                    return null;

                return JOBCOST_HDRS.OrderBy(x => x.JOBCODE).Select(x => x.JOBCODE);
            }
        }

        public IEnumerable<JOB_COSTGROUPS> JOB_COSTGROUPCollection
        {
            get
            {
                if (JOB_COSTGROUPS == null)
                    return null;

                return JOB_COSTGROUPS;
            }
        }

        public IEnumerable<JOB_COSTTYPES> JOB_COSTTYPECollection
        {
            get
            {
                if (JOB_COSTTYPES == null)
                    return null;

                return JOB_COSTTYPES;
            }
        }

        public IEnumerable<string> JOBCOST_RESOURCENameCollection
        {
            get
            {
                if (JOBCOST_RESOURCES == null)
                    return null;

                return JOBCOST_RESOURCES.Select(x => x.RESOURCENAME).OrderBy(x => x).ToList();
            }
        }

        public IEnumerable<STOCK_ITEMS> STOCK_ITEMCollection
        {
            get
            {
                if (STOCK_ITEMS == null)
                    return null;

                return STOCK_ITEMS;
            }
        }
        #endregion
    }
}