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
    /// Represents the single AREA object view model.
    /// </summary>
    public partial class AREAViewModel : SingleObjectViewModel<AREA, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of AREAViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static AREAViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new AREAViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the AREAViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the AREAViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected AREAViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.AREAS, x => x.TITLE)
        {
        }



        /// <summary>
		/// The view model that contains a look-up collection of BASELINE_ITEMS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<BASELINE_ITEM> LookUpBASELINE_ITEMS
        {
            get { return GetLookUpEntitiesViewModel((AREAViewModel x) => x.LookUpBASELINE_ITEMS, x => x.BASELINE_ITEMS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of ESTIMATION_ITEMS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<ESTIMATION_ITEM> LookUpESTIMATION_ITEMS
        {
            get { return GetLookUpEntitiesViewModel((AREAViewModel x) => x.LookUpESTIMATION_ITEMS, x => x.ESTIMATION_ITEMS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of PROJECTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<PROJECT> LookUpPROJECTS
        {
            get { return GetLookUpEntitiesViewModel((AREAViewModel x) => x.LookUpPROJECTS, x => x.PROJECTS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of WORKPACKS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<WORKPACK> LookUpWORKPACKS
        {
            get { return GetLookUpEntitiesViewModel((AREAViewModel x) => x.LookUpWORKPACKS, x => x.WORKPACKS); }
        }


        /// <summary>
        /// The view model for the AREABASELINE_ITEMS detail collection.
        /// </summary>
        public CollectionViewModel<BASELINE_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> AREABASELINE_ITEMSDetails
        {
            get { return GetDetailsCollectionViewModel((AREAViewModel x) => x.AREABASELINE_ITEMSDetails, x => x.BASELINE_ITEMS, x => x.GUID_AREA, (x, key) => { x.GUID_AREA = key; }); }
        }

        /// <summary>
        /// The view model for the AREAESTIMATION_ITEMS detail collection.
        /// </summary>
        public CollectionViewModel<ESTIMATION_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> AREAESTIMATION_ITEMSDetails
        {
            get { return GetDetailsCollectionViewModel((AREAViewModel x) => x.AREAESTIMATION_ITEMSDetails, x => x.ESTIMATION_ITEMS, x => x.GUID_AREA, (x, key) => { x.GUID_AREA = key; }); }
        }

        /// <summary>
        /// The view model for the AREAWORKPACKS detail collection.
        /// </summary>
        public CollectionViewModel<WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> AREAWORKPACKSDetails
        {
            get { return GetDetailsCollectionViewModel((AREAViewModel x) => x.AREAWORKPACKSDetails, x => x.WORKPACKS, x => x.GUID_DAREA, (x, key) => { x.GUID_DAREA = key; }); }
        }
    }
}
