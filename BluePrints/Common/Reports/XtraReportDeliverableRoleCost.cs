using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using BluePrints.ViewModels;
using System.Collections.Generic;

namespace BluePrints.Common.Reports
{
    public partial class XtraReportDeliverableRoleCost : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportDeliverableRoleCost()
        {
            InitializeComponent();
        }

        public void AssignProperties(IEnumerable<DeliverableRoleCost> roleCosts, string title)
        {
            objectDataSource1.DataSource = roleCosts;
            title1.Value = title;
        }
    }
}
