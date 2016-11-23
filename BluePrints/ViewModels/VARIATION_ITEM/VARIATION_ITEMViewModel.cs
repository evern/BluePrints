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
    /// Represents the single VARIATION_ITEMS object view model.
    /// </summary>
    public partial class VARIATION_ITEMSViewModel : SingleObjectViewModel<VARIATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of VARIATION_ITEMSViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static VARIATION_ITEMSViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new VARIATION_ITEMSViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the VARIATION_ITEMSViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the VARIATION_ITEMSViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected VARIATION_ITEMSViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.VARIATION_ITEMS, x => x.VARIATION_UNITS)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of VARIATIONS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<VARIATION> LookUpVARIATIONS
        {
            get { return GetLookUpEntitiesViewModel((VARIATION_ITEMSViewModel x) => x.LookUpVARIATIONS, x => x.VARIATIONS); }
        }

    }
}
