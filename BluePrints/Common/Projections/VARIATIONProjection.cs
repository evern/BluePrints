using BluePrints.Common.ViewModel;
using BluePrints.Data;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class VARIATIONProjection : BindableBase, IHaveGUID
    {
        public VARIATIONProjection()
        {
            VARIATION = new VARIATION();
        }

        [Key]
        public Guid GUID { get; set; }
        public VARIATION VARIATION { get; set; }
        public IEnumerable<VARIATION_ITEMProjection> VARIATION_ITEMS
        {
            get { return GetProperty(() => VARIATION_ITEMS); }
            set { SetProperty(() => VARIATION_ITEMS, value, OnVARIATION_ITEMSChanged); }
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
                if (VARIATION_ITEMS == null)
                    return 0;

                return VARIATION_ITEMS.Sum(x => x.FORECAST_UNITS);
            }
        }

        public decimal TOTAL_COSTS
        {
            get
            {
                if (VARIATION_ITEMS == null)
                    return 0;

                return VARIATION_ITEMS.Sum(x => x.VARIATION_COST);
            }
        }
    }

    public static class VARIATIONProjectionQueries
    {
        public static IQueryable<VARIATIONProjection> JoinVARIATION_ITEMSOnVARIATIONS(
            IQueryable<VARIATION> VARIATIONS)
        {
            return VARIATIONS.Select(x => new VARIATIONProjection() { GUID = x.GUID, VARIATION = x });
        }
    }
}
