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
    /// Represents the single DOCTYPE object view model.
    /// </summary>
    public partial class DOCTYPEViewModel : SingleObjectViewModel<DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of DOCTYPEViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static DOCTYPEViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new DOCTYPEViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the DOCTYPEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the DOCTYPEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected DOCTYPEViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.DOCTYPES, x => x.NAME)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of BASELINE_ITEMS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<BASELINE_ITEM> LookUpBASELINE_ITEMS
        {
            get { return GetLookUpEntitiesViewModel((DOCTYPEViewModel x) => x.LookUpBASELINE_ITEMS, x => x.BASELINE_ITEMS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of DEPARTMENTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DEPARTMENT> LookUpDEPARTMENTS
        {
            get { return GetLookUpEntitiesViewModel((DOCTYPEViewModel x) => x.LookUpDEPARTMENTS, x => x.DEPARTMENTS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of ESTIMATION_ITEMS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<ESTIMATION_ITEM> LookUpESTIMATION_ITEMS
        {
            get { return GetLookUpEntitiesViewModel((DOCTYPEViewModel x) => x.LookUpESTIMATION_ITEMS, x => x.ESTIMATION_ITEMS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of WORKPACKS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<WORKPACK> LookUpWORKPACKS
        {
            get { return GetLookUpEntitiesViewModel((DOCTYPEViewModel x) => x.LookUpWORKPACKS, x => x.WORKPACKS); }
        }


        /// <summary>
        /// The view model for the DOCTYPEBASELINE_ITEMS detail collection.
        /// </summary>
        public CollectionViewModel<BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> DOCTYPEBASELINE_ITEMSDetails
        {
            get { return GetDetailsCollectionViewModel((DOCTYPEViewModel x) => x.DOCTYPEBASELINE_ITEMSDetails, x => x.BASELINE_ITEMS, x => x.GUID_DOCTYPE, (x, key) => { x.GUID_DOCTYPE = key; }); }
        }

        /// <summary>
        /// The view model for the DOCTYPEWORKPACKS detail collection.
        /// </summary>
        public CollectionViewModel<WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> DOCTYPEWORKPACKSDetails
        {
            get { return GetDetailsCollectionViewModel((DOCTYPEViewModel x) => x.DOCTYPEWORKPACKSDetails, x => x.WORKPACKS, x => x.GUID_DDOCTYPE, (x, key) => { x.GUID_DDOCTYPE = key; }); }
        }
    }
}
