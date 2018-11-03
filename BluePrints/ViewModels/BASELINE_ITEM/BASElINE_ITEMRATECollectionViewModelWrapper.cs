using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Data;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.ViewModels
{
    public class BASElINE_ITEMRATECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <BASELINE_ITEM, BASELINE_ITEMProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of BASElINE_ITEMRATECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static BASElINE_ITEMRATECollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new BASElINE_ITEMRATECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// Initializes a new instance of the BASElINE_ITEMRATECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the BASElINE_ITEMRATECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected BASElINE_ITEMRATECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        public PROJECT loadPROJECT { get; set; }
        protected override void resolveParameters(object parameter)
        {
            var receiveParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = receiveParameter.GetEntity();
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<DEPARTMENT, DEPARTMENT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DEPARTMENTS);
            loaderCollection.AddLoaderDescription<DISCIPLINE, DISCIPLINE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DISCIPLINES);
            loaderCollection.AddLoaderDescription<DOCTYPE, DOCTYPE, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.DOCTYPES);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.SUBJOBS, SUBJOBProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.WORKPACKS, WORKPACKProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.PHASES, PHASEProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.AREAS, AREAProjectionFunc);
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.RATES, RATEProjectionFunc);
        }

        protected virtual Func<IRepositoryQuery<SUBJOB>, IQueryable<SUBJOB>> SUBJOBProjectionFunc()
        {
             return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<WORKPACK>, IQueryable<WORKPACK>> WORKPACKProjectionFunc()
        {
            return query => query.Where(x => x.SUBJOB.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected Func<IRepositoryQuery<PHASE>, IQueryable<PHASE>> PHASEProjectionFunc()
        {
            return query => query.Where(x => x.PHASE_TYPE == PhaseType.Design);
        }

        protected virtual Func<IRepositoryQuery<AREA>, IQueryable<AREA>> AREAProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected virtual Func<IRepositoryQuery<RATE>, IQueryable<RATE>> RATEProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.BASELINE_ITEMS);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<BASELINE_ITEM>, IQueryable<BASELINE_ITEMProjection>> specifyMainViewModelProjection()
        {
            return query => BASELINE_ITEMProjectionQueries.IDeliverable_Rates_Transformation(query.Where(x => x.BASELINE.STATUS == BaselineStatus.Live && x.BASELINE.GUID_PROJECT == loadPROJECT.GUID), RATECollection);
        }

        protected override void OnAfterAssignedCallbackAndRaisePropertyChanged()
        {
            HideZeroCost = true;
            ExpandAllGroups();
            this.RaisePropertyChanged(x => x.HideZeroCost);
            base.OnAfterAssignedCallbackAndRaisePropertyChanged();
        }

        ObservableCollection<BASELINE_ITEMProjection> displayEntities;
        public override ObservableCollection<BASELINE_ITEMProjection> DisplayEntities
        {
            get
            {
                if (IsLoading || MainViewModel == null || MainViewModel.Entities == null)
                    return new ObservableCollection<BASELINE_ITEMProjection>();

                if(displayEntities == null)
                {
                    displayEntities = new ObservableCollection<BASELINE_ITEMProjection>();
                    foreach (var baseline_item in MainViewModel.Entities)
                    {
                        List<BASELINE_ITEMProjection> splitProjections = splitBASELINE_ITEMProjection(baseline_item);
                        foreach(BASELINE_ITEMProjection splitProjection in splitProjections)
                        {
                            displayEntities.Add(splitProjection);
                        }
                    }
                }

                return displayEntities;
            }
        }

        private List<BASELINE_ITEMProjection> splitBASELINE_ITEMProjection(BASELINE_ITEMProjection projection)
        {
            List<BASELINE_ITEMProjection> splitProjections = new List<BASELINE_ITEMProjection>();
            if (projection.RATE != null && projection.RATE.IsGangRateCalculatable)
            {
                foreach (RateRole rateRole in (RateRole[])Enum.GetValues(typeof(RateRole)))
                {
                    if (rateRole == RateRole.None)
                        continue;

                    splitProjections.Add(getBASELINE_ITEMRateBy(rateRole, projection));
                }
            }
            else
            {
                BASELINE_ITEMProjection newProjection = new BASELINE_ITEMProjection();
                DataUtils.ShallowCopy(newProjection, projection);
                newProjection.RateRole = RateRole.None;
                newProjection.SplitRate = projection.RATE == null || projection.RATE.RATE1 == null ? 0 : (decimal)projection.RATE.RATE1;
                splitProjections.Add(newProjection);
            }

            return splitProjections;
        }

        private BASELINE_ITEMProjection getBASELINE_ITEMRateBy(RateRole rateRole, BASELINE_ITEMProjection projection)
        {
            decimal splitRate = 0;
            decimal splitHours = 0;

            switch(rateRole)
            {
                case RateRole.Manager:
                    splitRate = projection.RATE.ManagerRate;
                    splitHours = projection.RATE.ManagerPercent * projection.Budget_Units;
                    break;
                case RateRole.Principal:
                    splitRate = projection.RATE.PrincipalRate;
                    splitHours = projection.RATE.PrincipalPercent * projection.Budget_Units;
                    break;
                case RateRole.Lead:
                    splitRate = projection.RATE.LeadRate;
                    splitHours = projection.RATE.LeadPercent * projection.Budget_Units;
                    break;
                case RateRole.Senior:
                    splitRate = projection.RATE.SeniorRate;
                    splitHours = projection.RATE.SeniorPercent * projection.Budget_Units;
                    break;
                case RateRole.Engineer:
                    splitRate = projection.RATE.EngineerRate;
                    splitHours = projection.RATE.EngineerPercent * projection.Budget_Units;
                    break;
                case RateRole.Graduate:
                    splitRate = projection.RATE.GraduateRate;
                    splitHours = projection.RATE.GraduatePercent * projection.Budget_Units;
                    break;
                case RateRole.Undergraduate:
                    splitRate = projection.RATE.UndergraduateRate;
                    splitHours = projection.RATE.UndergraduatePercent * projection.Budget_Units;
                    break;
            }

            BASELINE_ITEMProjection projectionCopy = new BASELINE_ITEMProjection();
            DataUtils.ShallowCopy(projectionCopy, projection);
            projectionCopy.RateRole = rateRole;
            projectionCopy.SplitRate = splitRate;
            projectionCopy.SplitHours = splitHours;
            return projectionCopy;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<BASELINE_ITEMProjection> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override string UnifiedValueValidation(BASELINE_ITEMProjection projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(BASELINE_ITEMProjection projection)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "BASElINE_ITEMRATECollectionViewModelWrapper"; }
        }

        public IEnumerable<DOCTYPE> BASELINE_ITEMCollection
        {
            get
            {
                var collection = GetEntities<DOCTYPE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.CODE);
                return collection;
            }
        }

        public IEnumerable<DEPARTMENT> DEPARTMENTCollection
        {
            get
            {
                var collection = GetEntities<DEPARTMENT>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }



        public IEnumerable<RATE> RATECollection
        {
            get
            {
                return GetEntities<RATE>();
            }
        }

        public IEnumerable<SUBJOB> SUBJOBCollection
        {
            get
            {
                var collection = GetEntities<SUBJOB>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NAME1);
                return collection;
            }
        }

        public IEnumerable<WORKPACK> WORKPACKCollection
        {
            get
            {
                var collection = GetEntities<WORKPACK>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        public IEnumerable<PHASE> PHASECollection
        {
            get
            {
                var collection = GetEntities<PHASE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }

        public IEnumerable<AREA> AREACollection
        {
            get
            {
                var collection = GetEntities<AREA>();
                if (collection != null)
                    collection = collection.Where(x => x.GUID_PARENT == null).OrderBy(x => x.INTERNAL_NUM);
                return collection;
            }
        }


        public IEnumerable<DISCIPLINE> DISCIPLINECollection
        {
            get
            {
                var collection = GetEntities<DISCIPLINE>();
                if (collection != null)
                    collection = collection.OrderBy(x => x.NAME);
                return collection;
            }
        }

        bool hideZeroCost;
        public bool HideZeroCost
        {
            get
            {
                return hideZeroCost;
            }
            set
            {
                hideZeroCost = value;
                string criteria = "[RoleCost] <> 0.0m";
                if (GridControlService != null)
                {
                    if (value)
                    {
                        CriteriaOperator criteriaOperator = GridControlService.GetFilterCriteria();
                        CriteriaOperator newCriteriaOperator;
                        if (!ReferenceEquals(criteriaOperator, null))
                        {
                            string filterCriteria = criteriaOperator.ToString() + " And " + criteria;
                            newCriteriaOperator = CriteriaOperator.Parse(filterCriteria);
                        }
                        else
                        {
                            newCriteriaOperator = CriteriaOperator.Parse(criteria);
                        }

                        GridControlService.SetFilterCriteria(newCriteriaOperator);
                    }
                    else
                    {
                        CriteriaOperator criteriaOperator = GridControlService.GetFilterCriteria();
                        if (!ReferenceEquals(criteriaOperator, null))
                        {
                            CriteriaOperator newCriteriaOperator;
                            string currentFilterCriteria = criteriaOperator.ToString();
                            string newfilterCriteria = currentFilterCriteria.Replace("And " + criteria, "");
                            newfilterCriteria = newfilterCriteria.Replace(criteria, "");
                            if (newfilterCriteria.Length >= 5)
                            {
                                string firstFiveChar = newfilterCriteria.Substring(0, 5);
                                if (firstFiveChar.ToUpper().Contains("AND"))
                                    newfilterCriteria = newfilterCriteria.Substring(5, newfilterCriteria.Length - 5);
                            }


                            newCriteriaOperator = CriteriaOperator.Parse(newfilterCriteria);
                            GridControlService.SetFilterCriteria(newCriteriaOperator);
                        }
                    }
                }
            }
        }
        #endregion
    }
}