using System;
using System.Linq;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using DevExpress.Mvvm;
using BluePrints.Common.ViewModel.Filtering;
using System.Linq.Expressions;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the AREAS collection view model.
    /// </summary>
    public partial class AREACollectionViewModel : CollectionViewModel<AREA, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of AREACollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static AREACollectionViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null, Func<IRepositoryQuery<AREA>, IQueryable<AREA>> projection = null)
        {
            return ViewModelSource.Create(() => new AREACollectionViewModel(unitOfWorkFactory, projection));
        }

        /// <summary>
        /// Initializes a new instance of the AREACollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the AREACollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected AREACollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null, Func<IRepositoryQuery<AREA>, IQueryable<AREA>> projection = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.AREAS, projection)
        {
        }

        /// <summary>
        /// The view model that contains a look-up collection of PROJECTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<PROJECT> LookUpPROJECTS
        {
            get { return GetLookUpEntitiesViewModel((AREACollectionViewModel x) => x.LookUpPROJECTS, x => x.PROJECTS); }
        }
    }
}