using BaseModel.Misc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Common.Resources;
using DevExpress.Mvvm;
using BluePrints.Common.Base;

namespace BluePrints.Data
{
    public partial class ESTIMATION_DIRECT_ITEM : BluePrintsEntityBase, IGuidEntityKey, IOriginalGuidEntityKey, IHaveCreatedDate, IDeliverable, IHaveDBProductivityOverride, ISupportVariation, IHaveProcurementWorkpack
    {
        public ESTIMATION_DIRECT_ITEM()
        {
            DISCIPLINE_NUM = 1;
            PROGRESS_TYPE = 0;
        }

        [NotMapped]
        public Guid EntityKey
        {
            get
            {
                return GUID;
            }

            set
            {
                GUID = value;
            }
        }

        [NotMapped]
        public Guid OriginalEntityKey
        {
            get
            {
                return GUID_ORIGINAL;
            }
        }

        public void SetOriginalEntityKey(Guid newGuid) { GUID_ORIGINAL = newGuid; }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        //Used for direct property access validation in fill/undo-redo
        [NotMapped]
        public Guid? SubAreaGuid
        {
            get
            {
                return GUID_SUBAREA;
            }
            set
            {
                Guid? setValue = (Guid?)value;
                if (setValue == null)
                    GUID_SUBAREA = null;
                else if (IsSubAreaValid(setValue))
                    GUID_SUBAREA = setValue;
            }
        }

        [NotMapped]
        public IEnumerable<AREA> SubAreaCollection
        {
            get
            {
                if (AREA == null)
                    return null;

                return AREA.AREA1;
            }
        }

        public bool IsSubAreaValid(Guid? subAreaGuid)
        {
            if (subAreaGuid == null)
                return false;

            if (SubAreaCollection == null)
                return false;

            return SubAreaCollection.Any(x => x.GUID == subAreaGuid);
        }

        public string Deliverable_Name => STOCK_CODE == null ? string.Empty : STOCK_CODE.CODE;

        [NotMapped]
        public Guid? Workpack_Guid
        {
            get { return GUID_WORKPACK; }
            set { GUID_WORKPACK = value; }
        }

        [NotMapped]
        public string Discipline_Code
        {
            get
            {
                if (DISCIPLINE == null)
                    return string.Empty;

                return DISCIPLINE.CODE + DISCIPLINE_NUM;
            }
        }

        public string Phase_Code => BluePrintsResources.Default_Construction_Phase;

        public string Commodity_Code => COMMODITY_CODE == null ? string.Empty : COMMODITY_CODE.CODE;

        public Guid? Area_Guid => GUID_AREA;

        public Guid? SubArea_Guid => GUID_SUBAREA;

        public decimal Total_Units_IncludingByDuration => Estimated_Units;

        public decimal Estimated_Units => STOCK_CODE == null ? 0 : ESTIMATED_QUANTITY * STOCK_CODE.HOURS_INSTALL;

        public decimal Total_Units => Estimated_Units + Variation_Units;

        public decimal Variation_Units => STOCK_CODE == null ? 0 : DC_QUANTITY * STOCK_CODE.HOURS_INSTALL;

        [NotMapped]
        public decimal? DB_Productivity_Override { get => PRODUCTIVITY_OVERRIDE; set => PRODUCTIVITY_OVERRIDE = value; }

        [NotMapped]
        public Guid? Variation_Guid { get => GUID_VARIATION; set => GUID_VARIATION = value; }

        [NotMapped]
        public Guid? Baseline_Guid { get => GUID_ESTIMATION_DIRECT; set => GUID_ESTIMATION_DIRECT = value; }

        [NotMapped]
        public decimal Estimated_Value { get => ESTIMATED_QUANTITY; set => ESTIMATED_QUANTITY = value; }

        [NotMapped]
        public decimal DC_Value { get => DC_QUANTITY; set => DC_QUANTITY = value; }

        [NotMapped]
        public string Workpack_Name
        {
            get
            {
                if (WORKPACK == null)
                    return string.Empty;

                return WORKPACK.INTERNAL_NAME1;
            }
        }

        [NotMapped]
        public string Department_Code
        {
            get
            {
                return "CN";
            }
        }

        [NotMapped]
        public Guid? Phase_Guid { get => GUID_PHASE; set => GUID_PHASE = value; }

        [NotMapped]
        public Guid? Procurement_Workpack_Guid { get => GUID_PWORKPACK; set => GUID_PWORKPACK = value; }
    }
}
