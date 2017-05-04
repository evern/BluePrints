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
            if (LoginCredentials.hasPermission(PermissionResources.ManageProject))
                newModules.Add(new BluePrintsEntitiesModuleDescription(projectCategoryId, null, "Projects", "PROJECTCollectionView"));
            else
                newModules.Add(new BluePrintsEntitiesModuleDescription(projectCategoryId, null, "Projects"));

            if (entities.Any(x => x.STATUS == ProjectStatus.Active))
                newModules.Add(new BluePrintsEntitiesModuleDescription(activeCategoryId, projectCategoryId, "Active"));

            if (entities.Any(x => x.STATUS == ProjectStatus.Tender))
                newModules.Add(new BluePrintsEntitiesModuleDescription(tenderCategoryId, projectCategoryId, "Tender"));

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
            if (_projectCollectionViewModel == null)
                return;

            var primaryKey = (Guid)key;
            string projectViewId = projectViewIdPrefix + primaryKey.ToString();
            var projectModules = Modules.Where(x => x.Id.ToString() == projectViewId || (x.ParentId != null && x.ParentId.ToString() == projectViewId));

            if (messageType == EntityMessageType.Added || messageType == EntityMessageType.Changed)
            {
                PROJECT project = _projectCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == primaryKey);
                if (project != null)
                    if (messageType == EntityMessageType.Added)
                    {
                        if (projectModules.Count() == 0)
                            Modules.InsertRangeBackground(CreateProjectTree(project));
                    }
                    else if (messageType == EntityMessageType.Changed)
                    {

                        if (projectModules.Count() > 0)
                            Modules.RemoveRangeBackground(projectModules.ToArray());

                        if(project.STATUS == ProjectStatus.Active || project.STATUS == ProjectStatus.Tender)
                        {
                            Modules.InsertRangeBackground(CreateProjectTree(project));
                        }
                    }
                    else
                        Modules.RemoveRangeBackground(projectModules.ToArray());
            }
            else
                Modules.RemoveRangeBackground(projectModules.ToArray());
        }

        const string projectCategoryId = "View_Projects";
        const string dataCategoryId = "Category_Data";
        const string activeCategoryId = "Category_Active";
        const string tenderCategoryId = "Category_Tender";
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

            bluePrintsEntitiesModuleDescriptions.Add(new BluePrintsEntitiesModuleDescription(dataCategoryId, null, "Data", null, null, null, null, false));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDepartment))
                bluePrintsEntitiesModuleDescriptions.Add(new BluePrintsEntitiesModuleDescription("View_Departments", dataCategoryId, "Departments", "DEPARTMENTCollectionView"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDiscipline))
                bluePrintsEntitiesModuleDescriptions.Add(new BluePrintsEntitiesModuleDescription("View_Disciplines", dataCategoryId, "Disciplines", "DISCIPLINECollectionView"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDocType))
                bluePrintsEntitiesModuleDescriptions.Add(new BluePrintsEntitiesModuleDescription("View_DocTypes", dataCategoryId, "Document Types", "DOCTYPECollectionView"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageUser))
                bluePrintsEntitiesModuleDescriptions.Add(new BluePrintsEntitiesModuleDescription("View_Users", dataCategoryId, "Users", "USERCollectionView"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageRole))
                bluePrintsEntitiesModuleDescriptions.Add(new BluePrintsEntitiesModuleDescription("View_Roles", dataCategoryId, "Roles", "ROLECollectionView"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDeliverableStatuses))
                bluePrintsEntitiesModuleDescriptions.Add(new BluePrintsEntitiesModuleDescription("View_DeliverableStatuses", dataCategoryId, "Deliverable Statuses", "DELIVERABLES_STATUSCollectionView"));

            bluePrintsEntitiesModuleDescriptions.Add(new BluePrintsEntitiesModuleDescription("View_UOMs", dataCategoryId, "Unit of Measures", "UOMCollectionView"));

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

            if (entity.STATUS == ProjectStatus.Active)
                parentId = activeCategoryId;
            else
                parentId = tenderCategoryId;

            newModules.Add(new BluePrintsEntitiesModuleDescription(projectKey, parentId, projectTitle, "PROJECTView", new EntitiesParameter<PROJECT>(entity), null, null, false));

            if (LoginCredentials.hasPermission(PermissionResources.ManageAreaAndPhases))
                newModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectAreas" + keyString, projectKey, childTitlePrefix + "Areas", "AREACollectionView", new EntitiesParameter<PROJECT>(entity), null, "Areas"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageRate))
                newModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectRates" + keyString, projectKey, childTitlePrefix + "Rates", "RATECollectionView", new EntitiesParameter<PROJECT>(entity), null, "Rates"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageWorkpack))
                newModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectWorkpacks" + keyString, projectKey, childTitlePrefix + "Workpacks", "WORKPACKCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Workpacks"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageBaseline))
                newModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectDeliverables" + keyString, projectKey, childTitlePrefix + "Deliverables", "BASELINE_ITEMCollectionView", new OptionalEntitiesParameter<PROJECT, BASELINE>(entity, null), null, "Deliverables"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageVariation))
                newModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectVariations" + keyString, projectKey, childTitlePrefix + "Variations", "VARIATIONCollectionView", new EntitiesParameter<PROJECT>(entity), null, "Variations"));

            if (LoginCredentials.hasPermission(PermissionResources.ManageProgress))
                newModules.Add(new BluePrintsEntitiesModuleDescription("View_ProjectProgresses" + keyString, projectKey, childTitlePrefix + "Progresses", "PROGRESS_ITEMCollectionView", new OptionalEntitiesParameter<PROJECT, PROGRESS>(entity, null), null, "Progresses"));

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
            if (!CanNavigate)
                Image = new BitmapImage(
                    new Uri(
                        "pack://application:,,,/DevExpress.Images.v16.2;component/Images/Data/ManageDataSource_16x16.png"));
            else
                Image = new BitmapImage(
                    new Uri("pack://application:,,,/DevExpress.Images.v16.2;component/Images/Actions/Open_16x16.png"));
        }
    }
}