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
    /// Represents the single ROLE object view model.
    /// </summary>
    public partial class ROLEViewModel : SingleObjectViewModel<ROLE, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of ROLEViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ROLEViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ROLEViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the ROLEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the ROLEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ROLEViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.ROLES, x => x.NAME)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of ROLE_PERMISSIONS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<ROLE_PERMISSION> LookUpROLE_PERMISSIONS
        {
            get { return GetLookUpEntitiesViewModel((ROLEViewModel x) => x.LookUpROLE_PERMISSIONS, x => x.ROLE_PERMISSIONS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of USERS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<USER> LookUpUSERS
        {
            get { return GetLookUpEntitiesViewModel((ROLEViewModel x) => x.LookUpUSERS, x => x.USERS); }
        }


        /// <summary>
        /// The view model for the ROLEROLE_PERMISSIONS detail collection.
        /// </summary>
        public CollectionViewModel<ROLE_PERMISSION, Guid, IBluePrintsEntitiesUnitOfWork> ROLEROLE_PERMISSIONSDetails
        {
            get { return GetDetailsCollectionViewModel((ROLEViewModel x) => x.ROLEROLE_PERMISSIONSDetails, x => x.ROLE_PERMISSIONS, x => x.GUID_ROLE, (x, key) => { x.GUID_ROLE = key; }); }
        }

        /// <summary>
        /// The view model for the ROLEUSERS detail collection.
        /// </summary>
        public CollectionViewModel<USER, Guid, IBluePrintsEntitiesUnitOfWork> ROLEUSERSDetails
        {
            get { return GetDetailsCollectionViewModel((ROLEViewModel x) => x.ROLEUSERSDetails, x => x.USERS, x => x.GUID_ROLE, (x, key) => { x.GUID_ROLE = key; }); }
        }
    }
}
