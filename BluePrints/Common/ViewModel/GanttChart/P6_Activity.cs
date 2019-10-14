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
    public class P6_Activity
    {
        public int Id => Task != null ? Task.task_id : Proj_WBS.wbs_id;
        public int? ParentId => Task != null ? Task.wbs_id : Proj_WBS.parent_wbs_id;
        public string P6_ActivityId => Task != null ? Task.task_code : Proj_WBS.wbs_short_name;
        public string Description => Task != null ? Task.task_name : Proj_WBS.wbs_name;
        public AppointmentActivityType? ActivityType => Task != null ? Task.task_type.ToUpper().Contains("MILE") ? AppointmentActivityType.Milestone : AppointmentActivityType.Activity : AppointmentActivityType.WBS;
        public string DisplayActivityType
        {
            get
            {
                if (ActivityType == null)
                    return string.Empty;

                return ActivityType.ToString();
            }
        }

        public decimal Assigned_Units { get; set; }
        public int WBSLevel { get; set; }

        private DateTime? start;
        public virtual DateTime? Start { get => Task != null ? Task.target_start_date : null; set => start = value; }
        private DateTime? end;
        public virtual DateTime? End { get => Task != null ? Task.target_end_date : null; set => end = value; }
        public decimal? Budgeted_Units => Task != null ? Task.target_work_qty : (decimal?)null;
        public bool IsTask => Task != null;

        public virtual double Left { get; set; }
        public virtual double Width { get; set; }
        public virtual double DayWidth { get; set; }
        public virtual IEnumerable Days { get; set; }

        public IHaveCanvasWidth ParentViewModel { get; private set; }

        public static P6_Activity Create(TASK task, IHaveCanvasWidth parentViewModel)
        {
            return ViewModelSource.Create(() => new P6_Activity(task, parentViewModel));
        }

        public static P6_Activity Create(PROJWBS projWBS, IHaveCanvasWidth parentViewModel)
        {
            return ViewModelSource.Create(() => new P6_Activity(projWBS, parentViewModel));
        }

        public TASK Task { get; private set; }
        public PROJWBS Proj_WBS { get; private set; }

        public P6_Activity(TASK task, IHaveCanvasWidth parentViewModel)
            : this(parentViewModel)
        {
            Task = task;
            WBSLevel = 0;
        }

        public P6_Activity(PROJWBS projWBS, IHaveCanvasWidth parentViewModel)
            : this(parentViewModel)
        {
            this.Proj_WBS = projWBS;
            WBSLevel = 0;
        }

        private P6_Activity(IHaveCanvasWidth parentViewModel)
        {
            ParentViewModel = parentViewModel;
        }
    }

    public interface IHaveCanvasWidth
    {
        double CanvasWidth { get; set; }
    }
}
