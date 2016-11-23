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
    /// Represents the single ESTIMATION_ITEMS object view model.
    /// </summary>
    public partial class ESTIMATION_ITEMSViewModel : SingleObjectViewModel<ESTIMATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of ESTIMATION_ITEMSViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static ESTIMATION_ITEMSViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new ESTIMATION_ITEMSViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the ESTIMATION_ITEMSViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the ESTIMATION_ITEMSViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected ESTIMATION_ITEMSViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.ESTIMATION_ITEMS, x => x.COMMENTS)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of AREAS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<AREA> LookUpAREAS
        {
            get { return GetLookUpEntitiesViewModel((ESTIMATION_ITEMSViewModel x) => x.LookUpAREAS, x => x.AREAS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of COMMODITIES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<COMMODITY> LookUpCOMMODITIES
        {
            get { return GetLookUpEntitiesViewModel((ESTIMATION_ITEMSViewModel x) => x.LookUpCOMMODITIES, x => x.COMMODITIES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of DISCIPLINES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DISCIPLINE> LookUpDISCIPLINES
        {
            get { return GetLookUpEntitiesViewModel((ESTIMATION_ITEMSViewModel x) => x.LookUpDISCIPLINES, x => x.DISCIPLINES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of DOCTYPES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DOCTYPE> LookUpDOCTYPES
        {
            get { return GetLookUpEntitiesViewModel((ESTIMATION_ITEMSViewModel x) => x.LookUpDOCTYPES, x => x.DOCTYPES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of ESTIMATIONS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<ESTIMATION> LookUpESTIMATIONS
        {
            get { return GetLookUpEntitiesViewModel((ESTIMATION_ITEMSViewModel x) => x.LookUpESTIMATIONS, x => x.ESTIMATIONS); }
        }

    }
}
