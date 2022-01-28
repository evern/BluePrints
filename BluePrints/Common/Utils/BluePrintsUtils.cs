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

namespace BluePrints.Common.Utils
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
                if (loadPROGRESS.INTERVAL_TYPE == ProgressIntervalType.Daily)
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
                else if (isReportDate && !loadPROGRESS.DISABLE_AUTO_REPORT_DATE)
                {
                    //rewind the data one week when progress is updated for the current week but reporting is done on the previous week
                    //will be saved when data date is saved
                    if (loadPROGRESS.REPORT_DATE == null)
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
                            if (!loadPROGRESS.DISABLE_AUTO_REPORT_DATE)
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

                if (isReportDate)
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
            if (exoAuthorisations == null)
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
            if (projections != null)
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
            if (subJob != null && subJob.JobStatus.ToUpper() == "G")
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

}
