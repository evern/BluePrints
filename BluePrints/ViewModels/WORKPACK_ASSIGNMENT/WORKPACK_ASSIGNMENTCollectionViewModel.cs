using System;
using System.Linq;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the WORKPACK_ASSIGNMENTS collection view model.
    /// </summary>
    public partial class WORKPACK_ASSIGNMENTSCollectionViewModel : CollectionViewModel<WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of WORKPACK_ASSIGNMENTSCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static WORKPACK_ASSIGNMENTSCollectionViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new WORKPACK_ASSIGNMENTSCollectionViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the WORKPACK_ASSIGNMENTSCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the WORKPACK_ASSIGNMENTSCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected WORKPACK_ASSIGNMENTSCollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.WORKPACK_ASSIGNMENTS)
        {
        }

        /// <summary>
        /// Creates a new instance of WORKPACK_ASSIGNMENTSCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static WORKPACK_ASSIGNMENTSCollectionViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null, Func<IRepositoryQuery<WORKPACK_ASSIGNMENT>, IQueryable<WORKPACK_ASSIGNMENT>> projection = null)
        {
            return ViewModelSource.Create(() => new WORKPACK_ASSIGNMENTSCollectionViewModel(unitOfWorkFactory, projection));
        }

        /// <summary>
        /// Initializes a new instance of the WORKPACK_ASSIGNMENTSCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the WORKPACK_ASSIGNMENTSCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected WORKPACK_ASSIGNMENTSCollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null, Func<IRepositoryQuery<WORKPACK_ASSIGNMENT>, IQueryable<WORKPACK_ASSIGNMENT>> projection = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.WORKPACK_ASSIGNMENTS, projection)
        {
        }
    }
}