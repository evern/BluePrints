using BaseModel.DataModel;
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
    public class PIPELINECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <PIPELINE, PIPELINE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PIPELINECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PIPELINECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PIPELINECollectionViewModelWrapper(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PIPELINECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PIPELINECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PIPELINECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void resolveParameters(object parameter)
        {
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.PIPELINES);
        }

        protected override Func<IRepositoryQuery<PIPELINE>, IQueryable<PIPELINE>> specifyMainViewModelProjection()
        {
            return query => populatePipelineProject(query);
        }

        private IQueryable<PIPELINE> populatePipelineProject(IQueryable<PIPELINE> query)
        {
            List<PIPELINE> pipeline = query.ToList();
            return pipeline.AsQueryable();
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<PIPELINE> entities)
        {
            MainViewModel.OnAfterProjectionSavedCallBack = onAfterEntitySaved;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Saving Behavior
        private void onAfterEntitySaved(PIPELINE entity, PIPELINE projection, bool isNewEntity)
        {
            onAfterPipelineSaved(entity);
        }

        private void onAfterPipelineSaved(PIPELINE entity)
        {

        }

        public override string UnifiedRowValidation(PIPELINE projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(PIPELINE projection, string field_name, object new_value, bool isPaste)
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
            get { return "PIPELINECollectionViewModelWrapper"; }
        }

        public IEnumerable<PROJECT> PROJECTCollection
        {
            get
            {
                var collection = GetEntities<PROJECT>();
                if (collection == null)
                    return new List<PROJECT>();

                //need to call ToList for tokenComboBoxEditSettings to work
                return collection.OrderBy(x => x.NUMBER).ToList();
            }
        }
        #endregion
    }
}