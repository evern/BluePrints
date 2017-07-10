using BaseModel.Misc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using BluePrints.Common.Projections;
using BluePrints.Common.ViewModel.Reporting;

namespace BluePrints.Data
{
    public partial class ESTIMATION_DIRECT_ITEM : IGuidEntityKey, IOriginalGuidEntityKey, IHaveCreatedDate, ISortableDeliverable
    {
        public ESTIMATION_DIRECT_ITEM()
        {
            DISCIPLINE_NUM = 1;
            TRACK = true;
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

        public string ReportableItem_Name => STOCK_CODE == null ? string.Empty : STOCK_CODE.CODE;

        public Guid? Workpack_Guid => GUID_WORKPACK;

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

        public string Commodity_Code => COMMODITY_CODE == null ? string.Empty : COMMODITY_CODE.CODE;

        public Guid? Area_Guid => GUID_AREA;

        public Guid? SubArea_Guid => GUID_SUBAREA;

        public decimal TotalHoursIncludeByDuration => EstimatedHours;

        public decimal EstimatedHours => STOCK_CODE == null ? 0 : ESTIMATED_QUANTITY * STOCK_CODE.HOURS_INSTALL;

        public decimal TotalHours => EstimatedHours;

    }
}
