using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class VARIATIONProjection : BluePrintsProjectionBase<VARIATION>
    {
        public ObservableCollection<ISupportVariationSummary> DetailEntities
        {
            get { return GetProperty(() => DetailEntities); }
            set { SetProperty(() => DetailEntities, value, OnVARIATION_ITEMSChanged); }
        }

        void OnVARIATION_ITEMSChanged()
        {
            RaisePropertyChanged(() => TOTAL_UNITS);
            RaisePropertyChanged(() => TOTAL_COSTS);
        }

        public decimal TOTAL_UNITS
        {
            get
            {
                if (DetailEntities == null)
                    return 0;

                return DetailEntities.Sum(x => x.Forecast_Units);
            }
        }

        public decimal TOTAL_COSTS
        {
            get
            {
                if (DetailEntities == null)
                    return 0;

                return DetailEntities.Sum(x => x.Forecast_Costs);
            }
        }
    }

    public static class VARIATIONProjectionQueries
    {
        public static IQueryable<VARIATIONProjection> VariationProjection_Transformation(
            IQueryable<VARIATION> VARIATIONS, PROJECT PROJECT)
        {
            return VARIATIONS.ToArray().Where(x => x.GUID_PROJECT == PROJECT.GUID).OrderBy(x => x.NAME).Select(x => new VARIATIONProjection() { Entity = x }).AsQueryable();
        }
    }
}
