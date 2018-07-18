using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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

        public decimal Incident_Target
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Total_Recordable_Injuries == 0 ? 0 : Total_Incidents * 0.1m;
            }
        }

        public decimal Total_ManHours
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Total_Employees * Entity.QTY_DAYSONSITE * Entity.QTY_HRSADAY;
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
                if (Entity == null || Total_ManHours == 0)
                    return 0;

                return Total_ManHours / 100000;
            }
        }

        public decimal Total_Recordable_Injuries_Freq
        {
            get
            {
                if (Entity == null || Total_ManHours == 0)
                    return 0;

                return Total_Recordable_Injuries;
            }
        }

        public decimal Total_Recordable_Injuries_Freq_Target
        {
            get
            {
                if (Entity == null || Total_ManHours == 0)
                    return 0;

                return Total_ManHours / 100000;
            }
        }


        public decimal All_Injuries
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Total_Recordable_Injuries + Entity.INJURIES_OTH_FAI;
            }
        }

        public decimal All_Injuries_Target
        {
            get
            {
                if (Entity == null || Total_ManHours == 0)
                    return 0;

                return ((Total_Recordable_Injuries + Entity.INJURIES_OTH_FAI) * 1000000) / Total_ManHours; 
            }
        }

        public decimal Total_Incidents
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Total_Recordable_Injuries + Entity.INJURIES_OTH_FAI + Entity.INJURIES_OTH_NWR + Entity.INCIDENT_DAM + Entity.INCIDENT_PDT + Entity.INCIDENT_PDT + Entity.INCIDENT_PDT + Entity.INCIDENT_HSE_BREACH + Entity.INCIDENT_NOTICE;
            }
        }

        public decimal DaysOnSite
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Math.Round(Entity.QTY_DAYSONSITE);
            }
        }

        #region Criteria
        const string attendanceTarget = ">= 90% Attendance";
        const string notApplicableTarget = "Not Applicable";
        public string KPI_PRESTART_Target
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_PRESTART_CRITERIA == 0 ? KPI_PRESTART_TargetNumber.ToString() : notApplicableTarget;
            }
        }

        public decimal KPI_PRESTART_TargetNumber => DaysOnSite;

        public string KPI_TOOLBOX_Target
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_TOOLBOX_CRITERIA == 0 ? KPI_TOOLBOX_TargetNumber.ToString() : notApplicableTarget;
            }
        }

        public decimal KPI_TOOLBOX_TargetNumber => DaysOnSite;

        public decimal KPI_HAZOB_TargetNumber
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Math.Round(Total_ManHours / 100, 0);
            }
        }

        public string KPI_HAZOB_Target
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_HAZOB_CRITERIA == 0 ? KPI_HAZOB_TargetNumber.ToString() : notApplicableTarget;
            }
        }

        public decimal KPI_SWO_TargetNumber
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Math.Round((Entity.QTY_MGMT + Entity.QTY_HSE) * Entity.QTY_DAYSONSITE, 0);
            }
        }

        public string KPI_SWO_Target
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_SWO_CRITERIA == 0 ? KPI_SWO_TargetNumber.ToString() : notApplicableTarget;
            }
        }

        public string KPI_DRILL_Target
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_DRILL_CRITERIA == 0 ? ">= 1/QTR" : notApplicableTarget;
            }
        }

        public string KPI_INSPECTION_Target
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_INSPECTION_CRITERIA == 0 ? ">= 85%" : notApplicableTarget;
            }
        }

        public decimal KPI_SUPERVISOR_PRIMER_TargetNumber
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Math.Round((Entity.QTY_MGMT * Entity.QTY_DAYSONSITE) / 6.5m, 0);
            }
        }

        public string KPI_SUPERVISOR_PRIMER_Target
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_SUPERVISOR_PRIMER_CRITERIA == 0 ? KPI_SUPERVISOR_PRIMER_TargetNumber.ToString() : notApplicableTarget;
            }
        }


        public string KPI_HSE_PRIMER_Target
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_HSE_PRIMER_CRITERIA == 0 ? KPI_HSE_PRIMER_TargetNumber.ToString() : notApplicableTarget;
            }
        }

        public decimal KPI_HSE_PRIMER_TargetNumber
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Math.Round((Entity.QTY_HSE * Entity.QTY_DAYSONSITE), 0);
            }
        }

        public decimal KPI_CORRECTIVE_ACT_TargetNumber
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Math.Round(Total_Recordable_Injuries + Entity.INJURIES_OTH_FAI + Entity.INCIDENT_DAM + Entity.INCIDENT_PDT + Entity.INCIDENT_PDT + Entity.INCIDENT_PDT + Entity.INCIDENT_HSE_BREACH + Entity.INCIDENT_NOTICE + Entity.KPI_HAZOB, 0);
            }
        }
        
        public string KPI_CORRECTIVE_ACT_Target
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_CORRECTIVE_ACT_CRITERIA == 0 ? KPI_CORRECTIVE_ACT_TargetNumber.ToString() : notApplicableTarget;
            }
        }

        public decimal KPI_CORRECTIVE_ACT_CLOSED_TargetNumber
        {
            get
            {
                if (Entity == null)
                    return 0;

                return Math.Round(Entity.KPI_CORRECTIVE_ACT * 0.85m, 0);
            }
        }

        public string KPI_CORRECTIVE_ACT_CLOSED_Target
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_CORRECTIVE_ACT_CLOSED_CRITERIA == 0 ? KPI_CORRECTIVE_ACT_CLOSED_TargetNumber.ToString() : notApplicableTarget;
            }
        }

        public decimal KPI_HSE_RECOGNITION_TargetNumber
        {
            get
            {
                if (Entity == null)
                    return 0;

                return 1;
            }
        }

        public string KPI_HSE_RECOGNITION_Target
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_HSE_RECOGNITION_CRITERIA == 0 ? KPI_HSE_RECOGNITION_TargetNumber.ToString() : notApplicableTarget;
            }
        }

        public string KPI_RISK_REGISTER_Target
        {
            get
            {
                if (Entity == null)
                    return string.Empty;

                return Entity.KPI_RISK_REGISTER_CRITERIA == 0 ? ">= 1/Month" : notApplicableTarget;
            }
        }
        #endregion


        #region Conditional Formatting
        public bool INJURIES_REC_LTI_Format => Entity == null ? false : Entity.INJURIES_REC_LTI == 0 ? true : false;
        public bool INJURIES_REC_RWI_Format => Entity == null ? false : Entity.INJURIES_REC_RWI == 0 ? true : false;
        public bool INJURIES_REC_MTI_Format => Entity == null ? false : Entity.INJURIES_REC_MTI == 0 ? true : false;
        public bool Total_Recordable_Injuries_Format => Entity == null ? false : Total_Recordable_Injuries <= Total_Recordable_Injuries_Target ? true : false;
        public bool Total_Recordable_Injuries_Freq_Format => Entity == null ? false : Total_Recordable_Injuries_Freq <= Total_Recordable_Injuries_Freq_Target ? true : false;
        public bool All_Injuries_Format => Entity == null ? false : All_Injuries <= All_Injuries_Target ? true : false;

        public bool INCIDENT_ENV_Format => Entity == null ? false : Entity.INCIDENT_ENV <= Incident_Target ? true : false;
        public bool INCIDENT_DAM_Format => Entity == null ? false : Entity.INCIDENT_DAM <= Incident_Target ? true : false;
        public bool INCIDENT_PDT_Format => Entity == null ? false : Entity.INCIDENT_PDT <= Incident_Target ? true : false;
        public bool INCIDENT_BAC_Format => Entity == null ? false : Entity.INCIDENT_BAC <= Incident_Target ? true : false;
        public bool INCIDENT_HSE_BREACH_Format => Entity == null ? false : Entity.INCIDENT_HSE_BREACH <= Incident_Target ? true : false;
        public bool INCIDENT_NOTICE_Format => Entity == null ? false : Entity.INCIDENT_NOTICE == 0 ? true : false;
        public bool KPI_PRESTART_Format => Entity == null ? false : Entity.KPI_PRESTART_CRITERIA == 1 ? true : Entity.KPI_PRESTART >= KPI_PRESTART_TargetNumber ? true : false;
        public bool KPI_TOOLBOX_Format => Entity == null ? false : Entity.KPI_TOOLBOX_CRITERIA == 1 ? true : Entity.KPI_TOOLBOX >= KPI_TOOLBOX_TargetNumber ? true : false;
        public bool KPI_HAZOB_Format => Entity == null ? false : Entity.KPI_HAZOB_CRITERIA == 1 ? true : Entity.KPI_HAZOB >= KPI_HAZOB_TargetNumber ? true : false;
        public bool KPI_SWO_Format => Entity == null ? false : Entity.KPI_SWO_CRITERIA == 1 ? true : Entity.KPI_SWO >= KPI_SWO_TargetNumber ? true : false;
        public bool KPI_DRILL_Format => Entity == null ? false : Entity.KPI_DRILL_CRITERIA == 1 ? true : Entity.KPI_DRILL >= 1 ? true : false;
        public bool KPI_INSPECTION_Format => Entity == null ? false : Entity.KPI_INSPECTION_CRITERIA == 1 ? true : Entity.KPI_INSPECTION >= 0.85m ? true : false;
        public bool KPI_SUPERVISOR_PRIMER_Format => Entity == null ? false : Entity.KPI_SUPERVISOR_PRIMER_CRITERIA == 1 ? true : Entity.KPI_SUPERVISOR_PRIMER >= KPI_SUPERVISOR_PRIMER_TargetNumber ? true : false;
        public bool KPI_HSE_PRIMER_Format => Entity == null ? false : Entity.KPI_HSE_PRIMER_CRITERIA == 1 ? true : Entity.KPI_HSE_PRIMER >= KPI_HSE_PRIMER_TargetNumber ? true : false;
        public bool KPI_CORRECTIVE_ACT_Format => Entity == null ? false : Entity.KPI_CORRECTIVE_ACT_CRITERIA == 1 ? true : Entity.KPI_CORRECTIVE_ACT >= KPI_CORRECTIVE_ACT_TargetNumber ? true : false;
        public bool KPI_CORRECTIVE_ACT_CLOSED_Format => Entity == null ? false : Entity.KPI_CORRECTIVE_ACT_CLOSED_CRITERIA == 1 ? true : Entity.KPI_CORRECTIVE_ACT_CLOSED >= KPI_CORRECTIVE_ACT_CLOSED_TargetNumber ? true : false;
        public bool KPI_HSE_RECOGNITION_Format => Entity == null ? false : Entity.KPI_HSE_RECOGNITION_CRITERIA == 1 ? true : Entity.KPI_HSE_RECOGNITION >= KPI_HSE_RECOGNITION_TargetNumber ? true : false;
        public bool KPI_RISK_REGISTER_Format => Entity == null ? false : Entity.KPI_RISK_REGISTER_CRITERIA == 1 ? true : Entity.KPI_RISK_REGISTER >= 1 ? true : false;


        public SolidColorBrush INJURIES_REC_LTI_Background => INJURIES_REC_LTI_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush INJURIES_REC_RWI_Background => INJURIES_REC_RWI_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush INJURIES_REC_MTI_Background => INJURIES_REC_MTI_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush Total_Recordable_Injuries_Background => Total_Recordable_Injuries_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush Total_Recordable_Injuries_Freq_Background => Total_Recordable_Injuries_Freq_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush All_Injuries_Background => All_Injuries_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush INCIDENT_ENV_Background => INCIDENT_ENV_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush INCIDENT_DAM_Background => INCIDENT_DAM_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush INCIDENT_PDT_Background => INCIDENT_PDT_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush INCIDENT_BAC_Background => INCIDENT_BAC_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush INCIDENT_HSE_BREACH_Background => INCIDENT_HSE_BREACH_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush INCIDENT_NOTICE_Background => INCIDENT_NOTICE_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush KPI_PRESTART_Background => KPI_PRESTART_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush KPI_TOOLBOX_Background => KPI_TOOLBOX_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush KPI_HAZOB_Background => KPI_HAZOB_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush KPI_SWO_Background => KPI_SWO_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush KPI_DRILL_Background => KPI_DRILL_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush KPI_INSPECTION_Background => KPI_INSPECTION_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush KPI_SUPERVISOR_PRIMER_Background => KPI_SUPERVISOR_PRIMER_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush KPI_HSE_PRIMER_Background => KPI_HSE_PRIMER_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush KPI_CORRECTIVE_ACT_Background => KPI_CORRECTIVE_ACT_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush KPI_CORRECTIVE_ACT_CLOSED_Background => KPI_CORRECTIVE_ACT_CLOSED_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush KPI_HSE_RECOGNITION_Background => KPI_HSE_RECOGNITION_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);
        public SolidColorBrush KPI_RISK_REGISTER_Background => KPI_RISK_REGISTER_Format ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightSalmon);

        #endregion
    }
}
