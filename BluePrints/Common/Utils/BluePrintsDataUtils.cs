using BaseModel.Data.Helpers;
using BaseModel.DataModel;
using BaseModel.Helpers;
using BaseModel.Misc;
using BaseModel.ViewModel.Base;
using BaseModel.ViewModel.Dialogs;
using BaseModel.ViewModel.Services;
using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common;
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
using System.Deployment.Application;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Utils
{
    public static class BluePrintsUtils
    {/// <summary>
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
        /// Change the progress data date
        /// </summary>
        /// <param name="navigationType">Forward, backward or last week ending</param>
        /// <param name="loadPROGRESS">Progress to change</param>
        /// <returns>Whether should save</returns>
        public static bool ProgressDateChange(DateNavigationType navigationType, PROGRESS loadPROGRESS)
        {
            var interval = ChronologicalHelpers.ConvertProgressIntervalToPeriod(loadPROGRESS);
            int multiplier;
            if (navigationType == DateNavigationType.Current)
            {
                DateTime oneWeekAgo = DateTime.Now.AddDays(-1 * interval.Days);
                if (loadPROGRESS.DATA_DATE < oneWeekAgo)
                {
                    do
                    {
                        loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(1 * interval.Days);
                    } while (loadPROGRESS.DATA_DATE < oneWeekAgo);

                    return true;
                }
                else if (loadPROGRESS.DATA_DATE > DateTime.Now)
                {
                    do
                    {
                        loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(-1 * interval.Days);
                    } while (loadPROGRESS.DATA_DATE > DateTime.Now);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                multiplier = navigationType == DateNavigationType.Forward ? 1 : -1;
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
            List<ExoTimeAuthorisation> cacheExoAuthorisations = new List<ExoTimeAuthorisation>();
            foreach (var projectContext in projectContexts)
            {
                int? firstFoundStaffNo = null;
                if(useFirstFoundStaffNo && userIdForCanBook != null && userIdForCanBook.Count > 0)
                {
                    UserIdsAuthorisationContext userIdAuthorisation = userIdForCanBook.First();
                    firstFoundStaffNo = userIdAuthorisation.Id;
                }

                List<ExoTimeAuthorisation> projectExoTimeAuths = ExoQueries.GetExoLinesAuthorisations(projectContext.PrimeroEntitiesUnitOfWork, projectContext.ProjectNumber, firstFoundStaffNo);
                projectExoTimeAuths.ForEach(x => x.OfficeName = projectContext.OfficeName);
                cacheExoAuthorisations.AddRange(projectExoTimeAuths);
            }

            exoAuthorisations = new List<ExoTimeAuthorisation>(cacheExoAuthorisations);
            //view can be closed if this is a async task and projection can be disposed
            if(projections != null)
                foreach (var deliverable in projections)
                {
                    ExoTimeAuthorisation findAuthorisation = exoAuthorisations.Where(x => userIdForCanBook.Any(y => y.Id == x.ResourceStaffId && y.OfficeName == x.OfficeName)).FirstOrDefault(x => x.SubJobCode == deliverable.Subjob_Name && x.DisciplineCode == deliverable.Discipline_Code && x.CommodityCode == deliverable.Commodity_Code);
                    deliverable.CanBook = findAuthorisation != null;
                    deliverable.Update();
                }
            else
            {
                cacheExoAuthorisations.Clear();
            }
        }

        public static int? GetUpdatedProjectLocaleUserExoId(string officeName, IEnumerable<USER> USERCollection, Guid currentUserGuid)
        {
            USER currentUser = USERCollection.FirstOrDefault(x => x.GUID == currentUserGuid);
            if (currentUser == null)
                return null;

            if (officeName == BluePrintsResources.OfficeMontreal)
                return currentUser.EXO_STAFF_ID_REMOTE;
            else
                return currentUser.EXO_STAFF_ID;
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
                MessageBoxService.ShowMessage("You do not have authorisation to book time to\nSub Job: " + deliverable.Subjob_Name + "\nCost Group: " + deliverable.Discipline_Code + "\nCost Type: " + deliverable.Commodity_Code + "\n\nPlease contact " + pmName + " for assistance");
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
                                  join JOBCOST_HDR2 in primeroUOW.JOBCOST_HDR
                                  on JOBTRANS.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                                  join JOBCOST_HDR1 in primeroUOW.JOBCOST_HDR
                                  on JOBTRANS.JOBNO equals JOBCOST_HDR1.JOBNO
                                  where JOBCOST_HDR2.JOBCODE == projectNumber && JOBTRANS.TRANSTYPE == "C" && JOBTRANS.LINE_STATUS != "X" && JOBTRANS.STOCKCODE == "@" && JOBTRANS.TRANSDATE <= dataDate
                                  select new { JOBCOST_HDR1.JOBCODE, JOBTRANS.QUANTITY, JOBTRANS.STOCKCODE, JOBTRANS.LINETOTAL, JOBTRANS.LINECOST, JOBTRANS.TRANSDATE, VARIATIONCODE = JOBTRANS.X_VARIATIONCODE, JOBTRANS.INVOICED, JOBTRANS.INVOICEDATE, JOBTRANS.INVSEQNO };

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
                revenueDataPoint.Costs = jobTransaction.LINETOTAL == null ? 0 : (decimal)jobTransaction.LINETOTAL * currencyConversion;
                revenueDataPoint.CostPerQty = revenueDataPoint.Units == 0 ? 0 : revenueDataPoint.Costs / revenueDataPoint.Units;
                //burnedDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobTransaction.TRANSDATE);
                revenueDataPoint.ActualDate = jobTransaction.TRANSDATE == null ? DateTime.Now : (DateTime)jobTransaction.TRANSDATE;
                revenueDataPoint.ProgressDate = revenueDataPoint.ActualDate;
                revenueDataPoint.Subjob_Name = jobTransaction.JOBCODE;
                revenueDataPoint.Quantity = (decimal)jobTransaction.QUANTITY;
                revenueDataPoint.StockCode = jobTransaction.STOCKCODE;
                revenueDataPoint.Variation_Code = BluePrintsDataUtils.normalizeVariationCode(jobTransaction.VARIATIONCODE);
                revenueDataPoint.InvoiceNo = jobTransaction.INVSEQNO.ToString();
                revenueDataPoint.InvoiceAmount = Convert.ToDecimal(jobTransaction.INVOICED);
                revenueDataPoint.InvoiceDate = jobTransaction.INVOICEDATE;

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
                                  select new { JOBCOST_HDR1.JOBCODE, JOBTRANS.EXCHRATE, JOBTRANS.QUANTITY, JOBTRANS.STOCKCODE, JOBTRANS.LINETOTAL, JOBTRANS.LINECOST, JOBTRANS.TRANSDATE, JOBCOST_RESOURCE.RESOURCENAME, JOBCOST_RESOURCE.TITLE, JOB_COSTGROUPS.COSTDESC, COSTDESC3 = JOB_COSTTYPES.COSTDESC, VARIATIONCODE = JOBTRANS.X_VARIATIONCODE, JOBTRANS.INVOICED, JOBTRANS.INVOICEDATE, JOBTRANS.INVSEQNO, PONarrate.NARRATIVE };

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
                    if (qualifiedSubjobs == null || (jobTransaction.COSTDESC3 != null && (jobTransaction.COSTDESC3.Length >= 3 && (!jobTransaction.COSTDESC3.Substring(0, 3).Contains("G99") && !jobTransaction.COSTDESC3.Substring(0, 3).Contains("010")))))
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
                        burnedDataPoint.Quantity = jobTransaction.QUANTITY == null ? 0 :(decimal)jobTransaction.QUANTITY;
                        burnedDataPoint.Role = jobTransaction.TITLE;
                        burnedDataPoint.CostGroup = jobTransaction.COSTDESC;
                        burnedDataPoint.CostType = jobTransaction.COSTDESC3;
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

            if(missingSUBJOBS != null)
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
            return burnedDataPoints;
        }

        public static List<ExoDataPoint> GetMaterials(IPrimeroEntitiesUnitOfWork primeroUOW, string projectNumber, DateTime dataDate, List<DateTime> alignedDataDates = null, decimal currencyConversion = 1, bool showLoadingScreen = false)
        {
            List<ExoDataPoint> materialDataPoints = new List<ExoDataPoint>();
            primeroUOW.AutoDetectChangesEnabled(false);
            DateTime invoiceCutOffDate = dataDate.Date.AddDays(1).AddHours(-1);
            var jobMaterials = from X_JOB_TRANSACTIONS_DETAIL in primeroUOW.X_JOB_TRANSACTIONS_DETAILS
                               join JOBCOST_HDR in primeroUOW.JOBCOST_HDR
                               on X_JOB_TRANSACTIONS_DETAIL.jobno equals JOBCOST_HDR.JOBNO
                               join JOBCOST_HDR2 in primeroUOW.JOBCOST_HDR
                               on JOBCOST_HDR.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                               join DR_ACCS in primeroUOW.DR_ACCS
                               on JOBCOST_HDR.ACCNO equals DR_ACCS.ACCNO
                               join STOCK_ITEMS in primeroUOW.STOCK_ITEMS
                               on X_JOB_TRANSACTIONS_DETAIL.stockcode equals STOCK_ITEMS.STOCKCODE
                               join GLP in primeroUOW.GLACCS
                               on STOCK_ITEMS.PURCH_GL_CODE equals GLP.ACCNO
                               join GLCOS in primeroUOW.GLACCS
                               on STOCK_ITEMS.COS_GL_CODE equals GLCOS.ACCNO
                               where X_JOB_TRANSACTIONS_DETAIL.linecharge == 0 && X_JOB_TRANSACTIONS_DETAIL.transtype == "C" && JOBCOST_HDR2.JOBCODE == projectNumber && X_JOB_TRANSACTIONS_DETAIL.transdate <= invoiceCutOffDate
                               select new { X_JOB_TRANSACTIONS_DETAIL.jobno, X_JOB_TRANSACTIONS_DETAIL.EXCHRATE, X_JOB_TRANSACTIONS_DETAIL.master_jobno, X_JOB_TRANSACTIONS_DETAIL.jobcode, X_JOB_TRANSACTIONS_DETAIL.transdate, X_JOB_TRANSACTIONS_DETAIL.transtype, X_JOB_TRANSACTIONS_DETAIL.stockcode, X_JOB_TRANSACTIONS_DETAIL.description, X_JOB_TRANSACTIONS_DETAIL.quantity, X_JOB_TRANSACTIONS_DETAIL.unitcost, X_JOB_TRANSACTIONS_DETAIL.UNITPRICE, X_JOB_TRANSACTIONS_DETAIL.LINECOST, X_JOB_TRANSACTIONS_DETAIL.linecharge, X_JOB_TRANSACTIONS_DETAIL.LINETOTAL, X_JOB_TRANSACTIONS_DETAIL.LINETOTAL_INCTAX, X_JOB_TRANSACTIONS_DETAIL.LINETOTAL_TAX, X_JOB_TRANSACTIONS_DETAIL.LINE_STATUS, X_JOB_TRANSACTIONS_DETAIL.CostType, X_JOB_TRANSACTIONS_DETAIL.CostTypeDesc, X_JOB_TRANSACTIONS_DETAIL.Typeshortcode, X_JOB_TRANSACTIONS_DETAIL.COST_GROUP, X_JOB_TRANSACTIONS_DETAIL.CostGroupDesc, X_JOB_TRANSACTIONS_DETAIL.GroupShortcode, X_JOB_TRANSACTIONS_DETAIL.branchno, X_JOB_TRANSACTIONS_DETAIL.LINE_SOURCE, X_JOB_TRANSACTIONS_DETAIL.SOURCE_SEQNO, X_JOB_TRANSACTIONS_DETAIL.PO_LINESEQNO, X_JOB_TRANSACTIONS_DETAIL.POno, X_JOB_TRANSACTIONS_DETAIL.invseqno, X_JOB_TRANSACTIONS_DETAIL.refno, X_JOB_TRANSACTIONS_DETAIL.name, X_JOB_TRANSACTIONS_DETAIL.invno, X_JOB_TRANSACTIONS_DETAIL.INVOICED, X_JOB_TRANSACTIONS_DETAIL.INVOICEDATE, X_JOB_TRANSACTIONS_DETAIL.CostActual, X_JOB_TRANSACTIONS_DETAIL.glcode, X_JOB_TRANSACTIONS_DETAIL.accno, JOBCOST_HDR.QUOTEDATE, JOBCOST_HDR.STARTDATE, JOBCOST_HDR.DUEDATE, JOBCOST_HDR.CUSTORDNO, JOBCOST_HDR.TITLE, NAME_2 = DR_ACCS.NAME, MasterJobcode = JOBCOST_HDR2.JOBCODE, STOCK_ITEMS.PURCH_GL_CODE, PurchGLName = GLP.NAME, STOCK_ITEMS.COS_GL_CODE, COSGlName = GLCOS.NAME, VariationCode = X_JOB_TRANSACTIONS_DETAIL.X_VARIATIONCODE };

            if (showLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(jobMaterials.Count());
                LoadingScreenManager.SetMessage("Loading Materials...");
            }

            var jobMaterialsList = jobMaterials.ToList();
            foreach(var jobMaterial in jobMaterialsList)
            {
                if (jobMaterial.CostGroupDesc != null && (jobMaterial.CostGroupDesc.Length >= 3 && (!jobMaterial.CostGroupDesc.Substring(0, 3).Contains("G99") && !jobMaterial.CostGroupDesc.Substring(0, 3).Contains("010"))))
                {
                    ExoDataPoint materialDataPoint = new ExoDataPoint();
                    materialDataPoint.BudgetedUnits = 0;
                    materialDataPoint.BudgetedCosts = 0;

                    decimal qty = jobMaterial.quantity == null ? 0 : (decimal)jobMaterial.quantity;
                    decimal lineCost = jobMaterial.LINECOST == null ? 0 : (decimal)jobMaterial.LINECOST;
                    materialDataPoint.Units = qty;
                    materialDataPoint.Costs = lineCost * currencyConversion;
                    materialDataPoint.CostPerQty = materialDataPoint.Units == 0 ? 0 : materialDataPoint.Costs / materialDataPoint.Units;

                    if (alignedDataDates != null)
                        materialDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobMaterial.transdate);

                    materialDataPoint.ActualDate = jobMaterial.transdate == null ? DateTime.Now : (DateTime)jobMaterial.transdate;
                    materialDataPoint.Subjob_Name = jobMaterial.jobcode;
                    materialDataPoint.ResourceName = string.Empty;
                    materialDataPoint.Quantity = qty;
                    materialDataPoint.Description = jobMaterial.description;
                    materialDataPoint.Supplier = jobMaterial.name;
                    materialDataPoint.InvoiceNo = jobMaterial.invno;
                    materialDataPoint.CostGroup = jobMaterial.CostGroupDesc;
                    materialDataPoint.CostType = jobMaterial.CostTypeDesc;
                    materialDataPoint.StockCode = jobMaterial.stockcode;
                    materialDataPoint.Cost_GLName = jobMaterial.COSGlName;
                    materialDataPoint.Purchase_GLName = jobMaterial.PurchGLName;
                    materialDataPoint.Variation_Code = normalizeVariationCode(jobMaterial.VariationCode);
                    materialDataPoint.InvoiceAmount = Convert.ToDecimal(jobMaterial.INVOICED);
                    materialDataPoint.InvoiceDate = jobMaterial.INVOICEDATE;
                    materialDataPoint.PONumber = jobMaterial.POno == null ? string.Empty : ((int)jobMaterial.POno).ToString();

                    materialDataPoints.Add(materialDataPoint);
                }

                if (showLoadingScreen)
                    LoadingScreenManager.Progress();
            }

            if (showLoadingScreen)
                LoadingScreenManager.CloseLoadingScreen();

            primeroUOW.AutoDetectChangesEnabled(true);
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

        public static List<ExoDataPoint> GetEXOPO(IPrimeroEntitiesUnitOfWork primeroUOW, string projectNumber, DateTime queryDate, List<DateTime> alignedDataDates = null, bool showLoadingScreen = false)
        {
            List<ExoDataPoint> poDataPoints = new List<ExoDataPoint>();

            if (showLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(1);
                LoadingScreenManager.SetMessage("Loading POs...");
            }

            DateTime poCutOffDate = queryDate.Date.AddDays(1).AddHours(-1);
            var pos = from PURCHORD_LINES in primeroUOW.PURCHORD_LINES
                      join PURCHORD_HDR in primeroUOW.PURCHORD_HDR
                      on PURCHORD_LINES.HDR_SEQNO equals PURCHORD_HDR.SEQNO
                      join CR_ACCS in primeroUOW.CR_ACCS
                      on PURCHORD_HDR.ACCNO equals CR_ACCS.ACCNO
                      join JOBCOST_HDR in primeroUOW.JOBCOST_HDR
                      on PURCHORD_LINES.JOBNO equals JOBCOST_HDR.JOBNO
                      join JOBCOST_HDR2 in primeroUOW.JOBCOST_HDR
                      on JOBCOST_HDR.MASTER_JOBNO equals JOBCOST_HDR2.JOBNO
                      join JOB_COSTTYPES in primeroUOW.JOB_COSTTYPES
                      on PURCHORD_LINES.COSTTYPE equals JOB_COSTTYPES.SEQNO
                      join JOB_COSTGROUPS in primeroUOW.JOB_COSTGROUPS
                      on PURCHORD_LINES.COSTGROUP equals JOB_COSTGROUPS.SEQNO
                      join NARRATIVES in primeroUOW.NARRATIVES
                      on PURCHORD_LINES.NARRATIVE_SEQNO equals NARRATIVES.SEQNO into PONarratives
                      from PONarrate in PONarratives.DefaultIfEmpty()
                      where PURCHORD_HDR.STATUS != 2 && JOBCOST_HDR2.JOBCODE == projectNumber && PURCHORD_HDR.ORDERDATE < poCutOffDate
                      select new { PURCHORD_HDR.EXCHRATE, PURCHORD_LINES.POLINEID, PURCHORD_LINES.STOCKCODE, PURCHORD_LINES.DESCRIPTION, PONarrate.NARRATIVE, PURCHORD_HDR.SEQNO, PURCHORD_LINES.LINETOTAL, CR_ACCS.NAME, JOBCOST_HDR.JOBCODE, JOBCOST_HDR.TITLE, COSTTYPEDESC = JOB_COSTTYPES.COSTDESC, COSTGROUPDESC = JOB_COSTGROUPS.COSTDESC, PURCHORD_LINES.ORD_QUANT, PURCHORD_LINES.SUP_QUANT, PURCHORD_LINES.UNITPRICE, PURCHORD_HDR.STATUS, PURCHORD_HDR.DUEDATE, PURCHORD_HDR.ORDERDATE, PURCHORD_LINES.X_VARIATIONCODE };

            var poList = pos.ToList();

            IQueryable<INWARDS_GOODS_LINES> inwardGoods = from INWARDS_GOODS_LINES in primeroUOW.INWARDS_GOODS_LINES
                                                          join PURCHORD_LINES in primeroUOW.PURCHORD_LINES
                                                          on INWARDS_GOODS_LINES.PO_LINE_NUM equals PURCHORD_LINES.POLINEID
                                                          join SUBJOB in primeroUOW.JOBCOST_HDR
                                                          on PURCHORD_LINES.JOBNO equals SUBJOB.JOBNO
                                                          join MASTERJOB in primeroUOW.JOBCOST_HDR
                                                          on SUBJOB.MASTER_JOBNO equals MASTERJOB.JOBNO
                                                          where MASTERJOB.JOBCODE == projectNumber && INWARDS_GOODS_LINES.INV_TRANSDATE < poCutOffDate
                                                          select INWARDS_GOODS_LINES;

            List<INWARDS_GOODS_LINES> inwardGoodsList = inwardGoods.ToList();
            if (showLoadingScreen)
            {
                LoadingScreenManager.CloseLoadingScreen();
                LoadingScreenManager.ShowLoadingScreen(poList.Count);
                LoadingScreenManager.SetMessage("Loading POs...");
            }

            foreach(var po in poList)
            {
                if (po.COSTGROUPDESC != null && (po.COSTGROUPDESC.Length >= 3 && !po.COSTGROUPDESC.Substring(0, 3).Contains("G99") && !po.COSTGROUPDESC.Substring(0, 3).Contains("010")))
                {
                    ExoDataPoint poDataPoint = new ExoDataPoint();
                    poDataPoint.BudgetedUnits = 0;
                    poDataPoint.BudgetedCosts = 0;
                    decimal orderQty = po.ORD_QUANT == null ? 0 : ((decimal)po.ORD_QUANT);

                    List<INWARDS_GOODS_LINES> currentPOInwardGoods = inwardGoodsList.Where(x => x.PO_LINE_NUM == po.POLINEID).Where(x => x.QUANTITY != null).ToList();
                    double supplyQty = currentPOInwardGoods.Sum(x => (double)x.QUANTITY);
                    decimal unitPrice = po.UNITPRICE == null ? 0 : po.EXCHRATE == null || po.EXCHRATE == 0 ? ((decimal)po.UNITPRICE) : ((decimal)po.UNITPRICE) / ((decimal)po.EXCHRATE);
                    poDataPoint.TotalUnits = orderQty;

                    poDataPoint.Units = orderQty - (decimal)supplyQty;
                    poDataPoint.Costs = poDataPoint.Units * unitPrice;
                    poDataPoint.CostPerQty = unitPrice;
                    poDataPoint.TotalCosts = po.LINETOTAL == null ? 0 : (decimal)po.LINETOTAL;
                    if (alignedDataDates != null)
                        poDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= (DateTime)po.ORDERDATE);

                    poDataPoint.ActualDate = po.ORDERDATE == null ? DateTime.Now : (DateTime)po.ORDERDATE;
                    poDataPoint.Subjob_Name = po.JOBCODE;
                    poDataPoint.ResourceName = string.Empty;
                    poDataPoint.Quantity = poDataPoint.Units;
                    poDataPoint.Description = po.DESCRIPTION;
                    poDataPoint.Narrative = po.NARRATIVE;
                    poDataPoint.Supplier = po.NAME;
                    poDataPoint.InvoiceNo = string.Empty;
                    poDataPoint.CostGroup = po.COSTGROUPDESC;
                    poDataPoint.CostType = po.COSTTYPEDESC;
                    poDataPoint.StockCode = po.STOCKCODE;
                    poDataPoint.Cost_GLName = string.Empty;
                    poDataPoint.Purchase_GLName = string.Empty;
                    poDataPoint.IsPO = true;
                    poDataPoint.PONumber = po.SEQNO.ToString();
                    poDataPoint.POOrderQty = po.ORD_QUANT == null ? 0 : Convert.ToDecimal((double)po.ORD_QUANT);
                    poDataPoint.POSuppliedQty = po.SUP_QUANT == null ? 0 : Convert.ToDecimal((double)po.SUP_QUANT);
                    poDataPoint.Variation_Code = normalizeVariationCode(po.X_VARIATIONCODE);
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

        public static decimal GetStockLevelProductivity(IReportable reportable, ref bool isOverride)
        {
            decimal reportableProductivity = reportable.Override_Productivity == null ? reportable.Current_Productivity : (decimal)reportable.Override_Productivity;
            if (reportableProductivity == 0)
                reportableProductivity = 1;

            isOverride = reportable.Override_Productivity != null;
            return reportableProductivity;
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
            IEnumerable<SUBJOB> SUBJOBCollection = bluePrintsUnitOfWork.SUBJOBS;
            ////provision for when subjob is manually assigned or using legacy subjob
            if (entity.Subjob_Guid != null)
            {
                SUBJOB subjob = SUBJOBCollection.FirstOrDefault(x => x.GUID == entity.Subjob_Guid);
                if (subjob != null && subjob.INTERNAL_NAME1 == internalNumber)
                    return;
            }

            if (internalNumber != string.Empty)
            {
                SUBJOB existingSUBJOB = bluePrintsUnitOfWork.SUBJOBS.Where(x => x.GUID_PROJECT == loadPROJECT.GUID).FirstOrDefault(x => x.INTERNAL_NAME1 == internalNumber);
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

        public static string GetPhaseCode(string subjobCode)
        {
            if (subjobCode == string.Empty)
                return string.Empty;
            else if (subjobCode.Length < 15)
                return string.Empty;

            return subjobCode.Substring(13, 2);
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
        /// Searches rate cascadingly for IRATE interface
        /// </summary>
        /// <returns></returns>
        public static RATE CascadeRateSearch(Guid? phaseGuid, Guid? disciplineGuid, Guid? departmentGuid, string commodityCode, IEnumerable<RATE> RATECollection, CostType CostType)
        {
            IEnumerable<RATE> rateByPhase = RATECollection.Where(y => y.COST_TYPE == CostType && (y.GUID_PHASE == phaseGuid));
            //order by descending places null GUID's at the end, so First() won't pick it up
            IEnumerable<RATE> rateByCommodities;
            rateByCommodities = rateByPhase.Where(y => (y.COMMODITY_CODE == commodityCode) || (y.COMMODITY_CODE == string.Empty || y.COMMODITY_CODE == null)).OrderByDescending(y => y.COMMODITY_CODE);

            IEnumerable<RATE> rateByDiscipline = rateByCommodities.Where(y => (y.GUID_DISCIPLINE == disciplineGuid) || (y.GUID_DISCIPLINE == null)).OrderByDescending(y => y.GUID_DISCIPLINE);
            IEnumerable<RATE> rateByDepartment = rateByDiscipline.Where(y => (y.GUID_DEPARTMENT == departmentGuid) || (y.GUID_DEPARTMENT == null)).OrderByDescending(y => y.GUID_DEPARTMENT);

            return rateByDepartment.FirstOrDefault();
        }

        public static IEnumerable<COMMODITY_CODE> FilterForValidCommodityCodes(IEnumerable<COMMODITY_CODE> COMMODITY_CODES, string fullDisciplineCode, PhaseType? phaseType = null)
        {
            if (COMMODITY_CODES == null || fullDisciplineCode.Length < 2)
                return new List<COMMODITY_CODE>();

            List<COMMODITY_CODE> validCommodityCodes;
            string disciplineCode = fullDisciplineCode.Substring(0, 2);
            if (phaseType == null)
            {
                validCommodityCodes = COMMODITY_CODES.Where(x => (x.DISCIPLINE == null || (x.DISCIPLINE.CODE.Length >= 2 && x.DISCIPLINE.CODE.Substring(0, 2) == disciplineCode))).OrderBy(x => x.CODE).ToList();
            }
            else if (phaseType == PhaseType.Tender)
                validCommodityCodes = COMMODITY_CODES.Where(x => (x.DISCIPLINE == null || (x.DISCIPLINE.CODE.Length >= 2 && x.DISCIPLINE.CODE.Substring(0, 2) == BluePrintsResources.Default_TenderDisciplineCode))).OrderBy(x => x.CODE).ToList();
            else
            {
                IEnumerable<COMMODITY_CODE> phaseCommodityCodes;
                if (phaseType == Common.PhaseType.Design)
                    //because design deliverable's have indirect components also
                    phaseCommodityCodes = COMMODITY_CODES.Where(x => x.PHASE_TYPE == Common.PhaseType.Design || x.PHASE_TYPE == Common.PhaseType.Indirect);
                else
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

        public static void UpdatePercentagesByStatus(IMessageBoxService MessageBoxService, CollectionViewModel<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESS_ITEMSCollectionViewModel, IEnumerable<BASELINE_ITEMProgress> entities)
        {
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
                            decimal postProgressEarnedUnit = (iterateEarnedUnits + progressByDate.EARNED_UNITS);
                            decimal oldProgressEarnUnit = progressByDate.EARNED_UNITS;
                            if (postProgressEarnedUnit > maxAllowableEarnedUnit)
                            {
                                decimal newProgressEarnUnit = (maxAllowableEarnedUnit - iterateEarnedUnits);
                                progressByDate.EARNED_UNITS = newProgressEarnUnit < 0 ? 0 : newProgressEarnUnit;
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
            //if (LoginCredentials.IsAdmin)
            //    return;

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
    }
}