using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Charts;
using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DevExpress.Mvvm;
using BluePrints.Common.Base;
using BluePrints.Views;
using System.Windows.Controls;

namespace BluePrints.ViewModels
{
    /// <summary>
    ///     Represents the root POCO view model for the BluePrintsEntities data model.
    /// </summary>
    public class BluePrintsEntitiesViewModel :
        DocumentsViewModel<BluePrintsEntitiesModuleDescription, IBluePrintsEntitiesUnitOfWork>
    {
        private const string ViewLayoutName = "BluePrintsEntitiesViewModel";

        protected Dispatcher MainThreadDispatcher = Application.Current.Dispatcher;
        private CollectionViewModel<PROJECT, Guid, IBluePrintsEntitiesUnitOfWork> _projectCollectionViewModel;

        /// <summary>
        ///     Initializes a new instance of the BluePrintsEntitiesViewModel class.
        ///     This constructor is declared protected to avoid undesired instantiation of the BluePrintsEntitiesViewModel type
        ///     without the POCO proxy factory.
        /// </summary>
        protected BluePrintsEntitiesViewModel()
            : base(BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory())
        {
            initializeCategoryDescription();
            _projectCollectionViewModel = CollectionViewModel<PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>.CreateCollectionViewModel(unitOfWorkFactory, x => x.PROJECTS);
            _projectCollectionViewModel.OnEntitiesLoadedCallBack = OnEntitiesLoadedCallBack;
            _projectCollectionViewModel.OnAfterEntitiesChangedCallBack = OnAfterEntitiesChanged;
            _projectCollectionViewModel.Entities.ToList();
        }

        private void clearAllProjectModules()
        {
            List<BluePrintsEntitiesModuleDescription> bluePrintsModules = CreateBluePrintsModules().ToList();
            List<BluePrintsEntitiesModuleDescription> removeModules = new List<BluePrintsEntitiesModuleDescription>();
            foreach(BluePrintsEntitiesModuleDescription bluePrintsModule in Modules)
            {
                if (!bluePrintsModules.Any(x => x.Id.ToString() == bluePrintsModule.Id.ToString()))
                    removeModules.Add(bluePrintsModule);
            }

            foreach(BluePrintsEntitiesModuleDescription removeModule in removeModules)
            {
                Modules.Remove(removeModule);
            }

            initializeCategoryDescription();
        }

        /// <summary>
        ///     Creates a new instance of BluePrintsEntitiesViewModel as a POCO view model.
        /// </summary>
        public static BluePrintsEntitiesViewModel Create()
        {
            return ViewModelSource.Create(() => new BluePrintsEntitiesViewModel());
        }

        public override void OnLoaded()
        {

        }

        public override void OnClosing(CancelEventArgs cancelEventArgs)
        {
            Properties.Settings.Default["ThemeName"] = ApplicationThemeHelper.ApplicationThemeName;
            Properties.Settings.Default.Save();
            Environment.Exit(1);
        }

        private void OnEntitiesLoadedCallBack(IEnumerable<PROJECT> entities)
        {
            IsLoaded = true;
            _projectCollectionViewModel.OnEntitiesLoadedCallBack = null;
            MainThreadDispatcher.BeginInvoke(new Action(() => preloadAssemblies(entities)));
        }

        private void CreateProjectModules(IEnumerable<PROJECT> entities)
        {
            List<BluePrintsEntitiesModuleDescription> newModules = new List<BluePrintsEntitiesModuleDescription>();
            BluePrintsEntitiesModuleDescription projectCategoryHeader;
            if (LoginCredentials.hasPermission(PermissionResources.ManageProject))
                projectCategoryHeader = projectEditableCategoryDescription;
            else
                projectCategoryHeader = projectCategoryDescription;

            newModules.Add(projectCategoryHeader);

            if (entities.Any(x => x.GUID_MANAGEUSER == LoginCredentials.CurrentUserGuid))
            {
                IEnumerable<PROJECT> userProjects = entities.Where(x => x.GUID_MANAGEUSER == LoginCredentials.CurrentUserGuid);
                if(userProjects.Any(x => x.STATUS == ProjectStatus.Active))
                    projectCategoryHeader.ChildModules.Add(myProjectsCategoryDescription);

                if(userProjects.Any(x => x.STATUS == ProjectStatus.Tender))
                    projectCategoryHeader.ChildModules.Add(myTendersCategoryDescription);
            }

            if (entities.Any(x => x.STATUS == ProjectStatus.Active))
                projectCategoryHeader.ChildModules.Add(projectActiveCategoryDescription);

            if (entities.Any(x => x.STATUS == ProjectStatus.TenderSubmitted))
                projectCategoryHeader.ChildModules.Add(projectSubmittedTenderCategoryDescription);

            if (entities.Any(x => x.STATUS == ProjectStatus.Tender))
                projectCategoryHeader.ChildModules.Add(projectWIPTenderCategoryDescription);

            newModules.AddRange(CreateDataModules());
            var projects =
            entities.Where(x => x.STATUS == ProjectStatus.Active || x.STATUS == ProjectStatus.TenderSubmitted || x.STATUS == ProjectStatus.Tender)
                .OrderBy(x => x.NUMBER)
                .ToArray()
                .AsEnumerable();

            if (projects.Any())
            {
                foreach (var project in projects)
                {
                    CreateProjectTree(project);
                }
            }

            Modules.AddRange(newModules);
        }

        /// <summary>
        /// Used for preloading assemblies
        /// </summary>
        private void preloadAssemblies(IEnumerable<PROJECT> entities)
        {
            //if (!LoginCredentials.isPreloadMode())
            CreateProjectModules(entities);
            //startPreloading(new BluePrintsEntitiesModuleDescription("View_PreloadDashboard", null, "Preloading...", "PROJECTDashboardView", new ActionObject(this.ClosePreloadDocument)));
            //else
        }

        private void showDashboard()
        {
            if (LoginCredentials.hasPermission(PermissionResources.ViewDashboard))
            {
                var dashboard = Modules.FirstOrDefault(x => x.DocumentType == "PROJECTDashboardView");
                if (dashboard != null)
                    NavigateCore(dashboard);
            }
        }

        const string projectViewIdPrefix = "View_Project";
        private void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, bool isBulkRefresh)
        {
            RefreshProjectNavigations();
            //Guid primaryKey = (Guid)key;
            //RemoveProjectModule(primaryKey);

            //if (messageType == EntityMessageType.Added || messageType == EntityMessageType.Changed)
            //{
            //    PROJECT project = _projectCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == primaryKey);
            //    if (project != null)
            //    {
            //        if (messageType == EntityMessageType.Added)
            //        {
            //            CreateProjectTree(project);
            //        }
            //        else if (messageType == EntityMessageType.Changed)
            //        {
            //            if (project.STATUS == ProjectStatus.Active || project.STATUS == ProjectStatus.Tender)
            //            {
            //                CreateProjectTree(project);
            //            }
            //        }
            //    }
            //}
        }

