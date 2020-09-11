namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common;
    using BluePrints.Common.Base;
    using BluePrints.PrimeroData;
    using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
    using DevExpress.Mvvm;
    using DevExpress.XtraEditors.DXErrorProvider;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class VARIATION_CONSTRUCTION_ITEM : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate, IDXDataErrorInfo
    {
        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        public VariationConstructionItemType TYPEProxy
        {
            get => TYPE;
            set
            {
                TYPE = value;
                validJOBCOST_HDRNames = null;
                Update();
            }
        }

        [NotMapped]
        [StringLength(50)]
        public string COSTGROUPProxy
        {
            get => COSTGROUP;
            set
            {
                COSTGROUP = value;
                taggedValidJobCostTypes = null;
                Update();
            }
        }

        [NotMapped]
        [StringLength(50)]
        public string COSTTYPEProxy
        {
            get => COSTTYPE;
            set
            {
                COSTTYPE = value;
                taggedValidJobStockItems = null;
                Update();
            }
        }

        [NotMapped]
        private string defaultCOSTGROUP
        {
            get
            {
               return COSTGROUP == null || COSTGROUP == string.Empty ? null : COSTGROUP.Length > 2 ? string.Concat(COSTGROUP.Substring(0, 2), "01") : COSTGROUP;
            }
        }

        [NotMapped]
        private IPrimeroEntitiesUnitOfWork primeroUOW;

        public void SetUnitOfWork(IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork)
        {
            primeroUOW = primeroEntitiesUnitOfWork;
        }

        public void SetJOBCOST_HDRS(List<JOBCOST_HDR> ProjectJOBCOST_HDRS)
        {
            JOBCOST_HDRNames = ProjectJOBCOST_HDRS.OrderBy(x => x.JOBCODE).Select(x => x.JOBCODE).ToList();
            establishValidJOBCOST_HDRNames();
        }

        [NotMapped]
        List<string> JOBCOST_HDRNames;

        [NotMapped]
        List<string> validJOBCOST_HDRNames;

        [NotMapped]
        public IEnumerable<string> JOBCOST_HDRNameCollection
        {
            get
            {
                if (primeroUOW == null || JOBCOST_HDRNames == null)
                    return null;

                establishValidJOBCOST_HDRNames();
                return validJOBCOST_HDRNames;
            }
        }

        private void establishValidJOBCOST_HDRNames()
        {
            if (validJOBCOST_HDRNames == null)
            {
                if (TYPE == Common.VariationConstructionItemType.Engineering)
                    validJOBCOST_HDRNames = JOBCOST_HDRNames.Where(x => x.Contains("D")).ToList();
                else if (TYPE == Common.VariationConstructionItemType.Equipment)
                    validJOBCOST_HDRNames = JOBCOST_HDRNames.Where(x => x.Contains("I")).ToList();
                else if (TYPE == Common.VariationConstructionItemType.Management)
                    validJOBCOST_HDRNames = JOBCOST_HDRNames.Where(x => x.Contains("I")).ToList();
                else if (TYPE == Common.VariationConstructionItemType.Material)
                    validJOBCOST_HDRNames = JOBCOST_HDRNames.Where(x => x.Contains("P")).ToList();
                else
                    validJOBCOST_HDRNames = JOBCOST_HDRNames.Where(x => x.Contains("C")).ToList();
            }
        }

        [NotMapped]
        List<JOB_COSTTYPES> taggedValidJobCostTypes;

        [NotMapped]
        public List<JOB_COSTTYPES> TaggedValidJOB_COSTTYPES
        {
            get
            {
                if (primeroUOW == null)
                    return null;

                if (taggedValidJobCostTypes == null)
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

                    JOB_COSTGROUPS findJOB_COSTGROUPS = COSTGROUP == null || COSTGROUP == string.Empty ? null : primeroUOW.JOB_COSTGROUPS.FirstOrDefault(x => x.SHORTCODE == defaultCOSTGROUP);
                    if (findJOB_COSTGROUPS != null)
                    {
                        foreach (JOB_COSTTYPES jobCostTypes in taggedValidJobCostTypes.Where(x => x.DEF_COSTGROUP == findJOB_COSTGROUPS.SEQNO))
                        {
                            jobCostTypes.IsValid = true;
                        }
                    }
                }

                return taggedValidJobCostTypes;
            }
        }

        [NotMapped]
        List<STOCK_ITEMS> taggedValidJobStockItems;
        [NotMapped]
        public List<STOCK_ITEMS> TaggedValidSTOCK_ITEMS
        {
            get
            {
                if (primeroUOW == null)
                    return null;

                if (taggedValidJobStockItems == null)
                {
                    taggedValidJobStockItems = new List<STOCK_ITEMS>();
                    foreach (STOCK_ITEMS STOCK_ITEMS in primeroUOW.STOCK_ITEMS.OrderBy(x => x.STOCKCODE))
                    {
                        STOCK_ITEMS copySTOCK_ITEMS = new STOCK_ITEMS();
                        copySTOCK_ITEMS.STOCKCODE = STOCK_ITEMS.STOCKCODE;
                        copySTOCK_ITEMS.DESCRIPTION = STOCK_ITEMS.DESCRIPTION;
                        copySTOCK_ITEMS.COSTTYPE = STOCK_ITEMS.COSTTYPE;
                        copySTOCK_ITEMS.COSTGROUP = STOCK_ITEMS.COSTGROUP;

                        taggedValidJobStockItems.Add(copySTOCK_ITEMS);
                    }

                    JOB_COSTTYPES findJOB_COSTTYPES = COSTTYPE == null || COSTTYPE == string.Empty ? null : primeroUOW.JOB_COSTTYPES.FirstOrDefault(x => x.SHORTCODE == COSTTYPE);
                    if (findJOB_COSTTYPES != null)
                    {
                        foreach (STOCK_ITEMS stockItem in taggedValidJobStockItems.Where(x => x.COSTTYPE == findJOB_COSTTYPES.SEQNO))
                        {
                            stockItem.IsValid = true;
                        }
                    }
                }

                return taggedValidJobStockItems;
            }
        }

        [NotMapped]
        public decimal TotalCosts => HOURS * RATE;

        [NotMapped]
        public string Office => this.VARIATION_CONSTRUCTION.PROJECT.NUMBER + " " + this.VARIATION_CONSTRUCTION.PROJECT.OfficeName;

        public void GetPropertyError(string propertyName, ErrorInfo info)
        {
            if (propertyName == BindableBase.GetPropertyName(() => new VARIATION_CONSTRUCTION_ITEM().SUBJOB))
            {
                if (validJOBCOST_HDRNames != null)
                {
                    if (!validJOBCOST_HDRNames.Any(x => x == SUBJOB))
                    {
                        info.ErrorText = "Invalid sub job code, please check type";
                    }
                }
            }
        }

        public void GetError(ErrorInfo info)
        {
        }
    }
}