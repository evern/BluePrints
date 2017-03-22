using BluePrints.P6Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Reporting
{
    public class ReportablesDataPointsBuilder
    {
        private IQueryable<TASK> originalBaselineP6Tasks { get; set; }
        private IQueryable<TASK> modifiedBaselineP6Tasks { get; set; }
        private IQueryable<TASK> progressP6Tasks { get; set; }
        private TimeSpan progressInterval { get; set; }
        private DateTime reportingDataDate { get; set; }
        private DateTime firstAlignedDataDate { get; set; }
        private IEnumerable<DateTime> alignedWeekEndingDates { get; set; }
        private IEnumerable<Period> exceptionPeriods { get; set; }

        public ReportablesDataPointsBuilder()
        {

        }
    }
}
