using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.DataModel;
using BluePrints.Common.ViewModel;
using BluePrints.Data;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Common.Projections
{
    public class VARIATION_ITEMProjection : PROGRESS_ITEMProjection
    {
        public VARIATION_ITEMProjection()
        {
            VARIATION_ITEM = new VARIATION_ITEM();
            BASELINE_ITEMJoinRATE = new BASELINE_ITEMProjection();
            VARIATION_ITEM.ACTION = VariationAction.NoAction;
        }

        VARIATION_ITEM variation_item { get; set; }
        public VARIATION_ITEM VARIATION_ITEM
        {
            get { return variation_item; }
            set
            {
                if (value == null)
                    return;
                else
                    variation_item = value;
            }
        }

        public bool ISLOCKED { get; set; }

        public bool ISREADONLY
        {
            get 
            {
                if (ISLOCKED == true)
                    return true;

                if (VARIATION_ITEM.ACTION != VariationAction.Add)
                    return true;

                return false;
            }
        }

        public bool CANTOGGLECANCELLATION
        {
            get { return !ISLOCKED && VARIATION_ITEM.ACTION != VariationAction.Add; }
        }
    }

    public static class VARIATION_ITEMProjectionQuery
    {
        public static IQueryable<VARIATION_ITEMProjection> JoinRATESAndPROGRESS_ITEMSAndVARIATION_ITEMSOnBASELINE_ITEMS(IQueryable<BASELINE_ITEM> BASELINE_ITEMS, Func<PROGRESS> getPROGRESSFunc, Func<BASELINE> getBASELINEFunc, Func<VARIATION> getVARIATIONFunc, Func<IQueryable<PROGRESS_ITEM>> getPROGRESS_ITEMSFunc, Func<IQueryable<VARIATION_ITEM>> getVARIATION_ITEMSFunc, Func<IQueryable<RATE>> getRATESFunc, bool IsLocked)
        {
            BASELINE BASELINE = getBASELINEFunc();
            PROGRESS PROGRESS = getPROGRESSFunc();
            VARIATION VARIATION = getVARIATIONFunc();
            IQueryable<RATE> RATES = getRATESFunc();

            IQueryable<VARIATION_ITEM> LoadVARIATION_ITEMS;
            if (VARIATION == null)
                LoadVARIATION_ITEMS = getVARIATION_ITEMSFunc().Where(x => x.GUID_VARIATION == Guid.Empty).ToArray().AsQueryable();
            else
                LoadVARIATION_ITEMS = getVARIATION_ITEMSFunc().ToArray().AsQueryable();

            IQueryable<PROGRESS_ITEM> LoadPROGRESS_ITEMS;
            if (PROGRESS == null)
                LoadPROGRESS_ITEMS = getPROGRESS_ITEMSFunc().Where(x => x.GUID_PROGRESS == Guid.Empty).ToArray().AsQueryable();
            else
                LoadPROGRESS_ITEMS = getPROGRESS_ITEMSFunc().ToArray().AsQueryable();

            IQueryable<PROGRESS_ITEMProjection> BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS;
            if (PROGRESS == null || VARIATION == null)
                BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS = PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(BASELINE_ITEMS.Where(x => x.GUID == Guid.Empty), getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc, true);
            else
            {
                if(VARIATION.SUBMITTED != null)
                    BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS = PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(BASELINE_ITEMS.Where(x => x.GUID_VARIATION == VARIATION.GUID && x.GUID_BASELINE != null), getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc, true);
                else
                    BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS = PROGRESS_ITEMProjectionQueries.JoinRATESAndPROGRESS_ITEMSOnBASELINE_ITEMS(BASELINE_ITEMS.Where(x => x.GUID_BASELINE == BASELINE.GUID || (x.GUID_VARIATION == VARIATION.GUID && x.GUID_BASELINE == null)), getPROGRESSFunc, getBASELINEFunc, getPROGRESS_ITEMSFunc, getRATESFunc, true);
            }

            return BASELINE_ITEMJoinRATESJoinPROGRESS_ITEMS.ToArray().AsQueryable().Select(x => new VARIATION_ITEMProjection() 
            {   GUID = x.GUID,
                VARIATION_ITEM = LoadVARIATION_ITEMS.Where(y => y.GUID_ORIBASEITEM == x.BASELINE_ITEMJoinRATE.BASELINE_ITEM.GUID_ORIGINAL).FirstOrDefault(),
                BASELINE_ITEMJoinRATE = x.BASELINE_ITEMJoinRATE,
                ISLOCKED = IsLocked
            });
        }
    }
}
