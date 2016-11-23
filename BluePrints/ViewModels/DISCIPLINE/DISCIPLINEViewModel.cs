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
    /// Represents the single DISCIPLINE object view model.
    /// </summary>
    public partial class DISCIPLINEViewModel : SingleObjectViewModel<DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>
    {

        /// <summary>
        /// Creates a new instance of DISCIPLINEViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static DISCIPLINEViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new DISCIPLINEViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the DISCIPLINEViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the DISCIPLINEViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected DISCIPLINEViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.DISCIPLINES, x => x.NAME)
        {
        }

        /// <summary>
		/// The view model that contains a look-up collection of COMMODITY_CODES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<COMMODITY_CODE> LookUpCOMMODITY_CODES
        {
            get { return GetLookUpEntitiesViewModel((DISCIPLINEViewModel x) => x.LookUpCOMMODITY_CODES, x => x.COMMODITY_CODES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of ESTIMATION_ITEMS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<ESTIMATION_ITEM> LookUpESTIMATION_ITEMS
        {
            get { return GetLookUpEntitiesViewModel((DISCIPLINEViewModel x) => x.LookUpESTIMATION_ITEMS, x => x.ESTIMATION_ITEMS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of RATES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<RATE> LookUpRATES
        {
            get { return GetLookUpEntitiesViewModel((DISCIPLINEViewModel x) => x.LookUpRATES, x => x.RATES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of WORKPACKS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<WORKPACK> LookUpWORKPACKS
        {
            get { return GetLookUpEntitiesViewModel((DISCIPLINEViewModel x) => x.LookUpWORKPACKS, x => x.WORKPACKS); }
        }


        /// <summary>
        /// The view model for the DISCIPLINEBASELINE_ITEMS detail collection.
        /// </summary>
        public CollectionViewModel<BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> DISCIPLINEBASELINE_ITEMSDetails
        {
            get { return GetDetailsCollectionViewModel((DISCIPLINEViewModel x) => x.DISCIPLINEBASELINE_ITEMSDetails, x => x.BASELINE_ITEMS, x => x.GUID_DISCIPLINE, (x, key) => { x.GUID_DISCIPLINE = key; }); }
        }

        /// <summary>
        /// The view model for the DISCIPLINECOMMODITY_CODES detail collection.
        /// </summary>
        public CollectionViewModel<COMMODITY_CODE, Guid, IBluePrintsEntitiesUnitOfWork> DISCIPLINECOMMODITY_CODESDetails
        {
            get { return GetDetailsCollectionViewModel((DISCIPLINEViewModel x) => x.DISCIPLINECOMMODITY_CODESDetails, x => x.COMMODITY_CODES, x => x.GUID_DISCIPLINE, (x, key) => { x.GUID_DISCIPLINE = key; }); }
        }

        /// <summary>
        /// The view model for the DISCIPLINEESTIMATION_ITEMS detail collection.
        /// </summary>
        public CollectionViewModel<ESTIMATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> DISCIPLINEESTIMATION_ITEMSDetails
        {
            get { return GetDetailsCollectionViewModel((DISCIPLINEViewModel x) => x.DISCIPLINEESTIMATION_ITEMSDetails, x => x.ESTIMATION_ITEMS, x => x.GUID_DISCIPLINE, (x, key) => { x.GUID_DISCIPLINE = key; }); }
        }

        /// <summary>
        /// The view model for the DISCIPLINERATES detail collection.
        /// </summary>
        public CollectionViewModel<RATE, Guid, IBluePrintsEntitiesUnitOfWork> DISCIPLINERATESDetails
        {
            get { return GetDetailsCollectionViewModel((DISCIPLINEViewModel x) => x.DISCIPLINERATESDetails, x => x.RATES, x => x.GUID_DISCIPLINE, (x, key) => { x.GUID_DISCIPLINE = key; }); }
        }

        /// <summary>
        /// The view model for the DISCIPLINEWORKPACKS detail collection.
        /// </summary>
        public CollectionViewModel<WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> DISCIPLINEWORKPACKSDetails
        {
            get { return GetDetailsCollectionViewModel((DISCIPLINEViewModel x) => x.DISCIPLINEWORKPACKSDetails, x => x.WORKPACKS, x => x.GUID_DDISCIPLINE, (x, key) => { x.GUID_DDISCIPLINE = key; }); }
        }
    }
}
