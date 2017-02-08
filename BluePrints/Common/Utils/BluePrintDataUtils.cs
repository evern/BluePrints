using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Projections;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.ViewModel.Utils
{
    public static class BluePrintDataUtils
    {
        /// <summary>
        /// Calculates workpack end date using project settings and start date
        /// </summary>
        public static DateTime WORKPACK_Calculate_EndDate(DateTime startDate, PROJECT fromPROJECT)
        {
            var periodPercentage = 1 - Convert.ToDouble(fromPROJECT.REVIEWPERCENTAGE);
            var periodMultiplier = 1 / periodPercentage;
            var reviewPeriod = Convert.ToDouble(fromPROJECT.REVIEWPERIOD);
            var period = TimeSpan.FromDays(reviewPeriod * periodMultiplier);

            var EndDate = startDate.Date.AddDays(period.Days).AddSeconds(-1);
            return EndDate;
        }

        /// <summary>
        /// Calculates workpack start date using project settings and end date
        /// </summary>
        public static DateTime WORKPACK_Calculate_StartDate(DateTime endDate, PROJECT fromPROJECT)
        {
            var periodPercentage = 1 - Convert.ToDouble(fromPROJECT.REVIEWPERCENTAGE);
            var periodMultiplier = 1 / periodPercentage;
            var reviewPeriod = Convert.ToDouble(fromPROJECT.REVIEWPERIOD);
            var period = TimeSpan.FromDays(reviewPeriod * periodMultiplier);

            var StartDate = endDate.Date.AddDays(period.Days * -1);
            return StartDate;
        }

        /// <summary>
        /// Calculate the review start date or end date
        /// </summary>
        /// <param name="getEndDate">whether to get end date else return start date</param>
        public static void WORKPACK_Calculate_ReviewPeriod(ref DateTime StartDate, ref DateTime EndDate,
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


        /// <summary>
        /// Generate internal number1 when all required fields are populated
        /// </summary>
        public static string BASELINEITEM_Generate_InternalNumber(PROJECT fromPROJECT,
            IEnumerable<BASELINE_ITEMProjection> BASELINE_ITEMEntities, AREA selectedAREA, DISCIPLINE selectedDISCIPLINE,
            DOCTYPE selectedDOCTYPE, Guid? excludeGUID = null)
        {
            if (selectedAREA != null && selectedDISCIPLINE != null && selectedDOCTYPE != null)
            {
                var InternalNum = fromPROJECT.NUMBER;
                if (selectedAREA != null)
                    InternalNum += "-" + selectedAREA.INTERNAL_NUM;
                if (selectedDOCTYPE != null)
                    InternalNum += "-" + selectedDOCTYPE.CODE;
                if (selectedDISCIPLINE != null)
                    InternalNum += "-" + selectedDISCIPLINE.CODE;

                var internalNameCount =
                    BASELINE_ITEMEntities.Where(x => x.GUID != excludeGUID)
                        .Count(
                            x =>
                                x.BASELINE_ITEM.INTERNAL_NUM != null &&
                                x.BASELINE_ITEM.INTERNAL_NUM.Contains(InternalNum));
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
            {
                return string.Empty;
            }
        }

        public static string BASELINEITEM_Generate_InternalNumber(PROJECT fromPROJECT,
            IEnumerable<BASELINE_ITEM> BASELINE_ITEMEntities, AREA selectedAREA, DISCIPLINE selectedDISCIPLINE,
            DOCTYPE selectedDOCTYPE, Guid? excludeGUID = null)
        {
            if (selectedAREA != null && selectedDISCIPLINE != null && selectedDOCTYPE != null)
            {
                var InternalNum = fromPROJECT.NUMBER;
                if (selectedAREA != null)
                    InternalNum += "-" + selectedAREA.INTERNAL_NUM;
                if (selectedDOCTYPE != null)
                    InternalNum += "-" + selectedDOCTYPE.CODE;
                if (selectedDISCIPLINE != null)
                    InternalNum += "-" + selectedDISCIPLINE.CODE;

                var internalNameCount =
                    BASELINE_ITEMEntities.Where(x => x.GUID != excludeGUID)
                        .Count(x => x.INTERNAL_NUM != null && x.INTERNAL_NUM.Contains(InternalNum));
                internalNameCount += 1;

                var countString = string.Empty;
                if (internalNameCount < 10)
                    countString = "00" + internalNameCount.ToString();
                else if (internalNameCount < 100)
                    countString = "0" + internalNameCount.ToString();
                else
                    countString = internalNameCount.ToString();

                InternalNum += countString;
                return InternalNum;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Generate internal number1 when all required fields are populated
        /// </summary>
        public static string WORKPACK_Generate_InternalNumber1(PROJECT fromPROJECT, WORKPACK fromWORKPACK,
            IEnumerable<WORKPACK> WORKPACKEntities, IEnumerable<AREA> lookUpAREA,
            IEnumerable<DISCIPLINE> lookUpDISCIPLINE, IEnumerable<DOCTYPE> lookUpDOCTYPE)
        {
            AREA findAREA;
            DISCIPLINE findDISCIPLINE;
            DOCTYPE findDOCTYPE;

            if (fromWORKPACK.AREA == null || fromWORKPACK.DISCIPLINE == null || fromWORKPACK.DOCTYPE == null)
            {
                findAREA = lookUpAREA.FirstOrDefault(area => area.GUID == fromWORKPACK.GUID_DAREA);
                findDISCIPLINE =
                    lookUpDISCIPLINE.FirstOrDefault(discipline => discipline.GUID == fromWORKPACK.GUID_DDISCIPLINE);
                findDOCTYPE = lookUpDOCTYPE.FirstOrDefault(doctype => doctype.GUID == fromWORKPACK.GUID_DDOCTYPE);
            }
            else
            {
                findAREA = fromWORKPACK.AREA;
                findDISCIPLINE = fromWORKPACK.DISCIPLINE;
                findDOCTYPE = fromWORKPACK.DOCTYPE;
            }

            if (findAREA != null && findDISCIPLINE != null && findDOCTYPE != null)
            {
                var InternalName = fromPROJECT.NUMBER;
                InternalName += "-" + findAREA.INTERNAL_NUM;
                InternalName += findDOCTYPE.CODE;
                InternalName += findDISCIPLINE.CODE;

                var InternalNameCount =
                    WORKPACKEntities.Count(
                        obj => obj.INTERNAL_NAME1 != null && obj.INTERNAL_NAME1.Contains(InternalName)) + 1;

                InternalName += InternalNameCount.ToString();

                return InternalName;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Generate internal number2 when all required fields are populated
        /// </summary>
        public static string WORKPACK_Generate_InternalNumber2(PROJECT fromPROJECT, WORKPACK fromWORKPACK,
            IEnumerable<WORKPACK> WORKPACKEntities, IEnumerable<AREA> lookUpAREA,
            IEnumerable<DISCIPLINE> lookUpDISCIPLINE, IEnumerable<PHASE> lookUpPHASE)
        {
            AREA findAREA;
            DISCIPLINE findDISCIPLINE;
            PHASE findPHASE;

            if (fromWORKPACK.AREA == null || fromWORKPACK.DISCIPLINE == null || fromWORKPACK.AREA == null)
            {
                findAREA = lookUpAREA.FirstOrDefault(area => area.GUID == fromWORKPACK.GUID_DAREA);
                findPHASE = lookUpPHASE.FirstOrDefault(phase => phase.GUID == fromWORKPACK.GUID_DPHASE);
                findDISCIPLINE =
                    lookUpDISCIPLINE.FirstOrDefault(discipline => discipline.GUID == fromWORKPACK.GUID_DDISCIPLINE);
            }
            else
            {
                findAREA = fromWORKPACK.AREA;
                findPHASE = fromWORKPACK.PHASE;
                findDISCIPLINE = fromWORKPACK.DISCIPLINE;
            }

            if (findAREA != null && findDISCIPLINE != null && findPHASE != null)
            {
                var InternalName = fromPROJECT.NUMBER;
                InternalName += "-" + findPHASE.INTERNAL_NUM;
                InternalName += findAREA.INTERNAL_NUM;
                InternalName += findDISCIPLINE.CODE;

                var InternalNameCount =
                    WORKPACKEntities.Count(
                        obj => obj.INTERNAL_NAME2 != null && obj.INTERNAL_NAME2.Contains(InternalName)) + 1;

                InternalName += InternalNameCount.ToString();

                return InternalName;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Generate internal number2 when all required fields are populated
        /// </summary>
        public static string WORKPACK_Generate_InstallSupplyInternalNumber(PROJECT fromPROJECT, WORKPACK fromWORKPACK,
            IEnumerable<WORKPACK> WORKPACKEntities, IEntitiesViewModel<AREA> lookUpAREA,
            IEntitiesViewModel<DISCIPLINE> lookUpDISCIPLINE, IEntitiesViewModel<PHASE> lookUpPHASE, bool IsInstall)
        {
            AREA findAREA;
            DISCIPLINE findDISCIPLINE;
            PHASE findPHASE;

            if (fromWORKPACK.AREA == null || fromWORKPACK.DISCIPLINE == null || fromWORKPACK.AREA == null)
            {
                findAREA = lookUpAREA.Entities.FirstOrDefault(area => area.GUID == fromWORKPACK.GUID_DAREA);
                findPHASE = lookUpPHASE.Entities.FirstOrDefault(phase => phase.GUID == fromWORKPACK.GUID_DPHASE);
                findDISCIPLINE =
                    lookUpDISCIPLINE.Entities.FirstOrDefault(
                        discipline => discipline.GUID == fromWORKPACK.GUID_DDISCIPLINE);
            }
            else
            {
                findAREA = fromWORKPACK.AREA;
                findPHASE = fromWORKPACK.PHASE;
                findDISCIPLINE = fromWORKPACK.DISCIPLINE;
            }

            if (findAREA != null && findDISCIPLINE != null && findPHASE != null)
            {
                var InternalName = fromPROJECT.NUMBER;
                InternalName += IsInstall == true ? "I" : "S";
                InternalName += findAREA.INTERNAL_NUM;
                InternalName += findDISCIPLINE.CODE;

                var InternalNameCount =
                    WORKPACKEntities.Count(
                        obj => obj.INTERNAL_NAME2 != null && obj.INTERNAL_NAME2.Contains(InternalName)) + 1;

                if (InternalNameCount < 10)
                    InternalName += "0";

                InternalName += InternalNameCount.ToString();

                return InternalName;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}