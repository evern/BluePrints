using BaseModel.DataModel;
using BaseModel.Misc;
using BluePrints.Common.Resources;
using BluePrints.Data;
using BluePrints.PrimeroData;
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
        public JOB_TIMESHEETS timesheet;
        public DesignTimesheet()
        {

        }

        public DesignTimesheet(List<ExoTimeAuthorisation> exoTimeAuthorisations, IQueryable<BASELINE_ITEM> deliverables, JOB_TIMESHEETS timesheet = null)
        {
            this.exoTimeAuthorisations = exoTimeAuthorisations;
            this.deliverables = deliverables;
            this.timesheet = timesheet;

            filteredExoTimeAuthorisations = new List<ExoTimeAuthorisation>();

            if(this.timesheet == null)
                timesheet = new JOB_TIMESHEETS();
        }

        List<ExoTimeAuthorisation> filteredExoTimeAuthorisations;
        List<int> jobNumberCollection;
        public List<int> JobNumberCollection
        {
            get
            {
                if (jobNumberCollection == null)
                    jobNumberCollection = this.exoTimeAuthorisations.Select(x => x.MasterJobNo).ToList();

                return jobNumberCollection;
            }
        }

        int jobNumber;
        public int JobNumber
        {
            get => jobNumber;
            set
            {
                disciplineCodeCollection = null;
                areaSubAreaCollection = null;
                jobNumber = value;
                filteredExoTimeAuthorisations = this.exoTimeAuthorisations.Where(x => x.MasterJobNo == value).ToList();
            }
        }

        List<string> disciplineCodeCollection;
        public List<string> DisciplineCodeCollection
        {
            get
            {
                if (disciplineCodeCollection == null)
                    disciplineCodeCollection = filteredExoTimeAuthorisations.Select(x => x.DisciplineCode).ToList();

                return disciplineCodeCollection;
            }
        }

        string disciplineSubCode;
        string disciplineCode;
        public string DisciplineCode
        {
            get => disciplineCode;
            set
            {
                areaSubAreaCollection = null;
                disciplineCode = value;
                disciplineSubCode = value.Length > 2 ? value.Substring(0, 2) : string.Empty;
                filteredExoTimeAuthorisations = filteredExoTimeAuthorisations.Where(x => x.DisciplineCode == value).ToList();
            }
        }

        List<string> areaSubAreaCollection;
        public List<string> AreaSubAreaCollection
        {
            get
            {
                if (areaSubAreaCollection == null)
                    areaSubAreaCollection = filteredExoTimeAuthorisations.Select(x => x.AreaSubAreaCode).ToList();

                return areaSubAreaCollection;
            }
        }

        string areaCode;
        string subAreaCode;
        string areaSubareaCode;
        public string AreaSubAreaCode
        {
            get => areaSubareaCode;
            set
            {
                areaSubareaCode = value;
                string[] areaSubAreaSplit = value.Split('-');
                if(areaSubAreaSplit.Count() == 2)
                {
                    areaCode = areaSubAreaSplit[0];
                    subAreaCode = areaSubAreaSplit[1];
                }
            }
        }

        List<BASELINE_ITEM> deliverableCollection;
        public List<BASELINE_ITEM> DeliverableCollection
        {
            get
            {
                string jobNumberStr = JobNumber.ToString();
                if (deliverableCollection == null)
                    deliverableCollection = deliverables.Where(x => x.BASELINE.PROJECT.NUMBER == jobNumberStr && x.AREA.INTERNAL_NUM == areaCode && x.AREA1.INTERNAL_NUM == subAreaCode && x.DISCIPLINE.CODE == disciplineSubCode).ToList();

                return deliverableCollection;
            }
        }

        public string DeliverableInternalName { get; set; }
    }
}