        public bool CanRefreshProjectNavigations()
        {
            return _projectCollectionViewModel != null;
        }

        public void RefreshProjectNavigations()
        {
            if (_projectCollectionViewModel == null)
                return;

            clearAllProjectModules();
            CreateProjectModules(_projectCollectionViewModel.Entities);
        }

        private void RemoveProjectModule(Guid primaryKey)
        {
            string projectViewId = projectViewIdPrefix + primaryKey.ToString();
            string projectPrimaryKey = primaryKey.ToString();

            var projectCategoryModules = Modules.SelectMany(x => x.ChildModules)
            .Where(x => x.Id.ToString() == tenderSubmittedCategoryId || x.Id.ToString() == activeCategoryId || x.Id.ToString() == myProjectCategoryId || x.Id.ToString() == myTenderCategoryId);
            var projectModules = projectCategoryModules.SelectMany(x => x.ChildModules);
            var projectModule = projectModules.FirstOrDefault(x => x.Id.ToString() == projectViewId || (x.ParentId != null && x.ParentId.ToString().Contains(projectPrimaryKey)));

            if(projectModule != null)
            {
                var projectCategoryModule = projectCategoryModules.First(x => x.Id.ToString() == projectModule.ParentId.ToString());
                projectCategoryModule.ChildModules.Remove(projectModule);
            }
        }

        const string dashboardCategoryId = "Category_Dashboard";
        const string projectCategoryId = "View_Projects";
        const string dataCategoryId = "Category_Data";
        const string activeCategoryId = "Category_Active";
        const string tenderSubmittedCategoryId = "Category_Tender_Submitted";
        const string tenderWIPCategoryId = "Category_Tender_WIP";
        const string myProjectCategoryId = "Category_MyProject";
        const string myTenderCategoryId = "Category_MyTender";
        const string stockGroupCategoryId = "Category_StockGroup";
        BluePrintsEntitiesModuleDescription dashboardCategoryDescription;
        BluePrintsEntitiesModuleDescription projectEditableCategoryDescription;
        BluePrintsEntitiesModuleDescription projectCategoryDescription;
        BluePrintsEntitiesModuleDescription myProjectsCategoryDescription;
        BluePrintsEntitiesModuleDescription myTendersCategoryDescription;
        BluePrintsEntitiesModuleDescription projectActiveCategoryDescription;
        BluePrintsEntitiesModuleDescription projectSubmittedTenderCategoryDescription;
        BluePrintsEntitiesModuleDescription projectWIPTenderCategoryDescription;
        BluePrintsEntitiesModuleDescription dataCategoryDescription;
        BluePrintsEntitiesModuleDescription stockGroupCategoryDescription;

