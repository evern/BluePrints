using BluePrints.BluePrintsEntitiesDataModel;
using BluePrints.Common.Base;
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
        public List<TENDER_PROFILE_ITEM> TENDER_PROFILE_ITEMS { get; set; }
        public IBluePrintsEntitiesUnitOfWork BluePrintsEntitiesUnitOfWork { get; set; }
    }
}
