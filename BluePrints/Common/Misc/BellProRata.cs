using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Misc
{
    public static class BellProRata
    {
        public static decimal BetaPer(decimal a, decimal b, decimal periodNum, decimal totalPeriod, decimal beginPercentage)
        {
            decimal remainingPercentage = 1 - beginPercentage;
            if (remainingPercentage == 0)
                return 0;

            decimal inflatedTotalPeriod = totalPeriod / remainingPercentage;
            decimal absoluteStartPeriod = beginPercentage * inflatedTotalPeriod;
            decimal currentStartPeriod = absoluteStartPeriod + periodNum;

            if (inflatedTotalPeriod == 0)
                return 0;

            decimal betaTotal = BetaCum(a, b, 1) - BetaCum(a, b, absoluteStartPeriod / inflatedTotalPeriod);
            decimal betaThisPeriod = BetaCum(a, b, currentStartPeriod / inflatedTotalPeriod);
            decimal betaPreviousPeriod = BetaCum(a, b, (currentStartPeriod - 1) / inflatedTotalPeriod);
            decimal returnValue = betaThisPeriod - betaPreviousPeriod;

            return returnValue / betaTotal;
        }


        public static decimal BetaCum(decimal a, decimal b, decimal t)
        {
            if (t < 0)
                return 0;
            else if (t >= 1)
                return 1;
            else
            {
                double aDouble = (Convert.ToDouble(a));
                double bDouble = (Convert.ToDouble(b));
                double tDouble = (Convert.ToDouble(t));
                return Convert.ToDecimal(10 * Math.Pow(tDouble, 2) * Math.Pow(Convert.ToDouble(1 - tDouble), 2) * (aDouble + bDouble * tDouble) + Math.Pow(tDouble, 4) * (5 - 4 * tDouble));
            }
        }
    }
}