        private void initializeCategoryDescription()
        {
            projectEditableCategoryDescription = new BluePrintsEntitiesModuleDescription(projectCategoryId, null, "Projects", "PROJECTCollectionView", null, null, null, true, true, @"Programming\Project_16x16.png", null, null, false);
            projectCategoryDescription = new BluePrintsEntitiesModuleDescription(projectCategoryId, null, "Projects", null, null, null, null, true, true, @"Programming\Project_16x16.png");
            myProjectsCategoryDescription = new BluePrintsEntitiesModuleDescription(myProjectCategoryId, projectCategoryId, "My Projects", null, null, null, null, true, false, @"Business Objects\BOTask_16x16.png");
            myTendersCategoryDescription = new BluePrintsEntitiesModuleDescription(myTenderCategoryId, projectCategoryId, "My Tenders", null, null, null, null, true, false, @"Business Objects\BOReport2_16x16.png");
            projectActiveCategoryDescription = new BluePrintsEntitiesModuleDescription(activeCategoryId, projectCategoryId, "Active", null, null, null, null, true, false, @"Function Library\Financial_16x16.png");
            projectSubmittedTenderCategoryDescription = new BluePrintsEntitiesModuleDescription(tenderSubmittedCategoryId, projectCategoryId, "Submitted Tender", null, null, null, null, false, false, @"Function Library\Statistical_16x16.png");
            projectWIPTenderCategoryDescription = new BluePrintsEntitiesModuleDescription(tenderWIPCategoryId, projectCategoryId, "WIP Tender", null, null, null, null, false, false, @"Function Library\Compatibility_16x16.png");
            dataCategoryDescription = new BluePrintsEntitiesModuleDescription(dataCategoryId, null, "Data", null, null, null, null, false, true, @"Navigation\DocumentMap_16x16.png");
            stockGroupCategoryDescription = new BluePrintsEntitiesModuleDescription(stockGroupCategoryId, null, "Stock Group", null, null, null, null, false, false, @"Business Objects\BOOrder_16x16.png");
        }

        protected override BluePrintsEntitiesModuleDescription[] CreateModules()
        {
            return CreateBluePrintsModules();
        }

        private BluePrintsEntitiesModuleDescription[] CreateBluePrintsModules()
        {
            List<BluePrintsEntitiesModuleDescription> bluePrintsEntitiesModuleDescriptions = new List<BluePrintsEntitiesModuleDescription>();

            dashboardCategoryDescription = new BluePrintsEntitiesModuleDescription(dashboardCategoryId, null, "Dashboards", null, null, null, null, false, true, @"Chart\BarOfPie_16x16.png");
            bluePrintsEntitiesModuleDescriptions.Add(dashboardCategoryDescription);
            if (LoginCredentials.hasPermission(PermissionResources.ViewDashboard))
                dashboardCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_Dashboard", dashboardCategoryId, "Dashboard", "PROJECTDashboardView", null, null, null, true, false, @"Chart\Chart_16x16.png"));

            dashboardCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_UserDashboard", dashboardCategoryId, "My Dashboard", "USERDashboardView", new EntitiesParameter<USER>(LoginCredentials.CurrentUser), null, null, true, false, @"Chart\Bar_16x16.png"));
            dashboardCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_UserDeliverables", dashboardCategoryId, "My Deliverables", "User_OffisteDirectProgressCollectionView", new EntitiesParameter<USER>(LoginCredentials.CurrentUser), null, null, true, false, @"Chart\ChartsShowLegend_16x16.png"));

            return bluePrintsEntitiesModuleDescriptions.ToArray();
        }

        private BluePrintsEntitiesModuleDescription[] CreatePreloadModules()
        {
            List<BluePrintsEntitiesModuleDescription> bluePrintsEntitiesModuleDescriptions = new List<BluePrintsEntitiesModuleDescription>();
            bluePrintsEntitiesModuleDescriptions.Add(new BluePrintsEntitiesModuleDescription("View_Dashboard", null, "Dashboard", "PROJECTDashboardView"));
            return bluePrintsEntitiesModuleDescriptions.ToArray();
        }

