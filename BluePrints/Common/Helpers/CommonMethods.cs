using BluePrints.Common.Projections;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Management;

namespace BluePrints.Common.Helpers
{
    public static class CommonMethods
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
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

        public static void SubJobLineValueChanged(string field_name, object old_value, object new_value, ExoSubJobEditableProjection projection, IEnumerable<ExoSubJobEditableProjection> projections, bool isNew, string projectNumber, IPrimeroEntitiesUnitOfWork localPrimeroUnitOfWork, IMessageBoxService MessageBoxService, IDialogService BulkColumnEditDialogService, JOBCOST_HDR masterJob, Action raiseCommodityCodesChangeAction = null, Action raiseStockCodesChangeAction = null)
        {
            if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().SubJobTitle)))
            {
                ExoMethods.CommitSubJobTitle(projection, projectNumber, localPrimeroUnitOfWork, MessageBoxService);
                ExoMethods.ViewUpdateSubJobTitle(projection, projections, localPrimeroUnitOfWork, projectNumber, projection.SubJobCode, true);
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().DisciplineName)))
            {
                ExoMethods.CommitCostGroupName(projection, localPrimeroUnitOfWork, MessageBoxService);
                ExoMethods.ViewUpdateCostGroupTitle(projection, projections, localPrimeroUnitOfWork, projection.DisciplineCode, false);
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().SubJobCode)))
            {
                ExoMethods.CommitLineSubJob(projection, true, BulkColumnEditDialogService, masterJob, projectNumber, localPrimeroUnitOfWork);
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().DisciplineCode)))
            {
                ExoMethods.CommitLineDiscipline(projection, true, BulkColumnEditDialogService, masterJob, projectNumber, localPrimeroUnitOfWork);
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().CommodityCode)))
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
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().StockCode)))
            {
                if (new_value != null)
                {
                    STOCK_ITEMS stock_item = ExoQueries.FindSTOCK_ITEM(localPrimeroUnitOfWork, new_value.ToString());
                    projection.StockName = stock_item.DESCRIPTION;
                    if (ExoMethods.CommitLineCommodity(projection, stock_item, true, BulkColumnEditDialogService, masterJob, projectNumber, localPrimeroUnitOfWork))
                        raiseStockCodesChangeAction?.Invoke();
                }
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().VariationCode)))
            {
                ExoMethods.CommitLineVariation(projection, localPrimeroUnitOfWork);
            }
            else if (field_name.Contains(BindableBase.GetPropertyName(() => new ExoSubJobEditableProjection().ExoBudget)))
            {
                ExoMethods.CommitLineBudgetCost(projection, localPrimeroUnitOfWork);
            }

            projection.Update();
        }
    }
}
