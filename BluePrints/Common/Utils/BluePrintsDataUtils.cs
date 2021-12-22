using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Services;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
using BluePrints.Common.Helpers;
using BluePrints.Common.Misc;
using BluePrints.Common.Projections;
using BluePrints.Common.Resources;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using DevExpress.Data.Filtering;
using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Deployment.Application;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BluePrints.Common.ViewModel.Utils
{
    public static class BluePrintsUtils
    {
        /// <summary>
        /// Encrypt a string
        /// </summary>
        /// <param name="toEncrypt">string to encrypt</param>
        /// <param name="useHashing">use hashing</param>
         /// <returns>encrypted string</returns>
        public static string Encrypt(string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            string key = BluePrintsResources.SecurityKey;

            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //Always release the resources and flush data
                // of the Cryptographic service provide. Best Practice

                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// Decrypts into a string
        /// </summary>
        /// <param name="cipherString">encrypted string</param>
        /// <param name="useHashing">use hashing</param>
        /// <returns>decrypted string</returns>
        public static string Decrypt(string cipherString, bool useHashing)
        {
            byte[] keyArray;
            //get the byte code of the string

            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            string key = BluePrintsResources.SecurityKey;

            if (useHashing)
            {
                //if hashing was used get the hash code with regards to your key
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //release any resource held by the MD5CryptoServiceProvider

                hashmd5.Clear();
            }
            else
            {
                //if hashing was not implemented get the byte code of the key
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. 
            //We choose ECB(Electronic code Book)

            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        /// <summary>
        /// Day of week with Sunday being 7
        /// </summary>
        public static int GetTrueDayOfWeek(int DayOfWeekUS)
        {
            int retVal = 0;
            if (DayOfWeekUS == 0)
                retVal = 7;
            else
                retVal = DayOfWeekUS;

            return retVal;
        }

        /// <summary>
        /// Change the progress data date
        /// </summary>
        /// <param name="navigationType">Forward, backward or last week ending</param>
        /// <param name="loadPROGRESS">Progress to change</param>
        /// <returns>Whether should save</returns>
        public static bool ProgressDateChange(DateNavigationType navigationType, PROGRESS loadPROGRESS, bool isReportDate)
        {
            var interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            DateTime endOfDayToday = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
            int multiplier;

            int trueDayOfWeekDataDate = GetTrueDayOfWeek((int)loadPROGRESS.DATA_DATE.DayOfWeek);
            int trueDayOfWeekToday = GetTrueDayOfWeek((int)DateTime.Today.DayOfWeek);

            DateTime endOfCurrentWeek = DateTime.Today.Date.AddDays(trueDayOfWeekDataDate - trueDayOfWeekToday).AddDays(1).AddSeconds(-1);
            DateTime endOfPreviousWeek = endOfCurrentWeek.AddDays(-7);

            if (navigationType == DateNavigationType.Current)
            {
                if(loadPROGRESS.INTERVAL_TYPE == ProgressIntervalType.Daily)
                {
                    bool shouldSave = false;
                    if (isReportDate && !loadPROGRESS.DISABLE_AUTO_REPORT_DATE)
                    {
                        loadPROGRESS.REPORT_DATE = endOfDayToday;
                        shouldSave = true;
                    }

                    if (loadPROGRESS.DATA_DATE != endOfDayToday)
                    {
                        loadPROGRESS.DATA_DATE = endOfDayToday;
                        shouldSave = true;
                    }

                    return shouldSave;
                }
                //for users that always uses report date
                else if(isReportDate && !loadPROGRESS.DISABLE_AUTO_REPORT_DATE)
                {
                    //rewind the data one week when progress is updated for the current week but reporting is done on the previous week
                    //will be saved when data date is saved
                    if(loadPROGRESS.REPORT_DATE == null)
                    {
                        loadPROGRESS.REPORT_DATE = endOfPreviousWeek;
                        return true;
                    }
                    else if (((DateTime)loadPROGRESS.REPORT_DATE).Date != endOfPreviousWeek.Date)
                    {
                        loadPROGRESS.REPORT_DATE = endOfPreviousWeek;
                        return true;
                    }
                    else
                        return false;
                }
                //for users that always uses data date
                else
                {
                    if (loadPROGRESS.USE_CURRENT_WEEK)
                    {
                        if (loadPROGRESS.DATA_DATE.Date != endOfCurrentWeek.Date)
                        {
                            loadPROGRESS.DATA_DATE = endOfCurrentWeek;
                            if(!loadPROGRESS.DISABLE_AUTO_REPORT_DATE)
                                loadPROGRESS.REPORT_DATE = endOfPreviousWeek;

                            return true;
                        }
                        else
                            return false;
                    }
                    else
                    {
                        if (loadPROGRESS.DATA_DATE.Date != endOfPreviousWeek.Date)
                        {
                            loadPROGRESS.DATA_DATE = endOfPreviousWeek;
                            if (!loadPROGRESS.DISABLE_AUTO_REPORT_DATE)
                                loadPROGRESS.REPORT_DATE = endOfPreviousWeek;

                            return true;
                        }
                        else
                            return false;
                    }
                }
            }
            else
            {
                multiplier = navigationType == DateNavigationType.Forward ? 1 : -1;
                if (loadPROGRESS.REPORT_DATE == null)
                    loadPROGRESS.REPORT_DATE = loadPROGRESS.DATA_DATE;

                if(isReportDate)
                    loadPROGRESS.REPORT_DATE = ((DateTime)loadPROGRESS.REPORT_DATE).AddDays(multiplier * interval.Days);
                else
                    loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(multiplier * interval.Days);

                return true;
            }
        }

        public static DateTime GetNearestSundayOfTheMonth(DateTime date)
        {
            DateTime lastDayOfMonth = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
            DayOfWeek dayOfWeek = lastDayOfMonth.DayOfWeek;

            if (dayOfWeek > DayOfWeek.Wednesday)
                return ChronologicalHelpers.GetFirstWeekdayOfNextMonth(date, DayOfWeek.Sunday);
            else
                return ChronologicalHelpers.GetLastWeekdayOfMonth(date, DayOfWeek.Sunday);
        }

        public static void ApplyShowBookableFilter(IGridControlService gridControlService, bool bookableToggleValue)
        {
            if (gridControlService != null)
            {
                if (bookableToggleValue)
                {
                    CriteriaOperator criteriaOperator = gridControlService.FilterCriteria;
                    CriteriaOperator newCriteriaOperator;
                    if (!ReferenceEquals(criteriaOperator, null))
                    {
                        string filterCriteria = criteriaOperator.ToString() + " And [CanBook] In (True)";
                        newCriteriaOperator = CriteriaOperator.Parse(filterCriteria);
                    }
                    else
                    {
                        newCriteriaOperator = CriteriaOperator.Parse("[CanBook] In (True)");
                    }

                    gridControlService.FilterCriteria = newCriteriaOperator;
                }
                else
                {
                    CriteriaOperator criteriaOperator = gridControlService.FilterCriteria;
                    if (!ReferenceEquals(criteriaOperator, null))
                    {
                        CriteriaOperator newCriteriaOperator;
                        string currentFilterCriteria = criteriaOperator.ToString();
                        string newfilterCriteria = currentFilterCriteria.Replace("And [CanBook] In (True)", "");
                        newfilterCriteria = newfilterCriteria.Replace("[CanBook] In (True)", "");
                        if (newfilterCriteria.Length >= 5)
                        {
                            string firstFiveChar = newfilterCriteria.Substring(0, 5);
                            if (firstFiveChar.ToUpper().Contains("AND"))
                                newfilterCriteria = newfilterCriteria.Substring(5, newfilterCriteria.Length - 5);
                        }


                        newCriteriaOperator = CriteriaOperator.Parse(newfilterCriteria);
                        gridControlService.FilterCriteria = newCriteriaOperator;
                    }
                }
            }
        }

        public static void LoadExoAuthorisation<TProjection>(IEnumerable<TProjection> projections, ref List<ExoTimeAuthorisation> exoAuthorisations, List<ProjectUnitOfWorkContext> projectContexts, List<UserIdsAuthorisationContext> userIdForCanBook, bool useFirstFoundStaffNo = false)
            where TProjection : IReportable, IBookable
        {
            if(exoAuthorisations == null)
            {
                List<ExoTimeAuthorisation> cacheExoAuthorisations = new List<ExoTimeAuthorisation>();
                foreach (var projectContext in projectContexts)
                {
                    int? firstFoundStaffNo = null;
                    if (useFirstFoundStaffNo && userIdForCanBook != null && userIdForCanBook.Count > 0)
                    {
                        UserIdsAuthorisationContext userIdAuthorisation = userIdForCanBook.First();
                        firstFoundStaffNo = userIdAuthorisation.Id;
                    }

                    List<ExoTimeAuthorisation> projectExoTimeAuths = ExoQueries.GetExoLinesAuthorisations(projectContext.PrimeroEntitiesUnitOfWork, projectContext.ProjectNumber, firstFoundStaffNo);
                    projectExoTimeAuths.ForEach(x => x.OfficeName = projectContext.OfficeName);
                    cacheExoAuthorisations.AddRange(projectExoTimeAuths);
                }

                exoAuthorisations = new List<ExoTimeAuthorisation>(cacheExoAuthorisations);
            }

            //view can be closed if this is a async task and projection can be disposed
            if(projections != null)
            {
                //to prevent enumeration change error
                List<TProjection> projectionsList = projections.ToList();
                foreach (var deliverable in projectionsList)
                {
                    ExoTimeAuthorisation findAuthorisation = exoAuthorisations.Where(x => userIdForCanBook.Any(y => y.Id == x.ResourceStaffId && y.OfficeName == x.OfficeName)).FirstOrDefault(x => x.SubJobCode == deliverable.Subjob_Name && x.DisciplineCode == deliverable.Discipline_Code && x.CommodityCode == deliverable.Commodity_Code);
                    deliverable.CanBook = findAuthorisation != null;
                    deliverable.Update();
                }
            }
        }

        public static int? GetUpdatedProjectLocaleUserExoId(string officeName, IEnumerable<USER> USERCollection, Guid currentUserGuid)
        {
            USER currentUser = USERCollection.FirstOrDefault(x => x.GUID == currentUserGuid);
            if (currentUser == null)
                return null;

            if (officeName == BluePrintsResources.OfficeMontreal)
                return currentUser.EXO_STAFF_ID_MONTREAL;
            else if (officeName == BluePrintsResources.OfficeUSA)
                return currentUser.EXO_STAFF_ID_USA;
            else
                return currentUser.EXO_STAFF_ID_PERTH;
        }

        public static void BookTime(IDeliverable deliverable, IPrimeroEntitiesUnitOfWork primeroUnitOfWork, List<ExoTimeAuthorisation> exoAuthorisations, string defaultNarrative, IMessageBoxService MessageBoxService, IDialogService BookTimeDialogService, PROJECT project, IEnumerable<USER> USERCollection)
        {
            string pmName = project.USER == null ? string.Empty : project.USER.NAME;
            int? currentUserExoId = BluePrintsUtils.GetUpdatedProjectLocaleUserExoId(project.OfficeNameForExo, USERCollection, LoginCredentials.CurrentUserGuid);
            if (currentUserExoId == null)
            {
                MessageBoxService.ShowMessage(project.OfficeName + " EXO account is not set for user " + LoginCredentials.CurrentUser.NAME + "\nPlease email " + BluePrintsResources.ITEmail);
                return;
            }

            var bookTimeViewModel = BookTimeSheetViewModel.Create(deliverable, primeroUnitOfWork, exoAuthorisations, defaultNarrative, (int)currentUserExoId);
            if (bookTimeViewModel.GetResource() == null)
            {
                MessageBoxService.ShowMessage("You are not authorised to book time on this subjob, please contact the project manager for assistance");
                return;
            }
            else if (bookTimeViewModel.GetCostType() == null)
            {
                MessageBoxService.ShowMessage("You do not have authorisation to book time to\nSub Job: " + deliverable.Subjob_Name + "\nDiscipline Code: " + deliverable.Discipline_Code + "\nCommodity Code: " + deliverable.Commodity_Code + "\n\nPlease contact " + pmName + " for assistance");
                return;
            }

            PrimeroSubJob subJob = bookTimeViewModel.GetSubJob();
            if(subJob != null && subJob.JobStatus.ToUpper() == "G")
            {
                MessageBoxService.ShowMessage("Job " + subJob.Code + " has already been closed, please contact cost control to open the job");
                return;
            }
            else if (BookTimeDialogService.ShowDialog(MessageButton.OKCancel, "Enter time to book", "BookTimeDialog", bookTimeViewModel) == MessageResult.OK)
            {
                string narrative = bookTimeViewModel.GetNarratives();
                PrimeroResource bookResource = bookTimeViewModel.GetResource();
                TimesheetDate bookDate = bookTimeViewModel.GetTimesheetDate();
                PrimeroDiscipline bookCostGroup = bookTimeViewModel.GetCostGroup();
                PrimeroCommodity bookCostType = bookTimeViewModel.GetCostType();
                string variationCode = bookTimeViewModel.GetVariationCode();
                decimal bookTime = bookTimeViewModel.BookHours;

                //variation code is always saved in exo as null
                variationCode = variationCode == string.Empty ? null : variationCode;
                if (bookResource != null && bookCostGroup != null && bookCostType != null)
                {
                    JOB_TIMESHEETS findTimeSheetNarrative = FindJOB_TIMESHEETS(primeroUnitOfWork, bookResource.SeqNo, subJob.Id, bookResource.StockCode, bookCostGroup.Id, bookCostType.Id, variationCode, bookDate.WeekStartDate, narrative);
                    if (findTimeSheetNarrative != null)
                    {
                        AdjustTimeSheetHours(findTimeSheetNarrative, bookDate, deliverable, bookTime, primeroUnitOfWork);
                    }
                    else
                    {
                        string title = subJob.Code + " : " + subJob.Title;
                        if (title.Length >= 60)
                            title = title.Substring(0, 59);

                        JOB_TIMESHEETS newTimeSheet = InitNewTimeSheet();
                        newTimeSheet.STAFFNO = bookResource.SeqNo;
                        newTimeSheet.JOBNO = subJob.Id;
                        newTimeSheet.TITLE = title;
                        newTimeSheet.STOCKCODE = bookResource.StockCode;
                        newTimeSheet.DESCRIPTION = bookCostType.StockDescription;
                        newTimeSheet.WEEK_START_DATE = bookDate.WeekStartDate;
                        AdjustTimeSheetHours(newTimeSheet, bookDate, deliverable, bookTime, primeroUnitOfWork);
                        newTimeSheet.COST_GROUP = bookCostGroup.Id;
                        newTimeSheet.COST_TYPE = bookCostType.Id;
                        newTimeSheet.X_APPROVAL_MANAGER = -1;
                        newTimeSheet.X_NARRATIVE = narrative;
                        newTimeSheet.X_VARIATIONCODE = variationCode;
                        primeroUnitOfWork.JOB_TIMESHEETS.Add(newTimeSheet);
                    }

                    primeroUnitOfWork.SaveChanges();
                }
            }
        }

        public static JOB_TIMESHEETS InitNewTimeSheet()
        {
            JOB_TIMESHEETS newTimeSheet = new JOB_TIMESHEETS();
            newTimeSheet.UNITPRICE = 0;
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
            newTimeSheet.LABOUR_ALLOWANCE = 0;
            newTimeSheet.HAS_ALLOWANCE = "N";
            newTimeSheet.X_DECLINED = false;
            newTimeSheet.X_APPROVAL_MANAGER = -1;
            newTimeSheet.X_SUBMITTED = false;

            return newTimeSheet;
        }

        public static JOB_TIMESHEETS FindJOB_TIMESHEETS(IPrimeroEntitiesUnitOfWork primeroUnitOfWork, int? staffNo, int? subJobId, string stockCode, int? costGroup, int? costType, string variationCode, DateTime? weekStartDate, string narrative)
        {
            IQueryable<JOB_TIMESHEETS> findTimeSheets = primeroUnitOfWork.JOB_TIMESHEETS.Where(x => x.STAFFNO == staffNo && x.JOBNO == subJobId && x.STOCKCODE == stockCode && x.COST_GROUP == costGroup && x.COST_TYPE == costType && x.X_VARIATIONCODE == variationCode && x.WEEK_START_DATE == weekStartDate);
            JOB_TIMESHEETS findTimeSheetNarrative;
            if (narrative == null || narrative == string.Empty)
                findTimeSheetNarrative = findTimeSheets.FirstOrDefault(x => x.X_NARRATIVE == null || x.X_NARRATIVE == string.Empty);
            else
                findTimeSheetNarrative = findTimeSheets.FirstOrDefault(x => x.X_NARRATIVE == narrative);

            return findTimeSheetNarrative;
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

        public static int FindExistingOrAddNewNarrative(string description, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
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
        public static void OnBeforeSavingBASELINE_ITEM(IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork, BASELINE_ITEMProgress projection, PROJECT loadPROJECT, BASELINE loadBASELINE, DeliverablesViewType viewType, IEnumerable<PHASE> PHASECollection, IEnumerable<BASELINE_ITEM> Entities, IEnumerable<AREA> AREACollection, IEnumerable<AREA> SUBAREACollection, IEnumerable<DISCIPLINE> DISCIPLINECollection, IEnumerable<DOCTYPE> DOCTYPECollection, IEnumerable<WORKPACK> WORKPACKCollection, IEnumerable<SUBJOB> SUBJOBCollection, CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACKSCollectionViewModel, bool allowSubJobDeletion, bool allowWorkpackDeletion)
        {
            if (projection.Entity.Entity.GUID_OFFICE == null)
                projection.Entity.Entity.GUID_OFFICE = loadPROJECT.GUID_OFFICE;

            //this context is inherited by variation and is used to save newly added variation
            if (projection.Entity.Entity.GUID_BASELINE == null && projection.Entity.Entity.GUID_VARIATION == null)
                projection.Entity.Entity.GUID_BASELINE = loadBASELINE.GUID;

            PhaseType? phaseType = null;
            ChargeType? chargeType = null;
            PHASE defaultPHASE = null;
            if (viewType == DeliverablesViewType.Direct || viewType == DeliverablesViewType.Both)
            {
                phaseType = PhaseType.Design;
                chargeType = ChargeType.Chargeable;
                defaultPHASE = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE != null && x.PHASE_TYPE == PhaseType.Design && x.CHARGE_TYPE != null && x.CHARGE_TYPE == ChargeType.Chargeable);
                if (viewType != DeliverablesViewType.Both && defaultPHASE != null)
                    projection.Phase_Guid = defaultPHASE.GUID;
            }
            else if (viewType == DeliverablesViewType.Indirect)
            {
                phaseType = PhaseType.Design;
                chargeType = ChargeType.NotChargeable;
                PHASE indirectPHASE = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE != null && x.PHASE_TYPE == PhaseType.Design && x.CHARGE_TYPE != null && x.CHARGE_TYPE == ChargeType.NotChargeable);
                if (indirectPHASE != null)
                    projection.Phase_Guid = indirectPHASE.GUID;
            }
            else if (projection.Phase_Guid == null)
            {
                defaultPHASE = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE != null && x.PHASE_TYPE == PhaseType.Design && x.CHARGE_TYPE != null && x.CHARGE_TYPE == ChargeType.Chargeable);
                if (defaultPHASE != null)
                    projection.Phase_Guid = defaultPHASE.GUID;
            }

            string errorMessage = string.Empty;
            if (projection.GUID == Guid.Empty && projection.Entity.Entity.INTERNAL_NUM == string.Empty && projection.IsInternalNumberEditable)
                projection.Entity.Entity.INTERNAL_NUM = GenerateInternalNumber(projection, loadPROJECT, Entities, AREACollection, DISCIPLINECollection, DOCTYPECollection, out errorMessage);

            assignDeliverablePhase(projection, defaultPHASE, DOCTYPECollection, PHASECollection);
            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignSubjob(loadPROJECT, PHASECollection, AREACollection, SUBAREACollection, projection, bluePrintsUnitOfWork, phaseType, chargeType, false, allowSubJobDeletion);
            BluePrintsDataUtils.OnBeforeSavedGenerateAndAssignWorkpack(loadPROJECT, projection, WORKPACKSCollectionViewModel, SUBJOBCollection, DISCIPLINECollection, allowWorkpackDeletion);
            projection.Update();
        }

        private static void assignDeliverablePhase(BASELINE_ITEMProgress projection, PHASE defaultPHASE, IEnumerable<DOCTYPE> DOCTYPECollection, IEnumerable<PHASE> PHASECollection)
        {
            if (defaultPHASE == null)
                return;

            if (projection.Phase_Guid == null || (projection.Entity.Entity.GUID_DOCTYPE != null && IsDocTypePhaseValid(projection.Entity.Entity.GUID_DOCTYPE, projection.Phase_Guid, PHASECollection, DOCTYPECollection) != string.Empty))
            {
                if (projection.Entity.Entity.DOCTYPE != null)
                {
                    DOCTYPE findDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_DOCTYPE);
                    if (findDOCTYPE != null)
                    {
                        if (findDOCTYPE.IS_INDIRECT_ONLY)
                            defaultPHASE = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE != null && x.PHASE_TYPE == PhaseType.Indirect && x.CHARGE_TYPE != null && x.CHARGE_TYPE == ChargeType.Chargeable);
                        else
                            defaultPHASE = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE != null && x.PHASE_TYPE == PhaseType.Design && x.CHARGE_TYPE != null && x.CHARGE_TYPE == ChargeType.Chargeable);

                        if (defaultPHASE != null)
                            projection.Phase_Guid = defaultPHASE.GUID;
                    }
                }
                else
                {
                    defaultPHASE = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE != null && x.PHASE_TYPE == PhaseType.Design && x.CHARGE_TYPE != null && x.CHARGE_TYPE == ChargeType.Chargeable);
                    if (defaultPHASE != null)
                        projection.Phase_Guid = defaultPHASE.GUID;
                }
            }
        }

        public static string IsDocTypePhaseValid(Guid? doctypeGuid, Guid? phaseGuid, IEnumerable<PHASE> PHASECollection, IEnumerable<DOCTYPE> DOCTYPECollection)
        {
            if (doctypeGuid != null && phaseGuid != null)
            {
                PHASE phase = PHASECollection.FirstOrDefault(x => x.GUID == phaseGuid);
                if (phase != null)
                {
                    DOCTYPE doctype = DOCTYPECollection.FirstOrDefault(x => x.GUID == doctypeGuid);
                    if (doctype != null)
                    {
                        if (!doctype.IS_INDIRECT_ONLY && phase.PHASE_TYPE == PhaseType.Indirect)
                            return "Doc type cannot be assigned with indirect phase";
                        else if (doctype.IS_INDIRECT_ONLY && phase.PHASE_TYPE == PhaseType.Design)
                            return "Doc type must be assigned with indirect phase";
                    }
                }
            }

            return string.Empty;
        }

        public static string GenerateInternalNumber(BASELINE_ITEMProgress projectionEntity, PROJECT loadPROJECT, IEnumerable<BASELINE_ITEM> Entities, IEnumerable<AREA> AREACollection, IEnumerable<DISCIPLINE> DISCIPLINECollection, IEnumerable<DOCTYPE> DOCTYPECollection, out string errorMessage)
        {
            if (projectionEntity.Entity.Entity.INTERNAL_NUM != null && projectionEntity.Entity.Entity.INTERNAL_NUM != string.Empty)
            {
                errorMessage = string.Empty;
                return projectionEntity.Entity.Entity.INTERNAL_NUM;
            }

            AREA currentItemAREA = AREACollection.FirstOrDefault(x => x.GUID == projectionEntity.Entity.Entity.GUID_AREA);
            DISCIPLINE currentItemDISCIPLINE = DISCIPLINECollection.FirstOrDefault(x => x.GUID == projectionEntity.Entity.Entity.GUID_DISCIPLINE);
            DOCTYPE currentItemDOCTYPE = DOCTYPECollection.FirstOrDefault(x => x.GUID == projectionEntity.Entity.Entity.GUID_DOCTYPE);

            errorMessage = string.Empty;
            if (currentItemAREA == null)
                errorMessage += "Area, ";

            if (currentItemDISCIPLINE == null)
                errorMessage += "Discipline, ";

            if (currentItemDOCTYPE == null)
                errorMessage += "Document Type, ";

            if (errorMessage.Length > 2)
                errorMessage = errorMessage.Substring(0, errorMessage.Length - 2) + " is missing";

            string internalNum = BluePrintsDataUtils.BASELINEITEM_Generate_InternalNumber(loadPROJECT, Entities, currentItemAREA, currentItemDISCIPLINE, currentItemDOCTYPE, projectionEntity.GUID);

            return internalNum;
        }

        public static void CreateNewProjectDefaults(PROJECT entity, IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork)
        {
            Tuple<DateTime, DateTime> tenderStartEndDate = BluePrintsDataUtils.GetTenderStartEndDate(entity);
            DateTime? tenderStartDate = tenderStartEndDate.Item1;
            DateTime? tenderEndDate = tenderStartEndDate.Item2;
            //only way to determine whether current entity is new to avoid creating multiple 

            BASELINE newBASELINE = new BASELINE();
            newBASELINE.GUID_PROJECT = entity.GUID;
            newBASELINE.NAME = entity.NUMBER + "_001";
            newBASELINE.REVISION = "A";
            newBASELINE.STATUS = BaselineStatus.Live;
            bluePrintsEntitiesUnitOfWork.BASELINES.Add(newBASELINE);
            //BASELINEViewModel.Save(newBASELINE);

            ESTIMATE newESTIMATE_DIRECT = new ESTIMATE();
            newESTIMATE_DIRECT.GUID_PROJECT = entity.GUID;
            newESTIMATE_DIRECT.NAME = entity.NUMBER + "_001";
            newESTIMATE_DIRECT.REVISION = "A";
            newESTIMATE_DIRECT.STATUS = BaselineStatus.Live;
            bluePrintsEntitiesUnitOfWork.ESTIMATES.Add(newESTIMATE_DIRECT);
            //ESTIMATEViewModel.Save(newESTIMATE_DIRECT);

            PROGRESS newDesignPROGRESS = new PROGRESS();
            newDesignPROGRESS.GUID_PROJECT = entity.GUID;
            newDesignPROGRESS.NAME = entity.NUMBER + "WEEKLY_001";
            newDesignPROGRESS.PROGRESS_START = tenderStartDate == null ? DateTime.Now : (DateTime)tenderStartDate;
            newDesignPROGRESS.DATA_DATE = CommonMethods.GetStartOfWeek(newDesignPROGRESS.PROGRESS_START, DayOfWeek.Sunday).AddDays(1).AddSeconds(-1);
            newDesignPROGRESS.INTERVAL_COUNT = 1;
            newDesignPROGRESS.INTERVAL_TYPE = ProgressIntervalType.Weekly;
            newDesignPROGRESS.STATUS = ProgressStatus.Live;
            newDesignPROGRESS.TYPE = PhaseType.Design;
            bluePrintsEntitiesUnitOfWork.PROGRESSES.Add(newDesignPROGRESS);
            //PROGRESSViewModel.Save(newDesignPROGRESS);

            PROGRESS newConstructionPROGRESS = new PROGRESS();
            newConstructionPROGRESS.GUID_PROJECT = entity.GUID;
            newConstructionPROGRESS.NAME = entity.NUMBER + "DAILY_001";
            newConstructionPROGRESS.PROGRESS_START = tenderStartDate == null ? DateTime.Now : (DateTime)tenderStartDate;
            newConstructionPROGRESS.DATA_DATE = CommonMethods.GetStartOfWeek(newConstructionPROGRESS.PROGRESS_START, DayOfWeek.Sunday).AddDays(1).AddSeconds(-1);
            newConstructionPROGRESS.INTERVAL_COUNT = 1;
            newConstructionPROGRESS.INTERVAL_TYPE = ProgressIntervalType.Daily;
            newConstructionPROGRESS.STATUS = ProgressStatus.Live;
            newConstructionPROGRESS.TYPE = PhaseType.Construct;
            bluePrintsEntitiesUnitOfWork.PROGRESSES.Add(newConstructionPROGRESS);
            //PROGRESSViewModel.Save(newConstructionPROGRESS);

            AREA defaultArea = new AREA();
            defaultArea.GUID_PROJECT = entity.GUID;
            defaultArea.INTERNAL_NUM = "000";
            defaultArea.CLIENT_NUM = "000";
            defaultArea.TITLE = "General";
            bluePrintsEntitiesUnitOfWork.AREAS.Add(defaultArea);
            //AREAViewModel.Save(defaultArea);
            bluePrintsEntitiesUnitOfWork.SaveChanges();

            PHASE defaultDirectPhase = bluePrintsEntitiesUnitOfWork.PHASES.FirstOrDefault(x => x.INTERNAL_NUM == "D1");
            PHASE defaultIndirectPhase = bluePrintsEntitiesUnitOfWork.PHASES.FirstOrDefault(x => x.INTERNAL_NUM == "I1");

            DEPARTMENT defaultDepartment = bluePrintsEntitiesUnitOfWork.DEPARTMENTS.FirstOrDefault(x => x.NAME == BluePrintsResources.Default_New_Project_Department);
            DISCIPLINE defaultDiscipline = bluePrintsEntitiesUnitOfWork.DISCIPLINES.FirstOrDefault(x => x.NAME == BluePrintsResources.Default_New_Project_Discipline);
            DOCTYPE defaultDocType = bluePrintsEntitiesUnitOfWork.DOCTYPES.FirstOrDefault(x => x.NAME == BluePrintsResources.Default_New_Project_DocType);

            string defaultPROJECTNumber;
#if PERTH
            defaultPROJECTNumber = "00000";
#else
            defaultPROJECTNumber = "00015";
#endif

            PROJECT defaultCopyProject = bluePrintsEntitiesUnitOfWork.PROJECTS.FirstOrDefault(x => x.NUMBER == defaultPROJECTNumber);
            if (defaultCopyProject != null)
            {
                foreach (RATE rate in defaultCopyProject.RATE)
                {
                    RATE newRATE = new RATE();
                    DataUtils.ShallowCopy(newRATE, rate);
                    newRATE.GUID = Guid.Empty;
                    newRATE.GUID_PROJECT = entity.GUID;
                    bluePrintsEntitiesUnitOfWork.RATES.Add(newRATE);
                    //RATEViewModel.Save(newRATE);
                }

                foreach (DELIVERABLES_STATUS status in defaultCopyProject.DELIVERABLES_STATUS)
                {
                    DELIVERABLES_STATUS newSTATUS = new DELIVERABLES_STATUS();
                    DataUtils.ShallowCopy(newSTATUS, status);
                    newSTATUS.GUID = Guid.Empty;
                    newSTATUS.GUID_PROJECT = entity.GUID;
                    bluePrintsEntitiesUnitOfWork.DELIVERABLES_STATUSES.Add(newSTATUS);

                    foreach (DSTATUS_DOCTYPE statusDocType in status.DSTATUS_DOCTYPE)
                    {
                        DSTATUS_DOCTYPE newSTATUS_DOCTYPE = new DSTATUS_DOCTYPE();
                        DataUtils.ShallowCopy(newSTATUS_DOCTYPE, statusDocType);
                        newSTATUS_DOCTYPE.GUID = Guid.Empty;
                        newSTATUS_DOCTYPE.GUID_STATUS = Guid.Empty;
                        newSTATUS.DSTATUS_DOCTYPE.Add(newSTATUS_DOCTYPE);
                    }
                }

                foreach (HOLIDAY holiday in defaultCopyProject.HOLIDAY)
                {
                    HOLIDAY newHOLIDAY = new HOLIDAY();
                    DataUtils.ShallowCopy(newHOLIDAY, holiday);
                    newHOLIDAY.GUID = Guid.Empty;
                    newHOLIDAY.GUID_PROJECT = entity.GUID;
                    bluePrintsEntitiesUnitOfWork.HOLIDAYS.Add(newHOLIDAY);
                }
            }

            SUBJOB newSUBJOB = new SUBJOB();
            newSUBJOB.GUID_PROJECT = entity.GUID;
            newSUBJOB.INTERNAL_NAME1 = entity.NUMBER;
            newSUBJOB.STARTDATE = tenderStartDate == null ? CommonMethods.GetStartOfWeek(DateTime.Now, DayOfWeek.Sunday) : tenderStartDate;
            newSUBJOB.ENDDATE = tenderEndDate == null ? ((DateTime)newSUBJOB.STARTDATE).AddDays(7).AddSeconds(-1) : tenderEndDate;
            newSUBJOB.REVIEWSTARTDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
            newSUBJOB.REVIEWENDDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
            newSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;

            if (entity.STATUS == ProjectStatus.Tender || entity.STATUS == ProjectStatus.TenderSubmitted)
            {
                newSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;
            }

            newSUBJOB.GUID_DAREA = defaultArea.GUID;
            newSUBJOB.GUID_DPHASE = defaultDirectPhase.GUID;
            bluePrintsEntitiesUnitOfWork.SUBJOBS.Add(newSUBJOB);

            if (defaultDirectPhase != null)
            {
                SUBJOB defaultDesignSUBJOB = new SUBJOB();
                defaultDesignSUBJOB.GUID_PROJECT = entity.GUID;
                defaultDesignSUBJOB.INTERNAL_NAME1 = entity.NUMBER + "-000-00-D1";
                defaultDesignSUBJOB.STARTDATE = tenderStartDate == null ? CommonMethods.GetStartOfWeek(DateTime.Now, DayOfWeek.Sunday) : tenderStartDate;
                defaultDesignSUBJOB.ENDDATE = tenderEndDate == null ? ((DateTime)newSUBJOB.STARTDATE).AddDays(7).AddSeconds(-1) : tenderEndDate;
                defaultDesignSUBJOB.REVIEWSTARTDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                defaultDesignSUBJOB.REVIEWENDDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                defaultDesignSUBJOB.GUID_DAREA = defaultArea.GUID;
                defaultDesignSUBJOB.GUID_DPHASE = defaultDirectPhase.GUID;
                defaultDesignSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;
                if (entity.STATUS == ProjectStatus.Tender || entity.STATUS == ProjectStatus.TenderSubmitted)
                {
                    defaultDesignSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;
                }
                bluePrintsEntitiesUnitOfWork.SUBJOBS.Add(defaultDesignSUBJOB);
                bluePrintsEntitiesUnitOfWork.SaveChanges();
                //SUBJOBViewModel.Save(defaultDesignSUBJOB);

                DISCIPLINE PMDiscipline = bluePrintsEntitiesUnitOfWork.DISCIPLINES.FirstOrDefault(x => x.CODE == "PM");
                if (PMDiscipline != null)
                {
                    WORKPACK pmWORKPACK = new WORKPACK();
                    pmWORKPACK.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                    pmWORKPACK.GUID_DISCIPLINE = PMDiscipline.GUID;
                    pmWORKPACK.NAME = entity.NUMBER + "-000-00-D1-PM01";
                    bluePrintsEntitiesUnitOfWork.WORKPACKS.Add(pmWORKPACK);
                    //WORKPACKViewModel.Save(pmWORKPACK);
                    bluePrintsEntitiesUnitOfWork.SaveChanges();

                    DOCTYPE manDOCTYPE = bluePrintsEntitiesUnitOfWork.DOCTYPES.FirstOrDefault(x => x.CODE == "MAN");
                    DEPARTMENT emDEPARTMENT = bluePrintsEntitiesUnitOfWork.DEPARTMENTS.FirstOrDefault(x => x.CODE == "EM");
                    if (manDOCTYPE != null && emDEPARTMENT != null)
                    {
                        BASELINE_ITEM dmBASELINE_ITEM = new BASELINE_ITEM();
                        dmBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                        dmBASELINE_ITEM.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                        dmBASELINE_ITEM.GUID_DEPARTMENT = emDEPARTMENT.GUID;
                        dmBASELINE_ITEM.GUID_DISCIPLINE = PMDiscipline.GUID;
                        dmBASELINE_ITEM.GUID_DOCTYPE = manDOCTYPE.GUID;
                        dmBASELINE_ITEM.INTERNAL_NUM = entity.NUMBER + "-000-MAN-PM-001";
                        dmBASELINE_ITEM.PRIMARY_TITLE = "Design Management";
                        dmBASELINE_ITEM.GUID_WORKPACK = pmWORKPACK.GUID;
                        dmBASELINE_ITEM.GUID_AREA = defaultArea.GUID;
                        dmBASELINE_ITEM.GUID_PHASE = defaultDirectPhase.GUID;
                        bluePrintsEntitiesUnitOfWork.BASELINE_ITEMS.Add(dmBASELINE_ITEM);
                        //BASELINE_ITEMViewModel.Save(dmBASELINE_ITEM);
                    }
                }

                DISCIPLINE GEDiscipline = bluePrintsEntitiesUnitOfWork.DISCIPLINES.FirstOrDefault(x => x.CODE == "GE");
                if (GEDiscipline != null)
                {
                    WORKPACK geWORKPACK = new WORKPACK();
                    geWORKPACK.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                    geWORKPACK.GUID_DISCIPLINE = GEDiscipline.GUID;
                    geWORKPACK.NAME = entity.NUMBER + "-000-00-D1-GE01";
                    bluePrintsEntitiesUnitOfWork.WORKPACKS.Add(geWORKPACK);
                    //WORKPACKViewModel.Save(geWORKPACK);
                    bluePrintsEntitiesUnitOfWork.SaveChanges();

                    DOCTYPE mtgDOCTYPE = bluePrintsEntitiesUnitOfWork.DOCTYPES.FirstOrDefault(x => x.CODE == "MTG");
                    DOCTYPE repDOCTYPE = bluePrintsEntitiesUnitOfWork.DOCTYPES.FirstOrDefault(x => x.CODE == "REP");
                    DEPARTMENT enDEPARTMENT = bluePrintsEntitiesUnitOfWork.DEPARTMENTS.FirstOrDefault(x => x.CODE == "EN");
                    if (mtgDOCTYPE != null && enDEPARTMENT != null)
                    {
                        BASELINE_ITEM meetBASELINE_ITEM = new BASELINE_ITEM();
                        meetBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                        meetBASELINE_ITEM.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                        meetBASELINE_ITEM.GUID_DEPARTMENT = enDEPARTMENT.GUID;
                        meetBASELINE_ITEM.GUID_DISCIPLINE = GEDiscipline.GUID;
                        meetBASELINE_ITEM.GUID_DOCTYPE = mtgDOCTYPE.GUID;
                        meetBASELINE_ITEM.INTERNAL_NUM = entity.NUMBER + "-000-MTG-GE-001";
                        meetBASELINE_ITEM.PRIMARY_TITLE = "Meetings";
                        meetBASELINE_ITEM.GUID_WORKPACK = geWORKPACK.GUID;
                        meetBASELINE_ITEM.GUID_AREA = defaultArea.GUID;
                        meetBASELINE_ITEM.GUID_PHASE = defaultDirectPhase.GUID;
                        bluePrintsEntitiesUnitOfWork.BASELINE_ITEMS.Add(meetBASELINE_ITEM);
                        //BASELINE_ITEMViewModel.Save(meetBASELINE_ITEM);
                    }

                    if (repDOCTYPE != null && enDEPARTMENT != null)
                    {
                        BASELINE_ITEM rptBASELINE_ITEM = new BASELINE_ITEM();
                        rptBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                        rptBASELINE_ITEM.GUID_SUBJOB = defaultDesignSUBJOB.GUID;
                        rptBASELINE_ITEM.GUID_DEPARTMENT = enDEPARTMENT.GUID;
                        rptBASELINE_ITEM.GUID_DISCIPLINE = GEDiscipline.GUID;
                        rptBASELINE_ITEM.GUID_DOCTYPE = repDOCTYPE.GUID;
                        rptBASELINE_ITEM.INTERNAL_NUM = entity.NUMBER + "-000-REP-GE-001";
                        rptBASELINE_ITEM.GUID_WORKPACK = geWORKPACK.GUID;
                        rptBASELINE_ITEM.GUID_AREA = defaultArea.GUID;
                        rptBASELINE_ITEM.GUID_PHASE = defaultDirectPhase.GUID;
                        rptBASELINE_ITEM.PRIMARY_TITLE = "Report";
                        bluePrintsEntitiesUnitOfWork.BASELINE_ITEMS.Add(rptBASELINE_ITEM);
                    }
                }
            }

            if (defaultIndirectPhase != null)
            {
                SUBJOB indirectDesignSUBJOB = new SUBJOB();
                indirectDesignSUBJOB.GUID_PROJECT = entity.GUID;
                indirectDesignSUBJOB.INTERNAL_NAME1 = entity.NUMBER + "-000-00-I1";
                indirectDesignSUBJOB.STARTDATE = tenderStartDate == null ? CommonMethods.GetStartOfWeek(DateTime.Now, DayOfWeek.Sunday) : tenderStartDate;
                indirectDesignSUBJOB.ENDDATE = tenderEndDate == null ? ((DateTime)newSUBJOB.STARTDATE).AddDays(7).AddSeconds(-1) : tenderEndDate;
                indirectDesignSUBJOB.REVIEWSTARTDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                indirectDesignSUBJOB.REVIEWENDDATE = (DateTime)newSUBJOB.STARTDATE; //effectively nullifies review date
                indirectDesignSUBJOB.GUID_DAREA = defaultArea.GUID;
                indirectDesignSUBJOB.GUID_DPHASE = defaultIndirectPhase.GUID;
                indirectDesignSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;

                if (entity.STATUS == ProjectStatus.Tender || entity.STATUS == ProjectStatus.TenderSubmitted)
                {
                    indirectDesignSUBJOB.BELLCURVESHAPE = BellCurveShape.Balanced;
                }

                bluePrintsEntitiesUnitOfWork.SUBJOBS.Add(indirectDesignSUBJOB);

                bluePrintsEntitiesUnitOfWork.SaveChanges();
                DOCTYPE g02DOCTYPE = bluePrintsEntitiesUnitOfWork.DOCTYPES.FirstOrDefault(x => x.CODE == "G02");
                DEPARTMENT adDEPARTMENT = bluePrintsEntitiesUnitOfWork.DEPARTMENTS.FirstOrDefault(x => x.CODE == "AD");
                DISCIPLINE PMDiscipline = bluePrintsEntitiesUnitOfWork.DISCIPLINES.FirstOrDefault(x => x.CODE == "PM");
                if (PMDiscipline != null)
                {
                    WORKPACK pmWORKPACK = new WORKPACK();
                    pmWORKPACK.GUID_SUBJOB = indirectDesignSUBJOB.GUID;
                    pmWORKPACK.GUID_DISCIPLINE = PMDiscipline.GUID;
                    pmWORKPACK.NAME = entity.NUMBER + "-000-00-I1-PM01";
                    bluePrintsEntitiesUnitOfWork.WORKPACKS.Add(pmWORKPACK);
                    bluePrintsEntitiesUnitOfWork.SaveChanges();

                    if (g02DOCTYPE != null && adDEPARTMENT != null)
                    {
                        BASELINE_ITEM dcBASELINE_ITEM = new BASELINE_ITEM();
                        dcBASELINE_ITEM.GUID_BASELINE = newBASELINE.GUID;
                        dcBASELINE_ITEM.GUID_SUBJOB = indirectDesignSUBJOB.GUID;
                        dcBASELINE_ITEM.GUID_DEPARTMENT = adDEPARTMENT.GUID;
                        dcBASELINE_ITEM.GUID_DISCIPLINE = PMDiscipline.GUID;
                        dcBASELINE_ITEM.GUID_DOCTYPE = g02DOCTYPE.GUID;
                        dcBASELINE_ITEM.INTERNAL_NUM = entity.NUMBER + "-000-G02-PM-001";
                        dcBASELINE_ITEM.PRIMARY_TITLE = "Document Control";
                        dcBASELINE_ITEM.GUID_WORKPACK = pmWORKPACK.GUID;
                        dcBASELINE_ITEM.GUID_AREA = defaultArea.GUID;
                        dcBASELINE_ITEM.GUID_PHASE = defaultIndirectPhase.GUID;
                        bluePrintsEntitiesUnitOfWork.BASELINE_ITEMS.Add(dcBASELINE_ITEM);
                    }
                }
            }
        }

        public static Tuple<DateTime, DateTime> GetTenderStartEndDate(PROJECT PROJECT)
        {
            DateTime PROJECTPlanStartDate = PROJECT.TENDER_PROJECT_START == null ? DateTime.Now : ((DateTime)PROJECT.TENDER_PROJECT_START);
            int duration = PROJECT.TENDER_PROJECT_DURATION == null ? 1 : (int)PROJECT.TENDER_PROJECT_DURATION;
            DateTime PROJECTPlanEndDate = PROJECTPlanStartDate.AddDays(duration * 7);

            return new Tuple<DateTime, DateTime>(PROJECTPlanStartDate, PROJECTPlanEndDate);
        }

        public static List<StatsCalculationType> AllCalcTypes
        {
            get
            {
                List<StatsCalculationType> allCalcTypes = new List<StatsCalculationType>();
                allCalcTypes.Add(StatsCalculationType.Earned);
                allCalcTypes.Add(StatsCalculationType.Planned);
                allCalcTypes.Add(StatsCalculationType.Remaining);
                allCalcTypes.Add(StatsCalculationType.Burned);

                return allCalcTypes;
            }
        }

        public static DateTime GetEarliestTransactionDate(IPrimeroEntitiesUnitOfWork primeroUOW, string projectNumber)
        {
            var jobTransactions = from JOBTRANS in primeroUOW.JOB_TRANSACTIONS
                                  join JOBCOST_HDR2 in primeroUOW.JOBCOST_HDR
                                  on JOBTRANS.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                                  join JOBCOST_HDR1 in primeroUOW.JOBCOST_HDR
                                  on JOBTRANS.JOBNO equals JOBCOST_HDR1.JOBNO
                                  where JOBCOST_HDR2.JOBCODE == projectNumber
                                  select new { JOBTRANS.TRANSDATE };

            var jobTransactionsList = jobTransactions.Where(x => x.TRANSDATE != null).ToList();
            if (jobTransactionsList.Count > 0)
                return jobTransactionsList.Min(x => (DateTime)x.TRANSDATE);
            else
                return DateTime.Now;
        }

        public static List<ExoDataPoint> GetRevenue(IPrimeroEntitiesUnitOfWork primeroUOW, string projectNumber, DateTime dataDate, decimal currencyConversion = 1, bool showLoadingScreen = false)
        {
            ConcurrentBag<ExoDataPoint> revenueDataPoints = new ConcurrentBag<ExoDataPoint>();
            HashSet<string> missingSubJobNames = new HashSet<string>();

            if (showLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(1);
                LoadingScreenManager.SetMessage("Loading Revenue...");
            }

            var jobTransactions = from JOBTRANS in primeroUOW.JOB_TRANSACTIONS
                                  join JOBCOST_HDR in primeroUOW.JOBCOST_HDR
                                  on JOBTRANS.MASTER_JOBNO equals JOBCOST_HDR.JOBNO
                                  join JOBCOST_HDR1 in primeroUOW.JOBCOST_HDR
                                  on JOBTRANS.JOBNO equals JOBCOST_HDR1.JOBNO
                                  where JOBCOST_HDR.JOBCODE == projectNumber && JOBTRANS.LINE_STATUS != "X"
                                  select new { JOBCOST_HDR1.JOBCODE, JOBTRANS.QUANTITY, JOBTRANS.STOCKCODE, JOBTRANS.LINECOST, JOBTRANS.TRANSDATE, VARIATIONCODE = JOBTRANS.X_VARIATIONCODE, JOBTRANS.INVOICED, JOBTRANS.INVOICEDATE, JOBTRANS.INVSEQNO, JOBTRANS.EXCHRATE };

            var jobTransactionsList = jobTransactions.ToList();
            if (showLoadingScreen)
            {
                LoadingScreenManager.CloseLoadingScreen();
                LoadingScreenManager.ShowLoadingScreen(jobTransactionsList.Count);
                LoadingScreenManager.SetMessage("Loading Revenue...");
            }

            foreach (var jobTransaction in jobTransactionsList)
            {
                ExoDataPoint revenueDataPoint = new ExoDataPoint();
                revenueDataPoint.BudgetedUnits = 0;
                revenueDataPoint.BudgetedCosts = 0;
                revenueDataPoint.Units = (decimal)jobTransaction.QUANTITY;
                //burnedDataPoint.Costs = (decimal)jobTransaction.LINETOTAL * currencyConversion;
                currencyConversion = jobTransaction.EXCHRATE != null ? 1 / (decimal)jobTransaction.EXCHRATE : currencyConversion;

                revenueDataPoint.Costs = (decimal)jobTransaction.INVOICED * currencyConversion;
                revenueDataPoint.CostPerQty = revenueDataPoint.Units == 0 ? 0 : revenueDataPoint.Costs / revenueDataPoint.Units;
                //burnedDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobTransaction.TRANSDATE);
                revenueDataPoint.ActualDate = jobTransaction.TRANSDATE == null ? DateTime.Now : (DateTime)jobTransaction.TRANSDATE;
                revenueDataPoint.ProgressDate = revenueDataPoint.ActualDate;
                revenueDataPoint.Subjob_Name = jobTransaction.JOBCODE;
                revenueDataPoint.Quantity = (decimal)jobTransaction.QUANTITY;
                revenueDataPoint.StockCode = jobTransaction.STOCKCODE;
                revenueDataPoint.Variation_Code = BluePrintsDataUtils.normalizeVariationCode(jobTransaction.VARIATIONCODE);
                revenueDataPoint.InvoiceNo = jobTransaction.INVSEQNO.ToString();
                revenueDataPoint.InvoiceAmount = Convert.ToDecimal(jobTransaction.INVOICED) * currencyConversion;
                revenueDataPoint.InvoiceDate = jobTransaction.INVOICEDATE;

                revenueDataPoints.Add(revenueDataPoint);
                if (showLoadingScreen)
                    LoadingScreenManager.Progress();
            }

            return revenueDataPoints.ToList();
        }

        public static List<ExoDataPoint> GetRevenueLumpSum(IPrimeroEntitiesUnitOfWork primeroUOW, string projectNumber, DateTime dataDate, decimal currencyConversion = 1, bool showLoadingScreen = false)
        {
            ConcurrentBag<ExoDataPoint> revenueDataPoints = new ConcurrentBag<ExoDataPoint>();
            HashSet<string> missingSubJobNames = new HashSet<string>();

            if (showLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(1);
                LoadingScreenManager.SetMessage("Loading Revenue...");
            }

            var invoiceLines = from DR_INVLINES in primeroUOW.DR_INVLINES
                               join SUBJOB in primeroUOW.JOBCOST_HDR
                               on DR_INVLINES.JOBNO equals SUBJOB.JOBNO
                               join MASTERJOB in primeroUOW.JOBCOST_HDR
                               on SUBJOB.MASTER_JOBNO equals MASTERJOB.JOBNO
                               where MASTERJOB.JOBCODE == projectNumber
                               select new { MASTERJOB.JOBCODE, DR_INVLINES.QUANTITY, DR_INVLINES.STOCKCODE, DR_INVLINES.TRANSDATE, DR_INVLINES.LINETOTAL, DR_INVLINES.INVNO, DR_INVLINES.EXCHRATE };

            var invoiceLineList = invoiceLines.ToList();
            if (showLoadingScreen)
            {
                LoadingScreenManager.CloseLoadingScreen();
                LoadingScreenManager.ShowLoadingScreen(invoiceLineList.Count);
                LoadingScreenManager.SetMessage("Loading Revenue...");
            }

            foreach (var invoiceLine in invoiceLineList)
            {
                ExoDataPoint revenueDataPoint = new ExoDataPoint();
                revenueDataPoint.BudgetedUnits = 0;
                revenueDataPoint.BudgetedCosts = 0;
                revenueDataPoint.Units = (decimal)invoiceLine.QUANTITY;
                //burnedDataPoint.Costs = (decimal)jobTransaction.LINETOTAL * currencyConversion;
                currencyConversion = invoiceLine.EXCHRATE != null ? 1 / (decimal)invoiceLine.EXCHRATE : currencyConversion;

                revenueDataPoint.Costs = (decimal)invoiceLine.LINETOTAL * currencyConversion;
                revenueDataPoint.CostPerQty = revenueDataPoint.Units == 0 ? 0 : revenueDataPoint.Costs / revenueDataPoint.Units;
                //burnedDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobTransaction.TRANSDATE);
                revenueDataPoint.ActualDate = invoiceLine.TRANSDATE == null ? DateTime.Now : (DateTime)invoiceLine.TRANSDATE;
                revenueDataPoint.ProgressDate = revenueDataPoint.ActualDate;
                revenueDataPoint.Subjob_Name = invoiceLine.JOBCODE;
                revenueDataPoint.Quantity = (decimal)invoiceLine.QUANTITY;
                revenueDataPoint.StockCode = invoiceLine.STOCKCODE;
                //revenueDataPoint.Variation_Code = BluePrintsDataUtils.normalizeVariationCode(invoiceLine.VARIATIONCODE);
                revenueDataPoint.InvoiceNo = invoiceLine.INVNO.ToString();
                revenueDataPoint.InvoiceAmount = Convert.ToDecimal(invoiceLine.LINETOTAL) * currencyConversion;
                revenueDataPoint.InvoiceDate = invoiceLine.TRANSDATE;

                revenueDataPoints.Add(revenueDataPoint);
                if (showLoadingScreen)
                    LoadingScreenManager.Progress();
            }

            return revenueDataPoints.ToList();
        }

        public static List<ExoDataPoint> GetBurned(IPrimeroEntitiesUnitOfWork primeroUOW, string projectNumber, DateTime dataDate, IEnumerable<string> qualifiedSubjobs = null, List<SUBJOB> missingSUBJOBS = null, decimal currencyConversion = 1, bool showLoadingScreen = false)
        {
            List<ExoDataPoint> burnedDataPoints = new List<ExoDataPoint>();
            HashSet<string> missingSubJobNames = new HashSet<string>();

            primeroUOW.AutoDetectChangesEnabled(false);
            using (var t = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                var jobTransactions = from JOBTRANS in primeroUOW.JOB_TRANSACTIONS
                                      join JOBCOST_HDR2 in primeroUOW.JOBCOST_HDR
                                      on JOBTRANS.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                                      join JOBCOST_HDR1 in primeroUOW.JOBCOST_HDR
                                      on JOBTRANS.JOBNO equals JOBCOST_HDR1.JOBNO
                                      join JOBCOST_RESOURCE in primeroUOW.JOBCOST_RESOURCE
                                      on JOBTRANS.STAFFNO equals JOBCOST_RESOURCE.SEQNO
                                      join JOB_COSTGROUPS in primeroUOW.JOB_COSTGROUPS
                                      on JOBTRANS.COST_GROUP equals JOB_COSTGROUPS.SEQNO
                                      join JOB_COSTTYPES in primeroUOW.JOB_COSTTYPES
                                      on JOBTRANS.COST_TYPE equals JOB_COSTTYPES.SEQNO
                                      join NARRATIVES in primeroUOW.NARRATIVES
                                      on JOBTRANS.NARRATIVE_SEQNO equals NARRATIVES.SEQNO into PONarratives
                                      from PONarrate in PONarratives.DefaultIfEmpty()
                                      where JOBCOST_HDR2.JOBCODE == projectNumber && JOBTRANS.TRANSTYPE == "T" && JOBTRANS.LINE_STATUS != "X" && JOBTRANS.TRANSDATE <= dataDate
                                      select new { JOBCOST_HDR1.JOBCODE, JOBTRANS.EXCHRATE, JOBTRANS.QUANTITY, JOBTRANS.STOCKCODE, JOBTRANS.LINETOTAL, JOBTRANS.LINECOST, JOBTRANS.TRANSDATE, JOBCOST_RESOURCE.RESOURCENAME, JOBCOST_RESOURCE.TITLE, JOB_COSTGROUPS.COSTDESC, GROUP_SHORTCODE = JOB_COSTGROUPS.SHORTCODE, COSTDESC3 = JOB_COSTTYPES.COSTDESC, JOB_COSTTYPES.SHORTCODE, VARIATIONCODE = JOBTRANS.X_VARIATIONCODE, JOBTRANS.INVOICED, JOBTRANS.INVOICEDATE, JOBTRANS.INVSEQNO, PONarrate.NARRATIVE };

                var jobTransactionsList = jobTransactions.ToList();
                if (showLoadingScreen)
                {
                    LoadingScreenManager.ShowLoadingScreen(jobTransactionsList.Count());
                    LoadingScreenManager.SetMessage("Loading Actuals...");
                }

                foreach (var jobTransaction in jobTransactionsList)
                {
                    if (qualifiedSubjobs == null || qualifiedSubjobs.Contains(jobTransaction.JOBCODE))
                    {
                        if (qualifiedSubjobs == null || (jobTransaction.SHORTCODE != null && (!jobTransaction.SHORTCODE.Contains("G99") && !jobTransaction.SHORTCODE.Contains("010"))))
                        {
                            ExoDataPoint burnedDataPoint = new ExoDataPoint();
                            burnedDataPoint.BudgetedUnits = 0;
                            burnedDataPoint.BudgetedCosts = 0;
                            burnedDataPoint.Units = jobTransaction.QUANTITY == null ? 0 : (decimal)jobTransaction.QUANTITY;
                            //burnedDataPoint.Costs = (decimal)jobTransaction.LINETOTAL * currencyConversion;
                            burnedDataPoint.Costs = jobTransaction.LINECOST == null ? 0 : (decimal)jobTransaction.LINECOST * currencyConversion;
                            burnedDataPoint.CostPerQty = burnedDataPoint.Units == 0 ? 0 : burnedDataPoint.Costs / burnedDataPoint.Units;
                            //burnedDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobTransaction.TRANSDATE);
                            burnedDataPoint.ActualDate = jobTransaction.TRANSDATE == null ? DateTime.Now : (DateTime)jobTransaction.TRANSDATE;
                            burnedDataPoint.ProgressDate = burnedDataPoint.ActualDate;
                            burnedDataPoint.Subjob_Name = jobTransaction.JOBCODE;
                            burnedDataPoint.ResourceName = jobTransaction.RESOURCENAME;
                            burnedDataPoint.Description = jobTransaction.RESOURCENAME;
                            burnedDataPoint.Quantity = jobTransaction.QUANTITY == null ? 0 : (decimal)jobTransaction.QUANTITY;
                            burnedDataPoint.Role = jobTransaction.TITLE;
                            burnedDataPoint.CostGroup = jobTransaction.COSTDESC;
                            burnedDataPoint.Discipline_Code = jobTransaction.GROUP_SHORTCODE;
                            burnedDataPoint.CostType = jobTransaction.COSTDESC3;
                            burnedDataPoint.Commodity_Code = jobTransaction.SHORTCODE;
                            burnedDataPoint.StockCode = jobTransaction.STOCKCODE;
                            burnedDataPoint.Narrative = jobTransaction.NARRATIVE;
                            burnedDataPoint.Variation_Code = BluePrintsDataUtils.normalizeVariationCode(jobTransaction.VARIATIONCODE);
                            burnedDataPoint.InvoiceNo = jobTransaction.INVSEQNO.ToString();
                            burnedDataPoint.InvoiceAmount = Convert.ToDecimal(jobTransaction.INVOICED);
                            burnedDataPoint.InvoiceDate = jobTransaction.INVOICEDATE;

                            burnedDataPoints.Add(burnedDataPoint);
                        }
                    }
                    else
                        missingSubJobNames.Add(jobTransaction.JOBCODE);

                    if (showLoadingScreen)
                        LoadingScreenManager.Progress();
                }

                if (missingSUBJOBS != null)
                    foreach (string missingSubJobName in missingSubJobNames)
                    {
                        SUBJOB missingSUBJOB = new SUBJOB();
                        missingSUBJOB.INTERNAL_NAME1 = missingSubJobName;
                        missingSUBJOB.MissingQuantity = Convert.ToDecimal(jobTransactionsList.Where(x => x.JOBCODE == missingSubJobName && x.QUANTITY != null).Sum(x => x.QUANTITY));
                        missingSUBJOBS.Add(missingSUBJOB);
                    }

                if (showLoadingScreen)
                    LoadingScreenManager.CloseLoadingScreen();

                primeroUOW.AutoDetectChangesEnabled(true);
            }
            
            return burnedDataPoints;
        }

        public static List<ExoDataPoint> GetTimeByWBS(IPrimeroEntitiesUnitOfWork primeroUOW, string projectNumber, DateTime dataDate, IEnumerable<string> qualifiedSubjobs = null, List<SUBJOB> missingSUBJOBS = null, decimal currencyConversion = 1, bool showLoadingScreen = false)
        {
            List<ExoDataPoint> burnedDataPoints = new List<ExoDataPoint>();
            HashSet<string> missingSubJobNames = new HashSet<string>();

            primeroUOW.AutoDetectChangesEnabled(false);
            using (var t = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                List<X_TRANSACTION> timeLines = PrimeroEntities.GetTimeSummary(primeroUOW, projectNumber, dataDate);
                if (showLoadingScreen)
                {
                    LoadingScreenManager.ShowLoadingScreen(timeLines.Count());
                    LoadingScreenManager.SetMessage("Loading Actuals...");
                }

                foreach (var jobTransaction in timeLines)
                {
                    if (jobTransaction == null)
                        continue;

                    if (qualifiedSubjobs == null || qualifiedSubjobs.Contains(jobTransaction.SUB_JOBCODE))
                    {
                        if (qualifiedSubjobs == null || jobTransaction.COMMODITY_CODE != null && !jobTransaction.COMMODITY_CODE.Contains("G99") && !jobTransaction.COMMODITY_CODE.Contains("010"))
                        {
                            ExoDataPoint burnedDataPoint = new ExoDataPoint();
                            burnedDataPoint.BudgetedUnits = 0;
                            burnedDataPoint.BudgetedCosts = 0;
                            burnedDataPoint.Units = Convert.ToDecimal(jobTransaction.TOTAL_QUANTITY);
                            burnedDataPoint.Costs = Convert.ToDecimal(jobTransaction.TOTAL_COSTS);
                            burnedDataPoint.CostPerQty = burnedDataPoint.Units == 0 ? 0 : burnedDataPoint.Costs / burnedDataPoint.Units;
                            burnedDataPoint.ActualDate = jobTransaction.FIRST_WEEK_DATE;
                            burnedDataPoint.ProgressDate = burnedDataPoint.ActualDate;
                            burnedDataPoint.Subjob_Name = jobTransaction.SUB_JOBCODE;
                            burnedDataPoint.ResourceName = string.Empty;
                            burnedDataPoint.Description = string.Empty;
                            burnedDataPoint.Quantity = burnedDataPoint.Units;
                            burnedDataPoint.Role = string.Empty;
                            burnedDataPoint.CostGroup = jobTransaction.DISCIPLINE_CODE;
                            burnedDataPoint.Discipline_Code = jobTransaction.DISCIPLINE_CODE;
                            burnedDataPoint.CostType = jobTransaction.COMMODITY_CODE;
                            burnedDataPoint.Commodity_Code = jobTransaction.COMMODITY_CODE;
                            //stock code is not required for time since it indicates person booked to it
                            //burnedDataPoint.StockCode = jobTransaction.StockCode;
                            burnedDataPoint.Narrative = string.Empty;
                            burnedDataPoint.Variation_Code = jobTransaction.VARIATION_CODE;
                            burnedDataPoint.InvoiceNo = string.Empty;
                            burnedDataPoint.InvoiceAmount = 0;
                            burnedDataPoint.InvoiceDate = null;

                            burnedDataPoints.Add(burnedDataPoint);
                        }
                    }
                    else
                        missingSubJobNames.Add(jobTransaction.SUB_JOBCODE);

                    if (showLoadingScreen)
                        LoadingScreenManager.Progress();
                }

                if (missingSUBJOBS != null)
                    foreach (string missingSubJobName in missingSubJobNames)
                    {
                        SUBJOB missingSUBJOB = new SUBJOB();
                        missingSUBJOB.INTERNAL_NAME1 = missingSubJobName;
                        missingSUBJOB.MissingQuantity = Convert.ToDecimal(timeLines.Where(x => x.SUB_JOBCODE == missingSubJobName).Sum(x => x.TOTAL_QUANTITY));
                        missingSUBJOBS.Add(missingSUBJOB);
                    }

                if (showLoadingScreen)
                    LoadingScreenManager.CloseLoadingScreen();

                primeroUOW.AutoDetectChangesEnabled(true);
            }

            return burnedDataPoints;
        }

        public static List<ExoDataPoint> GetMaterials(IPrimeroEntitiesUnitOfWork primeroUOW, string projectNumber, DateTime dataDate, List<DateTime> alignedDataDates = null, decimal currencyConversion = 1, bool showLoadingScreen = false, ExoQueryType materialQueryType = ExoQueryType.All, bool groupByMonth = false)
        {
            List<ExoDataPoint> materialDataPoints = new List<ExoDataPoint>();
            primeroUOW.AutoDetectChangesEnabled(false);
            DateTime invoiceCutOffDate = dataDate.Date.AddDays(1).AddHours(-1);

            using (var t = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                var jobMaterials = from X_JOB_TRANSACTIONS_DETAIL in primeroUOW.X_JOB_TRANSACTIONS_DETAIL_V4
                                   where X_JOB_TRANSACTIONS_DETAIL.TRANSTYPE == "C" && X_JOB_TRANSACTIONS_DETAIL.MASTER_JOBCODE == projectNumber && X_JOB_TRANSACTIONS_DETAIL.TRANSDATE <= invoiceCutOffDate
                                   select X_JOB_TRANSACTIONS_DETAIL;

                if (showLoadingScreen)
                {
                    LoadingScreenManager.ShowLoadingScreen(jobMaterials.Count());
                    LoadingScreenManager.SetMessage("Loading Materials...");
                }

                string equipmentHireStockCodeInitials = BluePrintsResources.EquipmentHireStockCodeInitials;
                var jobMaterialsList = materialQueryType == ExoQueryType.All ? jobMaterials.ToList() : materialQueryType == ExoQueryType.EquipmentHireOnly ? jobMaterials.Where(x => x.STOCKCODE.StartsWith(equipmentHireStockCodeInitials)).ToList() : jobMaterials.Where(x => !x.STOCKCODE.StartsWith(equipmentHireStockCodeInitials)).ToList();

                foreach (var jobMaterial in jobMaterialsList)
                {
                    if (jobMaterial.DISCIPLINE_CODE != null && !jobMaterial.DISCIPLINE_CODE.Contains("G99") && !jobMaterial.DISCIPLINE_CODE.Contains("010"))
                    {
                        ExoDataPoint materialDataPoint = new ExoDataPoint();
                        materialDataPoint.BudgetedUnits = 0;
                        materialDataPoint.BudgetedCosts = 0;

                        decimal qty = jobMaterial.QUANTITY == null ? 0 : (decimal)jobMaterial.QUANTITY;
                        decimal lineCost = jobMaterial.LINECOST == null ? 0 : (decimal)jobMaterial.LINECOST;
                        materialDataPoint.Units = qty;
                        materialDataPoint.Costs = lineCost;
                        materialDataPoint.CostPerQty = jobMaterial.UNITPRICE == null ? 0 : (decimal)jobMaterial.UNITPRICE;

                        if (alignedDataDates != null)
                            materialDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobMaterial.TRANSDATE);

                        materialDataPoint.ActualDate = jobMaterial.TRANSDATE == null ? DateTime.Now : (DateTime)jobMaterial.TRANSDATE;
                        materialDataPoint.Subjob_Name = jobMaterial.SUB_JOBCODE;
                        materialDataPoint.ResourceName = jobMaterial.RESOURCE_NAME;
                        materialDataPoint.Quantity = qty;
                        materialDataPoint.Description = jobMaterial.DESCRIPTION;
                        materialDataPoint.Supplier = jobMaterial.SUPPLIER_NAME;
                        materialDataPoint.InvoiceNo = jobMaterial.INVNO == null ? string.Empty : jobMaterial.INVNO.ToString();
                        materialDataPoint.CostGroup = jobMaterial.DISCIPLINE_CODE.Trim();
                        materialDataPoint.Discipline_Code = jobMaterial.DISCIPLINE_CODE.Trim();
                        materialDataPoint.CostType = jobMaterial.COMMODITY_CODE;
                        materialDataPoint.Commodity_Code = jobMaterial.COMMODITY_CODE.Trim();
                        materialDataPoint.StockCode = jobMaterial.STOCKCODE.Trim();
                        materialDataPoint.Cost_GLName = jobMaterial.COST_GL_NAME;
                        materialDataPoint.Purchase_GLName = jobMaterial.PURCH_GL_NAME;
                        materialDataPoint.Variation_Code = normalizeVariationCode(jobMaterial.VARIATION_CODE);
                        materialDataPoint.InvoiceAmount = Convert.ToDecimal(jobMaterial.INVOICED);
                        materialDataPoint.InvoiceDate = jobMaterial.INVOICEDATE;
                        materialDataPoint.PONumber = jobMaterial.PO_NUMBER.ToString();

                        materialDataPoints.Add(materialDataPoint);
                    }

                    if (showLoadingScreen)
                        LoadingScreenManager.Progress();
                }

                if (showLoadingScreen)
                    LoadingScreenManager.CloseLoadingScreen();

                primeroUOW.AutoDetectChangesEnabled(true);
            }

            return materialDataPoints;
        }

        public static List<ExoDataPoint> GetMaterialsByWBS(IPrimeroEntitiesUnitOfWork primeroUOW, string projectNumber, DateTime dataDate, List<DateTime> alignedDataDates = null, decimal currencyConversion = 1, bool showLoadingScreen = false, ExoQueryType materialQueryType = ExoQueryType.All, bool groupByMonth = false)
        {
            List<ExoDataPoint> materialDataPoints = new List<ExoDataPoint>();
            primeroUOW.AutoDetectChangesEnabled(false);
            DateTime invoiceCutOffDate = dataDate.Date.AddDays(1).AddHours(-1);

            using (var t = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                List<X_TRANSACTION> materialLines = PrimeroEntities.GetMaterialSummary(primeroUOW, projectNumber, invoiceCutOffDate);
                if (showLoadingScreen)
                {
                    LoadingScreenManager.ShowLoadingScreen(materialLines.Count());
                    LoadingScreenManager.SetMessage("Loading Materials...");
                }

                string equipmentHireStockCodeInitials = BluePrintsResources.EquipmentHireStockCodeInitials;
                var jobMaterialsList = materialQueryType == ExoQueryType.All ? materialLines.ToList() : materialQueryType == ExoQueryType.EquipmentHireOnly ? materialLines.Where(x => x.STOCK_CODE.StartsWith(equipmentHireStockCodeInitials)).ToList() : materialLines.Where(x => !x.STOCK_CODE.StartsWith(equipmentHireStockCodeInitials)).ToList();

                foreach (var jobMaterial in jobMaterialsList)
                {
                    if (jobMaterial == null)
                        continue;

                    if (jobMaterial.DISCIPLINE_CODE != null && !jobMaterial.DISCIPLINE_CODE.Contains("G99") && !jobMaterial.DISCIPLINE_CODE.Contains("010"))
                    {
                        ExoDataPoint materialDataPoint = new ExoDataPoint();
                        materialDataPoint.BudgetedUnits = 0;
                        materialDataPoint.BudgetedCosts = 0;

                        decimal qty = Convert.ToDecimal(jobMaterial.TOTAL_QUANTITY);
                        decimal lineCost = Convert.ToDecimal(jobMaterial.TOTAL_COSTS);
                        materialDataPoint.Units = qty;
                        materialDataPoint.Costs = lineCost * currencyConversion;
                        materialDataPoint.CostPerQty = materialDataPoint.Units == 0 ? 0 : materialDataPoint.Costs / materialDataPoint.Units;

                        if (alignedDataDates != null)
                            materialDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobMaterial.FIRST_WEEK_DATE);

                        materialDataPoint.ActualDate = jobMaterial.FIRST_WEEK_DATE;
                        materialDataPoint.Subjob_Name = jobMaterial.SUB_JOBCODE;
                        materialDataPoint.ResourceName = string.Empty;
                        materialDataPoint.Quantity = qty;
                        materialDataPoint.Description = string.Empty;
                        materialDataPoint.Supplier = string.Empty;
                        materialDataPoint.InvoiceNo = string.Empty;
                        materialDataPoint.Discipline_Code = jobMaterial.DISCIPLINE_CODE;
                        materialDataPoint.Commodity_Code = jobMaterial.COMMODITY_CODE;
                        materialDataPoint.StockCode = jobMaterial.STOCK_CODE;
                        materialDataPoint.Cost_GLName = string.Empty;
                        materialDataPoint.Purchase_GLName = string.Empty;
                        materialDataPoint.Variation_Code = jobMaterial.VARIATION_CODE;
                        materialDataPoint.InvoiceAmount = 0;
                        materialDataPoint.InvoiceDate = null;
                        materialDataPoint.PONumber = string.Empty;

                        materialDataPoints.Add(materialDataPoint);
                    }

                    if (showLoadingScreen)
                        LoadingScreenManager.Progress();
                }

                if (showLoadingScreen)
                    LoadingScreenManager.CloseLoadingScreen();

                primeroUOW.AutoDetectChangesEnabled(true);
            }

            return materialDataPoints;
        }

        public static Version GetClickOncePublishVersion()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
                return ApplicationDeployment.CurrentDeployment.CurrentVersion;

            return null;
        }

        public static string normalizeVariationCode(string variationCode)
        {
            if (variationCode == null)
                return string.Empty;

            //cannot use this because subjob code isn't formatted to 6 characters, because user's don't use variation code as 6 characters sometimes
            //if (variationCode.Length >= 6)
            //    return variationCode.Substring(0, 6);

            //return string.Empty;
            return variationCode;
        }

        public static List<ExoDataPoint> GetEXOPO(IPrimeroEntitiesUnitOfWork primeroUOW, string projectNumber, DateTime queryDate, List<DateTime> alignedDataDates = null, bool showLoadingScreen = false, ExoQueryType exoQueryType = ExoQueryType.All)
        {
            List<ExoDataPoint> poDataPoints = new List<ExoDataPoint>();

            if (showLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(1);
                LoadingScreenManager.SetMessage("Loading POs...");
            }

            List<X_PURCHORD_LINE_DETAIL> X_PURCHORD_LINE_DETAILS = PrimeroEntities.GetPurchaseOrdersDetail(primeroUOW, projectNumber, queryDate);
            string equipmentHireStockCodeInitials = BluePrintsResources.EquipmentHireStockCodeInitials;
            X_PURCHORD_LINE_DETAILS = exoQueryType == ExoQueryType.All ? X_PURCHORD_LINE_DETAILS : exoQueryType == ExoQueryType.EquipmentHireOnly ? X_PURCHORD_LINE_DETAILS.Where(x => x.STOCKCODE.StartsWith(equipmentHireStockCodeInitials)).ToList() : X_PURCHORD_LINE_DETAILS.Where(x => !x.STOCKCODE.StartsWith(equipmentHireStockCodeInitials)).ToList();

            if (showLoadingScreen)
            {
                LoadingScreenManager.CloseLoadingScreen();
                LoadingScreenManager.ShowLoadingScreen(X_PURCHORD_LINE_DETAILS.Count);
                LoadingScreenManager.SetMessage("Loading POs...");
            }

            foreach(var po in X_PURCHORD_LINE_DETAILS)
            {
                ExoDataPoint poDataPoint = new ExoDataPoint();
                poDataPoint.BudgetedUnits = 0;
                poDataPoint.BudgetedCosts = 0;
                decimal orderQty = ((decimal)po.ORDER_QTY);

                decimal unitPrice = po.UNIT_PRICE == null ? 0 : ((decimal)po.UNIT_PRICE);
                poDataPoint.TotalUnits = orderQty;

                decimal remainingQty = Convert.ToDecimal(po.RemainingQty);
                poDataPoint.Units = remainingQty < 0 ? 0 : remainingQty;
                poDataPoint.Costs = po.OUTSTANDING_COSTS == null ? 0 : Convert.ToDecimal(po.OUTSTANDING_COSTS);
                poDataPoint.CostPerQty = unitPrice;
                poDataPoint.TotalCosts = po.LINETOTAL == null ? 0 : (decimal)po.LINETOTAL;
                if (alignedDataDates != null)
                    poDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= (DateTime)po.ORDERDATE);

                poDataPoint.ActualDate = po.ORDERDATE == null ? DateTime.Now : (DateTime)po.ORDERDATE;
                poDataPoint.PURCHORD_HDRLastUpdated = po.LAST_UPDATED;
                poDataPoint.Subjob_Name = po.SUB_JOBCODE.Trim();
                poDataPoint.ResourceName = string.Empty;
                poDataPoint.Quantity = poDataPoint.Units;
                poDataPoint.Description = po.DESCRIPTION;
                poDataPoint.Narrative = po.NARRATIVE;
                poDataPoint.Supplier = po.SUPPLIER_NAME;
                poDataPoint.InvoiceNo = string.Empty;
                poDataPoint.CostGroup = po.DISCIPLINE_CODE.Trim();
                poDataPoint.Discipline_Code = po.DISCIPLINE_CODE.Trim();
                poDataPoint.CostType = po.COMMODITY_CODE_DESC;
                poDataPoint.Commodity_Code = po.COMMODITY_CODE.Trim();
                poDataPoint.StockCode = po.STOCKCODE.Trim();
                poDataPoint.Cost_GLName = string.Empty;
                poDataPoint.Purchase_GLName = string.Empty;
                poDataPoint.IsPO = true;
                poDataPoint.PONumber = po.PO_NUMBER.ToString();
                poDataPoint.POOrderQty = Convert.ToDecimal((double)po.ORDER_QTY);
                poDataPoint.POSuppliedQty = Convert.ToDecimal((double)po.CUT_OFF_SUPPLIED);
                poDataPoint.Variation_Code = po.VARIATION_CODE;
                poDataPoints.Add(poDataPoint);

                if (showLoadingScreen)
                    LoadingScreenManager.Progress();
            }

            if (showLoadingScreen)
                LoadingScreenManager.CloseLoadingScreen();

            return poDataPoints.ToList();
        }

        public static List<ExoDataPoint> GetEXOPOSnapshot(IPrimeroEntitiesUnitOfWork primeroUOW, string projectNumber, DateTime queryDate, List<DateTime> alignedDataDates = null, bool showLoadingScreen = false, ExoQueryType exoQueryType = ExoQueryType.All)
        {
            List<ExoDataPoint> poDataPoints = new List<ExoDataPoint>();
            if (showLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(1);
                LoadingScreenManager.SetMessage("Loading POs...");
            }

            List<X_PURCHORD_LINE_DETAIL> X_PURCHORD_LINE_DETAILS = PrimeroEntities.GetPurchaseOrdersDetail(primeroUOW, projectNumber, queryDate);
            string equipmentHireStockCodeInitials = BluePrintsResources.EquipmentHireStockCodeInitials;
            X_PURCHORD_LINE_DETAILS = exoQueryType == ExoQueryType.All ? X_PURCHORD_LINE_DETAILS : exoQueryType == ExoQueryType.EquipmentHireOnly ? X_PURCHORD_LINE_DETAILS.Where(x => x.STOCKCODE.StartsWith(equipmentHireStockCodeInitials)).ToList() : X_PURCHORD_LINE_DETAILS.Where(x => !x.STOCKCODE.StartsWith(equipmentHireStockCodeInitials)).ToList();

            if (showLoadingScreen)
            {
                LoadingScreenManager.CloseLoadingScreen();
                LoadingScreenManager.ShowLoadingScreen(X_PURCHORD_LINE_DETAILS.Count);
                LoadingScreenManager.SetMessage("Loading POs...");
            }

            foreach (var po in X_PURCHORD_LINE_DETAILS)
            {
                ExoDataPoint poDataPoint = new ExoDataPoint();
                poDataPoint.BudgetedUnits = 0;
                poDataPoint.BudgetedCosts = 0;
                decimal orderQty = ((decimal)po.ORDER_QTY);

                decimal unitPrice = po.UNIT_PRICE == null ? 0 : ((decimal)po.UNIT_PRICE);
                poDataPoint.TotalUnits = orderQty;

                decimal remainingQty = Convert.ToDecimal(po.RemainingQty);
                poDataPoint.Units = remainingQty < 0 ? 0 : remainingQty;
                poDataPoint.Costs = po.OUTSTANDING_COSTS == null ? 0 : Convert.ToDecimal(po.OUTSTANDING_COSTS);
                poDataPoint.CostPerQty = unitPrice;
                poDataPoint.TotalCosts = po.LINETOTAL == null ? 0 : (decimal)po.LINETOTAL;
                if (alignedDataDates != null)
                    poDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= (DateTime)po.ORDERDATE);

                poDataPoint.ActualDate = po.ORDERDATE == null ? DateTime.Now : (DateTime)po.ORDERDATE;
                poDataPoint.PURCHORD_HDRLastUpdated = po.LAST_UPDATED;
                poDataPoint.Subjob_Name = po.SUB_JOBCODE.Trim();
                poDataPoint.ResourceName = string.Empty;
                poDataPoint.Quantity = poDataPoint.Units;
                poDataPoint.Description = po.DESCRIPTION;
                poDataPoint.Narrative = po.NARRATIVE;
                poDataPoint.Supplier = po.SUPPLIER_NAME;
                poDataPoint.InvoiceNo = string.Empty;
                poDataPoint.CostGroup = po.DISCIPLINE_CODE.Trim();
                poDataPoint.Discipline_Code = po.DISCIPLINE_CODE.Trim();
                poDataPoint.CostType = po.COMMODITY_CODE_DESC;
                poDataPoint.Commodity_Code = po.COMMODITY_CODE.Trim();
                poDataPoint.StockCode = po.STOCKCODE.Trim();
                poDataPoint.Cost_GLName = string.Empty;
                poDataPoint.Purchase_GLName = string.Empty;
                poDataPoint.IsPO = true;
                poDataPoint.PONumber = po.PO_NUMBER.ToString();
                poDataPoint.POOrderQty = Convert.ToDecimal((double)po.ORDER_QTY);
                poDataPoint.POSuppliedQty = Convert.ToDecimal((double)po.CUT_OFF_SUPPLIED);
                poDataPoint.Variation_Code = po.VARIATION_CODE;
                poDataPoints.Add(poDataPoint);

                if (showLoadingScreen)
                    LoadingScreenManager.Progress();
            }

            if (showLoadingScreen)
                LoadingScreenManager.CloseLoadingScreen();

            return poDataPoints.ToList();
        }

        public static void PopulateNewJobcostResourcesDefaults(JOBCOST_RESOURCE JOBCOST_RESOURCE)
        {
            JOBCOST_RESOURCE.COSTRATE0 = 0;
            JOBCOST_RESOURCE.COSTRATE1 = 0;
            JOBCOST_RESOURCE.COSTRATE2 = 0;
            JOBCOST_RESOURCE.COSTRATE3 = 0;
            JOBCOST_RESOURCE.SELLRATE0 = 0;
            JOBCOST_RESOURCE.SELLRATE1 = 0;
            JOBCOST_RESOURCE.SELLRATE2 = 0;
            JOBCOST_RESOURCE.SELLRATE3 = 0;
            JOBCOST_RESOURCE.NORMALHOURS = 0;
            JOBCOST_RESOURCE.ISACTIVE = "Y";
        }

        public static void PopulateNewStockItemsDefaults(STOCK_ITEMS STOCK_ITEM)
        {
            STOCK_ITEM.STOCKGROUP = 2;
            STOCK_ITEM.STATUS = "L";
            STOCK_ITEM.STDCOST = 0;
            STOCK_ITEM.SELLPRICE1 = 0;
            STOCK_ITEM.SELLPRICE2 = 0;
            STOCK_ITEM.SELLPRICE3 = 0;
            STOCK_ITEM.SELLPRICE4 = 0;
            STOCK_ITEM.SELLPRICE5 = 0;
            STOCK_ITEM.SELLPRICE6 = 0;
            STOCK_ITEM.SELLPRICE7 = 0;
            STOCK_ITEM.SELLPRICE8 = 0;
            STOCK_ITEM.SELLPRICE9 = 0;
            STOCK_ITEM.SELLPRICE10 = 0;
            STOCK_ITEM.LATESTCOST = 0;
            STOCK_ITEM.AVECOST = 0;
            STOCK_ITEM.MINSTOCK = 0;
            STOCK_ITEM.MAXSTOCK = 0;
            STOCK_ITEM.SUPPLIERNO = 0;
            STOCK_ITEM.MONTHUNITS = 0;
            STOCK_ITEM.YEARUNITS = 0;
            STOCK_ITEM.LASTYEARUNITS = 0;
            STOCK_ITEM.MONTHVALUE = 0;
            STOCK_ITEM.YEARVALUE = 0;
            STOCK_ITEM.LASTYEARVALUE = 0;
            STOCK_ITEM.DISCOUNTLEVEL = 0;
            STOCK_ITEM.DEFDAYS = 0;
            STOCK_ITEM.LASTMONTHVALUE = 0;
            STOCK_ITEM.LASTMONTHUNITS = 0;
            STOCK_ITEM.WEB_SHOW = "N";
            STOCK_ITEM.ISACTIVE = "Y";
            STOCK_ITEM.WEIGHT = 0;
            STOCK_ITEM.CUBIC = 0;
            STOCK_ITEM.PQTY = 1;
            STOCK_ITEM.HAS_SN = "N";
            STOCK_ITEM.SALES_GLSUBCODE = 0;
            STOCK_ITEM.PURCH_GLSUBCODE = 0;
            STOCK_ITEM.BRANCHNO = 0;
            STOCK_ITEM.SALESTAXRATE = -1;
            STOCK_ITEM.PURCHTAXRATE = -1;
            STOCK_ITEM.LAST_UPDATED = DateTime.Now;
            STOCK_ITEM.UPDATEITEM_QTY = 0;
            STOCK_ITEM.COS_GLSUBCODE = 0;
            STOCK_ITEM.STOCKPRICEGROUP = 0;
            STOCK_ITEM.SUPPLIERCOST = 0;
            STOCK_ITEM.ECONORDERQTY = 1;
            STOCK_ITEM.STOCK_CLASSIFICATION = 0;
            STOCK_ITEM.STOCKGROUP2 = 0;
            STOCK_ITEM.TOTALSTOCK = 0;
            STOCK_ITEM.HAS_BN = "N";
            STOCK_ITEM.HAS_EXPIRY = "N";
            STOCK_ITEM.EXPIRY_DAYS = 1;
            STOCK_ITEM.DUTY = 0;
            STOCK_ITEM.SERIALNO_TYPE = 0;
            STOCK_ITEM.LABEL_QTY = 1;
            STOCK_ITEM.IS_DISCOUNTABLE = "Y";
            STOCK_ITEM.RESTRICTED_ITEM = "N";
            STOCK_ITEM.NUMDECIMALS = -1;
            STOCK_ITEM.COGSMETHOD = 0;
            STOCK_ITEM.DEFAULTWARRANTYNO = -2;
            STOCK_ITEM.DIMENSIONS = 0;
            STOCK_ITEM.AUTO_NARRATIVE = 0;
            STOCK_ITEM.X_SIZEID = 0;
            STOCK_ITEM.X_COLOURID = 0;
            STOCK_ITEM.VARIABLECOST = "N";
            STOCK_ITEM.LOOKUP_RECOVERABLE = 'Y';
            STOCK_ITEM.X_PAYTYPE = 'H';
            STOCK_ITEM.X_ALLOWNO = 0;
        }

        public static List<ExoDataPoint> GetEXOPOByWBS(IPrimeroEntitiesUnitOfWork primeroUOW, string projectNumber, DateTime queryDate, List<DateTime> alignedDataDates = null, bool showLoadingScreen = false, ExoQueryType exoQueryType = ExoQueryType.All)
        {
            List<ExoDataPoint> poDataPoints = new List<ExoDataPoint>();

            if (showLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(1);
                LoadingScreenManager.SetMessage("Loading POs...");
            }

            DateTime poCutOffDate = queryDate.Date.AddDays(1).AddSeconds(-1);
            List<X_PURCHORD_LINE> purchaseOrderLines = PrimeroEntities.GetPurchaseOrdersSummary(primeroUOW, projectNumber, poCutOffDate);

            if (showLoadingScreen)
            {
                LoadingScreenManager.CloseLoadingScreen();
                LoadingScreenManager.ShowLoadingScreen(purchaseOrderLines.Count());
                LoadingScreenManager.SetMessage("Loading POs...");
            }

            foreach (var po in purchaseOrderLines)
            {
                if (po.DISCIPLINE_CODE != null && !po.DISCIPLINE_CODE.Contains("G99") && !po.DISCIPLINE_CODE.Contains("010"))
                {
                    ExoDataPoint poDataPoint = new ExoDataPoint();
                    poDataPoint.BudgetedUnits = 0;
                    poDataPoint.BudgetedCosts = 0;
                    decimal orderQty = Convert.ToDecimal(po.TOTAL_ORD_QUANT);
                    poDataPoint.TotalUnits = orderQty;

                    decimal remainingQty = orderQty - Convert.ToDecimal(po.TOTAL_SUP_QUANT);
                    poDataPoint.PONumber = po.PO_NUMBER.ToString();
                    poDataPoint.Units = remainingQty < 0 ? 0 : remainingQty;
                    poDataPoint.Costs = Convert.ToDecimal(po.TOTAL_OUTSTANDING_COSTS);
                    poDataPoint.CostPerQty = poDataPoint.Units == 0 ? 0 : (poDataPoint.Costs / poDataPoint.Units);
                    poDataPoint.TotalCosts = Convert.ToDecimal(po.TOTAL_COSTS);
                    poDataPoint.Subjob_Name = po.SUB_JOBCODE;
                    poDataPoint.Quantity = poDataPoint.Units;
                    poDataPoint.Discipline_Code = po.DISCIPLINE_CODE;
                    poDataPoint.Commodity_Code = po.COMMODITY_CODE;
                    poDataPoint.IsPO = true;
                    poDataPoint.POOrderQty = orderQty;
                    poDataPoint.POSuppliedQty = Convert.ToDecimal(po.TOTAL_SUP_QUANT);
                    poDataPoint.Variation_Code = normalizeVariationCode(po.VARIATION_CODE);
                    poDataPoints.Add(poDataPoint);
                }

                if (showLoadingScreen)
                    LoadingScreenManager.Progress();
            }

            if (showLoadingScreen)
                LoadingScreenManager.CloseLoadingScreen();

            return poDataPoints.ToList();
        }

        public static List<PURCHORD_LINES> GetAllNativeEXOPO(IPrimeroEntitiesUnitOfWork primeroUOW, IRepositoryQuery<PURCHORD_LINES> PURCHORD_LINESCollection, string projectNumber)
        {
            var pos = from PURCHORD_LINES in PURCHORD_LINESCollection
                      join PURCHORD_HDR in primeroUOW.PURCHORD_HDR
                      on PURCHORD_LINES.HDR_SEQNO equals PURCHORD_HDR.SEQNO
                      join JOBCOST_HDR in primeroUOW.JOBCOST_HDR
                      on PURCHORD_LINES.JOBNO equals JOBCOST_HDR.JOBNO
                      join JOBCOST_HDR2 in primeroUOW.JOBCOST_HDR
                      on JOBCOST_HDR.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                      join CR_ACCS in primeroUOW.CR_ACCS
                      on PURCHORD_HDR.ACCNO equals CR_ACCS.ACCNO
                      join NARRATIVES in primeroUOW.NARRATIVES
                      on PURCHORD_LINES.NARRATIVE_SEQNO equals NARRATIVES.SEQNO into PONarrativeTable
                      from PONarratives in PONarrativeTable.DefaultIfEmpty()
                      join JOB_COSTTYPES in primeroUOW.JOB_COSTTYPES
                      on PURCHORD_LINES.COSTTYPE equals JOB_COSTTYPES.SEQNO into CostTypeTable
                      from CostTypes in CostTypeTable.DefaultIfEmpty()
                      join JOB_COSTGROUPS in primeroUOW.JOB_COSTGROUPS
                      on PURCHORD_LINES.COSTGROUP equals JOB_COSTGROUPS.SEQNO into CostGroupTable
                      from CostGroups in CostGroupTable.DefaultIfEmpty()
                      where JOBCOST_HDR2.JOBCODE == projectNumber && PURCHORD_LINES.HDR_SEQNO != null
                      //select PURCHORD_LINES;
            select new { ExchangeRate = PURCHORD_HDR.EXCHRATE, OrderDate = PURCHORD_HDR.ORDERDATE, Narrative = PONarratives.NARRATIVE, Status = PURCHORD_HDR.STATUS, SupplierName = CR_ACCS.NAME, PurchaseOrderLine = PURCHORD_LINES, SubJob_Name = JOBCOST_HDR.JOBCODE, Discipline_Code = CostGroups.SHORTCODE, Commodity_Code = CostTypes.SHORTCODE };

            dynamic POQueryLines = pos.ToList();
            foreach(var po in POQueryLines)
            {
                po.PurchaseOrderLine.ExchangeRate = po.ExchangeRate == null ? 0 : (double)po.ExchangeRate;
                po.PurchaseOrderLine.OrderDate = po.OrderDate;
                po.PurchaseOrderLine.Narrative = po.Narrative;
                po.PurchaseOrderLine.Status = po.Status;
                po.PurchaseOrderLine.SupplierName = po.SupplierName;
                po.PurchaseOrderLine.Subjob_Name = po.SubJob_Name;
                po.PurchaseOrderLine.Discipline_Code = po.Discipline_Code;
                po.PurchaseOrderLine.Commodity_Code = po.Commodity_Code;
                po.PurchaseOrderLine.X_VARIATIONCODE = normalizeVariationCode(po.PurchaseOrderLine.X_VARIATIONCODE);
            }

            return pos.Select(x => x.PurchaseOrderLine).ToList();
        }

        public static bool GuidEquals<T>(T x, T y)
            where T : class, ICanSync, new()
        {
            return x.GUID == y.GUID;
        }

        public static decimal GetProductivity(decimal earnedUnits, decimal burnedUnits)
        {
            decimal defaultProductivity = decimal.Parse(BluePrintsResources.Default_Productivity);
            if (earnedUnits == 0 && burnedUnits == 0)
                return 1;
            else if (earnedUnits > 0 && burnedUnits == 0)
                return 1;
            else if (earnedUnits == 0 && burnedUnits > 0)
                return 1;
                //return defaultProductivity; //use only when user is ready for it
            else
                return 1;
                //return burnedUnits / earnedUnits; //use only when user is ready for it
        }

        /// <summary>
        /// Assign subjob to deliverables or estimation direct item before saving
        /// Optional parameter of phase type or charge type, otherwise use deliverables phase guid to generate subjob name
        /// </summary>
        /// <param name="entity"></param>
        public static void OnBeforeSavedGenerateAndAssignSubjob(PROJECT loadPROJECT, IEnumerable<PHASE> PHASECollection, IEnumerable<AREA> AREACollection, IEnumerable<AREA> SUBAREACollection, IDeliverable entity, IBluePrintsEntitiesUnitOfWork bluePrintsUnitOfWork, PhaseType? PhaseType = null, ChargeType? ChargeType = null, bool isProcurementSubjob = false, bool forceIgnore = false)
        {
            //when user wish to override default subjob
            if (forceIgnore)
                return;

            Guid? existingOrNewPhaseGuid;
            IHaveProcurementSubjob iHaveProcurementSubjobEntity = entity as IHaveProcurementSubjob;
            bool assignToProcurementSubjob = (isProcurementSubjob && iHaveProcurementSubjobEntity != null);

            string internalNumber = BluePrintsDataUtils.SUBJOB_Generate_InternalNumber(entity.Area_Guid, entity.SubArea_Guid, loadPROJECT, AREACollection, SUBAREACollection, out existingOrNewPhaseGuid, entity.Phase_Guid, PHASECollection, PhaseType, ChargeType);
            IEnumerable<SUBJOB> SUBJOBCollection = bluePrintsUnitOfWork.SUBJOBS.Where(x => x.GUID_PROJECT == loadPROJECT.GUID);
            ////provision for when subjob is manually assigned or using legacy subjob
            if (entity.Subjob_Guid != null)
            {
                SUBJOB subjob = SUBJOBCollection.FirstOrDefault(x => x.GUID == entity.Subjob_Guid);
                if (subjob != null && subjob.INTERNAL_NAME1 == internalNumber)
                    return;
            }

            if (internalNumber != string.Empty)
            {
                SUBJOB existingSUBJOB = SUBJOBCollection.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).FirstOrDefault(x => x.INTERNAL_NAME1 == internalNumber);
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

                    bluePrintsUnitOfWork.SUBJOBS.Add(newSUBJOB);
                    bluePrintsUnitOfWork.SaveChanges();
                    Messenger.Default.Send(new EntityMessage<SUBJOB, Guid>(newSUBJOB.GUID, Guid.NewGuid(), EntityMessageType.Added));

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

        public static bool FuncManualCellPastingIsContinue(BASELINE_ITEMProgress projection, ColumnBase column, string pasteValue, List<UndoRedoArg<BASELINE_ITEMProgress>> undoRedoArgs)
        {
            bool isProgress = DataUtils.FormatColumnFieldname(column.FieldName) == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().DeliverableStatusProgressGuid);

            if (isProgress || DataUtils.FormatColumnFieldname(column.FieldName) == BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Entity.Entity.DeliverableStatusGuid))
            {
                object oldValue = projection.Entity.Entity.GUID_STATUS;
                if (projection.Entity.Entity.SetDeliverableStatusByName(pasteValue))
                {
                    object newValue = projection.Entity.Entity.GUID_STATUS;
                    if (undoRedoArgs != null)
                        undoRedoArgs.Add(new UndoRedoArg<BASELINE_ITEMProgress>() { FieldName = column.FieldName, Projection = projection, OldValue = oldValue, NewValue = newValue });

                    if(isProgress)
                    {
                        DELIVERABLES_STATUS currentDELIVERABLE_STATUS = projection.Entity.Entity.DeliverableStatusCollection.FirstOrDefault(x => x.GUID == projection.Entity.Entity.GUID_STATUS);
                        if(currentDELIVERABLE_STATUS != null && currentDELIVERABLE_STATUS.AUTO_PERCENTAGE != null)
                        {
                            decimal oldTotalPercentage = projection.Total_Percentage;
                            decimal auto_percentage = (decimal)currentDELIVERABLE_STATUS.AUTO_PERCENTAGE;
                            if (auto_percentage > projection.Total_Percentage)
                            {
                                projection.Total_Earned_Percentage = auto_percentage;

                                if (undoRedoArgs != null)
                                    undoRedoArgs.Add(new UndoRedoArg<BASELINE_ITEMProgress>() { FieldName = BindableBase.GetPropertyName(() => new BASELINE_ITEMProgress().Total_Earned_Percentage), Projection = projection, OldValue = oldTotalPercentage, NewValue = auto_percentage });
                            }
                        }
                    }
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Assign workpack to deliverables or estimation direct item before saving
        /// Optional parameter of phase type or charge type, otherwise use deliverables phase guid to generate workpack name
        /// </summary>
        /// <param name="entity"></param>
        public static void OnBeforeSavedGenerateAndAssignWorkpack(PROJECT loadPROJECT, IDeliverable entity, CollectionViewModel<WORKPACK, WORKPACK, Guid, IBluePrintsEntitiesUnitOfWork> WORKPACKCollectionViewModel, IEnumerable<SUBJOB> SUBJOBCollection, IEnumerable<DISCIPLINE> DISCIPLINECollection, bool forceIgnore = false)
        {
            if (!loadPROJECT.USE_WORKPACKS)
                return;

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

                List<string> insertSelectedEntitiesSimilarNames = insertSelectedEntities.Where(x => x.EntityNumber != null && x.EntityNumber.Contains(valueToFillStringOnly)).Select(x => x.EntityNumber).ToList();

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

                        if (!isExistsInAll)
                            return nextName;
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

                var internalNameCount = BASELINE_ITEMEntities.Where(x => x.GUID != excludeGUID).Count(x => x.INTERNAL_NUM != null && x.INTERNAL_NUM.Contains(InternalNum));
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

        public static int GetTenderDuration(PROJECT project)
        {
            decimal tenderDuration = project.TENDER_PROJECT_DURATION == null ? 0 : (decimal)project.TENDER_PROJECT_DURATION;
            int totalDurationInDays = Convert.ToInt32(tenderDuration * 7);

            return totalDurationInDays;
        }

        public static string GetPhaseCode(string subjobCode)
        {
            if (subjobCode == string.Empty || subjobCode == null)
                return string.Empty;
            else if (subjobCode.Length < 15)
                return string.Empty;

            return subjobCode.Substring(13, 2);
        }

        public static string GetAreaCode(string subJobcode)
        {
            if (subJobcode == string.Empty)
                return string.Empty;
            else if (subJobcode.Length < 15)
                return string.Empty;

            return subJobcode.Substring(6, 3);
        }

        public static string GetSubAreaCode(string subJobcode)
        {
            if (subJobcode == string.Empty)
                return string.Empty;
            else if (subJobcode.Length < 15)
                return string.Empty;

            return subJobcode.Substring(10, 2);
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

        public static void CreateProgressBackup(PROGRESS selectedPROGRESS, string backupPrefix = "")
        {
            IUnitOfWorkFactory<IBluePrintsEntitiesUnitOfWork> bluePrintsUnitOfWorkFactory = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory();
            IBluePrintsEntitiesUnitOfWork bluePrintsUOW = bluePrintsUnitOfWorkFactory.CreateUnitOfWork();

            if (selectedPROGRESS == null)
                return;

            string backupPrefixStr = backupPrefix == string.Empty ? "BACKUP " : backupPrefix + " ";
            PROGRESS backupPROGRESS = new PROGRESS();
            DataUtils.ShallowCopy(backupPROGRESS, selectedPROGRESS);
            backupPROGRESS.GUID = Guid.Empty;
            backupPROGRESS.NAME = backupPrefixStr + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToShortTimeString();
            backupPROGRESS.STATUS = ProgressStatus.Superseded;
            bluePrintsUOW.PROGRESSES.Add(backupPROGRESS);
            //need to save progress to get GUID
            bluePrintsUOW.SaveChanges();

            if (selectedPROGRESS.PROGRESS_ITEM != null)
            {
                foreach (PROGRESS_ITEM progress_item in selectedPROGRESS.PROGRESS_ITEM)
                {
                    PROGRESS_ITEM newPROGRESS_ITEM = new PROGRESS_ITEM();
                    DataUtils.ShallowCopy(newPROGRESS_ITEM, progress_item);
                    newPROGRESS_ITEM.GUID = Guid.Empty;
                    newPROGRESS_ITEM.GUID_PROGRESS = backupPROGRESS.GUID;
                    bluePrintsUOW.PROGRESS_ITEMS.Add(newPROGRESS_ITEM);
                }
            }

            if (selectedPROGRESS.PROGRESS_ETC != null)
            {
                foreach (PROGRESS_ETC progressETC in selectedPROGRESS.PROGRESS_ETC)
                {
                    PROGRESS_ETC newPROGRESS_ETC = new PROGRESS_ETC();
                    DataUtils.ShallowCopy(newPROGRESS_ETC, progressETC);
                    newPROGRESS_ETC.GUID = Guid.Empty;
                    newPROGRESS_ETC.GUID_PROGRESS = backupPROGRESS.GUID;
                    bluePrintsUOW.PROGRESS_ETCS.Add(newPROGRESS_ETC);
                }
            }

            bluePrintsUOW.SaveChanges();
        }

        /// <summary>
        /// Searches rate cascadingly for IRATE interface
        /// </summary>
        /// <returns></returns>
        public static RATE CascadeRateSearch(Guid? areaGuid, Guid? subAreaGuid, Guid? disciplineGuid, int? disciplineNum, Guid? departmentGuid, string commodityCode, string variationCode, IEnumerable<RATE> RATECollection, CostType costType, PhaseType phaseType)
        {
            IEnumerable<RATE> rateByPhase = RATECollection.Where(y => y.COST_TYPE == costType && y.Phase_Type == phaseType);
            //order by descending places null GUID's at the end, so First() won't pick it up
            IEnumerable<RATE> rateByVariations = rateByPhase.Where(y => (y.VARIATION_CODE == variationCode) || (y.VARIATION_CODE == string.Empty || y.VARIATION_CODE == null)).OrderByDescending(y => y.COMMODITY_CODE);
            IEnumerable<RATE> rateByCommodities = rateByVariations.Where(y => (y.COMMODITY_CODE == commodityCode) || (y.COMMODITY_CODE == string.Empty || y.COMMODITY_CODE == null)).OrderByDescending(y => y.COMMODITY_CODE);
            IEnumerable<RATE> rateByDiscipline = rateByCommodities.Where(y => (y.GUID_DISCIPLINE == disciplineGuid) || (y.GUID_DISCIPLINE == null)).OrderByDescending(y => y.GUID_DISCIPLINE);
            IEnumerable<RATE> rateByDisciplineNum = rateByDiscipline.Where(y => (y.DISCIPLINE_NUM == disciplineNum) || (y.DISCIPLINE_NUM == null)).OrderByDescending(y => y.DISCIPLINE_NUM);
            IEnumerable<RATE> rateByDepartment = rateByDisciplineNum.Where(y => (y.GUID_DEPARTMENT == departmentGuid) || (y.GUID_DEPARTMENT == null)).OrderByDescending(y => y.GUID_DEPARTMENT);
            IEnumerable<RATE> rateBySubArea = rateByDepartment.Where(y => (y.GUID_SUBAREA == subAreaGuid) || (y.GUID_SUBAREA == null)).OrderByDescending(y => y.GUID_SUBAREA);
            IEnumerable<RATE> rateByArea = rateBySubArea.Where(y => (y.GUID_AREA == areaGuid) || (y.GUID_AREA == null)).OrderByDescending(y => y.GUID_AREA);

            return rateByArea.FirstOrDefault();
        }

        /// <summary>
        /// Searches rate cascadingly for IRATE interface
        /// </summary>
        /// <returns></returns>
        public static RATE CascadeRateSearchByCode(string areaCode, string subAreaCode, string disciplineCode, string departmentCode, string commodityCode, string variationCode, IEnumerable<RATE> RATECollection, CostType costType, PhaseType phaseType)
        {
            IEnumerable<RATE> rateByPhase = RATECollection.Where(y => y.COST_TYPE == costType && y.Phase_Type == phaseType);
            //order by descending places null GUID's at the end, so First() won't pick it up
            IEnumerable<RATE> rateByVariations = rateByPhase.Where(y => (y.VARIATION_CODE == variationCode) || (y.VARIATION_CODE == string.Empty || y.VARIATION_CODE == null)).OrderByDescending(y => y.COMMODITY_CODE);
            IEnumerable<RATE> rateByCommodities = rateByVariations.Where(y => (y.COMMODITY_CODE == commodityCode) || (y.COMMODITY_CODE == string.Empty || y.COMMODITY_CODE == null)).OrderByDescending(y => y.COMMODITY_CODE);
            IEnumerable<RATE> rateByDiscipline = rateByCommodities.Where(y => (y.DISCIPLINE != null && y.DISCIPLINE.CODE == disciplineCode) || (y.GUID_DISCIPLINE == null)).OrderByDescending(y => y.GUID_DISCIPLINE);
            IEnumerable<RATE> rateByDepartment = rateByDiscipline.Where(y => (y.DEPARTMENT != null && y.DEPARTMENT.CODE == departmentCode) || (y.GUID_DEPARTMENT == null)).OrderByDescending(y => y.GUID_DEPARTMENT);
            IEnumerable<RATE> rateBySubArea = rateByDepartment.Where(y => (y.SUBAREA != null && y.SUBAREA.INTERNAL_NUM == subAreaCode) || (y.GUID_SUBAREA == null)).OrderByDescending(y => y.GUID_SUBAREA);
            IEnumerable<RATE> rateByArea = rateBySubArea.Where(y => (y.AREA != null && y.AREA.INTERNAL_NUM == areaCode) || (y.GUID_AREA == null)).OrderByDescending(y => y.GUID_AREA);

            return rateByArea.FirstOrDefault();
        }

        public static IEnumerable<COMMODITY_CODE> FilterForValidCommodityCodes(IEnumerable<COMMODITY_CODE> COMMODITY_CODES, string fullDisciplineCode, PhaseType? phaseType = null)
        {
            if (COMMODITY_CODES == null || fullDisciplineCode.Length < 2)
                return new List<COMMODITY_CODE>();

            List<COMMODITY_CODE> validCommodityCodes;
            string disciplineCode = fullDisciplineCode.Substring(0, 2);
            if (phaseType == null)
            {
                validCommodityCodes = COMMODITY_CODES.Where(x => x.DISCIPLINE == null || (x.DISCIPLINE.CODE.Length >= 2 && x.DISCIPLINE.CODE.Substring(0, 2) == disciplineCode)).OrderBy(x => x.CODE).ToList();
            }
            else if (phaseType == PhaseType.Tender)
                validCommodityCodes = COMMODITY_CODES.Where(x => x.DISCIPLINE == null || (x.DISCIPLINE.CODE.Length >= 2 && x.DISCIPLINE.CODE.Substring(0, 2) == BluePrintsResources.Default_TenderDisciplineCode)).OrderBy(x => x.CODE).ToList();
            else
            {
                IEnumerable<COMMODITY_CODE> phaseCommodityCodes;
                //if (phaseType == Common.PhaseType.Design)
                //    //because design deliverable's have indirect components also
                //    phaseCommodityCodes = COMMODITY_CODES.Where(x => x.PHASE_TYPE == Common.PhaseType.Design || x.PHASE_TYPE == Common.PhaseType.Indirect);
                //else
                phaseCommodityCodes = COMMODITY_CODES.Where(x => x.PHASE_TYPE == phaseType);

                validCommodityCodes = phaseCommodityCodes.Where(x => (x.DISCIPLINE == null || (x.DISCIPLINE.CODE.Length >= 2 && x.DISCIPLINE.CODE.Substring(0, 2) == disciplineCode))).OrderBy(x => x.CODE).ToList();
            }

            return validCommodityCodes;
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

            if (assignedPHASEKey != null)
                phase = PHASECollection.FirstOrDefault(x => x.GUID == assignedPHASEKey);
            else if(PhaseType != null || ChargeType != null)
            {
                if (ChargeType == null)
                    phase = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE == PhaseType);
                else
                    phase = PHASECollection.FirstOrDefault(x => x.PHASE_TYPE == PhaseType && x.CHARGE_TYPE == ChargeType);
            }
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

        public static BenchmarkTarget GetBenchmarkTargetFromNullableBool(bool? benchmarkBool)
        {
            if (benchmarkBool == null)
                return BenchmarkTarget.Current;
            else if ((bool)benchmarkBool)
                return BenchmarkTarget.Budget;
            else
                return BenchmarkTarget.Current;
        }

        public static void SetReportingDataPointBenchmark(BenchmarkTarget benchmarkTarget, Stats stats)
        {
            if (stats != null)
            {
                if (stats.CurrentPeriodCumulativeDataPoint != null)
                    stats.CurrentPeriodCumulativeDataPoint.IsReportBudgetPercentage = benchmarkTarget == BenchmarkTarget.Budget;

                if (stats.CumulativeDataPoints != null)
                    foreach (Common.ViewModel.Reporting.DataPoint dataPoint in stats.CumulativeDataPoints)
                        dataPoint.IsReportBudgetPercentage = benchmarkTarget == BenchmarkTarget.Budget;
            }
        }

        public static void UpdatePercentagesByStatus(IMessageBoxService MessageBoxService, CollectionViewModel<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESS_ITEMSCollectionViewModel, IEnumerable<BASELINE_ITEMProgress> entities)
        {
            if (entities == null)
                return;

            IEnumerable<BASELINE_ITEMProgress> deliverables = entities.Where(x => x.Entity.Entity.GUID_STATUS != null);
            List<PROGRESS_ITEM> updateProgress = new List<PROGRESS_ITEM>();

            foreach (var deliverable in deliverables)
            {
                if (deliverable.Entity.Entity.DOCTYPE == null)
                    continue;

                DELIVERABLES_STATUS deliverableStatus = deliverable.Entity.Deliverable_Status;

                //when this is null it means the deliverable status is no longer valid (e.g. deleted)
                if (deliverableStatus == null)
                    continue;

                //user are able to fill up/down on statuses that might result in assigned status isn't valid to doctype, so check if status is valid before continuing
                bool isValidStatus = deliverable.Entity.Entity.IsDeliverableStatusValid(deliverableStatus.GUID);
                if (!isValidStatus)
                    continue;

                decimal? autoPercentage = deliverableStatus.AUTO_PERCENTAGE;
                if (autoPercentage != null)
                {
                    if (deliverable.Total_Percentage < autoPercentage)
                    {
                        decimal newPercentage = (decimal)autoPercentage;

                        deliverable.Total_Earned_Percentage = newPercentage;
                        IEnumerable<PROGRESS_ITEM> newPRORESS_ITEMS = deliverable.GetExistingOrNewEditedProgresses(PROGRESS_ITEMSCollectionViewModel.FindActualProjectionByExpression);
                        updateProgress.AddRange(newPRORESS_ITEMS);
                    }
                }

                if (deliverable.Total_Percentage > deliverableStatus.MAX_PERCENTAGE)
                {
                    decimal totalDeliverableUnits = deliverable.Total_Units;
                    decimal maxAllowableEarnedUnit = totalDeliverableUnits * deliverableStatus.MAX_PERCENTAGE;
                    if (maxAllowableEarnedUnit > 0)
                    {
                        decimal iterateEarnedUnits = 0;
                        List<PROGRESS_ITEM> progressesByDate = deliverable.PROGRESS_ITEMS.OrderBy(x => x.EARNED_DATE).ToList();
                        foreach (PROGRESS_ITEM progressByDate in progressesByDate)
                        {
                            decimal postProgressEarnedUnit = (iterateEarnedUnits + progressByDate.EarnedUnits);
                            decimal oldProgressEarnUnit = progressByDate.EarnedUnits;
                            if (postProgressEarnedUnit > maxAllowableEarnedUnit)
                            {
                                decimal newProgressEarnUnit = (maxAllowableEarnedUnit - iterateEarnedUnits);
                                progressByDate.EarnedUnits = newProgressEarnUnit < 0 ? 0 : newProgressEarnUnit;
                                updateProgress.Add(progressByDate);
                            }

                            iterateEarnedUnits += oldProgressEarnUnit;
                        }
                    }
                }

            }

            PROGRESS_ITEMSCollectionViewModel.BaseBulkSave(updateProgress);
        }

        public static void SaveUserPreference(string preferenceName, string preferenceValue)
        {
            if (LoginCredentials.IsAdmin)
                return;

            if (LoginCredentials.CurrentUser == null)
                return;

            IBluePrintsEntitiesUnitOfWork bluePrintsEntitiesUnitOfWork = BluePrintsEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            USER_PREFERENCE dbCurrentUserPreference = bluePrintsEntitiesUnitOfWork.USER_PREFERENCES.FirstOrDefault(x => x.GUID_USER == LoginCredentials.CurrentUserGuid && x.PREFERENCE_NAME == preferenceName);
            if (dbCurrentUserPreference != null)
                dbCurrentUserPreference.PREFERENCE_VALUE = preferenceValue;
            else
            {
                dbCurrentUserPreference = new USER_PREFERENCE();
                dbCurrentUserPreference.PREFERENCE_NAME = preferenceName;
                dbCurrentUserPreference.PREFERENCE_VALUE = preferenceValue;
                dbCurrentUserPreference.GUID_USER = LoginCredentials.CurrentUserGuid;
                bluePrintsEntitiesUnitOfWork.USER_PREFERENCES.Add(dbCurrentUserPreference);
            }
            bluePrintsEntitiesUnitOfWork.SaveChanges();

            USER_PREFERENCE currentUserPreference = LoginCredentials.CurrentUser.UserPreferences.FirstOrDefault(x => x.PREFERENCE_NAME == preferenceName);
            if (currentUserPreference != null)
                currentUserPreference.PREFERENCE_VALUE = preferenceValue;
            else
                LoginCredentials.CurrentUser.UserPreferences.Add(dbCurrentUserPreference);
        }

        public static void SaveDateAndRefresh(PROJECT project, DateTime? LoadDataDate, ref DateTime? ChangedStartDataDate, DateTime endDate, IEnumerable<FORECAST_EAC> FORECAST_EACCollection, CollectionViewModel<PROJECT, PROJECT, Guid, IBluePrintsEntitiesUnitOfWork> PROJECTCollectionViewModel, IMessageBoxService messageBoxService)
        {
            if (ChangedStartDataDate != LoadDataDate)
            {
                if (ChangedStartDataDate < LoadDataDate)
                {
                    if (FORECAST_EACCollection.Count() > 0)
                    {
                        DateTime lastEACDataDate = FORECAST_EACCollection.Max(x => x.FORECAST_DATE);
                        if (ChangedStartDataDate < lastEACDataDate)
                        {
                            if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_Forecast_MoveDataDate)) == LoginCredentials.PermissionStatus.None)
                            {
                                messageBoxService.ShowMessage("Cannot move data date backwards because EAC is finalised for " + ((DateTime)lastEACDataDate).ToShortDateString(), "Error", MessageButton.OK, MessageIcon.Exclamation);
                                ChangedStartDataDate = LoadDataDate;
                                return;
                            }
                        }

                    }
                }
                //restrict user from moving data date forward if there are forecast but EAC isn't saved
                else if (ChangedStartDataDate > LoadDataDate)
                {
                    bool hasEACOnCurrentDataDate = FORECAST_EACCollection.Where(x => x.FORECAST_DATE == LoadDataDate).Count() > 0;
                    if (LoadDataDate != null && !hasEACOnCurrentDataDate)
                    {
                        if (LoginCredentials.getPermissionStatus(DataUtils.GetNameOf(() => NavigationResources.Permission_Forecast_MoveDataDate)) == LoginCredentials.PermissionStatus.None)
                        {
                            messageBoxService.ShowMessage("Cannot move data date forward because EAC isn't saved for " + ((DateTime)LoadDataDate).ToShortDateString(), "Error", MessageButton.OK, MessageIcon.Exclamation);
                            ChangedStartDataDate = LoadDataDate;
                            return;
                        }
                    }
                }
            }

            DateTime saveDateTime = new DateTime(((DateTime)ChangedStartDataDate).Year, ((DateTime)ChangedStartDataDate).Month, 1).AddMonths(1).AddDays(-1);
            project.FORECAST_END_DATE = endDate;
            project.FORECAST_DATA_DATE = saveDateTime;
            PROJECTCollectionViewModel.Save(project);
            ChangedStartDataDate = saveDateTime;
            LoadDataDate = saveDateTime;
        }

        public static string GetPhaseCodeFromSubJobCode(string SubJobCode)
        {
            if (SubJobCode == null || SubJobCode == string.Empty)
                return string.Empty;

            List<string> codePartition = SubJobCode.Split('-').ToList();
            if (codePartition.Count < 4)
                return string.Empty;

            return codePartition[3];
        }

        public static string GetPreferredDocumentTypeName(string preferenceName)
        {
            string viewName = string.Empty;
            if(preferenceName == DataUtils.GetNameOf(() => UserPreferences.EXO_PreloadTransactions))
            {
                viewName = "TransactionCollectionInstantFeedbackView";
                bool? isUsePreloadModePreference = LoginCredentials.GetUserPreferenceBool(preferenceName);
                if (isUsePreloadModePreference != null && (bool)isUsePreloadModePreference)
                    viewName = "TransactionCollectionView";
            }

            return viewName;
        }
    }
}