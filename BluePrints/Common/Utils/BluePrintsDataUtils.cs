using BaseModel.Data.Helpers;
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
                var timeDifferenceFromCurrent = loadPROGRESS.DATA_DATE - DateTime.Now;

                if (timeDifferenceFromCurrent.TotalSeconds > interval.TotalSeconds)
                {
                    do
                    {
                        loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(-1 * interval.Days);
                    } while (loadPROGRESS.DATA_DATE > DateTime.Now);

                    return true;
                }
                else if (timeDifferenceFromCurrent.TotalSeconds < interval.TotalSeconds)
                {
                    if (timeDifferenceFromCurrent.TotalSeconds < -1 * interval.TotalSeconds)
                    {
                        do
                        {
                            loadPROGRESS.DATA_DATE = loadPROGRESS.DATA_DATE.AddDays(1 * interval.Days);
                        } while (loadPROGRESS.DATA_DATE < DateTime.Now - interval);

                        return true;
                    }
                    else
                        return false;
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

        public static void LoadExoAuthorisation<TProjection>(IEnumerable<TProjection> projections, ref List<ExoTimeAuthorisation> exoAuthorisations, ref List<string> narratives, HashSet<string> projectNumbers, IPrimeroEntitiesUnitOfWork primeroUnitOfWork)
            where TProjection : IReportable, IBookable
        {
            if (exoAuthorisations != null && narratives != null)
                return;

            List<ExoTimeAuthorisation> cacheExoAuthorisations = new List<ExoTimeAuthorisation>();
            List<string> cacheNarratives = new List<string>();
            foreach (var projectNumber in projectNumbers)
            {
                List<ExoTimeAuthorisation> projectExoTimeAuths = ExoQueries.GetExoLinesAuthorisations(primeroUnitOfWork, projectNumber, false);
                List<string> projectNarratives = ExoQueries.GetJobNarratives(primeroUnitOfWork, projectNumber);

                cacheExoAuthorisations.AddRange(projectExoTimeAuths);

                cacheExoAuthorisations.AddRange(projectExoTimeAuths);
                cacheNarratives.AddRange(projectNarratives);
            }

            exoAuthorisations = new List<ExoTimeAuthorisation>(cacheExoAuthorisations);
            narratives = new List<string>(cacheNarratives);

            //view can be closed if this is a async task and projection can be disposed
            if(projections != null)
                foreach (var deliverable in projections)
                {
                    ExoTimeAuthorisation findAuthorisation = exoAuthorisations.Where(x => x.ResourceStaffId == LoginCredentials.CurrentUser.EXO_STAFF_ID).FirstOrDefault(x => x.SubJobCode == deliverable.Subjob_Name && x.DisciplineCode == deliverable.Discipline_Code && x.CommodityCode == deliverable.Commodity_Code);
                    deliverable.CanBook = findAuthorisation != null;
                    deliverable.Update();
                }
            else
            {
                cacheExoAuthorisations.Clear();
                cacheNarratives.Clear();
            }
        }

        public static void BookTime(PROJECT project, IDeliverable deliverable, IPrimeroEntitiesUnitOfWork primeroUnitOfWork, List<ExoTimeAuthorisation> exoAuthorisations, List<string> narratives, IMessageBoxService MessageBoxService, IDialogService BookTimeDialogService)
        {
            var bookTimeViewModel = BookTimeSheetViewModel.Create(project, deliverable, primeroUnitOfWork, exoAuthorisations, narratives);
            if (bookTimeViewModel.GetResource() == null)
            {
                MessageBoxService.ShowMessage("You are not authorised to book time on this subjob, please contact the project manager for assistance");
            }
            else if (bookTimeViewModel.GetCostType() == null)
            {
                MessageBoxService.ShowMessage("You do not have \nSub Job: " + deliverable.Subjob_Name + "\nCost Group: " + deliverable.Discipline_Code + "\nCost Type: " + deliverable.Commodity_Code + "\nAdded in exo, please contact the project manager for assistance");
            }

            PrimeroSubJob subJob = bookTimeViewModel.GetSubJob();
            if(subJob != null && subJob.JobStatus.ToUpper() == "G")
            {
                MessageBoxService.ShowMessage("Job " + subJob.Code + " has already been closed, please contact cost control to open the job");
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
        public static List<StatsCalculationType> AllCalcTypes
        {
            get
            {
                List<StatsCalculationType> allCalcTypes = new List<StatsCalculationType>();
                allCalcTypes.Add(StatsCalculationType.Earned);
                allCalcTypes.Add(StatsCalculationType.Planned);
                allCalcTypes.Add(StatsCalculationType.Remaining);
                AllCalcTypes.Add(StatsCalculationType.Burned);

                return allCalcTypes;
            }
        }

        public static List<ExoDataPoint> GetMaterials(string projectNumber, List<DateTime> alignedDataDates = null, decimal currencyConversion = 1, bool showLoadingScreen = false)
        {
            ConcurrentBag<ExoDataPoint> materialDataPoints = new ConcurrentBag<ExoDataPoint>();
            var primeroUOW = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();
            if (showLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(1);
                LoadingScreenManager.SetMessage("Loading Materials...");
            }

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
                               where X_JOB_TRANSACTIONS_DETAIL.linecharge == 0 && X_JOB_TRANSACTIONS_DETAIL.transtype == "C" && JOBCOST_HDR2.JOBCODE == projectNumber
                               select new { X_JOB_TRANSACTIONS_DETAIL.jobno, X_JOB_TRANSACTIONS_DETAIL.master_jobno, X_JOB_TRANSACTIONS_DETAIL.jobcode, X_JOB_TRANSACTIONS_DETAIL.transdate, X_JOB_TRANSACTIONS_DETAIL.transtype, X_JOB_TRANSACTIONS_DETAIL.stockcode, X_JOB_TRANSACTIONS_DETAIL.description, X_JOB_TRANSACTIONS_DETAIL.quantity, X_JOB_TRANSACTIONS_DETAIL.unitcost, X_JOB_TRANSACTIONS_DETAIL.UNITPRICE, X_JOB_TRANSACTIONS_DETAIL.LINECOST, X_JOB_TRANSACTIONS_DETAIL.linecharge, X_JOB_TRANSACTIONS_DETAIL.LINETOTAL, X_JOB_TRANSACTIONS_DETAIL.LINETOTAL_INCTAX, X_JOB_TRANSACTIONS_DETAIL.LINETOTAL_TAX, X_JOB_TRANSACTIONS_DETAIL.LINE_STATUS, X_JOB_TRANSACTIONS_DETAIL.CostType, X_JOB_TRANSACTIONS_DETAIL.CostTypeDesc, X_JOB_TRANSACTIONS_DETAIL.Typeshortcode, X_JOB_TRANSACTIONS_DETAIL.COST_GROUP, X_JOB_TRANSACTIONS_DETAIL.CostGroupDesc, X_JOB_TRANSACTIONS_DETAIL.GroupShortcode, X_JOB_TRANSACTIONS_DETAIL.branchno, X_JOB_TRANSACTIONS_DETAIL.LINE_SOURCE, X_JOB_TRANSACTIONS_DETAIL.SOURCE_SEQNO, X_JOB_TRANSACTIONS_DETAIL.PO_LINESEQNO, X_JOB_TRANSACTIONS_DETAIL.POno, X_JOB_TRANSACTIONS_DETAIL.invseqno, X_JOB_TRANSACTIONS_DETAIL.refno, X_JOB_TRANSACTIONS_DETAIL.name, X_JOB_TRANSACTIONS_DETAIL.invno, X_JOB_TRANSACTIONS_DETAIL.INVOICED, X_JOB_TRANSACTIONS_DETAIL.INVOICEDATE, X_JOB_TRANSACTIONS_DETAIL.CostActual, X_JOB_TRANSACTIONS_DETAIL.glcode, X_JOB_TRANSACTIONS_DETAIL.accno, JOBCOST_HDR.QUOTEDATE, JOBCOST_HDR.STARTDATE, JOBCOST_HDR.DUEDATE, JOBCOST_HDR.CUSTORDNO, JOBCOST_HDR.TITLE, NAME_2 = DR_ACCS.NAME, MasterJobcode = JOBCOST_HDR2.JOBCODE, STOCK_ITEMS.PURCH_GL_CODE, PurchGLName = GLP.NAME, STOCK_ITEMS.COS_GL_CODE, COSGlName = GLCOS.NAME, VariationCode = X_JOB_TRANSACTIONS_DETAIL.X_VARIATIONCODE };

            var jobMaterialsList = jobMaterials.ToList();

            if (showLoadingScreen)
            {
                LoadingScreenManager.CloseLoadingScreen();
                LoadingScreenManager.ShowLoadingScreen(jobMaterialsList.Count);
                LoadingScreenManager.SetMessage("Loading Materials...");
            }

            foreach(var jobMaterial in jobMaterialsList)
            {
                if (jobMaterial.CostGroupDesc != null && (jobMaterial.CostGroupDesc.Length >= 3 && (!jobMaterial.CostGroupDesc.Substring(0, 3).Contains("G99") && !jobMaterial.CostGroupDesc.Substring(0, 3).Contains("010"))))
                {
                    ExoDataPoint materialDataPoint = new ExoDataPoint();
                    materialDataPoint.BudgetedUnits = 0;
                    materialDataPoint.BudgetedCosts = 0;
                    materialDataPoint.Units = (decimal)jobMaterial.quantity;
                    materialDataPoint.Costs = (decimal)jobMaterial.LINECOST * currencyConversion;

                    if (alignedDataDates != null)
                        materialDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= jobMaterial.transdate);

                    materialDataPoint.ActualDate = jobMaterial.transdate == null ? DateTime.Now : (DateTime)jobMaterial.transdate;
                    materialDataPoint.Subjob_Name = jobMaterial.jobcode;
                    materialDataPoint.ResourceName = string.Empty;
                    materialDataPoint.Quantity = (decimal)jobMaterial.quantity;
                    materialDataPoint.Description = jobMaterial.description;
                    materialDataPoint.Supplier = jobMaterial.name;
                    materialDataPoint.InvoiceNo = jobMaterial.invno;
                    materialDataPoint.CostGroup = jobMaterial.CostGroupDesc;
                    materialDataPoint.CostType = jobMaterial.CostTypeDesc;
                    materialDataPoint.Cost_GLName = jobMaterial.COSGlName;
                    materialDataPoint.Purchase_GLName = jobMaterial.PurchGLName;
                    materialDataPoint.Variation_Code = ExtractVariationCode(jobMaterial.VariationCode);
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

            return materialDataPoints.ToList();
        }

        public static string ExtractVariationCode(string variationCode)
        {
            if (variationCode == null)
                return string.Empty;

            if (variationCode.Length >= 6)
                return variationCode.Substring(0, 6);

            return string.Empty;
        }

        public static List<ExoDataPoint> GetEXOPO(string projectNumber, List<DateTime> alignedDataDates = null, bool showLoadingScreen = false)
        {
            ConcurrentBag<ExoDataPoint> poDataPoints = new ConcurrentBag<ExoDataPoint>();
            var primeroUOW = PrimeroEntitiesUnitOfWorkSource.GetUnitOfWorkFactory().CreateUnitOfWork();

            if (showLoadingScreen)
            {
                LoadingScreenManager.ShowLoadingScreen(1);
                LoadingScreenManager.SetMessage("Loading POs...");
            }

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
                      where PURCHORD_LINES.ORD_QUANT > PURCHORD_LINES.SUP_QUANT && PURCHORD_HDR.STATUS != 2 && JOBCOST_HDR2.JOBCODE == projectNumber
                      select new { PURCHORD_LINES.STOCKCODE, PURCHORD_LINES.DESCRIPTION, PURCHORD_HDR.SEQNO, PURCHORD_LINES.LINETOTAL, CR_ACCS.NAME, JOBCOST_HDR.JOBCODE, JOBCOST_HDR.TITLE, COSTTYPEDESC = JOB_COSTTYPES.COSTDESC, COSTGROUPDESC = JOB_COSTGROUPS.COSTDESC, PURCHORD_LINES.ORD_QUANT, PURCHORD_LINES.SUP_QUANT, PURCHORD_LINES.UNITPRICE, PURCHORD_HDR.STATUS, PURCHORD_HDR.DUEDATE, PURCHORD_HDR.ORDERDATE };

            var poList = pos.ToList();

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
                    poDataPoint.Units = ((decimal)po.ORD_QUANT) - ((decimal)po.SUP_QUANT);
                    poDataPoint.Costs = poDataPoint.Units * ((decimal)po.UNITPRICE);
                    poDataPoint.CostPerQty = ((decimal)po.UNITPRICE);
                    if (alignedDataDates != null)
                        poDataPoint.ProgressDate = alignedDataDates.FirstOrDefault(dates => dates.Date >= (DateTime)po.ORDERDATE);

                    poDataPoint.ActualDate = po.ORDERDATE == null ? DateTime.Now : (DateTime)po.ORDERDATE;
                    poDataPoint.Subjob_Name = po.JOBCODE;
                    poDataPoint.ResourceName = string.Empty;
                    poDataPoint.Quantity = poDataPoint.Units;
                    poDataPoint.Description = po.DESCRIPTION;
                    poDataPoint.Supplier = po.NAME;
                    poDataPoint.InvoiceNo = string.Empty;
                    poDataPoint.CostGroup = po.COSTGROUPDESC;
                    poDataPoint.CostType = po.COSTTYPEDESC;
                    poDataPoint.Cost_GLName = string.Empty;
                    poDataPoint.Purchase_GLName = string.Empty;
                    poDataPoint.IsPO = true;
                    poDataPoint.PONumber = po.SEQNO.ToString();
                    poDataPoint.POOrderQty = po.ORD_QUANT == null ? 0 : Convert.ToDecimal((double)po.ORD_QUANT);
                    poDataPoint.POSuppliedQty = po.SUP_QUANT == null ? 0 : Convert.ToDecimal((double)po.SUP_QUANT);
                    poDataPoint.Variation_Code = string.Empty;
                    poDataPoints.Add(poDataPoint);
                }

                if (showLoadingScreen)
                    LoadingScreenManager.Progress();
            }

            if (showLoadingScreen)
                LoadingScreenManager.CloseLoadingScreen();

            return poDataPoints.ToList();
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

        public static void UpdateAllPercentagesByStatus(IMessageBoxService MessageBoxService, CollectionViewModel<PROGRESS_ITEM, PROGRESS_ITEM, Guid, IBluePrintsEntitiesUnitOfWork> PROGRESS_ITEMSCollectionViewModel, IEnumerable<BASELINE_ITEMProgress> entities)
        {
            if (MessageBoxService.ShowMessage("Warning\nThis action will update or delete progresses based on deliverable status and is not reversible\nDo you wish to continue?",
                         BluePrintsResources.Warning_Caption, MessageButton.YesNo) == MessageResult.No)
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

            PROGRESS_ITEMSCollectionViewModel.BulkSave(updateProgress);
        }
    }
}