        private List<BluePrintsEntitiesModuleDescription> CreateDataModules()
        {
            List<BluePrintsEntitiesModuleDescription> bluePrintsEntitiesModuleDescriptions = new List<BluePrintsEntitiesModuleDescription>();

            bluePrintsEntitiesModuleDescriptions.Add(dataCategoryDescription);

            if (LoginCredentials.hasPermission(PermissionResources.ManageDepartment))
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_Departments", dataCategoryId, "Departments", "DEPARTMENTCollectionView", null, null, null, false, false, @"Business Objects\BOPosition_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDiscipline))
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_Disciplines", dataCategoryId, "Disciplines", "DISCIPLINECollectionView", null, null, null, false, false, @"Business Objects\BOContact2_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDocType))
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_DocTypes", dataCategoryId, "Document Types", "DOCTYPECollectionView", null, null, null, false, false, @"Business Objects\BOReport2_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManagePhase))
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_Phases", dataCategoryId, "Phases", "PHASECollectionView", null, null, null, false, false, @"Filter Elements\TreeView_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageUser))
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_Users", dataCategoryId, "Users", "USERCollectionView", null, null, null, false, false, @"Business Objects\BOPerson_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageRole))
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_Roles", dataCategoryId, "Roles", "ROLECollectionView", null, null, null, false, false, @"Business Objects\BORole_16x16.png"));

            //if (LoginCredentials.hasPermission(PermissionResources.ManageDeliverableStatuses))
            //    dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_DeliverableStatuses", dataCategoryId, "Deliverable Gates", "DELIVERABLES_STATUSCollectionView"));

            dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_UOMs", dataCategoryId, "Unit of Measures", "UOMCollectionView", null, null, null, false, false, @"RichEdit\RulerHorizontal_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageStockCode))
            {
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageStock_Group", dataCategoryId, "Global Stock Groups", "STOCK_GROUPCollectionView", null, null, null, false, false, @"Business Objects\BOProductGroup_16x16.png"));
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageStock_Direct", dataCategoryId, "Global Stock Codes", "STOCK_CODECollectionView", new DualEntitiesParameter<PROJECT, StockCodeTypeClass>(null, new StockCodeTypeClass(StockCodeType.Estimate)), null, null, false, false, @"Business Objects\BOProduct_16x16.png"));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageCommodity))
            {
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageCommodity_Code", dataCategoryId, "Global Commodity Codes", "COMMODITY_CODECollectionView", null, null, null, false, false, @"Business Objects\BOOrder_16x16.png"));
                //dataCategoryDescription.ChildModules.Add(commodityCategoryDescription);
                //commodityCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageCommodity_Design", commodityCodeCategoryId, "Commodity Code [Design]", "STOCK_GROUPCollectionView", new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null, new CommodityCodeTypeClass(CommodityCodeType.Design)), null, "Design"));
                //commodityCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageCommodity_Direct", commodityCodeCategoryId, "Commodity Code [Direct]", "STOCK_GROUPCollectionView", new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null, new CommodityCodeTypeClass(CommodityCodeType.Direct)), null, "Direct"));
                //commodityCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageCommodity_DirectGroup", commodityCodeCategoryId, "Commodity Group [Direct]", "COMMODITY_GROUP_DIRECTCollectionView", null, null, "Direct Group"));
                //commodityCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageCommodity_Indirect", commodityCodeCategoryId, "Commodity Code [Indirect]", "STOCK_GROUPCollectionView", new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null, new CommodityCodeTypeClass(CommodityCodeType.Indirect)), null, "Indirect"));
                //commodityCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageCommodity_Overhead", commodityCodeCategoryId, "Commodity Code [Overhead]", "STOCK_GROUPCollectionView", new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null, new CommodityCodeTypeClass(CommodityCodeType.Overhead)), null, "Overhead"));
            }

            dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageClient", dataCategoryId, "Clients", "CLIENTCollectionView", null, null, null, false, false, @"Business Objects\BOCustomer_16x16.png"));
            dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_MeetingAction", dataCategoryId, "Meeting Actions", "MEETING_ACTIONCollectionView", null, null, null, false, false, @"Reports\Parameters_16x16.png"));

            return bluePrintsEntitiesModuleDescriptions;
        }


        private Dictionary<string, PROJECTViewWindow> openedProjectView = new Dictionary<string, PROJECTViewWindow>();
        public override IDocument NavigateCore(BluePrintsEntitiesModuleDescription module)
        {
            if (module == null || DocumentManagerService == null)
                return null;

            //if (module.DocumentType == "PROJECTView")
            //{
            //    KeyValuePair<string, PROJECTViewWindow> existingProjectView = openedProjectView.FirstOrDefault(x => x.Key == module.Id.ToString());
            //    if (existingProjectView.Value != null)
            //    {
            //        PROJECTViewWindow projectView = existingProjectView.Value;
            //        projectView.Tag = module.Id.ToString();
            //        projectView.WindowClosed = windowClosed;
            //        projectView.Activate();
            //    }
            //    else
            //    {
            //        PROJECTViewWindow projectView = new PROJECTViewWindow();
            //        projectView.Tag = module.Id.ToString();
            //        projectView.WindowClosed = windowClosed;
            //        PROJECTViewModelWrapper viewModel = (PROJECTViewModelWrapper)projectView.DataContext;
            //        viewModel.OnParameterChanged(module.DocumentParameter);
            //        openedProjectView.Add(module.Id.ToString(), projectView);
            //        projectView.Show();
            //    }

            //    return null;
            //}
            //else
            //{
                DocumentInfo documentInfo = new DocumentInfo(module.Id, module.DocumentParameter, module.DocumentType, module.ModuleTitle);
                var document = DocumentManagerService.ShowExistingEntityDocumentWithLogging(documentInfo, this);
                //var document = DocumentManagerService.FindDocumentByIdOrCreate(module.ModuleTitle,
                //    x => NavigateToDocument(module));
                //document.Show();

                return document;
            //}
        }

        private void windowClosed(string Id)
        {
            openedProjectView.Remove(Id);
        }

        private void CreateProjectTree(PROJECT entity)
        {
            //List<BluePrintsEntitiesModuleDescription> newModules = new List<BluePrintsEntitiesModuleDescription>();
            string projectTitle = entity.NUMBER + " " + entity.NAME;
            string childTitlePrefix = "[" + entity.NUMBER + "] ";
            string keyString = entity.EntityKey.ToString();
            string projectKey = projectViewIdPrefix + keyString;

            object parentId;
            BluePrintsEntitiesModuleDescription projectStatusDescription;

            if ((entity.STATUS == ProjectStatus.Active || entity.STATUS == ProjectStatus.Tender) && entity.GUID_MANAGEUSER == LoginCredentials.CurrentUserGuid)
            {
                if (entity.STATUS == ProjectStatus.Active)
                {
                    projectStatusDescription = myProjectsCategoryDescription;
                    parentId = myProjectCategoryId;
                }
                else
                {
                    projectStatusDescription = myTendersCategoryDescription;
                    parentId = myTenderCategoryId;
                }
            }
            else if (entity.STATUS == ProjectStatus.Active)
            {
                projectStatusDescription = projectActiveCategoryDescription;
                parentId = activeCategoryId;
            }
            else if (entity.STATUS == ProjectStatus.TenderSubmitted)
            {
                projectStatusDescription = projectSubmittedTenderCategoryDescription;
                parentId = tenderSubmittedCategoryId;
            }
            else
            {
                projectStatusDescription = projectWIPTenderCategoryDescription;
                parentId = tenderWIPCategoryId;
            }

            List<BluePrintsEntitiesModuleDescription> projectModuleContextMenuItems = new List<BluePrintsEntitiesModuleDescription>();
            BluePrintsEntitiesModuleDescription projectRateMenuItem = new BluePrintsEntitiesModuleDescription("View_ProjectRates" + keyString, projectKey, childTitlePrefix + "Rates", "RATECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Rates", false, false, @"Number Formats\Currency2_16x16.png");
            BluePrintsEntitiesModuleDescription projectAreaMenuItem = new BluePrintsEntitiesModuleDescription("View_ProjectAreas" + keyString, projectKey, childTitlePrefix + "Areas", "AREACollectionView", new EntitiesParameter<PROJECT>(entity), null, "Areas", false, false, @"Maps\Map_16x16.png");
            BluePrintsEntitiesModuleDescription projectBaselineMenuItem = new BluePrintsEntitiesModuleDescription("View_ProjectBaselines" + keyString, projectKey, childTitlePrefix + "Baseline Revisions", "BASELINECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Baseline Revisions", false, false, @"Support\Version_16x16.png");
            BluePrintsEntitiesModuleDescription projectProgressMenuItem = new BluePrintsEntitiesModuleDescription("View_ProjectProgress" + keyString, projectKey, childTitlePrefix + "Progress Revisions", "PROGRESSCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Progress Revisions", false, false, @"Support\Version_16x16.png");
            BluePrintsEntitiesModuleDescription projectEstimateMenuItem = new BluePrintsEntitiesModuleDescription("View_ProjectEstimates" + keyString, projectKey, childTitlePrefix + "Estimate Revisions", "ESTIMATECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Estimate Revisions", false, false, @"Support\Version_16x16.png");

            projectModuleContextMenuItems.Add(projectRateMenuItem);
            projectModuleContextMenuItems.Add(projectAreaMenuItem);
            projectModuleContextMenuItems.Add(projectBaselineMenuItem);
            projectModuleContextMenuItems.Add(projectProgressMenuItem);
            projectModuleContextMenuItems.Add(projectEstimateMenuItem);
            BluePrintsEntitiesModuleDescription projectModuleDescription = new BluePrintsEntitiesModuleDescription(projectKey, parentId, projectTitle, "PROJECTView", new EntitiesParameter<PROJECT>(entity), null, null, false, false, @"Programming\ProjectDirectory_16x16.png", projectModuleContextMenuItems, NavigateCoreCommand);
            projectStatusDescription.ChildModules.Add(projectModuleDescription);

            BluePrintsEntitiesModuleDescription design_category_description = new BluePrintsEntitiesModuleDescription("Category_Design" + keyString, null, "Design", null, null, null, null, false, false, @"Miscellaneous\Design_16x16.png");
            BluePrintsEntitiesModuleDescription construct_category_description = new BluePrintsEntitiesModuleDescription("Category_Construct" + keyString, null, "Construct", null, null, null, null, false, false, @"Programming\IDE_16x16.png");

            projectModuleDescription.ChildModules.Add(design_category_description);
            projectModuleDescription.ChildModules.Add(construct_category_description);

            projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectUserDashboard", dashboardCategoryId, "Resourcing", "PROJECT_USERDashboardView", new EntitiesParameter<PROJECT>(entity), null, "Resourcing", false, false, @"Toolbox Items\Sparkline_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageArea))
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectAreas" + keyString, projectKey, childTitlePrefix + "Areas", "AREACollectionView", new EntitiesParameter<PROJECT>(entity), null, "Areas", false, false, @"Maps\Map_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageRate))
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectRates" + keyString, projectKey, childTitlePrefix + "Rates", "RATECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Rates", false, false, @"Number Formats\Currency2_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageSubjob))
            {
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectSubjobs" + keyString, projectKey, childTitlePrefix + "Subjobs", "SUBJOBCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Subjobs", false, false, @"Programming\ProjectFile_16x16.png"));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageWorkpack) && entity.USE_WORKPACKS)
            {
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectWorkpacks" + keyString, projectKey, childTitlePrefix + "Workpacks", "WORKPACKCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Workpacks", false, false, @"Support\PackageProduct_16x16.png"));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageBaseline))
            {
                design_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_LiveProjectDirectDeliverables" + keyString, projectKey, childTitlePrefix + "Deliverables [Direct]", "BASELINE_ITEMCollectionView", new TripleEntitiesParameter<PROJECT, IAmBaseline, object>(entity, null, DeliverablesViewType.Direct), null, "Deliverables [Direct]", false, false, @"Business Objects\BOEmployee_16x16.png"));
                design_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_LiveProjectIndirectDeliverables" + keyString, projectKey, childTitlePrefix + "Deliverables [Indirect]", "BASELINE_ITEMCollectionView", new TripleEntitiesParameter<PROJECT, IAmBaseline, object>(entity, null, DeliverablesViewType.Indirect), null, "Deliverables [Indirect]", false, false, @"Business Objects\BOCustomer_16x16.png"));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageEstimation))
            {
                construct_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_LiveProjectDirectEstimates" + keyString, projectKey, childTitlePrefix + "Estimates [Direct]", "ESTIMATE_ITEMCollectionView", new TripleEntitiesParameter<PROJECT, IAmBaseline, object>(entity, null, new KeyValuePair<DeliverablesViewType, EstimateViewMode>(DeliverablesViewType.Direct, EstimateViewMode.Estimate)), null, "Estimate [Direct]", false, false, @"Business Objects\BOEmployee_16x16.png"));
                construct_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_LiveProjectIndirectEstimates" + keyString, projectKey, childTitlePrefix + "Estimates [Indirect]", "ESTIMATE_ITEMCollectionView", new TripleEntitiesParameter<PROJECT, IAmBaseline, object>(entity, null, new KeyValuePair<DeliverablesViewType, EstimateViewMode>(DeliverablesViewType.Indirect, EstimateViewMode.Estimate)), null, "Estimate [Indirect]", false, false, @"Business Objects\BOCustomer_16x16.png"));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageBudget))
            {
                construct_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_LiveProjectDirectBudgets" + keyString, projectKey, childTitlePrefix + "Budgets [Direct]", "BUDGET_ITEMCollectionView", new TripleEntitiesParameter<PROJECT, IAmBaseline, object>(entity, null, new KeyValuePair<DeliverablesViewType, EstimateViewMode>(DeliverablesViewType.Direct, EstimateViewMode.Budget)), null, "Budget [Direct]", false, false, @"Business Objects\BOEmployee_16x16.png"));
                construct_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_LiveProjectIndirectBudgets" + keyString, projectKey, childTitlePrefix + "Budgets [Indirect]", "BUDGET_ITEMCollectionView", new TripleEntitiesParameter<PROJECT, IAmBaseline, object>(entity, null, new KeyValuePair<DeliverablesViewType, EstimateViewMode>(DeliverablesViewType.Indirect, EstimateViewMode.Budget)), null, "Budget [Indirect]", false, false, @"Business Objects\BOCustomer_16x16.png"));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageProgress))
            {
                design_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_LiveProjectOffsiteProgress" + keyString, projectKey, childTitlePrefix + "Design Progress", "OffsiteDirectProgressCollectionView", new DualEntitiesParameter<PROJECT, PROGRESS>(entity, null), null, "Progress", false, false, @"Chart\Bar2_16x16.png"));
                construct_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_LiveProjectSiteDirectProgress" + keyString, projectKey, childTitlePrefix + "Site Direct Progress", "SiteDirectProgressCollectionView", new DualEntitiesParameter<PROJECT, PROGRESS>(entity, null), null, "Progress", false, false, @"Chart\Bar2_16x16.png"));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageProgressDistribution))
            {
                design_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_LiveProjectOffsiteProgressDistribution" + keyString, projectKey, childTitlePrefix + "Design Progress Distribution", "OffsiteDirectDistributionCollectionView", new DualEntitiesParameter<PROJECT, PROGRESS>(entity, null), null, "Progress Distribution", false, false, @"Chart\Area2_16x16.png"));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageVariation))
            {
                design_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectDesignVariations" + keyString, projectKey, childTitlePrefix + "Variations", "VARIATIONCollectionView", new DualEntitiesParameter<PROJECT, PhaseTypeClass>(entity, new PhaseTypeClass(PhaseType.Design)), null, "Variations", false, false, @"Scheduling\ShowWorkTimeOnly_16x16.png"));
                construct_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectConstructionVariations" + keyString, projectKey, childTitlePrefix + "Variations", "VARIATIONCollectionView", new DualEntitiesParameter<PROJECT, PhaseTypeClass>(entity, new PhaseTypeClass(PhaseType.Construct)), null, "Variations", false, false, @"Scheduling\ShowWorkTimeOnly_16x16.png"));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageDeliverableStatuses))
                design_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectDeliverableStatuses" + keyString, projectKey, childTitlePrefix + "Deliverable Gates", "DELIVERABLES_STATUSCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Deliverable Gates", false, false, @"Business Objects\BORules_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageCommodity))
                construct_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectCommodity_Codes" + keyString, projectKey, childTitlePrefix + "Commodity Codes", "COMMODITY_CODECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Commodity Codes", false, false, @"Business Objects\BOOrder_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageStockCode))
            {
                construct_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectStock_Groups" + keyString, projectKey, childTitlePrefix + "Stock Groups", "STOCK_GROUPCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Stock Groups", false, false, @"Business Objects\BOProductGroup_16x16.png"));
                construct_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectStock_Codes" + keyString, projectKey, childTitlePrefix + "Stock Codes", "STOCK_CODECollectionView", new DualEntitiesParameter<PROJECT, StockCodeTypeClass>(entity, new StockCodeTypeClass(StockCodeType.Estimate)), null, "Stock Codes", false, false, @"Business Objects\BOProduct_16x16.png"));
            }

            projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectMeetings" + keyString, projectKey, childTitlePrefix + "Meetings", "MEETINGCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Meetings", false, false, @"Business Objects\BOPosition2_16x16.png"));
            projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectMeetingTypes" + keyString, projectKey, childTitlePrefix + "Meeting Types", "MEETING_TYPECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Meeting Types", false, false, @"Business Objects\BOFileAttachment_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageSubjob))
            {
                design_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectDesignExoJobSetup" + keyString, projectKey, childTitlePrefix + "Exo Job Setup", "EXOSUBJOBCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Exo Permission", false, false, @"Business Objects\BOUser_16x16.png"));
                construct_category_description.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectConstructExoJobSetup" + keyString, projectKey, childTitlePrefix + "Exo Job Setup", "EXONATIVESUBJOBCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Exo Permission", false, false, @"Business Objects\BOUser_16x16.png"));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageHoliday))
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_Holidays" + keyString, projectKey, childTitlePrefix + "Holidays", "HOLIDAYCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Holidays", false, false, @"Scheduling\Calendar_16x16.png"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageRegisters))
            {
                string registerCategoryId = "View_RegisterCategory" + keyString;

                BluePrintsEntitiesModuleDescription registerCategoryDescription = new BluePrintsEntitiesModuleDescription(registerCategoryId, projectKey, "Registers", null, null, null, null, false);
                design_category_description.ChildModules.Add(registerCategoryDescription);
                registerCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_IssueRegister" + keyString, registerCategoryId, childTitlePrefix + "Issue Register", "REGISTER_ISSUECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Issue Register", false, false, @"Support\Issue_16x16.png"));
                registerCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ChangeRegister" + keyString, registerCategoryId, childTitlePrefix + "Change Register", "REGISTER_CHANGECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Change Register", false, false, @"Scheduling\ChangeStatus_16x16.png"));
                registerCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_RiskRegister" + keyString, registerCategoryId, childTitlePrefix + "Risk Register", "REGISTER_RISKCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Risk Register", false, false, @"Support\BreakingChange_16x16.png"));
                registerCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_HoldRegister" + keyString, registerCategoryId, childTitlePrefix + "Hold Register", "REGISTER_HOLDCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Hold Register", false, false, @"Business Objects\BORules_16x16.png"));
                registerCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_LLRegister" + keyString, registerCategoryId, childTitlePrefix + "Lesson's Learned Register", "REGISTER_LLCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Lesson's Learned Register", false, false, @"Business Objects\BOTask_16x16.png"));
                registerCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_NCRegister" + keyString, registerCategoryId, childTitlePrefix + "Non-Conformance Register", "REGISTER_NCCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Non-Conformance Register", false, false, @"Programming\BugReport_16x16.png"));
            }
        }

    }
}

