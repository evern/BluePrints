using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.LayoutControl;

namespace BluePrints.ViewModels
{
    /// <summary>
    ///     Represents the root POCO view model for the BluePrintsEntities data model.
    /// </summary>
    public class BluePrintsEntitiesViewModel :
        DocumentsViewModel<BluePrintsEntitiesModuleDescription, IBluePrintsEntitiesUnitOfWork>
    {
        private const string ViewLayoutName = "BluePrintsEntitiesViewModel";

        private bool _isLoaded;

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
            Modules = new RangeObservableCollection<BluePrintsEntitiesModuleDescription>();
        }

        private INavigationService NavigationService
        {
            get { return this.GetService<INavigationService>(); }
        }

        public virtual BluePrintsEntitiesModuleDescription SelectedItem { get; set; }

        /// <summary>
        ///     Creates a new instance of BluePrintsEntitiesViewModel as a POCO view model.
        /// </summary>
        public static BluePrintsEntitiesViewModel Create()
        {
            return ViewModelSource.Create(() => new BluePrintsEntitiesViewModel());
        }

        public override void OnLoaded(BluePrintsEntitiesModuleDescription module)
        {
            PersistentLayoutHelper.TryDeserializeLayout(LayoutSerializationService, ViewLayoutName);
            IsLoaded = true;
            string themeName;
            if (LayoutSettings.Default.ThemeName == string.Empty)
                themeName = "Office2016Colorful";
            else
                themeName = LayoutSettings.Default.ThemeName;

            ApplicationThemeHelper.ApplicationThemeName = themeName;
        }

        public void Unloaded()
        {
            if (LayoutSerializationService != null)
            {
                PersistentLayoutHelper.PersistentViewsLayout[ViewLayoutName] = LayoutSerializationService.Serialize();
                PersistentLayoutHelper.SaveLayout();
            }

            _projectCollectionViewModel.OnDestroy();
        }

        private void OnEntitiesLoadedCallBack(IEnumerable<PROJECT> entities)
        {
            if (_isLoaded)
                return;

            MainThreadDispatcher.BeginInvoke(new Action(() => PopulateAllModules(entities)));
            MainThreadDispatcher.BeginInvoke(new Action(() => ShowDashboardModule()));
            _isLoaded = true;
        }

        private void PopulateAllModules(IEnumerable<PROJECT> entities)
        {
            var navigationItem = new List<BluePrintsEntitiesModuleDescription>(CreateModules());
            navigationItem.AddRange(PopulateProjectModules(entities));
            Modules.AddRange(navigationItem);
        }

        private void ShowDashboardModule()
        {
            if (LoginCredentials.hasPermission(PermissionResources.ViewDashboard))
            {
                var dashboard = Modules.FirstOrDefault(x => x.DocumentType == "PROJECTDashboardView");
                if (dashboard != null)
                    Show(dashboard);
            }
        }

        private IEnumerable<BluePrintsEntitiesModuleDescription> PopulateProjectModules(IEnumerable<PROJECT> entities)
        {
            var projects =
                entities.Where(x => x.STATUS == ProjectStatus.Active || x.STATUS == ProjectStatus.Tender)
                    .OrderBy(x => x.NUMBER)
                    .ToArray()
                    .AsEnumerable();
            if (projects.Any())
            {
                foreach (var project in projects)
                {
                    var newModules = CreateProjectTree(project);
                    foreach (var module in newModules)
                        yield return module;
                } 
            }
        }

        private void OnAfterEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (_projectCollectionViewModel == null)
                return;

            var primaryKey = (Guid) key;

