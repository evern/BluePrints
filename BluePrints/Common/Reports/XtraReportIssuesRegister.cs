using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using BluePrints.Data;
using System.Collections.Generic;

namespace BluePrints.Common.Reports
{
    public partial class XtraReportIssuesRegister : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportIssuesRegister()
        {
            InitializeComponent();
        }

        public void AssignProperties(IEnumerable<REGISTER_ISSUE> issues)
        {
            objectDataSource1.DataSource = issues;
        }
    }
}
