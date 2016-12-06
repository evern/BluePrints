using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.DataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.Data.Helpers;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BluePrints.Common.Helpers;

namespace BluePrints.ViewModels
{
    public class WORKPACKCollectionViewModelWrapper : CollectionViewModelsWrapper<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork, CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        /// <summary>
        /// Creates a new instance of WORKPACKCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static WORKPACKCollectionViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new WORKPACKCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the WORKPACKCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the WORKPACKCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected WORKPACKCollectionViewModelWrapper(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        PROJECT loadPROJECT;
        IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void InitializeParameters(object parameter)
        {
            EntitiesParameter<PROJECT> PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            this.loadPROJECT = PROJECTParameter.GetEntity();
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddEntitiesLoader<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(0, bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, null, isContinueLoadingAfterPROJECT, OnEntitiesChanged);
            loaderCollection.AddEntitiesLoader<PHASE, PHASE, Guid, IBluePrintsEntitiesUnitOfWork>(1, bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc, typeof(PROJECT));
            loaderCollection.AddEntitiesLoader<AREA, AREA, Guid, IBluePrintsEntitiesUnitOfWork>(2, bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc, typeof(PROJECT));
            loaderCollection.AddEntitiesLoader<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(3, bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddEntitiesLoader<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(4, bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddEntitiesLoader<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(5, bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            
            InvokeEntitiesLoaderDescriptionLoading();
        }

        bool isContinueLoadingAfterPROJECT(IEnumerable<PROJECT> entities)
        {
            if (entities.Count() == 0)
            {
                mainThreadDispatcher.BeginInvoke(new Action(() => MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, "PROJECT"))));
                return false;
            }

            this.loadPROJECT = entities.First();
            return true;
        }

        Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == this.loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(this.bluePrintsUnitOfWorkFactory, x => x.WORKPACKS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoader.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> ConstructMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<WORKPACK> entities)
        {
            MainViewModel.OnBeforeEntitySavedCallBack = this.OnBeforeEntitySaved;
            MainViewModel.SetParentViewModel(this);
            mainThreadDispatcher.BeginInvoke(new Action(() => this.RaisePropertiesChanged()));
        }

        protected override void OnEntitiesChanged(object key, Type changedType, EntityMessageType messageType, object sender)
        {
            if (sender == MainViewModel)
                return;

            if (loadPROJECT != null && changedType == typeof(BluePrints.Data.PROJECT) && loadPROJECT.GUID.ToString() == key.ToString())
            {
                if (messageType == EntityMessageType.Added)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Restored, StringFormatUtils.GetEntityNameByType(changedType)));
                else if (messageType == EntityMessageType.Deleted)
                    MessageBoxService.ShowMessage(string.Format(CommonResources.Notify_View_Removed, StringFormatUtils.GetEntityNameByType(changedType)));
            }

