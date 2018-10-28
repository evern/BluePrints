using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrints.Common.ViewModel.Utils
{
    public static class BluePrintsUtils
    {
        public static void BookTime(PROJECT project, IDeliverable deliverable, IPrimeroEntitiesUnitOfWork primeroUnitOfWork, List<ExoTimeAuthorisation> exoAuthorisations, List<string> variationCodes, List<string> narratives, IMessageBoxService MessageBoxService, IDialogService BookTimeDialogService)
        {
            var bookTimeViewModel = BookTimeSheetViewModel.Create(project, deliverable, primeroUnitOfWork, exoAuthorisations, variationCodes, narratives);
            if (bookTimeViewModel.GetResource() == null)
            {
                MessageBoxService.ShowMessage("You are not authorised to book time on this subjob, please contact the project manager for assistance");
            }
            else if (bookTimeViewModel.GetCostType() == null)
            {
                MessageBoxService.ShowMessage("You do not have \nSub Job: " + deliverable.Subjob_Name + "\nCost Group: " + deliverable.Discipline_Code + "\nCost Type: " + deliverable.Commodity_Code + "\nAdded in exo, please contact the project manager for assistance");
            }
            else if (BookTimeDialogService.ShowDialog(MessageButton.OKCancel, "Enter time to book", "BookTimeDialog", bookTimeViewModel) == MessageResult.OK)
            {
                string variationCode = bookTimeViewModel.GetVariationCode();
                string narrative = bookTimeViewModel.GetNarratives();
                PrimeroSubJob subJob = bookTimeViewModel.GetSubJob();
                PrimeroResource bookResource = bookTimeViewModel.GetResource();
                TimesheetDate bookDate = bookTimeViewModel.GetTimesheetDate();
                PrimeroDiscipline bookCostGroup = bookTimeViewModel.GetCostGroup();
                PrimeroCommodity bookCostType = bookTimeViewModel.GetCostType();
                decimal bookTime = bookTimeViewModel.BookHours;

                if (bookResource != null && bookCostGroup != null && bookCostType != null)
                {
                    JOB_TIMESHEETS timesheet = primeroUnitOfWork.JOB_TIMESHEETS.FirstOrDefault(x => x.STAFFNO == bookResource.SeqNo && x.JOBNO == subJob.Id && x.STOCKCODE == bookCostType.StockCode && x.COST_GROUP == bookCostGroup.Id && x.COST_TYPE == bookCostType.Id && x.X_VARIATIONCODE == variationCode && x.WEEK_START_DATE == bookDate.WeekStartDate);
                    if (timesheet != null)
                    {
                        AdjustTimeSheetHours(timesheet, bookDate, deliverable, bookTime, primeroUnitOfWork);
                    }
                    else
                    {
                        string title = subJob.Code + " : " + subJob.Title;
                        if (title.Length >= 60)
                            title = title.Substring(0, 59);

                        JOB_TIMESHEETS newTimeSheet = new JOB_TIMESHEETS();
                        newTimeSheet.STAFFNO = bookResource.SeqNo;
                        newTimeSheet.JOBNO = subJob.Id;
                        newTimeSheet.TITLE = title;
                        newTimeSheet.STOCKCODE = bookCostType.StockCode;
                        newTimeSheet.DESCRIPTION = bookCostType.StockDescription;
                        newTimeSheet.UNITPRICE = 0;
                        newTimeSheet.WEEK_START_DATE = bookDate.WeekStartDate;
                        AdjustTimeSheetHours(newTimeSheet, bookDate, deliverable, bookTime, primeroUnitOfWork);
                        newTimeSheet.IS_OVERTIME = "N";
                        newTimeSheet.DAY1_POSTED = "N";
                        newTimeSheet.DAY2_POSTED = "N";
                        newTimeSheet.DAY3_POSTED = "N";
                        newTimeSheet.DAY4_POSTED = "N";
                        newTimeSheet.DAY5_POSTED = "N";
                        newTimeSheet.DAY6_POSTED = "N";
                        newTimeSheet.DAY7_POSTED = "N";
                        newTimeSheet.RATE_SEQNO = 0;
                        newTimeSheet.RATE_FACTOR = 1;
                        newTimeSheet.COST_GROUP = bookCostGroup.Id;
                        newTimeSheet.COST_TYPE = bookCostType.Id;
                        newTimeSheet.LABOUR_ALLOWANCE = 0;
                        newTimeSheet.HAS_ALLOWANCE = "N";
                        newTimeSheet.X_DECLINED = false;
                        newTimeSheet.X_APPROVAL_MANAGER = -1;
                        newTimeSheet.X_SUBMITTED = false;
                        newTimeSheet.X_NARRATIVE = narrative;
                        newTimeSheet.X_VARIATIONCODE = variationCode;
                        primeroUnitOfWork.JOB_TIMESHEETS.Add(newTimeSheet);
                    }

                    primeroUnitOfWork.SaveChanges();
                }
            }
        }

        private static void AdjustTimeSheetHours(JOB_TIMESHEETS timesheet, TimesheetDate bookDate, IDeliverable deliverable, decimal bookTime, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            Double dblTime = Convert.ToDouble(bookTime);
            switch (bookDate.DayNumber)
            {
                case 1:
                    timesheet.DAY1 = dblTime;
                    timesheet.DAY1_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name, primeroUnitOfWork);
                    break;
                case 2:
                    timesheet.DAY2 = dblTime;
                    timesheet.DAY2_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name, primeroUnitOfWork);
                    break;
                case 3:
                    timesheet.DAY3 = dblTime;
                    timesheet.DAY3_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name, primeroUnitOfWork);
                    break;
                case 4:
                    timesheet.DAY4 = dblTime;
                    timesheet.DAY4_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name, primeroUnitOfWork);
                    break;
                case 5:
                    timesheet.DAY5 = dblTime;
                    timesheet.DAY5_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name, primeroUnitOfWork);
                    break;
                case 6:
                    timesheet.DAY6 = dblTime;
                    timesheet.DAY6_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name, primeroUnitOfWork);
                    break;
                case 7:
                    timesheet.DAY7 = dblTime;
                    timesheet.DAY7_NARRATIVE = FindExistingOrAddNewNarrative(deliverable.Deliverable_Name, primeroUnitOfWork);
                    break;
            }
        }

        private static int FindExistingOrAddNewNarrative(string description, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
        {
            NARRATIVES narrative = primeroUnitOfWork.NARRATIVES.FirstOrDefault(x => x.NARRATIVE == description);
            if (narrative != null)
                return narrative.SEQNO;
            else
            {
                NARRATIVES newNarrative = new NARRATIVES();
                newNarrative.NARRATIVE = description;
                primeroUnitOfWork.NARRATIVES.Add(newNarrative);
                primeroUnitOfWork.SaveChanges();
                return newNarrative.SEQNO;
            }
        }
    }

    public static class BluePrintsDataUtils
    {

        public static bool GuidEquals<T>(T x, T y)
            where T : class, ICanSync, new()
        {
            return x.GUID == y.GUID;
        }
        /// <summary>
        /// Assign subjob to deliverables or estimation direct item before saving
        /// Optional parameter of phase type or charge type, otherwise use deliverables phase guid to generate subjob name
        /// </summary>
        /// <param name="entity"></param>
        public static void OnBeforeSavedGenerateAndAssignSubjob(PROJECT loadPROJECT, IEnumerable<PHASE> PHASECollection, IEnumerable<AREA> AREACollection, IEnumerable<AREA> SUBAREACollection, IDeliverable entity, CollectionViewModel<SUBJOB, SUBJOB, Guid, IBluePrintsEntitiesUnitOfWork> SUBJOBCollectionViewModel, PhaseType? PhaseType = null, ChargeType? ChargeType = null, bool isProcurementSubjob = false, bool forceIgnore = false)
        {
            //when user wish to override default subjob
            if (forceIgnore)
                return;

            Guid? existingOrNewPhaseGuid;
            IHaveProcurementSubjob iHaveProcurementSubjobEntity = entity as IHaveProcurementSubjob;
            bool assignToProcurementSubjob = (isProcurementSubjob && iHaveProcurementSubjobEntity != null);

            string internalNumber = BluePrintsDataUtils.SUBJOB_Generate_InternalNumber(entity.Area_Guid, entity.SubArea_Guid, loadPROJECT, AREACollection, SUBAREACollection, out existingOrNewPhaseGuid, entity.Phase_Guid, PHASECollection, PhaseType, ChargeType);
            IEnumerable<SUBJOB> SUBJOBCollection = SUBJOBCollectionViewModel.Entities;
            ////provision for when subjob is manually assigned or using legacy subjob
            if (entity.Subjob_Guid != null)
            {
                SUBJOB subjob = SUBJOBCollection.FirstOrDefault(x => x.GUID == entity.Subjob_Guid);
                if (subjob != null &&subjob.INTERNAL_NAME1 == internalNumber)
                    return;
            }

            if (internalNumber != string.Empty)
            {
                SUBJOB existingSUBJOB = SUBJOBCollection.FirstOrDefault(x => x.INTERNAL_NAME1 == internalNumber);
                if (existingSUBJOB == null)
                {
                    var newSUBJOB = new SUBJOB();

                    List<AREA> sub_area_collection = new List<AREA>();
                    AREA defaultSubArea = null;
                    if (sub_area_collection.Count > 0)
                    {
                        defaultSubArea = sub_area_collection.FirstOrDefault(x => x.INTERNAL_NUM == BluePrintsResources.Default_Sub_Area);
                    }

                    newSUBJOB.GUID_PROJECT = loadPROJECT.GUID;
                    newSUBJOB.GUID_DAREA = entity.Area_Guid;
                    newSUBJOB.GUID_DSUBAREA = entity.SubArea_Guid == null ? defaultSubArea != null ? defaultSubArea.GUID : (Guid?)null : entity.SubArea_Guid;
                    AREA findAREA = AREACollection.FirstOrDefault(x => x.GUID == newSUBJOB.GUID_DAREA);
                    if (findAREA != null)
                        newSUBJOB.INTERNAL_NAME2 = findAREA.TITLE;

                    newSUBJOB.GUID_DPHASE = existingOrNewPhaseGuid;
                    newSUBJOB.INTERNAL_NAME1 = internalNumber;
                    newSUBJOB.STARTDATE = DateTime.Now;
                    newSUBJOB.ENDDATE = BluePrintsDataUtils.SUBJOB_Calculate_EndDate((DateTime)newSUBJOB.STARTDATE, loadPROJECT);
                    var reviewStartDate = (DateTime)newSUBJOB.STARTDATE;
                    var reviewEndDate = (DateTime)newSUBJOB.ENDDATE;
                    BluePrintsDataUtils.SUBJOB_Calculate_ReviewPeriod(ref reviewStartDate, ref reviewEndDate, loadPROJECT, false);
                    newSUBJOB.REVIEWSTARTDATE = reviewStartDate;
                    newSUBJOB.REVIEWENDDATE = reviewEndDate;
                    newSUBJOB.AUTOGENERATED = true;
                    if (loadPROJECT.STATUS == ProjectStatus.Tender || loadPROJECT.STATUS == ProjectStatus.TenderSubmitted)
                    {
                        newSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;
                    }

                    SUBJOBCollectionViewModel.Save(newSUBJOB);

                    if (assignToProcurementSubjob)
                        iHaveProcurementSubjobEntity.Procurement_Subjob_Guid = newSUBJOB.GUID;
                    else
                    {
                        entity.Subjob_Guid = newSUBJOB.GUID;
                        entity.Phase_Guid = existingOrNewPhaseGuid;
                    }
                }
                else
                {
                    if (assignToProcurementSubjob)
                        iHaveProcurementSubjobEntity.Procurement_Subjob_Guid = existingSUBJOB.GUID;
                    else
                    {
                        entity.Phase_Guid = existingOrNewPhaseGuid;
                        entity.Subjob_Guid = existingSUBJOB.GUID;
                    }
                }
            }
            else
            {
                entity.Subjob_Guid = null;
                if (assignToProcurementSubjob)
                    iHaveProcurementSubjobEntity.Procurement_Subjob_Guid = null;
            }
        }

        /// <summary>
        /// Assign workpack to deliverables or estimation direct item before saving
        /// Optional parameter of phase type or charge type, otherwise use deliverables phase guid to generate workpack name
        /// </summary>
        /// <param name="entity"></param>
        public static void OnBeforeSavedGenerateAndAssignWorkpack(IDeliverable entity, CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACKCollectionViewModel, IEnumerable<SUBJOB> SUBJOBCollection, IEnumerable<DISCIPLINE> DISCIPLINECollection, bool forceIgnore = false)
        {
            //provision for when workpack is manually assigned or using legacy workpack
            if (forceIgnore || (entity.Subjob_Guid == null || entity.Discipline_Guid == null))
                return;

            ////when user wish to override default workpack
            //if (entity.Workpack_Guid != null)
            //    return;

            WORKPACK existingWORKPACK = WORKPACKCollectionViewModel.Entities.FirstOrDefault(x => x.GUID_SUBJOB == entity.Subjob_Guid && x.GUID_DISCIPLINE == entity.Discipline_Guid && x.DISCIPLINE_NUM == entity.Discipline_Number);
            if((existingWORKPACK != null) && entity.Workpack_Guid != null)
            {
                WORKPACK findWORKPACK = WORKPACKCollectionViewModel.Entities.FirstOrDefault(x => x.GUID == entity.Workpack_Guid);
                if (findWORKPACK != null && (findWORKPACK == existingWORKPACK))
                    return;
            }

            if (existingWORKPACK == null)
            {
                WORKPACK newWORKPACK = new WORKPACK();
                newWORKPACK.GUID_SUBJOB = (Guid)entity.Subjob_Guid;
                newWORKPACK.GUID_DISCIPLINE = (Guid)entity.Discipline_Guid;
                newWORKPACK.DISCIPLINE_NUM = entity.Discipline_Number;
                BluePrintsDataUtils.WORKPACK_Populate_Name(newWORKPACK, SUBJOBCollection, DISCIPLINECollection);
                WORKPACKCollectionViewModel.Save(newWORKPACK);
                entity.Workpack_Guid = newWORKPACK.GUID;
            }
            else
            {
                entity.Workpack_Guid = existingWORKPACK.GUID;
            }
        }

        /// <summary>
        /// Calculates subjob end date using project settings and start date
        /// </summary>
        public static DateTime SUBJOB_Calculate_EndDate(DateTime startDate, PROJECT fromPROJECT)
        {
            var periodPercentage = 1 - Convert.ToDouble(fromPROJECT.REVIEWPERCENTAGE);
            var periodMultiplier = 1 / periodPercentage;
            var reviewPeriod = Convert.ToDouble(fromPROJECT.REVIEWPERIOD);
            var period = TimeSpan.FromDays(reviewPeriod * periodMultiplier);

            var EndDate = startDate.Date.AddDays(period.Days).AddSeconds(-1);
            return EndDate;
        }

        /// <summary>
        /// Calculates subjob start date using project settings and end date
        /// </summary>
        public static DateTime SUBJOB_Calculate_StartDate(DateTime endDate, PROJECT fromPROJECT)
        {
            var periodPercentage = 1 - Convert.ToDouble(fromPROJECT.REVIEWPERCENTAGE);
            var periodMultiplier = 1 / periodPercentage;
            var reviewPeriod = Convert.ToDouble(fromPROJECT.REVIEWPERIOD);
            var period = TimeSpan.FromDays(reviewPeriod * periodMultiplier);

            var StartDate = endDate.Date.AddDays(period.Days * -1);
            return StartDate;
        }

        public static string GenerateStockCode(Guid areaGuid, Guid disciplineGuid, string stock_code, bool createDefaultSubArea, IEnumerable<AREA> areaCollection, IEnumerable<DISCIPLINE> disciplineCollection, Guid? subAreaGuid = null, IEnumerable<AREA> subAreaCollection = null)
        {
            AREA area = areaCollection.FirstOrDefault(x => x.GUID == areaGuid);
            if (area == null)
                return string.Empty;

            DISCIPLINE discipline = disciplineCollection.FirstOrDefault(x => x.GUID == disciplineGuid);
            if (discipline == null)
                return string.Empty;

            AREA subArea = null;
            if (subAreaGuid != null && subAreaCollection != null)
                subArea = subAreaCollection.FirstOrDefault(x => x.GUID == subAreaGuid);


            string subAreaName;
            if (subArea == null)
            {
                if (createDefaultSubArea)
                    subAreaName = BluePrintsResources.Default_Sub_Area;
                else
                    return string.Empty;
            }
            else
                subAreaName = subArea.INTERNAL_NUM;

            return area.INTERNAL_NUM + "-" + discipline.CODE + "-" + subAreaName + "-" + stock_code;
        }

        /// <summary>
        /// Calculate the review start date or end date
        /// </summary>
        /// <param name="getEndDate">whether to get end date else return start date</param>
        public static void SUBJOB_Calculate_ReviewPeriod(ref DateTime StartDate, ref DateTime EndDate,
            PROJECT fromPROJECT, bool getEndDate)
        {
            var timeDifference = EndDate.Date.Subtract(StartDate.Date);
            var percentage = Convert.ToDouble(fromPROJECT.REVIEWPERCENTAGE);
            var timeDifferencePercent =
                TimeSpan.FromTicks(Convert.ToInt64(timeDifference.Ticks * fromPROJECT.REVIEWPERCENTAGE));
            var ReviewStartDate = StartDate.Date.Add(timeDifferencePercent);

            StartDate = ReviewStartDate.Date;
            var ReviewEndDate =
                ReviewStartDate.Date.AddDays(Convert.ToDouble(fromPROJECT.REVIEWPERIOD)).AddSeconds(-1);
            EndDate = ReviewEndDate.Date;
        }

        public static string GetNewInternalNumber(IEnumerable<IEntityNumber> originalEntities, IEnumerable<IEntityNumber> unsavedEntities, string duplicateInternalNumber, IEnumerable<IEntityNumber> insertSelectedEntities, bool isInsert)
        {
            if (duplicateInternalNumber != string.Empty && duplicateInternalNumber != null)
            {
                string stringValueToFill = duplicateInternalNumber;
                int numericFieldLength = 0;
                long valueToFillNumberOnly = 0;
                string valueToFillStringOnly = StringFormatUtils.ParseStringIntoComponents(duplicateInternalNumber, out numericFieldLength, out valueToFillNumberOnly);

                List<IEntityNumber> allEntities = new List<IEntityNumber>(originalEntities);
                allEntities.AddRange(unsavedEntities);

                List<string> originalEntitiesSimilarNames =
                originalEntities.Where(x => x.EntityNumber != null && x.EntityNumber.Contains(valueToFillStringOnly)).Select(x => x.EntityNumber).ToList();

                List<string> allEntitiesSimilarNames =
                allEntities.Where(x => x.EntityNumber != null && x.EntityNumber.Contains(valueToFillStringOnly)).Select(x => x.EntityNumber).ToList();

                List<string> unsavedEntitiesSimilarNames =
                unsavedEntities.Where(x => x.EntityNumber != null && x.EntityNumber.Contains(valueToFillStringOnly)).Select(x => x.EntityNumber).ToList();

                List<string> insertSelectedEntitiesSimilarNames = insertSelectedEntities.Where(x => x.EntityNumber.Contains(valueToFillStringOnly)).Select(x => x.EntityNumber).ToList();

                do
                {
                    valueToFillNumberOnly += 1;
                    string nextName = StringFormatUtils.AppendStringWithEnumerator(valueToFillStringOnly, valueToFillNumberOnly, numericFieldLength);

                    bool isExistsInInsert = insertSelectedEntitiesSimilarNames.Any(x => x == nextName);
                    bool isExistsInUnsaved = unsavedEntitiesSimilarNames.Any(x => x == nextName);

                    //when inserting all names are safe to be used, existing names need to be renamed
                    if (isInsert)
                    {
                        //when current name exists in unsaved it means that nextName is not safe to be used
                        if (isExistsInUnsaved)
                            continue;
                        //when current name exists in insert it means that nextName is not safe to be used
                        else if (isExistsInInsert)
                            continue;
                        else
                            return nextName;
                    }
                    else
                    {
                        bool isExistsInAll = allEntitiesSimilarNames.Any(x => x == nextName);
                        bool isExistsInOriginal = originalEntitiesSimilarNames.Any(x => x == nextName);

                        //when current name exists in original it means that nextName is not safe to be used, use original duplicate internal number instead
                        if (isExistsInOriginal)
                        {
                            //if current name exists in insert it means that user is aware of nextName being duplicated, continue to iterate new name
                            if (isExistsInInsert)
                                continue;
                            //if it doesn't exists in insert it means that nextName is not safe to be used, use the previous name
                            else
                                return StringFormatUtils.AppendStringWithEnumerator(valueToFillStringOnly, valueToFillNumberOnly - 1, numericFieldLength);
                        }
                        //when it doesn't exists in all and doesn't exists is original it means that this number is safe to be used
                        else if (!isExistsInAll)
                            return nextName;
                        //when it doesn't exist in original (no need to be identified for maximum series sequence) but exists in all means that it needs a new number not existing in the unsaved set yet
                        else
                            continue;
                    }
                } while (valueToFillNumberOnly < 1000000);
            }

            return string.Empty;
        }

        public static string Insert_InternalNumber(IEnumerable<BASELINE_ITEMProjection> originalEntities, IEnumerable<BASELINE_ITEMProjection> unsavedEntities, string duplicateInternalNumber)
        {
            if (duplicateInternalNumber != string.Empty && duplicateInternalNumber != null)
            {
                string stringValueToFill = duplicateInternalNumber;
                int numericFieldLength = 0;
                int? numericIndex = StringFormatUtils.GetNumericIndex(stringValueToFill, out numericFieldLength);
                if (numericIndex == null)
                    return duplicateInternalNumber;

                string valueToFillStringOnly = stringValueToFill.Substring(0, stringValueToFill.Length - numericFieldLength);

                long valueToFillNumberOnly = Int64.Parse(stringValueToFill.Substring(numericIndex.Value, duplicateInternalNumber.Length - numericIndex.Value));

                List<BASELINE_ITEMProjection> allEntities = new List<BASELINE_ITEMProjection>(originalEntities);
                allEntities.AddRange(unsavedEntities);

                List<string> originalEntitiesSimilarNames =
                originalEntities.Where(x => x.Entity.INTERNAL_NUM != null && x.Entity.INTERNAL_NUM.Contains(valueToFillStringOnly)).Select(x => x.Entity.INTERNAL_NUM).ToList();

                List<string> allEntitiesSimilarNames =
                allEntities.Where(x => x.Entity.INTERNAL_NUM != null && x.Entity.INTERNAL_NUM.Contains(valueToFillStringOnly)).Select(x => x.Entity.INTERNAL_NUM).ToList();

                do
                {
                    valueToFillNumberOnly += 1;

                    string nextName = StringFormatUtils.AppendStringWithEnumerator(valueToFillStringOnly, valueToFillNumberOnly, numericFieldLength);

                    bool isExistsInAll = allEntitiesSimilarNames.Any(x => x == nextName);
                    bool isExistsInOriginal = originalEntitiesSimilarNames.Any(x => x == nextName);

                    //when current name exists in original do not keep adding so user can identify that this issue needs to be addressed
                    if (isExistsInOriginal)
                        return duplicateInternalNumber;
                    //when it doesn't exists in all and doesn't exists is original it means that this number is save to be used
                    else if (!isExistsInAll)
                        return duplicateInternalNumber;
                    //when it doesn't exist in original (no need to be identified for maximum series sequence) but exists in all means that it needs a new number not existing in the unsaved set yet
                    else
                        continue;

                } while (valueToFillNumberOnly < 1000000);
            }

            return string.Empty;
        }

        public static string BASELINEITEM_Generate_InternalNumber(PROJECT fromPROJECT,
            IEnumerable<BASELINE_ITEM> BASELINE_ITEMEntities, AREA selectedAREA, DISCIPLINE selectedDISCIPLINE,
            DOCTYPE selectedDOCTYPE, Guid? excludeGUID = null)
        {
            if (selectedAREA != null && selectedDISCIPLINE != null && selectedDOCTYPE != null)
            {
                var InternalNum = fromPROJECT.NUMBER;
                if (selectedAREA != null)
                {
                    if(selectedDOCTYPE != null && selectedDOCTYPE.IS_AREA_SIGNIFICANT)
                        InternalNum += "-" + selectedAREA.INTERNAL_NUM;
                }
                if (selectedDOCTYPE != null)
                    InternalNum += "-" + selectedDOCTYPE.CODE;
                if (selectedDISCIPLINE != null)
                    InternalNum += "-" + selectedDISCIPLINE.CODE;

                var internalNameCount = BASELINE_ITEMEntities.Where(x => x.EntityKey != excludeGUID).Count(x => x.INTERNAL_NUM != null && x.INTERNAL_NUM.Contains(InternalNum));
                internalNameCount += 1;

                var countString = string.Empty;
                if (internalNameCount < 10)
                    countString = "00" + internalNameCount.ToString();
                else if (internalNameCount < 100)
                    countString = "0" + internalNameCount.ToString();
                else
                    countString = internalNameCount.ToString();

                InternalNum += "-" + countString;
                return InternalNum;
            }
            else
                return string.Empty;
        }

        public static void WORKPACK_Populate_Name(WORKPACK workpack, IEnumerable<SUBJOB> SUBJOBCollection, IEnumerable<DISCIPLINE> DISCIPLINECollection)
        {
            SUBJOB querySUBJOB = SUBJOBCollection.FirstOrDefault(x => x.GUID == workpack.GUID_SUBJOB);
            DISCIPLINE queryDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == workpack.GUID_DISCIPLINE);

            if (querySUBJOB != null && queryDISCIPLINE != null)
            {
                string discipline_number = workpack.DISCIPLINE_NUM > 9 ? workpack.DISCIPLINE_NUM.ToString("0") : "0" + workpack.DISCIPLINE_NUM.ToString("0");
                workpack.NAME = querySUBJOB.INTERNAL_NAME1 + "-" + queryDISCIPLINE.CODE + discipline_number;
            }
            else
                workpack.NAME = string.Empty;
        }

        /// <summary>
        /// Generate internal number2 when all required fields are populated
        /// Phase type and charge type option will precede the condition of checking by entity phase guid
        /// </summary>
        public static string SUBJOB_Generate_InternalNumber(Guid? entityAreaGuid, Guid? entitySubAreaGuid, PROJECT PROJECT, IEnumerable<AREA> AREACollection, IEnumerable<AREA> SUBAREACollection, out Guid? phase_guid, Guid? assignedPHASEKey = null, IEnumerable<PHASE> PHASECollection = null, PhaseType? PhaseType = null, ChargeType? ChargeType = null)
        {
            phase_guid = null;
            if (entityAreaGuid == Guid.Empty)
                return string.Empty;

            AREA area = AREACollection.FirstOrDefault(x => x.GUID == entityAreaGuid);
            AREA subarea = SUBAREACollection.FirstOrDefault(x => x.GUID == entitySubAreaGuid);

            PHASE phase;
            if (PHASECollection == null)
                return string.Empty;

            if (PhaseType != null || ChargeType != null)
            {
                if(ChargeType == null)
                    phase = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE == PhaseType);
                else
                    phase = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE == PhaseType && x.CHARGE_TYPE == ChargeType);
            }
            else if (assignedPHASEKey != null)
                phase = PHASECollection.FirstOrDefault(x => x.GUID == assignedPHASEKey);
            else
                return string.Empty;

            if (phase == null)
                return string.Empty;

            if (area != null)
            {
                phase_guid = phase.GUID;
                string phaseNumber = phase.INTERNAL_NUM;
                string areaNumber = area == null ? BluePrintsResources.Default_Area : area.INTERNAL_NUM;
                string subAreaNumber = subarea == null ? BluePrintsResources.Default_Sub_Area : subarea.INTERNAL_NUM;

                var InternalName = PROJECT.NUMBER;
                InternalName += "-" + areaNumber;
                InternalName += "-" + subAreaNumber;
                InternalName += "-" + phaseNumber;
                return InternalName;
            }
            else
                return string.Empty;
        }
    }
}