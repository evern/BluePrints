using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Loader;
using BaseModel.ViewModel.Services;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.Projections;
using BluePrints.Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace BluePrints.ViewModels
{
    public class HSECollectionViewModelWrapper :
        BluePrintsEntitiesCollectionWrapper
        <HSE, HSEReportProjection, Guid, IBluePrintsEntitiesUnitOfWork>
    {
        /// <summary>
        /// Creates a new instance of HSECollectionViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public static HSECollectionViewModelWrapper Create(IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
            return ViewModelSource.Create(() => new HSECollectionViewModelWrapper(unitOfWorkFactory));
        }


        /// <summary>
        /// Initializes a new instance of the HSECollectionViewModelWrapper class.
        /// This constructor is declared protected to avoid undesired instantiation of the HSECollectionViewModelWrapper type without the POCO proxy factory.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        protected HSECollectionViewModelWrapper(
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> unitOfWorkFactory = null)
        {
        }

        #region Database Operations
        public PROJECT loadPROJECT { get; set; }
        private IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
        protected override void resolveParameters(object parameter)
        {
            var PROJECTParameter = (EntitiesParameter<PROJECT>)parameter;
            loadPROJECT = PROJECTParameter.GetEntity();
        }

        protected override void initializeEntitiesLoadersDescription()
        {
            loaderCollection = new EntitiesLoaderDescriptionCollection(this);
        }

        protected override void onAuxiliaryEntitiesCollectionLoaded()
        {
            CreateMainViewModel(bluePrintsUnitOfWorkFactory, x => x.HSES);
            mainThreadDispatcher.BeginInvoke(new Action(() => mainEntityLoaderDescription.CreateCollectionViewModel()));
        }

        protected override Func<IRepositoryQuery<HSE>, IQueryable<HSEReportProjection>> specifyMainViewModelProjection()
        {
            return query => HSEReportProjectionQueries.UnwrapHSE(query.Where(x => x.GUID_PROJECT == loadPROJECT.GUID));
        }

        protected override void AssignCallBacksAndRaisePropertyChange(IEnumerable<HSEReportProjection> entities)
        {
            MainViewModel.AlwaysSkipMessage = true;
            MainViewModel.SetParentViewModel(this);
            base.AssignCallBacksAndRaisePropertyChange(entities);
        }

        public override string UnifiedValueValidation(HSEReportProjection projection, string field_name, object new_value)
        {
            return string.Empty;
        }

        public override string UnifiedRowValidation(HSEReportProjection projection)
        {
            return string.Empty;
        }
        #endregion

        #region View Properties
        [ServiceProperty(Key = "PivotGridControlService")]
        public virtual IPivotGridControlService PivotGridControlService { get { return null; } }


        protected override string ExportExcelFilename()
        {
            return "hse_stats_export.xlsx";
        }

        public override void ExportToExcel()
        {
            string ResultPath = string.Empty;
            if (FolderBrowserDialogService.ShowDialog())
            {
                ResultPath = FolderBrowserDialogService.ResultPath;
                PivotGridControlService.ExportToExcel(ResultPath + "\\" + ExportExcelFilename());
            }
        }


        decimal runningCompany;
        public void CustomSummary(CustomSummaryEventArgs e)
        {
            if (e.SummaryProcess == CustomSummaryProcess.Start)
            {
                runningCompany = 0;
            }
            if (e.SummaryProcess == CustomSummaryProcess.Calculate)
            {
                GridSummaryItem gridSummaryItem = e.Item as GridSummaryItem;
                if (gridSummaryItem != null)
                {
                    string fieldName = gridSummaryItem.FieldName;
                    HSEReportProjection row = (HSEReportProjection)e.Row;
                    if (fieldName == "StatsMask")
                        runningCompany += row.StatsValue;

                    e.TotalValue = runningCompany;
                }
                else
                    e.TotalValue = 0;
            }
        }

        /// <summary>
        /// The view name to be used when saving layout for IDocumentContent
        /// </summary>
        protected override string ViewName
        {
            get { return "HSECollectionViewModelWrapper"; }
        }
        #endregion
    }
}