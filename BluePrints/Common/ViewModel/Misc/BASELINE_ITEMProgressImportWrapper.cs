using BluePrints.Common.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Misc
{
    public class BASELINE_ITEMProgressImportWrapper : BASELINE_ITEMProgress
    {
        public OldNewValueCompare<Guid?> Compare_GUID_PHASE { get; set; }
        public OldNewValueCompare<Guid?> Compare_GUID_AREA { get; set; }
        public OldNewValueCompare<Guid?> Compare_GUID_SUBAREA { get; set; }
        public OldNewValueCompare<Guid?> Compare_GUID_DISCIPLINE { get; set; }
        public OldNewValueCompare<decimal> Compare_DISCIPLINE_NUM { get; set; }
        public OldNewValueCompare<Guid?> Compare_GUID_DOCTYPE { get; set; }
        public OldNewValueCompare<DeliverableType> Compare_DELIVERABLE_TYPE { get; set; }
        public OldNewValueCompare<Guid?> Compare_GUID_DEPARTMENT { get; set; }
        public OldNewValueCompare<string> Compare_INTERNAL_NUM { get; set; }
        public OldNewValueCompare<string> Compare_CLIENT_NUM { get; set; }
        public OldNewValueCompare<string> Compare_PRIMARY_TITLE { get; set; }
        public OldNewValueCompare<string> Compare_SECONDARY_TITLE { get; set; }
        public OldNewValueCompare<string> Compare_COMMENTS { get; set; }
        public OldNewValueCompare<Guid?> Compare_GUID_SUBJOB { get; set; }

        public BASELINE_ITEMProgressImportWrapper(BASELINE_ITEMProgress originalProjection, BASELINE_ITEMProgress importProjection)
        {
            Compare_GUID_PHASE = new OldNewValueCompare<Guid?>(originalProjection.Phase_Guid, importProjection.Phase_Guid);
            Compare_GUID_AREA = new OldNewValueCompare<Guid?>(originalProjection.Area_Guid, importProjection.Area_Guid);
            Compare_GUID_SUBAREA = new OldNewValueCompare<Guid?>(originalProjection.SubArea_Guid, importProjection.SubArea_Guid);
            Compare_GUID_DISCIPLINE = new OldNewValueCompare<Guid?>(originalProjection.Discipline_Guid, importProjection.Discipline_Guid);
            Compare_DISCIPLINE_NUM = new OldNewValueCompare<decimal>(originalProjection.Discipline_Number, originalProjection.Discipline_Number);
            Compare_GUID_DOCTYPE = new OldNewValueCompare<Guid?>(originalProjection.Entity.Entity.GUID_DOCTYPE, importProjection.Entity.Entity.GUID_DOCTYPE);
            Compare_DELIVERABLE_TYPE = new OldNewValueCompare<DeliverableType>(originalProjection.Entity.Entity.DELIVERABLE_TYPE, importProjection.Entity.Entity.DELIVERABLE_TYPE);
            Compare_GUID_DEPARTMENT = new OldNewValueCompare<Guid?>(originalProjection.Department_Guid, importProjection.Department_Guid);
            Compare_INTERNAL_NUM = new OldNewValueCompare<string>(originalProjection.Entity.Entity.INTERNAL_NUM, importProjection.Entity.Entity.INTERNAL_NUM);
            Compare_CLIENT_NUM = new OldNewValueCompare<string>(originalProjection.Entity.Entity.CLIENT_NUM, importProjection.Entity.Entity.CLIENT_NUM);
            Compare_PRIMARY_TITLE = new OldNewValueCompare<string>(originalProjection.Entity.Entity.PRIMARY_TITLE, importProjection.Entity.Entity.PRIMARY_TITLE);
            Compare_SECONDARY_TITLE = new OldNewValueCompare<string>(originalProjection.Entity.Entity.SECONDARY_TITLE, importProjection.Entity.Entity.SECONDARY_TITLE);
            Compare_COMMENTS = new OldNewValueCompare<string>(originalProjection.Entity.Entity.COMMENTS, importProjection.Entity.Entity.COMMENTS);
            Compare_GUID_SUBJOB = new OldNewValueCompare<Guid?>(originalProjection.Subjob_Guid, importProjection.Subjob_Guid);
        }
    }

    public class OldNewValueCompare<T>
    {
        readonly T OldValue;
        readonly T NewValue;
        public OldNewValueCompare(T OldValue, T NewValue)
        {
            this.OldValue = OldValue;
            this.NewValue = NewValue;
        }

        public string ToolTip
        {
            get
            {
                if (isDifferent && this.OldValue != null)
                    return this.OldValue.ToString();

                return string.Empty;
            }
        }

        private bool isDifferent
        {
            get
            {
                if (OldValue == null && NewValue != null)
                    return true;
                else if (OldValue != null && NewValue == null)
                    return true;
                else if (OldValue.ToString() != NewValue.ToString())
                    return true;

                return false;
            }
        }

        public SolidColorBrush ColumnBackColor
        {
            get
            {
                if(isDifferent)
                    return new SolidColorBrush(Colors.LightSalmon);

                return new SolidColorBrush(Colors.Transparent);
            } 
        }
    }
}
