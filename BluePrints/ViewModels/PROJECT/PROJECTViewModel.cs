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
using BluePrints.Common.ViewModel.Filtering;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the single PROJECT object view model.
    /// </summary>
    public partial class PROJECTViewModel : SingleObjectViewModel<PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECTViewModel as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTViewModel Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTViewModel(unitOfWorkFactory));
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTViewModel(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
            : base(unitOfWorkFactory ?? BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory(), x => x.PROJECTS, x => x.NUMBER)
        {
        }

        /// <summary>
		/// The view model that contains a look-up collection of AREAS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<AREA> LookUpAREAS
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpAREAS, x => x.AREAS); }
        }

        /// <summary>
        /// The view model that contains a look-up collection of DEPARTMENTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DEPARTMENT> LookUpDEPARTMENTS
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpDEPARTMENTS, x => x.DEPARTMENTS); }
        }

        /// <summary>
        /// The view model that contains a look-up collection of DISCIPLINES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DISCIPLINE> LookUpDISCIPLINES
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpDISCIPLINES, x => x.DISCIPLINES); }
        }

        /// <summary>
        /// The view model that contains a look-up collection of DOCTYPES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<DOCTYPE> LookUpDOCTYPES
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpDOCTYPES, x => x.DOCTYPES); }
        }

        /// <summary>
		/// The view model that contains a look-up collection of BASELINES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<BASELINE> LookUpBASELINES
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpBASELINES, x => x.BASELINES); }
        }

        /// <summary>
		/// The view model that contains a look-up collection of COMMODITIES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<COMMODITY> LookUpCOMMODITIES
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpCOMMODITIES, x => x.COMMODITIES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of ESTIMATIONS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<ESTIMATION> LookUpESTIMATIONS
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpESTIMATIONS, x => x.ESTIMATIONS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of PHASES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<PHASE> LookUpPHASES
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpPHASES, x => x.PHASES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of PROGRESSES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<PROGRESS> LookUpPROGRESSES
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpPROGRESSES, x => x.PROGRESSES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of PROJECT_REPORTS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<PROJECT_REPORT> LookUpPROJECT_REPORTS
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpPROJECT_REPORTS, x => x.PROJECT_REPORTS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of RATES for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<RATE> LookUpRATES
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpRATES, x => x.RATES); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of REGISTERS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<REGISTER> LookUpREGISTERS
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpREGISTERS, x => x.REGISTERS); }
        }
        /// <summary>
		/// The view model that contains a look-up collection of VARIATIONS for the corresponding navigation property in the view.
        /// </summary>
        public IEntitiesViewModel<VARIATION> LookUpVARIATIONS
        {
            get { return GetLookUpEntitiesViewModel((PROJECTViewModel x) => x.LookUpVARIATIONS, x => x.VARIATIONS); }
        }


        /// <summary>
        /// The view model for the PROJECTAREAS detail collection.
        /// </summary>
        public CollectionViewModel<AREA, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTAREASDetails
        {
            get { return GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTAREASDetails, x => x.AREAS, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; }); }
        }

        /// <summary>
        /// The view model for the PROJECTBASELINES detail collection.
        /// </summary>
        public CollectionViewModel<BASELINE, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTBASELINESDetails
        {
            get { return GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTBASELINESDetails, x => x.BASELINES, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; }); }
        }

        /// <summary>
        /// The view model for the PROJECTCOMMODITIES detail collection.
        /// </summary>
        public CollectionViewModel<COMMODITY, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTCOMMODITIESDetails
        {
            get { return GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTCOMMODITIESDetails, x => x.COMMODITIES, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; }); }
        }

        /// <summary>
        /// The view model for the PROJECTESTIMATIONS detail collection.
        /// </summary>
        public CollectionViewModel<ESTIMATION, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTESTIMATIONSDetails
        {
            get { return GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTESTIMATIONSDetails, x => x.ESTIMATIONS, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; }); }
        }

        /// <summary>
        /// The view model for the PROJECTPHASES detail collection.
        /// </summary>
        public CollectionViewModel<PHASE, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTPHASESDetails
        {
            get { return GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTPHASESDetails, x => x.PHASES, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; }); }
        }

        /// <summary>
        /// The view model for the PROJECTPROGRESSES detail collection.
        /// </summary>
        public CollectionViewModel<PROGRESS, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTPROGRESSESDetails
        {
            get
            {
                var collectionViewModel = GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTPROGRESSESDetails, x => x.PROGRESSES, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; });
                collectionViewModel.OnBeforeEntitySavedCallBack = this.PROJECTPROGRESSES_OnBeforeEntitySaved;
                return collectionViewModel;
            }
        }

        /// <summary>
        /// CallBack to apply global convention
        /// </summary>
        public void PROJECTPROGRESSES_OnBeforeEntitySaved(PROGRESS entity)
        {
            entity.DATA_DATE = entity.DATA_DATE.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        /// <summary>
        /// The view model for the PROJECTREGISTERS detail collection.
        /// </summary>
        public CollectionViewModel<REGISTER, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTREGISTERSDetails
        {
            get { return GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTREGISTERSDetails, x => x.REGISTERS, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; }); }
        }

        /// <summary>
        /// The view model for the PROJECTPROJECT_REPORTS detail collection.
        /// </summary>
        public CollectionViewModel<PROJECT_REPORT, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTPROJECT_REPORTSDetails
        {
            get { return GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTPROJECT_REPORTSDetails, x => x.PROJECT_REPORTS, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; }); }
        }

        /// <summary>
        /// The view model for the PROJECTRATES detail collection.
        /// </summary>
        public CollectionViewModel<RATE, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTRATESDetails
        {
            get { return GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTRATESDetails, x => x.RATES, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; }); }
        }

        /// <summary>
        /// The view model for the PROJECTVARIATIONS detail collection.
        /// </summary>
        public CollectionViewModel<VARIATION, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTVARIATIONSDetails
        {
            get { return GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTVARIATIONSDetails, x => x.VARIATIONS, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; }); }
        }

        /// <summary>
        /// The view model for the PROJECTWORKPACKS detail collection.
        /// </summary>
        public CollectionViewModel<WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTWORKPACKSDetails
        {
            get { return GetDetailsCollectionViewModel((PROJECTViewModel x) => x.PROJECTWORKPACKSDetails, x => x.WORKPACKS, x => x.GUID_PROJECT, (x, key) => { x.GUID_PROJECT = key; }); }
        }
    }
}
