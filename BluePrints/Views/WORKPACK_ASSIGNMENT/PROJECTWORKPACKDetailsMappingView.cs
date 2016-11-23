using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BluePrints.Common;
using BluePrints.P6Data;
using BluePrints.Data;
using BluePrints.ViewModels;
using DevExpress.XtraScheduler;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraScheduler.Drawing;
using System.Reflection;
using DevExpress.XtraTreeList.Menu;
using System.Collections.ObjectModel;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel;

namespace BluePrints.Views
{
    public partial class PROJECTWORKPACKDetailsMappingView : UserControl
    {
        IQueryable<TASK> TASKS;
        IQueryable<PROJWBS> WBSS;
        IEnumerable<WORKPACK_Dashboard> WORKPACK_Dashboards;
        CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACK_ASSIGNMENTSViewModel;
        IEnumerable<TASK_AppointmentInfo> TASK_Appointments;
        IEnumerable<TASK_AppointmentInfo> WBS_Appointments;
        IEnumerable<TASK_AppointmentInfo> TASK_WBSAppointments;
        bool ISMODIFIED; //Specify whether the context is a original or modified P6BASELINE
        public PROJECTWORKPACKDetailsMappingView(Func<IQueryable<TASK>> getTASKsFunc, Func<IQueryable<PROJWBS>> getWBSSFunc,
            Func<IQueryable<WORKPACK_Dashboard>> getWORKPACK_DashboardFunc,
            CollectionViewModel<WORKPACK_ASSIGNMENT, WORKPACK_ASSIGNMENT, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACK_ASSIGNMENTSViewModel, bool IsModified)
        {
            InitializeComponent();
            this.TASKS = getTASKsFunc();
            this.WBSS = getWBSSFunc();
            this.WORKPACK_Dashboards = getWORKPACK_DashboardFunc();
            this.ISMODIFIED = IsModified;
            this.WORKPACK_ASSIGNMENTSViewModel = WORKPACK_ASSIGNMENTSViewModel;
            this.TASK_Appointments = this.TASKS.OrderBy(x => x.task_id).Select(x => new TASK_AppointmentInfo(x)).ToArray().AsEnumerable();
            this.WBS_Appointments = this.WBSS.OrderBy(x => x.wbs_id).Select(x => new TASK_AppointmentInfo(x)).ToArray().AsEnumerable();
            this.TASK_WBSAppointments = this.TASK_Appointments.Concat(this.WBS_Appointments);

            SetDataBinding(this.TASK_Appointments, this.TASK_WBSAppointments, this.WORKPACK_Dashboards);
            SubscribeEvents();
            CalculateAppointmentsUnits();
        }

        private void SubscribeEvents()
        {
            gridViewWorkpack.MouseDown += new System.Windows.Forms.MouseEventHandler(gridViewWorkpack_MouseDown);
            gridViewWorkpack.MouseMove += new System.Windows.Forms.MouseEventHandler(gridViewWorkpack_MouseMove);
            schedulerControl1.InitAppointmentDisplayText += new DevExpress.XtraScheduler.AppointmentDisplayTextEventHandler(schedulerControl1_InitAppointmentDisplayText);
            schedulerControl1.DragDrop += new System.Windows.Forms.DragEventHandler(schedulerControl1_DragDrop);
            schedulerControl1.DragEnter += new System.Windows.Forms.DragEventHandler(schedulerControl1_DragEnter);
            schedulerControl1.DragOver += new System.Windows.Forms.DragEventHandler(schedulerControl1_DragOver);
            schedulerControl1.DoubleClick += new System.EventHandler(schedulerControl1_DoubleClick);
            resourcesTree1.CustomDrawNodeCell += new DevExpress.XtraTreeList.CustomDrawNodeCellEventHandler(resourcesTree1_CustomDrawNodeCell);
            resourcesTree1.PopupMenuShowing += new DevExpress.XtraTreeList.PopupMenuShowingEventHandler(resourcesTree1_PopupMenuShowing);
            gridControlWorkpack.DoubleClick += new System.EventHandler(gridControlWorkpack_DoubleClick);
        }

