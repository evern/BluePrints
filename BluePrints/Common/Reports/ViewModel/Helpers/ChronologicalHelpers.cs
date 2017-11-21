using BluePrints.Data;
using System;
using System.Collections.Generic;

namespace BluePrints.Common.ViewModel.Reporting
{
    public static class ChronologicalHelpers
    {
        public static DateTime? GetEarliestFirstAlignedDataDate(IEnumerable<PROGRESS> PROGRESSES)
        {
            DateTime? earliest_first_aligned_data_date = null;
            foreach (PROGRESS PROGRESS in PROGRESSES)
            {
                DateTime current_first_aligned_data_date = ChronologicalHelpers.GenerateFirstAlignedDataDate(PROGRESS);
                if (earliest_first_aligned_data_date == null)
                    earliest_first_aligned_data_date = current_first_aligned_data_date;
                else if (earliest_first_aligned_data_date > current_first_aligned_data_date)
                    earliest_first_aligned_data_date = current_first_aligned_data_date;
            }

            return earliest_first_aligned_data_date;
        }

        public static DateTime? GetLastDataDate(IEnumerable<PROGRESS> PROGRESSES)
        {
            DateTime? latest_data_date = null;
            foreach (PROGRESS PROGRESS in PROGRESSES)
            {
                if (latest_data_date == null)
                    latest_data_date = PROGRESS.DATA_DATE;
                else if (latest_data_date < PROGRESS.DATA_DATE)
                    latest_data_date = PROGRESS.DATA_DATE;
            }

            return latest_data_date;
        }

        public static TimeSpan GetDefaultIntervalTimeSpan()
        {
            return new TimeSpan(7, 0, 0, 0);
        }

        /// <summary>
        /// Default exception periods, may be replaced by calendar settings
        /// </summary>
        public static List<Period> NonWorkingPeriods
        {
            get
            {
                var XmasStart = new DateTime(2015, 12, 21);
                var XmasEnd = new DateTime(2016, 1, 3);
                var nonworkingperiod = new List<Period>();
                nonworkingperiod.Add(new Period(XmasStart.Date, XmasEnd.Date));
                return nonworkingperiod;
            }
        }

        /// <summary>
        /// Calculates the aligned data date for subjob start date, used for remaining datapoints calculation
        /// </summary>
        public static DateTime GenerateSubjobAlignedDataDate(DateTime firstAlignedDataDate, DateTime subjobStartDate,
            TimeSpan intervalPeriod)
        {
            var weekEndingAlignedDataDate = firstAlignedDataDate;

            do
            {
                weekEndingAlignedDataDate = weekEndingAlignedDataDate.AddDays(intervalPeriod.Days);
            } while (weekEndingAlignedDataDate <= subjobStartDate);

            return weekEndingAlignedDataDate;
        }

        /// <summary>
        /// Calculates the data date backwards to get the first aligned data date as per the project start date
        /// </summary>
        public static DateTime GenerateFirstAlignedDataDate(PROGRESS principalProgress, TimeSpan? periodInterval = null)
        {
            if (periodInterval == null)
                periodInterval = ConvertProgressIntervalToPeriod(principalProgress);

            var intervalPeriod = (TimeSpan)periodInterval;
            var firstProgressDate = principalProgress.DATA_DATE;

            //rewind the first progress date to scan to before the datadate aligned to startdate day of week
            while (firstProgressDate.AddDays(-1 * intervalPeriod.Days) >
                   principalProgress.PROGRESS_START.Date.AddSeconds(-1))
                firstProgressDate = firstProgressDate.AddDays(-1 * intervalPeriod.Days);

            return firstProgressDate;
        }

        public static TimeSpan ConvertProgressIntervalToPeriod(PROGRESS PROGRESS)
        {
            int intervalCount = PROGRESS.INTERVAL_COUNT;
            if (intervalCount == 0)
                intervalCount = 1;

            TimeSpan intervalPeriod = TimeSpan.FromDays((int)PROGRESS.INTERVAL_TYPE * intervalCount);
            return intervalPeriod;
        }

        /// <summary>
        /// Calculates the data date forward to get the last aligned data date as per the first aligned data date
        /// </summary>
        public static List<DateTime> GenerateAlignedDatesCollection(DateTime firstAlignedDataDate,
            DateTime lastDataPointDate, TimeSpan intervalPeriod)
        {
            var lastProgressDate = firstAlignedDataDate;
            lastDataPointDate = lastDataPointDate.AddDays(intervalPeriod.Days);
            var alignedDataDatesCollection = new List<DateTime>();
            alignedDataDatesCollection.Add(firstAlignedDataDate);
            //forward the first progress date to scan to after the datadate aligned to end day of week
            do
            {
                lastProgressDate = lastProgressDate.AddDays(intervalPeriod.Days);
                alignedDataDatesCollection.Add(lastProgressDate);
            } while (lastProgressDate < lastDataPointDate);

            return alignedDataDatesCollection;
        }
    }
}
