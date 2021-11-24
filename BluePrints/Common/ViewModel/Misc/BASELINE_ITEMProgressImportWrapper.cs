using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BluePrints.Common.ViewModel.Misc
{
    public static class BASELINE_ITEMProgressImportWrapperHelper
    {
        public static BASELINE_ITEMProgress ConvertDataRowToBASELINE_ITEMProgress(DataRow dataRow, 
            IEnumerable<PHASE> PHASECollection, IEnumerable<AREA> AREACollection, IEnumerable<DISCIPLINE> DISCIPLINECollection, IEnumerable<DOCTYPE> DOCTYPECollection, IEnumerable<DEPARTMENT> DEPARTMENTCollection)
        {
            BASELINE_ITEM newBASELINE_ITEM = new BASELINE_ITEM();

            BASELINE_ITEMProjection newBASELINE_ITEMProjection = new BASELINE_ITEMProjection();
            newBASELINE_ITEMProjection.Entity = newBASELINE_ITEM;
            BASELINE_ITEMProgress newBASELINE_ITEMProgress = new BASELINE_ITEMProgress();
            newBASELINE_ITEMProgress.Entity = newBASELINE_ITEMProjection;

            string phase = dataRow[ColumnHeaderResources.PhaseHeaderString].ToString();
            string area = dataRow[ColumnHeaderResources.AreaHeaderString].ToString();
            string subArea = dataRow[ColumnHeaderResources.SubAreaHeaderString].ToString();
            string discipline = dataRow[ColumnHeaderResources.DisciplineHeaderString].ToString();
            string disciplineNum = dataRow[ColumnHeaderResources.DisciplineNumberHeaderString].ToString();
            string docType = dataRow[ColumnHeaderResources.DocumentTypeHeaderString].ToString();
            string deliverableType = dataRow[ColumnHeaderResources.DeliverableTypeHeaderString].ToString();
            string department = dataRow[ColumnHeaderResources.DepartmentHeaderString].ToString();
            string internalNum = dataRow[ColumnHeaderResources.InternalNumberHeaderString].ToString();
            string clientNum = dataRow[ColumnHeaderResources.ClientNumberHeaderString].ToString();
            string primaryTitle = dataRow[ColumnHeaderResources.PrimaryTitleHeaderString].ToString();
            string secondaryTitle = dataRow[ColumnHeaderResources.SecondaryTitleHeaderString].ToString();
            string comments = dataRow[ColumnHeaderResources.CommentsHeaderString].ToString();
            string currentEarnedPercentage = dataRow[ColumnHeaderResources.CurrentPercentageHeaderString].ToString();

            string s;
            if (comments != string.Empty)
                s = string.Empty;

            PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.INTERNAL_NUM.ToUpper() == phase.ToUpper());
            AREA findAREA = AREACollection.FirstOrDefault(x => x.INTERNAL_NUM.ToUpper() == area.ToUpper());
            AREA findSUBAREA = findAREA == null ? null : AREACollection.FirstOrDefault(x => x.GUID_PARENT == findAREA.GUID && x.INTERNAL_NUM.ToUpper() == subArea.ToUpper());
            DISCIPLINE findDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.NAME.ToUpper() == discipline.ToUpper());

            int? findDisciplineNum = null;
            int parseDisciplineNum;
            if (int.TryParse(disciplineNum, out parseDisciplineNum))
                findDisciplineNum = parseDisciplineNum;

            DOCTYPE findDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.NAME.ToUpper() == docType.ToUpper());

            DeliverableType? findDeliverableType = null;
            DeliverableType parseDeliverableType;
            if (Enum.TryParse(deliverableType, out parseDeliverableType))
                findDeliverableType = parseDeliverableType;

            DEPARTMENT findDEPARTMENT = DEPARTMENTCollection.FirstOrDefault(x => x.NAME.ToUpper() == department.ToUpper());

            decimal? findEarnedPercentage = null;
            decimal parseEarnedPercentage = 0;
            if (decimal.TryParse(currentEarnedPercentage, out parseEarnedPercentage))
                findEarnedPercentage = parseEarnedPercentage;

            newBASELINE_ITEM.GUID_PHASE = findPHASE == null ? (Guid?)null : findPHASE.GUID;
            newBASELINE_ITEM.GUID_AREA = findAREA == null ? (Guid?)null : findAREA.GUID;
            newBASELINE_ITEM.GUID_SUBAREA = findSUBAREA == null ? (Guid?)null : findSUBAREA.GUID;
            newBASELINE_ITEM.GUID_DISCIPLINE = findDISCIPLINE == null ? (Guid?)null : findDISCIPLINE.GUID;
            newBASELINE_ITEM.DISCIPLINE_NUM = findDisciplineNum == null ? 1 : (int)findDisciplineNum;
            newBASELINE_ITEM.GUID_DOCTYPE = findDOCTYPE == null ? (Guid?)null : findDOCTYPE.GUID;
            newBASELINE_ITEM.DELIVERABLE_TYPE = findDeliverableType == null ? DeliverableType.Deliverable : (DeliverableType)findDeliverableType;
            newBASELINE_ITEM.GUID_DEPARTMENT = findDEPARTMENT == null ? (Guid?)null : findDEPARTMENT.GUID;
            newBASELINE_ITEM.INTERNAL_NUM = internalNum;
            newBASELINE_ITEM.CLIENT_NUM = clientNum;
            newBASELINE_ITEM.PRIMARY_TITLE = primaryTitle;
            newBASELINE_ITEM.SECONDARY_TITLE = secondaryTitle;
            newBASELINE_ITEM.COMMENTS = comments;

            if(findEarnedPercentage != null)
                newBASELINE_ITEMProgress.Total_Earned_Percentage = (decimal)findEarnedPercentage;

            return newBASELINE_ITEMProgress;
        }
    }

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
        public OldNewValueCompare<decimal> Compare_EARNED_PERCENTAGE { get; set; }
        public bool Import { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }

        public BASELINE_ITEMProgressImportWrapper(BASELINE_ITEMProgress originalProjection, BASELINE_ITEMProgress importProjection)
        {
            BASELINE_ITEMProgress compareProjection = originalProjection == null ? importProjection : originalProjection;

            Compare_GUID_PHASE = new OldNewValueCompare<Guid?>(compareProjection.Phase_Guid, importProjection.Phase_Guid);
            Compare_GUID_AREA = new OldNewValueCompare<Guid?>(compareProjection.Area_Guid, importProjection.Area_Guid);
            Compare_GUID_SUBAREA = new OldNewValueCompare<Guid?>(compareProjection.SubArea_Guid, importProjection.SubArea_Guid);
            Compare_GUID_DISCIPLINE = new OldNewValueCompare<Guid?>(compareProjection.Discipline_Guid, importProjection.Discipline_Guid);
            Compare_DISCIPLINE_NUM = new OldNewValueCompare<decimal>(compareProjection.Discipline_Number, compareProjection.Discipline_Number);
            Compare_GUID_DOCTYPE = new OldNewValueCompare<Guid?>(compareProjection.Entity.Entity.GUID_DOCTYPE, importProjection.Entity.Entity.GUID_DOCTYPE);
            Compare_DELIVERABLE_TYPE = new OldNewValueCompare<DeliverableType>(compareProjection.Entity.Entity.DELIVERABLE_TYPE, importProjection.Entity.Entity.DELIVERABLE_TYPE);
            Compare_GUID_DEPARTMENT = new OldNewValueCompare<Guid?>(compareProjection.Department_Guid, importProjection.Department_Guid);
            Compare_INTERNAL_NUM = new OldNewValueCompare<string>(compareProjection.Entity.Entity.INTERNAL_NUM, importProjection.Entity.Entity.INTERNAL_NUM);
            Compare_CLIENT_NUM = new OldNewValueCompare<string>(compareProjection.Entity.Entity.CLIENT_NUM, importProjection.Entity.Entity.CLIENT_NUM);
            Compare_PRIMARY_TITLE = new OldNewValueCompare<string>(compareProjection.Entity.Entity.PRIMARY_TITLE, importProjection.Entity.Entity.PRIMARY_TITLE);
            Compare_SECONDARY_TITLE = new OldNewValueCompare<string>(compareProjection.Entity.Entity.SECONDARY_TITLE, importProjection.Entity.Entity.SECONDARY_TITLE);
            Compare_COMMENTS = new OldNewValueCompare<string>(compareProjection.Entity.Entity.COMMENTS, importProjection.Entity.Entity.COMMENTS);
            Compare_EARNED_PERCENTAGE = new OldNewValueCompare<decimal>(compareProjection.Total_Earned_Percentage, importProjection.Total_Earned_Percentage);

            this.Entity = compareProjection.Entity;
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

        public SolidColorBrush BackColor
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