        private void SetDataBinding(IEnumerable<TASK_AppointmentInfo> TASK_Appointments, IEnumerable<TASK_AppointmentInfo> TASK_WBSAppointments, IEnumerable<WORKPACK_Dashboard> WORKPACK_Dashboards)
        {
            schedulerControl1.Start = TASK_WBSAppointments.Where(x => x.StartDate.Year > 1800).Min(x => x.StartDate);
            this.schedulerBindingSource.DataSource = TASK_WBSAppointments;
            this.gridBindingSource.DataSource = WORKPACK_Dashboards;
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
        GridHitInfo downHitInfo;
        private void gridViewWorkpack_MouseDown(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            downHitInfo = null;

            if (view == null)
                return;

            GridHitInfo hitInfo = view.CalcHitInfo(new Point(e.X, e.Y));
            if (Control.ModifierKeys != Keys.None)
                return;
            if (e.Button == MouseButtons.Left && hitInfo.InRow && hitInfo.HitTest != GridHitTest.RowIndicator)
                downHitInfo = hitInfo;
        }

        private void gridViewWorkpack_MouseMove(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.Button == MouseButtons.Left && downHitInfo != null)
            {
                Size dragSize = SystemInformation.DragSize;
                Rectangle dragRect = new Rectangle(new Point(downHitInfo.HitPoint.X - dragSize.Width / 2,
                    downHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);

                if (!dragRect.Contains(new Point(e.X, e.Y)))
                {
                    view.GridControl.DoDragDrop(GetDragData(view), DragDropEffects.All);
                    downHitInfo = null;
                }
            }
        }

        object GetDragData(GridView view)
        {
            WORKPACK_Dashboard dragWorkpack = (WORKPACK_Dashboard)view.GetFocusedRow();
            return dragWorkpack;
        }

        private void schedulerControl1_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                WORKPACK_Dashboard dragEnterWorkpack = (WORKPACK_Dashboard)((DataObject)e.Data).GetData(typeof(WORKPACK_Dashboard));
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
            WORKPACK_Dashboard dragDropWorkpack = (WORKPACK_Dashboard)((DataObject)e.Data).GetData(typeof(WORKPACK_Dashboard));
            Point pt = schedulerControl1.PointToClient(Control.MousePosition);
            SchedulerHitInfo schHitInfo = schedulerControl1.ActiveView.ViewInfo.CalcHitInfo(pt, false);
            if (schHitInfo.HitTest == SchedulerHitTest.AppointmentContent)
            {
                var dropAppointment = ((AppointmentViewInfo)schHitInfo.ViewInfo).Appointment;
                PROJECTWORKPACKDetailsWorkpackAssignment view = new PROJECTWORKPACKDetailsWorkpackAssignment(TASK_WBSAppointments, WORKPACK_Dashboards, WORKPACK_ASSIGNMENTSViewModel, ISMODIFIED, dropAppointment, dragDropWorkpack);
                view.ShowDialog();
                view.Dispose();

                CalculateAppointmentsUnits();
            }
        }
        #endregion

        private void CalculateAppointmentsUnits()
        {
            foreach (var WBSTASKAppointmentInfo in TASK_WBSAppointments.Where(x => x.Status == AppointmentActivityType.Activity))
            {
                if (WBSTASKAppointmentInfo.Subject == null || WBSTASKAppointmentInfo.Subject == string.Empty)
                    continue;

                decimal P6ActivityAssignedUnits = WORKPACK_Dashboards.Sum(x => x.ObservableWORKPACK_ASSIGNMENTS.Where(obj2 => obj2.P6_ACTIVITYID == WBSTASKAppointmentInfo.Subject).Sum(obj3 => (obj3.HIGH_VALUE - obj3.LOW_VALUE) + 1));
                WBSTASKAppointmentInfo.AssignedUnits = P6ActivityAssignedUnits;
            }

            RecurseSummarizeWBS(TASK_WBSAppointments);
            schedulerControl1.RefreshData();
        }

        //Recurse childrens to sum budgeted units
        private decimal RecurseSummarizeWBS(IEnumerable<TASK_AppointmentInfo> ChildTASKs)
        {
            foreach (var childTASK in ChildTASKs)
            {
                if(childTASK.Status == AppointmentActivityType.WBS)
                {
                    IEnumerable<TASK_AppointmentInfo> childBaselineMapItems = ChildTASKs.Where(x => x.ParentId == childTASK.task_id).AsEnumerable();
                    if (childBaselineMapItems.Count() != 0)
                        childTASK.AssignedUnits = RecurseSummarizeWBS(childBaselineMapItems);
                }
            }

            //if the foreach loop doesn't iterate
            return ChildTASKs.Sum(x => x.AssignedUnits);
        }

