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
    /// Represents the DEPARTMENTS collection view model.
    /// </summary>
    public partial class DEPARTMENTCollectionViewModel : CollectionViewModel<DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of DEPARTMENTCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static DEPARTMENTCollectionViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null, Func<IRepositoryQuery<DEPARTMENT>, IQueryable<DEPARTMENT>> projection = null)
        {
            return ViewModelSource.Create(() => new DEPARTMENTCollectionViewModel(unitOfWorkFactory, projection));
        }

        /// <summary>
        /// Initializes a new instance of the DEPARTMENTCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the DEPARTMENTCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected DEPARTMENTCollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null, Func<IRepositoryQuery<DEPARTMENT>, IQueryable<DEPARTMENT>> projection = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.DEPARTMENTS, projection)
        {
        }
    }
}