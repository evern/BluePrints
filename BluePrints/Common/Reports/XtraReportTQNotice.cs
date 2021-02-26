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
	public partial class XtraReportTQNotice : DevExpress.XtraReports.UI.XtraReport
	{	
		public XtraReportTQNotice()
		{
			InitializeComponent();
		}

        IEnumerable<REGISTER_TQ> registerTQ;
        public void AssignProperties(PROJECT project, IEnumerable<REGISTER_TQ> registerTQs)
        {
            this.registerTQ = registerTQs;
            objectDataSource1.DataSource = registerTQs;
            projectName.Value = string.Concat(project.NUMBER, " - ", project.NAME);

            if (registerTQs.Count() > 0)
            {
                REGISTER_TQ registerTQ = registerTQs.First();
                documentName.Value = string.Concat(project.NUMBER, BluePrintsResources.Register_TQ_Suffix, registerTQ.NUMBER);
            }

            revision.Value = "A";

            if (registerTQs.Count() > 0)
            {
                //need to set false because previous report might have set it to true
                xrCheckBoxCommercial.Checked = false;
                xrCheckBoxSchedule.Checked = false;
                xrCheckBoxQuality.Checked = false;
                xrCheckBoxOther.Checked = false;
                xrCheckBoxAccepted.Checked = false;
                xrCheckBoxAcceptedWComments.Checked = false;
                xrCheckBoxRevise.Checked = false;

                REGISTER_TQ rptRegisterTQ = registerTQs.First();
                if (rptRegisterTQ.STATUS == RegisterTQ_Status.Accepted)
                    xrCheckBoxAccepted.Checked = true;
                else if (rptRegisterTQ.STATUS == RegisterTQ_Status.Rejected)
                    xrCheckBoxRevise.Checked = true;
            }
        }
    }
}
