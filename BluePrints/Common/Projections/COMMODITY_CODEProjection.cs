using BaseModel.Attributes;
using BaseModel.Data.Helpers;
using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class COMMODITY_CODEProjection : BluePrintsProjectionBase<COMMODITY_CODE>, ICanUpdate
    {
        public COMMODITY_CODEProjection()
            : base()
        {
        }

        public JOB_COSTTYPES EXO_COSTTYPE { get; set; }
        public STOCK_ITEMS EXO_STOCKITEM { get; set; }

        public bool EXO_COSTTYPES_COSTGROUP_NOTFOUND { get; set; }
    }

    public static class COMMODITY_CODEProjectionQueries
    {
        public static IQueryable<COMMODITY_CODEProjection> COMMODITY_CODE_Transformation(
            IQueryable<COMMODITY_CODE> COMMODITY_CODES, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            List<COMMODITY_CODEProjection> returnCOMMODITY_CODEProjection = new List<COMMODITY_CODEProjection>();

            foreach (COMMODITY_CODE COMMODITY_CODE in COMMODITY_CODES)
            {
                COMMODITY_CODEProjection newCOMMODITY_CODE = new COMMODITY_CODEProjection();
                newCOMMODITY_CODE.Entity = COMMODITY_CODE;

                newCOMMODITY_CODE.EXO_COSTTYPE = primeroUnitOfWork.JOB_COSTTYPES.FirstOrDefault(x => x.SHORTCODE == COMMODITY_CODE.CODE);

                    //Cost group for stock code and commodity code shouldn't be validated in EXO
                    //if (COMMODITY_CODE.DEFAULT_COSTGROUP == null || COMMODITY_CODE.DEFAULT_COSTGROUP == string.Empty)
                    //{
                    //    newCOMMODITY_CODE.EXO_COSTTYPE = primeroUnitOfWork.JOB_COSTTYPES.FirstOrDefault(x => x.SHORTCODE == COMMODITY_CODE.CODE && x.DEF_COSTGROUP == 0);
                    //}
                    //else
                    //{
                    //    JOB_COSTGROUPS costGroup = primeroUnitOfWork.JOB_COSTGROUPS.FirstOrDefault(x => x.SHORTCODE == COMMODITY_CODE.DEFAULT_COSTGROUP);
                    //    if (costGroup != null)
                    //    {
                    //        newCOMMODITY_CODE.EXO_COSTTYPE = primeroUnitOfWork.JOB_COSTTYPES.FirstOrDefault(x => x.SHORTCODE == COMMODITY_CODE.CODE && x.DEF_COSTGROUP == costGroup.SEQNO);
                    //    }
                    //    else
                    //        newCOMMODITY_CODE.EXO_COSTTYPES_COSTGROUP_NOTFOUND = true;
                    //}

                newCOMMODITY_CODE.EXO_STOCKITEM = primeroUnitOfWork.STOCK_ITEMS.FirstOrDefault(x => x.STOCKCODE == COMMODITY_CODE.DEFAULT_STOCKCODE);
                returnCOMMODITY_CODEProjection.Add(newCOMMODITY_CODE);
            }

            return returnCOMMODITY_CODEProjection.AsQueryable();
        }
    }
}