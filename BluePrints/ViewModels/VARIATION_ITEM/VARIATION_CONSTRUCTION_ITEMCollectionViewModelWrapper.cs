using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Filtering;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single VARIATION object view model.
    /// </summary>
    public partial class VARIATION_CONSTRUCTION_ITEMCollectionViewModelWrapper : BluePrintsEntitiesCollectionWrapper<VARIATION_CONSTRUCTION_ITEM, VARIATION_CONSTRUCTION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static VARIATION_CONSTRUCTION_ITEMCollectionViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new VARIATION_CONSTRUCTION_ITEMCollectionViewModelWrapper());
        }

        protected PROJECT loadPROJECT;
        protected VARIATION_CONSTRUCTION loadVARIATION;
        protected IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory;
        protected readonly IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected IPrimeroEntitiesUnitOfWork primeroUnitOfWork;
        protected IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork;
        protected override void resolveParameters(object parameter)
        {            
            var receiveParameter = (DualEntitiesParameter<PROJECT, VARIATION_CONSTRUCTION>)parameter;
            loadPROJECT = receiveParameter.GetFirstEntity();
            primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(loadPROJECT.OfficeNameForExo == BluePrintsResources.OfficeMontreal);
            primeroUnitOfWork = primeroUnitOfWorkFactory.CreateUnitOfWork();
            bluePrintsUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            loadVARIATION = receiveParameter.GetSecondEntity();
            //base.resolveParameters(parameter);
        }
        
        protected override void addEntitiesLoader()
        {
        }

        public override string ViewName => "CONSTRUCTION_VARIATION_ITEMSViewModelWrapper_v4";

        protected override Func<IRepositoryQuery<VARIATION_CONSTRUCTION_ITEM>, IQueryable<VARIATION_CONSTRUCTION_ITEM>> specifyMainViewModelProjection()
        {
            return query => query.Where(x => x.GUID_VARIATION == loadVARIATION.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.VARIATION_CONSTRUCTION_ITEMS);
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<VARIATION_CONSTRUCTION_ITEM> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        #region Tag saving behavior
        protected override OperationInterceptMode OnBeforeProjectionSaveIsContinue(VARIATION_CONSTRUCTION_ITEM projection, out bool isNew)
        {
            projection.GUID_VARIATION = loadVARIATION.GUID;
            isNew = false;
            return base.OnBeforeProjectionSaveIsContinue(projection, out isNew);
        }

        public override string UnifiedValueValidation(VARIATION_CONSTRUCTION_ITEM projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(VARIATION_CONSTRUCTION_ITEM projection)
        {
            return string.Empty;
        }
        #endregion

        #region View Property
        #endregion
    }
}