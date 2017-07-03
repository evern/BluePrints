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
    public class STOCK_CODEProjection : BluePrintsProjectionBase<STOCK_CODE>, IReportableGroup
    {
        public STOCK_CODEProjection()
            : base()
        {
        }

        public List<IReportable> Reportables { get; set; }

        public IDeliverable Deliverable => throw new NotImplementedException();

        public ProgressStats Stats
        {
            get { return new ProgressStats(Reportables.Select(x => x.Stats)); }
            set => throw new NotImplementedException();
        }

        public string ReportableItem_Name
        {
            get { return string.Empty; }
        }

        public string Stock_Code
        {
            get { return Entity.CODE; }
        }

        public Guid? Area_Guid => Entity.GUID_AREA;

        public Guid? SubArea_Guid => Entity.GUID_SUBAREA;

        public decimal TotalHoursIncludeByDuration
        {
            get { return Reportables.Sum(x => x.TotalHoursIncludeByDuration); }
        }

        public decimal EstimatedHours
        {
            get { return Reportables.Sum(x => x.EstimatedHours); }
        }

        public decimal TotalHours
        {
            get { return Reportables.Sum(x => x.TotalHours); }
        }

        public decimal EstimatedCosts
        {
            get { return Reportables.Sum(x => x.EstimatedCosts); }
        }

        public decimal TotalCosts
        {
            get { return Reportables.Sum(x => x.TotalCosts); }
        }

        public DateTime ReportingDataDate { get; set; }
        public List<PROGRESS_ITEM> PROGRESS_ITEMS
        {
            get { return Reportables.SelectMany(x => x.PROGRESS_ITEMS).ToList(); }
            set => throw new NotImplementedException();
        }

        public decimal Estimated_Quantity
        {
            get { return Reportables.Sum(x => x.Estimated_Quantity); }
        }

        public decimal Total_Quantity => Reportables.Sum(x => x.Estimated_Quantity);

        public decimal ItemRate => Reportables.Sum(x => x.ItemRate);

        public string UOM => Entity.UOM;

        public string Commodity_Code => string.Empty;

        public Guid? Workpack_Guid => null;

        public Guid OriginalEntityKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Refreshes current row
        /// </summary>
        public void Update()
        {
            RaisePropertyChanged();
        }

        public void UpdateGroup()
        {
            Reportables.ForEach(x => x.Update());
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