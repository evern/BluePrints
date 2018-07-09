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
    public class HSEReportProjection : BluePrintsProjectionBase<HSE> 
    {
        public PROJECT Project { get; set; }
        public DateTime HSEDate { get; set; }
        public string Group { get; set; }
        public string StatsName { get; set; }
        public decimal StatsValue { get; set; }
        public decimal StatsTarget { get; set; }
        public bool StatsFormat { get; set; }
        public string StatsCriteria { get; set; }
        public string StatsComments { get; set; }
        public string StatsMask { get; set; }
        public HSEStatsType StatsType { get; set; }
    }

    public enum HSEStatsType
    {
        INJURIES_REC_LTI,
        INJURIES_REC_RWI,
        INJURIES_REC_MTI,
        INJURIES_OTH_FAI,
        INJURIES_OTH_NWR,
        Total_Recordable_Injuries,
        INCIDENT_DAM,
        INCIDENT_ENV,
        INCIDENT_FIRE,
        INCIDENT_MAJOR_ENV,
        INCIDENT_HSE_BREACH,
        INCIDENT_NOTICE,
        Total_Incidents,
        KPI_NM,
        KPI_PRESTART,
        KPI_TOOLBOX,
        KPI_HSE_COMMITTEE,
        KPI_HAZOB,
        KPI_SWO,
        KPI_TAKE5,
        KPI_DRILL,
        KPI_INSPECTION,
        KPI_INSPECTION_FREQ,
        KPI_CORRECTIVE_ACT,
        KPI_CORRECTIVE_ACT_CLOSED,
        KPI_WEEKLY_HSE,
        KPI_RISK_REGISTER,
        TRAIN_COMPLIANCE,
        TRAIN_VOC,
        Total_ManHours
    }

    public class StaticHSEReportProjection
    {
        readonly HSEProjection hseProjection;
        public StaticHSEReportProjection(HSEProjection hseProjection)
        {
            this.hseProjection = hseProjection;
        }

        public IEnumerable<HSEReportProjection> SummarizeStats()
        {
            List<HSEReportProjection> Stats = new List<HSEReportProjection>();
            string group1 = "1. RECORDABLE INJURIES";
            string group2 = "2. OTHER INJURIES";
            string group3 = "3. NON INJURY INCIDENTS";
            string group4 = "4. HSE KPI's";
            string group5 = "5. HSE TRAINING";
            string group6 = "6. TOTALS";

            HSEReportProjection INJURIES_REC_LTI = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group1 };
            INJURIES_REC_LTI.StatsName = "1.1 Lost Time Injuries (LTI)";
            INJURIES_REC_LTI.StatsValue = hseProjection.Entity.INJURIES_REC_LTI;
            INJURIES_REC_LTI.StatsTarget = 0;
            INJURIES_REC_LTI.StatsCriteria = "= Zero";
            INJURIES_REC_LTI.StatsFormat = hseProjection.INJURIES_REC_LTI_Format;
            INJURIES_REC_LTI.StatsMask = "N0";
            INJURIES_REC_LTI.StatsType = HSEStatsType.INJURIES_REC_LTI;
            Stats.Add(INJURIES_REC_LTI);

            HSEReportProjection INJURIES_REC_RWI = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group1 };
            INJURIES_REC_RWI.StatsName = "1.2 Restricted Work Injuries (RWI)";
            INJURIES_REC_RWI.StatsValue = hseProjection.Entity.INJURIES_REC_RWI;
            INJURIES_REC_RWI.StatsTarget = 0;
            INJURIES_REC_RWI.StatsCriteria = "= Zero";
            INJURIES_REC_RWI.StatsFormat = hseProjection.INJURIES_REC_RWI_Format;
            INJURIES_REC_RWI.StatsMask = "N0";
            INJURIES_REC_RWI.StatsType = HSEStatsType.INJURIES_REC_RWI;
            Stats.Add(INJURIES_REC_RWI);

            HSEReportProjection INJURIES_REC_MTI = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group1 };
            INJURIES_REC_MTI.StatsName = "1.3 Medical Treatment Injuries (MTI)";
            INJURIES_REC_MTI.StatsValue = hseProjection.Entity.INJURIES_REC_MTI;
            INJURIES_REC_MTI.StatsTarget = 0;
            INJURIES_REC_MTI.StatsCriteria = "= Zero";
            INJURIES_REC_MTI.StatsFormat = hseProjection.INJURIES_REC_MTI_Format;
            INJURIES_REC_MTI.StatsMask = "N0";
            INJURIES_REC_MTI.StatsType = HSEStatsType.INJURIES_REC_MTI;
            Stats.Add(INJURIES_REC_MTI);

            HSEReportProjection Total_Recordable_Injuries = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group1 };
            Total_Recordable_Injuries.StatsName = "1.4 Total Recordable Injuries (TRI)";
            Total_Recordable_Injuries.StatsValue = hseProjection.Total_Recordable_Injuries;
            Total_Recordable_Injuries.StatsTarget = hseProjection.Total_Recordable_Injuries_Target;
            Total_Recordable_Injuries.StatsCriteria = "< 10 Per Million Manhours";
            Total_Recordable_Injuries.StatsFormat = hseProjection.Total_Recordable_Injuries_Format;
            Total_Recordable_Injuries.StatsMask = "N0";
            Total_Recordable_Injuries.StatsType = HSEStatsType.Total_Recordable_Injuries;
            Stats.Add(Total_Recordable_Injuries);

            HSEReportProjection INJURIES_OTH_FAI = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group2 };
            INJURIES_OTH_FAI.StatsName = "2.1 First Aid (or no-treatment required) Injuries (FAI)";
            INJURIES_OTH_FAI.StatsValue = hseProjection.Entity.INJURIES_OTH_FAI;
            INJURIES_OTH_FAI.StatsTarget = 0;
            INJURIES_OTH_FAI.StatsCriteria = "";
            INJURIES_OTH_FAI.StatsFormat = true;
            INJURIES_OTH_FAI.StatsMask = "N0";
            INJURIES_OTH_FAI.StatsType = HSEStatsType.INJURIES_OTH_FAI;
            Stats.Add(INJURIES_OTH_FAI);

            HSEReportProjection INJURIES_OTH_NWR = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group2 };
            INJURIES_OTH_NWR.StatsName = "2.2 Non-Work Related Injuries or Illness (NWR)";
            INJURIES_OTH_NWR.StatsValue = hseProjection.Entity.INJURIES_OTH_NWR;
            INJURIES_OTH_NWR.StatsTarget = 0;
            INJURIES_OTH_NWR.StatsCriteria = "";
            INJURIES_OTH_NWR.StatsFormat = true;
            INJURIES_OTH_NWR.StatsMask = "N0";
            INJURIES_OTH_NWR.StatsType = HSEStatsType.INJURIES_OTH_NWR;
            Stats.Add(INJURIES_OTH_NWR);

            HSEReportProjection INCIDENT_DAM = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group3 };
            INCIDENT_DAM.StatsName = "3.1 Property, Plant, Equipment Damage (DAM)";
            INCIDENT_DAM.StatsValue = hseProjection.Entity.INCIDENT_DAM;
            INCIDENT_DAM.StatsTarget = hseProjection.Incident_Target;
            INCIDENT_DAM.StatsCriteria = "< 10% of Incidents";
            INCIDENT_DAM.StatsFormat = hseProjection.INCIDENT_DAM_Format;
            INCIDENT_DAM.StatsMask = "N0";
            INCIDENT_DAM.StatsType = HSEStatsType.INCIDENT_DAM;
            Stats.Add(INCIDENT_DAM);

            HSEReportProjection INCIDENT_ENV = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group3 };
            INCIDENT_ENV.StatsName = "3.2 Environmental Spill, Damage (ENV)";
            INCIDENT_ENV.StatsValue = hseProjection.Entity.INCIDENT_ENV;
            INCIDENT_ENV.StatsTarget = hseProjection.Incident_Target;
            INCIDENT_ENV.StatsCriteria = "< 10% of Incidents";
            INCIDENT_ENV.StatsFormat = hseProjection.INCIDENT_ENV_Format;
            INCIDENT_ENV.StatsMask = "N0";
            INCIDENT_ENV.StatsType = HSEStatsType.INCIDENT_ENV;
            Stats.Add(INCIDENT_ENV);

            HSEReportProjection INCIDENT_FIRE = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group3 };
            INCIDENT_FIRE.StatsName = "3.3 Major Fire (FIRE)";
            INCIDENT_FIRE.StatsValue = hseProjection.Entity.INCIDENT_FIRE;
            INCIDENT_FIRE.StatsTarget = 0;
            INCIDENT_FIRE.StatsCriteria = "= Zero";
            INCIDENT_FIRE.StatsFormat = hseProjection.INCIDENT_ENV_Format;
            INCIDENT_FIRE.StatsMask = "N0";
            INCIDENT_FIRE.StatsType = HSEStatsType.INCIDENT_FIRE;
            Stats.Add(INCIDENT_FIRE);

            HSEReportProjection INCIDENT_MAJOR_ENV = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group3 };
            INCIDENT_MAJOR_ENV.StatsName = "3.4 Major Environmental Incident";
            INCIDENT_MAJOR_ENV.StatsValue = hseProjection.Entity.INCIDENT_MAJOR_ENV;
            INCIDENT_MAJOR_ENV.StatsTarget = 0;
            INCIDENT_MAJOR_ENV.StatsCriteria = "= Zero";
            INCIDENT_MAJOR_ENV.StatsFormat = hseProjection.MAJOR_ENV_Format;
            INCIDENT_MAJOR_ENV.StatsMask = "N0";
            INCIDENT_MAJOR_ENV.StatsType = HSEStatsType.INCIDENT_MAJOR_ENV;
            Stats.Add(INCIDENT_MAJOR_ENV);

            HSEReportProjection INCIDENT_HSE_BREACH = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group3 };
            INCIDENT_HSE_BREACH.StatsName = "3.5 HSE Breaches";
            INCIDENT_HSE_BREACH.StatsValue = hseProjection.Entity.INCIDENT_HSE_BREACH;
            INCIDENT_HSE_BREACH.StatsTarget = 0;
            INCIDENT_HSE_BREACH.StatsCriteria = "";
            INCIDENT_HSE_BREACH.StatsFormat = true;
            INCIDENT_HSE_BREACH.StatsMask = "N0";
            INCIDENT_HSE_BREACH.StatsType = HSEStatsType.INCIDENT_HSE_BREACH;
            Stats.Add(INCIDENT_HSE_BREACH);

            HSEReportProjection INCIDENT_NOTICE = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group3 };
            INCIDENT_NOTICE.StatsName = "3.6 (Prohibition) Notices issued by authorities";
            INCIDENT_NOTICE.StatsValue = hseProjection.Entity.INCIDENT_NOTICE;
            INCIDENT_NOTICE.StatsTarget = 0;
            INCIDENT_NOTICE.StatsCriteria = "= Zero";
            INCIDENT_NOTICE.StatsFormat = hseProjection.INCIDENT_NOTICE_Format;
            INCIDENT_NOTICE.StatsMask = "N0";
            INCIDENT_NOTICE.StatsType = HSEStatsType.INCIDENT_NOTICE;
            Stats.Add(INCIDENT_NOTICE);

            HSEReportProjection Total_Incidents = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group3 };
            Total_Incidents.StatsName = "3.7 Total Incidents";
            Total_Incidents.StatsValue = hseProjection.Total_Incidents;
            Total_Incidents.StatsTarget = 0;
            Total_Incidents.StatsCriteria = "";
            Total_Incidents.StatsFormat = true;
            Total_Incidents.StatsMask = "N0";
            Total_Incidents.StatsType = HSEStatsType.Total_Incidents;
            Stats.Add(Total_Incidents);

            HSEReportProjection KPI_NM = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_NM.StatsName = "4.01 Near Miss (NM)";
            KPI_NM.StatsValue = hseProjection.Entity.KPI_NM;
            KPI_NM.StatsTarget = 0;
            KPI_NM.StatsCriteria = "";
            KPI_NM.StatsFormat = true;
            KPI_NM.StatsMask = "N0";
            KPI_NM.StatsType = HSEStatsType.KPI_NM;
            Stats.Add(KPI_NM);

            HSEReportProjection KPI_PRESTART = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_PRESTART.StatsName = "4.02 Daily Pre-Start Meetings";
            KPI_PRESTART.StatsValue = hseProjection.Entity.KPI_PRESTART;
            KPI_PRESTART.StatsTarget = hseProjection.Entity.KPI_PRESTART_CRITERIA == 1 ? 0 : 0.9m;
            KPI_PRESTART.StatsCriteria = hseProjection.Entity.KPI_PRESTART_CRITERIA == 1 ? "Not Applicable" : ">= 90% Attendance";
            KPI_PRESTART.StatsFormat = hseProjection.KPI_PRESTART_Format;
            KPI_PRESTART.StatsMask = "P0";
            KPI_PRESTART.StatsType = HSEStatsType.KPI_PRESTART;
            Stats.Add(KPI_PRESTART);

            HSEReportProjection KPI_TOOLBOX = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_TOOLBOX.StatsName = "4.03 Weekly Toolbox Meetings";
            KPI_TOOLBOX.StatsValue = hseProjection.Entity.KPI_TOOLBOX;
            KPI_TOOLBOX.StatsTarget = hseProjection.Entity.KPI_TOOLBOX_CRITERIA == 1 ? 0 : 0.9m;
            KPI_TOOLBOX.StatsCriteria = hseProjection.Entity.KPI_TOOLBOX_CRITERIA == 1 ? "Not Applicable" : ">= 90% Attendance";
            KPI_TOOLBOX.StatsFormat = hseProjection.KPI_TOOLBOX_Format;
            KPI_TOOLBOX.StatsMask = "P0";
            KPI_TOOLBOX.StatsType = HSEStatsType.KPI_TOOLBOX;
            Stats.Add(KPI_TOOLBOX);

            HSEReportProjection KPI_HSE_COMMITTEE = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_HSE_COMMITTEE.StatsName = "4.04 HSE Committee Meetings (if >100 personnel)";
            KPI_HSE_COMMITTEE.StatsValue = hseProjection.Entity.KPI_HSE_COMMITTEE;
            KPI_HSE_COMMITTEE.StatsTarget = hseProjection.Entity.KPI_HSE_COMMITTEE_CRITERIA == 1 ? 0 : 0.9m;
            KPI_HSE_COMMITTEE.StatsCriteria = hseProjection.Entity.KPI_HSE_COMMITTEE_CRITERIA == 1 ? "Not Applicable" : ">= 90% Attendance";
            KPI_HSE_COMMITTEE.StatsFormat = hseProjection.KPI_HSE_COMMITTEE_Format;
            KPI_HSE_COMMITTEE.StatsMask = "P0";
            KPI_HSE_COMMITTEE.StatsType = HSEStatsType.KPI_HSE_COMMITTEE;
            Stats.Add(KPI_HSE_COMMITTEE);

            HSEReportProjection KPI_HAZOB = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_HAZOB.StatsName = "4.05 Hazard Observations (HAZOB)";
            KPI_HAZOB.StatsValue = hseProjection.Entity.KPI_HAZOB;
            KPI_HAZOB.StatsTarget = hseProjection.Entity.KPI_HAZOB_CRITERIA == 1 ? 0 : hseProjection.KPI_HAZOB_TargetNumber;
            KPI_HAZOB.StatsCriteria = hseProjection.Entity.KPI_HAZOB_CRITERIA == 1 ? "Not Applicable" : ">= 1/100 Manhours";
            KPI_HAZOB.StatsFormat = hseProjection.KPI_HAZOB_Format;
            KPI_HAZOB.StatsMask = "N0";
            KPI_HAZOB.StatsType = HSEStatsType.KPI_HAZOB;
            Stats.Add(KPI_HAZOB);

            HSEReportProjection KPI_SWO = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_SWO.StatsName = "4.06 Safe Work Observations (SWO)";
            KPI_SWO.StatsValue = hseProjection.Entity.KPI_SWO;
            KPI_SWO.StatsTarget = hseProjection.Entity.KPI_SWO_CRITERIA == 1 ? 0 : hseProjection.KPI_SWO_TargetNumber;
            KPI_SWO.StatsCriteria = hseProjection.Entity.KPI_SWO_CRITERIA == 1 ? "Not Applicable" : ">= 1 per Day for each Mgr/HSE Advisor";
            KPI_SWO.StatsFormat = hseProjection.KPI_SWO_Format;
            KPI_SWO.StatsMask = "N0";
            KPI_SWO.StatsType = HSEStatsType.KPI_SWO;
            Stats.Add(KPI_SWO);

            HSEReportProjection KPI_TAKE5 = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_TAKE5.StatsName = "4.07 Take 5 (or job start card)";
            KPI_TAKE5.StatsValue = hseProjection.Entity.KPI_TAKE5;
            KPI_TAKE5.StatsTarget = 0;
            KPI_TAKE5.StatsCriteria = "";
            KPI_TAKE5.StatsFormat = true;
            KPI_TAKE5.StatsMask = "N0";
            KPI_TAKE5.StatsType = HSEStatsType.KPI_TAKE5;
            Stats.Add(KPI_TAKE5);

            HSEReportProjection KPI_DRILL = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_DRILL.StatsName = "4.08 Emergency Response Drills";
            KPI_DRILL.StatsValue = hseProjection.Entity.KPI_DRILL;
            KPI_DRILL.StatsTarget = hseProjection.Entity.KPI_DRILL_CRITERIA == 1 ? 0 : 1;
            KPI_DRILL.StatsCriteria = hseProjection.Entity.KPI_DRILL_CRITERIA == 1 ? "Not Applicable" : ">= 1/QTR";
            KPI_DRILL.StatsFormat = hseProjection.KPI_DRILL_Format;
            KPI_DRILL.StatsMask = "N0";
            KPI_DRILL.StatsType = HSEStatsType.KPI_DRILL;
            Stats.Add(KPI_DRILL);

            HSEReportProjection KPI_INSPECTION = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_INSPECTION.StatsName = "4.09 Six Monthly Workplace Inspection Performance";
            KPI_INSPECTION.StatsValue = hseProjection.Entity.KPI_INSPECTION;
            KPI_INSPECTION.StatsTarget = hseProjection.Entity.KPI_INSPECTION_CRITERIA == 1 ? 0 : 0.85m;
            KPI_INSPECTION.StatsCriteria = hseProjection.Entity.KPI_INSPECTION_CRITERIA == 1 ? "Not Applicable" : ">= 85%";
            KPI_INSPECTION.StatsFormat = hseProjection.KPI_INSPECTION_Format;
            KPI_INSPECTION.StatsMask = "P0";
            KPI_INSPECTION.StatsType = HSEStatsType.KPI_INSPECTION;
            Stats.Add(KPI_INSPECTION);

            HSEReportProjection KPI_INSPECTION_FREQ = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_INSPECTION_FREQ.StatsName = "4.10 Workplace Inspection Frequency (per site)";
            KPI_INSPECTION_FREQ.StatsValue = hseProjection.Entity.KPI_INSPECTION_FREQ;
            KPI_INSPECTION_FREQ.StatsTarget = hseProjection.Entity.KPI_INSPECTION_FREQ_CRITERIA == 1 ? 0 : hseProjection.KPI_INSPECTION_FREQ_TargetNumber;
            KPI_INSPECTION_FREQ.StatsCriteria = hseProjection.Entity.KPI_INSPECTION_FREQ_CRITERIA == 1 ? "Not Applicable" : ">= HSE Advisor:1/Day|Mgrs:1/Wk";
            KPI_INSPECTION_FREQ.StatsFormat = hseProjection.KPI_INSPECTION_FREQ_Format;
            KPI_INSPECTION_FREQ.StatsMask = "N0";
            KPI_INSPECTION_FREQ.StatsType = HSEStatsType.KPI_INSPECTION_FREQ;
            Stats.Add(KPI_INSPECTION_FREQ);

            HSEReportProjection KPI_CORRECTIVE_ACT = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_CORRECTIVE_ACT.StatsName = "4.11 Corrective actions recorded";
            KPI_CORRECTIVE_ACT.StatsValue = hseProjection.Entity.KPI_CORRECTIVE_ACT;
            KPI_CORRECTIVE_ACT.StatsTarget = hseProjection.Entity.KPI_CORRECTIVE_ACT_CRITERIA == 1 ? 0 : hseProjection.KPI_CORRECTIVE_ACT_TargetNumber;
            KPI_CORRECTIVE_ACT.StatsCriteria = hseProjection.Entity.KPI_CORRECTIVE_ACT_CRITERIA == 1 ? "Not Applicable" : ">= 1 per HAZOB or Incident";
            KPI_CORRECTIVE_ACT.StatsFormat = hseProjection.KPI_INSPECTION_FREQ_Format;
            KPI_CORRECTIVE_ACT.StatsMask = "N0";
            KPI_CORRECTIVE_ACT.StatsType = HSEStatsType.KPI_CORRECTIVE_ACT;
            Stats.Add(KPI_CORRECTIVE_ACT);

            HSEReportProjection KPI_CORRECTIVE_ACT_CLOSED = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_CORRECTIVE_ACT_CLOSED.StatsName = "4.12 Corrective actions closed out";
            KPI_CORRECTIVE_ACT_CLOSED.StatsValue = hseProjection.Entity.KPI_CORRECTIVE_ACT_CLOSED;
            KPI_CORRECTIVE_ACT_CLOSED.StatsTarget = hseProjection.Entity.KPI_CORRECTIVE_ACT_CLOSED_CRITERIA == 1 ? 0 : hseProjection.KPI_CORRECTIVE_ACT_CLOSED_TargetNumber;
            KPI_CORRECTIVE_ACT_CLOSED.StatsCriteria = hseProjection.Entity.KPI_CORRECTIVE_ACT_CLOSED_CRITERIA == 1 ? "Not Applicable" : ">= 85% per Month";
            KPI_CORRECTIVE_ACT_CLOSED.StatsFormat = hseProjection.KPI_CORRECTIVE_ACT_CLOSED_Format;
            KPI_CORRECTIVE_ACT_CLOSED.StatsMask = "N0";
            KPI_CORRECTIVE_ACT_CLOSED.StatsType = HSEStatsType.KPI_CORRECTIVE_ACT_CLOSED;
            Stats.Add(KPI_CORRECTIVE_ACT_CLOSED);

            HSEReportProjection KPI_WEEKLY_HSE = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_WEEKLY_HSE.StatsName = "4.13 Weekly site HSE Inspection with client";
            KPI_WEEKLY_HSE.StatsValue = hseProjection.Entity.KPI_WEEKLY_HSE;
            KPI_WEEKLY_HSE.StatsTarget = hseProjection.Entity.KPI_WEEKLY_HSE_CRITERIA == 1 ? 0 : hseProjection.KPI_WEEKLY_HSE_TargetNumber;
            KPI_WEEKLY_HSE.StatsCriteria = hseProjection.Entity.KPI_WEEKLY_HSE_CRITERIA == 1 ? "Not Applicable" : ">= 1/Week";
            KPI_WEEKLY_HSE.StatsFormat = hseProjection.KPI_CORRECTIVE_ACT_CLOSED_Format;
            KPI_WEEKLY_HSE.StatsMask = "N0";
            KPI_WEEKLY_HSE.StatsType = HSEStatsType.KPI_WEEKLY_HSE;
            Stats.Add(KPI_WEEKLY_HSE);

            HSEReportProjection KPI_RISK_REGISTER = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group4 };
            KPI_RISK_REGISTER.StatsName = "4.14 Risk register review";
            KPI_RISK_REGISTER.StatsValue = hseProjection.Entity.KPI_RISK_REGISTER;
            KPI_RISK_REGISTER.StatsTarget = hseProjection.Entity.KPI_RISK_REGISTER_CRITERIA == 1 ? 0 : 1;
            KPI_RISK_REGISTER.StatsCriteria = hseProjection.Entity.KPI_RISK_REGISTER_CRITERIA == 1 ? "Not Applicable" : ">= 1/Month";
            KPI_RISK_REGISTER.StatsFormat = hseProjection.KPI_RISK_REGISTER_Format;
            KPI_RISK_REGISTER.StatsMask = "N0";
            KPI_RISK_REGISTER.StatsType = HSEStatsType.KPI_RISK_REGISTER;
            Stats.Add(KPI_RISK_REGISTER);

            HSEReportProjection TRAIN_COMPLIANCE = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group5 };
            TRAIN_COMPLIANCE.StatsName = "5.1 Compliance with training matrix";
            TRAIN_COMPLIANCE.StatsValue = hseProjection.Entity.TRAIN_COMPLIANCE;
            TRAIN_COMPLIANCE.StatsTarget = hseProjection.Entity.TRAIN_COMPLIANCE_CRITERIA == 1 ? 0 : 1;
            TRAIN_COMPLIANCE.StatsCriteria = hseProjection.Entity.TRAIN_COMPLIANCE_CRITERIA == 1 ? "Not Applicable" : ">= 1/Month (or >= 1/Project)";
            TRAIN_COMPLIANCE.StatsFormat = hseProjection.TRAIN_COMPLIANCE_Format;
            TRAIN_COMPLIANCE.StatsMask = "N0";
            TRAIN_COMPLIANCE.StatsType = HSEStatsType.TRAIN_COMPLIANCE;
            Stats.Add(TRAIN_COMPLIANCE);

            HSEReportProjection TRAIN_VOC = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group5 };
            TRAIN_VOC.StatsName = "5.2 Employee Verification of Competency (VOC's)";
            TRAIN_VOC.StatsValue = hseProjection.Entity.TRAIN_VOC;
            TRAIN_VOC.StatsTarget = 1;
            TRAIN_VOC.StatsCriteria = "= 100%";
            TRAIN_VOC.StatsFormat = hseProjection.TRAIN_VOC_Format;
            TRAIN_VOC.StatsMask = "P0";
            TRAIN_VOC.StatsType = HSEStatsType.TRAIN_VOC;
            Stats.Add(TRAIN_VOC);

            HSEReportProjection Total_ManHours = new HSEReportProjection() { Project = hseProjection.Entity.PROJECT, HSEDate = hseProjection.Entity.HSE_DATE, Group = group6 };
            Total_ManHours.StatsName = "5.3 Total Man Hours";
            Total_ManHours.StatsValue = hseProjection.Total_ManHours;
            Total_ManHours.StatsTarget = 0;
            Total_ManHours.StatsCriteria = "";
            Total_ManHours.StatsFormat = true;
            Total_ManHours.StatsMask = "N0";
            Total_ManHours.StatsType = HSEStatsType.Total_ManHours;
            Stats.Add(Total_ManHours);

            return Stats;
        }
    }

    public static class HSEReportProjectionQueries
    {
        public static IQueryable<HSEReportProjection> UnwrapHSE(
            IQueryable<HSE> HSEs)
        {
            List<HSEReportProjection> HSEReportStats = new List<HSEReportProjection>();
            foreach(HSE HSE in HSEs)
            {
                HSEProjection HSEProjection = new HSEProjection() { Entity = HSE };
                StaticHSEReportProjection staticStats = new StaticHSEReportProjection(HSEProjection);
                HSEReportStats.AddRange(staticStats.SummarizeStats());
            }

            return HSEReportStats.AsQueryable();
        }
    }
}
