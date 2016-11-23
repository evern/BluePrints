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
    /// Represents the single WORKPACK_ASSIGNMENTS object view model.
    /// </summary>
    public partial class WORKPACK_ASSIGNMENTSViewModel : SingleObjectViewModel<WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of WORKPACK_ASSIGNMENTSViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static WORKPACK_ASSIGNMENTSViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new WORKPACK_ASSIGNMENTSViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the WORKPACK_ASSIGNMENTSViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the WORKPACK_ASSIGNMENTSViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected WORKPACK_ASSIGNMENTSViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.WORKPACK_ASSIGNMENTS, x => x.P6_ACTIVITYID)
        {
        }

        /// <summary>
		/// The view model that contains a look-up collection of WORKPACKS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<WORKPACK> LookUpWORKPACKS
        {
            get { return GetLookUpEntitiesViewModel((WORKPACK_ASSIGNMENTSViewModel x) => x.LookUpWORKPACKS, x => x.WORKPACKS); }
        }
    }
}
