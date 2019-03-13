using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.P6Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace BluePrints.Common
{
    public static class BluePrintsConstants
    {
        public static decimal DurationBasedTotalUnits
        {
            get { return 0.0001m; }
        }

        public static decimal DurationBasedDisplayUnits
        {
            get { return 0; }
        }

        public static DateTime DefaultStartTime = new DateTime(1899, 1, 1, 0, 0, 0);
        public static DateTime DefaultFirstDay = new DateTime(DateTime.Now.Year, 1, 1);
        public static DateTime DefaultLastDay = new DateTime(2099, 1, 1);
    }

    public enum StaticSummaryRowTypes
    {
        [Display(Name = "Hours (manhour indirect)")]
        Indirect_Man_Hours = 0,
        [Display(Name = "Hours (manhours direct)")]
        Direct_Man_Hours = 1,
        [Display(Name = "Costs")]
        Costs = 2
    }

    public enum DocumentNumberStatus
    {
        [Display(Name = "Preliminary")] Preliminary = 0,
        [Display(Name = "Awaiting Approval")] Awaiting = 1,
        [Display(Name = "Approved")] Approved = 2
    }

    public enum RosterStatus
    {
        [Display(Name = "Drive in AM")] DIA = 0,
        [Display(Name = "Fly in AM ")] FIA = 1,
        [Display(Name = "Drive out PM")] DOP = 2,
        [Display(Name = "Fly out PM")] FOP = 3,
        [Display(Name = "Perth")] Perth = 4,
        [Display(Name = "R&R")] RnR = 5
    }

    public enum RateRole
    {
        [Display(Name = "None")] None = 0,
        [Display(Name = "Manager")] Manager = 1,
        [Display(Name = "Principal")] Principal = 2,
        [Display(Name = "Lead")] Lead = 3,
        [Display(Name = "Senior")] Senior = 4,
        [Display(Name = "Engineer")] Engineer = 5,
        [Display(Name = "Graduade")] Graduate = 6,
        [Display(Name = "Undergraduate")] Undergraduate = 7
    }

    public enum HSE_NonApplicableCriteria
    {
        Applicable = 0,
        NonApplicable = 1
    }

    public enum BellCurveShape
    {
        [Display(Name = "FrontLoaded2")] FrontLoaded1 = 0,
        [Display(Name = "FrontLoaded1")] FrontLoaded2 = 1,
        [Display(Name = "Balanced")] Balanced = 2,
        [Display(Name = "BackLoaded1")] BackLoaded1 = 3,
        [Display(Name = "BackLoaded2")] BackLoaded2 = 4
    }

    public enum Office
    {
        [Display(Name = "Perth")] Perth = 0,
        [Display(Name = "Montreal")] Montreal = 1,
    }

    public enum DeliverableInternalNumberMode
    {
        [Display(Name = "Only Editable On Unprogressed")] Default = 0,
        [Display(Name = "Always Editable")] AlwaysEditable = 1,
        [Display(Name = "Manual")] Manual = 2,
        [Display(Name = "Locked")] Locked = 3
    }

    public enum DesignManager
    {
        [Display(Name = "None")] None = 0,
        [Display(Name = "Peter Grigsby")] PeterGrigsby = 1,
        [Display(Name = "Ben Davies")] BenDavies = 2,
        [Display(Name = "Ned Hambling")] NedHambling = 3
    }

    public enum ContractType
    {
        [Display(Name = "Lump Sum")] LumpSum = 0,
        [Display(Name = "Rates")] Rates = 1,
    }

    public enum ProjectType
    {
        [Display(Name = "Design")] Design = 0,
        [Display(Name = "Design & Construct")] DesignConstruct = 1,
        [Display(Name = "Study")] Study = 2,
        [Display(Name = "Construct")] Construct = 3,
        [Display(Name = "Operation")] Operation = 4
    }

    public enum ProjectStatus
    {
        [Display(Name = "Active")] Active = 0,
        [Display(Name = "On Hold")] OnHold = 1,
        [Display(Name = "Closed")] Closed = 2,
        [Display(Name = "Tender WIP")] Tender = 3,
        [Display(Name = "Tender Submitted")] TenderSubmitted = 4,
        [Display(Name = "Lost")] Lost = 5,
        [Display(Name = "Lead")] Lead = 6
    }

    public enum VariationRegisterStatus
    {
        [Display(Name = "Identified")] Identified = 0,
        [Display(Name = "Submitted")] Submitted = 1,
        [Display(Name = "Approved")] Approved = 2,
    }

    public enum BaselineStatus
    {
        [Display(Name = "Superseded")] Superseded = 0,
        [Display(Name = "Working")] Working = 1,
        [Display(Name = "Live")] Live = 2, 
        [Display(Name = "Variation")] Variation = 3
    }

    public enum DeliverableType
    {
        [Display(Name = "Deliverable ICR")] DeliverableICR = 0,
        [Display(Name = "Deliverable")] Deliverable = 1,
        [Display(Name = "Task")] Task = 2,
        [Display(Name = "Non-Deliverable")] NonDeliverable = 3
    }

    public enum ProgressIntervalType
    {
        [Display(Name = "Daily")] Daily = 1,
        [Display(Name = "Weekly")] Weekly = 7,
        [Display(Name = "Monthly")] Monthly = 30
    }

    public enum ProgressStatus
    {
        [Display(Name = "Superseded")] Superseded = 0,
        [Display(Name = "Working")] Working = 1,
        [Display(Name = "Live")] Live = 2
    }

    public enum VariationAction
    {
        [Display(Name = "No Action")] NoAction = 0,
        [Display(Name = "Append")] Append = 1,
        [Display(Name = "Add")] Add = 2,
        [Display(Name = "Cancel")] Cancel = 3
    }

    public enum BaselineMappingType
    {
        [Display(Name = "Original")] Original = 0,
        [Display(Name = "Modified")] Modified = 1
    }

    public enum RegisterRaisedByType
    {
        [Display(Name = "Internal")] Internal = 0,
        [Display(Name = "Client")] Client = 1,
        [Display(Name = "Sub-contractor")] SubContractor = 2
    }

    public enum CorrectiveActionType
    {
        [Display(Name = "Design Use")] DesignUse = 0,
        [Display(Name = "Concessional Use")] ConcessionalUse = 1,
        [Display(Name = "Preclude Use")] PrecludeUse = 2
    }

    public enum ScheduleImpact
    {
        [Display(Name = "Yes")]
        Yes = 0,
        [Display(Name = "No")]
        No = 1,
        [Display(Name = "Potential")]
        Potential = 2
    }

    public enum AgendaPriority
    {
        [Display(Name = "Low")]
        Low = 0,
        [Display(Name = "Medium")]
        Medium = 1,
        [Display(Name = "High")]
        High = 2
    }

    public enum ActionedOnDrawing
    {
        [Display(Name = "N/A")]
        NA = 0,
        [Display(Name = "Yes")]
        Yes = 1
    }

    public enum DashboardViewType
    {
        Costs,
        Units, 
        Quantity
    }

    public enum BaselineMappingSelectionType
    {
        None,
        Original,
        Modified
    }

    public enum BaselineMappingMode
    {
        Default,
        ByWorkpack
    }

    public enum AppointmentActivityType
    {
        WBS = 0,
        Activity = 1,
        Milestone = 2
    }

    public enum ReportType
    {
        Progress_Report,
        Baseline_Report,
        Meeting_Minute,
        Project_Report,
        Risk_Assessment,
        RateRole_Report,
        RateDiscipline_Report,
        RateDisciplineRole_Report,
        Project_Summary
    }

    public enum P6TASKTYPE
    {
        TT_Task = 0,
        TT_Mile = 1,
        TT_FinMile = 2
    }

    public enum P6TASKSTATUS
    {
        TK_NotStart = 0,
        TK_Active = 1,
        TK_Complete = 2
    }

    public enum P6DURATION_TYPE
    {
        [Display(Name = "Fixed Duration and Units/Time")]
        DT_FixedDrtn = 0,
        [Display(Name = "Fixed Duration & Units")]
        DT_FixedDUR2 = 1,
        [Display(Name = "Fixed Units/Time")]
        DT_FixedRate = 2,
        [Display(Name = "Fixed Units")]
        DT_FixedQty = 3
    }

    public enum P6COMPLETE_TYPE
    {
        [Display(Name = "Duration")]
        CP_Drtn = 0,
        [Display(Name = "Physical")]
        CP_Phys = 1,
        [Display(Name = "Units")]
        CP_Units = 2
    }

    public enum DateNavigationType
    {
        Forward,
        Backward,
        Current
    }

    public enum VariationType
    {
        Internal = 0,
        External = 1
    }

    public enum StockCodeType
    {
        Estimate = 0,
        Budget = 1
    }

    public enum DialogAction
    {
        Add,
        Edit,
        Cancel
    }

    public enum TimesheetDateDialogAction
    {
        Ok,
        UseWeekStart
    }

    public enum ExoBurnedFilterType
    {
        [Display(Name = "All")]
        All,
        [Display(Name = "Design")]
        Design,
        [Display(Name = "Construct")]
        Construct
    }

    public enum EstimateProgressType
    {
        [Display(Name = "Standalone")]
        Standalone,
        [Display(Name = "Trackable")] 
        Trackable,
        [Display(Name = "Auto")] 
        Auto
    }

    public enum CostGroup
    {
        [Display(Name = "Offsite")]
        Offsite = 0,
        [Display(Name = "Site")]
        Site = 1,
    }

    public enum ProjectDocumentStatus
    {
        [Display(Name = "N/A")]
        NA = 0,
        [Display(Name = "Yes")]
        Yes = 1,
        [Display(Name = "No")]
        No = 2,
    }

    public enum PhaseType
    {
        [Display(Name = "Design")]
        Design = 0,
        [Display(Name = "Construct")]
        Construct = 1,
        [Display(Name = "Indirect")]
        Indirect = 2
    }

    public enum ChargeType
    {
        [Display(Name = "Direct")]
        Direct = 0,
        [Display(Name = "Indirect")]
        Indirect = 1
    }
    public enum ProgressType
    {
        [Display(Name = "Design")]
        Design = 0,
        [Display(Name = "Construct")]
        Construct = 1,
        [Display(Name = "Procurement")]
        Procurement = 2
    }

    public enum STOCK_GROUPProjectionType
    {
        [Display(Name = "Grouped Project Specific")] ProjectSpecificGrouped,
        [Display(Name = "Not Grouped Project Specific")] ProjectSpecificNotGrouped,
        [Display(Name = "Grouped Not Project Specific")] GeneralGrouped,
        [Display(Name = "Not Grouped Not Project Specific")] GeneralNotGrouped
    }

    public enum Register_ImpactType
    {
        [Display(Name = "No Impact")]
        NoImpact = 0,
        [Display(Name = "Internal")]
        Internal = 1,
        [Display(Name = "Variation")]
        Variation = 2
    }

    public enum Register_RiskLikelihood
    {
        [Display(Name = "A Certain")]
        Certain = 0,
        [Display(Name = "B Likely")]
        Likely = 1,
        [Display(Name = "C Possible")]
        Possible = 2,
        [Display(Name = "D Unlikely")]
        Unlikely = 3,
        [Display(Name = "E Rare")]
        Rare = 4
    }

    public enum Register_RiskConsequence
    {
        [Display(Name = "1 Insignificant")]
        Insignificant = 0,
        [Display(Name = "2 Minor")]
        Minor = 1,
        [Display(Name = "3 Moderate")]
        Moderate = 2,
        [Display(Name = "4 Major")]
        Major = 3,
        [Display(Name = "5 Catastrophic")]
        Catastrophic = 4
    }

    public enum Minute_Status
    {
        [Display(Name = "Open")]
        Open = 0,
        [Display(Name = "Close")]
        Close = 1,
        [Display(Name = "Note")]
        Note = 2
    }

    public enum Register_RiskRanking
    {
        [Display(Name = "1 Low")]
        Low1 = 0,
        [Display(Name = "2 Low")]
        Low2 = 1,
        [Display(Name = "3 Low")]
        Low3 = 2,
        [Display(Name = "4 Low")]
        Low4 = 3,
        [Display(Name = "5 Low")]
        Low5 = 4,
        [Display(Name = "6 Low")]
        Low6 = 5,
        [Display(Name = "7 Medium")]
        Medium7 = 6,
        [Display(Name = "8 Medium")]
        Medium8 = 7,
        [Display(Name = "9 Medium")]
        Medium9 = 8,
        [Display(Name = "10 Medium")]
        Medium10 = 9,
        [Display(Name = "11 Medium")]
        Medium11 = 10,
        [Display(Name = "12 High")]
        High12 = 11,
        [Display(Name = "13 High")]
        High13 = 12,
        [Display(Name = "14 High")]
        High14 = 13,
        [Display(Name = "15 High")]
        High15 = 14,
        [Display(Name = "16 High")]
        High16 = 15,
        [Display(Name = "17 High")]
        High17 = 16,
        [Display(Name = "18 High")]
        High18 = 17,
        [Display(Name = "19 High")]
        High19 = 18,
        [Display(Name = "20 Extreme")]
        Extreme20 = 19,
        [Display(Name = "21 Extreme")]
        Extreme21 = 20,
        [Display(Name = "22 Extreme")]
        Extreme22 = 21,
        [Display(Name = "23 Extreme")]
        Extreme23 = 22,
        [Display(Name = "24 Extreme")]
        Extreme24 = 23,
        [Display(Name = "25 Extreme")]
        Extreme25 = 24,
        [Display(Name = "Unassigned")]
        Unassigned = 25
    }

    public enum Register_HazardGroup
    {
        [Display(Name = "Electrical")]
        Electrical = 0,
        [Display(Name = "Fire and Emergencies")]
        Fire_and_Emergencies = 1,
        [Display(Name = "People, Material Movement")]
        People_Material_Movement = 2,
        [Display(Name = "Radiation")]
        Radiation = 3,
        [Display(Name = "Working Environment")]
        Working_Environment = 4,
        [Display(Name = "Plant")]
        Plant = 5,
        [Display(Name = "Amenities")]
        Amenities = 6,
        [Display(Name = "Earthworks")]
        Earthworks = 7,
        [Display(Name = "Structural Safety")]
        Structural_Safety = 8,
        [Display(Name = "Manual Tasks")]
        Manual_Tasks = 9,
        [Display(Name = "Substances")]
        Substances = 10,
        [Display(Name = "Falls Prevention")]
        Falls_Prevention = 11,
        [Display(Name = "Specific Risks")]
        Specific_Risks = 12,
        [Display(Name = "Noise Exposure")]
        Noise_Exposure = 13
    }

    public enum Register_IssueType
    {
        [Display(Name = "Client")]
        Client = 0,
        [Display(Name = "Internal")]
        Internal = 1,
        [Display(Name = "Vendor")]
        Vendor = 2
    }

    public enum Register_IssueImportance
    {
        [Display(Name = "Low")]
        Low = 0,
        [Display(Name = "Medium")]
        Medium = 1,
        [Display(Name = "High")]
        High = 2
    }

    public enum P6_BluePrints_Override
    {
        [Display(Name = "Start")]
        START = 0,
        [Display(Name = "Finish")]
        FINISH = 1,
        [Display(Name = "None")]
        NONE = 2
    }

    public enum MeetingUserSection
    {
        [Display(Name = "Attendees")]
        Attendees = 0,
        [Display(Name = "Apologies")]
        Apologies = 1,
        [Display(Name = "Distribution")]
        Distribution = 2,
        [Display(Name = "SignOff")]
        SignOff = 3
    }

    public enum MeetingUserType
    {
        [Display(Name = "Internal")]
        Internal = 0,
        [Display(Name = "Client")]
        Client = 1,
    }

    public enum DeliverablesViewType
    {
        [Display(Name = "Indirect")]
        Indirect = 0,
        [Display(Name = "Direct")]
        Direct = 1,
        [Display(Name = "Both")]
        Both = 2
    }

    public enum EstimateViewMode
    {
        [Display(Name = "Estimate")]
        Estimate = 0,
        [Display(Name = "Budget")]
        Budget = 1,
        [Display(Name = "Both")]
        Both = 2,
    }

    public enum EstimateLineType
    {
        [Display(Name = "Install")]
        Install = 0,
        [Display(Name = "Supply")]
        Supply = 1,
        [Display(Name = "Indirect")]
        Indirect = 0
    }

    public enum IncidentClassification
    {
        [Display(Name = "Lost Time Injury")]
        LTI = 0,
        [Display(Name = "Restricted Work Injury")]
        RWI = 1,
        [Display(Name = "Medical Treatment Injury")]
        MTI = 2,
        [Display(Name = "First Aid (or no-treatment required) Injury")]
        FAI = 3,
        [Display(Name = "Non-Work Related Injury or Illness")]
        NWR = 4,
        [Display(Name = "Motor Vehicle Accident")]
        MVA = 5,
        [Display(Name = "Property, Plant, Equipment Damage")]
        DAM = 6,
        [Display(Name = "Security, Theft, Public Disturbance")]
        SEC = 7,
        [Display(Name = "Environmental Spill, Damage")]
        ENV = 8,
        [Display(Name = "Fire")]
        FIRE = 9,
        [Display(Name = "Near Miss")]
        NM = 10,
        [Display(Name = "Other")]
        OTH = 11
    }

    public class ReportingEnum
    {
        /// <summary>
        /// Specifies P6 DataPoints Processing Type
        /// </summary>
        public enum DataPointsType
        {
            Planned = 0,
            Earned = 1,
            Remaining = 2
        }

        /// <summary>
        /// Specifies P6 Task Assignment Load Type
        /// </summary>
        public enum AssignmentLoadType
        {
            Original,
            Modified,
            Both
        }
    }

    public class PhaseTypeClass
    {
        public PhaseType phaseType { get; set; }
        public PhaseTypeClass(PhaseType phaseType)
        {
            this.phaseType = phaseType;
        }
    }

    /// <summary>
    /// Used for optional parameter purpose
    /// </summary>
    public class StockCodeTypeClass
    {
        public StockCodeType commodityCodeType { get; set; }

        public StockCodeTypeClass(StockCodeType commodityCodeType)
        {
            this.commodityCodeType = commodityCodeType;
        }
    }

    public class ProjectIssue
    {
        public string Severity { get; set; }
        public string Type { get; set; } 
        public string Description { get; set; }
        public string Resolve { get; set; }
        public int IconIndex { get; set; }
    }

    /// <summary>
    /// Used for retrieving units and displaying missing assignments
    /// </summary>
    public class P6_AssignmentProjection
    {
        public P6_AssignmentProjection(ICanAssignP6 deliverableProjection, P6_ASSIGNMENT baseline_item_assignment)
        {
            this.deliverableProjection = deliverableProjection;
            this.deliverable_assignment = baseline_item_assignment;
        }

        public readonly P6_ASSIGNMENT deliverable_assignment;
        public readonly ICanAssignP6 deliverableProjection;

        TASK p6_task;
        public TASK P6_TASK
        {
            get { return p6_task; }
            set
            {
                if (value == null)
                    return;

                if (value.act_end_date != null)
                    value.early_end_date = null;

                if (value.act_start_date != null)
                    value.early_start_date = null;

                p6_task = value;
            }
        }

        public string INTERNAL_NUM
        {
            get { return deliverableProjection.P6AssignmentName; }
        }

        public string P6_ACTIVITY
        {
            get { return deliverable_assignment.P6_ACTIVITYID; }
        }

        public decimal UNITS
        {
            get
            {
                if (deliverableProjection == null)
                    return 0;

                return ((deliverable_assignment.HIGH_VALUE - deliverable_assignment.LOW_VALUE) + 0.01m) * deliverableProjection.Total_Units;
            }
        }

        public Guid DeliverableKey
        {
            get { return deliverableProjection.DeliverableKey; }
        }

        public string Deliverable_Description { get; set; }

        public string P6_Description { get; set; }

        public decimal FromPercentage
        {
            get { return deliverable_assignment.LOW_VALUE; }
        }

        public decimal ToPercentage
        {
            get { return deliverable_assignment.HIGH_VALUE; }
        }

        public void Reassign(string p6NewActivity)
        {
            if (p6NewActivity == null || p6NewActivity == string.Empty)
                return;

            deliverable_assignment.P6_ACTIVITYID = p6NewActivity;
        }
    }

    /// <summary>
    /// Used for remapping P6 activities
    /// </summary>
    public class P6ActivityRemap
    {
        public string P6_OLD_ACTIVITY { get; set; }
        public string OLD_ACTIVITY_DESCRIPTION { get; set; }
        public string P6_NEW_ACTIVITY { get; set; }
    }

    public class ClientNumberAssignment
    {
        public string INTERNAL_NUM { get; set; }
        public string CLIENT_NUM { get; set; }
    }
}