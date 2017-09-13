using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Collections.Generic;
using BluePrints.Data;
using BluePrints.Common.Projections;

namespace BluePrints.Common.Reports
{
    public partial class XtraReportMeeting_Minute : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportMeeting_Minute()
        {
            InitializeComponent();
        }

        public void AssignProperties(MEETING meeting, List<MINUTE_TITLE> minute_titles, IEnumerable<MeetingUser> meeting_users, IEnumerable<MINUTE_AGENDAMasterDetailProjection> minute_agendas)
        {
            MEETING_MINUTEProjection meeting_minuteProjection = new MEETING_MINUTEProjection(meeting, minute_titles, meeting_users, minute_agendas);
            objectDataSource1.DataSource = meeting_minuteProjection;
        }
    }
}