            if (messageType == EntityMessageType.Added || messageType == EntityMessageType.Changed)
            {
                var project = _projectCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == primaryKey);
                if (project != null)
                    if (messageType == EntityMessageType.Added)
                    {
                        if (
                            !Modules.Any(
                                x =>
                                    x.TreeViewId.ToString() == "PROJECTView" + primaryKey.ToString() ||
                                    x.TreeViewParentId.ToString() == "PROJECTView" + primaryKey.ToString()))
                            Modules.InsertRangeBackground(CreateProjectTree(project));
                    }
                    else if (messageType == EntityMessageType.Changed)
                    {
                        if (project.STATUS == ProjectStatus.Closed || project.STATUS == ProjectStatus.OnHold)
                        {
                            Modules.RemoveRangeBackground(
                                Modules.Where(
                                        x =>
                                            x.TreeViewId.ToString() == "PROJECTView" + primaryKey.ToString() ||
                                            x.TreeViewParentId.ToString() == "PROJECTView" + primaryKey.ToString())
                                    .ToArray());
                        }
                        else
                        {
                            Modules.RemoveRangeBackground(
                                Modules.Where(
                                        x =>
                                            x.TreeViewId.ToString() == "PROJECTView" + primaryKey.ToString() ||
                                            x.TreeViewParentId.ToString() == "PROJECTView" + primaryKey.ToString())
                                    .ToArray());
                            Modules.InsertRangeBackground(CreateProjectTree(project));
                        }
                    }
                    else
                    {
                        Modules.RemoveRangeBackground(
                            Modules.Where(
                                x =>
                                    x.TreeViewId.ToString() == "PROJECTView" + primaryKey.ToString() ||
                                    x.TreeViewParentId.ToString() == "PROJECTView" + primaryKey.ToString()).ToArray());
                    }
            }
            else
            {
                Modules.RemoveRangeBackground(
                    Modules.Where(
                        x =>
                            x.TreeViewId.ToString() == "PROJECTView" + primaryKey.ToString() ||
                            x.TreeViewParentId.ToString() == "PROJECTView" + primaryKey.ToString()).ToArray());
            }
        }

        protected override BluePrintsEntitiesModuleDescription[] CreateModules()
        {
            var bluePrintsEntitiesModuleDescriptions = new List<BluePrintsEntitiesModuleDescription>();

            if (LoginCredentials.hasPermission(PermissionResources.ViewDashboard))
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Dashboard",
                    "PROJECTDashboardView", null, ModuleTreeProperty.ProjectdashboardCollectionModuleTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageProject))
            {
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Projects",
                    "PROJECTCollectionView", null, ModuleTreeProperty.ProjectCollectionModuleTreeProperty));
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Active",
                    "ACTIVEPROJECTCategoryView", null, ModuleTreeProperty.ActiveprojectCategoryTreeProperty));
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Tender",
                    "TENDERPROJECTCategoryView", null, ModuleTreeProperty.TenderprojectCategoryTreeProperty));
            }

            bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Data",
                "DATACategoryView", null, ModuleTreeProperty.DataCategoryTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageCommodity))
            {
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Commodity Code",
                    "COMMODITY_CODECategoryView", null, ModuleTreeProperty.CommodityCodeCategoryTreeProperty));
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Indirect Type",
                    "INDIRECT_TYPECollectionView", null, ModuleTreeProperty.IndirectTypeModuleTreeProperty));
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Direct",
                    "COMMODITY_CODECollectionView",
                    new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null,
                        new CommodityCodeTypeClass(CommodityCodeType.Direct)),
                    ModuleTreeProperty.DirectcommodityCodeModuleTreeProperty));
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Indirect",
                    "COMMODITY_CODECollectionView",
                    new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null,
                        new CommodityCodeTypeClass(CommodityCodeType.Indirect)),
                    ModuleTreeProperty.IndirectcommodityCodeModuleTreeProperty));
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Design",
                    "COMMODITY_CODECollectionView",
                    new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null,
                        new CommodityCodeTypeClass(CommodityCodeType.Design)),
                    ModuleTreeProperty.DesigncommodityCodeModuleTreeProperty));

                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Commodity Group",
                    "COMMODITY_GROUPCategoryView", null, ModuleTreeProperty.CommodityGroupCategoryTreeProperty));
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Direct",
                    "COMMODITY_GROUP_DIRECTCollectionView", null,
                    ModuleTreeProperty.CommodityGroupDirectModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageDepartment))
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Departments",
                    "DEPARTMENTCollectionView", null, ModuleTreeProperty.DepartmentCollectionModuleTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDeliverableStatuses))
                bluePrintsEntitiesModuleDescriptions.Add(
                    BluePrintsEntitiesModuleDescription.Create("Deliverable Statuses",
                        "DELIVERABLES_STATUSCollectionView", null,
                        ModuleTreeProperty.DeliverablesStatusCollectionModuleTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDiscipline))
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Disciplines",
                    "DISCIPLINECollectionView", null, ModuleTreeProperty.DisciplineCollectionModuleTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDocType))
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Doctypes",
                    "DOCTYPECollectionView", null, ModuleTreeProperty.DoctypeCollectionModuleTreeProperty));

            bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("UOM",
                "UOMCollectionView", null, ModuleTreeProperty.UomCollectionModuleTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageUser))
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("User",
                    "USERCollectionView", null, ModuleTreeProperty.UserCollectionModuleTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageRole))
                bluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Role",
                    "ROLECollectionView", null, ModuleTreeProperty.RoleCollectionModuleTreeProperty));

            return bluePrintsEntitiesModuleDescriptions.ToArray();
        }

        private IEnumerable<BluePrintsEntitiesModuleDescription> CreateProjectTree(PROJECT entity)
        {
            var newModules = new List<BluePrintsEntitiesModuleDescription>();
            var projectModuleTreeViewProperty = ModuleTreeProperty.ProjectTreeProperty(entity);

            var moduleTitle = entity.NUMBER + " " + entity.NAME;
            object parentId;
            if (entity.STATUS == ProjectStatus.Active)
                parentId = ModuleTreeProperty.ActiveprojectCategoryTreeProperty.Id;
            else
                parentId = ModuleTreeProperty.TenderprojectCategoryTreeProperty.Id;

            projectModuleTreeViewProperty.ParentId = parentId;

            //moduleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create(entity.NUMBER, "PROJECTView", null, entity.GUID, PROJECTModuleTreeProperty));
            newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "PROJECTView",
                new EntitiesParameter<PROJECT>(entity), projectModuleTreeViewProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageAreaAndPhases))
            {
                var projectphaseModuleTreeProperty = new TreeViewProperty
                {
                    Id = "PHASECollectionView" + entity.NUMBER,
                    ParentId = projectModuleTreeViewProperty.Id,
                    Image = ModuleTreeProperty.TreeViewImage
                };
                moduleTitle = "Phases";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "PHASECollectionView",
                    new EntitiesParameter<PROJECT>(entity), projectphaseModuleTreeProperty));

                var projectareaModuleTreeProperty = new TreeViewProperty
                {
                    Id = "AREACollectionView" + entity.NUMBER,
                    ParentId = projectModuleTreeViewProperty.Id,
                    Image = ModuleTreeProperty.TreeViewImage
                };
                moduleTitle = "Areas";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "AREACollectionView",
                    new EntitiesParameter<PROJECT>(entity), projectareaModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageRate))
            {
                var projectrateModuleTreeProperty = new TreeViewProperty
                {
                    Id = "RATECollectionView" + entity.NUMBER,
                    ParentId = projectModuleTreeViewProperty.Id,
                    Image = ModuleTreeProperty.TreeViewImage
                };
                moduleTitle = "Rates";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "RATECollectionView",
                    new EntitiesParameter<PROJECT>(entity), projectrateModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageWorkpack))
            {
                var projectworkpackModuleTreeProperty = new TreeViewProperty
                {
                    Id = "WORKPACKCollectionView" + entity.NUMBER,
                    ParentId = projectModuleTreeViewProperty.Id,
                    Image = ModuleTreeProperty.TreeViewImage
                };
                moduleTitle = "Workpacks";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "WORKPACKCollectionView",
                    new EntitiesParameter<PROJECT>(entity), projectworkpackModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageBaseline))
            {
                var projectlivebaselineModuleTreeProperty = new TreeViewProperty
                {
                    Id = "LiveBASELINEView" + entity.NUMBER,
                    ParentId = projectModuleTreeViewProperty.Id,
                    Image = ModuleTreeProperty.TreeViewImage
                };
                moduleTitle = "Deliverables";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "BASELINE_ITEMCollectionView",
                    new OptionalEntitiesParameter<PROJECT, BASELINE>(entity, null),
                    projectlivebaselineModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageVariation))
            {
                var projectvariationModuleTreeProperty = new TreeViewProperty
                {
                    Id = "VARIATIONCollectionView" + entity.NUMBER,
                    ParentId = projectModuleTreeViewProperty.Id,
                    Image = ModuleTreeProperty.TreeViewImage
                };
                moduleTitle = "Variations";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "VARIATIONCollectionView",
                    new EntitiesParameter<PROJECT>(entity), projectvariationModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageProgress))
            {
                var projectliveprogressModuleTreeProperty = new TreeViewProperty
                {
                    Id = "LivePROGRESSView" + entity.NUMBER,
                    ParentId = projectModuleTreeViewProperty.Id,
                    Image = ModuleTreeProperty.TreeViewImage
                };
                moduleTitle = "Progress";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "PROGRESS_ITEMCollectionView",
                    new OptionalEntitiesParameter<PROJECT, PROGRESS>(entity, null),
                    projectliveprogressModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageCommodity))
            {
                var projectcommodityCodeModuleTreeProperty = new TreeViewProperty
                {
                    Id = "MasterDetailCOMMODITY_CODEView" + entity.NUMBER,
                    ParentId = projectModuleTreeViewProperty.Id,
                    Image = ModuleTreeProperty.TreeViewImage
                };
                moduleTitle = "Commodity Code";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle,
                    "COMMODITY_CODEMasterDetailCollectionView",
                    new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(entity,
                        new CommodityCodeTypeClass(CommodityCodeType.Direct)), projectcommodityCodeModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageEstimation))
            {
                var projectliveestimationModuleTreeProperty = new TreeViewProperty
                {
                    Id = "LiveESTIMATIONDIRECTView" + entity.NUMBER,
                    ParentId = projectModuleTreeViewProperty.Id,
                    Image = ModuleTreeProperty.TreeViewImage
                };
                moduleTitle = "Estimation";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle,
                    "ESTIMATION_DIRECT_ITEMCollectionView",
                    new OptionalEntitiesParameter<PROJECT, ESTIMATION_DIRECT>(entity, null),
                    projectliveestimationModuleTreeProperty));
            }

            return newModules;
        }

        protected override void OnSelectedModuleChanged(BluePrintsEntitiesModuleDescription oldModule)
        {
            //Not to execute base.OnSelectedModuleChanged because navigation is invoked on double click instead
        }

        public static class ModuleTreeProperty
        {
            public static ImageSource TreeViewImage =
                new BitmapImage(
                    new Uri("pack://application:,,,/DevExpress.Images.v16.2;component/Images/Actions/Open_16x16.png"));

            public static ImageSource CategoryViewImage =
                new BitmapImage(
                    new Uri(
                        "pack://application:,,,/DevExpress.Images.v16.2;component/Images/Data/ManageDataSource_16x16.png"));

            public static TreeViewProperty ProjectdashboardCollectionModuleTreeProperty
            {
                get { return new TreeViewProperty {Id = "PROJECTDashboardView", ParentId = 0, Image = TreeViewImage}; }
            }

            public static TreeViewProperty ProjectCollectionModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "PROJECTCollectionView",
                        ParentId = 0,
                        Image = TreeViewImage,
                        IsExpanded = true
                    };
                }
            }

            public static TreeViewProperty DepartmentCollectionModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "DEPARTMENTCollectionView",
                        ParentId = DataCategoryTreeProperty.Id,
                        Image = TreeViewImage
                    };
                }
            }

            public static TreeViewProperty DeliverablesStatusCollectionModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "DELIVERABLES_STATUSCollectionView",
                        ParentId = DataCategoryTreeProperty.Id,
                        Image = TreeViewImage
                    };
                }
            }

            public static TreeViewProperty DisciplineCollectionModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "DISCIPLINECollectionView",
                        ParentId = DataCategoryTreeProperty.Id,
                        Image = TreeViewImage
                    };
                }
            }

            public static TreeViewProperty DoctypeCollectionModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "DOCTYPECollectionView",
                        ParentId = DataCategoryTreeProperty.Id,
                        Image = TreeViewImage
                    };
                }
            }

            public static TreeViewProperty UomCollectionModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "UOMCollectionView",
                        ParentId = DataCategoryTreeProperty.Id,
                        Image = TreeViewImage
                    };
                }
            }

            public static TreeViewProperty UserCollectionModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "USERCollectionView",
                        ParentId = DataCategoryTreeProperty.Id,
                        Image = TreeViewImage
                    };
                }
            }

            public static TreeViewProperty RoleCollectionModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "ROLECollectionView",
                        ParentId = DataCategoryTreeProperty.Id,
                        Image = TreeViewImage
                    };
                }
            }

            public static TreeViewProperty IndirectTypeModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "INDIRECT_TYPECollectionView",
                        ParentId = CommodityCodeCategoryTreeProperty.Id,
                        Image = TreeViewImage
                    };
                }
            }

            public static TreeViewProperty DirectcommodityCodeModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "DIRECTCOMMODITY_CODECollectionView",
                        ParentId = CommodityCodeCategoryTreeProperty.Id,
                        Image = TreeViewImage
                    };
                }
            }

            public static TreeViewProperty IndirectcommodityCodeModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "INDIRECTCOMMODITY_CODECollectionView",
                        ParentId = CommodityCodeCategoryTreeProperty.Id,
                        Image = TreeViewImage
                    };
                }
            }

            public static TreeViewProperty DesigncommodityCodeModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "DESIGNCOMMODITY_CODECollectionView",
                        ParentId = CommodityCodeCategoryTreeProperty.Id,
                        Image = TreeViewImage
                    };
                }
            }

            public static TreeViewProperty CommodityGroupDirectModuleTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "COMMODITY_GROUP_DIRECTCollectionView",
                        ParentId = CommodityGroupCategoryTreeProperty.Id,
                        Image = TreeViewImage
                    };
                }
            }


            //Category View TreeViewProperty
            public static TreeViewProperty DataCategoryTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "DATACategoryView",
                        ParentId = 0,
                        Image = CategoryViewImage,
                        IsExpanded = false
                    };
                }
            }

            public static TreeViewProperty CommodityCodeCategoryTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "COMMODITY_CODECategoryView",
                        ParentId = DataCategoryTreeProperty.Id,
                        Image = CategoryViewImage,
                        IsExpanded = true
                    };
                }
            }

            public static TreeViewProperty CommodityGroupCategoryTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "COMMODITY_GROUPCategoryView",
                        ParentId = DataCategoryTreeProperty.Id,
                        Image = CategoryViewImage,
                        IsExpanded = true
                    };
                }
            }

            public static TreeViewProperty ActiveprojectCategoryTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "ACTIVEPROJECTCategoryView",
                        ParentId = ProjectCollectionModuleTreeProperty.Id,
                        Image = CategoryViewImage,
                        IsExpanded = true
                    };
                }
            }

            public static TreeViewProperty TenderprojectCategoryTreeProperty
            {
                get
                {
                    return new TreeViewProperty
                    {
                        Id = "TENDERPROJECTCategoryView",
                        ParentId = ProjectCollectionModuleTreeProperty.Id,
                        Image = CategoryViewImage,
                        IsExpanded = true
                    };
                }
            }

            //Project Specific TreeViewProperty
            public static TreeViewProperty ProjectTreeProperty(PROJECT entity)
            {
                return new TreeViewProperty
                {
                    Id = "PROJECTView" + entity.GUID,
                    ParentId = ProjectCollectionModuleTreeProperty.Id,
                    IsExpanded = false,
                    Image = TreeViewImage
                };
            }
        }
    }

    public class BluePrintsEntitiesModuleDescription : ModuleDescription<BluePrintsEntitiesModuleDescription>
    {
        protected BluePrintsEntitiesModuleDescription(string title, string documentType, object documentParameter = null,
            TreeViewProperty treeViewProperty = null)
            : base(title, documentType, documentParameter, treeViewProperty)
        {
        }

        public virtual bool IsSelected { get; set; }

        public string DisplayTitle
        {
            get
            {
                if (ModuleTitle.Length > 8)
                    return ModuleTitle.Substring(8, ModuleTitle.Length - 8);
                return ModuleTitle;
            }
        }

        public BluePrintsEntitiesModuleDescription Clone()
        {
            return Create(ModuleTitle, DocumentType, DocumentParameter, TreeViewProperty);
        }

        public static BluePrintsEntitiesModuleDescription Create(string title, string documentType,
            object documentParameter = null, TreeViewProperty treeViewProperty = null)
        {
            return
                ViewModelSource.Create(
                    () =>
                        new BluePrintsEntitiesModuleDescription(title, documentType, documentParameter, treeViewProperty));
        }
    }

    public sealed class NavigationTreeViewModel
    {
        private readonly DispatcherTimer _defaultSelectionTimer;

        private NavigationTreeViewModel(BluePrintsEntitiesModuleDescription[] modules, string selectedTitle,
            BluePrintsEntitiesViewModel owner)
        {
            Owner = owner;
            Modules = modules;
            DefaultItem = Modules.FirstOrDefault(x => x.ModuleTitle == selectedTitle);
            if (DefaultItem != null)
            {
                DefaultItem.IsSelected = true;
                SelectedItem = DefaultItem;
            }

            if (selectedTitle.Length > 7)
                StaticCategoryName = selectedTitle.Substring(0, 7);
            else
                StaticCategoryName = "[" + selectedTitle + "]";

            _defaultSelectionTimer = new DispatcherTimer();
            _defaultSelectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            _defaultSelectionTimer.Tick += defaultSelectionTimer_Tick;
        }

        private BluePrintsEntitiesViewModel Owner { get; set; }
        public BluePrintsEntitiesModuleDescription[] Modules { get; set; }
        private BluePrintsEntitiesModuleDescription LastSelectedItem { get; set; }
        public BluePrintsEntitiesModuleDescription SelectedItem { get; set; }
        public BluePrintsEntitiesModuleDescription DefaultItem { get; set; }

        public string StaticCategoryName { get; set; }

        public static NavigationTreeViewModel Create(BluePrintsEntitiesModuleDescription[] modules, string selectedTitle,
            BluePrintsEntitiesViewModel owner)
        {
            return ViewModelSource.Create(() => new NavigationTreeViewModel(modules, selectedTitle, owner));
        }

        public void ResetToAll()
        {
            SelectedItem = Modules[0];
        }

        private void OnSelectedItemChanged()
        {
            if (SelectedItem == null)
                SelectedItem = DefaultItem;

            if (SelectedItem != DefaultItem)
            {
                Owner.NavigateCore(SelectedItem);
                _defaultSelectionTimer.Start();
            }
            else
            {
                this.RaisePropertyChanged(x => x.SelectedItem);
            }
        }

        //need to be invoked externally because during selecteditem time the routine is still within OnTreeViewSelectedItemChanged in TreeViewSelectedItemBehavior
        private void defaultSelectionTimer_Tick(object sender, EventArgs e)
        {
            _defaultSelectionTimer.Stop();
            SelectedItem = DefaultItem;
        }
    }
}

