using BaseModel.DataModel.EntityFramework;
using BaseModel.Misc;
using BluePrints.Common;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.PrimeroData;
using EntityFramework.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace BluePrints.PrimeroData
{

    public partial class PrimeroEntities : DbContext
    {
        public static List<X_PURCHORD_LINE> GetPurchaseOrdersSummary(string projectNumber, DateTime cutOffDate)
        {
            using (PrimeroEntities dbContext = new PrimeroEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@ProjectNumber", projectNumber);
                SqlParameter cutOffDateParameter = new SqlParameter("@CutOffDate", cutOffDate);
                List<X_PURCHORD_LINE> purchaseOrderLines = dbContext.Database.SqlQuery<X_PURCHORD_LINE>("X_PURCHORD_LINES @ProjectNumber, @CutOffDate", projectNumberParameter, cutOffDateParameter).ToList();
                return purchaseOrderLines;
            }
        }

        public static List<X_TIME_TRANSACTION> GetTimeSummary(string projectNumber, DateTime cutOffDate)
        {
            using (PrimeroEntities dbContext = new PrimeroEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@ProjectNumber", projectNumber);
                SqlParameter cutOffDateParameter = new SqlParameter("@CutOffDate", cutOffDate);
                List<X_TIME_TRANSACTION> purchaseOrderLines = dbContext.Database.SqlQuery<X_TIME_TRANSACTION>("X_TIME_TRANSACTIONS @ProjectNumber, @CutOffDate", projectNumberParameter, cutOffDateParameter).ToList();
                return purchaseOrderLines;
            }
        }

        public static List<X_MATERIAL_TRANSACTION> GetMaterialSummary(string projectNumber, DateTime cutOffDate)
        {
            using (PrimeroEntities dbContext = new PrimeroEntities())
            {
                dbContext.Database.CommandTimeout = 5000;
                SqlParameter projectNumberParameter = new SqlParameter("@ProjectNumber", projectNumber);
                SqlParameter cutOffDateParameter = new SqlParameter("@CutOffDate", cutOffDate);
                List<X_MATERIAL_TRANSACTION> purchaseOrderLines = dbContext.Database.SqlQuery<X_MATERIAL_TRANSACTION>("X_MATERIAL_TRANSACTIONS @ProjectNumber, @CutOffDate", projectNumberParameter, cutOffDateParameter).ToList();
                return purchaseOrderLines;
            }
        }
    }
}