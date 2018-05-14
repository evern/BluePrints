namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.Misc;
    using DevExpress.Mvvm;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using BluePrints.Common.Base;
    
    public partial class RA_STUDY_DATA : BluePrintsEntityBase, IGuidEntityKey, IHaveCreatedDate
    {
        [NotMapped]
        public Guid EntityKey
        {
            get
            {
                return GUID;
            }

            set
            {
                GUID = value;
            }
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        public Guid? GuideSubPromptId
        {
            get { return GUID_GUIDE_SUBPROMPT; }
            set
            {
                if (value == null || GuideSubPrompts.Any(x => x.EntityKey.ToString().ToUpper() == value.ToString().ToUpper()))
                    GUID_GUIDE_SUBPROMPT = value;
            }
        }

        [NotMapped]
        private IEnumerable<RA_GUIDE_SUBPROMPT> guideSubPrompts;
        public IEnumerable<RA_GUIDE_SUBPROMPT> GuideSubPrompts
        {
            get
            {
                if (guideSubPrompts != null)
                    return guideSubPrompts.Where(x => x.GUID_GUIDE_PROMPT == GUID_GUIDE_PROMPT);

                return null;
            }
        }

        public void SetGuideSubPrompts(IEnumerable<RA_GUIDE_SUBPROMPT> guideSubPrompts)
        {
            this.guideSubPrompts = guideSubPrompts;
        }
    }
}