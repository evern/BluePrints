using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class EXO_JobHistoryCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <JOBCOST_LINES_AUDIT, JOBCOST_LINES_AUDIT, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of EXO_JobHistoryCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static EXO_JobHistoryCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new EXO_JobHistoryCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the EXO_JobHistoryCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the EXO_JobHistoryCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected EXO_JobHistoryCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        BluePrintsNativeEntities bluePrintsEntitiesUnitOfWork;
        protected Data.PROJECT loadPROJECT;
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<Data.PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
            bluePrintsEntitiesUnitOfWork = new BluePrintsNativeEntities("name=BluePrintsPerthEntities");
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<USER, USER, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.USERS);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.JOBCOST_LINES_AUDITS);
        }

        protected override Func<IRepositoryQuery<JOBCOST_LINES_AUDIT>, IQueryable<JOBCOST_LINES_AUDIT>> specifyMainViewModelProjection()
        {
            return query => populateEXO_JobHistoryProject(query);
        }

        private IQueryable<JOBCOST_LINES_AUDIT> populateEXO_JobHistoryProject(IQueryable<JOBCOST_LINES_AUDIT> query)
        {
            //get include deleted
            return bluePrintsEntitiesUnitOfWork.JOBCOST_LINES_AUDIT.Where(x => x.JOBCODE.Contains(loadPROJECT.NUMBER)).AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<JOBCOST_LINES_AUDIT> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override void FullRefresh()
        {
            bluePrintsEntitiesUnitOfWork = new BluePrintsNativeEntities("name=BluePrintsPerthEntities");
            base.FullRefresh();
        }
        #endregion

        #region Saving Behavior
        public override string UnifiedRowValidation(JOBCOST_LINES_AUDIT projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(JOBCOST_LINES_AUDIT projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "EXO_JobHistoryCollectionViewModelWrapper_v2"; }
        }

        public IEnumerable<USER> USERCollection
        {
            get
            {
                var collection = GetEntities<USER>();
                return collection.OrderBy(x => x.NAME);
            }
        }
        #endregion
    }
}