        private void schedulerControl1_DragOver(object sender, DragEventArgs e)
        {
            Point pt = schedulerControl1.PointToClient(Control.MousePosition);
            SchedulerHitInfo schHitInfo = schedulerControl1.ActiveView.ViewInfo.CalcHitInfo(pt, false);
            if (schHitInfo.HitTest == SchedulerHitTest.AppointmentContent)
            {
                Appointment moveAppointment = ((AppointmentViewInfo)schHitInfo.ViewInfo).Appointment;
                if (moveAppointment.StatusKey.ToString() == AppointmentActivityType.Milestone.ToString())
                    e.Effect = DragDropEffects.None;
                else
                {
                    schedulerControl1.GanttView.SelectAppointment(moveAppointment);
                    e.Effect = DragDropEffects.All;
                }
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void gridControlWorkpack_DoubleClick(object sender, EventArgs e)
        {
            WORKPACK_Dashboard selectedWORKPACK = (WORKPACK_Dashboard)gridViewWorkpack.GetFocusedRow();
            if (selectedWORKPACK != null)
            {
                PROJECTWORKPACKDetailsWorkpackAssignment view = new PROJECTWORKPACKDetailsWorkpackAssignment(TASK_WBSAppointments, WORKPACK_Dashboards, WORKPACK_ASSIGNMENTSViewModel, ISMODIFIED, null, selectedWORKPACK);
                view.ShowDialog();
                view.Dispose();

                CalculateAppointmentsUnits();
            }
        }

        private void schedulerControl1_DoubleClick(object sender, EventArgs e)
        {
            Point pt = schedulerControl1.PointToClient(Control.MousePosition);
            SchedulerHitInfo schHitInfo = schedulerControl1.ActiveView.ViewInfo.CalcHitInfo(pt, false);
            if (schHitInfo.HitTest == SchedulerHitTest.AppointmentContent)
            {
                Appointment dropAppointment = ((AppointmentViewInfo)schHitInfo.ViewInfo).Appointment;
                PROJECTWORKPACKDetailsActivityAssignment view = new PROJECTWORKPACKDetailsActivityAssignment(TASK_WBSAppointments, WORKPACK_Dashboards, WORKPACK_ASSIGNMENTSViewModel, ISMODIFIED, dropAppointment);
                view.ShowDialog();
                view.Dispose();

                CalculateAppointmentsUnits();
            }
        }

        List<Brush> predefinedWBSBrushes;
        private void resourcesTree1_CustomDrawNodeCell(object sender, DevExpress.XtraTreeList.CustomDrawNodeCellEventArgs e)
        {
            AppointmentActivityType nodeAppointmentActivityType = (AppointmentActivityType)e.Node.GetValue(3);

            if (nodeAppointmentActivityType == AppointmentActivityType.WBS)
            {
                if (predefinedWBSBrushes == null)
                    InitializePredefinedBrushColor();

                // Create brushes for cells.
                Brush backBrush;
                Brush WBSBrush = Brushes.Transparent;
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
            Brush result = Brushes.Transparent;

            Random rnd = new Random();

            Type brushesType = typeof(Brushes);

            PropertyInfo[] properties = brushesType.GetProperties();

            int random = rnd.Next(properties.Length);
            result = (Brush)properties[random].GetValue(null, null);

            return result;
        }

        private void resourcesTree1_PopupMenuShowing(object sender, DevExpress.XtraTreeList.PopupMenuShowingEventArgs e)
        {
            if (e.Menu is TreeListNodeMenu)
            {
                e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Collapse All", resourcesTree1_CollapseAll, DevExpress.Images.ImageResourceCache.Default.GetImage("office2013/actions/squeeze_16x16.png")));
                e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Expand All", resourcesTree1_ExpandAll, DevExpress.Images.ImageResourceCache.Default.GetImage("office2013/actions/stretch_16x16.png")));
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
