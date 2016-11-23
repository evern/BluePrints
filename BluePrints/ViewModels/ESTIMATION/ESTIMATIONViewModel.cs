using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single ESTIMATION object view model.
    /// </summary>
    public partial class ESTIMATIONViewModel : SingleObjectViewModel<ESTIMATION, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of ESTIMATIONViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATIONViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ESTIMATIONViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the ESTIMATIONViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the ESTIMATIONViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ESTIMATIONViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.ESTIMATIONS, x => x.NAME)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of ESTIMATION_ITEMS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<ESTIMATION_ITEM> LookUpESTIMATION_ITEMS
        {
            get { return GetLookUpEntitiesViewModel((ESTIMATIONViewModel x) => x.LookUpESTIMATION_ITEMS, x => x.ESTIMATION_ITEMS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of PROJECTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<PROJECT> LookUpPROJECTS
        {
            get { return GetLookUpEntitiesViewModel((ESTIMATIONViewModel x) => x.LookUpPROJECTS, x => x.PROJECTS); }
        }


        /// <summary>
        /// The view model for the ESTIMATIONESTIMATION_ITEMS detail collection.
        /// </summary>
        public CollectionViewModel<ESTIMATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> ESTIMATIONESTIMATION_ITEMSDetails
        {
            get { return GetDetailsCollectionViewModel((ESTIMATIONViewModel x) => x.ESTIMATIONESTIMATION_ITEMSDetails, x => x.ESTIMATION_ITEMS, x => x.GUID_ESTIMATION, (x, key) => { x.GUID_ESTIMATION = key; }); }
        }
    }
}
