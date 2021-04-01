using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Management;

namespace BluePrints.Common.Helpers
{
    public static class CommonMethods
    {
        public static DateTime GetStartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Gets the Disk ID for this computer
        /// </summary>
        /// <returns>Processor ID</returns>
        public static string GetHWID()
        {
            string drive = "C";
            ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + drive + @":""");
            dsk.Get();
            string volumeSerial = dsk["VolumeSerialNumber"].ToString();
            return volumeSerial;
        }

        public static void SubJobLineValueChanged(string field_name, object old_value, object new_value, ExoSubJobProjection projection, IEnumerable<ExoSubJobProjection> projections, bool isNew, string projectNumber, IPrimeroEntitiesUnitOfWork localPrimeroUnitOfWork, IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork, IMessageBoxService MessageBoxService, IDialogService BulkColumnEditDialogService, JOBCOST_HDR masterJob, Action raiseCommodityCodesChangeAction = null, Action raiseStockCodesChangeAction = null)
        {
            if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobProjection().SubJobTitle)))
            {
                ExoMethods.CommitSubJobTitle(projection, projectNumber, localPrimeroUnitOfWork, MessageBoxService);
                ExoMethods.ViewUpdateSubJobTitle(projection, projections, localPrimeroUnitOfWork, projectNumber, projection.SubJobCode, true);
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobProjection().DisciplineName)))
            {
                ExoMethods.CommitCostGroupName(projection, localPrimeroUnitOfWork, MessageBoxService);
                ExoMethods.ViewUpdateCostGroupTitle(projection, projections, localPrimeroUnitOfWork, projection.DisciplineCode, false);
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobProjection().SubJobCode)))
            {
                ExoMethods.CommitLineSubJob(projection, true, BulkColumnEditDialogService, masterJob, projectNumber, localPrimeroUnitOfWork);
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobProjection().DisciplineCode)))
            {
                ExoMethods.CommitLineDiscipline(projection, true, BulkColumnEditDialogService, masterJob, projectNumber, localPrimeroUnitOfWork);
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobProjection().CommodityCode)))
            {
                //stock item cannot be added, so it must exists before commodity can be added using it
                string stockCode = projection.GetStockCode();
                STOCK_ITEMS stock_item = ExoQueries.FindSTOCK_ITEM(localPrimeroUnitOfWork, stockCode);
                if (stock_item != null)
                {
                    projection.StockName = stock_item.DESCRIPTION;
                    if (ExoMethods.CommitLineCommodity(projection, stock_item, true, BulkColumnEditDialogService, masterJob, projectNumber, localPrimeroUnitOfWork))
                        raiseCommodityCodesChangeAction?.Invoke();
                }
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobProjection().StockCode)))
            {
                if (new_value != null)
                {
                    STOCK_ITEMS stock_item = ExoQueries.FindSTOCK_ITEM(localPrimeroUnitOfWork, new_value.ToString());
                    projection.StockName = stock_item.DESCRIPTION;
                    if (ExoMethods.CommitLineCommodity(projection, stock_item, true, BulkColumnEditDialogService, masterJob, projectNumber, localPrimeroUnitOfWork))
                        raiseStockCodesChangeAction?.Invoke();
                }
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobProjection().VariationCode)))
            {
                ExoMethods.CommitLineVariation(projection, localPrimeroUnitOfWork);
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobProjection().ExoBudget)))
            {
                ExoMethods.CommitLineBudgetCost(projection, localPrimeroUnitOfWork, bluePrintsEntitiesUnitOfWork);
            }

            projection.Update();
        }

        /// <summary>
        /// prevent dynamically generated dates column from being saved when saving layout
        /// </summary>
        public static void AddSaveLayoutHandler(List<GridColumn> GridColumns)
        {
            foreach (GridColumn column in GridColumns)
            {
                DateTime parsedate;
                if (DateTime.TryParse(column.FieldName, out parsedate))
                    column.AddHandler(DXSerializer.AllowPropertyEvent, new AllowPropertyEventHandler(column_AllowProperty));
            }
        }

        /// <summary>
        /// prevent saving all properties
        /// </summary>
        private static void column_AllowProperty(object sender, AllowPropertyEventArgs e)
        {
            e.Allow = false;
        }
    }
}
