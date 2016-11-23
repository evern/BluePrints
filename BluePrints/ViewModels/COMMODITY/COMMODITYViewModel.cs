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
    /// Represents the single COMMODITY object view model.
    /// </summary>
    public partial class COMMODITYViewModel : SingleObjectViewModel<COMMODITY, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of COMMODITYViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static COMMODITYViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new COMMODITYViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the COMMODITYViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the COMMODITYViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected COMMODITYViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.COMMODITIES, x => x.ITEM_DESC)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of ESTIMATION_ITEMS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<ESTIMATION_ITEM> LookUpESTIMATION_ITEMS
        {
            get { return GetLookUpEntitiesViewModel((COMMODITYViewModel x) => x.LookUpESTIMATION_ITEMS, x => x.ESTIMATION_ITEMS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of PROJECTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<PROJECT> LookUpPROJECTS
        {
            get { return GetLookUpEntitiesViewModel((COMMODITYViewModel x) => x.LookUpPROJECTS, x => x.PROJECTS); }
        }


        /// <summary>
        /// The view model for the COMMODITYESTIMATION_ITEMS detail collection.
        /// </summary>
        public CollectionViewModel<ESTIMATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> COMMODITYESTIMATION_ITEMSDetails
        {
            get { return GetDetailsCollectionViewModel((COMMODITYViewModel x) => x.COMMODITYESTIMATION_ITEMSDetails, x => x.ESTIMATION_ITEMS, x => x.GUID_COMMODITY, (x, key) => { x.GUID_COMMODITY = key; }); }
        }
    }
}
