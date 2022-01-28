using BluePrints.Common.Utils;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.ViewModel.Reporting
{
    public static class ChronologicalHelpers
    {
        public static DateTime? GetEarliestFirstAlignedDataDate(IEnumerable<PROGRESS> PROGRESSES)
        {
            DateTime? earliest_first_aligned_data_date = null;
            //when construction progress have an earlier data date it'll skew the day of week of the weekly progress thats responsible of retrieving period data point, so always use the weekly one and make sure all weekly falls on the same day of week
            foreach (PROGRESS PROGRESS in PROGRESSES.Where(x => x.INTERVAL_TYPE == ProgressIntervalType.Weekly))
            {
                DateTime current_first_aligned_data_date = ChronologicalHelpers.GenerateFirstAlignedDataDate(PROGRESS);
                if (earliest_first_aligned_data_date == null)
                    earliest_first_aligned_data_date = current_first_aligned_data_date;
                else if (earliest_first_aligned_data_date > current_first_aligned_data_date)
                    earliest_first_aligned_data_date = current_first_aligned_data_date;
            }

            return earliest_first_aligned_data_date;
        }

        public static DateTime? GetReportLastDataDate(IEnumerable<PROGRESS> PROGRESSES)
        {
            DateTime? latestDataDate = null;

            //when there's no design progress use Sunday as end of week
            int endOfWeek = 7;
            PROGRESS designPROGRESS = PROGRESSES.FirstOrDefault(x => x.TYPE == PhaseType.Design);
            if (designPROGRESS != null)
                endOfWeek = BluePrintsUtils.GetTrueDayOfWeek((int)getProgressDataDate(designPROGRESS).DayOfWeek);

            //when construction progress have an earlier data date it'll skew the day of week of the weekly progress thats responsible of retrieving period data point, so always use the weekly one and make sure all weekly falls on the same day of week
            foreach (PROGRESS PROGRESS in PROGRESSES.Where(x => x.INTERVAL_TYPE == ProgressIntervalType.Weekly))
            {
                if (latestDataDate == null)
                    latestDataDate = getProgressDataDate(PROGRESS);
                else if (latestDataDate < PROGRESS.DATA_DATE)
                    latestDataDate = getProgressDataDate(PROGRESS);
            }

            //don't use end of week as data date or else CurrentPeriodDataPoint will not show anything
            //if (latestDataDate != null)
            //{
            //    int dataDateDayOfWeek = BluePrintsUtils.GetTrueDayOfWeek((int)((DateTime)latestDataDate).DayOfWeek);
            //    DateTime endOfLatestDataDateWeek = DateTime.Today.Date.AddDays(endOfWeek - dataDateDayOfWeek).AddDays(1).AddSeconds(-1);
            //    latestDataDate = endOfLatestDataDateWeek;
            //}

            return latestDataDate;
        }

        public static DateTime getProgressDataDate(PROGRESS progress)
        {
            return progress.REPORT_DATE != null ? (DateTime)progress.REPORT_DATE : progress.DATA_DATE;
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
        public static DateTime GenerateAlignedDataDate(DateTime firstAlignedDataDate, DateTime arbitraryStartDate,
            TimeSpan intervalPeriod)
        {
            var weekEndingAlignedDataDate = firstAlignedDataDate;

            do
            {
                weekEndingAlignedDataDate = weekEndingAlignedDataDate.AddDays(intervalPeriod.Days);
            } while (weekEndingAlignedDataDate <= arbitraryStartDate);

            return weekEndingAlignedDataDate;
        }

        public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
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

            return RewindDataDate(principalProgress.PROGRESS_START, principalProgress.DATA_DATE, intervalPeriod);
        }

        /// <summary>
        /// Rewind the data date backwards to get the first aligned data date as per the project start date
        /// </summary>
        public static DateTime RewindDataDate(DateTime fixedStartDate, DateTime fixedDataDate, TimeSpan periodInterval)
        {
            var dataDate = fixedDataDate;
            if (fixedDataDate.Year < fixedStartDate.Year)
                return fixedDataDate;

            //rewind the first progress date to scan to before the datadate aligned to startdate day of week
            while (dataDate.AddDays(-1 * periodInterval.Days) > fixedStartDate.Date.AddSeconds(-1))
                dataDate = dataDate.AddDays(-1 * periodInterval.Days);

            return dataDate;
        }

        public static TimeSpan ConvertProgressIntervalToPeriod(PROGRESS PROGRESS)
        {
            int intervalCount = PROGRESS.INTERVAL_COUNT;
            if (intervalCount == 0)
                intervalCount = 1;

            TimeSpan intervalPeriod = TimeSpan.FromDays((int)PROGRESS.INTERVAL_TYPE * intervalCount);
            return intervalPeriod;
        }

        public static void AutosetProgressDataDate(PROGRESS progress)
        {
            var interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(progress);
            DateTime firstAlignedDataDate = ChronologicalHelpers.GenerateFirstAlignedDataDate(progress);

            //multiply by 2 to avoid missing out dates
            DateTime lastAlignedDataDate = DateTime.Now.AddDays(interval.Days * 2);

            List<DateTime> alignedDataDates = GenerateAlignedDatesCollection(firstAlignedDataDate, lastAlignedDataDate, interval);
            DateTime currentDateTime = DateTime.Now;
            if(currentDateTime.DayOfWeek == DayOfWeek.Monday)
            {
                DateTime previousWeekDataDate = alignedDataDates.LastOrDefault(x => x < currentDateTime);
                if (previousWeekDataDate != null)
                    progress.DATA_DATE = previousWeekDataDate;
            }
            else
            {
                DateTime currentWeekDataDate = alignedDataDates.FirstOrDefault(x => x > currentDateTime);
                if (currentWeekDataDate != null)
                    progress.DATA_DATE = currentWeekDataDate;
            }
        }

        /// <summary>
        /// Calculates the data date forward to get the last aligned data date as per the first aligned data date
        /// </summary>
        public static List<DateTime> GenerateAlignedDatesCollection(DateTime firstAlignedDataDate,
            DateTime lastDataPointDate, TimeSpan intervalPeriod)
        {
            var lastProgressDate = firstAlignedDataDate;
            //lastDataPointDate = lastDataPointDate.AddDays(intervalPeriod.Days);
            var alignedDataDatesCollection = new List<DateTime>();
            alignedDataDatesCollection.Add(firstAlignedDataDate);
            //forward the first progress date to scan to after the datadate aligned to end day of week

            if(lastProgressDate < lastDataPointDate)
            {
                do
                {
                    lastProgressDate = lastProgressDate.AddDays(intervalPeriod.Days);
                    alignedDataDatesCollection.Add(lastProgressDate);
                } while (lastProgressDate < lastDataPointDate);
            }

            return alignedDataDatesCollection;
        }

        /// <summary>
        /// Calculates the data date forward to get the last aligned data date as per the first aligned data date
        /// </summary>
        public static List<DateTime> GenerateEndDatesCollection(DateTime firstAlignedDataDate, DateTime lastDataPointDate, bool isWeeks = false)
        {
            DateTime lastProgressDate;
            if(isWeeks)
            {
                lastProgressDate = GetFirstWeekdayOfNextMonth(firstAlignedDataDate, DayOfWeek.Sunday);
            }
            else
            {
                lastProgressDate = new DateTime(firstAlignedDataDate.Year, firstAlignedDataDate.Month, 1);
                lastProgressDate = lastProgressDate.AddDays(-1);
            }

            //adjust last datapoint date to end of the month
            DateTime lastEndOfMonthDate = new DateTime(lastDataPointDate.Year, lastDataPointDate.Month, 1);
            lastEndOfMonthDate = lastEndOfMonthDate.AddMonths(1).AddDays(-1);
            var alignedDataDatesCollection = new List<DateTime>();

            DateTime currentProgressDate = new DateTime(lastProgressDate.Year, lastProgressDate.Month, 1);
            //forward the first progress date to scan to after the datadate aligned to end day of week
            do
            {
                if (isWeeks)
                {
                    alignedDataDatesCollection.Add(lastProgressDate);
                    lastProgressDate = lastProgressDate.AddDays(7);
                    currentProgressDate = lastProgressDate;
                }
                else
                {
                    lastProgressDate = lastProgressDate.AddMonths(1);
                    currentProgressDate = new DateTime(lastProgressDate.Year, lastProgressDate.Month, 1).AddMonths(1).AddDays(-1);
                    alignedDataDatesCollection.Add(currentProgressDate);
                }
            } while (currentProgressDate < lastEndOfMonthDate);

            return alignedDataDatesCollection;
        }

        /// <summary>
        /// Calculates the data date forward to get the last aligned data date as per the first aligned data date
        /// </summary>
        public static List<DateTime> GenerateWeekDayEndDatesCollection(DateTime firstAlignedDataDate,
            DateTime lastDataPointDate, DayOfWeek dayOfWeek, bool isWeeks = false)
        {
            DateTime lastProgressDate = new DateTime(firstAlignedDataDate.Year, firstAlignedDataDate.Month, 1);
            lastProgressDate = lastProgressDate.AddDays(-1);

            //adjust last datapoint date to end of the month
            DateTime lastEndOfMonthDate = new DateTime(lastDataPointDate.Year, lastDataPointDate.Month, 1);
            lastEndOfMonthDate = lastEndOfMonthDate.AddMonths(1).AddDays(-1);

            var alignedDataDatesCollection = new List<DateTime>();
            DateTime currentProgressDate = lastProgressDate;
            DateTime lastNearestDayOfWeekDate = BluePrintsUtils.GetNearestSundayOfTheMonth(lastProgressDate);
            //forward the first progress date to scan to after the datadate aligned to end day of week
            do
            {
                if (isWeeks)
                {
                    alignedDataDatesCollection.Add(lastNearestDayOfWeekDate);
                    lastNearestDayOfWeekDate = lastNearestDayOfWeekDate.AddDays(7);
                    currentProgressDate = lastNearestDayOfWeekDate;
                }
                else
                {
                    lastProgressDate = new DateTime(lastProgressDate.Year, lastProgressDate.Month, 1).AddMonths(1);
                    currentProgressDate = BluePrintsUtils.GetNearestSundayOfTheMonth(lastProgressDate);
                    alignedDataDatesCollection.Add(currentProgressDate);
                }
            } while (currentProgressDate < lastEndOfMonthDate);

            return alignedDataDatesCollection;
        }

        public static DateTime GetLastWeekdayOfMonth(DateTime date, DayOfWeek day)
        {
            DateTime lastDayOfMonth = new DateTime(date.Year, date.Month, 1)
                .AddMonths(1).AddDays(-1);
            int wantedDay = (int)day;
            int lastDay = (int)lastDayOfMonth.DayOfWeek;
            return lastDayOfMonth.AddDays(
                lastDay >= wantedDay ? wantedDay - lastDay : wantedDay - lastDay - 7);
        }

        public static DateTime GetFirstWeekdayOfNextMonth(DateTime date, DayOfWeek day)
        {
            DateTime firstDayNextMonth = date.AddDays(-date.Day + 1).AddMonths(1);
            int wantedDay = (int)day;
            int diff = (wantedDay - (int)firstDayNextMonth.DayOfWeek);

            if (diff < 0)
                diff += 7;

            return firstDayNextMonth.AddDays(diff);
        }
    }
}
