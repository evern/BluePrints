using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Misc;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class DesignTimesheet : EntityBase
    {
        [Key]
        public int Key => Timesheet == null ? 0 : Timesheet.SEQNO;

        List<ExoTimeAuthorisation> exoTimeAuthorisations;
        IQueryable<BASELINE_ITEM> deliverables;
        public JOB_TIMESHEETS Timesheet { get; set; }
        public bool IsSubmitted => Timesheet == null ? false : Timesheet.X_SUBMITTED == true ? true : false;
        IPrimeroEntitiesUnitOfWork primeroUOW;
        JOBCOST_HDR masterJob;
        int defaultTenderCostGroupSeqNo;
        int defaultTenderCostTypeSeqNo;
        IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection;

        bool canUnsubmit { get; set; }
        public bool CanSubmit
        {
            get
            {
                if (canUnsubmit)
                    return true;

                return !IsSubmitted;
            }
        }

        public DesignTimesheet()
        {

        }

        public DesignTimesheet(List<ExoTimeAuthorisation> exoTimeAuthorisations, IPrimeroEntitiesUnitOfWork primeroUOW, IQueryable<BASELINE_ITEM> BASELINE_ITEMQueryable, JOBCOST_RESOURCE resource, DateTime weekStartDate, bool canUnsubmit, int defaultTenderCostGroupSeqNo, int defaultTenderCostTypeSeqNo, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection, JOB_TIMESHEETS timesheet = null)
        {
            SetInitProperties(exoTimeAuthorisations, primeroUOW, BASELINE_ITEMQueryable, resource, weekStartDate, canUnsubmit, defaultTenderCostGroupSeqNo, defaultTenderCostTypeSeqNo, COMMODITY_CODECollection, timesheet);
        }

        public void SetInitProperties(List<ExoTimeAuthorisation> exoTimeAuthorisations, IPrimeroEntitiesUnitOfWork primeroUOW, IQueryable<BASELINE_ITEM> BASELINE_ITEMQueryable, JOBCOST_RESOURCE resource, DateTime weekStartDate, bool canUnsubmit, int defaultTenderCostGroupSeqNo, int defaultTenderCostTypeSeqNo, IEnumerable<COMMODITY_CODE> COMMODITY_CODECollection, JOB_TIMESHEETS timesheet = null)
        {
            if (this.Timesheet != null)
                return;

            this.exoTimeAuthorisations = exoTimeAuthorisations;
            this.deliverables = BASELINE_ITEMQueryable;
            this.Timesheet = timesheet;
            this.primeroUOW = primeroUOW;
            this.canUnsubmit = canUnsubmit;
            this.defaultTenderCostGroupSeqNo = defaultTenderCostGroupSeqNo;
            this.defaultTenderCostTypeSeqNo = defaultTenderCostTypeSeqNo;
            this.COMMODITY_CODECollection = COMMODITY_CODECollection;

            if(timesheet == null)
            {
                this.Timesheet = BluePrintsUtils.InitNewTimeSheet();
                this.Timesheet.STOCKCODE = resource.DEFAULT_STOCKCODE;
                this.Timesheet.STAFFNO = resource.SEQNO;
                this.Timesheet.DESCRIPTION = resource.RESOURCENAME;
                this.Timesheet.WEEK_START_DATE = weekStartDate;
            }
            else
            {
                //store narrative before assigning masterjobno and costgroupno because that'll clear narrative
                string narrative = timesheet.X_NARRATIVE;

                JOBCOST_HDR findSubJob = primeroUOW.JOBCOST_HDR.FirstOrDefault(x => x.JOBNO == this.Timesheet.JOBNO);
                string areaCode = string.Empty;
                if(findSubJob != null)
                {
                    //store narrative before assigning masterjobno and costgroupno because that'll clear areacode
                    if (findSubJob.JOBCODE.Length >= 12)
                        areaCode = findSubJob.JOBCODE.Substring(6, 3);

                    JOBCOST_HDR findMasterJob = primeroUOW.JOBCOST_HDR.FirstOrDefault(x => x.JOBNO == findSubJob.MASTER_JOBNO);
                    MasterJobNo = findMasterJob?.JOBNO;
                }

                CostGroupNo = timesheet.COST_GROUP;

                //reassign areaCode
                AreaCode = areaCode;

                //reassign narrative
                DeliverableInternalName = narrative;
            }

            this.Update();
        }

        List<ExoTimeAuthorisation> filteredExoTimeAuthorisations = new List<ExoTimeAuthorisation>();
        int? masterJobNo;
        string masterJobCode;
        public int? MasterJobNo
        {
            get => masterJobNo;
            set
            {
                masterJobNo = value;
                masterJob = primeroUOW.JOBCOST_HDR.FirstOrDefault(x => x.JOBNO == value);
                if(masterJob != null)
                {
                    masterJobCode = masterJob?.JOBCODE;
                    if(AreaCode == null || AreaCode == string.Empty)
                    {
                        this.Timesheet.JOBNO = masterJob.JOBNO;
                        this.Timesheet.TITLE = getTitle(masterJob);
                    }
                }
                filteredExoTimeAuthorisations = this.exoTimeAuthorisations.Where(x => x.MasterJobNo == value).ToList();

                costGroupCollection = null;
                CostGroupNo = null;
                areaCollection = null;
                AreaCode = null;
                variationCodes = null;
                deliverableCollection = null;
                this.Update();
            }
        }

        List<JOB_COSTGROUPS> costGroupCollection;
        public List<JOB_COSTGROUPS> CostGroupCollection
        {
            get
            {
                if (costGroupCollection == null && primeroUOW != null && MasterJobNo != null)
                {
                    List<string> filteredDisciplineCode = filteredExoTimeAuthorisations.Select(x => x.DisciplineCode).Distinct().ToList();
                    costGroupCollection = primeroUOW.JOB_COSTGROUPS.Where(x => filteredDisciplineCode.Contains(x.SHORTCODE)).ToList();
                }

                return costGroupCollection;
            }
        }

        string costGroupCode;
        int? costGroupNo;
        public int? CostGroupNo
        {
            get => costGroupNo;
            set
            {
                costGroupNo = value;
                
                if (value == null)
                {
                    costGroupCode = string.Empty;
                }
                else
                {
                    Timesheet.COST_GROUP = value;
                    costGroupCode = primeroUOW.JOB_COSTGROUPS.FirstOrDefault(x => x.SEQNO == value)?.SHORTCODE;
                }

                //if (value == null)
                    filteredExoTimeAuthorisations = this.exoTimeAuthorisations.Where(x => x.MasterJobNo == masterJobNo).ToList();
                //else
                //    filteredExoTimeAuthorisations = this.exoTimeAuthorisations.Where(x => x.MasterJobNo == masterJobNo).Where(x => x.DisciplineId == value).ToList();

                areaCollection = null;
                AreaCode = string.Empty;
                variationCodes = null;
                deliverableCollection = null;
                DeliverableInternalName = null;
                validCommodityCodes = null;
                taggedValidJobCostTypes = null;

                this.Update();
            }
        }

        protected List<COMMODITY_CODE> validCommodityCodes = null;
        public List<COMMODITY_CODE> ValidCommodityCodes
        {
            get
            {
                if (COMMODITY_CODECollection == null || costGroupCode == null)
                    return new List<COMMODITY_CODE>();

                if (validCommodityCodes == null)
                {
                    validCommodityCodes = BluePrintsDataUtils.FilterForValidCommodityCodes(COMMODITY_CODECollection, costGroupCode).ToList();
                }

                return validCommodityCodes;
            }
        }

        List<string> areaCollection;
        public List<string> AreaCollection
        {
            get
            {
                if (areaCollection == null)
                    areaCollection = filteredExoTimeAuthorisations.Select(x => x.AreaCode).Distinct().ToList();

                return areaCollection;
            }
        }

        List<string> variationCodes;
        public List<string> VariationCodes
        {
            get
            {
                if (variationCodes == null)
                    variationCodes = this.exoTimeAuthorisations.Where(x => x.MasterJobNo == masterJobNo).Select(x => x.VariationCode).Distinct().ToList();

                return variationCodes;
            }
        }

        string areaCode;
        public string AreaCode
        {
            get => areaCode;
            set
            {
                areaCode = value;
                if (value == null || value == string.Empty)
                {
                    if(masterJob != null)
                    {
                        this.Timesheet.JOBNO = masterJob.JOBNO;
                        this.Timesheet.TITLE = getTitle(masterJob);
                    }
                }
                else
                {
                    if (masterJobCode != null && masterJobCode != string.Empty && value != null && value != string.Empty)
                    {
                        string designSubjobCode = masterJobCode + "-" + value + "-" + BluePrintsResources.Default_Sub_Area + "-" + BluePrintsResources.Default_Design_Phase;
                        JOBCOST_HDR subJob = primeroUOW.JOBCOST_HDR.FirstOrDefault(x => x.JOBCODE == designSubjobCode);
                        if (subJob != null)
                        {
                            Timesheet.JOBNO = subJob.JOBNO;
                            Timesheet.TITLE = getTitle(subJob);
                        }
                    }
                }

                deliverableCollection = null;
                this.Update();
            }
        }

        private string getTitle(JOBCOST_HDR subJob)
        {
            string title = subJob.JOBCODE + " : " + subJob.TITLE;
            if (title.Length >= 60)
                title = title.Substring(0, 59);

            return title;
        }

        List<BASELINE_ITEM> deliverableCollection;
        public List<BASELINE_ITEM> DeliverableCollection
        {
            get
            {
                if (deliverableCollection == null && deliverables != null && CostGroupNo != null && masterJobCode != null)
                {
                    string costGroupSubCode = costGroupCode.Length >= 2 ? costGroupCode.Substring(0, 2) : costGroupCode;
                    deliverableCollection = deliverables.Where(x => x.BASELINE != null && x.BASELINE.PROJECT != null && x.AREA != null && x.DISCIPLINE != null && x.DOCTYPE != null)
                                            .Where(x => x.BASELINE.PROJECT.NUMBER == masterJobCode && x.AREA.INTERNAL_NUM == AreaCode && x.DISCIPLINE.CODE == costGroupSubCode).ToList();
                }

                return deliverableCollection;
            }
        }

        string deliverableInternalName;
        public string DeliverableInternalName
        {
            get => deliverableInternalName;
            set
            {
                deliverableInternalName = value;
                if (value == null || value == string.Empty)
                {
                    Timesheet.X_NARRATIVE = string.Empty;
                }
                else
                {
                    if (DeliverableCollection != null)
                    {
                        BASELINE_ITEM deliverable = DeliverableCollection.FirstOrDefault(x => x.INTERNAL_NUM == value);
                        if (deliverable != null && deliverable.DOCTYPE != null)
                        {
                            JOB_COSTTYPES costType = primeroUOW.JOB_COSTTYPES.FirstOrDefault(x => x.SHORTCODE == deliverable.DOCTYPE.CODE);
                            if (costType != null)
                            {
                                COST_TYPE = costType.SEQNO;
                                Timesheet.X_NARRATIVE = deliverable.INTERNAL_NUM;
                            }
                        }
                    }
                }

                this.Update();
            }
        }

        List<JOB_COSTTYPES> taggedValidJobCostTypes;
        public List<JOB_COSTTYPES> TaggedValidJOB_COSTTYPES
        {
            get
            {
                if (CostGroupNo == null)
                    return new List<JOB_COSTTYPES>();

                if(taggedValidJobCostTypes == null)
                {
                    taggedValidJobCostTypes = new List<JOB_COSTTYPES>();
                    foreach (JOB_COSTTYPES jobCostTypes in primeroUOW.JOB_COSTTYPES.OrderBy(x => x.COSTDESC))
                    {
                        JOB_COSTTYPES copyJOB_COSTTYPES = new JOB_COSTTYPES();
                        copyJOB_COSTTYPES.SEQNO = jobCostTypes.SEQNO;
                        copyJOB_COSTTYPES.SHORTCODE = jobCostTypes.SHORTCODE;
                        copyJOB_COSTTYPES.COSTDESC = jobCostTypes.COSTDESC;
                        copyJOB_COSTTYPES.DEF_COSTGROUP = jobCostTypes.DEF_COSTGROUP;

                        taggedValidJobCostTypes.Add(copyJOB_COSTTYPES);
                    }

                    foreach(JOB_COSTTYPES jobCostTypes in taggedValidJobCostTypes.Where(x => x.DEF_COSTGROUP == CostGroupNo))
                    {
                        jobCostTypes.IsValid = true;
                    }
                }

                return taggedValidJobCostTypes;
            }
        }

        public int? COST_TYPE
        {
            get => Timesheet == null ? null : Timesheet.COST_TYPE;
            set
            {
                Timesheet.COST_TYPE = value;
            }
        }

        public double? Day1
        {
            get => Timesheet == null ? null : Timesheet.DAY1;
            set
            {
                if (Timesheet == null)
                    return;

                if (value == 0)
                    Timesheet.DAY1 = null;
                else
                    Timesheet.DAY1 = value;

                this.Update();
            }
        }

        public double? Day2
        {
            get => Timesheet == null ? null : Timesheet.DAY2;
            set
            {
                if (Timesheet == null)
                    return;

                if (value == 0)
                    Timesheet.DAY2 = null;
                else
                    Timesheet.DAY2 = value;

                this.Update();
            }
        }

        public double? Day3
        {
            get => Timesheet == null ? null : Timesheet.DAY3;
            set
            {
                if (Timesheet == null)
                    return;

                if (value == 0)
                    Timesheet.DAY3 = null;
                else
                    Timesheet.DAY3 = value;

                this.Update();
            }
        }

        public double? Day4
        {
            get => Timesheet == null ? null : Timesheet.DAY4;
            set
            {
                if (Timesheet == null)
                    return;

                if (value == 0)
                    Timesheet.DAY4 = null;
                else
                    Timesheet.DAY4 = value;

                this.Update();
            }
        }

        public double? Day5
        {
            get => Timesheet == null ? null : Timesheet.DAY5;
            set
            {
                if (Timesheet == null)
                    return;

                if (value == 0)
                    Timesheet.DAY5 = null;
                else
                    Timesheet.DAY5 = value;

                this.Update();
            }
        }

        public double? Day6
        {
            get => Timesheet == null ? null : Timesheet.DAY6;
            set
            {
                if (Timesheet == null)
                    return;

                if (value == 0)
                    Timesheet.DAY6 = null;
                else
                    Timesheet.DAY6 = value;

                this.Update();
            }
        }

        public double? Day7
        {
            get => Timesheet == null ? null : Timesheet.DAY7;
            set
            {
                if (Timesheet == null)
                    return;

                if (value == 0)
                    Timesheet.DAY7 = null;
                else
                    Timesheet.DAY7 = value;

                this.Update();
            }
        }

        public double TotalHours
        {
            get
            {
                if (Timesheet == null)
                    return 0;

                double day1Hour = Day1 == null ? 0 : (double)Day1;
                double day2Hour = Day2 == null ? 0 : (double)Day2;
                double day3Hour = Day3 == null ? 0 : (double)Day3;
                double day4Hour = Day4 == null ? 0 : (double)Day4;
                double day5Hour = Day5 == null ? 0 : (double)Day5;
                double day6Hour = Day6 == null ? 0 : (double)Day6;
                double day7Hour = Day7 == null ? 0 : (double)Day7;

                return day1Hour + day2Hour + day3Hour + day4Hour + day5Hour + day6Hour + day7Hour;
            }
        }
    }
}
