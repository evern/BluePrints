using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using BluePrints.Data;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.Reports
{
    public partial class XtraReportChangeRegister : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportChangeRegister()
        {
            InitializeComponent();
            this.PrintingSystem.Document.AutoFitToPagesWidth = 1;
        }

        IEnumerable<REGISTER_CHANGE> registerChanges;
        public void AssignProperties(PROJECT project, IEnumerable<REGISTER_CHANGE> registerChanges)
        {
            this.registerChanges = registerChanges;
            objectDataSource1.DataSource = registerChanges;
            this.Name = xrLabel1.Text;
            projectName.Value = project.NAME;
        }
    }
}
