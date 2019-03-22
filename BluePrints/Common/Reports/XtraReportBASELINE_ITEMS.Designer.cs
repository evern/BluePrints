namespace BluePrints.Reports
{
    partial class XtraReportBASELINE_ITEMS
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

        #region Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XtraReportBASELINE_ITEMS));
            DevExpress.XtraReports.UI.XRSummary xrSummary1 = new DevExpress.XtraReports.UI.XRSummary();
            DevExpress.XtraReports.UI.XRSummary xrSummary2 = new DevExpress.XtraReports.UI.XRSummary();
            this.Detail = new DevExpress.XtraReports.UI.DetailBand();
            this.xrlblDataDeliverableType = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblDataCosts = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblDataNumber = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblDataTitle = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblDataHours = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblDataDepartment = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblDataArea = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblDataType = new DevExpress.XtraReports.UI.XRLabel();
            this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
            this.xrLabel6 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrPictureBox2 = new DevExpress.XtraReports.UI.XRPictureBox();
            this.xrLabel13 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel14 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel12 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrPictureBox1 = new DevExpress.XtraReports.UI.XRPictureBox();
            this.projectName = new DevExpress.XtraReports.Parameters.Parameter();
            this.date1 = new DevExpress.XtraReports.Parameters.Parameter();
            this.title1 = new DevExpress.XtraReports.Parameters.Parameter();
            this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
            this.xrlblCosts = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblNumber = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblTitle = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblHours = new DevExpress.XtraReports.UI.XRLabel();
            this.pageFooterBand1 = new DevExpress.XtraReports.UI.PageFooterBand();
            this.xrPageInfo1 = new DevExpress.XtraReports.UI.XRPageInfo();
            this.xrPageInfo2 = new DevExpress.XtraReports.UI.XRPageInfo();
            this.reportHeaderBand1 = new DevExpress.XtraReports.UI.ReportHeaderBand();
            this.xrLabel11 = new DevExpress.XtraReports.UI.XRLabel();
            this.groupFooterBand1 = new DevExpress.XtraReports.UI.GroupFooterBand();
            this.Title = new DevExpress.XtraReports.UI.XRControlStyle();
            this.FieldCaption = new DevExpress.XtraReports.UI.XRControlStyle();
            this.PageInfo = new DevExpress.XtraReports.UI.XRControlStyle();
            this.DataField = new DevExpress.XtraReports.UI.XRControlStyle();
            this.calculatedCost = new DevExpress.XtraReports.UI.CalculatedField();
            this.PageHeader = new DevExpress.XtraReports.UI.PageHeaderBand();
            this.xrlblDeliverableType = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblArea = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblDepartment = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblType = new DevExpress.XtraReports.UI.XRLabel();
            this.showHours = new DevExpress.XtraReports.Parameters.Parameter();
            this.showCosts = new DevExpress.XtraReports.Parameters.Parameter();
            this.GroupFooter1 = new DevExpress.XtraReports.UI.GroupFooterBand();
            this.xrlblTotalCosts = new DevExpress.XtraReports.UI.XRLabel();
            this.xrlblTotalHours = new DevExpress.XtraReports.UI.XRLabel();
            this.showType = new DevExpress.XtraReports.Parameters.Parameter();
            this.showDeliverableType = new DevExpress.XtraReports.Parameters.Parameter();
            this.objectDataSource1 = new DevExpress.DataAccess.ObjectBinding.ObjectDataSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.objectDataSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // Detail
            // 
            this.Detail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrlblDataDeliverableType,
            this.xrlblDataCosts,
            this.xrlblDataNumber,
            this.xrlblDataTitle,
            this.xrlblDataHours,
            this.xrlblDataDepartment,
            this.xrlblDataArea,
            this.xrlblDataType});
            this.Detail.HeightF = 23F;
            this.Detail.Name = "Detail";
            this.Detail.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100F);
            this.Detail.SortFields.AddRange(new DevExpress.XtraReports.UI.GroupField[] {
            new DevExpress.XtraReports.UI.GroupField("Entity.AREA.INTERNAL_NUM", DevExpress.XtraReports.UI.XRColumnSortOrder.Ascending),
            new DevExpress.XtraReports.UI.GroupField("Entity.DEPARTMENT.NAME", DevExpress.XtraReports.UI.XRColumnSortOrder.Ascending),
            new DevExpress.XtraReports.UI.GroupField("Entity.INTERNAL_NUM", DevExpress.XtraReports.UI.XRColumnSortOrder.Ascending)});
            this.Detail.StyleName = "DataField";
            this.Detail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
            // 
            // xrlblDataDeliverableType
            // 
            this.xrlblDataDeliverableType.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Right | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblDataDeliverableType.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Entity.Entity.DELIVERABLE_TYPE]")});
            this.xrlblDataDeliverableType.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblDataDeliverableType.LocationFloat = new DevExpress.Utils.PointFloat(506F, 0F);
            this.xrlblDataDeliverableType.Name = "xrlblDataDeliverableType";
            this.xrlblDataDeliverableType.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblDataDeliverableType.SizeF = new System.Drawing.SizeF(100F, 23F);
            this.xrlblDataDeliverableType.StylePriority.UseBorders = false;
            this.xrlblDataDeliverableType.StylePriority.UseFont = false;
            // 
            // xrlblDataCosts
            // 
            this.xrlblDataCosts.AnchorHorizontal = DevExpress.XtraReports.UI.HorizontalAnchorStyles.Left;
            this.xrlblDataCosts.AnchorVertical = ((DevExpress.XtraReports.UI.VerticalAnchorStyles)((DevExpress.XtraReports.UI.VerticalAnchorStyles.Top | DevExpress.XtraReports.UI.VerticalAnchorStyles.Bottom)));
            this.xrlblDataCosts.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Right | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblDataCosts.CanGrow = false;
            this.xrlblDataCosts.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Total_Costs]")});
            this.xrlblDataCosts.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblDataCosts.LocationFloat = new DevExpress.Utils.PointFloat(905.0632F, 0F);
            this.xrlblDataCosts.Name = "xrlblDataCosts";
            this.xrlblDataCosts.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblDataCosts.SizeF = new System.Drawing.SizeF(133.9366F, 23F);
            this.xrlblDataCosts.StylePriority.UseBorders = false;
            this.xrlblDataCosts.StylePriority.UseFont = false;
            this.xrlblDataCosts.TextFormatString = "{0:$0.00}";
            // 
            // xrlblDataNumber
            // 
            this.xrlblDataNumber.AnchorVertical = ((DevExpress.XtraReports.UI.VerticalAnchorStyles)((DevExpress.XtraReports.UI.VerticalAnchorStyles.Top | DevExpress.XtraReports.UI.VerticalAnchorStyles.Bottom)));
            this.xrlblDataNumber.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Right | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblDataNumber.CanGrow = false;
            this.xrlblDataNumber.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Entity.Entity.INTERNAL_NUM]")});
            this.xrlblDataNumber.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblDataNumber.LocationFloat = new DevExpress.Utils.PointFloat(356F, 0F);
            this.xrlblDataNumber.Name = "xrlblDataNumber";
            this.xrlblDataNumber.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblDataNumber.SizeF = new System.Drawing.SizeF(150F, 23F);
            this.xrlblDataNumber.StylePriority.UseBorders = false;
            this.xrlblDataNumber.StylePriority.UseFont = false;
            // 
            // xrlblDataTitle
            // 
            this.xrlblDataTitle.AnchorVertical = ((DevExpress.XtraReports.UI.VerticalAnchorStyles)((DevExpress.XtraReports.UI.VerticalAnchorStyles.Top | DevExpress.XtraReports.UI.VerticalAnchorStyles.Bottom)));
            this.xrlblDataTitle.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Right | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblDataTitle.CanGrow = false;
            this.xrlblDataTitle.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Entity.Entity.PRIMARY_TITLE]")});
            this.xrlblDataTitle.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblDataTitle.LocationFloat = new DevExpress.Utils.PointFloat(606F, 0F);
            this.xrlblDataTitle.Name = "xrlblDataTitle";
            this.xrlblDataTitle.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblDataTitle.SizeF = new System.Drawing.SizeF(199.0633F, 23F);
            this.xrlblDataTitle.StylePriority.UseBorders = false;
            this.xrlblDataTitle.StylePriority.UseFont = false;
            // 
            // xrlblDataHours
            // 
            this.xrlblDataHours.AnchorHorizontal = DevExpress.XtraReports.UI.HorizontalAnchorStyles.Right;
            this.xrlblDataHours.AnchorVertical = ((DevExpress.XtraReports.UI.VerticalAnchorStyles)((DevExpress.XtraReports.UI.VerticalAnchorStyles.Top | DevExpress.XtraReports.UI.VerticalAnchorStyles.Bottom)));
            this.xrlblDataHours.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Right | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblDataHours.CanGrow = false;
            this.xrlblDataHours.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Total_Units]")});
            this.xrlblDataHours.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblDataHours.LocationFloat = new DevExpress.Utils.PointFloat(805.0632F, 0F);
            this.xrlblDataHours.Name = "xrlblDataHours";
            this.xrlblDataHours.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblDataHours.SizeF = new System.Drawing.SizeF(99.99994F, 23F);
            this.xrlblDataHours.StylePriority.UseBorders = false;
            this.xrlblDataHours.StylePriority.UseFont = false;
            this.xrlblDataHours.TextFormatString = "{0:n2}";
            // 
            // xrlblDataDepartment
            // 
            this.xrlblDataDepartment.AnchorVertical = ((DevExpress.XtraReports.UI.VerticalAnchorStyles)((DevExpress.XtraReports.UI.VerticalAnchorStyles.Top | DevExpress.XtraReports.UI.VerticalAnchorStyles.Bottom)));
            this.xrlblDataDepartment.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Right | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblDataDepartment.CanGrow = false;
            this.xrlblDataDepartment.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Entity.Entity.DEPARTMENT.NAME]")});
            this.xrlblDataDepartment.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblDataDepartment.LocationFloat = new DevExpress.Utils.PointFloat(110F, 0F);
            this.xrlblDataDepartment.Name = "xrlblDataDepartment";
            this.xrlblDataDepartment.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblDataDepartment.ProcessDuplicatesMode = DevExpress.XtraReports.UI.ProcessDuplicatesMode.Merge;
            this.xrlblDataDepartment.SizeF = new System.Drawing.SizeF(150F, 23F);
            this.xrlblDataDepartment.StylePriority.UseBorders = false;
            this.xrlblDataDepartment.StylePriority.UseFont = false;
            // 
            // xrlblDataArea
            // 
            this.xrlblDataArea.AnchorVertical = ((DevExpress.XtraReports.UI.VerticalAnchorStyles)((DevExpress.XtraReports.UI.VerticalAnchorStyles.Top | DevExpress.XtraReports.UI.VerticalAnchorStyles.Bottom)));
            this.xrlblDataArea.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblDataArea.CanGrow = false;
            this.xrlblDataArea.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Entity].[Entity].[AREA].[INTERNAL_NUM]")});
            this.xrlblDataArea.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblDataArea.LocationFloat = new DevExpress.Utils.PointFloat(9.99987F, 0F);
            this.xrlblDataArea.Name = "xrlblDataArea";
            this.xrlblDataArea.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblDataArea.ProcessDuplicatesMode = DevExpress.XtraReports.UI.ProcessDuplicatesMode.Merge;
            this.xrlblDataArea.SizeF = new System.Drawing.SizeF(100.0001F, 23F);
            this.xrlblDataArea.StylePriority.UseBorders = false;
            this.xrlblDataArea.StylePriority.UseFont = false;
            // 
            // xrlblDataType
            // 
            this.xrlblDataType.AnchorVertical = ((DevExpress.XtraReports.UI.VerticalAnchorStyles)((DevExpress.XtraReports.UI.VerticalAnchorStyles.Top | DevExpress.XtraReports.UI.VerticalAnchorStyles.Bottom)));
            this.xrlblDataType.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Right | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblDataType.CanGrow = false;
            this.xrlblDataType.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Entity.Entity.DOCTYPE.NAME]")});
            this.xrlblDataType.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblDataType.LocationFloat = new DevExpress.Utils.PointFloat(260F, 0F);
            this.xrlblDataType.Name = "xrlblDataType";
            this.xrlblDataType.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblDataType.SizeF = new System.Drawing.SizeF(96F, 23F);
            this.xrlblDataType.StylePriority.UseBorders = false;
            this.xrlblDataType.StylePriority.UseFont = false;
            // 
            // TopMargin
            // 
            this.TopMargin.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel6,
            this.xrPictureBox2,
            this.xrLabel13,
            this.xrLabel14,
            this.xrLabel12,
            this.xrPictureBox1});
            this.TopMargin.HeightF = 107.3333F;
            this.TopMargin.Name = "TopMargin";
            this.TopMargin.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100F);
            this.TopMargin.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
            // 
            // xrLabel6
            // 
            this.xrLabel6.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Parameters.projectName]")});
            this.xrLabel6.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrLabel6.LocationFloat = new DevExpress.Utils.PointFloat(292.4583F, 10.00001F);
            this.xrLabel6.Name = "xrLabel6";
            this.xrLabel6.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel6.SizeF = new System.Drawing.SizeF(533F, 56.33334F);
            this.xrLabel6.StylePriority.UseFont = false;
            this.xrLabel6.StylePriority.UseTextAlignment = false;
            this.xrLabel6.Text = "xrLabel6";
            this.xrLabel6.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // xrPictureBox2
            // 
            this.xrPictureBox2.LocationFloat = new DevExpress.Utils.PointFloat(825.4584F, 10.00001F);
            this.xrPictureBox2.Name = "xrPictureBox2";
            this.xrPictureBox2.SizeF = new System.Drawing.SizeF(213.5416F, 56.33334F);
            // 
            // xrLabel13
            // 
            this.xrLabel13.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.xrLabel13.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrLabel13.ForeColor = System.Drawing.Color.Black;
            this.xrLabel13.LocationFloat = new DevExpress.Utils.PointFloat(828.0832F, 66.33334F);
            this.xrLabel13.Name = "xrLabel13";
            this.xrLabel13.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel13.SizeF = new System.Drawing.SizeF(110.9166F, 41F);
            this.xrLabel13.StyleName = "FieldCaption";
            this.xrLabel13.StylePriority.UseBorders = false;
            this.xrLabel13.StylePriority.UseFont = false;
            this.xrLabel13.StylePriority.UseForeColor = false;
            this.xrLabel13.StylePriority.UseTextAlignment = false;
            this.xrLabel13.Text = "Date:";
            this.xrLabel13.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // xrLabel14
            // 
            this.xrLabel14.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Parameters.date1]")});
            this.xrLabel14.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrLabel14.LocationFloat = new DevExpress.Utils.PointFloat(938.9998F, 66.33334F);
            this.xrLabel14.Name = "xrLabel14";
            this.xrLabel14.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel14.SizeF = new System.Drawing.SizeF(100F, 41F);
            this.xrLabel14.StylePriority.UseFont = false;
            this.xrLabel14.StylePriority.UseTextAlignment = false;
            this.xrLabel14.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.xrLabel14.TextFormatString = "{0:dd-MMM-yy}";
            // 
            // xrLabel12
            // 
            this.xrLabel12.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Parameters.title1]")});
            this.xrLabel12.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrLabel12.LocationFloat = new DevExpress.Utils.PointFloat(6.00001F, 66.33334F);
            this.xrLabel12.Name = "xrLabel12";
            this.xrLabel12.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel12.SizeF = new System.Drawing.SizeF(448.0417F, 41F);
            this.xrLabel12.StylePriority.UseFont = false;
            this.xrLabel12.StylePriority.UseTextAlignment = false;
            this.xrLabel12.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // xrPictureBox1
            // 
            this.xrPictureBox1.ImageSource = new DevExpress.XtraPrinting.Drawing.ImageSource("img", resources.GetString("xrPictureBox1.ImageSource"));
            this.xrPictureBox1.LocationFloat = new DevExpress.Utils.PointFloat(6.00001F, 10.00001F);
            this.xrPictureBox1.Name = "xrPictureBox1";
            this.xrPictureBox1.SizeF = new System.Drawing.SizeF(286.4583F, 56.33334F);
            this.xrPictureBox1.Sizing = DevExpress.XtraPrinting.ImageSizeMode.StretchImage;
            // 
            // projectName
            // 
            this.projectName.Description = "Project Name";
            this.projectName.Name = "projectName";
            // 
            // date1
            // 
            this.date1.Description = "Date";
            this.date1.Name = "date1";
            this.date1.Type = typeof(System.DateTime);
            this.date1.Visible = false;
            // 
            // title1
            // 
            this.title1.Description = "Title";
            this.title1.Name = "title1";
            // 
            // BottomMargin
            // 
            this.BottomMargin.HeightF = 10F;
            this.BottomMargin.Name = "BottomMargin";
            this.BottomMargin.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 0, 0, 0, 100F);
            this.BottomMargin.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopLeft;
            // 
            // xrlblCosts
            // 
            this.xrlblCosts.AnchorHorizontal = DevExpress.XtraReports.UI.HorizontalAnchorStyles.Left;
            this.xrlblCosts.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblCosts.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblCosts.LocationFloat = new DevExpress.Utils.PointFloat(905.0633F, 0F);
            this.xrlblCosts.Name = "xrlblCosts";
            this.xrlblCosts.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblCosts.SizeF = new System.Drawing.SizeF(133.9367F, 18F);
            this.xrlblCosts.StylePriority.UseBorders = false;
            this.xrlblCosts.StylePriority.UseFont = false;
            this.xrlblCosts.Text = "Costs";
            // 
            // xrlblNumber
            // 
            this.xrlblNumber.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblNumber.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblNumber.LocationFloat = new DevExpress.Utils.PointFloat(356F, 0F);
            this.xrlblNumber.Name = "xrlblNumber";
            this.xrlblNumber.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblNumber.SizeF = new System.Drawing.SizeF(150F, 18F);
            this.xrlblNumber.StylePriority.UseBorders = false;
            this.xrlblNumber.StylePriority.UseFont = false;
            this.xrlblNumber.Text = "Number";
            // 
            // xrlblTitle
            // 
            this.xrlblTitle.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblTitle.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblTitle.LocationFloat = new DevExpress.Utils.PointFloat(605.9999F, 0F);
            this.xrlblTitle.Name = "xrlblTitle";
            this.xrlblTitle.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblTitle.SizeF = new System.Drawing.SizeF(199.0634F, 18F);
            this.xrlblTitle.StylePriority.UseBorders = false;
            this.xrlblTitle.StylePriority.UseFont = false;
            this.xrlblTitle.Text = "Title";
            // 
            // xrlblHours
            // 
            this.xrlblHours.AnchorHorizontal = DevExpress.XtraReports.UI.HorizontalAnchorStyles.Right;
            this.xrlblHours.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblHours.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblHours.LocationFloat = new DevExpress.Utils.PointFloat(805.0633F, 0F);
            this.xrlblHours.Name = "xrlblHours";
            this.xrlblHours.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblHours.SizeF = new System.Drawing.SizeF(99.99988F, 18F);
            this.xrlblHours.StylePriority.UseBorders = false;
            this.xrlblHours.StylePriority.UseFont = false;
            this.xrlblHours.Text = "Hours";
            // 
            // pageFooterBand1
            // 
            this.pageFooterBand1.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrPageInfo1,
            this.xrPageInfo2});
            this.pageFooterBand1.HeightF = 31F;
            this.pageFooterBand1.Name = "pageFooterBand1";
            // 
            // xrPageInfo1
            // 
            this.xrPageInfo1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrPageInfo1.LocationFloat = new DevExpress.Utils.PointFloat(6F, 8F);
            this.xrPageInfo1.Name = "xrPageInfo1";
            this.xrPageInfo1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrPageInfo1.PageInfo = DevExpress.XtraPrinting.PageInfo.DateTime;
            this.xrPageInfo1.SizeF = new System.Drawing.SizeF(438F, 23F);
            this.xrPageInfo1.StyleName = "PageInfo";
            this.xrPageInfo1.StylePriority.UseFont = false;
            // 
            // xrPageInfo2
            // 
            this.xrPageInfo2.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrPageInfo2.LocationFloat = new DevExpress.Utils.PointFloat(601F, 8F);
            this.xrPageInfo2.Name = "xrPageInfo2";
            this.xrPageInfo2.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrPageInfo2.SizeF = new System.Drawing.SizeF(438F, 23F);
            this.xrPageInfo2.StyleName = "PageInfo";
            this.xrPageInfo2.StylePriority.UseFont = false;
            this.xrPageInfo2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight;
            this.xrPageInfo2.TextFormatString = "Page {0} of {1}";
            // 
            // reportHeaderBand1
            // 
            this.reportHeaderBand1.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel11});
            this.reportHeaderBand1.HeightF = 39F;
            this.reportHeaderBand1.Name = "reportHeaderBand1";
            // 
            // xrLabel11
            // 
            this.xrLabel11.Font = new System.Drawing.Font("Calibri", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrLabel11.LocationFloat = new DevExpress.Utils.PointFloat(6.00001F, 0F);
            this.xrLabel11.Name = "xrLabel11";
            this.xrLabel11.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel11.SizeF = new System.Drawing.SizeF(1033F, 39F);
            this.xrLabel11.StyleName = "Title";
            this.xrLabel11.StylePriority.UseFont = false;
            this.xrLabel11.Text = "Deliverables";
            // 
            // groupFooterBand1
            // 
            this.groupFooterBand1.HeightF = 1F;
            this.groupFooterBand1.Name = "groupFooterBand1";
            // 
            // Title
            // 
            this.Title.BackColor = System.Drawing.Color.Transparent;
            this.Title.BorderColor = System.Drawing.Color.Black;
            this.Title.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.Title.BorderWidth = 1F;
            this.Title.Font = new System.Drawing.Font("Times New Roman", 24F);
            this.Title.ForeColor = System.Drawing.Color.Black;
            this.Title.Name = "Title";
            // 
            // FieldCaption
            // 
            this.FieldCaption.BackColor = System.Drawing.Color.Transparent;
            this.FieldCaption.BorderColor = System.Drawing.Color.Black;
            this.FieldCaption.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.FieldCaption.BorderWidth = 1F;
            this.FieldCaption.Font = new System.Drawing.Font("Times New Roman", 10F, System.Drawing.FontStyle.Bold);
            this.FieldCaption.ForeColor = System.Drawing.Color.Black;
            this.FieldCaption.Name = "FieldCaption";
            // 
            // PageInfo
            // 
            this.PageInfo.BackColor = System.Drawing.Color.Transparent;
            this.PageInfo.BorderColor = System.Drawing.Color.Black;
            this.PageInfo.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.PageInfo.BorderWidth = 1F;
            this.PageInfo.Font = new System.Drawing.Font("Times New Roman", 8F);
            this.PageInfo.ForeColor = System.Drawing.Color.Black;
            this.PageInfo.Name = "PageInfo";
            // 
            // DataField
            // 
            this.DataField.BackColor = System.Drawing.Color.Transparent;
            this.DataField.BorderColor = System.Drawing.Color.Black;
            this.DataField.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.DataField.BorderWidth = 1F;
            this.DataField.Font = new System.Drawing.Font("Times New Roman", 8F);
            this.DataField.ForeColor = System.Drawing.Color.Black;
            this.DataField.Name = "DataField";
            this.DataField.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            // 
            // calculatedCost
            // 
            this.calculatedCost.Expression = "[BaselineItem_TotalHours] * [BaselineItem_Rate]";
            this.calculatedCost.Name = "calculatedCost";
            // 
            // PageHeader
            // 
            this.PageHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrlblDeliverableType,
            this.xrlblTitle,
            this.xrlblNumber,
            this.xrlblHours,
            this.xrlblCosts,
            this.xrlblArea,
            this.xrlblDepartment,
            this.xrlblType});
            this.PageHeader.HeightF = 18F;
            this.PageHeader.Name = "PageHeader";
            // 
            // xrlblDeliverableType
            // 
            this.xrlblDeliverableType.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblDeliverableType.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblDeliverableType.LocationFloat = new DevExpress.Utils.PointFloat(506F, 0F);
            this.xrlblDeliverableType.Name = "xrlblDeliverableType";
            this.xrlblDeliverableType.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblDeliverableType.SizeF = new System.Drawing.SizeF(100F, 18F);
            this.xrlblDeliverableType.StylePriority.UseBorders = false;
            this.xrlblDeliverableType.StylePriority.UseFont = false;
            this.xrlblDeliverableType.Text = "Deliverable Type";
            // 
            // xrlblArea
            // 
            this.xrlblArea.Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblArea.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblArea.LocationFloat = new DevExpress.Utils.PointFloat(10F, 0F);
            this.xrlblArea.Name = "xrlblArea";
            this.xrlblArea.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblArea.SizeF = new System.Drawing.SizeF(100F, 18F);
            this.xrlblArea.StylePriority.UseBorders = false;
            this.xrlblArea.StylePriority.UseFont = false;
            this.xrlblArea.Text = "Area";
            // 
            // xrlblDepartment
            // 
            this.xrlblDepartment.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblDepartment.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblDepartment.LocationFloat = new DevExpress.Utils.PointFloat(110F, 0F);
            this.xrlblDepartment.Name = "xrlblDepartment";
            this.xrlblDepartment.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblDepartment.SizeF = new System.Drawing.SizeF(149.9999F, 18F);
            this.xrlblDepartment.StylePriority.UseBorders = false;
            this.xrlblDepartment.StylePriority.UseFont = false;
            this.xrlblDepartment.Text = "Department";
            // 
            // xrlblType
            // 
            this.xrlblType.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Top | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblType.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblType.LocationFloat = new DevExpress.Utils.PointFloat(260F, 0F);
            this.xrlblType.Name = "xrlblType";
            this.xrlblType.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblType.SizeF = new System.Drawing.SizeF(96F, 18F);
            this.xrlblType.StylePriority.UseBorders = false;
            this.xrlblType.StylePriority.UseFont = false;
            this.xrlblType.Text = "Type";
            // 
            // showHours
            // 
            this.showHours.Description = "Show Hours";
            this.showHours.Name = "showHours";
            this.showHours.Type = typeof(bool);
            this.showHours.ValueInfo = "True";
            // 
            // showCosts
            // 
            this.showCosts.Description = "Show Costs";
            this.showCosts.Name = "showCosts";
            this.showCosts.Type = typeof(bool);
            this.showCosts.ValueInfo = "True";
            // 
            // GroupFooter1
            // 
            this.GroupFooter1.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrlblTotalCosts,
            this.xrlblTotalHours});
            this.GroupFooter1.HeightF = 28.04166F;
            this.GroupFooter1.Name = "GroupFooter1";
            // 
            // xrlblTotalCosts
            // 
            this.xrlblTotalCosts.AnchorHorizontal = DevExpress.XtraReports.UI.HorizontalAnchorStyles.Left;
            this.xrlblTotalCosts.AnchorVertical = DevExpress.XtraReports.UI.VerticalAnchorStyles.Top;
            this.xrlblTotalCosts.Borders = ((DevExpress.XtraPrinting.BorderSide)((DevExpress.XtraPrinting.BorderSide.Right | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblTotalCosts.CanGrow = false;
            this.xrlblTotalCosts.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "sumSum([Total_Costs])")});
            this.xrlblTotalCosts.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblTotalCosts.LocationFloat = new DevExpress.Utils.PointFloat(905.0632F, 0F);
            this.xrlblTotalCosts.Name = "xrlblTotalCosts";
            this.xrlblTotalCosts.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblTotalCosts.SizeF = new System.Drawing.SizeF(133.9366F, 28.04166F);
            this.xrlblTotalCosts.StylePriority.UseBorders = false;
            this.xrlblTotalCosts.StylePriority.UseFont = false;
            this.xrlblTotalCosts.StylePriority.UseTextAlignment = false;
            xrSummary1.Running = DevExpress.XtraReports.UI.SummaryRunning.Report;
            this.xrlblTotalCosts.Summary = xrSummary1;
            this.xrlblTotalCosts.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleJustify;
            this.xrlblTotalCosts.TextFormatString = "{0:$0.00}";
            // 
            // xrlblTotalHours
            // 
            this.xrlblTotalHours.AnchorHorizontal = DevExpress.XtraReports.UI.HorizontalAnchorStyles.Right;
            this.xrlblTotalHours.AnchorVertical = DevExpress.XtraReports.UI.VerticalAnchorStyles.Top;
            this.xrlblTotalHours.Borders = ((DevExpress.XtraPrinting.BorderSide)(((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
            this.xrlblTotalHours.CanGrow = false;
            this.xrlblTotalHours.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "sumSum([Total_Units])")});
            this.xrlblTotalHours.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xrlblTotalHours.LocationFloat = new DevExpress.Utils.PointFloat(804.0634F, 0F);
            this.xrlblTotalHours.Name = "xrlblTotalHours";
            this.xrlblTotalHours.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrlblTotalHours.SizeF = new System.Drawing.SizeF(101F, 28.04166F);
            this.xrlblTotalHours.StylePriority.UseBorders = false;
            this.xrlblTotalHours.StylePriority.UseFont = false;
            this.xrlblTotalHours.StylePriority.UseTextAlignment = false;
            xrSummary2.Running = DevExpress.XtraReports.UI.SummaryRunning.Report;
            this.xrlblTotalHours.Summary = xrSummary2;
            this.xrlblTotalHours.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleJustify;
            // 
            // showType
            // 
            this.showType.Description = "Show Type";
            this.showType.Name = "showType";
            this.showType.Type = typeof(bool);
            this.showType.ValueInfo = "True";
            // 
            // showDeliverableType
            // 
            this.showDeliverableType.Description = "Show Deliverable Type";
            this.showDeliverableType.Name = "showDeliverableType";
            this.showDeliverableType.Type = typeof(bool);
            this.showDeliverableType.ValueInfo = "True";
            // 
            // objectDataSource1
            // 
            this.objectDataSource1.DataSource = typeof(BluePrints.Common.Projections.BASELINE_ITEMProgress);
            this.objectDataSource1.Name = "objectDataSource1";
            // 
            // XtraReportBASELINE_ITEMS
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.Detail,
            this.TopMargin,
            this.BottomMargin,
            this.pageFooterBand1,
            this.reportHeaderBand1,
            this.PageHeader,
            this.GroupFooter1});
            this.CalculatedFields.AddRange(new DevExpress.XtraReports.UI.CalculatedField[] {
            this.calculatedCost});
            this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.objectDataSource1});
            this.DataSource = this.objectDataSource1;
            this.Landscape = true;
            this.Margins = new System.Drawing.Printing.Margins(26, 25, 107, 10);
            this.PageHeight = 850;
            this.PageWidth = 1100;
            this.Parameters.AddRange(new DevExpress.XtraReports.Parameters.Parameter[] {
            this.title1,
            this.date1,
            this.showHours,
            this.showCosts,
            this.projectName,
            this.showType,
            this.showDeliverableType});
            this.Scripts.OnParametersRequestValueChanged = "rptBaselineItem_ParametersRequestValueChanged";
            this.StyleSheet.AddRange(new DevExpress.XtraReports.UI.XRControlStyle[] {
            this.Title,
            this.FieldCaption,
            this.PageInfo,
            this.DataField});
            this.Version = "18.2";
            ((System.ComponentModel.ISupportInitialize)(this.objectDataSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private DevExpress.XtraReports.UI.DetailBand Detail;
        private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        private DevExpress.XtraReports.UI.XRLabel xrlblDataNumber;
        private DevExpress.XtraReports.UI.XRLabel xrlblDataTitle;
        private DevExpress.XtraReports.UI.XRLabel xrlblDataHours;
        private DevExpress.DataAccess.ObjectBinding.ObjectDataSource objectDataSource1;
        private DevExpress.XtraReports.UI.XRLabel xrlblNumber;
        private DevExpress.XtraReports.UI.XRLabel xrlblTitle;
        private DevExpress.XtraReports.UI.XRLabel xrlblHours;
        private DevExpress.XtraReports.UI.PageFooterBand pageFooterBand1;
        private DevExpress.XtraReports.UI.XRPageInfo xrPageInfo1;
        private DevExpress.XtraReports.UI.XRPageInfo xrPageInfo2;
        private DevExpress.XtraReports.UI.ReportHeaderBand reportHeaderBand1;
        private DevExpress.XtraReports.UI.XRLabel xrLabel11;
        private DevExpress.XtraReports.UI.GroupFooterBand groupFooterBand1;
        private DevExpress.XtraReports.UI.XRControlStyle Title;
        private DevExpress.XtraReports.UI.XRControlStyle FieldCaption;
        private DevExpress.XtraReports.UI.XRControlStyle PageInfo;
        private DevExpress.XtraReports.UI.XRControlStyle DataField;
        private DevExpress.XtraReports.UI.XRLabel xrlblCosts;
        private DevExpress.XtraReports.UI.XRLabel xrlblDataCosts;
        private DevExpress.XtraReports.UI.CalculatedField calculatedCost;
        private DevExpress.XtraReports.UI.XRLabel xrlblDataArea;
        private DevExpress.XtraReports.UI.XRLabel xrlblDataDepartment;
        private DevExpress.XtraReports.UI.PageHeaderBand PageHeader;
        private DevExpress.XtraReports.UI.XRPictureBox xrPictureBox1;
        private DevExpress.XtraReports.UI.XRLabel xrLabel12;
        private DevExpress.XtraReports.UI.XRPictureBox xrPictureBox2;
        private DevExpress.XtraReports.UI.XRLabel xrLabel13;
        private DevExpress.XtraReports.UI.XRLabel xrLabel14;
        private DevExpress.XtraReports.Parameters.Parameter title1;
        private DevExpress.XtraReports.Parameters.Parameter date1;
        private DevExpress.XtraReports.UI.XRLabel xrlblArea;
        private DevExpress.XtraReports.UI.XRLabel xrlblDepartment;
        private DevExpress.XtraReports.Parameters.Parameter showHours;
        private DevExpress.XtraReports.Parameters.Parameter showCosts;
        private DevExpress.XtraReports.UI.XRLabel xrLabel6;
        private DevExpress.XtraReports.Parameters.Parameter projectName;
        private DevExpress.XtraReports.UI.GroupFooterBand GroupFooter1;
        private DevExpress.XtraReports.UI.XRLabel xrlblTotalCosts;
        private DevExpress.XtraReports.UI.XRLabel xrlblTotalHours;
        private DevExpress.XtraReports.UI.XRLabel xrlblDataType;
        private DevExpress.XtraReports.UI.XRLabel xrlblType;
        private DevExpress.XtraReports.Parameters.Parameter showType;
        private DevExpress.XtraReports.UI.XRLabel xrlblDataDeliverableType;
        private DevExpress.XtraReports.UI.XRLabel xrlblDeliverableType;
        private DevExpress.XtraReports.Parameters.Parameter showDeliverableType;
    }
}
