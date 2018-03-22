using BaseModel.DataModel;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.P6EntitiesDataModel;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm.POCO;
using System;
using System.Linq;
using System.Collections.Generic;
using BluePrints.Data;

namespace BaseModel.ViewModel.Dialogs
{
    public class BookTimeSheetViewModel
    {
        public static BookTimeSheetViewModel Create(PROJECT project, IDeliverable deliverable)
        {
            return ViewModelSource.Create(() => new BookTimeSheetViewModel(project, deliverable));
        }

        public DateTime BookDate { get; set; }
        private readonly IDeliverable deliverable;
        private IPrimeroEntitiesUnitOfWork primeroUnitOfWork = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
        public List<JOBCOST_HDR> PSUBJOBCollection { get; set; }
        public JOBCOST_HDR Selected_SubJob { get; set; }
        public List<PrimeroDiscipline> PDISCIPLINECollection { get; set; }
        public PrimeroDiscipline Selected_Discipline { get; set; }
        public List<PrimeroCommodity> PCOMMODITYCollection { get; set; }
        public PrimeroCommodity Selected_Commodity { get; set; }
        public List<PrimeroResource> PRESOURCECollection { get; set; }
        public PrimeroResource Selected_Resource { get; set; }
        public float BookHours { get; set; }
        protected BookTimeSheetViewModel(PROJECT project, IDeliverable deliverable)
        {
            BookDate = DateTime.Now.Date;
            initializeCollection();
            this.deliverable = deliverable;

            JOBCOST_HDR masterJob = primeroUnitOfWork.JOBCOST_HDR.FirstOrDefault(x => x.JOBCODE == project.NUMBER);
            if(masterJob != null)
            {
                PSUBJOBCollection = primeroUnitOfWork.JOBCOST_HDR.Where(x => x.MASTER_JOBNO == masterJob.JOBNO).ToList();
            }

            var availableLines = from JOBCOST_LINES in primeroUnitOfWork.JOBCOST_LINES
                                  join JOB_COSTGROUPS in primeroUnitOfWork.JOB_COSTGROUPS
                                  on JOBCOST_LINES.COST_CENTRE2 equals JOB_COSTGROUPS.SEQNO
                                  join JOB_COSTTYPES in primeroUnitOfWork.JOB_COSTTYPES
                                  on JOBCOST_LINES.COST_CENTRE equals JOB_COSTTYPES.SEQNO
                                  join SUBJOB in primeroUnitOfWork.JOBCOST_HDR
                                  on JOBCOST_LINES.JOBNO equals SUBJOB.JOBNO
                                  join MAINJOB in primeroUnitOfWork.JOBCOST_HDR
                                  on SUBJOB.MASTER_JOBNO equals MAINJOB.JOBNO
                                  join JOB_RESOURCE_ALLOCATION in primeroUnitOfWork.JOB_RESOURCE_ALLOCATION
                                  on JOBCOST_LINES.JOBNO equals JOB_RESOURCE_ALLOCATION.JOBNO
                                  join JOBCOST_RESOURCE in primeroUnitOfWork.JOBCOST_RESOURCE
                                  on JOB_RESOURCE_ALLOCATION.RESOURCE_SEQNO equals JOBCOST_RESOURCE.STAFFNO
                                  where MAINJOB.JOBCODE == project.NUMBER
                                  select new { SUBJOB.JOBCODE, DISCIPLINE_ID = JOBCOST_LINES.COST_CENTRE2, DISCIPLINE_CODE = JOB_COSTGROUPS.SHORTCODE, DISCIPLINE_NAME = JOB_COSTGROUPS.COSTDESC, COMMODITY_ID = JOBCOST_LINES.COST_CENTRE, COMMODITY_CODE = JOBCOST_LINES.STOCKCODE, COMMODITY_NAME = JOB_COSTTYPES.SHORTCODE, RESOURCE_ID = JOBCOST_RESOURCE.STAFFNO, JOBCOST_RESOURCE.RESOURCENAME, JOBCOST_RESOURCE.DEFAULT_STOCKCODE };

            var availableLinesList = availableLines.ToList();
            foreach(var availableLine in availableLinesList)
            {
                if(!PDISCIPLINECollection.Any(x => x.Id == availableLine.DISCIPLINE_ID))
                {
                    PrimeroDiscipline newDiscipline = new PrimeroDiscipline();
                    newDiscipline.Id = availableLine.DISCIPLINE_ID;
                    newDiscipline.Code = availableLine.DISCIPLINE_CODE;
                    newDiscipline.Name = availableLine.DISCIPLINE_NAME;
                    PDISCIPLINECollection.Add(newDiscipline);
                }

                if(!PCOMMODITYCollection.Any(x => x.Id == availableLine.COMMODITY_ID))
                {
                    PrimeroCommodity newCommodity = new PrimeroCommodity();
                    newCommodity.Id = availableLine.COMMODITY_ID;
                    newCommodity.Code = availableLine.COMMODITY_CODE;
                    PCOMMODITYCollection.Add(newCommodity);
                }

                if(!PRESOURCECollection.Any(x => x.Id == availableLine.RESOURCE_ID))
                {
                    PrimeroResource newResource = new PrimeroResource();
                    newResource.Id = availableLine.RESOURCE_ID;
                    newResource.Name = availableLine.RESOURCENAME;
                    newResource.StockCode = availableLine.DEFAULT_STOCKCODE;
                    PRESOURCECollection.Add(newResource);
                }
            }

            defaultDeliverableSelection(deliverable);
        }

        private void defaultDeliverableSelection(IDeliverable deliverable)
        {
            Selected_SubJob = PSUBJOBCollection.FirstOrDefault(x => x.JOBCODE == deliverable.Subjob_Name);
            Selected_Discipline = PDISCIPLINECollection.FirstOrDefault(x => x.Code == deliverable.Discipline_Code);
            Selected_Commodity = PCOMMODITYCollection.FirstOrDefault(x => x.Code == deliverable.Commodity_Code);
        }

        private void initializeCollection()
        {
            PDISCIPLINECollection = new List<PrimeroDiscipline>();
            PCOMMODITYCollection = new List<PrimeroCommodity>();
            PRESOURCECollection = new List<PrimeroResource>();
        }
    }

    public class PrimeroDiscipline
    {
        public int? Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class PrimeroCommodity
    {
        public int? Id { get; set; }
        public string Code { get; set; }
    }

    public class PrimeroStockCode
    {
        public string Code { get; set; }
    }

    public class PrimeroResource
    {
        public int? Id { get; set; }
        public string StockCode { get; set; }
        public string Name { get; set; }
    }
}