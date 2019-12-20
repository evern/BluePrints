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
        readonly IEnumerable<ExoDataPoint> actualsDataPoints;
        readonly IEnumerable<ExoDataPoint> materialsDataPoints;
        readonly DateTime maxClaimDate;
        readonly DateTime monthFloor;
        readonly DateTime monthCeiling;
        public DateTime DisplayMonth => monthCeiling;
        public PROJECT_REVENUEProjection()
        {

        }

        public PROJECT_REVENUEProjection(DateTime revenuePeriod, IEnumerable<ExoDataPoint> revenueDataPoints, IEnumerable<ExoDataPoint> actualsDataPoints, IEnumerable<ExoDataPoint> materialsDataPoints, PROJECT_REVENUE projectRevenue = null)
        {
            monthFloor = new DateTime(revenuePeriod.Year, revenuePeriod.Month, 1);
            monthCeiling = monthFloor.AddMonths(1).AddDays(-1);
            this.revenueDataPoints = revenueDataPoints;
            this.actualsDataPoints = actualsDataPoints;
            this.materialsDataPoints = materialsDataPoints;

            if (projectRevenue == null)
                this.Entity = new PROJECT_REVENUE();
            else
                this.Entity = projectRevenue;

            maxClaimDate = revenueDataPoints.Max(x => x.ActualDate);
        }

        public bool IsRevenueReadOnly => monthFloor < maxClaimDate;
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

        public decimal ActualCosts => actualsDataPoints.Where(x => x.ActualDate >= monthFloor && x.ActualDate <= monthCeiling).Sum(x => x.Costs);
        public decimal ActualCostsToDate => actualsDataPoints.Where(x => x.ActualDate <= monthCeiling).Sum(x => x.Costs);

        public decimal MaterialCosts => materialsDataPoints.Where(x => x.ActualDate >= monthFloor && x.ActualDate <= monthCeiling).Sum(x => x.Costs);
        public decimal MaterialCostsToDate => materialsDataPoints.Where(x => x.ActualDate <= monthCeiling).Sum(x => x.Costs);

        public decimal TotalCosts => ActualCosts + MaterialCosts;
        public decimal TotalCostsToDate => ActualCostsToDate + MaterialCostsToDate;

        public decimal Revenue => revenueDataPoints.Where(x => x.ActualDate >= monthFloor && x.ActualDate <= monthCeiling).Sum(x => x.Costs);
        public decimal RevenueToDate => revenueDataPoints.Where(x => x.ActualDate <= monthCeiling).Sum(x => x.Costs);

        decimal viewRevenue;
        public decimal ViewRevenue
        {
            get
            {
                if (IsRevenueReadOnly)
                    return Revenue;

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
            return viewRevenue;
        }

        public decimal? Nett => !IsRevenueReadOnly ? (decimal?)null : Revenue - TotalCosts;
        public decimal? NettToDate => !IsRevenueReadOnly ? (decimal?)null : RevenueToDate - TotalCostsToDate;
    }
}
