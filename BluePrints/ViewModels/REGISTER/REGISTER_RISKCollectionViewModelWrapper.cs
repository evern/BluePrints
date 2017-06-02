using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Misc;
using BluePrints.Common.Resources;
using BluePrints.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class REGISTER_RISKCollectionViewModelWrapper :
        BluePrintsEntitiesAutoNumberCollectionWrapper
        <REGISTER_RISK, REGISTER_RISK, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of REGISTERCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static REGISTER_RISKCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new REGISTER_RISKCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the REGISTERCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the REGISTERCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected REGISTER_RISKCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private PROJECT loadPROJECT;

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.REGISTER_RISK);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<REGISTER_RISK>, IQueryable<REGISTER_RISK>> ConstructMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<REGISTER_RISK> entities)
        {
            MainViewModel.SetParentAssociationCallBack = OnBeforeEntitySaved;
            MainViewModel.ExistingRowAddUndoAndSaveCallBack = ExistingRowAddUndoAndSaveCallBack;
            MainViewModel.IsContinueNewRowFromViewCallBack += IsContinueNewRowFromView;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void OnBeforeEntitySaved(REGISTER_RISK entity)
        {
            entity.GUID_PROJECT = loadPROJECT.GUID;
            if(entity.GUID == Guid.Empty && entity.DATE_IDENTIFIED == null)
                entity.DATE_IDENTIFIED = DateTime.Now.Date;
        }

        private bool IsContinueNewRowFromView(RowEventArgs e, REGISTER_RISK projection)
        {
            if (projection.RISK_LIKELIHOOD != null && projection.RISK_CONSEQUENCES != null)
            {
                Register_RiskRanking? oldValue = projection.RISK_RANKING;
                Register_RiskRanking? newValue = RiskMatrix.GetRanking(projection.RISK_LIKELIHOOD, projection.RISK_CONSEQUENCES);

                projection.RISK_RANKING = newValue;
            }
            if (projection.RESIDUE_RISK_LIKELIHOOD != null && projection.RESIDUE_RISK_CONSEQUENCES != null)
            {
                Register_RiskRanking? oldValue = projection.RESIDUE_RISK_RANKING;
                Register_RiskRanking? newValue = RiskMatrix.GetRanking(projection.RESIDUE_RISK_LIKELIHOOD, projection.RESIDUE_RISK_CONSEQUENCES);

                projection.RESIDUE_RISK_RANKING = newValue;
            }

            return true;
        }

        private bool ExistingRowAddUndoAndSaveCallBack(REGISTER_RISK projectionEntity, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == BindableBase.GetPropertyName(() => new REGISTER_RISK().RISK_CONSEQUENCES) || 
                e.Column.FieldName == BindableBase.GetPropertyName(() => new REGISTER_RISK().RISK_LIKELIHOOD))
            {
                if(projectionEntity.RISK_LIKELIHOOD != null && projectionEntity.RISK_CONSEQUENCES != null)
                {
                    Register_RiskRanking? oldValue = projectionEntity.RISK_RANKING;
                    Register_RiskRanking? newValue = RiskMatrix.GetRanking(projectionEntity.RISK_LIKELIHOOD, projectionEntity.RISK_CONSEQUENCES);

                    projectionEntity.RISK_RANKING = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity, BindableBase.GetPropertyName(() => new REGISTER_RISK().RISK_RANKING), oldValue, e.Value, EntityMessageType.Changed);
                }
            }
            else if (e.Column.FieldName == BindableBase.GetPropertyName(() => new REGISTER_RISK().RESIDUE_RISK_CONSEQUENCES) ||
                     e.Column.FieldName == BindableBase.GetPropertyName(() => new REGISTER_RISK().RESIDUE_RISK_LIKELIHOOD))
            {
                if (projectionEntity.RESIDUE_RISK_LIKELIHOOD != null && projectionEntity.RESIDUE_RISK_CONSEQUENCES != null)
                {
                    Register_RiskRanking? oldValue = projectionEntity.RESIDUE_RISK_RANKING;
                    Register_RiskRanking? newValue = RiskMatrix.GetRanking(projectionEntity.RESIDUE_RISK_LIKELIHOOD, projectionEntity.RESIDUE_RISK_CONSEQUENCES);

                    projectionEntity.RESIDUE_RISK_RANKING = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projectionEntity, BindableBase.GetPropertyName(() => new REGISTER_RISK().RESIDUE_RISK_RANKING), oldValue, e.Value, EntityMessageType.Changed);
                }
            }

            return true;
        }

        #endregion

        #endregion

        #region IEntityNumber
        protected override string GetEntityNumberFieldName()
        {
            return BindableBase.GetPropertyName(() => new REGISTER_CHANGE().NUMBER);
        }

        protected override int DefaultNumericFieldLength()
        {
            return Int32.Parse(BluePrintsResources.REGISTER_DefaultNumberFieldLength);
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "REGISTER_RISKCollectionViewModelWrapper"; }
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
        #endregion
    }
}