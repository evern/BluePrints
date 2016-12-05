using System;
using System.Linq;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using BluePrints.Common.ViewModel.Filtering;
using DevExpress.Mvvm;
using System.Linq.Expressions;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the INDIRECT_TYPES collection view model.
    /// </summary>
    public partial class INDIRECT_TYPECollectionViewModel : CollectionViewModel<INDIRECT_TYPE, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of INDIRECT_TYPECollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static INDIRECT_TYPECollectionViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null, Func<IRepositoryQuery<INDIRECT_TYPE>, IQueryable<INDIRECT_TYPE>> projection = null)
        {
            return ViewModelSource.Create(() => new INDIRECT_TYPECollectionViewModel(unitOfWorkFactory, projection));
        }

        /// <summary>
        /// Initializes a new instance of the INDIRECT_TYPECollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the INDIRECT_TYPECollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected INDIRECT_TYPECollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null, Func<IRepositoryQuery<INDIRECT_TYPE>, IQueryable<INDIRECT_TYPE>> projection = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.INDIRECT_TYPES, projection)
        {
        }
    }
}