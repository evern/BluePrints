using BluePrints.Common.ViewModel;
using BluePrints.Data;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace BluePrints.Common.Projections
{
    public class VARIATIONProjection : ProjectionMasterDetailBase<VARIATION, VARIATION_ITEMProjection>, IHaveGUID
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
            return VARIATIONS.Select(x => new VARIATIONProjection() { GUID = x.GUID, Entity = x });
        }
    }
}
