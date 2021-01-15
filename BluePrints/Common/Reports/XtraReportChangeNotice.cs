using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Collections.Generic;
using BluePrints.Data;
using BluePrints.Common.Resources;
using System.Linq;

namespace BluePrints.Common.Reports
{
    public partial class XtraReportChangeNotice : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportChangeNotice()
        {
            InitializeComponent();
        }

        IEnumerable<REGISTER_CHANGE> registerChanges;
        public void AssignProperties(PROJECT project, IEnumerable<REGISTER_CHANGE> registerChanges)
        {
            this.registerChanges = registerChanges;
            objectDataSource1.DataSource = registerChanges;
            projectName.Value = string.Concat(project.NUMBER, " - ", project.NAME);

            if(registerChanges.Count() > 0)
            {
                REGISTER_CHANGE registerChange = registerChanges.First();
                documentName.Value = string.Concat(project.NUMBER, BluePrintsResources.Register_Change_Suffix, registerChange.NUMBER);
            }

            revision.Value = "A";

            if (registerChanges.Count() > 0)
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
