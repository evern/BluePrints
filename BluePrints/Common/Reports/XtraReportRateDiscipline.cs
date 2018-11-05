using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Collections.Generic;
using BluePrints.Data;
using BluePrints.Common.Projections;
using System.Data;
using System.Linq;

namespace BluePrints.Common.Reports
{
    public partial class XtraReportRateDiscipline : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportRateDiscipline()
        {
            InitializeComponent();
        }

        public void AssignProperties(PROJECT PROJECT, IEnumerable<BASELINE_ITEMProjection> BASELINE_ITEMS)
        {
            objectDataSource1.DataSource = BASELINE_ITEMS;
            title1.Value = PROJECT.NUMBER + " Costs By Discipline";
        }
    }
}
