using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using BluePrints.P6Data;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraScheduler;
using DevExpress.XtraScheduler.Drawing;
using DevExpress.XtraTreeList.Menu;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BluePrints.Views
{
    public partial class PROJECTWORKPACKDetailsMappingView : UserControl
    {
        private IEnumerable<TASK> TASKS;
        private IEnumerable<PROJWBS> WBSS;
        private IEnumerable<WORKPACK_Dashboard> WORKPACK_Dashboards;

        private CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
            WORKPACK_ASSIGNMENTSViewModel;

        private IEnumerable<TASK_AppointmentInfo> TASK_Appointments;
        private IEnumerable<TASK_AppointmentInfo> WBS_Appointments;
        private IEnumerable<TASK_AppointmentInfo> TASK_WBSAppointments;
        private bool ISMODIFIED; //Specify whether the context is a original or modified P6BASELINE

        public PROJECTWORKPACKDetailsMappingView(Func<IEnumerable<TASK>> getTASKsFunc,
            Func<IEnumerable<PROJWBS>> getWBSSFunc,
            Func<IEnumerable<WORKPACK_Dashboard>> getWORKPACK_DashboardFunc,
            CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork>
                WORKPACK_ASSIGNMENTSViewModel, bool IsModified)
        {
            InitializeComponent();
            TASKS = getTASKsFunc();
            WBSS = getWBSSFunc();
            WORKPACK_Dashboards = getWORKPACK_DashboardFunc();
            ISMODIFIED = IsModified;
            this.WORKPACK_ASSIGNMENTSViewModel = WORKPACK_ASSIGNMENTSViewModel;
            TASK_Appointments =
                TASKS.OrderBy(x => x.target_start_date).Select(x => new TASK_AppointmentInfo(x)).ToArray().AsEnumerable();
            WBS_Appointments =
                WBSS.OrderBy(x => x.wbs_id).Select(x => new TASK_AppointmentInfo(x)).ToArray().AsEnumerable();
            TASK_WBSAppointments = TASK_Appointments.Concat(WBS_Appointments);

            SetDataBinding(TASK_Appointments, TASK_WBSAppointments, WORKPACK_Dashboards);
            SubscribeEvents();
            CalculateAppointmentsUnits();
        }

        private void SubscribeEvents()
        {
            gridViewWorkpack.MouseDown += new MouseEventHandler(gridViewWorkpack_MouseDown);
            gridViewWorkpack.MouseMove += new MouseEventHandler(gridViewWorkpack_MouseMove);
            schedulerControl1.InitAppointmentDisplayText +=
                new AppointmentDisplayTextEventHandler(
                    schedulerControl1_InitAppointmentDisplayText);
            schedulerControl1.DragDrop += new DragEventHandler(schedulerControl1_DragDrop);
            schedulerControl1.DragEnter += new DragEventHandler(schedulerControl1_DragEnter);
            schedulerControl1.DragOver += new DragEventHandler(schedulerControl1_DragOver);
            schedulerControl1.DoubleClick += new EventHandler(schedulerControl1_DoubleClick);
            resourcesTree1.CustomDrawNodeCell +=
                new DevExpress.XtraTreeList.CustomDrawNodeCellEventHandler(resourcesTree1_CustomDrawNodeCell);
            resourcesTree1.PopupMenuShowing +=
                new DevExpress.XtraTreeList.PopupMenuShowingEventHandler(resourcesTree1_PopupMenuShowing);
            gridControlWorkpack.DoubleClick += new EventHandler(gridControlWorkpack_DoubleClick);
        }

        private void SetDataBinding(IEnumerable<TASK_AppointmentInfo> TASK_Appointments,
            IEnumerable<TASK_AppointmentInfo> TASK_WBSAppointments, IEnumerable<WORKPACK_Dashboard> WORKPACK_Dashboards)
        {
            schedulerControl1.Start = TASK_WBSAppointments.Where(x => x.StartDate.Year > 1800).Min(x => x.StartDate);
            schedulerBindingSource.DataSource = TASK_WBSAppointments;
            gridBindingSource.DataSource = WORKPACK_Dashboards;
        }

        private void schedulerControl1_InitAppointmentDisplayText(object sender, AppointmentDisplayTextEventArgs e)
        {
            e.Text = e.Appointment.Description;
        }

        public class ModelAppointmentDependency
        {
            public int ParentId { get; set; }
            public int DependentId { get; set; }

            public ModelAppointmentDependency()
            {
            }
        }

        #region Drag N' Drop

        private GridHitInfo downHitInfo;

        private void gridViewWorkpack_MouseDown(object sender, MouseEventArgs e)
        {
            var view = sender as GridView;
            downHitInfo = null;

            if (view == null)
                return;

            var hitInfo = view.CalcHitInfo(new Point(e.X, e.Y));
            if (ModifierKeys != Keys.None)
                return;
            if (e.Button == MouseButtons.Left && hitInfo.InRow && hitInfo.HitTest != GridHitTest.RowIndicator)
                downHitInfo = hitInfo;
        }

        private void gridViewWorkpack_MouseMove(object sender, MouseEventArgs e)
        {
            var view = sender as GridView;
            if (e.Button == MouseButtons.Left && downHitInfo != null)
            {
                var dragSize = SystemInformation.DragSize;
                var dragRect = new Rectangle(new Point(downHitInfo.HitPoint.X - dragSize.Width / 2,
                    downHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);

                if (!dragRect.Contains(new Point(e.X, e.Y)))
                {
                    view.GridControl.DoDragDrop(GetDragData(view), DragDropEffects.All);
                    downHitInfo = null;
                }
            }
        }

        private object GetDragData(GridView view)
        {
            var dragWorkpack = (WORKPACK_Dashboard) view.GetFocusedRow();
            return dragWorkpack;
        }

        private void schedulerControl1_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                var dragEnterWorkpack =
                    (WORKPACK_Dashboard) ((DataObject) e.Data).GetData(typeof(WORKPACK_Dashboard));
            }
            catch
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            e.Effect = DragDropEffects.All;
        }

        private void schedulerControl1_DragDrop(object sender, DragEventArgs e)
        {
            var dragDropWorkpack =
                (WORKPACK_Dashboard) ((DataObject) e.Data).GetData(typeof(WORKPACK_Dashboard));
            var pt = schedulerControl1.PointToClient(MousePosition);
            var schHitInfo = schedulerControl1.ActiveView.ViewInfo.CalcHitInfo(pt, false);
            if (schHitInfo.HitTest == SchedulerHitTest.AppointmentContent)
            {
                var dropAppointment = ((AppointmentViewInfo) schHitInfo.ViewInfo).Appointment;
                var view = new WORKPACKAssignmentView(TASK_WBSAppointments, WORKPACK_Dashboards,
                    WORKPACK_ASSIGNMENTSViewModel, ISMODIFIED, dropAppointment, dragDropWorkpack);
                view.ShowDialog();
                view.Dispose();

                CalculateAppointmentsUnits();
            }
        }

        #endregion

        private void CalculateAppointmentsUnits()
        {
            foreach (
                var WBSTASKAppointmentInfo in
                TASK_WBSAppointments.Where(x => x.Status == AppointmentActivityType.Activity))
            {
                if (WBSTASKAppointmentInfo.Subject == null || WBSTASKAppointmentInfo.Subject == string.Empty)
                    continue;

                var P6ActivityAssignedUnits =
                    WORKPACK_Dashboards.Sum(
                        x =>
                            x.ObservableWORKPACK_ASSIGNMENTS.Where(
                                    obj2 => obj2.P6_ACTIVITYID == WBSTASKAppointmentInfo.Subject)
                                .Sum(obj3 => ((obj3.HIGH_VALUE - obj3.LOW_VALUE) + 0.01m) * x.Stats.totalUnits));
                WBSTASKAppointmentInfo.AssignedUnits = P6ActivityAssignedUnits;
            }

            RecurseSummarizeWBS(TASK_WBSAppointments);
            schedulerControl1.RefreshData();
        }

        //Recurse childrens to sum budgeted units
        private void RecurseSummarizeWBS(IEnumerable<TASK_AppointmentInfo> ChildTASKs)
        {
            foreach (var childTASK in ChildTASKs)
            {
                if (childTASK.Status == AppointmentActivityType.WBS)
                {
                    childTASK.AssignedUnits = 0;
                }
            }

            foreach (var childTASK in ChildTASKs)
                if (childTASK.Status == AppointmentActivityType.WBS)
                {
                    List<TASK_AppointmentInfo> childTASKInfos = new List<TASK_AppointmentInfo>();
                    AllChildActivityTask(ChildTASKs, childTASK, childTASKInfos);
                    //return childTASKInfos.Sum(x => x.AssignedUnits);
                    if (childTASKInfos.Count() != 0)
                        childTASK.AssignedUnits = childTASKInfos.Sum(x => x.AssignedUnits);
                }

            //if the foreach loop doesn't iterate
            //return ChildTASKs.Sum(x => x.AssignedUnits);
        }

        /// <summary>
        /// Recurse member instance to change its value
        /// </summary>
        /// <param name="propertyString">Property string to change</param>
        /// <param name="parentInstance">Instance to modify</param>
        /// <param name="value">Value to modify</param>
        public void AllChildActivityTask(IEnumerable<TASK_AppointmentInfo> childTASKInfo, TASK_AppointmentInfo parentTASKInfo, List<TASK_AppointmentInfo> childTASKInfosCollector)
        {
            IEnumerable<TASK_AppointmentInfo> childActivityTasks = childTASKInfo.Where(x => x.ParentId == parentTASKInfo.task_id);

            foreach (var childActivityTask in childActivityTasks)
            {
                childTASKInfosCollector.Add(childActivityTask);
                AllChildActivityTask(childTASKInfo, childActivityTask, childTASKInfosCollector);
            }
        }

        private void schedulerControl1_DragOver(object sender, DragEventArgs e)
        {
            var pt = schedulerControl1.PointToClient(MousePosition);
            var schHitInfo = schedulerControl1.ActiveView.ViewInfo.CalcHitInfo(pt, false);
            if (schHitInfo.HitTest == SchedulerHitTest.AppointmentContent)
            {
                var moveAppointment = ((AppointmentViewInfo) schHitInfo.ViewInfo).Appointment;
                if (moveAppointment.StatusKey.ToString() == AppointmentActivityType.Milestone.ToString())
                {
                    e.Effect = DragDropEffects.None;
                }
                else
                {
                    schedulerControl1.GanttView.SelectAppointment(moveAppointment);
                    e.Effect = DragDropEffects.All;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void gridControlWorkpack_DoubleClick(object sender, EventArgs e)
        {
            var selectedWORKPACK = (WORKPACK_Dashboard) gridViewWorkpack.GetFocusedRow();
            if (selectedWORKPACK != null)
            {
                var view = new WORKPACKAssignmentView(TASK_WBSAppointments, WORKPACK_Dashboards,
                    WORKPACK_ASSIGNMENTSViewModel, ISMODIFIED, null, selectedWORKPACK);
                view.ShowDialog();
                view.Dispose();

                CalculateAppointmentsUnits();
            }
        }

        private void schedulerControl1_DoubleClick(object sender, EventArgs e)
        {
            var pt = schedulerControl1.PointToClient(MousePosition);
            var schHitInfo = schedulerControl1.ActiveView.ViewInfo.CalcHitInfo(pt, false);
            if (schHitInfo.HitTest == SchedulerHitTest.AppointmentContent)
            {
                var dropAppointment = ((AppointmentViewInfo) schHitInfo.ViewInfo).Appointment;
                var view = new P6ACTIVITYAssignmentView(TASK_WBSAppointments, WORKPACK_Dashboards,
                    WORKPACK_ASSIGNMENTSViewModel, ISMODIFIED, dropAppointment);

                view.ShowDialog();
                view.Dispose();

                CalculateAppointmentsUnits();
            }
        }

        private List<Brush> predefinedWBSBrushes;

        private void resourcesTree1_CustomDrawNodeCell(object sender,
            DevExpress.XtraTreeList.CustomDrawNodeCellEventArgs e)
        {
            var nodeAppointmentActivityType = (AppointmentActivityType) e.Node.GetValue(3);

            if (nodeAppointmentActivityType == AppointmentActivityType.WBS)
            {
                if (predefinedWBSBrushes == null)
                    InitializePredefinedBrushColor();

                // Create brushes for cells.
                Brush backBrush;
                var WBSBrush = Brushes.Transparent;
                if (predefinedWBSBrushes.Count <= e.Node.Level)
                    predefinedWBSBrushes.Add(PickBrush());

                WBSBrush = predefinedWBSBrushes[e.Node.Level];
                backBrush = WBSBrush;

                // Fill the background.
                e.Graphics.FillRectangle(backBrush, e.Bounds);

                // Allow default painting for fonts.
                e.Handled = false;
            }
        }

        private void InitializePredefinedBrushColor()
        {
            predefinedWBSBrushes = new List<Brush>();
            predefinedWBSBrushes.Add(Brushes.MistyRose);
            predefinedWBSBrushes.Add(Brushes.LightSkyBlue);
            predefinedWBSBrushes.Add(Brushes.Linen);
            predefinedWBSBrushes.Add(Brushes.PaleGreen);
            predefinedWBSBrushes.Add(Brushes.DarkGray);
        }

        private Brush PickBrush()
        {
            var result = Brushes.Transparent;

            var rnd = new Random();

            var brushesType = typeof(Brushes);

            var properties = brushesType.GetProperties();

            var random = rnd.Next(properties.Length);
            result = (Brush) properties[random].GetValue(null, null);

            return result;
        }

        private void resourcesTree1_PopupMenuShowing(object sender, DevExpress.XtraTreeList.PopupMenuShowingEventArgs e)
        {
            if (e.Menu is TreeListNodeMenu)
            {
                e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Collapse All", resourcesTree1_CollapseAll,
                    DevExpress.Images.ImageResourceCache.Default.GetImage("office2013/actions/squeeze_16x16.png")));
                e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Expand All", resourcesTree1_ExpandAll,
                    DevExpress.Images.ImageResourceCache.Default.GetImage("office2013/actions/stretch_16x16.png")));
            }
        }

        private void resourcesTree1_ExpandAll(object sender, EventArgs e)
        {
            resourcesTree1.ExpandAll();
        }

        private void resourcesTree1_CollapseAll(object sender, EventArgs e)
        {
            resourcesTree1.CollapseAll();
            resourcesTree1.Nodes.First().Expanded = true;
        }
    }
}