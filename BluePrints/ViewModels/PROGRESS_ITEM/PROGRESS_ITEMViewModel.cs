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
    /// Represents the single PROGRESS_ITEMS object view model.
    /// </summary>
    public partial class PROGRESS_ITEMSViewModel : SingleObjectViewModel<PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of PROGRESS_ITEMSViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROGRESS_ITEMSViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROGRESS_ITEMSViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROGRESS_ITEMSViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROGRESS_ITEMSViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROGRESS_ITEMSViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.PROGRESS_ITEMS, x => x.EARNED_UNITS)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of PROGRESSES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<PROGRESS> LookUpPROGRESSES
        {
            get { return GetLookUpEntitiesViewModel((PROGRESS_ITEMSViewModel x) => x.LookUpPROGRESSES, x => x.PROGRESSES); }
        }

    }
}
