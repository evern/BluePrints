using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Utils;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class PROJECTTenderProfile : BluePrintsProjectionBase<PROJECT>
    {
        public TENDER_PROFILE TenderProfile { get; set; }
        public IQueryable<BASELINE_ITEM> Deliverables { get; set; }
        public List<TENDER_PROFILE_ITEM> TENDER_PROFILE_ITEMS { get; set; }

        List<BASELINE_ITEM> deliverableList;
        List<BASELINE_ITEM> DeliverableList
        {
            get
            {
                if (deliverableList == null)
                    deliverableList = Deliverables.ToList();

                return deliverableList;
            }
        }

        public void ResetDeliverableList()
        {
            deliverableList = null;
            isUnitsSynced = null;
            isDatesSynced = null;
        }

        public bool IsSynced => IsUnitsSynced == null || IsDatesSynced == null ? false : ((bool)IsUnitsSynced) && ((bool)IsDatesSynced);

        bool? isUnitsSynced = null;
        public bool? IsUnitsSynced
        {
            get
            {
                if (Deliverables != null && TenderProfile != null && Entity != null)
                {
                    if (isUnitsSynced == null)
                    {
                        isUnitsSynced = true;
                        List<BASELINE_ITEM> deliverables = Deliverables.ToList();
                        //when total tender profile item percentage is less than 100% this will show it's not synced
                        //decimal totalUnits = deliverables.Sum(x => x.BUDGET_HOURS);
                        //decimal differences = Math.Round(totalUnits) - Math.Round(TenderProfile.TENDER_HOURS);
                        //differences = Math.Abs(differences);

                        //if (differences > 2)
                        //    isUnitsSynced = false;

                        decimal differences = 0;
                        foreach (BASELINE_ITEM deliverable in deliverables)
                        {
                            if (deliverable.GUID_DEPARTMENT == null || deliverable.GUID_DISCIPLINE == null)
                                continue;

                            TENDER_PROFILE_ITEM findTENDER_PROFILE_ITEM = TENDER_PROFILE_ITEMS.FirstOrDefault(x => x.GUID_DEPARTMENT == deliverable.GUID_DEPARTMENT && x.GUID_DISCIPLINE == deliverable.GUID_DISCIPLINE);
                            if (findTENDER_PROFILE_ITEM == null)
                                isUnitsSynced = false;
                            else if (findTENDER_PROFILE_ITEM.BELLCURVESHAPE != deliverable.BELLCURVESHAPE)
                                isUnitsSynced = false;
                        }

                        foreach (TENDER_PROFILE_ITEM tenderProfileItem in TENDER_PROFILE_ITEMS)
                        {
                            IEnumerable<BASELINE_ITEM> profileItemDeliverables = deliverables.Where(x => x.GUID_DEPARTMENT == tenderProfileItem.GUID_DEPARTMENT && x.GUID_DISCIPLINE == tenderProfileItem.GUID_DISCIPLINE);
                            decimal totalProfileItemDeliverableUnits = profileItemDeliverables.Sum(x => x.BUDGET_HOURS);
                            decimal proRatedTenderProfileItemUnits = tenderProfileItem.HOURS_PERCENTAGE * TenderProfile.TENDER_HOURS;

                            differences = Math.Round(totalProfileItemDeliverableUnits) - Math.Round(proRatedTenderProfileItemUnits);
                            differences = Math.Abs(differences);

                            if (differences > 2)
                                isUnitsSynced = false;
                        }
                    }
                }

                return isUnitsSynced;
            }
        }

        bool? isDatesSynced = null;
        public bool? IsDatesSynced
        {
            get
            {
                if (Deliverables != null && TENDER_PROFILE_ITEMS != null && Entity != null && TenderProfile != null)
                {
                    if(isDatesSynced == null)
                    {
                        isDatesSynced = true;
                        List<BASELINE_ITEM> deliverables = Deliverables.ToList();

                        int durationInDays = BluePrintsDataUtils.GetTenderDuration(Entity);
                        Tuple<DateTime, DateTime> startEndDate = BluePrintsDataUtils.GetTenderStartEndDate(Entity);
                        foreach (TENDER_PROFILE_ITEM tenderProfileItem in TENDER_PROFILE_ITEMS)
                        {
                            Tuple<DateTime, DateTime> tenderStartEndDate = tenderProfileItem.GetProRatedStartEndDate(durationInDays, startEndDate.Item1, startEndDate.Item2);
                            IEnumerable<BASELINE_ITEM> profileItemDeliverables = deliverables.Where(x => x.GUID_DEPARTMENT == tenderProfileItem.GUID_DEPARTMENT && x.GUID_DISCIPLINE == tenderProfileItem.GUID_DISCIPLINE);
                            foreach (BASELINE_ITEM profileItemDeliverable in profileItemDeliverables)
                            {
                                if (profileItemDeliverable.TENDER_START_DATE == null || profileItemDeliverable.TENDER_END_DATE == null)
                                    isDatesSynced = false;
                                else if (((DateTime)profileItemDeliverable.TENDER_START_DATE).Date != tenderStartEndDate.Item1.Date)
                                    isDatesSynced = false;
                                else if (((DateTime)profileItemDeliverable.TENDER_END_DATE).Date != tenderStartEndDate.Item2.Date)
                                    isDatesSynced = false;
                            }
                        }
                    }
                }

                return isDatesSynced;
            }
        }
    }
}
