namespace BluePrints.Views
{
    partial class BASELINE_ITEMSchedulingView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            DevExpress.XtraGrid.GridFormatRule gridFormatRule1 = new DevExpress.XtraGrid.GridFormatRule();
            DevExpress.XtraEditors.FormatConditionRuleDataBar formatConditionRuleDataBar1 = new DevExpress.XtraEditors.FormatConditionRuleDataBar();
            DevExpress.XtraScheduler.TimeRuler timeRuler1 = new DevExpress.XtraScheduler.TimeRuler();
            DevExpress.XtraScheduler.TimeRuler timeRuler2 = new DevExpress.XtraScheduler.TimeRuler();
            DevExpress.XtraScheduler.TimeScaleYear timeScaleYear1 = new DevExpress.XtraScheduler.TimeScaleYear();
            DevExpress.XtraScheduler.TimeScaleQuarter timeScaleQuarter1 = new DevExpress.XtraScheduler.TimeScaleQuarter();
            DevExpress.XtraScheduler.TimeScaleMonth timeScaleMonth1 = new DevExpress.XtraScheduler.TimeScaleMonth();
            DevExpress.XtraScheduler.TimeScaleWeek timeScaleWeek1 = new DevExpress.XtraScheduler.TimeScaleWeek();
            DevExpress.XtraScheduler.TimeScaleDay timeScaleDay1 = new DevExpress.XtraScheduler.TimeScaleDay();
            DevExpress.XtraScheduler.TimeScaleHour timeScaleHour1 = new DevExpress.XtraScheduler.TimeScaleHour();
            DevExpress.XtraScheduler.TimeScale15Minutes timeScale15Minutes1 = new DevExpress.XtraScheduler.TimeScale15Minutes();
            DevExpress.XtraScheduler.TimeRuler timeRuler3 = new DevExpress.XtraScheduler.TimeRuler();
            this.colAssigned = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridControlDeliverable = new DevExpress.XtraGrid.GridControl();
            this.gridBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.gridViewDeliverable = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colWorkpack = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colArea = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDocType = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDepartment = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colDiscipline = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colEntity = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colPrimaryTitle = new DevExpress.XtraGrid.Columns.GridColumn();
            this.schedulerStorage1 = new DevExpress.XtraScheduler.SchedulerStorage(this.components);
            this.schedulerBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.barAndDockingController1 = new DevExpress.XtraBars.BarAndDockingController(this.components);
            this.schedulerBarController1 = new DevExpress.XtraScheduler.UI.SchedulerBarController();
            this.schedulerControl1 = new DevExpress.XtraScheduler.SchedulerControl();
            this.resourcesTree1 = new DevExpress.XtraScheduler.UI.ResourcesTree();
            this.resourceTreeColumn1 = new DevExpress.XtraScheduler.Native.ResourceTreeColumn();
            this.resourceTreeColumn4 = new DevExpress.XtraScheduler.Native.ResourceTreeColumn();
            this.resourceTreeColumn2 = new DevExpress.XtraScheduler.Native.ResourceTreeColumn();
            this.resourceTreeColumn3 = new DevExpress.XtraScheduler.Native.ResourceTreeColumn();
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.splitContainerControl2 = new DevExpress.XtraEditors.SplitContainerControl();
            ((System.ComponentModel.ISupportInitialize)(this.gridControlDeliverable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewDeliverable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.schedulerStorage1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.schedulerBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barAndDockingController1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.schedulerBarController1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.schedulerControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resourcesTree1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            this.splitContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl2)).BeginInit();
            this.splitContainerControl2.SuspendLayout();
            this.SuspendLayout();
            // 
            // colAssigned
            // 
            this.colAssigned.Caption = "Assigned %";
            this.colAssigned.DisplayFormat.FormatString = "p2";
            this.colAssigned.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.colAssigned.FieldName = "ASSIGNED_PERCENTAGE";
            this.colAssigned.Name = "colAssigned";
            this.colAssigned.OptionsColumn.AllowEdit = false;
            this.colAssigned.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.colAssigned.OptionsColumn.ReadOnly = true;
            this.colAssigned.Visible = true;
            this.colAssigned.VisibleIndex = 8;
            // 
            // gridControlDeliverable
            // 
            this.gridControlDeliverable.Cursor = System.Windows.Forms.Cursors.Default;
            this.gridControlDeliverable.DataSource = this.gridBindingSource;
            this.gridControlDeliverable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControlDeliverable.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridControlDeliverable.Location = new System.Drawing.Point(0, 0);
            this.gridControlDeliverable.LookAndFeel.SkinName = "Visual Studio 2013 Light";
            this.gridControlDeliverable.LookAndFeel.UseDefaultLookAndFeel = false;
            this.gridControlDeliverable.MainView = this.gridViewDeliverable;
            this.gridControlDeliverable.Name = "gridControlDeliverable";
            this.gridControlDeliverable.ShowOnlyPredefinedDetails = true;
            this.gridControlDeliverable.Size = new System.Drawing.Size(1904, 463);
            this.gridControlDeliverable.TabIndex = 1;
            this.gridControlDeliverable.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridViewDeliverable});
            // 
            // gridViewDeliverable
            // 
            this.gridViewDeliverable.Appearance.EvenRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.gridViewDeliverable.Appearance.EvenRow.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewDeliverable.Appearance.EvenRow.Options.UseBackColor = true;
            this.gridViewDeliverable.Appearance.EvenRow.Options.UseFont = true;
            this.gridViewDeliverable.Appearance.FocusedCell.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewDeliverable.Appearance.FocusedCell.Options.UseFont = true;
            this.gridViewDeliverable.Appearance.FocusedRow.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewDeliverable.Appearance.FocusedRow.Options.UseFont = true;
            this.gridViewDeliverable.Appearance.GroupFooter.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewDeliverable.Appearance.GroupFooter.Options.UseFont = true;
            this.gridViewDeliverable.Appearance.GroupPanel.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewDeliverable.Appearance.GroupPanel.Options.UseFont = true;
            this.gridViewDeliverable.Appearance.GroupRow.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewDeliverable.Appearance.GroupRow.Options.UseFont = true;
            this.gridViewDeliverable.Appearance.HeaderPanel.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewDeliverable.Appearance.HeaderPanel.Options.UseFont = true;
            this.gridViewDeliverable.Appearance.OddRow.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewDeliverable.Appearance.OddRow.Options.UseFont = true;
            this.gridViewDeliverable.Appearance.Row.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewDeliverable.Appearance.Row.Options.UseFont = true;
            this.gridViewDeliverable.Appearance.SelectedRow.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewDeliverable.Appearance.SelectedRow.Options.UseFont = true;
            this.gridViewDeliverable.Appearance.ViewCaption.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridViewDeliverable.Appearance.ViewCaption.Options.UseFont = true;
            this.gridViewDeliverable.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colWorkpack,
            this.colArea,
            this.colDocType,
            this.colDepartment,
            this.colDiscipline,
            this.colEntity,
            this.colPrimaryTitle,
            this.colAssigned});
            gridFormatRule1.Column = this.colAssigned;
            gridFormatRule1.ColumnApplyTo = this.colAssigned;
            gridFormatRule1.Name = "Format0";
            formatConditionRuleDataBar1.AllowNegativeAxis = false;
            formatConditionRuleDataBar1.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            formatConditionRuleDataBar1.Appearance.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            formatConditionRuleDataBar1.Appearance.Options.UseBackColor = true;
            formatConditionRuleDataBar1.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            formatConditionRuleDataBar1.PredefinedName = null;
            gridFormatRule1.Rule = formatConditionRuleDataBar1;
            this.gridViewDeliverable.FormatRules.Add(gridFormatRule1);
            this.gridViewDeliverable.GridControl = this.gridControlDeliverable;
            this.gridViewDeliverable.Name = "gridViewDeliverable";
            this.gridViewDeliverable.OptionsSelection.MultiSelect = true;
            this.gridViewDeliverable.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CheckBoxRowSelect;
            this.gridViewDeliverable.OptionsView.ShowFooter = true;
            this.gridViewDeliverable.PopupMenuShowing += new DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventHandler(this.gridViewDeliverable_PopupMenuShowing);
            // 
            // colWorkpack
            // 
            this.colWorkpack.Caption = "Workpack";
            this.colWorkpack.FieldName = "Entity.WORKPACK.INTERNAL_NAME1";
            this.colWorkpack.Name = "colWorkpack";
            this.colWorkpack.OptionsColumn.AllowEdit = false;
            this.colWorkpack.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.colWorkpack.OptionsColumn.ReadOnly = true;
            this.colWorkpack.Visible = true;
            this.colWorkpack.VisibleIndex = 1;
            // 
            // colArea
            // 
            this.colArea.Caption = "Area";
            this.colArea.FieldName = "Entity.AREA.INTERNAL_NUM";
            this.colArea.Name = "colArea";
            this.colArea.OptionsColumn.AllowEdit = false;
            this.colArea.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.colArea.Visible = true;
            this.colArea.VisibleIndex = 2;
            // 
            // colDocType
            // 
            this.colDocType.Caption = "Doc Type";
            this.colDocType.FieldName = "Entity.DOCTYPE.NAME";
            this.colDocType.Name = "colDocType";
            this.colDocType.OptionsColumn.AllowEdit = false;
            this.colDocType.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.colDocType.Visible = true;
            this.colDocType.VisibleIndex = 3;
            // 
            // colDepartment
            // 
            this.colDepartment.Caption = "Department";
            this.colDepartment.FieldName = "Entity.DEPARTMENT.NAME";
            this.colDepartment.Name = "colDepartment";
            this.colDepartment.OptionsColumn.AllowEdit = false;
            this.colDepartment.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.colDepartment.Visible = true;
            this.colDepartment.VisibleIndex = 4;
            // 
            // colDiscipline
            // 
            this.colDiscipline.Caption = "Discipline";
            this.colDiscipline.FieldName = "Entity.DISCIPLINE.NAME";
            this.colDiscipline.Name = "colDiscipline";
            this.colDiscipline.OptionsColumn.AllowEdit = false;
            this.colDiscipline.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.colDiscipline.Visible = true;
            this.colDiscipline.VisibleIndex = 5;
            // 
            // colEntity
            // 
            this.colEntity.Caption = "Internal Number";
            this.colEntity.FieldName = "Entity.INTERNAL_NUM";
            this.colEntity.Name = "colEntity";
            this.colEntity.OptionsColumn.AllowEdit = false;
            this.colEntity.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.True;
            this.colEntity.OptionsColumn.ReadOnly = true;
            this.colEntity.Visible = true;
            this.colEntity.VisibleIndex = 6;
            // 
            // colPrimaryTitle
            // 
            this.colPrimaryTitle.Caption = "Primary Title";
            this.colPrimaryTitle.FieldName = "Entity.PRIMARY_TITLE";
            this.colPrimaryTitle.Name = "colPrimaryTitle";
            this.colPrimaryTitle.Visible = true;
            this.colPrimaryTitle.VisibleIndex = 7;
            // 
            // schedulerStorage1
            // 
            this.schedulerStorage1.Appointments.DataSource = this.schedulerBindingSource;
            this.schedulerStorage1.Appointments.Mappings.AllDay = "AllDay";
            this.schedulerStorage1.Appointments.Mappings.AppointmentId = "task_id";
            this.schedulerStorage1.Appointments.Mappings.Description = "Description";
            this.schedulerStorage1.Appointments.Mappings.End = "EndDate";
            this.schedulerStorage1.Appointments.Mappings.Label = "Label";
            this.schedulerStorage1.Appointments.Mappings.Location = "Location";
            this.schedulerStorage1.Appointments.Mappings.PercentComplete = "Color";
            this.schedulerStorage1.Appointments.Mappings.RecurrenceInfo = "RecurrenceInfo";
            this.schedulerStorage1.Appointments.Mappings.ReminderInfo = "ReminderInfo";
            this.schedulerStorage1.Appointments.Mappings.ResourceId = "ResourceId";
            this.schedulerStorage1.Appointments.Mappings.Start = "StartDate";
            this.schedulerStorage1.Appointments.Mappings.Status = "Status";
            this.schedulerStorage1.Appointments.Mappings.Subject = "Subject";
            this.schedulerStorage1.Appointments.Mappings.Type = "Type";
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("AllDay", "AllDay"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("DependencyObjectType", "DependencyObjectType"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("Description", "Description"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("Dispatcher", "Dispatcher"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("EndDate", "EndDate"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("IsSealed", "IsSealed"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("Label", "Label"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("Location", "Location"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("Name", "Name"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("RecurrenceInfo", "RecurrenceInfo"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("ReminderInfo", "ReminderInfo"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("StartDate", "StartDate"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("Status", "Status"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("Type", "Type"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("UniqueID", "UniqueID"));
            this.schedulerStorage1.Resources.CustomFieldMappings.Add(new DevExpress.XtraScheduler.ResourceCustomFieldMapping("BudgetedUnits", "BudgetedUnits"));
            this.schedulerStorage1.Resources.DataSource = this.schedulerBindingSource;
            this.schedulerStorage1.Resources.Mappings.Caption = "Subject";
            this.schedulerStorage1.Resources.Mappings.Color = "Color";
            this.schedulerStorage1.Resources.Mappings.Id = "ResourceId";
            this.schedulerStorage1.Resources.Mappings.ParentId = "ParentId";
            // 
            // barAndDockingController1
            // 
            this.barAndDockingController1.LookAndFeel.SkinName = "Visual Studio 2013 Light";
            this.barAndDockingController1.LookAndFeel.UseDefaultLookAndFeel = false;
            this.barAndDockingController1.PropertiesBar.AllowLinkLighting = false;
            this.barAndDockingController1.PropertiesBar.DefaultGlyphSize = new System.Drawing.Size(16, 16);
            this.barAndDockingController1.PropertiesBar.DefaultLargeGlyphSize = new System.Drawing.Size(32, 32);
            // 
            // schedulerBarController1
            // 
            this.schedulerBarController1.Control = this.schedulerControl1;
            // 
            // schedulerControl1
            // 
            this.schedulerControl1.ActiveViewType = DevExpress.XtraScheduler.SchedulerViewType.Gantt;
            this.schedulerControl1.DataStorage = this.schedulerStorage1;
            this.schedulerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.schedulerControl1.Font = new System.Drawing.Font("Calibri", 9.75F);
            this.schedulerControl1.GroupType = DevExpress.XtraScheduler.SchedulerGroupType.Resource;
            this.schedulerControl1.Location = new System.Drawing.Point(0, 0);
            this.schedulerControl1.Name = "schedulerControl1";
            this.schedulerControl1.OptionsCustomization.AllowAppointmentCopy = DevExpress.XtraScheduler.UsedAppointmentType.None;
            this.schedulerControl1.OptionsCustomization.AllowAppointmentCreate = DevExpress.XtraScheduler.UsedAppointmentType.None;
            this.schedulerControl1.OptionsCustomization.AllowAppointmentDelete = DevExpress.XtraScheduler.UsedAppointmentType.None;
            this.schedulerControl1.OptionsCustomization.AllowAppointmentDrag = DevExpress.XtraScheduler.UsedAppointmentType.None;
            this.schedulerControl1.OptionsCustomization.AllowAppointmentDragBetweenResources = DevExpress.XtraScheduler.UsedAppointmentType.None;
            this.schedulerControl1.OptionsCustomization.AllowAppointmentEdit = DevExpress.XtraScheduler.UsedAppointmentType.None;
            this.schedulerControl1.OptionsCustomization.AllowAppointmentMultiSelect = false;
            this.schedulerControl1.OptionsCustomization.AllowAppointmentResize = DevExpress.XtraScheduler.UsedAppointmentType.None;
            this.schedulerControl1.OptionsCustomization.AllowDisplayAppointmentDependencyForm = DevExpress.XtraScheduler.AllowDisplayAppointmentDependencyForm.Never;
            this.schedulerControl1.OptionsCustomization.AllowDisplayAppointmentForm = DevExpress.XtraScheduler.AllowDisplayAppointmentForm.Never;
            this.schedulerControl1.OptionsCustomization.AllowInplaceEditor = DevExpress.XtraScheduler.UsedAppointmentType.None;
            this.schedulerControl1.Size = new System.Drawing.Size(1387, 573);
            this.schedulerControl1.Start = new System.DateTime(2012, 1, 1, 0, 0, 0, 0);
            this.schedulerControl1.TabIndex = 2;
            this.schedulerControl1.Text = "schedulerControl1";
            this.schedulerControl1.Views.DayView.TimeRulers.Add(timeRuler1);
            this.schedulerControl1.Views.FullWeekView.Enabled = true;
            this.schedulerControl1.Views.FullWeekView.TimeRulers.Add(timeRuler2);
            this.schedulerControl1.Views.GanttView.Appearance.AlternateHeaderCaption.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.schedulerControl1.Views.GanttView.Appearance.AlternateHeaderCaption.Options.UseFont = true;
            this.schedulerControl1.Views.GanttView.Appearance.AlternateHeaderCaptionLine.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.schedulerControl1.Views.GanttView.Appearance.AlternateHeaderCaptionLine.Options.UseFont = true;
            this.schedulerControl1.Views.GanttView.Appearance.Appointment.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.schedulerControl1.Views.GanttView.Appearance.Appointment.Options.UseFont = true;
            this.schedulerControl1.Views.GanttView.Appearance.HeaderCaption.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.schedulerControl1.Views.GanttView.Appearance.HeaderCaption.Options.UseFont = true;
            this.schedulerControl1.Views.GanttView.Appearance.HeaderCaptionLine.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.schedulerControl1.Views.GanttView.Appearance.HeaderCaptionLine.Options.UseFont = true;
            this.schedulerControl1.Views.GanttView.Appearance.NavigationButton.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.schedulerControl1.Views.GanttView.Appearance.NavigationButton.Options.UseFont = true;
            this.schedulerControl1.Views.GanttView.Appearance.ResourceHeaderCaption.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.schedulerControl1.Views.GanttView.Appearance.ResourceHeaderCaption.Options.UseFont = true;
            this.schedulerControl1.Views.GanttView.Appearance.ResourceHeaderCaptionLine.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.schedulerControl1.Views.GanttView.Appearance.ResourceHeaderCaptionLine.Options.UseFont = true;
            this.schedulerControl1.Views.GanttView.AppointmentDisplayOptions.AppointmentInterspacing = 1;
            this.schedulerControl1.Views.GanttView.AppointmentDisplayOptions.ContinueArrowDisplayType = DevExpress.XtraScheduler.AppointmentContinueArrowDisplayType.Never;
            this.schedulerControl1.Views.GanttView.AppointmentDisplayOptions.EndTimeVisibility = DevExpress.XtraScheduler.AppointmentTimeVisibility.Never;
            this.schedulerControl1.Views.GanttView.AppointmentDisplayOptions.PercentCompleteDisplayType = DevExpress.XtraScheduler.PercentCompleteDisplayType.BarProgress;
            this.schedulerControl1.Views.GanttView.AppointmentDisplayOptions.ShowRecurrence = false;
            this.schedulerControl1.Views.GanttView.AppointmentDisplayOptions.ShowReminder = false;
            this.schedulerControl1.Views.GanttView.AppointmentDisplayOptions.StartTimeVisibility = DevExpress.XtraScheduler.AppointmentTimeVisibility.Never;
            this.schedulerControl1.Views.GanttView.AppointmentDisplayOptions.StatusDisplayType = DevExpress.XtraScheduler.AppointmentStatusDisplayType.Time;
            this.schedulerControl1.Views.GanttView.AppointmentDisplayOptions.TimeDisplayType = DevExpress.XtraScheduler.AppointmentTimeDisplayType.Clock;
            this.schedulerControl1.Views.GanttView.CellsAutoHeightOptions.Enabled = true;
            this.schedulerControl1.Views.GanttView.OptionsSelectionBehavior.KeepSelectedAppointments = true;
            this.schedulerControl1.Views.GanttView.ResourcesPerPage = 13;
            timeScaleQuarter1.Enabled = false;
            timeScaleMonth1.DisplayFormat = "MMM";
            timeScaleWeek1.DisplayFormat = "dd";
            timeScaleDay1.Enabled = false;
            timeScaleHour1.Enabled = false;
            timeScale15Minutes1.Enabled = false;
            this.schedulerControl1.Views.GanttView.Scales.Add(timeScaleYear1);
            this.schedulerControl1.Views.GanttView.Scales.Add(timeScaleQuarter1);
            this.schedulerControl1.Views.GanttView.Scales.Add(timeScaleMonth1);
            this.schedulerControl1.Views.GanttView.Scales.Add(timeScaleWeek1);
            this.schedulerControl1.Views.GanttView.Scales.Add(timeScaleDay1);
            this.schedulerControl1.Views.GanttView.Scales.Add(timeScaleHour1);
            this.schedulerControl1.Views.GanttView.Scales.Add(timeScale15Minutes1);
            this.schedulerControl1.Views.GanttView.ShowMoreButtons = false;
            this.schedulerControl1.Views.GanttView.ShowResourceHeaders = false;
            this.schedulerControl1.Views.GanttView.TimeIndicatorDisplayOptions.Visibility = DevExpress.XtraScheduler.TimeIndicatorVisibility.Never;
            this.schedulerControl1.Views.WorkWeekView.TimeRulers.Add(timeRuler3);
            // 
            // resourcesTree1
            // 
            this.resourcesTree1.Appearance.HeaderPanel.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resourcesTree1.Appearance.HeaderPanel.Options.UseFont = true;
            this.resourcesTree1.Appearance.Row.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resourcesTree1.Appearance.Row.Options.UseFont = true;
            this.resourcesTree1.Appearance.SelectedRow.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resourcesTree1.Appearance.SelectedRow.Options.UseFont = true;
            this.resourcesTree1.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.resourceTreeColumn1,
            this.resourceTreeColumn4,
            this.resourceTreeColumn2,
            this.resourceTreeColumn3});
            this.resourcesTree1.Cursor = System.Windows.Forms.Cursors.Default;
            this.resourcesTree1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resourcesTree1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resourcesTree1.Location = new System.Drawing.Point(0, 0);
            this.resourcesTree1.Name = "resourcesTree1";
            this.resourcesTree1.OptionsBehavior.Editable = false;
            this.resourcesTree1.RefreshDataOnSchedulerChanges = false;
            this.resourcesTree1.SchedulerControl = this.schedulerControl1;
            this.resourcesTree1.Size = new System.Drawing.Size(512, 573);
            this.resourcesTree1.TabIndex = 3;
            this.resourcesTree1.TreeLevelWidth = 12;
            // 
            // resourceTreeColumn1
            // 
            this.resourceTreeColumn1.AppearanceCell.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resourceTreeColumn1.AppearanceCell.Options.UseFont = true;
            this.resourceTreeColumn1.Caption = "Tasks";
            this.resourceTreeColumn1.FieldName = "Subject";
            this.resourceTreeColumn1.Name = "resourceTreeColumn1";
            this.resourceTreeColumn1.Visible = true;
            this.resourceTreeColumn1.VisibleIndex = 0;
            this.resourceTreeColumn1.Width = 150;
            // 
            // resourceTreeColumn4
            // 
            this.resourceTreeColumn4.AppearanceCell.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resourceTreeColumn4.AppearanceCell.Options.UseFont = true;
            this.resourceTreeColumn4.Caption = "Description";
            this.resourceTreeColumn4.FieldName = "Description";
            this.resourceTreeColumn4.Name = "resourceTreeColumn4";
            this.resourceTreeColumn4.Visible = true;
            this.resourceTreeColumn4.VisibleIndex = 1;
            this.resourceTreeColumn4.Width = 300;
            // 
            // resourceTreeColumn2
            // 
            this.resourceTreeColumn2.AppearanceCell.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resourceTreeColumn2.AppearanceCell.Options.UseFont = true;
            this.resourceTreeColumn2.AppearanceCell.Options.UseTextOptions = true;
            this.resourceTreeColumn2.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.resourceTreeColumn2.Caption = "Assigned Units";
            this.resourceTreeColumn2.FieldName = "AssignedUnits";
            this.resourceTreeColumn2.Name = "resourceTreeColumn2";
            this.resourceTreeColumn2.OptionsColumn.FixedWidth = true;
            this.resourceTreeColumn2.OptionsColumn.ReadOnly = true;
            this.resourceTreeColumn2.RowFooterSummary = DevExpress.XtraTreeList.SummaryItemType.Sum;
            this.resourceTreeColumn2.SummaryFooter = DevExpress.XtraTreeList.SummaryItemType.Sum;
            this.resourceTreeColumn2.SummaryFooterStrFormat = "Total Sum {0:n1}";
            this.resourceTreeColumn2.Visible = true;
            this.resourceTreeColumn2.VisibleIndex = 2;
            this.resourceTreeColumn2.Width = 100;
            // 
            // resourceTreeColumn3
            // 
            this.resourceTreeColumn3.Caption = "Status";
            this.resourceTreeColumn3.FieldName = "Status";
            this.resourceTreeColumn3.Name = "resourceTreeColumn3";
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Horizontal = false;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControl1.Name = "splitContainerControl1";
            this.splitContainerControl1.Panel1.Controls.Add(this.splitContainerControl2);
            this.splitContainerControl1.Panel1.Text = "Panel1";
            this.splitContainerControl1.Panel2.Controls.Add(this.gridControlDeliverable);
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(1904, 1041);
            this.splitContainerControl1.SplitterPosition = 573;
            this.splitContainerControl1.TabIndex = 4;
            this.splitContainerControl1.Text = "splitContainerControl1";
            // 
            // splitContainerControl2
            // 
            this.splitContainerControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl2.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControl2.Name = "splitContainerControl2";
            this.splitContainerControl2.Panel1.Controls.Add(this.resourcesTree1);
            this.splitContainerControl2.Panel1.Text = "Panel1";
            this.splitContainerControl2.Panel2.Controls.Add(this.schedulerControl1);
            this.splitContainerControl2.Panel2.Text = "Panel2";
            this.splitContainerControl2.Size = new System.Drawing.Size(1904, 573);
            this.splitContainerControl2.SplitterPosition = 512;
            this.splitContainerControl2.TabIndex = 0;
            this.splitContainerControl2.Text = "splitContainerControl2";
            // 
            // BASELINE_ITEMSchedulingView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.splitContainerControl1);
            this.Name = "BASELINE_ITEMSchedulingView";
            this.Size = new System.Drawing.Size(1904, 1041);
            ((System.ComponentModel.ISupportInitialize)(this.gridControlDeliverable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewDeliverable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.schedulerStorage1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.schedulerBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barAndDockingController1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.schedulerBarController1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.schedulerControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.resourcesTree1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl2)).EndInit();
            this.splitContainerControl2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraScheduler.SchedulerStorage schedulerStorage1;
        private System.Windows.Forms.BindingSource gridBindingSource;
        private System.Windows.Forms.BindingSource schedulerBindingSource;
        private DevExpress.XtraBars.BarAndDockingController barAndDockingController1;
        private DevExpress.XtraScheduler.UI.SchedulerBarController schedulerBarController1;
        private DevExpress.XtraScheduler.SchedulerControl schedulerControl1;
        private DevExpress.XtraScheduler.UI.ResourcesTree resourcesTree1;
        private DevExpress.XtraScheduler.Native.ResourceTreeColumn resourceTreeColumn1;
        private DevExpress.XtraScheduler.Native.ResourceTreeColumn resourceTreeColumn4;
        private DevExpress.XtraScheduler.Native.ResourceTreeColumn resourceTreeColumn2;
        private DevExpress.XtraScheduler.Native.ResourceTreeColumn resourceTreeColumn3;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl2;
        private DevExpress.XtraGrid.GridControl gridControlDeliverable;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewDeliverable;
        private DevExpress.XtraGrid.Columns.GridColumn colEntity;
        private DevExpress.XtraGrid.Columns.GridColumn colWorkpack;
        private DevExpress.XtraGrid.Columns.GridColumn colAssigned;
        private DevExpress.XtraGrid.Columns.GridColumn colDiscipline;
        private DevExpress.XtraGrid.Columns.GridColumn colArea;
        private DevExpress.XtraGrid.Columns.GridColumn colDocType;
        private DevExpress.XtraGrid.Columns.GridColumn colDepartment;
        private DevExpress.XtraGrid.Columns.GridColumn colPrimaryTitle;
    }
}
