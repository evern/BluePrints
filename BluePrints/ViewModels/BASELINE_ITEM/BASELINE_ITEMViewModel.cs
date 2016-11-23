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
using DevExpress.Xpf.Grid;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single BASELINE_ITEMS object view model.
    /// </summary>
    public partial class BASELINE_ITEMSViewModel : SingleObjectViewModel<BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of BASELINE_ITEMSViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static BASELINE_ITEMSViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new BASELINE_ITEMSViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the BASELINE_ITEMSViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASELINE_ITEMSViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected BASELINE_ITEMSViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.BASELINE_ITEMS, x => x.PRIMARY_TITLE)
        {
        }

        /// <summary>
		/// The view model that contains a look-up collection of AREAS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<AREA> LookUpAREAS
        {
            get { return GetLookUpEntitiesViewModel((BASELINE_ITEMSViewModel x) => x.LookUpAREAS, x => x.AREAS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of BASELINES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<BASELINE> LookUpBASELINES
        {
            get { return GetLookUpEntitiesViewModel((BASELINE_ITEMSViewModel x) => x.LookUpBASELINES, x => x.BASELINES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of DEPARTMENTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DEPARTMENT> LookUpDEPARTMENTS
        {
            get { return GetLookUpEntitiesViewModel((BASELINE_ITEMSViewModel x) => x.LookUpDEPARTMENTS, x => x.DEPARTMENTS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of DISCIPLINES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DISCIPLINE> LookUpDISCIPLINES
        {
            get { return GetLookUpEntitiesViewModel((BASELINE_ITEMSViewModel x) => x.LookUpDISCIPLINES, x => x.DISCIPLINES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of DOCTYPES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DOCTYPE> LookUpDOCTYPES
        {
            get { return GetLookUpEntitiesViewModel((BASELINE_ITEMSViewModel x) => x.LookUpDOCTYPES, x => x.DOCTYPES); }
        }

    }
}
