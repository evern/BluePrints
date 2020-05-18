namespace BluePrints.PrimeroData
{
    using BaseModel.DataModel;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_JOB_TIMESHEETS : EntityBase
    {
        public static string UnapprovedStatus => "N";

        public double? Day1ApprovedHours => DAY1_POSTED == UnapprovedStatus ? (double?)null : DAY1;
        public double? Day2ApprovedHours => DAY2_POSTED == UnapprovedStatus ? (double?)null : DAY2;
        public double? Day3ApprovedHours => DAY3_POSTED == UnapprovedStatus ? (double?)null : DAY3;
        public double? Day4ApprovedHours => DAY4_POSTED == UnapprovedStatus ? (double?)null : DAY4;
        public double? Day5ApprovedHours => DAY5_POSTED == UnapprovedStatus ? (double?)null : DAY5;
        public double? Day6ApprovedHours => DAY6_POSTED == UnapprovedStatus ? (double?)null : DAY6;
        public double? Day7ApprovedHours => DAY7_POSTED == UnapprovedStatus ? (double?)null : DAY7;

        public double WeekTotal
        {
            get
            {
                double day1Hours = Day1ApprovedHours == null ? 0 : (double)Day1ApprovedHours;
                double day2Hours = Day2ApprovedHours == null ? 0 : (double)Day2ApprovedHours;
                double day3Hours = Day3ApprovedHours == null ? 0 : (double)Day3ApprovedHours;
                double day4Hours = Day4ApprovedHours == null ? 0 : (double)Day4ApprovedHours;
                double day5Hours = Day5ApprovedHours == null ? 0 : (double)Day5ApprovedHours;
                double day6Hours = Day6ApprovedHours == null ? 0 : (double)Day6ApprovedHours;
                double day7Hours = Day7ApprovedHours == null ? 0 : (double)Day7ApprovedHours;

                return day1Hours + day2Hours + day3Hours + day4Hours + day5Hours + day6Hours + day7Hours;
            }
        }

        public DateTime Day1Date => WEEK_START_DATE == null ? DateTime.Now : (DateTime)WEEK_START_DATE;
        public DateTime Day2Date => Day1Date.AddDays(1);
        public DateTime Day3Date => Day1Date.AddDays(2);
        public DateTime Day4Date => Day1Date.AddDays(3);
        public DateTime Day5Date => Day1Date.AddDays(4);
        public DateTime Day6Date => Day1Date.AddDays(5);
        public DateTime Day7Date => Day1Date.AddDays(6);
    }
}