namespace BluePrints.Common.ViewModel
{
    public class TileProperty
    {
        public TileProperty()
        {
            TileLayoutFlowBreak = false;
            TileLayoutSize = TileSize.Small;
        }

        /// <summary>
        ///     Specify whether the tile should break in tileLayout
        /// </summary>
        public bool TileLayoutFlowBreak { get; set; }

        /// <summary>
        ///     Specify whether the size of the tile in tileLayout
        /// </summary>
        public TileSize TileLayoutSize { get; set; }
    }

    public class TreeViewProperty
    {
        public TreeViewProperty()
        {
            Id = 0;
            ParentId = 0;
        }

        public object Id { get; set; }
        public object ParentId { get; set; }
        public ImageSource Image { get; set; }
        public bool IsExpanded { get; set; }
    }


    public abstract partial class ModuleDescription<TModule> where TModule : ModuleDescription<TModule>
    {
        /// <summary>
        ///     Initializes a new instance of the ModuleDescription class.
        /// </summary>
        /// <param name="title">A navigation list entry display text.</param>
        /// <param name="documentType">A string value that specifies the view type of corresponding document.</param>
        /// <param name="treeViewProperty">A property containing tree view specific properties for view binding</param>
        /// <param name="documentParameter">A document parameter to specify SingleObjectView to display</param>
        protected ModuleDescription(string title, string documentType, object documentParameter = null, TreeViewProperty treeViewProperty = null)
        {
            ModuleTitle = title;
            DocumentType = documentType;
            DocumentId = documentParameter == null
                ? documentType
                : documentType + documentParameter.ToString().Replace('-', '_');
            TreeViewProperty = treeViewProperty;
            DocumentParameter = documentParameter;
            if (treeViewProperty != null)
            {
                TreeViewParentId = treeViewProperty.ParentId;
                TreeViewId = treeViewProperty.Id;
                TreeViewImage = treeViewProperty.Image;
                TreeViewIsExpanded = treeViewProperty.IsExpanded;
            }
        }

        /// <summary>
        ///     The navigation parameter for SingleObjectViewModel.
        /// </summary>
        public object DocumentParameter { get; private set; }

        /// <summary>
        ///     Specifies the SingleObjectView document id
        /// </summary>
        public object DocumentId { get; private set; }

        /// <summary>
        ///     Specifies the parentId for treeview binding, cannot be nested since dxTreeView doesn't support nested for Ids
        /// </summary>
        public object TreeViewParentId { get; private set; }

        /// <summary>
        ///     Specifies the Id for treeview binding, cannot be nested since dxTreeView doesn't support nested for Ids
        /// </summary>
        public object TreeViewId { get; private set; }

        /// <summary>
        ///     Specify the treeview property when binded to TreeViewControl
        /// </summary>
        public TreeViewProperty TreeViewProperty { get; private set; }

        /// <summary>
        ///     Specify the treeview image property when binded to TreeViewControl
        /// </summary>
        public ImageSource TreeViewImage { get; set; }

        /// <summary>
        ///     Describe whether the treelist item is expanded
        /// </summary>
        public bool TreeViewIsExpanded { get; set; }
    }
}