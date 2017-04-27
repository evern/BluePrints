using BluePrints.Common;
using BluePrints.P6Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluePrints.Data
{
    public class TASK_AppointmentInfo
    {
        public TASK_AppointmentInfo()
        {
        }

        public TASK_AppointmentInfo(TASK task)
        {
            AllDay = false;
            task_id = task.task_id;
            Type = 0;
            StartDate = (DateTime) task.target_start_date;
            EndDate = (DateTime) task.target_end_date;
            Subject = task.task_code;
            Description = task.task_name;
            Status = task.task_type.ToUpper().Contains("MILE")
                ? AppointmentActivityType.Milestone
                : AppointmentActivityType.Activity;
            Label = 0;
            ResourceId = task.task_id;
            ParentId = task.wbs_id;
            //WPF Implementation
            //ResourceId = "<ResourceIds>\r\n<ResourceId Type=\"System.Int32\" Value=\"" + task.task_id.ToString() + "\" />\r\n</ResourceIds>";
            //Color = ToRgb(System.Drawing.Color.Green);
        }

        public TASK_AppointmentInfo(PROJWBS projWBS)
        {
            AllDay = false;
            task_id = projWBS.wbs_id;
            ResourceId = projWBS.wbs_id;
            ParentId = projWBS.parent_wbs_id;
            Type = 0;
            Subject = projWBS.wbs_short_name;
            Description = projWBS.wbs_name;
            Status = AppointmentActivityType.WBS;
        }

        private int ToRgb(System.Drawing.Color color)
        {
            return (color.B << 16) | (color.G << 8) | color.R;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int task_id { get; set; }

        //task_id is required as per collection view model projection requirement to have same primary key as the primary entity
        public int? ParentId { get; set; }
        public int Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AllDay { get; set; }
        public string Subject { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public AppointmentActivityType Status { get; set; }
        public int Label { get; set; }
        public object ResourceId { get; set; }
        public string ReminderInfo { get; set; }
        public string RecurrenceInfo { get; set; }
        public decimal AssignedUnits { get; set; }
        public int Color { get; set; }
    }
}