using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Collections.Generic;
using BluePrints.Data;
using BluePrints.Common.Resources;

namespace BluePrints.Common.Reports
{
    public partial class XtraReportTQRegister : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportTQRegister()
        {
            InitializeComponent();
        }

        public void AssignProperties(PROJECT project, IEnumerable<REGISTER_TQ> registerTQ)
        {
            objectDataSource1.DataSource = registerTQ;
            this.Name = xrLabel1.Text;
            projectName.Value = string.Concat(project.NUMBER, " - ", project.NAME);
            documentName.Value = string.Concat(project.NUMBER, BluePrintsResources.Register_TQ_Suffix);
        }
    }
}
