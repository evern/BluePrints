using BaseModel.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.Utils;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
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
            string budgetHours = dataRow[ColumnHeaderResources.BudgetHourHeaderString].ToString();

            PHASE findPHASE = PHASECollection.FirstOrDefault(x => x.INTERNAL_NUM.ToUpper() == phase.ToUpper());
            AREA findAREA = AREACollection.FirstOrDefault(x => x.INTERNAL_NUM.ToUpper() == area.ToUpper());
            AREA findSUBAREA = findAREA == null ? null : AREACollection.FirstOrDefault(x => x.GUID_PARENT == findAREA.GUID && x.INTERNAL_NUM.ToUpper() == subArea.ToUpper());
            DISCIPLINE findDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.NAME.ToUpper() == discipline.ToUpper());

            int? findDisciplineNum = null;
            int parseDisciplineNum;
            if (int.TryParse(disciplineNum, out parseDisciplineNum))
                findDisciplineNum = parseDisciplineNum;

            decimal? findBudgetHours = null;
            decimal parseBudgetHours;
            if (decimal.TryParse(budgetHours, out parseBudgetHours))
                findBudgetHours = parseBudgetHours;

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
            newBASELINE_ITEM.BUDGET_HOURS = findBudgetHours == null ? 0 : (decimal)findBudgetHours;

            if(findEarnedPercentage != null)
                newBASELINE_ITEMProgress.Total_Earned_Percentage = (decimal)findEarnedPercentage;

            return newBASELINE_ITEMProgress;
        }

        public static bool IsColumnHeadersExists(DataTable dataTable, out List<ErrorMessage> errorMessages)
        {
            errorMessages = new List<ErrorMessage>();
            string errorMessagePrefix = "Missing Column";

            if (!ContainColumn(ColumnHeaderResources.PhaseHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.PhaseHeaderString));
            if (!ContainColumn(ColumnHeaderResources.AreaHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.AreaHeaderString));
            if (!ContainColumn(ColumnHeaderResources.SubAreaHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.SubAreaHeaderString));
            if (!ContainColumn(ColumnHeaderResources.DisciplineHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.DisciplineHeaderString));
            if (!ContainColumn(ColumnHeaderResources.DisciplineNumberHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.DisciplineNumberHeaderString));
            if (!ContainColumn(ColumnHeaderResources.DocumentTypeHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.DocumentTypeHeaderString));
            if (!ContainColumn(ColumnHeaderResources.DeliverableTypeHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.DeliverableTypeHeaderString));
            if (!ContainColumn(ColumnHeaderResources.DepartmentHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.DepartmentHeaderString));
            if (!ContainColumn(ColumnHeaderResources.InternalNumberHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.InternalNumberHeaderString));
            if (!ContainColumn(ColumnHeaderResources.ClientNumberHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.ClientNumberHeaderString));
            if (!ContainColumn(ColumnHeaderResources.PrimaryTitleHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.PrimaryTitleHeaderString));
            if (!ContainColumn(ColumnHeaderResources.SecondaryTitleHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.SecondaryTitleHeaderString));
            if (!ContainColumn(ColumnHeaderResources.CommentsHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.CommentsHeaderString));
            if (!ContainColumn(ColumnHeaderResources.CurrentPercentageHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.CurrentPercentageHeaderString));
            if (!ContainColumn(ColumnHeaderResources.BudgetHourHeaderString, dataTable))
                errorMessages.Add(new ErrorMessage(errorMessagePrefix, ColumnHeaderResources.BudgetHourHeaderString));

            if (errorMessages.Count > 0)
                return false;

            return true;
        }

        private static bool ContainColumn(string columnName, DataTable table)
        {
            DataColumnCollection columns = table.Columns;
            if (columns.Contains(columnName))
            {
                return true;
            }

            return false;
        }
    }

    public class BASELINE_ITEMProgressImportWrapper : BASELINE_ITEMProgress, IOriginalGuidEntityKey
    {
        public OldNewValueCompare<string> Compare_PHASE { get; set; }
        public OldNewValueCompare<string> Compare_AREA { get; set; }
        public OldNewValueCompare<string> Compare_SUBAREA { get; set; }
        public OldNewValueCompare<string> Compare_DISCIPLINE { get; set; }
        public OldNewValueCompare<decimal> Compare_DISCIPLINE_NUM { get; set; }
        public OldNewValueCompare<string> Compare_DOCTYPE { get; set; }
        public OldNewValueCompare<string> Compare_DELIVERABLE_TYPE { get; set; }
        public OldNewValueCompare<string> Compare_DEPARTMENT { get; set; }
        public OldNewValueCompare<string> Compare_INTERNAL_NUM { get; set; }
        public OldNewValueCompare<string> Compare_CLIENT_NUM { get; set; }
        public OldNewValueCompare<string> Compare_PRIMARY_TITLE { get; set; }
        public OldNewValueCompare<string> Compare_SECONDARY_TITLE { get; set; }
        public OldNewValueCompare<string> Compare_COMMENTS { get; set; }
        public OldNewValueCompare<decimal> Compare_BUDGET_HOURS { get; set; }
        public OldNewValueCompare<decimal> Compare_EARNED_PERCENTAGE { get; set; }
        public decimal OldPercentage { get; set; }
        public bool Import { get; set; }
        public bool CanImport => IsNew || (!IsError && !IsSame);
        public string Message { get; set; }
        public bool IsError { get; set; }
        public bool IsNew { get; set; }
        public bool IsSame => !Compare_EARNED_PERCENTAGE.IsDifferent && !IsAnyPropertyDifferent();
        public static BASELINE_ITEMProgressImportWrapper Create(BASELINE_ITEMProgress originalProjection, BASELINE_ITEMProgress importProjection, IEnumerable<PHASE> PHASECollection, IEnumerable<AREA> AREACollection, IEnumerable<DISCIPLINE> DISCIPLINECollection, IEnumerable<DOCTYPE> DOCTYPECollection, IEnumerable<DEPARTMENT> DEPARTMENTCollection)
        {
            return ViewModelSource.Create(() => new BASELINE_ITEMProgressImportWrapper(originalProjection, importProjection, PHASECollection, AREACollection, DISCIPLINECollection, DOCTYPECollection, DEPARTMENTCollection));
        }

        protected BASELINE_ITEMProgressImportWrapper(BASELINE_ITEMProgress originalProjection, BASELINE_ITEMProgress importProjection,
            IEnumerable<PHASE> PHASECollection, IEnumerable<AREA> AREACollection, IEnumerable<DISCIPLINE> DISCIPLINECollection, IEnumerable<DOCTYPE> DOCTYPECollection, IEnumerable<DEPARTMENT> DEPARTMENTCollection)
        {
            BASELINE_ITEMProgress compareProjection = originalProjection == null ? importProjection : originalProjection;

            PHASE oldPHASE = PHASECollection.FirstOrDefault(x => x.GUID == compareProjection.Phase_Guid);
            PHASE newPHASE = PHASECollection.FirstOrDefault(x => x.GUID == importProjection.Phase_Guid);
            Compare_PHASE = new OldNewValueCompare<string>(oldPHASE == null ? string.Empty : oldPHASE.INTERNAL_NUM, newPHASE == null ? string.Empty : newPHASE.INTERNAL_NUM);

            AREA oldAREA = AREACollection.FirstOrDefault(x => x.GUID == compareProjection.SubArea_Guid);
            AREA newAREA = AREACollection.FirstOrDefault(x => x.GUID == importProjection.SubArea_Guid);
            Compare_AREA = new OldNewValueCompare<string>(oldAREA == null ? string.Empty : oldAREA.INTERNAL_NUM, newAREA == null ? string.Empty : newAREA.INTERNAL_NUM);

            AREA oldSUBAREA = AREACollection.FirstOrDefault(x => x.GUID == compareProjection.SubArea_Guid);
            AREA newSUBAREA = AREACollection.FirstOrDefault(x => x.GUID == importProjection.SubArea_Guid);
            Compare_SUBAREA = new OldNewValueCompare<string>(oldSUBAREA == null ? string.Empty : oldSUBAREA.INTERNAL_NUM, newSUBAREA == null ? string.Empty : newSUBAREA.INTERNAL_NUM);

            DISCIPLINE oldDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == compareProjection.Discipline_Guid);
            DISCIPLINE newDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == importProjection.Discipline_Guid);
            Compare_DISCIPLINE = new OldNewValueCompare<string>(oldDISCIPLINE == null ? string.Empty : oldDISCIPLINE.NAME, newDISCIPLINE == null ? string.Empty : newDISCIPLINE.NAME);
            Compare_DISCIPLINE_NUM = new OldNewValueCompare<decimal>(compareProjection.Discipline_Number, compareProjection.Discipline_Number);

            DOCTYPE oldDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == compareProjection.Entity.Entity.GUID_DOCTYPE);
            DOCTYPE newDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == importProjection.Entity.Entity.GUID_DOCTYPE);
            Compare_DOCTYPE = new OldNewValueCompare<string>(oldDOCTYPE == null ? string.Empty : oldDOCTYPE.NAME, newDOCTYPE == null ? string.Empty : newDOCTYPE.NAME);

            string oldDeliverableType = Enum.GetName(typeof(DeliverableType), compareProjection.Entity.Entity.DELIVERABLE_TYPE);
            string newDeliverableType = Enum.GetName(typeof(DeliverableType), importProjection.Entity.Entity.DELIVERABLE_TYPE);
            Compare_DELIVERABLE_TYPE = new OldNewValueCompare<string>(oldDeliverableType, newDeliverableType);

            DEPARTMENT oldDepartment = DEPARTMENTCollection.FirstOrDefault(x => x.GUID == compareProjection.Department_Guid);
            DEPARTMENT newDepartment = DEPARTMENTCollection.FirstOrDefault(x => x.GUID == importProjection.Department_Guid);
            Compare_DEPARTMENT = new OldNewValueCompare<string>(oldDepartment == null ? string.Empty : oldDepartment.NAME, newDepartment == null ? string.Empty : newDepartment.NAME);
            Compare_INTERNAL_NUM = new OldNewValueCompare<string>(compareProjection.Entity.Entity.INTERNAL_NUM, importProjection.Entity.Entity.INTERNAL_NUM);
            Compare_CLIENT_NUM = new OldNewValueCompare<string>(compareProjection.Entity.Entity.CLIENT_NUM, importProjection.Entity.Entity.CLIENT_NUM);
            Compare_PRIMARY_TITLE = new OldNewValueCompare<string>(compareProjection.Entity.Entity.PRIMARY_TITLE, importProjection.Entity.Entity.PRIMARY_TITLE);
            Compare_SECONDARY_TITLE = new OldNewValueCompare<string>(compareProjection.Entity.Entity.SECONDARY_TITLE, importProjection.Entity.Entity.SECONDARY_TITLE);
            Compare_COMMENTS = new OldNewValueCompare<string>(compareProjection.Entity.Entity.COMMENTS, importProjection.Entity.Entity.COMMENTS);
            Compare_EARNED_PERCENTAGE = new OldNewValueCompare<decimal>(compareProjection.Total_Earned_Percentage, importProjection.Total_Earned_Percentage, true);
            Compare_BUDGET_HOURS = new OldNewValueCompare<decimal>(compareProjection.Entity.Entity.BUDGET_HOURS, importProjection.Entity.Entity.BUDGET_HOURS);

            this.OldPercentage = compareProjection.Total_Earned_Percentage;
            this.Entity = importProjection.Entity;
            this.Total_Earned_Percentage = importProjection.Total_Earned_Percentage;
        }

        public bool IsAnyPropertyDifferent()
        {
            return Compare_PHASE.IsDifferent || Compare_AREA.IsDifferent || Compare_SUBAREA.IsDifferent || Compare_DISCIPLINE.IsDifferent || Compare_DISCIPLINE_NUM.IsDifferent || Compare_DOCTYPE.IsDifferent
                || Compare_DELIVERABLE_TYPE.IsDifferent || Compare_DEPARTMENT.IsDifferent || Compare_INTERNAL_NUM.IsDifferent || Compare_CLIENT_NUM.IsDifferent || Compare_PRIMARY_TITLE.IsDifferent
                || Compare_SECONDARY_TITLE.IsDifferent || Compare_COMMENTS.IsDifferent || Compare_BUDGET_HOURS.IsDifferent;
        }
    }

    public class OldNewValueCompare<T>
    {
        readonly T OldValue;
        readonly T NewValue;
        readonly bool UsePercentageFormat;
        public OldNewValueCompare(T OldValue, T NewValue, bool usePercentageFormat = false)
        {
            this.OldValue = OldValue;
            this.NewValue = NewValue;
            this.UsePercentageFormat = usePercentageFormat;
        }

        public string ToolTip
        {
            get
            {
                if (IsDifferent && this.OldValue != null)
                {
                    string displayString = this.OldValue.ToString();
                    if (UsePercentageFormat)
                        displayString = String.Format("{0:P2}", this.OldValue);

                    return "Original Value :" + displayString; 
                }
                else if (IsDifferent && this.OldValue == null)
                    return "Original Value is empty";

                return null;
            }
        }

        public bool IsDifferent
        {
            get
            {
                if (OldValue == null && NewValue != null && NewValue.ToString() != string.Empty)
                    return true;
                else if (OldValue != null && OldValue.ToString() != string.Empty && NewValue == null)
                    return true;
                else if(OldValue != null && NewValue != null && UsePercentageFormat)
                {
                    string oldDisplayString = String.Format("{0:P2}", this.OldValue);
                    string newDisplayString = String.Format("{0:P2}", this.NewValue);

                    if (oldDisplayString != newDisplayString)
                        return true;
                }
                else if(typeof(T) == typeof(decimal))
                {
                    string oldDisplayString = String.Format("{0:N2}", this.OldValue);
                    string newDisplayString = String.Format("{0:N2}", this.NewValue);

                    if (oldDisplayString != newDisplayString)
                        return true;
                }
                else if (OldValue != null && NewValue != null && OldValue.ToString() != NewValue.ToString())
                    return true;

                return false;
            }
        }

        public SolidColorBrush BackColor
        {
            get
            {
                if(IsDifferent)
                    return new SolidColorBrush(Colors.Chartreuse);

                return new SolidColorBrush(Colors.Transparent);
            } 
        }
    }
}
