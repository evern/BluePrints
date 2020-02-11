using BaseModel.Misc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Resources;
using DevExpress.Mvvm;
using BluePrints.Common.Base;
using BluePrints.Common;
using BaseModel.DataModel;
using DevExpress.XtraEditors.DXErrorProvider;

namespace BluePrints.Data
{
    public partial class CONSTRUCTION_JOB : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate, IHaveDBProductivityOverride, ICategorisable, ICanAssignP6, ICanAssignSubJobAndWorkpack
    {
        public CONSTRUCTION_JOB()
        {
            DISCIPLINE_NUM = 1;
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        //Used for direct property access validation in fill/undo-redo
        [NotMapped]
        public Guid? SubAreaGuid
        {
            get
            {
                return GUID_SUBAREA;
            }
            set
            {
                Guid? setValue = (Guid?)value;
                if (setValue == null)
                    GUID_SUBAREA = null;
                else if (IsSubAreaValid(setValue))
                    GUID_SUBAREA = setValue;
            }
        }

        [NotMapped]
        public IEnumerable<AREA> SubAreaCollection
        {
            get
            {
                if (AREA == null)
                    return null;

                return AREA.AREA1;
            }
        }

        public bool IsSubAreaValid(Guid? subAreaGuid)
        {
            if (subAreaGuid == null)
                return false;

            if (SubAreaCollection == null)
                return false;

            return SubAreaCollection.Any(x => x.GUID == subAreaGuid);
        }

        [NotMapped]
        public PHASE CachedPHASE { get; set; }

        [NotMapped]
        public PhaseType? PhaseType => CachedPHASE != null ? CachedPHASE.PHASE_TYPE : PHASE != null ? PHASE.PHASE_TYPE : null;

        [NotMapped]
        public IEnumerable<COMMODITY_CODE> FullCOMMODITY_CODECollection;
        [NotMapped]
        public IEnumerable<COMMODITY_CODE> CommodityCodeCollection
        {
            get
            {
                if (PhaseType == null || FullCOMMODITY_CODECollection == null || GUID_DISCIPLINE == null)
                    return null;

                return FullCOMMODITY_CODECollection.Where(x => x.PHASE_TYPE == PhaseType && x.GUID_DISCIPLINE == GUID_DISCIPLINE).OrderBy(x => x.CODE);
            }
        }

        [NotMapped]
        public IEnumerable<DISCIPLINE> DisciplineCollection
        {
            get
            {
                if (PhaseType == null || FullCOMMODITY_CODECollection == null)
                    return null;

                return FullCOMMODITY_CODECollection.Where(x => x.PHASE_TYPE == PhaseType).Where(x => x.DISCIPLINE != null).Select(x => x.DISCIPLINE).Distinct().OrderBy(x => x.CODE);
            }
        }

        public bool IsDisciplineCodeValid
        {
            get
            {
                if (PhaseType != null)
                {
                    //when commodity code collection is not set, don't show any error
                    if (CommodityCodeCollection == null)
                        return true;

                    return CommodityCodeCollection.Any(x => x.GUID_DISCIPLINE == GUID_DISCIPLINE);
                }
                //when phase type is not set, don't show any error
                else
                {
                    return true;
                }
            }
        }

        public bool IsCommodityCodeValid
        {
            get
            {
                if (PhaseType != null && COMMODITY_CODE != null)
                {
                    //when commodity code collection is not set, don't show any error
                    if (CommodityCodeCollection == null)
                        return true;

                    return CommodityCodeCollection.Any(x => x.CODE == COMMODITY_CODE);
                }
                //when phase type is not set, don't show any error
                else
                {
                    return true;
                }
            }
        }

        //public string Deliverable_Name => STOCK_CODE == null ? string.Empty : STOCK_CODE.CODE;
        //for scheduling view use
        public string Deliverable_Name
        {
            get
            {
                string name = string.Empty;
                name += Subjob_Name + "-";
                name += Discipline_Code + "-";
                name += Commodity_Code;
                return name;
            }
        }

        [NotMapped]
        public Guid? Subjob_Guid
        {
            get { return GUID_SUBJOB; }
            set { GUID_SUBJOB = value; }
        }

        [NotMapped]
        public string Discipline_Code
        {
            get
            {
                if (DISCIPLINE != null)
                    return DISCIPLINE.CODE + DISCIPLINE_NUM.ToString("00");
                else if (CachedDISCIPLINE != null)
                    return CachedDISCIPLINE.CODE + DISCIPLINE_NUM.ToString("00");
                else
                    return string.Empty;
            }
        }

        //used for storing newly added entity so that we don't change the context of the existing DISCIPLINE for newly added rows for Discipline_Code
        [NotMapped]
        public DISCIPLINE CachedDISCIPLINE { get; set; }

        public string Phase_Code => BluePrintsResources.Default_Construction_Phase;

        public string Commodity_Code
        {
            get
            {
                return COMMODITY_CODE;
            }
        }

        //used for storing newly added entity so that we don't change the context of the existing COMMODITY_CODE for newly added rows for Commodity_Code
        [NotMapped]
        public COMMODITY_CODE CachedCOMMODITY_CODE { get; set; }

        public Guid? Area_Guid => GUID_AREA;

        public Guid? SubArea_Guid => GUID_SUBAREA;

        [NotMapped]
        public decimal? DB_Productivity_Override { get => PRODUCTIVITY_OVERRIDE; set => PRODUCTIVITY_OVERRIDE = value; }

        [NotMapped]
        public string Subjob_Name
        {
            get
            {
                if (SUBJOB != null)
                    return SUBJOB.INTERNAL_NAME1;
                else if (CachedSUBJOB != null)
                    return CachedSUBJOB.INTERNAL_NAME1;
                else
                    return string.Empty;
            }
        }

        //used for storing newly added SUBJOB so that we don't change the context of the existing SUBJOB for newly added rows for Subjob_Name
        [NotMapped]
        public SUBJOB CachedSUBJOB { get; set; }

        [NotMapped]
        public string Department_Code
        {
            get
            {
                return "CN";
            }
        }

        [NotMapped]
        public Guid? Phase_Guid { get => GUID_PHASE; set => GUID_PHASE = value; }

        [NotMapped]
        public Guid? Workpack_Guid { get => GUID_WORKPACK; set => GUID_WORKPACK = value; }

        public Guid? Discipline_Guid => GUID_DISCIPLINE;

        public decimal Discipline_Number => DISCIPLINE_NUM;

        public string UniqueJobcode => Deliverable_Name + " " + VARIATION_CODE;

        public PhaseType? Phase => PHASE == null ? null : PHASE.PHASE_TYPE;

        [NotMapped]
        public ChargeType? Charge => PHASE == null ? null : PHASE.CHARGE_TYPE;

        public string Office
        {
            get
            {
                if (this.PROJECT != null)
                    return this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;

                return string.Empty;
            }
        }

        public string Project_Number
        {
            get
            {
                if (this.PROJECT != null)
                    return this.PROJECT.NUMBER;

                return string.Empty;
            }
        }

        public string Variation_Code => VARIATION_CODE;

        public void BuildStats(decimal weightingPortion, List<StatsCalculationType> calcTypes)
        {
            throw new NotImplementedException();
        }

        public void SetOriginalEntityKey(Guid newGuid)
        {
            throw new NotImplementedException();
        }

        [NotMapped]
        private List<P6_ASSIGNMENT> p6_assignments;
        [NotMapped]
        public List<P6_ASSIGNMENT> P6_Assignments
        {
            get
            {
                if (p6_assignments == null)
                    p6_assignments = new List<P6_ASSIGNMENT>();

                return p6_assignments;
            }
            set
            {
                p6_assignments = value;
            }
        }

        [NotMapped]
        List<PROGRESS_ITEM> progresses;
        [NotMapped]
        public IEnumerable<PROGRESS_ITEM> Progresses
        {
            get
            {
                if (progresses == null)
                    progresses = new List<PROGRESS_ITEM>();

                return progresses;
            }
        }

        public Guid DeliverableKey => GUID;

        public decimal Assigned_Percentage
        {
            get
            {
                return P6_Assignments.Sum(x => (x.HIGH_VALUE - (x.LOW_VALUE - 0.01m)));
            }
        }

        public decimal P6_Assignment_Total_Quantity => 1;

        public string P6_Assignment_UOM => "Job";

        public Guid? P6_WorkpackGuid => GUID_WORKPACK;

        public DateTime? TaskAssignmentStartDate { get; set; }

        public decimal Earned_Units_ToDate => 0;

        public Guid OriginalEntityKey => GUID;

        public decimal Budget_Units => 1;

        public decimal Unadjusted_Budget_Units => 1;

        public decimal Variation_Units => 0;

        public decimal Budget_Adjustment_Units => 1;

        public decimal Budget_Adjustment_Costs => 1;

        public decimal Variation_Costs => 0;

        public decimal Variation_InternalCosts => 0;

        public decimal Total_Units => 1;

        public decimal Total_Costs => 1;

        public decimal Total_InternalCosts => 1;

        [NotMapped]
        List<VariationAdjustment> approvedVariations;
        [NotMapped]
        public List<VariationAdjustment> ApprovedVariations
        {
            get
            {
                if (approvedVariations == null)
                    approvedVariations = new List<VariationAdjustment>();

                return approvedVariations;
            }
        }

        public string P6AssignmentName => Deliverable_Name;
    }


    public static class CONSTRUCTION_JOBQueries
    {
        public static IQueryable<CONSTRUCTION_JOB> CONSTRUCTION_JOBP6Assignment(IQueryable<CONSTRUCTION_JOB> CONSTRUCTION_JOBS, PROJECT PROJECT, IEnumerable<P6_ASSIGNMENT> P6_ASSIGNMENTS, IEnumerable<COMMODITY_CODE> COMMODITY_CODES)
        {
            foreach (CONSTRUCTION_JOB CONSTRUCTION_JOB in CONSTRUCTION_JOBS.Where(x => x.GUID_PROJECT == PROJECT.GUID))
            {
                CONSTRUCTION_JOB.P6_Assignments = P6_ASSIGNMENTS.Where(assignment => assignment.GUID_ORIGINAL == CONSTRUCTION_JOB.OriginalEntityKey).ToList();
                CONSTRUCTION_JOB.FullCOMMODITY_CODECollection = COMMODITY_CODES;
            }

            return CONSTRUCTION_JOBS;
        }
    }
}
