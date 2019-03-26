using BaseModel.Attributes;
using BaseModel.Data.Helpers;
using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
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

        public int? EXO_COSTTYPE_SEQNO { get; set; }
        public int? EXO_COSTGROUP_SEQNO { get; set; }

        public bool IsCostTypeInExo => EXO_COSTTYPE_SEQNO != null;
        public bool IsCostGroupInExo => EXO_COSTGROUP_SEQNO != null;
    }

    public static class COMMODITY_CODEProjectionQueries
    {
        public static IQueryable<COMMODITY_CODEProjection> COMMODITY_CODE_Transformation(
            IQueryable<COMMODITY_CODE> COMMODITY_CODES)
        {
            return
                COMMODITY_CODES.OrderBy(x => x.CODE).ToArray()
                    .Select(
                        COMMODITY_CODE =>
                            new COMMODITY_CODEProjection()
                            {
                                Entity = COMMODITY_CODE
                            }).AsQueryable();
        }
    }
}