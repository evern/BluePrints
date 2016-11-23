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
    /// Represents the single WORKPACK object view model.
    /// </summary>
    public partial class WORKPACKViewModel : SingleObjectViewModel<WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of WORKPACKViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static WORKPACKViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new WORKPACKViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the WORKPACKViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the WORKPACKViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected WORKPACKViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.WORKPACKS, x => x.INTERNAL_NAME1)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of AREAS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<AREA> LookUpAREAS
        {
            get { return GetLookUpEntitiesViewModel((WORKPACKViewModel x) => x.LookUpAREAS, x => x.AREAS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of DEPARTMENTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DEPARTMENT> LookUpDEPARTMENTS
        {
            get { return GetLookUpEntitiesViewModel((WORKPACKViewModel x) => x.LookUpDEPARTMENTS, x => x.DEPARTMENTS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of DISCIPLINES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DISCIPLINE> LookUpDISCIPLINES
        {
            get { return GetLookUpEntitiesViewModel((WORKPACKViewModel x) => x.LookUpDISCIPLINES, x => x.DISCIPLINES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of DOCTYPES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DOCTYPE> LookUpDOCTYPES
        {
            get { return GetLookUpEntitiesViewModel((WORKPACKViewModel x) => x.LookUpDOCTYPES, x => x.DOCTYPES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of WORKPACK_ASSIGNMENTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<WORKPACK_ASSIGNMENT> LookUpWORKPACK_ASSIGNMENTS
        {
            get { return GetLookUpEntitiesViewModel((WORKPACKViewModel x) => x.LookUpWORKPACK_ASSIGNMENTS, x => x.WORKPACK_ASSIGNMENTS); }
        }


        /// <summary>
        /// The view model for the WORKPACKWORKPACK_ASSIGNMENTS detail collection.
        /// </summary>
        public CollectionViewModel<WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACKWORKPACK_ASSIGNMENTSDetails
        {
            get { return GetDetailsCollectionViewModel((WORKPACKViewModel x) => x.WORKPACKWORKPACK_ASSIGNMENTSDetails, x => x.WORKPACK_ASSIGNMENTS, x => x.GUID_WORKPACK, (x, key) => { x.GUID_WORKPACK = key; }); }
        }
    }
}
