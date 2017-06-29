using BaseModel.Attributes;
using BaseModel.Data.Helpers;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    public class STOCK_CODEProjection : BluePrintsProjectionBase<STOCK_CODE>
    {
        public STOCK_CODEProjection()
            : base()
        {
        }


        /// <summary>
        /// Refreshes current row
        /// </summary>
        public void Update()
        {
            RaisePropertyChanged();
        }
    }

    public static class STOCK_CODEProjectionQueries
    {
        public static IQueryable<STOCK_CODEProjection> STOCK_CODEProjectionQuery(
            IQueryable<STOCK_CODE> STOCK_CODES)
        {
            return
                STOCK_CODES.OrderBy(x => x.CODE).ToArray()
                    .Select(
                        stock_code =>
                            new STOCK_CODEProjection()
                            {
                                Entity = stock_code,
                            }).AsQueryable();
        }
    }
}