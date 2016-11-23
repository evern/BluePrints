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
    /// Represents the single ROLE_PERMISSIONS object view model.
    /// </summary>
    public partial class ROLE_PERMISSIONSViewModel : SingleObjectViewModel<ROLE_PERMISSION, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of ROLE_PERMISSIONSViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ROLE_PERMISSIONSViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ROLE_PERMISSIONSViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the ROLE_PERMISSIONSViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the ROLE_PERMISSIONSViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ROLE_PERMISSIONSViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.ROLE_PERMISSIONS, x => x.PERMISSION)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of ROLES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<ROLE> LookUpROLES
        {
            get { return GetLookUpEntitiesViewModel((ROLE_PERMISSIONSViewModel x) => x.LookUpROLES, x => x.ROLES); }
        }
    }
}
