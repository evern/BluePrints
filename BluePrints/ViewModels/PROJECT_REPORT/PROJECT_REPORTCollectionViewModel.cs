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
    /// Represents the PROJECT_REPORTS collection view model.
    /// </summary>
    public partial class PROJECT_REPORTSCollectionViewModel : CollectionViewModel<PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of PROJECT_REPORTSCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECT_REPORTSCollectionViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null, Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> projection = null)
        {
            return ViewModelSource.Create(() => new PROJECT_REPORTSCollectionViewModel(unitOfWorkFactory, projection));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECT_REPORTSCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECT_REPORTSCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECT_REPORTSCollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null, Func<IRepositoryQuery<PROJECT_REPORT>, IQueryable<PROJECT_REPORT>> projection = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.PROJECT_REPORTS, projection)
        {
        }
    }
}