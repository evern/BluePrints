using BaseModel.Misc;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class POFlatForecastProjection : POForecastProjection
    {
        public string StockCode { get; set; }
        public string Narrative { get; set; }
        public DateTime? LastUpdated { get; set; }

        protected override List<FORECAST_PO> getPOForecasts(IEnumerable<FORECAST_PO> allFORECAST_POs)
        {
            return allFORECAST_POs.Where(x => x.PONO == this.PONO && x.VARIATION_CODE == this.VariationCode && x.STOCK_CODE == this.StockCode && x.DESCRIPTION == this.Description).ToList();
        }

        protected override List<ExoDataPoint> getCurrentActuals(IEnumerable<ExoDataPoint> allActuals)
        {
            return allActuals.Where(x => x.PONumber == this.PONO && x.Variation_Code == this.VariationCode && x.StockCode == this.StockCode && x.Description == this.Description).ToList();
        }
    }
}
