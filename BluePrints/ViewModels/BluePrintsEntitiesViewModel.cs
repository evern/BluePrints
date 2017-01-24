using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Data;
using DevExpress.Xpf.LayoutControl;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BluePrints.Common.ViewModel.Filtering;
using System.Windows.Threading;
using BluePrints.Data.Helpers;
using BluePrints.Common.Projections;
using BluePrints.Common;
using System.Threading.Tasks;
using System.Threading;
using DevExpress.Xpf.Core;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the root POCO view model for the BluePrintsEntities data model.
    /// </summary>
    public partial class BluePrintsEntitiesViewModel : DocumentsViewModel<BluePrintsEntitiesModuleDescription, IBluePrintsEntitiesUnitOfWork>
    {

        const string TablesGroup = "Tables";

        const string ViewsGroup = "Views";

        DispatcherTimer delayedProjectPopulationDispatcher;

        INavigationService NavigationService { get { return this.GetService<INavigationService>(); } }
        /// <summary>
        /// Creates a new instance of BluePrintsEntitiesViewModel as a POCO view model.
        /// </summary>
        public static BluePrintsEntitiesViewModel Create()
        {
            return ViewModelSource.Create(() => new BluePrintsEntitiesViewModel());
        }

        /// <summary>
        /// Initializes a new instance of the BluePrintsEntitiesViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BluePrintsEntitiesViewModel type without the POCO proxy factory.
        /// </summary>
        protected BluePrintsEntitiesViewModel()
            : base(BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory())
        {
            delayedProjectPopulationDispatcher = new DispatcherTimer();
            delayedProjectPopulationDispatcher.Interval = new TimeSpan(0, 0, 1);
            delayedProjectPopulationDispatcher.Tick += delayedProjectPopulationDispatcher_Tick;
            Messenger.Default.Register<EntityMessage<PROJECT, Guid>>(this, x => OnMessage(x));
        }

        public override void OnLoaded(BluePrintsEntitiesModuleDescription module)
        {
            IsLoaded = true;
            string themeName;
            if (LayoutSettings.Default.ThemeName == string.Empty)
                themeName = "Office2016Colorful";
            else
                themeName = LayoutSettings.Default.ThemeName;

            ApplicationThemeHelper.ApplicationThemeName = themeName;

            if (LoginCredentials.hasPermission(PermissionResources.ViewDashboard))
            {
                var dashboard = Modules.FirstOrDefault(x => x.DocumentType == "PROJECTDashboardView");
                if (dashboard != null)
                    Show(dashboard);
            }

            delayedProjectPopulationDispatcher.Start();
        }

        void delayedProjectPopulationDispatcher_Tick(object sender, EventArgs e)
        {
            delayedProjectPopulationDispatcher.Stop();
            //PopulateProjectModules();
        }

        private IEnumerable<BluePrintsEntitiesModuleDescription> PopulateProjectModules()
        {
            var Projects = this.CreateUnitOfWork().PROJECTS.Where(x => x.STATUS == ProjectStatus.Active || x.STATUS == ProjectStatus.Tender).OrderBy(x => x.NUMBER).ToArray().AsEnumerable();
            if (Projects.Count() > 0)
                foreach (var Project in Projects)
                {
                   IEnumerable<BluePrintsEntitiesModuleDescription> newModules = createPROJECTTree(Project);
                   foreach(var module in newModules)
                       yield return module;
                }
        }

        void OnMessage(EntityMessage<PROJECT, Guid> message)
        {
            if(message.MessageType == EntityMessageType.Added || message.MessageType == EntityMessageType.Changed)
            {
                var Project = this.CreateUnitOfWork().PROJECTS.FirstOrDefault(x => x.GUID == message.PrimaryKey);
                if(Project != null)
                {
                    if(Project.STATUS == ProjectStatus.Active)
                    {
                        if (!Modules.Any(x => x.TreeViewId.ToString() == "PROJECTView" + message.PrimaryKey.ToString() || x.TreeViewParentId.ToString() == "PROJECTView" + message.PrimaryKey.ToString()))
                            Modules.InsertRangeBackground(createPROJECTTree(Project));
                    }
                    else
                    {
                        Modules.RemoveRangeBackground(Modules.Where(x => x.TreeViewId.ToString() == "PROJECTView" + message.PrimaryKey.ToString() || x.TreeViewParentId.ToString() == "PROJECTView" + message.PrimaryKey.ToString()).ToArray());
                    }
                }
            }
            else
            {
                Modules.RemoveRangeBackground(Modules.Where(x => x.TreeViewId.ToString() == "PROJECTView" + message.PrimaryKey.ToString() || x.TreeViewParentId.ToString() == "PROJECTView" + message.PrimaryKey.ToString()).ToArray());
            }
        }

        public static class ModuleTreeProperty
        {
            public static ImageSource TreeViewImage = new BitmapImage(new Uri("pack://application:,,,/DevExpress.Images.v16.2;component/Images/Actions/Open_16x16.png"));
            public static ImageSource CategoryViewImage = new BitmapImage(new Uri("pack://application:,,,/DevExpress.Images.v16.2;component/Images/Data/ManageDataSource_16x16.png"));
            
            public static TreeViewProperty PROJECTDASHBOARDCollectionModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "PROJECTDashboardView", ParentId = 0, Image = TreeViewImage }; }
            }

            public static TreeViewProperty PROJECTCollectionModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "PROJECTCollectionView", ParentId = 0, Image = TreeViewImage, IsExpanded = true }; }
            }

            public static TreeViewProperty DEPARTMENTCollectionModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "DEPARTMENTCollectionView", ParentId = DATACategoryTreeProperty.Id, Image = TreeViewImage }; }   
            }

            public static TreeViewProperty DELIVERABLES_STATUSCollectionModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "DELIVERABLES_STATUSCollectionView", ParentId = DATACategoryTreeProperty.Id, Image = TreeViewImage }; }
            }

            public static TreeViewProperty DISCIPLINECollectionModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "DISCIPLINECollectionView", ParentId = DATACategoryTreeProperty.Id, Image = TreeViewImage }; }
            }

            public static TreeViewProperty DOCTYPECollectionModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "DOCTYPECollectionView", ParentId = DATACategoryTreeProperty.Id, Image = TreeViewImage }; }
            }

            public static TreeViewProperty UOMCollectionModuleTreeProperty 
            {
                get { return new TreeViewProperty() { Id = "UOMCollectionView", ParentId = DATACategoryTreeProperty.Id, Image = TreeViewImage }; }
            }

            public static TreeViewProperty USERCollectionModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "USERCollectionView", ParentId = DATACategoryTreeProperty.Id, Image = TreeViewImage }; }
            }

            public static TreeViewProperty ROLECollectionModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "ROLECollectionView", ParentId = DATACategoryTreeProperty.Id, Image = TreeViewImage }; }
            }

            public static TreeViewProperty INDIRECT_TYPEModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "INDIRECT_TYPECollectionView", ParentId = COMMODITY_CODECategoryTreeProperty.Id, Image = TreeViewImage }; }
            }

            public static TreeViewProperty DIRECTCOMMODITY_CODEModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "DIRECTCOMMODITY_CODECollectionView", ParentId = COMMODITY_CODECategoryTreeProperty.Id, Image = TreeViewImage }; }
            }

            public static TreeViewProperty INDIRECTCOMMODITY_CODEModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "INDIRECTCOMMODITY_CODECollectionView", ParentId = COMMODITY_CODECategoryTreeProperty.Id, Image = TreeViewImage }; }
            }

            public static TreeViewProperty DESIGNCOMMODITY_CODEModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "DESIGNCOMMODITY_CODECollectionView", ParentId = COMMODITY_CODECategoryTreeProperty.Id, Image = TreeViewImage }; }
            }

            public static TreeViewProperty COMMODITY_GROUP_DIRECTModuleTreeProperty
            {
                get { return new TreeViewProperty() { Id = "COMMODITY_GROUP_DIRECTCollectionView", ParentId = COMMODITY_GROUPCategoryTreeProperty.Id, Image = TreeViewImage }; }
            }


            //Category View TreeViewProperty
            public static TreeViewProperty DATACategoryTreeProperty
            {
                get { return new TreeViewProperty() { Id = "DATACategoryView", ParentId = 0, Image = CategoryViewImage, IsExpanded = false }; }
            }

            public static TreeViewProperty COMMODITY_CODECategoryTreeProperty
            {
                get { return new TreeViewProperty() { Id = "COMMODITY_CODECategoryView", ParentId = DATACategoryTreeProperty.Id, Image = CategoryViewImage, IsExpanded = true }; }
            }

            public static TreeViewProperty COMMODITY_GROUPCategoryTreeProperty
            {
                get { return new TreeViewProperty() { Id = "COMMODITY_GROUPCategoryView", ParentId = DATACategoryTreeProperty.Id, Image = CategoryViewImage, IsExpanded = true }; }
            }

            public static TreeViewProperty ACTIVEPROJECTCategoryTreeProperty
            {
                get { return new TreeViewProperty() { Id = "ACTIVEPROJECTCategoryView", ParentId = PROJECTCollectionModuleTreeProperty.Id, Image = CategoryViewImage, IsExpanded = true }; }
            }

            public static TreeViewProperty TENDERPROJECTCategoryTreeProperty
            {
                get { return new TreeViewProperty() { Id = "TENDERPROJECTCategoryView", ParentId = PROJECTCollectionModuleTreeProperty.Id, Image = CategoryViewImage, IsExpanded = true }; }
            }
            
            //Project Specific TreeViewProperty
            public static TreeViewProperty PROJECTTreeProperty(PROJECT entity)
            {
                return new TreeViewProperty() { Id = "PROJECTView" + entity.GUID, ParentId = PROJECTCollectionModuleTreeProperty.Id, IsExpanded = false, Image = TreeViewImage };
            }
        }

        protected override BluePrintsEntitiesModuleDescription[] CreateModules()
        {

            List<BluePrintsEntitiesModuleDescription> BluePrintsEntitiesModuleDescriptions = new List<BluePrintsEntitiesModuleDescription>();

            if (LoginCredentials.hasPermission(PermissionResources.ViewDashboard))
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Dashboard", "PROJECTDashboardView", TablesGroup, null, null, ModuleTreeProperty.PROJECTDASHBOARDCollectionModuleTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageProject))
            {
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Projects", "PROJECTCollectionView", TablesGroup, null, null, ModuleTreeProperty.PROJECTCollectionModuleTreeProperty));
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Active", "ACTIVEPROJECTCategoryView", TablesGroup, null, null, ModuleTreeProperty.ACTIVEPROJECTCategoryTreeProperty));
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Tender", "TENDERPROJECTCategoryView", TablesGroup, null, null, ModuleTreeProperty.TENDERPROJECTCategoryTreeProperty));
            }
 
            BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Data", "DATACategoryView", TablesGroup, null, null, ModuleTreeProperty.DATACategoryTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageCommodity))
            {
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Commodity Code", "COMMODITY_CODECategoryView", TablesGroup, null, null, ModuleTreeProperty.COMMODITY_CODECategoryTreeProperty));
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Indirect Type", "INDIRECT_TYPECollectionView", TablesGroup, null, null, ModuleTreeProperty.INDIRECT_TYPEModuleTreeProperty));
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Direct", "COMMODITY_CODECollectionView", TablesGroup, null, new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null, new CommodityCodeTypeClass(CommodityCodeType.Direct)), ModuleTreeProperty.DIRECTCOMMODITY_CODEModuleTreeProperty));
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Indirect", "COMMODITY_CODECollectionView", TablesGroup, null, new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null, new CommodityCodeTypeClass(CommodityCodeType.Indirect)), ModuleTreeProperty.INDIRECTCOMMODITY_CODEModuleTreeProperty));
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Design", "COMMODITY_CODECollectionView", TablesGroup, null, new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(null, new CommodityCodeTypeClass(CommodityCodeType.Design)), ModuleTreeProperty.DESIGNCOMMODITY_CODEModuleTreeProperty));

                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Commodity Group", "COMMODITY_GROUPCategoryView", TablesGroup, null, null, ModuleTreeProperty.COMMODITY_GROUPCategoryTreeProperty));
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Direct", "COMMODITY_GROUP_DIRECTCollectionView", TablesGroup, null, null, ModuleTreeProperty.COMMODITY_GROUP_DIRECTModuleTreeProperty));
            }

            if(LoginCredentials.hasPermission(PermissionResources.ManageDepartment))
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Departments", "DEPARTMENTCollectionView", TablesGroup, null, null, ModuleTreeProperty.DEPARTMENTCollectionModuleTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDeliverableStatuses))
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Deliverable Statuses", "DELIVERABLES_STATUSCollectionView", TablesGroup, null, null, ModuleTreeProperty.DELIVERABLES_STATUSCollectionModuleTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDiscipline))
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Disciplines", "DISCIPLINECollectionView", TablesGroup, null, null, ModuleTreeProperty.DISCIPLINECollectionModuleTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageDocType))
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Doctypes", "DOCTYPECollectionView", TablesGroup, null, null, ModuleTreeProperty.DOCTYPECollectionModuleTreeProperty));

            BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("UOM", "UOMCollectionView", TablesGroup, null, null, ModuleTreeProperty.UOMCollectionModuleTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageUser))
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("User", "USERCollectionView", TablesGroup, null, null, ModuleTreeProperty.USERCollectionModuleTreeProperty));

            if (LoginCredentials.hasPermission(PermissionResources.ManageRole))
                BluePrintsEntitiesModuleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create("Role", "ROLECollectionView", TablesGroup, null, null, ModuleTreeProperty.ROLECollectionModuleTreeProperty));

            BluePrintsEntitiesModuleDescriptions.AddRange(PopulateProjectModules());
            return BluePrintsEntitiesModuleDescriptions.ToArray();
        }

        public virtual BluePrintsEntitiesModuleDescription SelectedItem { get; set; }
        private IEnumerable<BluePrintsEntitiesModuleDescription> createPROJECTTree(PROJECT entity, bool createTree = false)
        {
            List<BluePrintsEntitiesModuleDescription> newModules = new List<BluePrintsEntitiesModuleDescription>();
            TreeViewProperty PROJECTModuleTreeViewProperty = ModuleTreeProperty.PROJECTTreeProperty(entity);

            string moduleTitle = entity.NUMBER + " " + entity.NAME;
            object parentId;
            if (entity.STATUS == ProjectStatus.Active)
                parentId = ModuleTreeProperty.ACTIVEPROJECTCategoryTreeProperty.Id;
            else
                parentId = ModuleTreeProperty.TENDERPROJECTCategoryTreeProperty.Id;

            PROJECTModuleTreeViewProperty.ParentId = parentId;

            //moduleDescriptions.Add(BluePrintsEntitiesModuleDescription.Create(entity.NUMBER, "PROJECTView", TablesGroup, null, entity.GUID, PROJECTModuleTreeProperty));
            newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "PROJECTView", TablesGroup, null, new EntitiesParameter<PROJECT>(entity), PROJECTModuleTreeViewProperty));

            if(LoginCredentials.hasPermission(PermissionResources.ManageAreaAndPhases))
            {
                TreeViewProperty PROJECTPHASEModuleTreeProperty = new TreeViewProperty() { Id = "PHASECollectionView" + entity.NUMBER, ParentId = PROJECTModuleTreeViewProperty.Id, Image = ModuleTreeProperty.TreeViewImage };
                moduleTitle = "Phases";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "PHASECollectionView", TablesGroup, null, new EntitiesParameter<PROJECT>(entity), PROJECTPHASEModuleTreeProperty));

                TreeViewProperty PROJECTAREAModuleTreeProperty = new TreeViewProperty() { Id = "AREACollectionView" + entity.NUMBER, ParentId = PROJECTModuleTreeViewProperty.Id, Image = ModuleTreeProperty.TreeViewImage };
                moduleTitle = "Areas";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "AREACollectionView", TablesGroup, null, new EntitiesParameter<PROJECT>(entity), PROJECTAREAModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageRate))
            {
                TreeViewProperty PROJECTRATEModuleTreeProperty = new TreeViewProperty() { Id = "RATECollectionView" + entity.NUMBER, ParentId = PROJECTModuleTreeViewProperty.Id, Image = ModuleTreeProperty.TreeViewImage };
                moduleTitle = "Rates";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "RATECollectionView", TablesGroup, null, new EntitiesParameter<PROJECT>(entity), PROJECTRATEModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageWorkpack))
            {
                TreeViewProperty PROJECTWORKPACKModuleTreeProperty = new TreeViewProperty() { Id = "WORKPACKCollectionView" + entity.NUMBER, ParentId = PROJECTModuleTreeViewProperty.Id, Image = ModuleTreeProperty.TreeViewImage };
                moduleTitle = "Workpacks";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "WORKPACKCollectionView", TablesGroup, null, new EntitiesParameter<PROJECT>(entity), PROJECTWORKPACKModuleTreeProperty));
            }

            if(LoginCredentials.hasPermission(PermissionResources.ManageBaseline))
            {
                TreeViewProperty PROJECTLIVEBASELINEModuleTreeProperty = new TreeViewProperty() { Id = "LiveBASELINEView" + entity.NUMBER, ParentId = PROJECTModuleTreeViewProperty.Id, Image = ModuleTreeProperty.TreeViewImage };
                moduleTitle = "Deliverables";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "BASELINE_ITEMCollectionView", TablesGroup, null, new OptionalEntitiesParameter<PROJECT, BASELINE>(entity, null), PROJECTLIVEBASELINEModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageVariation))
            {
                TreeViewProperty PROJECTVARIATIONModuleTreeProperty = new TreeViewProperty() { Id = "VARIATIONCollectionView" + entity.NUMBER, ParentId = PROJECTModuleTreeViewProperty.Id, Image = ModuleTreeProperty.TreeViewImage };
                moduleTitle = "Variations";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "VARIATIONCollectionView", TablesGroup, null, new EntitiesParameter<PROJECT>(entity), PROJECTVARIATIONModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageProgress))
            {
                TreeViewProperty PROJECTLIVEPROGRESSModuleTreeProperty = new TreeViewProperty() { Id = "LivePROGRESSView" + entity.NUMBER, ParentId = PROJECTModuleTreeViewProperty.Id, Image = ModuleTreeProperty.TreeViewImage };
                moduleTitle = "Progress";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "PROGRESS_ITEMCollectionView", TablesGroup, null, new OptionalEntitiesParameter<PROJECT, PROGRESS>(entity, null), PROJECTLIVEPROGRESSModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageCommodity))
            {
                TreeViewProperty PROJECTCOMMODITY_CODEModuleTreeProperty = new TreeViewProperty() { Id = "ProjectCOMMODITY_CODEView" + entity.NUMBER, ParentId = PROJECTModuleTreeViewProperty.Id, Image = ModuleTreeProperty.TreeViewImage };
                moduleTitle = "Commodity Code";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "COMMODITY_CODEProjectSpecificCollectionView", TablesGroup, null, new OptionalEntitiesParameter<PROJECT, CommodityCodeTypeClass>(entity, new CommodityCodeTypeClass(CommodityCodeType.Direct)), PROJECTCOMMODITY_CODEModuleTreeProperty));
            }

            if (LoginCredentials.hasPermission(PermissionResources.ManageEstimation))
            {
                TreeViewProperty PROJECTLIVEESTIMATIONModuleTreeProperty = new TreeViewProperty() { Id = "LiveESTIMATIONDIRECTView" + entity.NUMBER, ParentId = PROJECTModuleTreeViewProperty.Id, Image = ModuleTreeProperty.TreeViewImage };
                moduleTitle = "Estimation";
                newModules.Add(BluePrintsEntitiesModuleDescription.Create(moduleTitle, "ESTIMATION_DIRECT_ITEMCollectionView", TablesGroup, null, new OptionalEntitiesParameter<PROJECT, ESTIMATION_DIRECT>(entity, null), PROJECTLIVEESTIMATIONModuleTreeProperty));
            }

            return newModules;
        }

        protected override void OnSelectedModuleChanged(BluePrintsEntitiesModuleDescription oldModule)
        {
            //Not to execute base.OnSelectedModuleChanged because navigation is invoked on double click instead
        }
    }

    public partial class BluePrintsEntitiesModuleDescription : ModuleDescription<BluePrintsEntitiesModuleDescription>
    {
        public BluePrintsEntitiesModuleDescription Clone()
        {
            return BluePrintsEntitiesModuleDescription.Create(this.ModuleTitle, this.DocumentType, this.ModuleGroup, this.NavigationTreeViewModel, this.DocumentParameter, this.TreeViewProperty);
        }

        public static BluePrintsEntitiesModuleDescription Create(string title, string documentType, string group, NavigationTreeViewModel navigationTreeViewModel, object documentParameter = null, TreeViewProperty treeViewProperty = null)
        {
            return ViewModelSource.Create(() => new BluePrintsEntitiesModuleDescription(title, documentType, group, navigationTreeViewModel, documentParameter, treeViewProperty));
        }

        protected BluePrintsEntitiesModuleDescription(string title, string documentType, string group, NavigationTreeViewModel navigationTreeViewModel, object documentParameter = null, TreeViewProperty treeViewProperty = null)
            : base(title, documentType, group, null, documentParameter, treeViewProperty)
        {
            NavigationTreeViewModel = navigationTreeViewModel;
        }

        public virtual bool IsSelected { get; set; }
        public NavigationTreeViewModel NavigationTreeViewModel { get; private set; }

        public string DisplayTitle
        {
            get 
            {
                if (ModuleTitle.Length > 8)
                    return ModuleTitle.Substring(8, ModuleTitle.Length - 8);
                return ModuleTitle;
            }
        }
        //public IFilterTreeViewModel FilterTreeViewModel { get; private set; }
    }

    public class NavigationTreeViewModel
    {
        public static NavigationTreeViewModel Create(BluePrintsEntitiesModuleDescription[] modules, string selectedTitle, BluePrintsEntitiesViewModel owner)
        {
            return ViewModelSource.Create(() => new NavigationTreeViewModel(modules, selectedTitle, owner));
        }

        protected NavigationTreeViewModel(BluePrintsEntitiesModuleDescription[] modules, string selectedTitle, BluePrintsEntitiesViewModel owner)
        {
            Owner = owner;
            Modules = modules;
            DefaultItem = Modules.FirstOrDefault(x => x.ModuleTitle == selectedTitle);
            if(DefaultItem != null)
            {
                DefaultItem.IsSelected = true;
                SelectedItem = DefaultItem;
            }

            if (selectedTitle.Length > 7)
                StaticCategoryName = selectedTitle.Substring(0, 7);
            else
                StaticCategoryName = "[" + selectedTitle + "]";

            defaultSelectionTimer = new DispatcherTimer();
            defaultSelectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            defaultSelectionTimer.Tick += defaultSelectionTimer_Tick;
        }

        BluePrintsEntitiesViewModel Owner { get; set; }
        public virtual BluePrintsEntitiesModuleDescription[] Modules { get; protected set; }
        BluePrintsEntitiesModuleDescription lastSelectedItem { get; set; }
        public virtual BluePrintsEntitiesModuleDescription SelectedItem { get; set; }
        public virtual BluePrintsEntitiesModuleDescription DefaultItem { get; set; }

        public virtual string StaticCategoryName { get; set; }

        public void ResetToAll()
        {
            SelectedItem = Modules[0];
        }

        DispatcherTimer defaultSelectionTimer;
        protected virtual void OnSelectedItemChanged()
        {
            if (SelectedItem == null)
                SelectedItem = DefaultItem;

            if (SelectedItem != DefaultItem)
            {
                Owner.NavigateCore(SelectedItem);
                defaultSelectionTimer.Start();
            }
            else
                this.RaisePropertyChanged(x => x.SelectedItem);
        }

        //need to be invoked externally because during selecteditem time the routine is still within OnTreeViewSelectedItemChanged in TreeViewSelectedItemBehavior
        void defaultSelectionTimer_Tick(object sender, EventArgs e)
        {
            defaultSelectionTimer.Stop();
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
        /// Specify whether the tile should break in tileLayout
        /// </summary>
        public bool TileLayoutFlowBreak { get; set; }
        /// <summary>
        /// Specify whether the size of the tile in tileLayout
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
        /// The navigation parameter for SingleObjectViewModel.
        /// </summary>
        public object DocumentParameter { get; private set; }
        /// <summary>
        /// Specifies the SingleObjectView document id
        /// </summary>
        public object DocumentId { get; private set; }
        /// <summary>
        /// Specifies the parentId for treeview binding, cannot be nested since dxTreeView doesn't support nested for Ids
        /// </summary>
        public object TreeViewParentId { get; private set; }
        /// <summary>
        /// Specifies the Id for treeview binding, cannot be nested since dxTreeView doesn't support nested for Ids
        /// </summary>
        public object TreeViewId { get; private set; }
        /// <summary>
        /// Specify the treeview property when binded to TreeViewControl
        /// </summary>
        public TreeViewProperty TreeViewProperty { get; private set; }
        /// <summary>
        /// Specify the treeview image property when binded to TreeViewControl
        /// </summary>
        public ImageSource TreeViewImage { get; set; }

        /// <summary>
        /// Describe whether the treelist item is expanded
        /// </summary>
        public bool TreeViewIsExpanded { get; set; }

        /// <summary>
        /// Initializes a new instance of the ModuleDescription class.
        /// </summary>
        /// <param name="title">A navigation list entry display text.</param>
        /// <param name="documentType">A string value that specifies the view type of corresponding document.</param>
        /// <param name="group">A navigation list entry group name.</param>
        /// <param name="peekCollectionViewModelFactory">An optional parameter that provides a function used to create a PeekCollectionViewModel that provides quick navigation between collection views.</param>
        /// <param name="documentParameter">A document parameter to specify SingleObjectView to display</param>
        /// <param name="treeViewProperty">A property containing tree view specific properties for view binding</param>
        public ModuleDescription(string title, string documentType, string group, Func<TModule, object> peekCollectionViewModelFactory = null, object documentParameter = null, TreeViewProperty treeViewProperty = null)
        {
            ModuleTitle = title;
            ModuleGroup = group;
            DocumentType = documentType;
            DocumentId = documentParameter == null ? documentType : (documentType + documentParameter.ToString().Replace('-', '_'));
            DocumentParameter = documentParameter;
            TreeViewProperty = treeViewProperty;
            TreeViewParentId = treeViewProperty.ParentId;
            TreeViewId = treeViewProperty.Id;
            TreeViewImage = treeViewProperty.Image;
            TreeViewIsExpanded = treeViewProperty.IsExpanded;
            this.peekCollectionViewModelFactory = peekCollectionViewModelFactory;
        }
    }
}