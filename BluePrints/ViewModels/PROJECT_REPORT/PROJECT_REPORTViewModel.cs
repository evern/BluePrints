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
    /// Represents the single PROJECT_REPORTS object view model.
    /// </summary>
    public partial class PROJECT_REPORTSViewModel : SingleObjectViewModel<PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of PROJECT_REPORTSViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECT_REPORTSViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECT_REPORTSViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECT_REPORTSViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECT_REPORTSViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECT_REPORTSViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.PROJECT_REPORTS, x => x.REPORT_TYPE)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of PROJECTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<PROJECT> LookUpPROJECTS
        {
            get { return GetLookUpEntitiesViewModel((PROJECT_REPORTSViewModel x) => x.LookUpPROJECTS, x => x.PROJECTS); }
        }

    }
}
