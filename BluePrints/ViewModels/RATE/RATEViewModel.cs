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
    /// Represents the single RATE object view model.
    /// </summary>
    public partial class RATEViewModel : SingleObjectViewModel<RATE, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of RATEViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static RATEViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new RATEViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the RATEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the RATEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected RATEViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.RATES, x => x.DEPARTMENT)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of DEPARTMENTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DEPARTMENT> LookUpDEPARTMENTS
        {
            get { return GetLookUpEntitiesViewModel((RATEViewModel x) => x.LookUpDEPARTMENTS, x => x.DEPARTMENTS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of DISCIPLINES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DISCIPLINE> LookUpDISCIPLINES
        {
            get { return GetLookUpEntitiesViewModel((RATEViewModel x) => x.LookUpDISCIPLINES, x => x.DISCIPLINES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of PROJECTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<PROJECT> LookUpPROJECTS
        {
            get { return GetLookUpEntitiesViewModel((RATEViewModel x) => x.LookUpPROJECTS, x => x.PROJECTS); }
        }

    }
}
