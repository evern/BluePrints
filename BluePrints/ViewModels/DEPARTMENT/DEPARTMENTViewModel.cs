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
    /// Represents the single DEPARTMENT object view model.
    /// </summary>
    public partial class DEPARTMENTViewModel : SingleObjectViewModel<DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of DEPARTMENTViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static DEPARTMENTViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new DEPARTMENTViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the DEPARTMENTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the DEPARTMENTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected DEPARTMENTViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.DEPARTMENTS, x => x.NAME)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of BASELINE_ITEMS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<BASELINE_ITEM> LookUpBASELINE_ITEMS
        {
            get { return GetLookUpEntitiesViewModel((DEPARTMENTViewModel x) => x.LookUpBASELINE_ITEMS, x => x.BASELINE_ITEMS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of DEPARTMENTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DEPARTMENT> LookUpDEPARTMENTS
        {
            get { return GetLookUpEntitiesViewModel((DEPARTMENTViewModel x) => x.LookUpDEPARTMENTS, x => x.DEPARTMENTS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of DOCTYPES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DOCTYPE> LookUpDOCTYPES
        {
            get { return GetLookUpEntitiesViewModel((DEPARTMENTViewModel x) => x.LookUpDOCTYPES, x => x.DOCTYPES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of RATES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<RATE> LookUpRATES
        {
            get { return GetLookUpEntitiesViewModel((DEPARTMENTViewModel x) => x.LookUpRATES, x => x.RATES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of WORKPACKS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<WORKPACK> LookUpWORKPACKS
        {
            get { return GetLookUpEntitiesViewModel((DEPARTMENTViewModel x) => x.LookUpWORKPACKS, x => x.WORKPACKS); }
        }

        /// <summary>
        /// The view model for the DEPARTMENTBASELINE_ITEMS detail collection.
        /// </summary>
        public CollectionViewModel<BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> DEPARTMENTBASELINE_ITEMSDetails
        {
            get { return GetDetailsCollectionViewModel((DEPARTMENTViewModel x) => x.DEPARTMENTBASELINE_ITEMSDetails, x => x.BASELINE_ITEMS, x => x.GUID_DEPARTMENT, (x, key) => { x.GUID_DEPARTMENT = key; }); }
        }

        /// <summary>
        /// The view model for the DEPARTMENTDOCTYPES detail collection.
        /// </summary>
        public CollectionViewModel<DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork> DEPARTMENTDOCTYPESDetails
        {
            get { return GetDetailsCollectionViewModel((DEPARTMENTViewModel x) => x.DEPARTMENTDOCTYPESDetails, x => x.DOCTYPES, x => x.GUID_DDEPARTMENT, (x, key) => { x.GUID_DDEPARTMENT = key; }); }
        }

        /// <summary>
        /// The view model for the DEPARTMENTRATES detail collection.
        /// </summary>
        public CollectionViewModel<RATE, Guid, IBluePrintsEntitiesUnitOfWork> DEPARTMENTRATESDetails
        {
            get { return GetDetailsCollectionViewModel((DEPARTMENTViewModel x) => x.DEPARTMENTRATESDetails, x => x.RATES, x => x.GUID_DEPARTMENT, (x, key) => { x.GUID_DEPARTMENT = key; }); }
        }

        /// <summary>
        /// The view model for the DEPARTMENTWORKPACKS detail collection.
        /// </summary>
        public CollectionViewModel<WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> DEPARTMENTWORKPACKSDetails
        {
            get { return GetDetailsCollectionViewModel((DEPARTMENTViewModel x) => x.DEPARTMENTWORKPACKSDetails, x => x.WORKPACKS, x => x.GUID_DDEPARTMENT, (x, key) => { x.GUID_DDEPARTMENT = key; }); }
        }
    }
}
