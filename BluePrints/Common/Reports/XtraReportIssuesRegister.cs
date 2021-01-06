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
    public partial class XtraReportIssuesRegister : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportIssuesRegister()
        {
            InitializeComponent();
            this.PrintingSystem.Document.AutoFitToPagesWidth = 1;
        }

        IEnumerable<REGISTER_ISSUE> issues;
        public void AssignProperties(PROJECT project, IEnumerable<REGISTER_ISSUE> issues)
        {
            this.issues = issues;
            objectDataSource1.DataSource = issues;
            this.Name = xrLabel1.Text;
            projectName.Value = project.NAME;
        }

        private void XtraReportIssuesRegister_ParametersRequestSubmit(object sender, DevExpress.XtraReports.Parameters.ParametersRequestEventArgs e)
        {
            var inclInternal = true;
            var InclLow = true;

            foreach (var info in e.ParametersInformation)
            {
                if (info.Parameter.Name == "InclInternal")
                    inclInternal = (bool)info.Parameter.Value;
                if (info.Parameter.Name == "InclLow")
                    InclLow = (bool)info.Parameter.Value;
            }

            IEnumerable<REGISTER_ISSUE> filteredIssues = issues;
            if (!inclInternal)
                filteredIssues = filteredIssues.Where(x => x.ISSUE_TYPE != Register_IssueType.Internal);

            if (!InclLow)
                filteredIssues = filteredIssues.Where(x => x.IMPORTANCE != Register_IssueImportance.Low);

            objectDataSource1.DataSource = filteredIssues;
        }
    }
}
