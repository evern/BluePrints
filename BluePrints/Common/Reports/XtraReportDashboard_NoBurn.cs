using BluePrints.Common.ViewModel.Reporting;
using DevExpress.Xpf.Charts;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;

namespace BluePrints.Reports
{
    public partial class XtraReportDashboard_NoBurn : XtraReport
    {
        public XtraReportDashboard_NoBurn()
        {
            InitializeComponent();
        }

        private SummaryStats ReportData { get; set; }

        public void AssignProperties(SummaryStats reportData, DateTime reportingDataDate, string title)
        {
            ReportData = reportData;
            objectDataSource1.DataSource = ReportData;
            title1.Value = title;
            datadate1.Value = reportingDataDate;
        }
    }
}