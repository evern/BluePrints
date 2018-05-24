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
    public partial class XtraReportStudyData : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportStudyData()
        {
            InitializeComponent();
        }

        public void AssignProperties(RA_STUDY study, IEnumerable<RA_STUDY_DATA> study_datas, IEnumerable<RA_STUDY_DRAWING> study_drawings, IEnumerable<RA_STUDY_NODE> study_nodes, IEnumerable<RA_STUDY_TEAM> study_teams, IEnumerable<USER> users, PROJECT project)
        {
            RA_STUDYProjection risk_assessmentProjection = new RA_STUDYProjection(study, study_datas, study_drawings, study_nodes, study_teams, users, project);
            objectDataSource1.DataSource = risk_assessmentProjection;
        }

    }
}
