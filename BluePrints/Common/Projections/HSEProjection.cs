using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class HSEProjection : BluePrintsProjectionBase<HSE> 
    {

        public decimal Total_Employees
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Entity.QTY_STAFF + Entity.QTY_MGMT + Entity.QTY_HSE + Entity.QTY_CONTRACTOR;
            }
        }

        public decimal Total_Recordable_Injuries
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Entity.INJURIES_REC_LTI + Entity.INJURIES_REC_MTI + Entity.INJURIES_REC_RWI;
            }
        }

        public decimal Total_Recordable_Injuries_Target
        {
            get
            {
                if (Entity == null || Total_Employees == 0)
                    return 0;

                return Total_Employees / 1000000;
            }
        }

        public decimal Total_Incidents
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Total_Recordable_Injuries + Entity.INJURIES_OTH_FAI + Entity.INJURIES_OTH_NWR + Entity.INCIDENT_DAM + Entity.INCIDENT_ENV + Entity.INCIDENT_FIRE + Entity.INCIDENT_MAJOR_ENV + Entity.INCIDENT_HSE_BREACH + Entity.INCIDENT_NOTICE;
            }
        }

        #region Criteria
        const string attendanceTarget = ">= 90% Attendance";
        const string notApplicableTarget = "Not Applicable";
        public string PrestartAttendanceTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_PRESTART_CRITERIA == 0 ? attendanceTarget : notApplicableTarget;
            }
        }

        public string ToolboxAttendanceTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_TOOLBOX_CRITERIA == 0 ? attendanceTarget : notApplicableTarget;
            }
        }

        public string HSEAttendanceTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_HSE_COMMITTEE_CRITERIA == 0 ? attendanceTarget : notApplicableTarget;
            }
        }

        public string HAZOBTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_HAZOB_CRITERIA == 0 ? ">= 1/100 Manhours" : notApplicableTarget;
            }
        }

        public string SWOTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_SWO_CRITERIA == 0 ? ">= 1 per Day for each Mgr/HSE Advisor" : notApplicableTarget;
            }
        }

        public string DrillTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_DRILL_CRITERIA == 0 ? ">= 1/QTR" : notApplicableTarget;
            }
        }

        public string SixMonthlyInspectionPerformanceTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_INSPECTION_CRITERIA == 0 ? ">= 85%" : notApplicableTarget;
            }
        }

        public string WorkplaceInspectionFrequencyTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                decimal frequencyTarget = ((Entity.QTY_MGMT * Entity.QTY_DAYSONSITE) / 6.5m) + (Entity.QTY_HSE * Entity.QTY_DAYSONSITE);
                return Entity.KPI_INSPECTION_FREQ_CRITERIA == 0 ? frequencyTarget.ToString() : notApplicableTarget;
            }
        }

        public string CorrectiveActionsTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_CORRECTIVE_ACT_CRITERIA == 0 ? ">= 1 per HAZOB or Incident" : notApplicableTarget;
            }
        }

        public string CorrectiveActionsClosedTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_CORRECTIVE_ACT_CLOSED_CRITERIA == 0 ? ">= 85% per Month" : notApplicableTarget;
            }
        }

        public string HSEInspectionWithClientTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_WEEKLY_HSE_CRITERIA == 0 ? ">= 1/Week" : notApplicableTarget;
            }
        }

        public string RiskRegisterReviewTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_RISK_REGISTER_CRITERIA == 0 ? ">= 1/Month" : notApplicableTarget;
            }
        }

        public string TrainingTarget
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.TRAIN_COMPLIANCE_CRITERIA == 0 ? ">= 1/Month (or >= 1/Project)" : notApplicableTarget;
            }
        }
        #endregion
    }
}
