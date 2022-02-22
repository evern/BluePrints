namespace BluePrints.PrimeroData
{
    using BaseModel.DataModel;
    using BluePrints.Common;
    using BluePrints.Common.Misc;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class X_JOB_TRANSACTIONS_DETAIL_V5 : EntityBase
    {
        public X_JOB_TRANSACTIONS_DETAIL_V5()
        {
            JobNoChangeTracking = new ChangeTrackableProperty<int?>(() => JOBNO, () => NEW_JOBNO, () => OLD_JOBCODE);
            CostGroupChangeTracking = new ChangeTrackableProperty<int?>(() => COST_GROUP_NO, () => NEW_COST_GROUP_NO, () => OLD_DISCIPLINECODE);
            CostTypeChangeTracking = new ChangeTrackableProperty<int?>(() => COST_TYPE_NO, () => NEW_COST_TYPE_NO, () => OLD_COMMODITYCODE);
            StockCodeChangeTracking = new ChangeTrackableProperty<string>(() => STOCKCODE, () => NEW_STOCK_CODE, () => OLD_STOCK_CODE);
            VariationCodeChangeTracking = new ChangeTrackableProperty<string>(() => VARIATION_CODE, () => NEW_VARIATION_CODE, () => OLD_VARIATION_CODE);
        }

        //Use proxy because instant feedback mode couldn't access child property
        [NotMapped]
        public int? ProxyJobNo
        {
            get
            {
                return JobNoChangeTracking == null ? null : JobNoChangeTracking.TrackableProperty;
            }
            set
            {
                if (JobNoChangeTracking != null)
                    JobNoChangeTracking.TrackableProperty = value;
            }
        }

        //Use proxy because instant feedback mode couldn't access child property
        [NotMapped]
        public int? ProxyCostGroup
        {
            get
            {
                return CostGroupChangeTracking == null ? null : CostGroupChangeTracking.TrackableProperty;
            }
            set
            {
                if (CostGroupChangeTracking != null)
                    CostGroupChangeTracking.TrackableProperty = value;
            }
        }

        //Use proxy because instant feedback mode couldn't access child property
        [NotMapped]
        public int? ProxyCostType
        {
            get
            {
                return CostTypeChangeTracking == null ? null : CostTypeChangeTracking.TrackableProperty;
            }
            set
            {
                if (CostTypeChangeTracking != null)
                    CostTypeChangeTracking.TrackableProperty = value;
            }
        }

        //Use proxy because instant feedback mode couldn't access child property
        [NotMapped]
        public string ProxyStockCode
        {
            get
            {
                return StockCodeChangeTracking == null ? null : StockCodeChangeTracking.TrackableProperty;
            }
            set
            {
                if (StockCodeChangeTracking != null)
                    StockCodeChangeTracking.TrackableProperty = value;
            }
        }

        //Use proxy because instant feedback mode couldn't access child property
        [NotMapped]
        public string ProxyVariationCode
        {
            get
            {
                return VariationCodeChangeTracking == null ? null : VariationCodeChangeTracking.TrackableProperty;
            }
            set
            {
                if (VariationCodeChangeTracking != null)
                    VariationCodeChangeTracking.TrackableProperty = value;
            }
        }

        [NotMapped]
        public bool QtyEdited { get; set; }

        [NotMapped]
        public ChangeTrackableProperty<int?> JobNoChangeTracking { get; set; }

        [NotMapped]
        public ChangeTrackableProperty<int?> CostGroupChangeTracking { get; set; }

        [NotMapped]
        public ChangeTrackableProperty<int?> CostTypeChangeTracking { get; set; }

        [NotMapped]
        public ChangeTrackableProperty<string> StockCodeChangeTracking { get; set; }

        [NotMapped]
        public ChangeTrackableProperty<string> VariationCodeChangeTracking { get; set; }
    }
}
