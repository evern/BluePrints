using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using BluePrints.Data;
using System.Collections.Generic;
using BluePrints.PrimeroData;

namespace BluePrints.Common.Reports
{
    public partial class XtraReportTimesheet : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportTimesheet()
        {
            InitializeComponent();
        }

        public void AssignProperties(IEnumerable<X_JOB_TIMESHEETS> TIMESHEETS)
        {
            objectDataSource1.DataSource = TIMESHEETS;
        }
    }
}
