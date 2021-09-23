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
        <X_PL_SUMMARY, X_PL_SUMMARY, int, IPrimeroEntitiesUnitOfWork>
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
        InstantFeedbackPODetailsCollectionViewModelWrapper instantFeedbackPODetailViewModel = InstantFeedbackPODetailsCollectionViewModelWrapper.Create();
        InstantFeedbackInvoicedCollectionViewModelWrapper InstantFeedbackInvoicedViewModel = InstantFeedbackInvoicedCollectionViewModelWrapper.Create();
        protected override void resolveParameters(object parameter)
        {
            instantFeedbackActualDetailViewModel.OnParameterChange(true);
            instantFeedbackPODetailViewModel.OnParameterChange(true);
            InstantFeedbackInvoicedViewModel.OnParameterChange(true);
            ActualFilterCriteria = CriteriaOperator.Parse("[MASTER_JOBCODE] = 'X'");
            POFilterCriteria = CriteriaOperator.Parse("[MASTER_JOBCODE] = 'X'");
            InvoicedFilterCriteria = CriteriaOperator.Parse("[MASTER_JOBCODE] = 'X'");
            IsActualDetailsVisible = true;
            this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            this.RaisePropertyChanged(x => x.POFilterCriteria);
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
        public IListSource PODetail => instantFeedbackPODetailViewModel.InstantFeedbackEntities;
        public IListSource InvoiceDetail => InstantFeedbackInvoicedViewModel.InstantFeedbackEntities;
        public Visibility ActualDetailsVisibility => IsActualDetailsVisible ? Visibility.Visible : Visibility.Collapsed;
        public Visibility PODetailsVisibility => IsPoDetailsVisible ? Visibility.Visible : Visibility.Collapsed;
        public Visibility InvoiceDetailsVisibility => IsInvoiceDetailsVisible ? Visibility.Visible : Visibility.Collapsed;
        public int DateSortIndex => 1;
        public bool IsPoDetailsVisible { get; set; }
        public bool IsActualDetailsVisible { get; set; }
        public bool IsInvoiceDetailsVisible { get; set; }
        public CriteriaOperator ActualFilterCriteria { get; set; }
        public CriteriaOperator POFilterCriteria { get; set; }
        public CriteriaOperator InvoicedFilterCriteria { get; set; }
        private void setFilter(X_PL_SUMMARY projectSummary, GridColumn gridColumn)
        {
            if (gridColumn == null || projectSummary == null)
                return;

            if (gridColumn.FieldName.ToUpper().Contains("TOTALTIMECOSTS"))
            {
                ActualFilterCriteria = CriteriaOperator.Parse("[MASTER_JOBCODE] = '" + projectSummary.JOBCODE + "' AND [TRANSTYPE] = 'T'");
                IsActualDetailsVisible = true;
                IsPoDetailsVisible = false;
                IsInvoiceDetailsVisible = false;
                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }
            if (gridColumn.FieldName.ToUpper().Contains("TOTALMATERIALCOSTS"))
            {
                ActualFilterCriteria = CriteriaOperator.Parse("[MASTER_JOBCODE] = '" + projectSummary.JOBCODE + "' AND [TRANSTYPE] = 'C'");
                IsActualDetailsVisible = true;
                IsPoDetailsVisible = false;
                IsInvoiceDetailsVisible = false;
                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("TOTALCOSTS"))
            {
                ActualFilterCriteria = CriteriaOperator.Parse("[MASTER_JOBCODE] = '" + projectSummary.JOBCODE + "'");
                IsActualDetailsVisible = true;
                IsPoDetailsVisible = false;
                IsInvoiceDetailsVisible = false;
                this.RaisePropertyChanged(x => x.ActualsDetail);
                this.RaisePropertyChanged(x => x.ActualFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("TOTALINVOICED"))
            {
                InvoicedFilterCriteria = CriteriaOperator.Parse("[MASTER_JOBCODE] = '" + projectSummary.JOBCODE + "'");
                IsActualDetailsVisible = false;
                IsPoDetailsVisible = false;
                IsInvoiceDetailsVisible = true;
                this.RaisePropertyChanged(x => x.InvoiceDetail);
                this.RaisePropertyChanged(x => x.InvoicedFilterCriteria);
            }
            else if (gridColumn.FieldName.ToUpper().Contains("TOTALOUTSTANDING"))
            {
                POFilterCriteria = CriteriaOperator.Parse("[MASTER_JOBCODE] = '" + projectSummary.JOBCODE + "'");
                IsActualDetailsVisible = false;
                IsPoDetailsVisible = true;
                IsInvoiceDetailsVisible = false;

                this.RaisePropertyChanged(x => x.PODetail);
                this.RaisePropertyChanged(x => x.POFilterCriteria);
            }

            this.RaisePropertyChanged(x => x.ActualDetailsVisibility);
            this.RaisePropertyChanged(x => x.PODetailsVisibility);
            this.RaisePropertyChanged(x => x.InvoiceDetailsVisibility);
            this.RaisePropertyChanged(x => x.DateSortIndex);
        }

        #endregion
    }
}