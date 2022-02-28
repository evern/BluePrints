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
using System.Data.Entity;
using BaseModel.Data.Helpers;
using BaseModel.ViewModel.Dialogs;
using System.Threading;
using BluePrints.View;
using BluePrints.Common.Helpers;
using System.Deployment.Application;
using BluePrints.Common.Utils;
using DevExpress.Xpf.Accordion;

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
        private DispatcherTimer onAfterNavigationLoadedDispatcher;
        public bool isLoggingOut = true;
        protected BluePrintsEntitiesViewModel()
            : base(BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory())
        {
            initialize();
        }

        public string OfficeName
        {
            get
            {
#if PERTH
                return "Perth";
#else
                return "Montreal/USA";
#endif
            }
        }

        /// <summary>
        ///     Initializes a new instance of the BluePrintsEntitiesViewModel class.
        ///     This constructor is declared protected to avoid undesired instantiation of the BluePrintsEntitiesViewModel type
        ///     without the POCO proxy factory.
        /// </summary>
        public BluePrintsEntitiesViewModel(bool initialize)
            : base(BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory())
        {
            if(initialize)
                this.initialize();
            else
                initializeCategoryDescription();
        }

        private void initialize()
        {
            initializeCategoryDescription();
            _projectCollectionViewModel = CollectionViewModel<PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>.CreateCollectionViewModel(unitOfWorkFactory, x => x.PROJECTS);
            _projectCollectionViewModel.OnEntitiesLoadedCallBack = OnEntitiesLoadedCallBack;
            _projectCollectionViewModel.OnAfterEntitiesChangedCallBack = OnAfterEntitiesChanged;
            _projectCollectionViewModel.Entities.ToList();
            GlobalMethods.SetAccordionExpandedState = CollapseExpandAccordion;
            AccordionExpanded = true;
        }

        public void CollapseExpandAccordion(bool isExpand)
        {
            AccordionExpanded = isExpand;
            this.RaisePropertyChanged(x => x.AccordionExpanded);
        }

        private void clearAllProjectModules()
        {
            //cannot clear modules so remove one by one
            List<BluePrintsEntitiesModuleDescription> removeModules = new List<BluePrintsEntitiesModuleDescription>();
            foreach(BluePrintsEntitiesModuleDescription bluePrintsModule in Modules)
            {
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
            return ViewModelSource.Create(() => new BluePrintsEntitiesViewModel(true));
        }

        public override void OnLoaded()
        {
            WindowState = WindowState.Maximized;
            this.RaisePropertyChanged(x => x.WindowState);
        }

        WindowState windowState;
        public WindowState WindowState
        {
            get => windowState;
            set
            {
                windowState = value;
                this.RaisePropertyChanged(x => x.IsApplicationWindowMaximized);
            }
        }

        public override void OnClosing(CancelEventArgs cancelEventArgs)
        {
            Properties.Settings.Default["ThemeName"] = ApplicationThemeHelper.ApplicationThemeName;
            Properties.Settings.Default.Save();

            if(isLoggingOut)
                Environment.Exit(1);
        }

        private void OnEntitiesLoadedCallBack(IEnumerable<PROJECT> entities)
        {
            IsLoaded = true;
            _projectCollectionViewModel.OnEntitiesLoadedCallBack = null;
            MainThreadDispatcher.BeginInvoke(new Action(() => loadNavigationModules(entities)));
            onAfterNavigationLoadedDispatcher.Start();
        }


        private void onAfterNavigationLoadedDispatcher_Tick(object sender, EventArgs e)
        {
            onAfterNavigationLoadedDispatcher.Stop();

            if (LoginCredentials.CurrentUser != null && LoginCredentials.CurrentUserGuid != Guid.Empty)
            {
                if(LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Menu_UserDeliverables)) == LoginCredentials.PermissionStatus.All)
                {
                    bool? hideUserDeliverablesOnStartupPreference = LoginCredentials.GetUserPreferenceBool(DataUtils.GetNameOf(() => UserPreferences.Global_HideUserDeliverablesOnStartup));
                    if (hideUserDeliverablesOnStartupPreference == null || hideUserDeliverablesOnStartupPreference == false)
                        NavigateCore(myDeliverablesDescription);
                }
            }

            string lastChangeLogDisplayVersionStr = XMLHelpers.GetSettings_LastChangeLogDisplayVersion();
            Version currentDeploymentVersion = BluePrintsDataUtils.GetClickOncePublishVersion();
            if(currentDeploymentVersion != null)
            {
                if (lastChangeLogDisplayVersionStr == string.Empty)
                    showChangeLogWindow();
                else
                {
                    Version lastChangeLogDisplayVersion = null;
                    if (Version.TryParse(lastChangeLogDisplayVersionStr, out lastChangeLogDisplayVersion))
                    {
                        if (lastChangeLogDisplayVersion < currentDeploymentVersion)
                        {
                            showChangeLogWindow();
                        }
                    }
                }
            }

            if (LoginCredentials.CurrentUser != null && LoginCredentials.CurrentUserGuid != Guid.Empty && LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Menu_UserDeliverables)) != LoginCredentials.PermissionStatus.None)
                NavigateCore(myDeliverablesDescription);

            if(XMLHelpers.GetIntegrationSettings_AutoInvokeProject())
            {
                string projectNumber = XMLHelpers.GetIntegrationSettings_ProjectNumber();
                if(projectNumber != null && projectNumber != string.Empty)
                {
                    BluePrintsEntitiesModuleDescription projectModuleDescription = autoInvokeProjects.Where(x => x.PROJECT != null).FirstOrDefault(x => x.PROJECT.NUMBER == projectNumber);
                    if(projectModuleDescription != null)
                    {
                        populateProjectTree(projectModuleDescription.PROJECT, projectModuleDescription, false);
                        List<BluePrintsEntitiesModuleDescription> projectChildModuleDescriptions = getAllNodes(projectModuleDescription);

                        string deliverablesListIdentifier = DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignDeliverables);
                        BluePrintsEntitiesModuleDescription deliverableModuleDescription = projectChildModuleDescriptions.FirstOrDefault(x => x.NavigationId.Contains(deliverablesListIdentifier));
                        if (deliverableModuleDescription != null)
                            NavigateCore(deliverableModuleDescription);
                        else
                            MessageBoxService.ShowMessage("Deliverables list for " + projectNumber + " failed to open because you do not have authorisation to access it", "Error", MessageButton.OK, MessageIcon.Warning);
                    }
                    else
                        MessageBoxService.ShowMessage("Deliverables list for " + projectNumber + " failed to open because project isn't found or you do not have authorisation to access it, please make sure project status is not closed and authorisation has been granted", "Error", MessageButton.OK, MessageIcon.Warning);
                }
            }
        }

        private void showChangeLogWindow()
        {
            //if(LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Forecast_Indirect)) != LoginCredentials.PermissionStatus.None)
            //{
            //    ChangeLogWindow changeLogWindow = new ChangeLogWindow();
            //    changeLogWindow.Show();
            //}

            ChangeLogWindow changeLogWindow = new ChangeLogWindow();
            changeLogWindow.Show();
            XMLHelpers.UpdateSettingsXMLChangeLogDisplayVersion(BluePrintsDataUtils.GetClickOncePublishVersion());
        }

        private void CreateModules(IEnumerable<PROJECT> entities, bool isSecurityModule)
        {
            Modules.Add(dashboardCategoryDescription);

            moduleAdder(dashboardCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_UserDashboard), string.Empty, dashboardCategoryDescription.NavigationId, "My Dashboard", "USERDashboardView", new EntitiesParameter<USER>(LoginCredentials.CurrentUser), null, null, true, false, @"Chart\Bar_16x16.png"), isSecurityModule);
            moduleAdder(dashboardCategoryDescription, myDeliverablesDescription, isSecurityModule);
            moduleAdder(dashboardCategoryDescription, myDesignTimesheetDescription, isSecurityModule);
            moduleAdder(dashboardCategoryDescription, myReviewTimesheetDescription, isSecurityModule);
            moduleAdder(dashboardCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_DocumentControl), string.Empty, dashboardCategoryDescription.NavigationId, "Document Control", "DOCCONTROL_BASELINE_ITEMCollectionView", null, null, null, true, false, @"Edit\Customization_16x16.png"), isSecurityModule);

            BluePrintsEntitiesModuleDescription projectCategoryHeader;
            if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Dashboard)) != LoginCredentials.PermissionStatus.None)
                projectCategoryHeader = projectEditableCategoryDescription;
            else
                projectCategoryHeader = projectCategoryDescription;

            Modules.Add(projectCategoryHeader);
            if(!isSecurityModule)
            {
                if (entities.Any(x => x.GUID_MANAGEUSER == LoginCredentials.CurrentUserGuid))
                {
                    IEnumerable<PROJECT> userProjects = entities.Where(x => x.GUID_MANAGEUSER == LoginCredentials.CurrentUserGuid);
                    if (userProjects.Any(x => x.STATUS == ProjectStatus.Active))
                        projectCategoryHeader.ChildModules.Add(myProjectsCategoryDescription);

                    if (userProjects.Any(x => x.STATUS == ProjectStatus.Tender))
                        projectCategoryHeader.ChildModules.Add(myTendersCategoryDescription);
                }
            }

            if (entities.Any(x => x.STATUS == ProjectStatus.Active))
                projectCategoryHeader.ChildModules.Add(projectActiveCategoryDescription);

            if (entities.Any(x => x.STATUS == ProjectStatus.TenderSubmitted))
                projectCategoryHeader.ChildModules.Add(projectSubmittedTenderCategoryDescription);

            if (entities.Any(x => x.STATUS == ProjectStatus.Tender))
                projectCategoryHeader.ChildModules.Add(projectWIPTenderCategoryDescription);

            if (entities.Any(x => x.STATUS == ProjectStatus.Lead))
                projectCategoryHeader.ChildModules.Add(projectLeadCategoryDescription);

            var projects =
            entities.Where(x => x.STATUS == ProjectStatus.Active || x.STATUS == ProjectStatus.TenderSubmitted || x.STATUS == ProjectStatus.Tender || x.STATUS == ProjectStatus.Lead)
                .OrderBy(x => x.NUMBER)
                .ToArray()
                .AsEnumerable();

            //clear integration auto invoke before adding projects to list
            autoInvokeProjects.Clear();
            if (projects.Any())
            {
                foreach (var project in projects)
                {
                    createProjectTree(project, isSecurityModule);
                }
            }

            moduleAdder(projectCategoryHeader, companyHSECategoryDescription, isSecurityModule);
            Modules.AddRange(CreateDataModules(isSecurityModule));
        }
        
        public void ItemExpanded(AccordionItemExpandedEventArgs e)
        {
            BluePrintsEntitiesModuleDescription projectModuleDescription = e.Item as BluePrintsEntitiesModuleDescription;
            if(projectModuleDescription != null)
            {
                if(projectModuleDescription.PROJECT != null)
                {
                    projectModuleDescription.ChildModules.Clear();
                    populateProjectTree(projectModuleDescription.PROJECT, projectModuleDescription, false);
                }
            }
        }

        private void loadNavigationModules(IEnumerable<PROJECT> PROJECTS)
        {
            CreateModules(PROJECTS, false);
        }

        /// <summary>
        /// Used for roles
        /// </summary>
        public void LoadSecurityEntries()
        {
            //if (!LoginCredentials.isPreloadMode())
            List<PROJECT> samplePROJECTS = new List<PROJECT>();
            PROJECT project = new PROJECT();
            project.NUMBER = "XXXXX";
            project.STATUS = ProjectStatus.Active;
            samplePROJECTS.Add(project);

            CreateModules(samplePROJECTS, true);
            //startPreloading(new BluePrintsEntitiesModuleDescription("View_PreloadDashboard", null, "Preloading...", "PROJECTDashboardView", new ActionObject(this.ClosePreloadDocument)));
            //else
        }

        private void showDashboard()
        {
            if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Menu_Dashboard)) != LoginCredentials.PermissionStatus.None)
            {
                var dashboard = Modules.FirstOrDefault(x => x.DocumentType == "PROJECTDashboardView");
                if (dashboard != null)
                    NavigateCore(dashboard);
            }
        }

        const string projectViewIdPrefix = "View_Project";
        private void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender, Guid senderKey, bool isBulkRefresh)
        {
            if(messageType == EntityMessageType.Added)
                RefreshProjectNavigations();
            else if(messageType == EntityMessageType.Changed)
            {
                if (senderKey == Guid.Empty)
                    RefreshProjectNavigations();
            }
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
            CreateModules(_projectCollectionViewModel.Entities, false);
        }

        public void LogOut()
        {
            if(MessageBoxService.ShowMessage("Are you sure you wish to log out?", "Confirmation", MessageButton.YesNo) == MessageResult.Yes)
                BluePrintsGlobalMethods.LogOut();
        }

        public bool IsApplicationWindowMaximized => windowState == WindowState.Maximized;

        public void ApplicationToggleWindowSize()
        {
            if (windowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;

            this.RaisePropertyChanged(x => x.WindowState);
        }

        public void ApplicationMinimizedWindowSize()
        {
            WindowState = WindowState.Minimized;
            this.RaisePropertyChanged(x => x.WindowState);
        }

        public void ApplicationShutDown()
        {
            if (MessageBoxService.ShowMessage("Are you sure you wish to close the program?", "Confirmation", MessageButton.YesNo) == MessageResult.Yes)
                BluePrintsGlobalMethods.ApplicationShutDown();
        }

        private void RemoveProjectModule(Guid primaryKey)
        {
            string projectViewId = projectViewIdPrefix + primaryKey.ToString();
            string projectPrimaryKey = primaryKey.ToString();

            var projectCategoryModules = Modules.SelectMany(x => x.ChildModules)
            .Where(x => x.NavigationId.ToString() == DataUtils.GetNameOf(() => NavigationResources.Category_UserTenders) || x.NavigationId.ToString() == DataUtils.GetNameOf(() => NavigationResources.Category_ActiveProjects) || x.NavigationId.ToString() == DataUtils.GetNameOf(() => NavigationResources.Category_UserProjects) || x.NavigationId.ToString() == DataUtils.GetNameOf(() => NavigationResources.Category_UserTenders));
            var projectModules = projectCategoryModules.SelectMany(x => x.ChildModules);
            var projectModule = projectModules.FirstOrDefault(x => x.NavigationId.ToString() == projectViewId || (x.ParentId != null && x.ParentId.ToString().Contains(projectPrimaryKey)));

            if(projectModule != null)
            {
                var projectCategoryModule = projectCategoryModules.First(x => x.NavigationId.ToString() == projectModule.ParentId.ToString());
                projectCategoryModule.ChildModules.Remove(projectModule);
            }
        }

        BluePrintsEntitiesModuleDescription dashboardCategoryDescription;
        BluePrintsEntitiesModuleDescription projectEditableCategoryDescription;
        BluePrintsEntitiesModuleDescription projectCategoryDescription;
        BluePrintsEntitiesModuleDescription myProjectsCategoryDescription;
        BluePrintsEntitiesModuleDescription myTendersCategoryDescription;
        BluePrintsEntitiesModuleDescription projectActiveCategoryDescription;
        BluePrintsEntitiesModuleDescription projectSubmittedTenderCategoryDescription;
        BluePrintsEntitiesModuleDescription projectWIPTenderCategoryDescription;
        BluePrintsEntitiesModuleDescription projectLeadCategoryDescription;
        BluePrintsEntitiesModuleDescription companyHSECategoryDescription;
        BluePrintsEntitiesModuleDescription dataCategoryDescription;
        BluePrintsEntitiesModuleDescription exoDataCategoryDescription;
        BluePrintsEntitiesModuleDescription myDeliverablesDescription;
        BluePrintsEntitiesModuleDescription myDesignTimesheetDescription;
        BluePrintsEntitiesModuleDescription myReviewTimesheetDescription;
        private void initializeCategoryDescription()
        {
            onAfterNavigationLoadedDispatcher = new DispatcherTimer();
            onAfterNavigationLoadedDispatcher.Interval = new TimeSpan(0, 0, 0, 1);
            onAfterNavigationLoadedDispatcher.Tick += onAfterNavigationLoadedDispatcher_Tick;

            dashboardCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Dashboard), string.Empty, null, "Dashboards", null, null, null, null, false, true, @"Chart\BarOfPie_16x16.png");
            myDeliverablesDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_UserDeliverables), string.Empty, dashboardCategoryDescription.NavigationId, "My Deliverables", "User_OffsiteDirectProgressCollectionView", null, null, null, true, false, @"Chart\ChartsShowLegend_16x16.png");
            myDesignTimesheetDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_UserTimesheet), string.Empty, dashboardCategoryDescription.NavigationId, "My Design Timesheet", "DesignTimesheetEntryCollectionView", false, null, null, true, false, @"Scheduling\TimeLineView_16x16.png");
            myReviewTimesheetDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_ReviewTimesheet), string.Empty, dashboardCategoryDescription.NavigationId, "Review Timesheets", "DesignTimesheetEntryCollectionView", true, null, null, true, false, @"Scheduling\SwitchTimeScalesTo_16x16.png");

            projectEditableCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_AllProjects), string.Empty, null, "Projects", "PROJECTCollectionView", new EntitiesParameter<Action<object>>(NavigateCoreCommand), null, null, true, true, @"Programming\Project_16x16.png", null, null, false, null, "Double click to view all projects");
            projectCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_AllProjects), string.Empty, null, "Projects", null, null, null, null, true, true, @"Programming\Project_16x16.png");
            myProjectsCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_AllProjects), string.Empty, DataUtils.GetNameOf(() => NavigationResources.Menu_AllProjects), "My Projects", null, null, null, null, true, false, @"Business Objects\BOTask_16x16.png");
            myTendersCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_UserTenders), string.Empty, DataUtils.GetNameOf(() => NavigationResources.Menu_AllProjects), "My Tenders", null, null, null, null, true, false, @"Business Objects\BOReport2_16x16.png");
            projectActiveCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_ActiveProjects), string.Empty, DataUtils.GetNameOf(() => NavigationResources.Menu_AllProjects), "Active", null, null, null, null, true, false, @"Function Library\Financial_16x16.png");
            projectSubmittedTenderCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_UserTenders), string.Empty, DataUtils.GetNameOf(() => NavigationResources.Menu_AllProjects), "Submitted Tender", null, null, null, null, false, false, @"Function Library\Statistical_16x16.png");
            projectWIPTenderCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_WIPProjects), string.Empty, DataUtils.GetNameOf(() => NavigationResources.Menu_AllProjects), "WIP Tender", null, null, null, null, false, false, @"Function Library\Compatibility_16x16.png");
            projectLeadCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_LeadProjects), string.Empty, DataUtils.GetNameOf(() => NavigationResources.Menu_AllProjects), "Lead", null, null, null, null, false, false, @"Business Objects\BOOrderItem_16x16.png");

            companyHSECategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_HSE), string.Empty, DataUtils.GetNameOf(() => NavigationResources.Menu_AllProjects), "Company HSE Report", "HSECollectionView", null, null, "HSE Report", false, false, @"Gauges\GaugeStyleLinearHorizontal_16x16.png");
            dataCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_Data), string.Empty, null, "Data", null, null, null, null, false, true, @"Navigation\DocumentMap_16x16.png");
            exoDataCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_Project_EXO), string.Empty, dataCategoryDescription.NavigationId, "EXO", null, null, null, null, false, false, @"Function Library\Financial_16x16.png");
        }

        protected override BluePrintsEntitiesModuleDescription[] CreateModules()
        {
            //dummy impletation of abstract module
            return new List<BluePrintsEntitiesModuleDescription>().ToArray();
        }

        BluePrintsEntitiesModuleDescription allProjectsDashboardModuleDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Dashboard), null, "Dashboard", "PROJECTDashboardView");
        private BluePrintsEntitiesModuleDescription[] CreatePreloadModules()
        {
            List<BluePrintsEntitiesModuleDescription> bluePrintsEntitiesModuleDescriptions = new List<BluePrintsEntitiesModuleDescription>();
            bluePrintsEntitiesModuleDescriptions.Add(allProjectsDashboardModuleDescription);
            return bluePrintsEntitiesModuleDescriptions.ToArray();
        }

        private RangeObservableCollection<BluePrintsEntitiesModuleDescription> CreateDataModules(bool isSample)
        {
            RangeObservableCollection<BluePrintsEntitiesModuleDescription> dataModules = new RangeObservableCollection<BluePrintsEntitiesModuleDescription>();

            dataModules.Add(dataCategoryDescription);

            moduleAdder(dataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Client), string.Empty, dataCategoryDescription.NavigationId, "Clients", "CLIENTCollectionView", null, null, null, false, false, @"Business Objects\BOCustomer_16x16.png"), isSample);
            moduleAdder(dataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_CommodityCode), string.Empty, dataCategoryDescription.NavigationId, "Commodity Codes", "COMMODITY_CODECollectionView", null, null, null, false, false, @"Business Objects\BOOrder_16x16.png"), isSample);
            moduleAdder(dataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Departments), string.Empty, dataCategoryDescription.NavigationId, "Departments", "DEPARTMENTCollectionView", null, null, null, false, false, @"Business Objects\BOPosition_16x16.png"), isSample);
            moduleAdder(dataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Disciplines), string.Empty, dataCategoryDescription.NavigationId, "Disciplines", "DISCIPLINECollectionView", null, null, null, false, false, @"Business Objects\BOContact2_16x16.png"), isSample);
            moduleAdder(dataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_DocumentTypes), string.Empty, dataCategoryDescription.NavigationId, "Document Types", "DOCTYPECollectionView", null, null, null, false, false, @"Business Objects\BOReport2_16x16.png"), isSample);
            moduleAdder(dataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Offices), string.Empty, dataCategoryDescription.NavigationId, "Offices", "OFFICECollectionView", null, null, null, false, false, @"Maps\GeoPointMap_16x16.png"), isSample);
            moduleAdder(dataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Phases), string.Empty, dataCategoryDescription.NavigationId, "Phases", "PHASECollectionView", null, null, null, false, false, @"Filter Elements\TreeView_16x16.png"), isSample);
            moduleAdder(dataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Pipelines), string.Empty, dataCategoryDescription.NavigationId, "Design Resource Planning", "PROJECTPlanView", null, null, null, false, false, @"Business Objects\BOSaleItem_16x16.png"), isSample);
            moduleAdder(dataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Roles), string.Empty, dataCategoryDescription.NavigationId, "Roles", "ROLECollectionView", null, null, null, false, false, @"Business Objects\BORole_16x16.png"), isSample);
            moduleAdder(dataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_TenderProfiles), string.Empty, dataCategoryDescription.NavigationId, "Tender Profiles", "TENDER_PROFILECollectionView", null, null, null, false, false, @"Dashboards\PieLabelsDataLabels2_16x16.png"), isSample);
            moduleAdder(dataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_UOMs), string.Empty, dataCategoryDescription.NavigationId, "Unit of Measures", "UOMCollectionView", null, null, null, false, false, @"RichEdit\RulerHorizontal_16x16.png"), isSample);
            moduleAdder(dataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Users), string.Empty, dataCategoryDescription.NavigationId, NavigationResources.Menu_User_Title, "USERCollectionView", null, null, null, false, false, @"Business Objects\BOPerson_16x16.png"), isSample);

            moduleAdder(dataCategoryDescription, exoDataCategoryDescription, isSample, true);
            moduleAdder(exoDataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_EXO_Users), BluePrintsResources.OfficePerth, exoDataCategoryDescription.NavigationId, "Perth Resources", "EXO_ResourcesCollectionView", BluePrintsResources.OfficePerth, null, null, false, false, @"Business Objects\BOPosition2_16x16.png"), isSample);
            moduleAdder(exoDataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_EXO_Users), BluePrintsResources.OfficeMontreal, exoDataCategoryDescription.NavigationId, "Montreal Resources", "EXO_ResourcesCollectionView", BluePrintsResources.OfficeMontreal, null, null, false, false, @"Business Objects\BOPosition2_16x16.png"), isSample);
            moduleAdder(exoDataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_EXO_Users), BluePrintsResources.OfficeUSA, exoDataCategoryDescription.NavigationId, "USA Resources", "EXO_ResourcesCollectionView", BluePrintsResources.OfficeUSA, null, null, false, false, @"Business Objects\BOPosition2_16x16.png"), isSample);
            moduleAdder(exoDataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_EXO_JobResources), string.Empty, exoDataCategoryDescription.NavigationId, "Job Resources", "JOBCOST_RESOURCECollectionView", null, null, null, false, false, @"Business Objects\BODetails_16x16.png"), isSample);
            moduleAdder(exoDataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_EXO_StockItems), string.Empty, exoDataCategoryDescription.NavigationId, "Stock Items", "STOCK_ITEMSCollectionView", null, null, null, false, false, @"Business Objects\BOProduct_16x16.png"), isSample);
            moduleAdder(exoDataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_TransactionsPerthYearToDate), string.Empty, exoDataCategoryDescription.NavigationId, "All Perth Transactions", "TransactionCollectionInstantFeedbackView", new TripleEntitiesParameter<PROJECT, object, object>(null, false, DatabaseLocale.Perth), null, "All Perth Transactions", false, false, @"/Common/Images/Australia-Flag.ico"), isSample);
            moduleAdder(exoDataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_TransactionsMontrealYearToDate), string.Empty, exoDataCategoryDescription.NavigationId, "All Montreal Transactions", "TransactionCollectionInstantFeedbackView", new TripleEntitiesParameter<PROJECT, object, object>(null, false, DatabaseLocale.Montreal), null, "All Montreal Transactions", false, false, @"/Common/Images/Canada-Flag.png"), isSample);
            moduleAdder(exoDataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_TransactionsUSAYearToDate), string.Empty, exoDataCategoryDescription.NavigationId, "All USA Transactions", "TransactionCollectionInstantFeedbackView", new TripleEntitiesParameter<PROJECT, object, object>(null, false, DatabaseLocale.USA), null, "All USA Transactions", false, false, @"/Common/Images/USA-Flag.ico"), isSample);
            moduleAdder(exoDataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_TransactionsPerth2020ToDate), string.Empty, exoDataCategoryDescription.NavigationId, "2020 Perth Transactions", "TransactionCollectionInstantFeedbackView", new TripleEntitiesParameter<PROJECT, object, object>(null, true, DatabaseLocale.Perth), null, "2020 Perth Transactions", false, false, @"/Common/Images/Australia-Flag.ico"), isSample);
            moduleAdder(exoDataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_TransactionsMontreal2020ToDate), string.Empty, exoDataCategoryDescription.NavigationId, "2020 Montreal Transactions", "TransactionCollectionInstantFeedbackView", new TripleEntitiesParameter<PROJECT, object, object>(null, true, DatabaseLocale.Montreal), null, "2020 Montreal Transactions", false, false, @"/Common/Images/Canada-Flag.png"), isSample);
            moduleAdder(exoDataCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_AllProjects_PL), string.Empty, exoDataCategoryDescription.NavigationId, "Projects P&L", "PROJECTPLCollectionView", null, null, "Projects P&L", false, false, @"Dashboards\PieLabelsDataLabels2_16x16.png"), isSample);

            return dataModules;  
        }

        public override IDocument NavigateCore(BluePrintsEntitiesModuleDescription module)
        {
            if (module == null || DocumentManagerService == null)
                return null;

            string viewName = module.DocumentType;
            if (module.PreferredDocumentType != null)
                viewName = module.PreferredDocumentType();

            DocumentInfo documentInfo = new DocumentInfo(module.NavigationId, module.DocumentParameter, viewName, module.ModuleTitle);
            var document = DocumentManagerService.ShowExistingEntityDocumentWithLogging(documentInfo, this);

            return document;
        }

        protected IMessageBoxService MessageBoxService
        {
            get { return this.GetRequiredService<IMessageBoxService>(); }
        }

        private DevExpress.Mvvm.IDialogService ReportDialogService
        {
            get { return this.GetRequiredService<DevExpress.Mvvm.IDialogService>("ReportDialogService"); }
        }

        public bool IsSyncDatabaseVisible => CanSyncDatabase;
        public bool CanSyncDatabase => LoginCredentials.IsAdmin;
        public void SyncDatabase()
        {
            if (!LoginCredentials.IsAdmin)
            {
                MessageBoxService.ShowMessage("Unauthorised");
                return;
            }

            SyncScreenViewModel viewModel = SyncScreenViewModel.Create();
            viewModel.SetParentViewModel(this);
            ReportDialogService.ShowDialog(MessageButton.OK, "Sync Status", "SyncScreen", viewModel);
        }

        //store projects in collection for auto invoke to automatically start items in project
        List<BluePrintsEntitiesModuleDescription> autoInvokeProjects = new List<BluePrintsEntitiesModuleDescription>();
        private void createProjectTree(PROJECT entity, bool isSecurityModule)
        {
            //List<BluePrintsEntitiesModuleDescription> newModules = new List<BluePrintsEntitiesModuleDescription>();
            string projectTitle = entity.NUMBER + " " + entity.NAME;
            string childTitlePrefix = "[" + entity.NUMBER + "] ";
            string projectSpecificKey = entity.GUID.ToString();

            string parentId;
            BluePrintsEntitiesModuleDescription projectStatusDescription;

            if ((entity.STATUS == ProjectStatus.Active || entity.STATUS == ProjectStatus.Tender) && entity.GUID_MANAGEUSER == LoginCredentials.CurrentUserGuid)
            {
                if (entity.STATUS == ProjectStatus.Active)
                {
                    projectStatusDescription = myProjectsCategoryDescription;
                    parentId = DataUtils.GetNameOf(() => NavigationResources.Category_UserProjects);
                }
                else
                {
                    projectStatusDescription = myTendersCategoryDescription;
                    parentId = DataUtils.GetNameOf(() => NavigationResources.Category_UserTenders);
                }
            }
            else if (entity.STATUS == ProjectStatus.Active)
            {
                projectStatusDescription = projectActiveCategoryDescription;
                parentId = DataUtils.GetNameOf(() => NavigationResources.Category_ActiveProjects);
            }
            else if (entity.STATUS == ProjectStatus.TenderSubmitted)
            {
                projectStatusDescription = projectSubmittedTenderCategoryDescription;
                parentId = DataUtils.GetNameOf(() => NavigationResources.Category_SubmittedProjects);
            }
            else if (entity.STATUS == ProjectStatus.Tender)
            {
                projectStatusDescription = projectWIPTenderCategoryDescription;
                parentId = DataUtils.GetNameOf(() => NavigationResources.Category_WIPProjects);
            }
            else
            {
                projectStatusDescription = projectLeadCategoryDescription;
                parentId = DataUtils.GetNameOf(() => NavigationResources.Category_LeadProjects);
            }

            List<BluePrintsEntitiesModuleDescription> projectModuleContextMenuItems = new List<BluePrintsEntitiesModuleDescription>();
            BluePrintsEntitiesModuleDescription projectBaselineMenuItem = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.ContextMenu_ProjectBaselines), projectSpecificKey, projectSpecificKey, childTitlePrefix + "Design Budget", "BASELINECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Design Budget", false, false, @"Support\Version_16x16.png");
            BluePrintsEntitiesModuleDescription projectEstimateMenuItem = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.ContextMenu_ProjectEstimates), projectSpecificKey, projectSpecificKey, childTitlePrefix + "Construction Budget", "ESTIMATECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Construction Budget", false, false, @"Support\Version_16x16.png");
            BluePrintsEntitiesModuleDescription projectProgressMenuItem = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.ContextMenu_ProjectProgresses), projectSpecificKey, projectSpecificKey, childTitlePrefix + "Progress", "PROGRESSCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Progress", false, false, @"Support\Version_16x16.png");

            //projectModuleContextMenuItems.Add(projectRateMenuItem);
            projectModuleContextMenuItems.Add(projectBaselineMenuItem);
            projectModuleContextMenuItems.Add(projectEstimateMenuItem);
            projectModuleContextMenuItems.Add(projectProgressMenuItem);

            if (!isSecurityModule && LoginCredentials.CurrentUser.PROJECT_PERMISSION.Count > 0 && !LoginCredentials.CurrentUser.PROJECT_PERMISSION.Any(x => x.GUID_PROJECT == entity.GUID))
            {
                BluePrintsEntitiesModuleDescription unauthorisedProjectModuleDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Dashboard), projectSpecificKey, parentId, projectTitle, null, null, null, null, false, false, @"Programming\ProjectDirectory_16x16.png");
                BluePrintsEntitiesModuleDescription unauthorisedMessageModuleDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_Unauthorised), projectSpecificKey, unauthorisedProjectModuleDescription.NavigationId, "Contact " + BluePrintsResources.ITEmail + " for authorisation", null, null, null, null, false, false, @"Business Objects\BORules_16x16.png");

                moduleAdder(projectStatusDescription, unauthorisedProjectModuleDescription, isSecurityModule, true);
                moduleAdder(unauthorisedProjectModuleDescription, unauthorisedMessageModuleDescription, isSecurityModule, true);
                return;
            }

            BluePrintsEntitiesModuleDescription projectModuleDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Dashboard), projectSpecificKey, parentId, projectTitle, "PROJECTView", new DualEntitiesParameter<PROJECT, Action<object>>(entity, NavigateCoreCommand), null, null, false, false, @"Programming\ProjectDirectory_16x16.png", projectModuleContextMenuItems, NavigateCoreCommand, false, null, "Double click to view S-Curve. Right click to access more items", entity);
            moduleAdder(projectStatusDescription, projectModuleDescription, isSecurityModule, true);
            autoInvokeProjects.Add(projectModuleDescription);

            //add a dummy menu so that accordion expand button is shown
            BluePrintsEntitiesModuleDescription dummyCategoryDescription = new BluePrintsEntitiesModuleDescription("dummy", projectSpecificKey, parentId, projectTitle, null, null, null, null, false, false, string.Empty);
            moduleAdder(projectModuleDescription, dummyCategoryDescription, isSecurityModule, true);

            if (isSecurityModule)
                populateProjectTree(entity, projectModuleDescription, isSecurityModule);
        }

        private void populateProjectTree(PROJECT entity, BluePrintsEntitiesModuleDescription projectModuleDescription, bool isSecurityModule)
        {
            //List<BluePrintsEntitiesModuleDescription> newModules = new List<BluePrintsEntitiesModuleDescription>();
            string projectTitle = entity.NUMBER + " " + entity.NAME;
            string childTitlePrefix = "[" + entity.NUMBER + "] ";
            string projectSpecificKey = entity.GUID.ToString();

            BluePrintsEntitiesModuleDescription design_category_description = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_Project_Design), projectSpecificKey, projectModuleDescription.NavigationId, "Design", null, null, null, null, false, false, @"Miscellaneous\Design_16x16.png");
            BluePrintsEntitiesModuleDescription construct_category_description = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_Project_Construct), projectSpecificKey, projectModuleDescription.NavigationId, "Construct", null, null, null, null, false, false, @"Programming\IDE_16x16.png");
            BluePrintsEntitiesModuleDescription construct_progress_category_description = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_Project_Construct_Progress), projectSpecificKey, projectModuleDescription.NavigationId, "Progress", null, null, null, null, false, false, @"Chart\Column2_16x16.png");
            BluePrintsEntitiesModuleDescription exo_category_description = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_Project_EXO), projectSpecificKey, projectModuleDescription.NavigationId, "EXO", null, null, null, null, false, false, @"Function Library\Financial_16x16.png");
            BluePrintsEntitiesModuleDescription forecast_category_description = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_Project_Forecast), projectSpecificKey, projectModuleDescription.NavigationId, "Forecast", null, null, null, null, false, false, @"Data\SelectData_16x16.png");
            BluePrintsEntitiesModuleDescription registerCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_Register), string.Empty, projectModuleDescription.NavigationId, "Registers", null, null, null, null, false, false, @"Miscellaneous\Content_16x16.png");
            BluePrintsEntitiesModuleDescription designRateCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_Project_Design_Rate), string.Empty, projectModuleDescription.NavigationId, "Rates", null, null, null, null, false, false, @"Spreadsheet\FunctionsFinancial_16x16.png");
            BluePrintsEntitiesModuleDescription constructRateCategoryDescription = new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Category_Project_Construct_Rate), string.Empty, projectModuleDescription.NavigationId, "Rates", null, null, null, null, false, false, @"Spreadsheet\FunctionsFinancial_16x16.png");
            
            moduleAdder(projectModuleDescription, design_category_description, isSecurityModule, true);
            moduleAdder(projectModuleDescription, construct_category_description, isSecurityModule, true);
            moduleAdder(projectModuleDescription, exo_category_description, isSecurityModule, true);
            moduleAdder(projectModuleDescription, forecast_category_description, isSecurityModule, true);
            moduleAdder(projectModuleDescription, registerCategoryDescription, isSecurityModule, true);

            //projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectUserDashboard", dashboardCategoryId, "Resourcing", "PROJECT_USERDashboardView", new EntitiesParameter<PROJECT>(entity), null, "Resourcing", false, false, @"Toolbox Items\Sparkline_16x16.png"));
            moduleAdder(projectModuleDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Areas), projectSpecificKey, projectModuleDescription.NavigationId, childTitlePrefix + "Areas", "AREACollectionView", new EntitiesParameter<PROJECT>(entity), null, "Areas", false, false, @"Maps\Map_16x16.png"), isSecurityModule);

            if (entity.USE_WORKPACKS)
                moduleAdder(projectModuleDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Workpacks), projectSpecificKey, projectSpecificKey, childTitlePrefix + "Workpacks", "WORKPACKCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Workpacks", false, false, @"Support\PackageProduct_16x16.png"), isSecurityModule);

            moduleAdder(design_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignDeliverables), projectSpecificKey, design_category_description.NavigationId, childTitlePrefix + "Deliverables", "BASELINE_ITEMCollectionView", new TripleEntitiesParameter<PROJECT, IAmBaseline, object>(entity, null, DeliverablesViewType.Both), null, "Deliverables", false, false, @"Spreadsheet\NameManager_16x16.png", null, null, false, null, "Add/Delete/Edit Design Deliverables"), isSecurityModule);
            moduleAdder(construct_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructionJobs), projectSpecificKey, construct_category_description.NavigationId, childTitlePrefix + "Job Setup", "BUDGET_ITEMCollectionView", new TripleEntitiesParameter<PROJECT, IAmBaseline, object>(entity, null, new KeyValuePair<DeliverablesViewType, EstimateViewMode>(DeliverablesViewType.Direct, EstimateViewMode.Budget)), null, "Job Setup", false, false, @"Business Objects\BOEmployee_16x16.png", null, null, false, null, "Add/Delete/Edit Construction Deliverables"), isSecurityModule);
            moduleAdder(construct_category_description, construct_progress_category_description, isSecurityModule, true);
            moduleAdder(construct_progress_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructProgress_Concrete), projectSpecificKey, construct_category_description.NavigationId, childTitlePrefix + "Concrete Progress", "SiteDirectProgressCollectionView", new TripleEntitiesParameter<PROJECT, PROGRESS, object>(entity, null, ScoreCardDiscipline.Concrete), null, "Concrete Progress", false, false, @"Chart\Bar2_16x16.png"), isSecurityModule);
            moduleAdder(construct_progress_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructProgress_Electrical), projectSpecificKey, construct_category_description.NavigationId, childTitlePrefix + "Electrical Progress", "SiteDirectProgressCollectionView", new TripleEntitiesParameter<PROJECT, PROGRESS, object>(entity, null, ScoreCardDiscipline.Electrical), null, "Electrical Progress", false, false, @"Chart\Bar2_16x16.png"), isSecurityModule);
            moduleAdder(construct_progress_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructProgress_Mechanical), projectSpecificKey, construct_category_description.NavigationId, childTitlePrefix + "Mechanical Progress", "SiteDirectProgressCollectionView", new TripleEntitiesParameter<PROJECT, PROGRESS, object>(entity, null, ScoreCardDiscipline.Mechanical), null, "Mechanical Progress", false, false, @"Chart\Bar2_16x16.png"), isSecurityModule);
            moduleAdder(construct_progress_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructProgress_Piping), projectSpecificKey, construct_category_description.NavigationId, childTitlePrefix + "Piping Progress", "SiteDirectProgressCollectionView", new TripleEntitiesParameter<PROJECT, PROGRESS, object>(entity, null, ScoreCardDiscipline.Piping), null, "Piping Progress", false, false, @"Chart\Bar2_16x16.png"), isSecurityModule);
            moduleAdder(construct_progress_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructProgress_Structural), projectSpecificKey, construct_category_description.NavigationId, childTitlePrefix + "Structural Progress", "SiteDirectProgressCollectionView", new TripleEntitiesParameter<PROJECT, PROGRESS, object>(entity, null, ScoreCardDiscipline.Structural), null, "Structural Progress", false, false, @"Chart\Bar2_16x16.png"), isSecurityModule);

            moduleAdder(design_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignProgress), projectSpecificKey, design_category_description.NavigationId, childTitlePrefix + "Design Progress", "OffsiteDirectProgressCollectionView", new DualEntitiesParameter<PROJECT, PROGRESS>(entity, null), null, "Progress", false, false, @"Chart\Bar2_16x16.png", null, null, false, null, "Update Progress on Deliverables on a Single Week"), isSecurityModule);
            moduleAdder(design_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignProgressDistribution), projectSpecificKey, design_category_description.NavigationId, childTitlePrefix + NavigationResources.Menu_Project_DesignProgressDistribution_Title, "OffsiteDirectDistributionCollectionView", new DualEntitiesParameter<PROJECT, PROGRESS>(entity, null), null, NavigationResources.Menu_Project_DesignProgressDistribution_Title, false, false, @"Chart\Area2_16x16.png", null, null, false, null, "Update Progress on Deliverables for Multiple Weeks"), isSecurityModule);
            moduleAdder(design_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignVariation), projectSpecificKey, design_category_description.NavigationId, childTitlePrefix + "Variations", "VARIATIONCollectionView", new DualEntitiesParameter<PROJECT, PhaseTypeClass>(entity, new PhaseTypeClass(PhaseType.Design)), null, "Variations", false, false, @"Scheduling\ShowWorkTimeOnly_16x16.png", null, null, false, null, "Add/Delete/Edit Variation for Deliverables"), isSecurityModule);
            moduleAdder(design_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignVariationQuery), projectSpecificKey, design_category_description.NavigationId, childTitlePrefix + "Variations Breakdown", "VariationQueryCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Variations Breakdown", false, false, @"Filter\PieSeries_16x16.png", null, null, false, null, "Shows Hours Broken Down By Variation for Each Deliverable"), isSecurityModule);
            moduleAdder(design_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignStatuses), projectSpecificKey, design_category_description.NavigationId, childTitlePrefix + "Deliverable Gates", "DELIVERABLES_STATUSCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Deliverable Gates", false, false, @"Business Objects\BORules_16x16.png", null, null, false, null, "Add/Delete/Edit Deliverable Gates"), isSecurityModule);

            moduleAdder(projectModuleDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Subjob), projectSpecificKey, projectModuleDescription.NavigationId, childTitlePrefix + "Sub Jobs", "SUBJOBCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Sub Jobs", false, false, @"Programming\ProjectFile_16x16.png", null, null, false, null, "Add/Delete/Edit Sub Jobs with Timelines Used in Absence of P6 Schedule"), isSecurityModule);
            moduleAdder(construct_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructionResourceAllocation), projectSpecificKey, construct_category_description.NavigationId, childTitlePrefix + "Resource Allocation", "EXO_ConstructionSubjobCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Resource Allocation", false, false, @"Business Objects\BOUser_16x16.png", null, null, false, null, "Permission Maintenance of Required Sub Jobs Scrapped From Job Setup"), isSecurityModule);
            moduleAdder(design_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignResourceAllocation), projectSpecificKey, design_category_description.NavigationId, childTitlePrefix + "Design Jobs", "EXO_DesignSubjobCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Job Permissions", false, false, @"Business Objects\BOUser_16x16.png", null, null, false, null, "Permission Maintenance of Required Sub Jobs Scrapped From Deliverables"), isSecurityModule);

            moduleAdder(exo_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_Transactions), projectSpecificKey, exo_category_description.NavigationId, childTitlePrefix + "Transactions", null, new TripleEntitiesParameter<PROJECT, object, object>(entity, false, null), null, "Transactions", false, false, @"Function Library\Compatibility_16x16.png", null, null, false, () => BluePrintsDataUtils.GetPreferredDocumentTypeName(DataUtils.GetNameOf(() => UserPreferences.EXO_PreloadTransactions)), "Shows all time and material transactions excluding cancelled"), isSecurityModule);
            moduleAdder(exo_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_Timesheets), projectSpecificKey, exo_category_description.NavigationId, childTitlePrefix + "Timesheets", "TimesheetEntryCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Timesheets", false, false, @"Function Library\Date&Time_16x16.png", null, null, false, null, "Bulk Timesheet Entry"), isSecurityModule);
            moduleAdder(exo_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_Timesheets_Query), projectSpecificKey, exo_category_description.NavigationId, childTitlePrefix + "Approved Time", "TimesheetQueryCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Timesheets Query", false, false, @"Data\SelectData_16x16.png", null, null, false, null, "Shows Approved Time in Timesheet Report Format"), isSecurityModule);
            moduleAdder(exo_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_Jobs), projectSpecificKey, exo_category_description.NavigationId, childTitlePrefix + "Budget Input", "EXO_SubJobCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Budget Input", false, false, @"Function Library\MoreFunctions_16x16.png", null, null, false, null, "Add/Delete/Edit Jobs Including Budget"), isSecurityModule);
            moduleAdder(exo_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_JobPermission), projectSpecificKey, exo_category_description.NavigationId, childTitlePrefix + "Master Job Permissions", "EXO_JobPermissionCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Master Job Permissions", false, false, @"Business Objects\BOUser_16x16.png", null, null, false, null, "Assignment of Resources Permission to Book to Jobs"), isSecurityModule);
            moduleAdder(exo_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_AllPO), projectSpecificKey, exo_category_description.NavigationId, childTitlePrefix + "Purchase Orders", "EXO_POCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Purchase Orders", false, false, @"Business Objects\BOProductGroup_16x16.png", null, null, false, null, "Query All Purchase Orders"), isSecurityModule);
            moduleAdder(exo_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_JobsHistory), projectSpecificKey, exo_category_description.NavigationId, childTitlePrefix + "Jobs History", "EXO_JobHistoryCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Jobs History", false, false, @"Find\FindCustomers_16x16.png", null, null, false, null, "Shows All Modification Done in Budget Input From BluePrints"), isSecurityModule);
            moduleAdder(exo_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_SellPrices), projectSpecificKey, exo_category_description.NavigationId, childTitlePrefix + "Sell Prices", "EXO_PricesCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Sell Prices", false, false, @"XAF\BO_Price.png", null, null, false, null, "Add/Delete/Edit Sell Rates"), isSecurityModule);

            moduleAdder(forecast_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Forecast), projectSpecificKey, forecast_category_description.NavigationId, childTitlePrefix + "Forecast", "PROJECTForecastView", new DualEntitiesParameter<PROJECT, Action<object>>(entity, NavigateCoreCommand), null, "Forecast", false, false, @"Function Library\Statistical_16x16.png"), isSecurityModule);
            moduleAdder(forecast_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Snapshot_Forecast), projectSpecificKey, forecast_category_description.NavigationId, childTitlePrefix + "Forecast Snapshot", "PROJECTForecastSnapshotView", new EntitiesParameter<PROJECT>(entity), null, "Forecast Snapshot", false, false, @"PDF Viewer\MarqueeZoom_16x16.png"), isSecurityModule);
            moduleAdder(forecast_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Forecast_PO), projectSpecificKey, forecast_category_description.NavigationId, childTitlePrefix + "PO Forecast", "PROJECTPOForecastView", new EntitiesParameter<PROJECT>(entity), null, "PO Forecast", false, false, @"Business Objects\BOOrderItem_16x16.png", null, null, false, null, "Forecasting of PO's Outstanding Amounts Excluding PO's Related to Equipment Hires"), isSecurityModule);
            moduleAdder(forecast_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Snapshot_Forecast_PO), projectSpecificKey, forecast_category_description.NavigationId, childTitlePrefix + "PO Snapshot Forecast", "PROJECTPOSnapshotForecastView", new EntitiesParameter<PROJECT>(entity), null, "PO Snapshot Forecast", false, false, @"Business Objects\BOOrderItem_16x16.png", null, null, false, null, "Forecasting of a Snapshot of PO's Outstanding Amounts Excluding PO's Related to Equipment Hires"), isSecurityModule);
            moduleAdder(forecast_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Forecast_Flat_PO), projectSpecificKey, forecast_category_description.NavigationId, childTitlePrefix + "EH PO Forecast", "PROJECTFlatPOForecastView", new EntitiesParameter<PROJECT>(entity), null, "EH PO Forecast", false, false, @"Business Objects\BOOrderItem_16x16.png", null, null, false, null, "Forecasting of PO's Outstanding Amounts Related to Equipment Hires"), isSecurityModule);
            moduleAdder(forecast_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Snapshot_Forecast_Flat_PO), projectSpecificKey, forecast_category_description.NavigationId, childTitlePrefix + "EH PO Snapshot Forecast", "PROJECTFlatPOSnapshotForecastView", new EntitiesParameter<PROJECT>(entity), null, "EH PO Snapshot Forecast", false, false, @"Business Objects\BOOrderItem_16x16.png", null, null, false, null, "Forecasting of a Snapshot of PO's Outstanding Amounts Related to Equipment Hires"), isSecurityModule);
            moduleAdder(forecast_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Snapshot_PO_Invoice), projectSpecificKey, forecast_category_description.NavigationId, childTitlePrefix + "PO Invoice", "PROJECTPOInvoicedView", new EntitiesParameter<PROJECT>(entity), null, "PO Invoice", false, false, @"Toolbox Items\Sparkline_16x16.png", null, null, false, null, "Show PO invoiced by month"), isSecurityModule);
            moduleAdder(forecast_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Forecast_Indirect), projectSpecificKey, forecast_category_description.NavigationId, childTitlePrefix + "Indirects Forecast", "PROJECTIndirectForecastView", new EntitiesParameter<PROJECT>(entity), null, "Indirects Forecast", false, false, @"Function Library\Information_16x16.png"), isSecurityModule);
            moduleAdder(forecast_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Forecast_Claims), projectSpecificKey, forecast_category_description.NavigationId, childTitlePrefix + "Claims", "PROJECT_REVENUECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Claims", false, false, @"Number Formats\Accounting_16x16.png"), isSecurityModule);
            moduleAdder(forecast_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Forecast_AllEAC), projectSpecificKey, forecast_category_description.NavigationId, childTitlePrefix + "View EAC's", "PROJECTForecastEACReportView", new EntitiesParameter<PROJECT>(entity), null, "View EAC's", false, false, @"Scheduling\FullWeekView_16x16.png"), isSecurityModule);

            moduleAdder(projectModuleDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Holidays), projectSpecificKey, projectModuleDescription.NavigationId, childTitlePrefix + "Holidays", "HOLIDAYCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Holidays", false, false, @"Scheduling\Calendar_16x16.png", null, null, false, null, "Add/Delete Holiday Dates Used When Generating S-Curve"), isSecurityModule);

            moduleAdder(registerCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Register_Issue), projectSpecificKey, registerCategoryDescription.NavigationId, childTitlePrefix + "Issues Register", "REGISTER_ISSUECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Issues Register", false, false, @"Support\Issue_16x16.png"), isSecurityModule);
            moduleAdder(registerCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Register_Change), projectSpecificKey, registerCategoryDescription.NavigationId, childTitlePrefix + "Change Register", "REGISTER_CHANGECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Change Register", false, false, @"Scheduling\ChangeStatus_16x16.png"), isSecurityModule);
            moduleAdder(registerCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Register_Risk), projectSpecificKey, registerCategoryDescription.NavigationId, childTitlePrefix + "Risk Register", "REGISTER_RISKCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Risk Register", false, false, @"Support\BreakingChange_16x16.png"), isSecurityModule);
            moduleAdder(registerCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Register_Hold), projectSpecificKey, registerCategoryDescription.NavigationId, childTitlePrefix + "Hold Register", "REGISTER_HOLDCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Hold Register", false, false, @"Business Objects\BORules_16x16.png"), isSecurityModule);
            moduleAdder(registerCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Register_LessonsLearned), projectSpecificKey, registerCategoryDescription.NavigationId, childTitlePrefix + "Lesson's Learned Register", "REGISTER_LLCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Lesson's Learned Register", false, false, @"Business Objects\BOTask_16x16.png"), isSecurityModule);
            moduleAdder(registerCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Register_NonConformance), projectSpecificKey, registerCategoryDescription.NavigationId, childTitlePrefix + "Non-Conformance Register", "REGISTER_NCCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Non-Conformance Register", false, false, @"Programming\BugReport_16x16.png"), isSecurityModule);
            moduleAdder(registerCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Register_TQ), projectSpecificKey, registerCategoryDescription.NavigationId, childTitlePrefix + "TQ Register", "REGISTER_TQCollectionView", new EntitiesParameter<PROJECT>(entity), null, "TQ Register", false, false, @"Reports\Parameters_16x16.png"), isSecurityModule);

            moduleAdder(design_category_description, designRateCategoryDescription, isSecurityModule, true);
            moduleAdder(designRateCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignDirectCostRate), projectSpecificKey, designRateCategoryDescription.NavigationId, childTitlePrefix + "Design Cost Rates [Direct]", "CostRATECollectionView", new TripleEntitiesParameter<PROJECT, object, object>(entity, PhaseType.Design, ChargeType.Chargeable), null, "Cost Rates [Direct]", false, false, @"Business Objects\BOPerson_16x16.png", null, null, false, null, "Add/Delete/Edit Design Cost Rates and Enable Rates Scrapped from EXO"), isSecurityModule);
            moduleAdder(designRateCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignIndirectCostRate), projectSpecificKey, designRateCategoryDescription.NavigationId, childTitlePrefix + "Cost Rates [Chargeable Indirect]", "CostRATECollectionView", new TripleEntitiesParameter<PROJECT, object, object>(entity, PhaseType.Indirect, ChargeType.Chargeable), null, "Cost Rates [Chargeable Indirect]", false, false, @"Business Objects\BODetails_16x16.png", null, null, false, null, "Add/Delete/Edit Indirect Cost Rates and Enable Rates Scrapped from EXO"), isSecurityModule);

            moduleAdder(designRateCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignDirectChargeRate), projectSpecificKey, designRateCategoryDescription.NavigationId, childTitlePrefix + "Charge Rates [Direct]", "RATECollectionView", new TripleEntitiesParameter<PROJECT, object, object>(entity, PhaseType.Design, ChargeType.Chargeable), null, "Charge Rates [Direct]", false, false, @"Business Objects\BOPerson_16x16.png", null, null, false, null, "Add/Delete/Edit Construction Chargea Rates"), isSecurityModule);
            moduleAdder(designRateCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignIndirectChargeRate), projectSpecificKey, designRateCategoryDescription.NavigationId, childTitlePrefix + "Charge Rates [Chargeable Indirect]", "RATECollectionView", new TripleEntitiesParameter<PROJECT, object, object>(entity, PhaseType.Indirect, ChargeType.Chargeable), null, "Charge Rates [Chargeable Indirect]", false, false, @"Business Objects\BODetails_16x16.png", null, null, false, null, "Add/Delete/Edit Indirect Charge Rates"), isSecurityModule);

            moduleAdder(construct_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructionVariation), projectSpecificKey, construct_category_description.NavigationId, childTitlePrefix + "Variations", "VARIATION_CONSTRUCTIONCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Variations", false, false, @"Scheduling\ShowWorkTimeOnly_16x16.png", null, null, false, null, "Consruction Variation Register"), isSecurityModule);
            moduleAdder(construct_category_description, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructionStage), projectSpecificKey, construct_category_description.NavigationId, childTitlePrefix + "Stages", "CONSTRUCTION_STAGECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Stages", false, false, @"XAF\Action_Totals_Column.png", null, null, false, null, "Add/Delete/Edit Stages for Each Discipline for Construction Progress"), isSecurityModule);

            moduleAdder(construct_category_description, constructRateCategoryDescription, isSecurityModule, true);
            moduleAdder(constructRateCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructionDirectCostRate), projectSpecificKey, constructRateCategoryDescription.NavigationId, childTitlePrefix + "Construction Cost Rates [Direct]", "CostRATECollectionView", new TripleEntitiesParameter<PROJECT, object, object>(entity, PhaseType.Construct, ChargeType.Chargeable), null, "Cost Rates [Direct]", false, false, @"Business Objects\BOEmployee_16x16.png", null, null, false, null, "Add/Delete/Edit Construction Cost Rates and Enable Rates Scrapped from EXO"), isSecurityModule);
            moduleAdder(constructRateCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructionIndirectCostRate), projectSpecificKey, designRateCategoryDescription.NavigationId, childTitlePrefix + "Cost Rates [Chargeable Indirect]", "CostRATECollectionView", new TripleEntitiesParameter<PROJECT, object, object>(entity, PhaseType.Indirect, ChargeType.Chargeable), null, "Cost Rates [Chargeable Indirect]", false, false, @"Business Objects\BODetails_16x16.png", null, null, false, null, "Add/Delete/Edit Indirect Cost Rates and Enable Rates Scrapped from EXO"), isSecurityModule);
            moduleAdder(constructRateCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructionMaintenanceCostRate), projectSpecificKey, constructRateCategoryDescription.NavigationId, childTitlePrefix + "Maintenance Cost Rates [Direct]", "CostRATECollectionView", new TripleEntitiesParameter<PROJECT, object, object>(entity, PhaseType.Maintenance, ChargeType.Chargeable), null, "Cost Rates [Maintenance]", false, false, @"Business Objects\BODepartment_16x16.png", null, null, false, null, "Add/Delete/Edit Maintenace Cost Rates and Enable Rates Scrapped from EXO"), isSecurityModule);
            moduleAdder(constructRateCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructionDirectChargeRate), projectSpecificKey, constructRateCategoryDescription.NavigationId, childTitlePrefix + "Construction Charge Rates [Direct]", "RATECollectionView", new TripleEntitiesParameter<PROJECT, object, object>(entity, PhaseType.Construct, ChargeType.Chargeable), null, "Charge Rates [Direct]", false, false, @"Business Objects\BOPerson_16x16.png", null, null, false, null, "Add/Delete/Edit Construction Direct Charge Rates"), isSecurityModule);
            moduleAdder(constructRateCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructionIndirectChargeRate), projectSpecificKey, designRateCategoryDescription.NavigationId, childTitlePrefix + "Charge Rates [Chargeable Indirect]", "RATECollectionView", new TripleEntitiesParameter<PROJECT, object, object>(entity, PhaseType.Indirect, ChargeType.Chargeable), null, "Charge Rates [Chargeable Indirect]", false, false, @"Business Objects\BODetails_16x16.png", null, null, false, null, "Add/Delete/Edit Construction Indirect Charge Rates"), isSecurityModule);
            moduleAdder(constructRateCategoryDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_ConstructionMaintenanceChargeRate), projectSpecificKey, constructRateCategoryDescription.NavigationId, childTitlePrefix + "Maintenance Charge Rates [Direct]", "RATECollectionView", new TripleEntitiesParameter<PROJECT, object, object>(entity, PhaseType.Maintenance, ChargeType.Chargeable), null, "Charge Rates [Maintenance]", false, false, @"Business Objects\BODepartment_16x16.png", null, null, false, null, "Add/Delete/Edit Maintenace Charge Rates"), isSecurityModule);

            moduleAdder(projectModuleDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DisciplineDescription), projectSpecificKey, projectModuleDescription.NavigationId, childTitlePrefix + "Packages", "DISCIPLINE_DESCCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Packages", false, false, @"Support\PackageProduct_16x16.png", null, null, false, null, "Customise Discipline Code Description as Procurement Packages"), isSecurityModule);
            moduleAdder(projectModuleDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Contractor), projectSpecificKey, projectModuleDescription.NavigationId, childTitlePrefix + "Contractors", "PROJECT_CONTRACTORCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Contractors", false, false, @"XAF\BO_Department.png", null, null, false, null, "Add/Delete/Edit Project's Contractor"), isSecurityModule);
            moduleAdder(projectModuleDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_HSE), projectSpecificKey, projectModuleDescription.NavigationId, childTitlePrefix + "HSE", "HSESingleObjectView", new EntitiesParameter<PROJECT>(entity), null, "HSE", false, false, @"Function Library\Statistical_16x16.png"), isSecurityModule);
            moduleAdder(projectModuleDescription, new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Menu_Project_HSEReport), projectSpecificKey, projectModuleDescription.NavigationId, childTitlePrefix + "Monthly HSE Report", "HSECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Monthly HSE Report", false, false, @"Gauges\GaugeStyleLinearHorizontal_16x16.png"), isSecurityModule);

            //remove category when there isn't any child assigned
            moduleRemover(projectModuleDescription, design_category_description, isSecurityModule);
            moduleRemover(projectModuleDescription, construct_category_description, isSecurityModule);
            moduleRemover(projectModuleDescription, exo_category_description, isSecurityModule);
            moduleRemover(projectModuleDescription, forecast_category_description, isSecurityModule);
            moduleRemover(projectModuleDescription, registerCategoryDescription, isSecurityModule);
            moduleRemover(projectModuleDescription, designRateCategoryDescription, isSecurityModule);
        }

        private void moduleRemover(BluePrintsEntitiesModuleDescription parentModule, BluePrintsEntitiesModuleDescription existingModule, bool isSecurityModule)
        {
            if(isSecurityModule)
            {
                if (!Modules.Any(x => x.ParentId == existingModule.NavigationId) && Modules.Any(x => x.NavigationId == existingModule.NavigationId))
                    Modules.Remove(existingModule);
            }
            else
            {
                if (existingModule.ChildModules.Count == 0 && parentModule.ChildModules.Contains(existingModule))
                    parentModule.ChildModules.Remove(existingModule);
            }
        }

        private List<BluePrintsEntitiesModuleDescription> getAllNodes(BluePrintsEntitiesModuleDescription parentModule)
        {
            List<BluePrintsEntitiesModuleDescription> result = new List<BluePrintsEntitiesModuleDescription>();
            result.Add(parentModule);
            foreach (BluePrintsEntitiesModuleDescription child in parentModule.ChildModules)
            {
                result.AddRange(getAllNodes(child));
            }

            return result;
        }

        private void moduleAdder(BluePrintsEntitiesModuleDescription parentModule, BluePrintsEntitiesModuleDescription newModule, bool isSecurityModule, bool isCompulsory = false)
        {
            if (!isCompulsory && LoginCredentials.getPermissionStatus(newModule.SecurityKey) == LoginCredentials.PermissionStatus.None)
                return;

            if (isSecurityModule)
            {
                Modules.Add(newModule);
                permissionAdder(newModule.SecurityKey, newModule.NavigationId);
            }
            else
                parentModule.ChildModules.Add(newModule);
        }

        private void permissionAdder(string securityKey, string parentNavigationId)
        {
            if (securityKey == DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignDeliverables))
            {
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_InternalNumbersApproval), string.Empty, parentNavigationId, NavigationResources.Permission_DesignDeliverables_InternalNumbersApproval, NavigationResources.Permission_DesignDeliverables_InternalNumbersApproval));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_InternalNumbersUnapproval), string.Empty, parentNavigationId, NavigationResources.Permission_DesignDeliverables_InternalNumbersUnapproval, NavigationResources.Permission_DesignDeliverables_InternalNumbersUnapproval));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_BookTimeForOthers), string.Empty, parentNavigationId, NavigationResources.Permission_DesignDeliverables_BookTimeForOthers, NavigationResources.Permission_DesignDeliverables_BookTimeForOthers));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_FinaliseNumbers), string.Empty, parentNavigationId, NavigationResources.Permission_DesignDeliverables_FinaliseNumbers, NavigationResources.Permission_DesignDeliverables_FinaliseNumbers));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_LockUnlockBudget), string.Empty, parentNavigationId, NavigationResources.Permission_DesignDeliverables_LockUnlockBudget, NavigationResources.Permission_DesignDeliverables_LockUnlockBudget));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_DeleteVariationDeliverables), string.Empty, parentNavigationId, NavigationResources.Permission_DesignDeliverables_DeleteVariationDeliverables, NavigationResources.Permission_DesignDeliverables_DeleteVariationDeliverables));
            }
            if (securityKey == DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignVariation))
            {
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignVariation_ApproveVariations), string.Empty, parentNavigationId, NavigationResources.Permission_DesignVariation_ApproveVariations, NavigationResources.Permission_DesignVariation_ApproveVariations));
            }
            else if (securityKey == DataUtils.GetNameOf(() => NavigationResources.Menu_Project_DesignProgress))
            {
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_UpdateProgressByStatus), string.Empty, parentNavigationId, NavigationResources.Permission_DesignDeliverables_UpdateProgressByStatus, NavigationResources.Permission_DesignDeliverables_UpdateProgressByStatus));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_ProgressPreviousWeeksDate), string.Empty, parentNavigationId, NavigationResources.Permission_DesignDeliverables_ProgressPreviousWeeksDate, NavigationResources.Permission_DesignDeliverables_ProgressPreviousWeeksDate));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_DesignDeliverables_CanDateBackwardForward), string.Empty, parentNavigationId, NavigationResources.Permission_DesignDeliverables_CanDateBackwardForward, NavigationResources.Permission_DesignDeliverables_CanDateBackwardForward));
            }
            else if (securityKey == DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_Transactions))
            {
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_Transactions_ShowCosts), securityKey, parentNavigationId, NavigationResources.Permission_EXO_Transactions_ShowCosts, NavigationResources.Permission_EXO_Transactions_ShowCosts));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_Transactions_ChangeQuantity), securityKey, parentNavigationId, NavigationResources.Permission_EXO_Transactions_ChangeQuantity, NavigationResources.Permission_EXO_Transactions_ChangeQuantity));
            }
            else if (securityKey == DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_Timesheets))
            {
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_Timesheets_Commit), securityKey, parentNavigationId, NavigationResources.Permission_EXO_Timesheets_Commit, NavigationResources.Permission_EXO_Timesheets_Commit));
            }
            else if (securityKey == DataUtils.GetNameOf(() => NavigationResources.Menu_UserTimesheet))
            {
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_UserTimesheet_Unsubmit), securityKey, parentNavigationId, NavigationResources.Permission_EXO_UserTimesheet_Unsubmit, NavigationResources.Permission_EXO_UserTimesheet_Unsubmit));
            }
            else if (securityKey == DataUtils.GetNameOf(() => NavigationResources.Menu_Project_EXO_Jobs))
            {
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_EXO_ChangeBudget), securityKey, parentNavigationId, NavigationResources.Permission_EXO_ChangeBudget, NavigationResources.Permission_EXO_ChangeBudget));
            }
            else if (securityKey == DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Forecast) || securityKey == DataUtils.GetNameOf(() => NavigationResources.Menu_Project_Snapshot_Forecast))
            {
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_Forecast_MoveDataDate), string.Empty, parentNavigationId, NavigationResources.Permission_Forecast_MoveDataDate, NavigationResources.Permission_Forecast_MoveDataDate));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_Forecast_SaveProjectBudget), string.Empty, parentNavigationId, NavigationResources.Permission_Forecast_SaveProjectBudget, NavigationResources.Permission_Forecast_SaveProjectBudget));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_Forecast_SaveEAC), string.Empty, parentNavigationId, NavigationResources.Permission_Forecast_SaveEAC, NavigationResources.Permission_Forecast_SaveEAC));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_Forecast_EditPreviousEAC), securityKey, parentNavigationId, NavigationResources.Permission_Forecast_EditPreviousEAC, NavigationResources.Permission_Forecast_EditPreviousEAC));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_ConstructionUncommitted), securityKey, parentNavigationId, NavigationResources.Permission_ConstructionUncommitted, NavigationResources.Permission_ConstructionUncommitted));
                Modules.Add(new BluePrintsEntitiesModuleDescription(DataUtils.GetNameOf(() => NavigationResources.Permission_Forecast_FilterUninvoicedOnly), securityKey, parentNavigationId, NavigationResources.Permission_Forecast_FilterUninvoicedOnly, NavigationResources.Permission_Forecast_FilterUninvoicedOnly));
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
        public readonly PROJECT PROJECT;
        public BluePrintsEntitiesModuleDescription(string id, string projectSpecificKey, string parentId, string title, string documentType = null, object documentParameter = null, ImageSource image = null, string navigationTitle = null, bool treeViewIsExpanded = true, bool showInCollapseMode = false, string imagePath = "", List<BluePrintsEntitiesModuleDescription> menuItems = null, Action<object> navigateAction = null, bool showAnimation = false, Func<string> preferredDocumentType = null, string toolTip = "", PROJECT project = null)
            : base(id, projectSpecificKey, parentId, title, documentType, documentParameter, image, navigationTitle, treeViewIsExpanded, showInCollapseMode, preferredDocumentType)
        {
            if (toolTip != string.Empty)
                this.ToolTip = toolTip;
            else if (CanNavigate)
                this.ToolTip = "Double Click to Open " + title;
            else
                this.ToolTip = "Click Arrow on the Left to Expand Category";

            this.menuItems = menuItems;
            this.navigateAction = navigateAction;
            ChildModules = new RangeObservableCollection<BluePrintsEntitiesModuleDescription>();
            this.Animate = showAnimation;
            //common paths are local images
            if(imagePath != string.Empty)
            {
                if (!imagePath.ToUpper().Contains("COMMON"))
                    Image = new BitmapImage(new Uri("pack://application:,,,/DevExpress.Images.v19.2;component/Images/" + imagePath));
                else
                    Image = new BitmapImage(new Uri(imagePath, UriKind.Relative));
            }
            else
            {
                if (!CanNavigate)
                    Image = new BitmapImage(new Uri("pack://application:,,,/DevExpress.Images.v19.2;component/Images/Data/ManageDataSource_16x16.png"));
                    //new Uri(@"/Common/Images/PRIMERO.jpg", UriKind.Relative));
                else
                    Image = new BitmapImage(new Uri("pack://application:,,,/DevExpress.Images.v19.2;component/Images/Actions/Open_16x16.png"));
            }

            this.PROJECT = project;
        }

        public string Caption => this.NavigationTitle;

        public override string ToString()
        {
            return this.NavigationTitle;
        }

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

                        newMenuItem.Header = item.NavigationTitle;
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