namespace BluePrints.Common.ViewModel
{
    public partial class BluePrintsEntitiesModuleDescription : ModuleDescription<BluePrintsEntitiesModuleDescription>
    {
        List<BluePrintsEntitiesModuleDescription> menuItems;
        Action<object> navigateAction;
        public bool Animate { get; set; }
        public BluePrintsEntitiesModuleDescription(object id, object parentId, string title, string documentType = null, object documentParameter = null, ImageSource image = null, string navigationTitle = null, bool treeViewIsExpanded = true, bool showInCollapseMode = false, string imagePath = "", List<BluePrintsEntitiesModuleDescription> menuItems = null, Action<object> navigateAction = null, bool showAnimation = false)
            : base(id, parentId, title, documentType, documentParameter, image, navigationTitle, treeViewIsExpanded, showInCollapseMode)
        {
            this.menuItems = menuItems;
            this.navigateAction = navigateAction;
            ChildModules = new RangeObservableCollection<BluePrintsEntitiesModuleDescription>();
            this.Animate = showAnimation;
            if(imagePath != string.Empty)
                Image = new BitmapImage(new Uri("pack://application:,,,/DevExpress.Images.v17.2;component/Images/" + imagePath));
            else
            {
                if (!CanNavigate)
                    Image = new BitmapImage(new Uri("pack://application:,,,/DevExpress.Images.v17.2;component/Images/Data/ManageDataSource_16x16.png"));
                    //new Uri(@"/Common/Images/PRIMERO.jpg", UriKind.Relative));
                else
                    Image = new BitmapImage(new Uri("pack://application:,,,/DevExpress.Images.v17.2;component/Images/Actions/Open_16x16.png"));
            }
        }

        public RangeObservableCollection<BluePrintsEntitiesModuleDescription> ChildModules { get; set; }

        public string Caption => this.NavigationTitle;
        
        public ContextMenu Menu
        {
            get
            {
                if(menuItems != null)
                {
                    ContextMenu menu = new ContextMenu();
                    foreach(BluePrintsEntitiesModuleDescription item in menuItems)
                    {
                        MenuItem newMenuItem = new MenuItem();
                        if(item.Image != null)
                        {
                            Image image = new Image();
                            image.Source = item.Image;
                            newMenuItem.Icon = image;
                        }

                        newMenuItem.Header = item.ModuleTitle;
                        newMenuItem.Command = new RelayCommand(navigateAction);
                        newMenuItem.CommandParameter = item;
                        menu.Items.Add(newMenuItem);
                    }

                    return menu;
                }

                return null;
            }
        }
    }

    public class ActionObject
    {
        public ActionObject(Action actionParameter)
        {
            this.actionParameter = actionParameter;
        }

        Action actionParameter { get; set; }
        public void ExecuteAction()
        {
            actionParameter();
        }
    }
}