            if (loadPROJECT != null)
            {
                if (MainViewModel != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => MainViewModel.Refresh()));
                else if (loadPROJECT != null)
                    mainThreadDispatcher.BeginInvoke(new Action(() => InitializeAndLoadEntitiesLoaderDescription()));
            }
        }

        #region Collection Call Backs
        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnBeforeEntitySaved(WORKPACK entity)
        {
            entity.GUID_PROJECT = this.loadPROJECT.GUID;
        }
        #endregion
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get
            {
                return "WORKPACKCollectionViewModelWrapper";
            }
        }

        public IEnumerable<PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
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

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<DEPARTMENT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
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

        public IEnumerable<DOCTYPE> DOCTYPECollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }
        #endregion

        #region View Behavior

        /// <summary>
        /// Allow cells to commit immediately upon losing focus
        /// </summary>
        public void CellValueChanged(CellValueChangedEventArgs e)
        {
            WORKPACK changedWORKPACK = (WORKPACK)e.Row;
            if (e.Column.FieldName == "GUID_DDISCIPLINE" || e.Column.FieldName == "GUID_DDOCTYPE")
            {
                string newInternalName  = BluePrintDataUtils.WORKPACK_Generate_InternalNumber1(loadPROJECT, changedWORKPACK, MainViewModel.Entities, AREACollection, DISCIPLINECollection, DOCTYPECollection);
                if(newInternalName == string.Empty)
                    return;

                if (MessageBoxService.ShowMessage(CommonResources.WORKPACK_InternalNameChange, CommonResources.Confirmation_Caption, MessageButton.YesNo) != MessageResult.Yes)
                    return;

                changedWORKPACK.INTERNAL_NAME1 = newInternalName;

                if (e.Column.FieldName == "GUID_DDISCIPLINE")
                {
                    changedWORKPACK.INTERNAL_NAME2 = BluePrintDataUtils.WORKPACK_Generate_InternalNumber2(loadPROJECT, changedWORKPACK, MainViewModel.Entities, AREACollection, DISCIPLINECollection, PHASECollection);
                }
            }
            else if (e.Column.FieldName == "GUID_DPHASE" || e.Column.FieldName == "GUID_DAREA")
            {
                string newInternalName = BluePrintDataUtils.WORKPACK_Generate_InternalNumber2(loadPROJECT, changedWORKPACK, MainViewModel.Entities, AREACollection, DISCIPLINECollection, PHASECollection);
                if(newInternalName == string.Empty)
                    return;

                if (MessageBoxService.ShowMessage(CommonResources.WORKPACK_InternalNameChange, CommonResources.Confirmation_Caption, MessageButton.YesNo) != MessageResult.Yes)
                    return;

                changedWORKPACK.INTERNAL_NAME2 = newInternalName;
            }
            else if (e.Column.FieldName == "STARTDATE" || e.Column.FieldName == "ENDDATE" || e.Column.FieldName == "REVIEWSTARTDATE" || e.Column.FieldName == "REVIEWENDDATE")
            {
                changedWORKPACK.AUTOGENERATED = false;
            }

            if(e.RowHandle != GridControl.NewItemRowHandle)
                MainViewModel.Save(changedWORKPACK);
        }

        public void CellValueChanging(CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "GUID_DDOCTYPE")
            {
                WORKPACK changingWORKPACK = (WORKPACK)e.Row;
                DOCTYPE chosenDOCTYPE = DOCTYPECollection.FirstOrDefault(entity => entity.GUID == (Guid)e.Value);
                if (chosenDOCTYPE != null && chosenDOCTYPE.GUID_DDEPARTMENT != null)
                {
                    changingWORKPACK.GUID_DDEPARTMENT = chosenDOCTYPE.DEPARTMENT.GUID;
                    MainViewModel.UpdateSelectedEntity();
                }
            }
            else if (e.Column.FieldName == "STARTDATE" || e.Column.FieldName == "ENDDATE")
            {
                DateTime startDate;
                DateTime endDate;

                WORKPACK changingWORKPACK = (WORKPACK)e.Row;
                if (e.Column.FieldName == "STARTDATE")
                {
                    startDate = (DateTime)e.Value;
                    endDate = changingWORKPACK.ENDDATE;
                    if (endDate < startDate)
                    {
                        endDate = BluePrintDataUtils.WORKPACK_Calculate_EndDate(startDate, loadPROJECT);
                        changingWORKPACK.ENDDATE = endDate;
                    }
                }
                else
                {
                    endDate = (DateTime)e.Value;
                    startDate = changingWORKPACK.STARTDATE;
                    if (endDate < startDate)
                    {
                        startDate = BluePrintDataUtils.WORKPACK_Calculate_StartDate(endDate, loadPROJECT);
                        changingWORKPACK.STARTDATE = startDate;
                    }
                }

                DateTime reviewStartDate = startDate;
                DateTime reviewEndDate = endDate;

                BluePrintDataUtils.WORKPACK_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate, loadPROJECT, false);
                changingWORKPACK.REVIEWSTARTDATE = reviewStartDate;

                if (reviewEndDate >= endDate)
                    changingWORKPACK.REVIEWENDDATE = endDate;
                else
                    changingWORKPACK.REVIEWENDDATE = reviewEndDate;

                MainViewModel.UpdateSelectedEntity();
            }
        }
        #endregion
    }
}
