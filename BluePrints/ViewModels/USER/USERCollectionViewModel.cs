using System;
using System.Linq;
using DevExpress.Mvvm.POCO;
using BluePrints.Common.Utils;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Data;
using BluePrints.Common.ViewModel;
using DevExpress.Mvvm;
using System.Collections.Generic;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the USERS collection view model.
    /// </summary>
    public partial class USERCollectionViewModel : CollectionViewModel<USER, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of USERCollectionViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static USERCollectionViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new USERCollectionViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the USERCollectionViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the USERCollectionViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected USERCollectionViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.USERS)
        {
        }

        IDialogService USERImportDialogService { get { return this.GetRequiredService<IDialogService>("USERImportDialogService"); } }

        public void Import()
        {
            var selectEntitiesViewModel = SelectDetailUSERSViewModel.Create(this.GetExistingUSERS);
            if (USERImportDialogService.ShowDialog(MessageButton.OKCancel, "Select Users to Import", "SelectDetailUSERSView", selectEntitiesViewModel) == MessageResult.OK)
            {
                this.BulkSave(selectEntitiesViewModel.SelectedEntities);
            }

            selectEntitiesViewModel = null;
        }

        /// <summary>
        /// The view model that contains a look-up collection of ROLES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<ROLE> LookUpROLES
        {
            get { return GetLookUpEntitiesViewModel((ROLE_PERMISSIONSViewModel x) => x.LookUpROLES, x => x.ROLES); }
        }

        public IEnumerable<USER> GetExistingUSERS()
        {
            return this.Entities;
        }
    }
}