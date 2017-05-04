using BluePrints.Common.Base;
using BluePrints.Data;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class VARIATIONProjection : BluePrintsProjectionMasterDetailBase<VARIATION, VARIATION_ITEMProjection>
    {
        public override ObservableCollection<VARIATION_ITEMProjection> DetailEntities
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

                return DetailEntities.Sum(x => x.FORECAST_UNITS);
            }
        }

        public decimal TOTAL_COSTS
        {
            get
            {
                if (DetailEntities == null)
                    return 0;

                return DetailEntities.Sum(x => x.VARIATION_COST);
            }
        }
    }

    public static class VARIATIONProjectionQueries
    {
        public static IQueryable<VARIATIONProjection> JoinVARIATION_ITEMSOnVARIATIONS(
            IQueryable<VARIATION> VARIATIONS)
        {
            return VARIATIONS.Select(x => new VARIATIONProjection() { EntityKey = x.GUID, Entity = x });
        }
    }
}
