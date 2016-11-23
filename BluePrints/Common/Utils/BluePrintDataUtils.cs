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
            double periodPercentage = 1 - Convert.ToDouble(fromPROJECT.REVIEWPERCENTAGE);
            double periodMultiplier = 1 / periodPercentage;
            double reviewPeriod = Convert.ToDouble(fromPROJECT.REVIEWPERIOD);
            TimeSpan period = TimeSpan.FromDays(reviewPeriod * periodMultiplier);

            DateTime EndDate = startDate.Date.AddDays(period.Days).AddSeconds(-1);
            return EndDate;
        }

        /// <summary>
        /// Calculates workpack start date using project settings and end date
        /// </summary>
        public static DateTime WORKPACK_Calculate_StartDate(DateTime endDate, PROJECT fromPROJECT)
        {
            double periodPercentage = 1 - Convert.ToDouble(fromPROJECT.REVIEWPERCENTAGE);
            double periodMultiplier = 1 / periodPercentage;
            double reviewPeriod = Convert.ToDouble(fromPROJECT.REVIEWPERIOD);
            TimeSpan period = TimeSpan.FromDays(reviewPeriod * periodMultiplier);

            DateTime StartDate = endDate.Date.AddDays(period.Days * -1);
            return StartDate;
        }

        /// <summary>
        /// Calculate the review start date or end date
        /// </summary>
        /// <param name="getEndDate">whether to get end date else return start date</param>
        public static void WORKPACK_Calculate_ReviewPeriod(ref DateTime StartDate, ref DateTime EndDate, PROJECT fromPROJECT, bool getEndDate)
        {
            TimeSpan timeDifference = EndDate.Date.Subtract(StartDate.Date);
            double percentage = Convert.ToDouble(fromPROJECT.REVIEWPERCENTAGE);
            TimeSpan timeDifferencePercent = TimeSpan.FromTicks(Convert.ToInt64(timeDifference.Ticks * fromPROJECT.REVIEWPERCENTAGE));
            DateTime ReviewStartDate = StartDate.Date.Add(timeDifferencePercent);

            StartDate = ReviewStartDate.Date;
            DateTime ReviewEndDate = ReviewStartDate.Date.AddDays(Convert.ToDouble(fromPROJECT.REVIEWPERIOD)).AddSeconds(-1);
            EndDate = ReviewEndDate.Date;
        }


        /// <summary>
        /// Generate internal number1 when all required fields are populated
        /// </summary>
        public static string BASELINEITEM_Generate_InternalNumber(PROJECT fromPROJECT, IEnumerable<BASELINE_ITEMProjection> BASELINE_ITEMEntities, AREA selectedAREA, DISCIPLINE selectedDISCIPLINE, DOCTYPE selectedDOCTYPE, Guid? excludeGUID = null)
        {
            if (selectedAREA != null && selectedDISCIPLINE != null && selectedDOCTYPE != null)
            {
                string InternalNum = fromPROJECT.NUMBER;
                if(selectedAREA != null)
                    InternalNum += "-" + selectedAREA.INTERNAL_NUM;
                if (selectedDOCTYPE != null)
                    InternalNum += selectedDOCTYPE.CODE;
                if (selectedDISCIPLINE != null)
                    InternalNum += selectedDISCIPLINE.CODE;

                int internalNameCount = BASELINE_ITEMEntities.Where(x => x.GUID != excludeGUID).Count(x => x.BASELINE_ITEM.INTERNAL_NUM != null && x.BASELINE_ITEM.INTERNAL_NUM.Contains(InternalNum));
                internalNameCount += 1;
                InternalNum += internalNameCount.ToString();

                return InternalNum;
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Generate internal number1 when all required fields are populated
        /// </summary>
        public static string BASELINEITEM_Generate_InternalNumber(PROJECT fromPROJECT, BASELINE_ITEM fromBASELINE_ITEM, IEnumerable<BASELINE_ITEM> BASELINE_ITEMEntities, AREA selectedAREA, DISCIPLINE selectedDISCIPLINE, DOCTYPE selectedDOCTYPE)
        {
            if (selectedAREA != null && selectedDISCIPLINE != null && selectedDOCTYPE != null)
            {
                string InternalNum = fromPROJECT.NUMBER;
                InternalNum += "-" + selectedAREA.INTERNAL_NUM;
                InternalNum += selectedDOCTYPE.CODE;
                InternalNum += selectedDISCIPLINE.CODE;

                int InternalNameCount = BASELINE_ITEMEntities.Count(obj => obj.INTERNAL_NUM != null && obj.INTERNAL_NUM.Contains(InternalNum)) + 1;

                InternalNum += InternalNameCount.ToString();

                return InternalNum;
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Generate internal number1 when all required fields are populated
        /// </summary>
        public static string WORKPACK_Generate_InternalNumber1(PROJECT fromPROJECT, WORKPACK fromWORKPACK, IEnumerable<WORKPACK> WORKPACKEntities, IEntitiesViewModel<AREA> lookUpAREA, IEntitiesViewModel<DISCIPLINE> lookUpDISCIPLINE, IEntitiesViewModel<DOCTYPE> lookUpDOCTYPE)
        {
            AREA findAREA;
            DISCIPLINE findDISCIPLINE;
            DOCTYPE findDOCTYPE;

            if(fromWORKPACK.AREA == null || fromWORKPACK.DISCIPLINE == null || fromWORKPACK.DOCTYPE == null)
            {
                findAREA = lookUpAREA.Entities.FirstOrDefault(area => area.GUID == fromWORKPACK.GUID_DAREA);
                findDISCIPLINE = lookUpDISCIPLINE.Entities.FirstOrDefault(discipline => discipline.GUID == fromWORKPACK.GUID_DDISCIPLINE);
                findDOCTYPE = lookUpDOCTYPE.Entities.FirstOrDefault(doctype => doctype.GUID == fromWORKPACK.GUID_DDOCTYPE);
            }
            else
            {
                findAREA = fromWORKPACK.AREA;
                findDISCIPLINE = fromWORKPACK.DISCIPLINE;
                findDOCTYPE = fromWORKPACK.DOCTYPE;
            }

            if (findAREA != null && findDISCIPLINE != null && findDOCTYPE != null)
            {
                string InternalName = fromPROJECT.NUMBER;
                InternalName += "-" + findAREA.INTERNAL_NUM;
                InternalName += findDOCTYPE.CODE;
                InternalName += findDISCIPLINE.CODE;

                int InternalNameCount = WORKPACKEntities.Count(obj => obj.INTERNAL_NAME1 != null && obj.INTERNAL_NAME1.Contains(InternalName)) + 1;

                InternalName += InternalNameCount.ToString();

                return InternalName;
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Generate internal number2 when all required fields are populated
        /// </summary>
        public static string WORKPACK_Generate_InternalNumber2(PROJECT fromPROJECT, WORKPACK fromWORKPACK, IEnumerable<WORKPACK> WORKPACKEntities, IEntitiesViewModel<AREA> lookUpAREA, IEntitiesViewModel<DISCIPLINE> lookUpDISCIPLINE, IEntitiesViewModel<PHASE> lookUpPHASE)
        {
            AREA findAREA;
            DISCIPLINE findDISCIPLINE;
            PHASE findPHASE;

            if (fromWORKPACK.AREA == null || fromWORKPACK.DISCIPLINE == null || fromWORKPACK.AREA == null)
            {
                findAREA = lookUpAREA.Entities.FirstOrDefault(area => area.GUID == fromWORKPACK.GUID_DAREA);
                findPHASE = lookUpPHASE.Entities.FirstOrDefault(phase => phase.GUID == fromWORKPACK.GUID_DPHASE);
                findDISCIPLINE = lookUpDISCIPLINE.Entities.FirstOrDefault(discipline => discipline.GUID == fromWORKPACK.GUID_DDISCIPLINE);
            }
            else
            {
                findAREA = fromWORKPACK.AREA;
                findPHASE = fromWORKPACK.PHASE;
                findDISCIPLINE = fromWORKPACK.DISCIPLINE;
            }

            if (findAREA != null && findDISCIPLINE != null && findPHASE != null)
            {
                string InternalName = fromPROJECT.NUMBER;
                InternalName += "-" + findPHASE.INTERNAL_NUM;
                InternalName += findAREA.INTERNAL_NUM;
                InternalName += findDISCIPLINE.CODE;

                int InternalNameCount = WORKPACKEntities.Count(obj => obj.INTERNAL_NAME2 != null && obj.INTERNAL_NAME2.Contains(InternalName)) + 1;

                InternalName += InternalNameCount.ToString();

                return InternalName;
            }
            else
                return string.Empty;
        }
    }
}
