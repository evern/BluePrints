using BaseModel.DataModel.EntityFramework;
using BaseModel.Misc;
using BluePrints.Common;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
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
        public static List<X_PURCHORD_LINE_DETAIL> GetPurchaseOrdersDetail(IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork, string projectNumber, DateTime cutOffDate)
        {
            SqlParameter projectNumberParameter = new SqlParameter("@ProjectNumber", projectNumber);
            SqlParameter cutOffDateParameter = new SqlParameter("@CutOffDate", cutOffDate);
            List<X_PURCHORD_LINE_DETAIL> purchaseOrderLines = primeroEntitiesUnitOfWork.DbContext.Database.SqlQuery<X_PURCHORD_LINE_DETAIL>("X_PURCHORD_LINE_DETAILS_V1 @ProjectNumber, @CutOffDate", projectNumberParameter, cutOffDateParameter).ToList();
            return purchaseOrderLines;
        }

        public static List<X_PURCHORD_LINE> GetPurchaseOrdersSummary(IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork, string projectNumber, DateTime cutOffDate)
        {
            SqlParameter projectNumberParameter = new SqlParameter("@ProjectNumber", projectNumber);
            SqlParameter cutOffDateParameter = new SqlParameter("@CutOffDate", cutOffDate);
            List<X_PURCHORD_LINE> purchaseOrderLines = primeroEntitiesUnitOfWork.DbContext.Database.SqlQuery<X_PURCHORD_LINE>("X_PURCHORD_LINES @ProjectNumber, @CutOffDate", projectNumberParameter, cutOffDateParameter).ToList();
            return purchaseOrderLines;
        }

        public static List<X_TIME_TRANSACTION> GetTimeSummary(IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork, string projectNumber, DateTime cutOffDate)
        {
            SqlParameter projectNumberParameter = new SqlParameter("@ProjectNumber", projectNumber);
            SqlParameter cutOffDateParameter = new SqlParameter("@CutOffDate", cutOffDate);
            List<X_TIME_TRANSACTION> purchaseOrderLines = primeroEntitiesUnitOfWork.DbContext.Database.SqlQuery<X_TIME_TRANSACTION>("X_TIME_TRANSACTIONS @ProjectNumber, @CutOffDate", projectNumberParameter, cutOffDateParameter).ToList();
            return purchaseOrderLines;
        }

        public static List<X_MATERIAL_TRANSACTION> GetMaterialSummary(IPrimeroEntitiesUnitOfWork primeroEntitiesUnitOfWork, string projectNumber, DateTime cutOffDate)
        {
            SqlParameter projectNumberParameter = new SqlParameter("@ProjectNumber", projectNumber);
            SqlParameter cutOffDateParameter = new SqlParameter("@CutOffDate", cutOffDate);
            List<X_MATERIAL_TRANSACTION> purchaseOrderLines = primeroEntitiesUnitOfWork.DbContext.Database.SqlQuery<X_MATERIAL_TRANSACTION>("X_MATERIAL_TRANSACTIONS @ProjectNumber, @CutOffDate", projectNumberParameter, cutOffDateParameter).ToList();
            return purchaseOrderLines;
        }
    }
}