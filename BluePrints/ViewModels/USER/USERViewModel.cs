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
    /// Represents the single USER object view model.
    /// </summary>
    public partial class USERViewModel : SingleObjectViewModel<USER, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of USERViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static USERViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new USERViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the USERViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the USERViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected USERViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.USERS, x => x.NAME)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of ROLES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<ROLE> LookUpROLES
        {
            get { return GetLookUpEntitiesViewModel((USERViewModel x) => x.LookUpROLES, x => x.ROLES); }
        }

    }
}
