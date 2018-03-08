using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class WORKPACKCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of WORKPACKCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static WORKPACKCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new WORKPACKCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the WORKPACKCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the WORKPACKCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected WORKPACKCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        private PROJECT loadPROJECT;
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
        }

        private Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<WORKPACK> entities)
        {
            MainViewModel.OnBeforeEntitySavedIsContinueCallBack = OnBeforeEntitySave;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        private bool OnBeforeEntitySave(WORKPACK workpack)
        {
            BluePrintsDataUtils.WORKPACK_Populate_Name(workpack, SUBJOBCollection, DISCIPLINECollection);
            return true;
        }
        #endregion

        #region View Commands
        public void GenerateWORKPACKS()
        {
            loadBASELINE_ITEMViewModel();
        }

        BASELINE_ITEMCollectionViewModelWrapper baseline_itemCollectionViewModel;
        private void loadBASELINE_ITEMViewModel()
        {
            baseline_itemCollectionViewModel = BASELINE_ITEMCollectionViewModelWrapper.Create();
            baseline_itemCollectionViewModel.OnEntitiesLoadedCallBackManualDispose = true;
            baseline_itemCollectionViewModel.SetParentViewModel(this);
            baseline_itemCollectionViewModel.OnEntitiesLoadedCallBack = onBASELINE_ITEMLoaded;
            var baselineSupportParameterObj = baseline_itemCollectionViewModel as ISupportParameter;
            baselineSupportParameterObj.Parameter = new TripleEntitiesParameter<PROJECT, IAmBaseline, object>(loadPROJECT, null, DeliverablesViewType.Both);
        }

        private void onBASELINE_ITEMLoaded(IEnumerable<BASELINE_ITEMProgress> baseline_items, object parentId)
        {
            mainThreadDispatcher.BeginInvoke(new Action(() => generateWorkpacks(baseline_items, parentId)));
        }

        private void generateWorkpacks(IEnumerable<BASELINE_ITEMProgress> baseline_items, object parentId)
        {
            List<WORKPACK> removeWORKPACKS = new List<WORKPACK>();
            MainViewModel.EntitiesUndoRedoManager.PauseActionId();

            LoadingScreenManager.ShowLoadingScreen(MainViewModel.Entities.Count);
            //LoadingScreenManager.SetMessage("Removing redundant workpacks");
            foreach (WORKPACK workpack in MainViewModel.Entities)
            {
                if (!baseline_items.Any(x => x.Entity.Entity.GUID_WORKPACK == workpack.GUID))
                {
                    removeWORKPACKS.Add(workpack);
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(workpack, null, null, null, EntityMessageType.Deleted);
                }

                LoadingScreenManager.Progress();
            }
            MainViewModel.BaseBulkDelete(removeWORKPACKS);

            List<BASELINE_ITEMProgress> baseline_itemsToSave = new List<BASELINE_ITEMProgress>();
            LoadingScreenManager.CloseLoadingScreen();
            LoadingScreenManager.ShowLoadingScreen(baseline_items.Count() * 2);
            //LoadingScreenManager.SetMessage("Assigning workpacks to deliverables");
            foreach(BASELINE_ITEMProgress deliverable in baseline_items)
            {
                Guid? subjob_guid = deliverable.Entity.Entity.GUID_SUBJOB;
                Guid? discipline_guid = deliverable.Entity.Entity.GUID_DISCIPLINE;
                decimal discipline_number = deliverable.Entity.Entity.DISCIPLINE_NUM;

                if(subjob_guid != null && discipline_guid != null)
                {
                    WORKPACK queryWORKPACK = MainViewModel.Entities.FirstOrDefault(x => x.GUID_DISCIPLINE == discipline_guid && x.GUID_SUBJOB == subjob_guid && x.DISCIPLINE_NUM == discipline_number);
                    if (queryWORKPACK == null)
                    {
                        WORKPACK newWORKPACK = new WORKPACK();
                        newWORKPACK.GUID_SUBJOB = (Guid)subjob_guid;
                        newWORKPACK.GUID_DISCIPLINE = (Guid)discipline_guid;
                        newWORKPACK.DISCIPLINE_NUM = discipline_number;
                        MainViewModel.Save(newWORKPACK);
                        MainViewModel.EntitiesUndoRedoManager.AddUndo(newWORKPACK, null, null, null, EntityMessageType.Added);
                        queryWORKPACK = newWORKPACK;
                    }
                    else
                    {
                        //fix internal number with OnBeforeEntitySaved
                        MainViewModel.Save(queryWORKPACK);
                    }

                    deliverable.Entity.Entity.GUID_WORKPACK = queryWORKPACK.GUID;
                    baseline_itemsToSave.Add(deliverable);
                }

                LoadingScreenManager.Progress();
            }

            foreach(BASELINE_ITEMProgress deliverable in baseline_itemsToSave)
            {
                LoadingScreenManager.Progress();
                baseline_itemCollectionViewModel.MainViewModel.Save(deliverable);
            }

            LoadingScreenManager.CloseLoadingScreen();
            MainViewModel.EntitiesUndoRedoManager.UnpauseActionId();

            baseline_itemCollectionViewModel.CleanUpEntitiesLoader();
            baseline_itemCollectionViewModel = null;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "WORKPACKCollectionViewModelWrapper"; }
        }

        public IEnumerable<SUBJOB> SUBJOBCollection
        {
            get
            {
                var collection = GetEntities<SUBJOB>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1);
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
        #endregion

        public override string UnifiedValueValidation(WORKPACK projection, string field_name, object new_value)
        {
            return string.Empty;
        }
    }
}