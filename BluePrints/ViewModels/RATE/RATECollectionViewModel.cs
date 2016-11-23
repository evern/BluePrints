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
    /// Represents the RATES collection view model.
    /// </summary>
    public partial class RATECollectionViewModel : CollectionViewModel<RATE, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of RATECollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static RATECollectionViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new RATECollectionViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Creates a new instance of RATECollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static RATECollectionViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null, Func<IRepositoryQuery<RATE>, IQueryable<RATE>> projection = null)
        {
            return ViewModelSource.Create(() => new RATECollectionViewModel(unitOfWorkFactory, projection));
        }

        /// <summary>
        /// Initializes a new instance of the RATECollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the RATECollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected RATECollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.RATES)
        {
        }
        

        /// <summary>
        /// Initializes a new instance of the RATECollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the RATECollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected RATECollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null, Func<IRepositoryQuery<RATE>, IQueryable<RATE>> projection = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.RATES, projection)
        {
        }

        /// <summary>
        /// The view model that contains a look-up collection of DEPARTMENTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DEPARTMENT> LookUpDEPARTMENTS
        {
            get { return GetLookUpEntitiesViewModel((BASELINE_ITEMSViewModel x) => x.LookUpDEPARTMENTS, x => x.DEPARTMENTS); }
        }

        /// <summary>
        /// The view model that contains a look-up collection of DISCIPLINES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DISCIPLINE> LookUpDISCIPLINES
        {
            get { return GetLookUpEntitiesViewModel((BASELINE_ITEMSViewModel x) => x.LookUpDISCIPLINES, x => x.DISCIPLINES); }
        }
    }
}