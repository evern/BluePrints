using BluePrints.P6Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel
{
    public class GanttData
    {

        public int Id { get; set; }
        public int ParentId { get; set; }
        public string P6_ActivityId { get; set; }
        public string Description { get; set; }
        public AppointmentActivityType? ActivityType { get; set; }
        public string DisplayActivityType
        {
            get
            {
                if (ActivityType == null)
                    return string.Empty;

                return ActivityType.ToString();
            }
        }

        public decimal AssignedUnits { get; set; }
        public int WBSLevel { get; set; }

        public virtual DateTime Start { get; set; }
        public virtual DateTime End { get; set; }

        public virtual double Left { get; set; }
        public virtual double Width { get; set; }
        public virtual double DayWidth { get; set; }
        public virtual IEnumerable Days { get; set; }

        public IHaveCanvasWidth ParentViewModel { get; private set; }

        public static GanttData Create(TASK task, IHaveCanvasWidth parentViewModel)
        {
            return ViewModelSource.Create(() => new GanttData(task, parentViewModel));
        }

        public static GanttData Create(PROJWBS projWBS, IHaveCanvasWidth parentViewModel)
        {
            return ViewModelSource.Create(() => new GanttData(projWBS, parentViewModel));
        }

        public GanttData(int id, int parent, string description, string start, string end, IHaveCanvasWidth parentViewModel)
        {
            Id = id;
            ParentId = parent;
            Description = description;
            Start = DateTime.ParseExact(start, "MM/dd/yyyy HH:mm:ss", null);
            End = DateTime.ParseExact(end, "MM/dd/yyyy HH:mm:ss", null);

            Left = 100d;
            Width = 100d;

            DayWidth = 100;
            Days = Enumerable.Range(0, 30);

            ParentViewModel = parentViewModel;
        }

        public GanttData(TASK task, IHaveCanvasWidth parentViewModel)
            : this(parentViewModel)
        {
            Id = task.task_id;
            P6_ActivityId = task.task_code;
            Start = (DateTime)task.target_start_date;
            End = (DateTime)task.target_end_date;
            Description = task.task_name;
            ParentId = task.wbs_id;
            ActivityType = task.task_type.ToUpper().Contains("MILE")
            ? AppointmentActivityType.Milestone
            : AppointmentActivityType.Activity;
            WBSLevel = 0;
        }

        public GanttData(PROJWBS projWBS, IHaveCanvasWidth parentViewModel)
            : this(parentViewModel)
        {
            Id = projWBS.wbs_id;
            if(projWBS.parent_wbs_id != null)
                ParentId = (int)projWBS.parent_wbs_id;

            P6_ActivityId = projWBS.wbs_short_name;
            Description = projWBS.wbs_name;
            ActivityType = AppointmentActivityType.WBS;
            WBSLevel = 0;
        }

        private GanttData(IHaveCanvasWidth parentViewModel)
        {
            ParentViewModel = parentViewModel;
        }
    }

    public interface IHaveCanvasWidth
    {
        double CanvasWidth { get; set; }
    }
}
