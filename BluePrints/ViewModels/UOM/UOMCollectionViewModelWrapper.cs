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
    public class UOMCollectionViewModelWrapper :
        CollectionViewModelsWrapper
        <UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork,
            CollectionViewModel<UOM, UOM, Guid, IBluePrintsEntitiesUnitOfWork>>
    {
        /// <summary>
        /// Creates a new instance of UOMCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static UOMCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new UOMCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the UOMCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the UOMCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected UOMCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory =
            BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        protected override void InitializeParameters(object parameter)
        {
        }

        public override void InitializeAndLoadEntitiesLoaderDescription()
        {
            MainViewModel = null;
            base.CleanUpEntitiesLoader();

            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
            InvokeEntitiesLoaderDescriptionLoading();
        }

        protected override void OnAllEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.UOMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<UOM>, IQueryable<UOM>> ConstructMainViewModelProjection()
        {
            return query => query;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<UOM> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "UOMCollectionViewModelWrapper"; }
        }

        #endregion
    }
}