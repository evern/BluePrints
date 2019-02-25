using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using BluePrints.Common.Projections;
using System.Collections.Generic;

namespace BluePrints.Common.Reports
{
    public partial class XtraReportProjectSummary : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportProjectSummary()
        {
            InitializeComponent();
        }

        public void AssignProperties(IEnumerable<ProjectSummary> summary, string projectNumber, DateTime? dataDate)
        {
            objectDataSource1.DataSource = summary;
            Title1.Value = projectNumber + " Summary";
            DataDate1.Value = dataDate;
        }
    }
}
