using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using BluePrints.Data;
using System.Collections.Generic;
using System.Linq;
using BluePrints.Common.Resources;

namespace BluePrints.Common.Reports
{
    public partial class XtraReportChangeRegisterStandalone : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportChangeRegisterStandalone()
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
            projectName.Value = string.Concat(project.NUMBER, " - ", project.NAME);
            documentName.Value = string.Concat(project.NUMBER, BluePrintsResources.Register_Change_Suffix);
            revision.Value = "A";

            if(registerChanges.Count() > 0)
            {
                //need to set false because previous report might have set it to true
                xrCheckBoxInternal.Checked = false;
                xrCheckboxClient.Checked = false;

                REGISTER_CHANGE reportRegisterChange = registerChanges.First();
                if (reportRegisterChange.IMPACT_TYPE == Register_ImpactType.Internal)
                    xrCheckBoxInternal.Checked = true;
                else if (reportRegisterChange.IMPACT_TYPE == Register_ImpactType.Variation)
                    xrCheckboxClient.Checked = true;
            }
        }
    }
}
