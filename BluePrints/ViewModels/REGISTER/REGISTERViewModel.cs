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
    /// Represents the single REGISTER object view model.
    /// </summary>
    public partial class REGISTERViewModel : SingleObjectViewModel<REGISTER, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of REGISTERViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static REGISTERViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new REGISTERViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the REGISTERViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the REGISTERViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected REGISTERViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.REGISTERS, x => x.TITLE)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of PROJECTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<PROJECT> LookUpPROJECTS
        {
            get { return GetLookUpEntitiesViewModel((REGISTERViewModel x) => x.LookUpPROJECTS, x => x.PROJECTS); }
        }

    }
}
