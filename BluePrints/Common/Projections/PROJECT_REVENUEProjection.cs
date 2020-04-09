using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BluePrints.Common.Projections
{
    public class PROJECT_REVENUEProjection : BluePrintsProjectionBase<PROJECT_REVENUE>
    {
        readonly IEnumerable<ExoDataPoint> revenueDataPoints;
        IEnumerable<ExoDataPoint> actualsDataPoints;
        IEnumerable<ExoDataPoint> materialsDataPoints;
        readonly DateTime maxClaimDate;
        public DateTime MonthFloor { get; set; }
        public DateTime MonthCeiling { get; set; }
        IEnumerable<PROJECT_REVENUEProjection> allRevenues;
        public PROJECT_REVENUEProjection()
        {
            Editor = "progressEditor";
            actualsDataPoints = new List<ExoDataPoint>();
            materialsDataPoints = new List<ExoDataPoint>();
        }

        public PROJECT_REVENUEProjection(DateTime revenuePeriod, IEnumerable<ExoDataPoint> revenueDataPoints, bool isFirstRow = false, PROJECT_REVENUE projectRevenue = null)
            : this()
        {
            MonthFloor = isFirstRow ? new DateTime() : new DateTime(revenuePeriod.Year, revenuePeriod.Month, 1);
            MonthCeiling = new DateTime(revenuePeriod.Year, revenuePeriod.Month, 1).AddMonths(1).AddDays(-1);
            this.revenueDataPoints = revenueDataPoints;

            if (projectRevenue == null)
                this.Entity = new PROJECT_REVENUE();
            else
                this.Entity = projectRevenue;

            if (revenueDataPoints.Count() == 0)
                maxClaimDate = new DateTime();
            else
                maxClaimDate = revenueDataPoints.Where(x => x.InvoiceDate != null).Max(x => (DateTime)x.InvoiceDate);
        }

        public bool IsRevenueReadOnly => MonthFloor < maxClaimDate;
        public SolidColorBrush Revenue_Background
        {
            get
            {
                if (IsRevenueReadOnly)
                    return new SolidColorBrush(Colors.Transparent);
                        
                if (ViewRevenue <= 0)
                    return new SolidColorBrush(Colors.LightSalmon);
                else
                    return new SolidColorBrush(Colors.Chartreuse);
            }
        }

        public void SetRevenues(IEnumerable<PROJECT_REVENUEProjection> projections)
        {
            allRevenues = projections;
        }

        public void SetActualDataPoints(IEnumerable<ExoDataPoint> actualDataPoints)
        {
            actualsDataPoints = actualDataPoints;
        }

        public void SetMaterialDataPoints(IEnumerable<ExoDataPoint> materialDataPoints)
        {
            materialsDataPoints = materialDataPoints;
            Editor = "numberEditor";
        }

        public decimal ActualCosts => actualsDataPoints.Where(x => x.ActualDate >= MonthFloor && x.ActualDate <= MonthCeiling).Sum(x => x.Costs);
        public decimal ActualCostsToDate => actualsDataPoints.Where(x => x.ActualDate <= MonthCeiling).Sum(x => x.Costs);

        public decimal MaterialCosts => materialsDataPoints.Where(x => x.ActualDate >= MonthFloor && x.ActualDate <= MonthCeiling).Sum(x => x.Costs);
        public decimal MaterialCostsToDate => materialsDataPoints.Where(x => x.ActualDate <= MonthCeiling).Sum(x => x.Costs);

        public decimal ForecastCosts { get; set; }
        public decimal ForecastCostsToDate { get; set; }

        public decimal TotalCosts => ActualCosts + MaterialCosts + ForecastCosts;
        public decimal TotalCostsToDate => ActualCostsToDate + MaterialCostsToDate + ForecastCostsToDate;

        public decimal Revenue => revenueDataPoints.Where(x => x.InvoiceDate >= MonthFloor && x.InvoiceDate <= MonthCeiling).Sum(x => x.InvoiceAmount);
        public decimal RevenueToDate => allRevenues == null ? 0 : allRevenues.Where(x => x.MonthCeiling <= MonthCeiling).Sum(x => x.ViewRevenue);

        public decimal? viewRevenue;
        public decimal ViewRevenue
        {
            get
            {
                if (IsRevenueReadOnly)
                    return Revenue;

                if (viewRevenue != null)
                    return (decimal)viewRevenue;

                return Entity.REVENUE_PRICE;
            }
            set
            {
                if (Revenue > 0)
                    return;

                viewRevenue = value;
            }
        }

        public decimal GetNewEntityRevenuePrice()
        {
            if (viewRevenue == null)
                return 0;

            return (decimal)viewRevenue;
        }

        public string Editor { get; set; }
        public decimal? Nett => ViewRevenue - TotalCosts;
        public decimal? NettToDate => RevenueToDate - TotalCostsToDate;
    }
}
