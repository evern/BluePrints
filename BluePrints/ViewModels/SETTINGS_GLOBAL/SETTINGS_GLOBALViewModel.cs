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
    /// Represents the single SETTINGS_GLOBALS object view model.
    /// </summary>
    public partial class SETTINGS_GLOBALSViewModel :
        SingleObjectViewModel<SETTINGS_GLOBAL, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of SETTINGS_GLOBALSViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static SETTINGS_GLOBALSViewModel Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new SETTINGS_GLOBALSViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the SETTINGS_GLOBALSViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the SETTINGS_GLOBALSViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected SETTINGS_GLOBALSViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(
                unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.SETTINGS_GLOBALS,
                x => x.REVIEW_PERCENTAGE)
        {
        }
    }
}