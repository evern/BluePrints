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

        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>) parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PROJECTS, PROJECTProjectionFunc, x => loadPROJECT = x);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
        }

        private Func<IRepositoryQuery<PROJECT>, IQueryable<PROJECT>> PROJECTProjectionFunc()
        {
            return query => query.Where(x => x.GUID == loadPROJECT.GUID);
        }

        private Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.REGISTER_RISK);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<REGISTER_RISK>, IQueryable<REGISTER_RISK>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<REGISTER_RISK> entities)
        {
            MainViewModel.OnBeforeViewNewRowSavedIsContinueCallBack += IsContinueNewRowFromView;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Collection Call Backs
        protected override bool onBeforeEntitySavedIsContinue(REGISTER_RISK projection)
        {
            projection.GUID_PROJECT = loadPROJECT.GUID;
            if (projection.GUID == Guid.Empty && projection.DATE_IDENTIFIED == null)
                projection.DATE_IDENTIFIED = DateTime.Now.Date;
            return base.onBeforeEntitySavedIsContinue(projection);
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

        public override void UnifiedCellValueChanging(string field_name, object old_value, object new_value, REGISTER_RISK projection, bool isNew)
        {
            if (field_name == BindableBase.GetPropertyName(() => new REGISTER_RISK().RISK_CONSEQUENCES) || field_name == BindableBase.GetPropertyName(() => new REGISTER_RISK().RISK_LIKELIHOOD))
            {
                if (projection.RISK_LIKELIHOOD != null && projection.RISK_CONSEQUENCES != null)
                {
                    Register_RiskRanking? oldValue = projection.RISK_RANKING;
                    Register_RiskRanking? newValue = RiskMatrix.GetRanking(projection.RISK_LIKELIHOOD, projection.RISK_CONSEQUENCES);

                    projection.RISK_RANKING = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new REGISTER_RISK().RISK_RANKING), oldValue, newValue, EntityMessageType.Changed);
                }
            }
            else if (field_name == BindableBase.GetPropertyName(() => new REGISTER_RISK().RESIDUE_RISK_CONSEQUENCES) ||
                     field_name == BindableBase.GetPropertyName(() => new REGISTER_RISK().RESIDUE_RISK_LIKELIHOOD))
            {
                if (projection.RESIDUE_RISK_LIKELIHOOD != null && projection.RESIDUE_RISK_CONSEQUENCES != null)
                {
                    Register_RiskRanking? oldValue = projection.RESIDUE_RISK_RANKING;
                    Register_RiskRanking? newValue = RiskMatrix.GetRanking(projection.RESIDUE_RISK_LIKELIHOOD, projection.RESIDUE_RISK_CONSEQUENCES);

                    projection.RESIDUE_RISK_RANKING = newValue;
                    MainViewModel.EntitiesUndoRedoManager.AddUndo(projection, BindableBase.GetPropertyName(() => new REGISTER_RISK().RESIDUE_RISK_RANKING), oldValue, newValue, EntityMessageType.Changed);
                }
            }
            base.UnifiedCellValueChanging(field_name, old_value, new_value, projection, isNew);
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
            return Int32.Parse(BluePrintsResources.Default_Register_Numeric_Length);
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            //get { return "REGISTER_RISKCollectionViewModelWrapper" + view_project_specific_affix; }
            get { return "REGISTER_RISKCollectionViewModelWrapper_v1"; }
        }

        private string view_project_specific_affix
        {
            get
            {
                if (loadPROJECT == null)
                    return string.Empty;
                return loadPROJECT.GUID.ToString();
            }
        }

        protected override string ExportFilename()
        {
            return loadPROJECT.NUMBER + "_Register_Risk";
        }
        
        public override string UnifiedRowValidation(REGISTER_RISK projection)
        {
            return string.Empty;
        }


        public override string UnifiedValueValidation(REGISTER_RISK projection, string field_name, object new_value)
        {
            return string.Empty;
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