using BaseModel.DataModel;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Loader;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    public class PROJECTPLCollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <X_PL_SUMMARY, X_PL_SUMMARY, long, IPrimeroEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of PROJECTPLCollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static PROJECTPLCollectionViewModelWrapper Create(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new PROJECTPLCollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the PROJECTPLCollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTPLCollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected PROJECTPLCollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations

        private IUnitOfWorkFactory<IPrimeroEntitiesUnitOfWork> primeroUnitOfWorkFactory = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();

        InstantFeedbackActualDetailsCollectionViewModelWrapper instantFeedbackActualDetailViewModel = InstantFeedbackActualDetailsCollectionViewModelWrapper.Create();
        protected override void resolveParameters(object parameter)
        {
            instantFeedbackActualDetailViewModel.OnParameterChange(true);
            ActualFilterCriteria = CriteriaOperator.Parse("[MASTER_JOBCODE] = 'X'");
            this.RaisePropertyChanged(x => x.ActualFilterCriteria);
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork>(bluePrintsUnitOfWorkFactory, x => x.PROJECTS);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(primeroUnitOfWorkFactory, x => x.X_PL_SUMMARY);
        }

        protected override Func<IRepositoryQuery<X_PL_SUMMARY>, IQueryable<X_PL_SUMMARY>> specifyMainViewModelProjection()
        {
            return query => query;
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<X_PL_SUMMARY> entities)
        {
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }
        #endregion

        #region Saving Behavior
        public override string UnifiedRowValidation(X_PL_SUMMARY projection)
        {
            return string.Empty;
        }

        public override string UnifiedValueValidation(X_PL_SUMMARY projection, string field_name, object new_value, bool isPaste)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties
        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        public override string ViewName
        {
            get { return "PROJECTPLCollectionViewModelWrapper_v2"; }
        }

        public IEnumerable<PROJECT> PROJECTCollection
        {
            get
            {
                return GetEntities<PROJECT>();
            }
        }
        #endregion

        #region Filtering
        /// <summary>
        /// Because grid alternate between showing editor and focused row, use mousedown to invoke set filter
        /// </summary>
        public void MouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                TableView tableView = e.Source as TableView;
                if (tableView == null)
                    return;

                TableViewHitInfo hi = ((TableView)e.Source).CalcHitInfo(e.OriginalSource as DependencyObject);
                RowData clickRowData = tableView.FocusedRowData;

                if (clickRowData != null)
                    setFilter((X_PL_SUMMARY)clickRowData.Row, hi.Column);
            }
            catch (Exception ex)
            {
            }
        }

        public IListSource ActualsDetail => instantFeedbackActualDetailViewModel.InstantFeedbackEntities;
        //public List<X_PURCHORD_LINE_DETAIL> PODetail => X_PURCHORD_LINE_DETAILS;
        public Visibility ActualDetailsVisibility => !IsPoDetailsVisible ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PODetailsVisibility => IsPoDetailsVisible ? Visibility.Visible : Visibility.Collapsed;
        public bool IsPoDetailsVisible { get; set; }
        private bool isDetailBestFitApplied { get; set; }
        public CriteriaOperator ActualFilterCriteria { get; set; }
        public CriteriaOperator POFilterCriteria { get; set; }
        private void setFilter(X_PL_SUMMARY projectSummary, GridColumn gridColumn)
        {
            if (gridColumn == null || projectSummary == null)
                return;

            if (gridColumn.FieldName.ToUpper().Contains("TOTALTIMECOSTS"))
            {
                ActualFilterCriteria = CriteriaOperator.Parse("[MASTER_JOBCODE] = '" + projectSummary.JOBCODE + "' AND [TRANSTYPE] = 'T'");
                IsPoDetailsVisible = false;
                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }
            if (gridColumn.FieldName.ToUpper().Contains("TOTALMATERIALCOSTS"))
            {
                ActualFilterCriteria = CriteriaOperator.Parse("[MASTER_JOBCODE] = '" + projectSummary.JOBCODE + "' AND [TRANSTYPE] = 'C'");
                IsPoDetailsVisible = false;
                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("TOTALINVOICED"))
            {
                ActualFilterCriteria = CriteriaOperator.Parse("[MASTER_JOBCODE] = '" + projectSummary.JOBCODE + "' AND [INVOICED] > 0.0m");

                IsPoDetailsVisible = false;
                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }
            //else if (gridColumn.FieldName.ToUpper().Contains("OUTSTANDING"))
            //{
            //    if (entity.CommodityCode != string.Empty)
            //        POFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "' And [COMMODITY_CODE] = '" + entity.CommodityCode + "'");
            //    else
            //        POFilterCriteria = CriteriaOperator.Parse("[SUB_JOBCODE] = '" + entity.SubJobCode + "' And [DISCIPLINE_CODE] = '" + entity.DisciplineCode + "' And [VARIATION_CODE] = '" + entity.VariationCode + "'");

            //    IsPoDetailsVisible = true;
            //    this.RaisePropertyChanged(x => x.PODetail);
            //    this.RaisePropertyChanged(x => x.POFilterCriteria);
            //}

            this.RaisePropertyChanged(x => x.ActualDetailsVisibility);
            this.RaisePropertyChanged(x => x.PODetailsVisibility);
        }

        #endregion
    }
}