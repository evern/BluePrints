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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class DesignTimesheet : EntityBase
    {
        List<ExoTimeAuthorisation> exoTimeAuthorisations;
        IQueryable<BASELINE_ITEM> deliverables;
        public JOB_TIMESHEETS Timesheet { get; set; }
        public bool IsSubmitted => Timesheet == null ? false : Timesheet.X_SUBMITTED == true ? true : false;
        IPrimeroEntitiesUnitOfWork primeroUOW;
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

        public DesignTimesheet(List<ExoTimeAuthorisation> exoTimeAuthorisations, IPrimeroEntitiesUnitOfWork primeroUOW, IQueryable<BASELINE_ITEM> BASELINE_ITEMQueryable, JOBCOST_RESOURCE resource, DateTime weekStartDate, bool canUnsubmit, JOB_TIMESHEETS timesheet = null)
        {
            SetInitProperties(exoTimeAuthorisations, primeroUOW, BASELINE_ITEMQueryable, resource, weekStartDate, canUnsubmit, timesheet);
        }

        public void SetInitProperties(List<ExoTimeAuthorisation> exoTimeAuthorisations, IPrimeroEntitiesUnitOfWork primeroUOW, IQueryable<BASELINE_ITEM> BASELINE_ITEMQueryable, JOBCOST_RESOURCE resource, DateTime weekStartDate, bool canUnsubmit, JOB_TIMESHEETS timesheet = null)
        {
            if (this.Timesheet != null)
                return;

            this.exoTimeAuthorisations = exoTimeAuthorisations;
            this.deliverables = BASELINE_ITEMQueryable;
            this.Timesheet = timesheet;
            this.primeroUOW = primeroUOW;
            this.canUnsubmit = canUnsubmit;

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
                CostGroupNo = timesheet.COST_GROUP;
                DeliverableInternalName = timesheet.X_NARRATIVE;

                JOBCOST_HDR findSubJob = primeroUOW.JOBCOST_HDR.FirstOrDefault(x => x.JOBNO == this.Timesheet.JOBNO);
                if(findSubJob != null)
                {
                    if(findSubJob.JOBCODE.Length >= 12)
                        AreaCode = findSubJob.JOBCODE.Substring(6, 3);

                    JOBCOST_HDR findMasterJob = primeroUOW.JOBCOST_HDR.FirstOrDefault(x => x.JOBNO == findSubJob.MASTER_JOBNO);
                    MasterJobNo = findMasterJob?.JOBNO;
                }
            }
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
                costGroupCollection = null;
                areaCollection = null;
                variationCodes = null;
                deliverableCollection = null;
                JOBCOST_HDR masterJob = primeroUOW.JOBCOST_HDR.FirstOrDefault(x => x.JOBNO == value);
                masterJobCode = masterJob?.JOBCODE;
                filteredExoTimeAuthorisations = this.exoTimeAuthorisations.Where(x => x.MasterJobNo == value).ToList();
                this.Update();
            }
        }

        List<JOB_COSTGROUPS> costGroupCollection;
        public List<JOB_COSTGROUPS> CostGroupCollection
        {
            get
            {
                if (costGroupCollection == null && primeroUOW != null)
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

                areaCollection = null;
                variationCodes = null;
                deliverableCollection = null;
                Timesheet.COST_GROUP = value;
                costGroupCode = primeroUOW.JOB_COSTGROUPS.FirstOrDefault(x => x.SEQNO == value)?.SHORTCODE;

                if (value == null)
                    filteredExoTimeAuthorisations = this.exoTimeAuthorisations.Where(x => x.MasterJobNo == value).ToList();
                else
                    filteredExoTimeAuthorisations = filteredExoTimeAuthorisations.Where(x => x.DisciplineId == value).ToList();

                this.Update();
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
                    variationCodes = filteredExoTimeAuthorisations.Select(x => x.VariationCode).Distinct().ToList();

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

                deliverableCollection = null;

                if(masterJobCode != null && masterJobCode != string.Empty && value != null && value != string.Empty)
                {
                    string constructSubJobCode = masterJobCode + "-" + value + "-" + BluePrintsResources.Default_Sub_Area + "-" + BluePrintsResources.Default_Design_Phase;
                    JOBCOST_HDR subJob = primeroUOW.JOBCOST_HDR.FirstOrDefault(x => x.JOBCODE == constructSubJobCode);
                    if (subJob != null)
                    {
                        string title = subJob.JOBCODE + " : " + subJob.TITLE;
                        if (title.Length >= 60)
                            title = title.Substring(0, 59);

                        Timesheet.JOBNO = subJob.JOBNO;
                        Timesheet.TITLE = title;
                    }
                }

                this.Update();
            }
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
                if(DeliverableCollection != null)
                {
                    BASELINE_ITEM deliverable = DeliverableCollection.FirstOrDefault(x => x.INTERNAL_NUM == value);
                    if(deliverable != null && deliverable.DOCTYPE != null)
                    {
                        JOB_COSTTYPES costType = primeroUOW.JOB_COSTTYPES.FirstOrDefault(x => x.SHORTCODE == deliverable.DOCTYPE.CODE);
                        if (costType != null)
                        {
                            Timesheet.COST_TYPE = costType.SEQNO;
                            Timesheet.X_NARRATIVE = deliverable.INTERNAL_NUM;
                        }
                    }
                }
            }
        }
    }
}
