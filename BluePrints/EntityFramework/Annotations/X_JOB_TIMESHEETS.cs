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

        public double WeekTotal
        {
            get
            {
                return SUM_DAY1 + SUM_DAY2 + SUM_DAY3 + SUM_DAY4 + SUM_DAY5 + SUM_DAY6 + SUM_DAY7;
            }
        }

        public string JobTitle => SUB_JOBCODE + " : " + TITLE;

        public DateTime Day1Date => DAY1DATE;
        public DateTime Day2Date => Day1Date.AddDays(1);
        public DateTime Day3Date => Day1Date.AddDays(2);
        public DateTime Day4Date => Day1Date.AddDays(3);
        public DateTime Day5Date => Day1Date.AddDays(4);
        public DateTime Day6Date => Day1Date.AddDays(5);
        public DateTime Day7Date => Day1Date.AddDays(6);
    }
}
