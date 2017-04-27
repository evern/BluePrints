using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the SETTINGS_GLOBALS collection view model.
    /// </summary>
    public partial class SETTINGS_GLOBALSCollectionViewModel :
        CollectionViewModel<SETTINGS_GLOBAL, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of SETTINGS_GLOBALSCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static SETTINGS_GLOBALSCollectionViewModel Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new SETTINGS_GLOBALSCollectionViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the SETTINGS_GLOBALSCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the SETTINGS_GLOBALSCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected SETTINGS_GLOBALSCollectionViewModel(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(
                unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.SETTINGS_GLOBALS)
        {
        }
    }
}