using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Document;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
        private readonly CollectionViewModel<PROJECT, Guid, IBluePrintsEntitiesUnitOfWork> _projectCollectionViewModel;

        /// <summary>
        ///     Initializes a new instance of the BluePrintsEntitiesViewModel class.
        ///     This constructor is declared protected to avoid undesired instantiation of the BluePrintsEntitiesViewModel type
        ///     without the POCO proxy factory.
        /// </summary>
        protected BluePrintsEntitiesViewModel()
            : base(BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory())
        {
            _projectCollectionViewModel =
                CollectionViewModel<PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>.CreateCollectionViewModel(
                    unitOfWorkFactory, x => x.PROJECTS);
            _projectCollectionViewModel.OnEntitiesLoadedCallBack = OnEntitiesLoadedCallBack;
            _projectCollectionViewModel.OnAfterEntitiesChangedCallBack = OnAfterEntitiesChanged;
            _projectCollectionViewModel.Entities.ToList();
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
            string themeName = Properties.Settings.Default["ThemeName"] as string;
            if(themeName == "")
                themeName = "Office2016Colorful";
            ApplicationThemeHelper.ApplicationThemeName = themeName;
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
            MainThreadDispatcher.BeginInvoke(new Action(() => CreateProjectModules(entities)));
            _projectCollectionViewModel.OnEntitiesLoadedCallBack = null;
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

            if (entities.Any(x => x.STATUS == ProjectStatus.Tender))
                projectCategoryHeader.ChildModules.Add(projectTenderCategoryDescription);

            newModules.AddRange(CreateDataModules());
            var projects =
            entities.Where(x => x.STATUS == ProjectStatus.Active || x.STATUS == ProjectStatus.Tender)
                .OrderBy(x => x.NUMBER)
                .ToArray()
                .AsEnumerable();

            if (projects.Any())
            {
                foreach (var project in projects)
                {
                    newModules.AddRange(CreateProjectTree(project));
                }
            }

            Modules.AddRange(newModules);
        }

        private void ShowDashboardModule()
        {
            if (LoginCredentials.hasPermission(PermissionResources.ViewDashboard))
            {
                var dashboard = Modules.FirstOrDefault(x => x.DocumentType == "PROJECTDashboardView");
                if (dashboard != null)
                    NavigateCore(dashboard);
            }
        }

        const string projectViewIdPrefix = "View_Project";
        private void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            //if (_projectCollectionViewModel == null)
            //    return;

            //var primaryKey = (Guid)key;
            //string projectViewId = projectViewIdPrefix + primaryKey.ToString();
            //string projectPrimaryKey = primaryKey.ToString();
            //var projectModules = Modules.Where(x => x.Id.ToString() == projectViewId || (x.ParentId != null && x.ParentId.ToString().Contains(projectPrimaryKey)));

            //if (messageType == EntityMessageType.Added || messageType == EntityMessageType.Changed)
            //{
            //    PROJECT project = _projectCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == primaryKey);
            //    if (project != null)
            //        if (messageType == EntityMessageType.Added)
            //        {
            //            if (projectModules.Count() == 0)
            //                Modules.InsertRangeBackground(CreateProjectTree(project));
            //        }
            //        else if (messageType == EntityMessageType.Changed)
            //        {

            //            if (projectModules.Count() > 0)
            //                Modules.RemoveRangeBackground(projectModules.ToArray());

            //            if(project.STATUS == ProjectStatus.Active || project.STATUS == ProjectStatus.Tender)
            //            {
            //                Modules.InsertRangeBackground(CreateProjectTree(project));
            //            }
            //        }
            //        else
            //            Modules.RemoveRangeBackground(projectModules.ToArray());
            //}
            //else
            //    Modules.RemoveRangeBackground(projectModules.ToArray());
        }

        const string projectCategoryId = "View_Projects";
        const string dataCategoryId = "Category_Data";
        const string activeCategoryId = "Category_Active";
        const string tenderCategoryId = "Category_Tender";
        const string myProjectCategoryId = "Category_MyProject";
        const string myTenderCategoryId = "Category_MyTender";
        const string commodityCodeCategoryId = "Category_CommodityCode";
        BluePrintsEntitiesModuleDescription projectEditableCategoryDescription = new BluePrintsEntitiesModuleDescription(projectCategoryId, null, "Projects", "PROJECTCollectionView");
        BluePrintsEntitiesModuleDescription projectCategoryDescription = new BluePrintsEntitiesModuleDescription(projectCategoryId, null, "Projects");
        BluePrintsEntitiesModuleDescription myProjectsCategoryDescription = new BluePrintsEntitiesModuleDescription(myProjectCategoryId, projectCategoryId, "My Projects");
        BluePrintsEntitiesModuleDescription myTendersCategoryDescription = new BluePrintsEntitiesModuleDescription(myTenderCategoryId, projectCategoryId, "My Tenders");
        BluePrintsEntitiesModuleDescription projectActiveCategoryDescription = new BluePrintsEntitiesModuleDescription(activeCategoryId, projectCategoryId, "Active");
        BluePrintsEntitiesModuleDescription projectTenderCategoryDescription = new BluePrintsEntitiesModuleDescription(tenderCategoryId, projectCategoryId, "Tender", null, null, null, null, false);
        BluePrintsEntitiesModuleDescription dataCategoryDescription = new BluePrintsEntitiesModuleDescription(dataCategoryId, null, "Data", null, null, null, null, false);
        BluePrintsEntitiesModuleDescription commodityCategoryDescription = new BluePrintsEntitiesModuleDescription(commodityCodeCategoryId, null, "Commodity Code", null, null, null, null, false);
        protected override BluePrintsEntitiesModuleDescription[] CreateModules()
        {
            List<BluePrintsEntitiesModuleDescription> bluePrintsEntitiesModuleDescriptions = new List<BluePrintsEntitiesModuleDescription>();

            if (LoginCredentials.hasPermission(PermissionResources.ViewDashboard))
                bluePrintsEntitiesModuleDescriptions.Add(new BluePrintsEntitiesModuleDescription("View_Dashboard", null, "Dashboard", "PROJECTDashboardView"));

            bluePrintsEntitiesModuleDescriptions.Add(new BluePrintsEntitiesModuleDescription("View_UserDashboard", null, "User Dashboard", "USERDashboardView", new EntitiesParameter<USER>(LoginCredentials.CurrentUser)));
            
            return bluePrintsEntitiesModuleDescriptions.ToArray();
        }

        private List<BluePrintsEntitiesModuleDescription> CreateDataModules()
        {
            List<BluePrintsEntitiesModuleDescription> bluePrintsEntitiesModuleDescriptions = new List<BluePrintsEntitiesModuleDescription>();

            bluePrintsEntitiesModuleDescriptions.Add(dataCategoryDescription);

            if (LoginCredentials.hasPermission(PermissionResources.ManageDepartment))
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_Departments", dataCategoryId, "Departments", "DEPARTMENTCollectionView"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDiscipline))
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_Disciplines", dataCategoryId, "Disciplines", "DISCIPLINECollectionView"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDocType))
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_DocTypes", dataCategoryId, "Document Types", "DOCTYPECollectionView"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageUser))
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_Users", dataCategoryId, "Users", "USERCollectionView"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageRole))
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_Roles", dataCategoryId, "Roles", "ROLECollectionView"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDeliverableStatuses))
                dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_DeliverableStatuses", dataCategoryId, "Deliverable Statuses", "DELIVERABLES_STATUSCollectionView"));

            dataCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_UOMs", dataCategoryId, "Unit of Measures", "UOMCollectionView"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageCommodity))
            {
                dataCategoryDescription.ChildModules.Add(commodityCategoryDescription);
                commodityCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageCommodity_Design", commodityCodeCategoryId, "Commodity Code [Design]", "COMMODITY_CODECollectionView", new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null, new CommodityCodeTypeClass(CommodityCodeType.Design)), null, "Design"));
                commodityCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageCommodity_Direct", commodityCodeCategoryId, "Commodity Code [Direct]", "COMMODITY_CODECollectionView", new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null, new CommodityCodeTypeClass(CommodityCodeType.Direct)), null, "Direct"));
                commodityCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageCommodity_DirectGroup", commodityCodeCategoryId, "Commodity Group [Direct]", "COMMODITY_GROUP_DIRECTCollectionView", null, null, "Direct Group"));
                commodityCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageCommodity_Indirect", commodityCodeCategoryId, "Commodity Code [Indirect]", "COMMODITY_CODECollectionView", new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null, new CommodityCodeTypeClass(CommodityCodeType.Indirect)), null, "Indirect"));
                commodityCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ManageCommodity_Overhead", commodityCodeCategoryId, "Commodity Code [Overhead]", "COMMODITY_CODECollectionView", new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null, new CommodityCodeTypeClass(CommodityCodeType.Overhead)), null, "Overhead"));
            }

            return bluePrintsEntitiesModuleDescriptions;
        }
        
        private IEnumerable<BluePrintsEntitiesModuleDescription> CreateProjectTree(PROJECT entity)
        {
            List<BluePrintsEntitiesModuleDescription> newModules = new List<BluePrintsEntitiesModuleDescription>();
            string projectTitle = entity.NUMBER + " " + entity.NAME;
            string childTitlePrefix = "[" + entity.NUMBER + "] ";
            string keyString = entity.EntityKey.ToString();
            string projectKey = projectViewIdPrefix + keyString;

            object parentId;
            BluePrintsEntitiesModuleDescription projectStatusDescription;

            if((entity.STATUS == ProjectStatus.Active || entity.STATUS == ProjectStatus.Tender) && entity.GUID_MANAGEUSER == LoginCredentials.CurrentUserGuid)
            {
                if(entity.STATUS == ProjectStatus.Active)
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
            else
            {
                projectStatusDescription = projectTenderCategoryDescription;
                parentId = tenderCategoryId;
            }

            BluePrintsEntitiesModuleDescription projectModuleDescription = new BluePrintsEntitiesModuleDescription(projectKey, parentId, projectTitle, "PROJECTView", new EntitiesParameter<PROJECT>(entity), null, null, false);
            projectStatusDescription.ChildModules.Add(projectModuleDescription);

            if (LoginCredentials.hasPermission(PermissionResources.ManageAreaAndPhases))
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectAreas" + keyString, projectKey, childTitlePrefix + "Areas", "AREACollectionView", new EntitiesParameter<PROJECT>(entity), null, "Areas"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageRate))
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectRates" + keyString, projectKey, childTitlePrefix + "Rates", "RATECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Rates"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageWorkpack))
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectWorkpacks" + keyString, projectKey, childTitlePrefix + "Workpacks", "WORKPACKCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Workpacks"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageBaseline))
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectDeliverables" + keyString, projectKey, childTitlePrefix + "Deliverables", "BASELINE_ITEMCollectionView", new OptionalEntitiesParameter<PROJECT, BASELINE>(entity, null), null, "Deliverables"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageVariation))
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectVariations" + keyString, projectKey, childTitlePrefix + "Variations", "VARIATIONCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Variations"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageProgress))
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectProgresses" + keyString, projectKey, childTitlePrefix + "Progresses", "PROGRESS_ITEMCollectionView", new OptionalEntitiesParameter<PROJECT, PROGRESS>(entity, null), null, "Progresses"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDeliverableStatuses))
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectDeliverableStatuses" + keyString, projectKey, childTitlePrefix + "Deliverable Statuses", "DELIVERABLES_STATUSCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Deliverable Statuses"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageEstimation))
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectEstimates" + keyString, projectKey, childTitlePrefix + "Estimates", "ESTIMATION_DIRECT_ITEMCollectionView", new OptionalEntitiesParameter<PROJECT, ESTIMATION_DIRECT>(entity, null), null, "Direct Estimate"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageCommodity))
                projectModuleDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectCommodities" + keyString, projectKey, childTitlePrefix + "Commodities", "COMMODITY_GROUP_CODECollectionView", new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(entity, new CommodityCodeTypeClass(CommodityCodeType.Direct)), null, "Direct Commodities"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageRegisters))
            {
                string registerCategoryId = "View_RegisterCategory" + keyString;
                
                BluePrintsEntitiesModuleDescription registerCategoryDescription = new BluePrintsEntitiesModuleDescription(registerCategoryId, projectKey, "Registers", null, null, null, null, false);
                projectModuleDescription.ChildModules.Add(registerCategoryDescription);
                registerCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_IssueRegister" + keyString, registerCategoryId, childTitlePrefix + "Issue Register", "REGISTER_ISSUECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Issue Register"));
                registerCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_ChangeRegister" + keyString, registerCategoryId, childTitlePrefix + "Change Register", "REGISTER_CHANGECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Change Register"));
                registerCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_RiskRegister" + keyString, registerCategoryId, childTitlePrefix + "Risk Register", "REGISTER_RISKCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Risk Register"));
                registerCategoryDescription.ChildModules.Add(new BluePrintsEntitiesModuleDescription("View_HoldRegister" + keyString, registerCategoryId, childTitlePrefix + "Hold Register", "REGISTER_HOLDCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Hold Register"));
            }


            return newModules;
        }
    }
}

namespace BluePrints.Common.ViewModel
{
    public partial class BluePrintsEntitiesModuleDescription : ModuleDescription<BluePrintsEntitiesModuleDescription>
    {
        public BluePrintsEntitiesModuleDescription(object id, object parentId, string title, string documentType = null, object documentParameter = null, ImageSource image = null, string navigationTitle = null, bool treeViewIsExpanded = true)
            : base(id, parentId, title, documentType, documentParameter, image, navigationTitle, treeViewIsExpanded)
        {
            ChildModules = new RangeObservableCollection<BluePrintsEntitiesModuleDescription>();

            if (!CanNavigate)
                Image = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/DevExpress.Images.v17.1;component/Images/Data/ManageDataSource_16x16.png"));
            else
                Image = new BitmapImage(
                    new Uri("pack://application:,,,/DevExpress.Images.v17.1;component/Images/Actions/Open_16x16.png"));
        }

        public RangeObservableCollection<BluePrintsEntitiesModuleDescription> ChildModules { get; set; }

        public override string ToString()
        {
            return this.NavigationTitle;
        }
    }
}