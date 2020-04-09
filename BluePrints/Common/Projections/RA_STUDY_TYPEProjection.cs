using BaseModel.Attributes;
using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BluePrints.Common.Projections
{
    [ConstraintAttributes("Entity.STUDY_TYPE")]
    [RequiredAttributes("Entity.STUDY_TYPE")]
    public class RA_STUDY_TYPEProjection : BluePrintsProjectionBase<RA_STUDY_TYPE>
    {
        public ObservableCollection<RA_GUIDE_PROMPT> GUIDE_PROMPTS { get; set; }

        public Guid EntityGuid
        {
            get { return Entity.GUID; }
            set { Entity.GUID = value; }
        }
    }

    public static class RA_STUDY_TYPEProjectionQueries
    {
        public static IQueryable<RA_STUDY_TYPEProjection> RA_STUDY_TYPEProjection(
            IQueryable<RA_STUDY_TYPE> STUDY_TYPES, IEnumerable<RA_GUIDE_PROMPT> ALL_GUIDE_PROMPTS)
        {
            return STUDY_TYPES.ToArray().Select(x => new RA_STUDY_TYPEProjection()
            {
                Entity = x,
                GUIDE_PROMPTS = new ObservableCollection<RA_GUIDE_PROMPT>(ALL_GUIDE_PROMPTS.Where(z => z.GUID_STUDY_TYPE == x.GUID)),
            }).AsQueryable();
        }
    }
}
