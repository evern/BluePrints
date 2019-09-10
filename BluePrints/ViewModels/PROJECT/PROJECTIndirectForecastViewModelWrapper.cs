using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.IO;
using BaseModel.ViewModel.Dialogs;
using BluePrints.Common.Resources;
using BaseModel.ViewModel.Services;
using System.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Utils.Filtering;
using System.ComponentModel.DataAnnotations;
using DevExpress.Data.Filtering;
using BaseModel.ViewModel.UndoRedo;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Timers;
using DevExpress.Xpf.Spreadsheet;
using DevExpress.Spreadsheet;
using BluePrints.Common.ViewModel.Misc;
using System.Threading.Tasks;
using BluePrints.P6EntitiesDataModel;
using BluePrints.P6Data;
using DevExpress.Xpf.Editors;
using System.Windows.Threading;
using System.Windows.Media;
using DevExpress.Xpf.Core.Serialization;
using System.Windows.Input;
using BluePrints.Common.ViewModel.Utils;
using DevExpress.Xpf.Editors.Settings;
using System.Windows.Controls;
using System.Windows.Data;

namespace BluePrints.ViewModels
{
    /// <summary>
    /// Represents the PROJECTS collection view model.
    /// </summary>
    public class PROJECTIndirectForecastViewModelWrapper : PROJECTForecastViewModelWrapper
    {
        /// <summary>
        /// Creates a new instance of PROJECT_ITEMSViewModelWrapper as a POCO view model.
        /// </summary>
        /// <param name="unitOfWorkFactory">A factory used to create a unit of work instance.</param>
        public new static PROJECTIndirectForecastViewModelWrapper Create()
        {
            return ViewModelSource.Create(() => new PROJECTIndirectForecastViewModelWrapper());
        }

        /// <summary>
        /// Initializes a new instance of the PROJECTViewModel class.
        /// This constructor is declared protected to avoid undesired instantiation of the PROJECTViewModel type without the POCO proxy factory.
        /// </summary>
        protected PROJECTIndirectForecastViewModelWrapper()
        {
            UseForecastJobHourOverride = true;
            IsJobForecast = true;
            IsWeeks = true;
        }

        protected override void addEntitiesLoader()
        {
            loaderCollection.AddLoaderDescription(bluePrintsUnitOfWorkFactory, x => x.FORECAST_JOBS, FORECAST_JOBProjectionFunc);
            base.addEntitiesLoader();
        }

        protected override void resolveParameters(object parameter)
        {
            base.resolveParameters(parameter);
        }

        private Func<IRepositoryQuery<FORECAST_JOB>, IQueryable<FORECAST_JOB>> FORECAST_JOBProjectionFunc()
        {
            return query => query.Where(x => x.GUID_PROJECT == LoadPROJECT.GUID);
        }

        protected override bool OnMainViewModelLoaded(IEnumerable<PROJECT_Dashboard> entities)
        {
            return base.OnMainViewModelLoaded(entities);
        }

        protected override void updateAdditionalJobInfo(ForecastJobData commodityJob)
        {
            ExoSubJobProjection projection = commodityJob.Projection;
            FORECAST_JOB findFORECAST_JOB = forecastJobLookup(projection.SubJob.Code, projection.Discipline.Code, projection.Commodity.Code, projection.Variation_Code);
            if(findFORECAST_JOB != null)
            {
                commodityJob.Description = findFORECAST_JOB.DESCRIPTION;
                commodityJob.Reference = findFORECAST_JOB.REFERENCE;
                commodityJob.Note = findFORECAST_JOB.NOTE;
                commodityJob.UOM = findFORECAST_JOB.UOM;
                commodityJob.JobRate = findFORECAST_JOB.FORECAST_RATE;
            }

            base.updateAdditionalJobInfo(commodityJob);
        }

        private FORECAST_JOB forecastJobLookup(string subjobCode, string disciplineCode, string commodityCode, string variationCode)
        {
            IEnumerable<FORECAST_JOB> matchedJobsWithoutVariation = FORECAST_JOBCollection.Where(x => x.SUBJOB_CODE == subjobCode && x.DISCIPLINE_CODE == disciplineCode && x.COMMODITY_CODE == commodityCode);
            if(matchedJobsWithoutVariation.Count() > 0)
            {
                if (variationCode == string.Empty || variationCode == null)
                    return matchedJobsWithoutVariation.FirstOrDefault(x => x.VARIATION_CODE == string.Empty || x.VARIATION_CODE == null);
                else
                    return matchedJobsWithoutVariation.FirstOrDefault(x => x.VARIATION_CODE == variationCode);
            }

            return null;
        }

        protected override void OnClose(CancelEventArgs e)
        {
            base.OnClose(e);
        }

        public override string ViewName => "PROJECTIndirectForecastView_v1.00";

        public IEnumerable<FORECAST_JOB> FORECAST_JOBCollection
        {
            get
            {
                return GetEntities<FORECAST_JOB>();
            }
        }
    }
}