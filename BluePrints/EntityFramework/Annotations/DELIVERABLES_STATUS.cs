namespace BluePrints.Data
{
    using BaseModel.Attributes;
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using BluePrints.Common.Base;
    using BluePrints.Common.Resources;
    using DevExpress.Mvvm;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DELIVERABLES_STATUS : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DELIVERABLES_STATUS()
        {
            BASELINE_ITEM = new HashSet<BASELINE_ITEM>();
            DSTATUS_DOCTYPE = new HashSet<DSTATUS_DOCTYPE>();
            FOR_DELIVERABLE = true;
            FOR_NCR = true;
            FOR_TASK = true;
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        [NotMapped]
        private IEnumerable<object> multipleAssignedDocTypeObject;

        [NotMapped]
        public object MultipleAssignedDocTypeObject
        {
            get { return multipleAssignedDocTypeObject; }
            set
            {
                if (value != multipleAssignedDocTypeObject)
                {
                    multipleAssignedDocTypeObject = value as IEnumerable<object>;
                }
            }
        }

        public List<DOCTYPE> GetAssignedDocTypes()
        {
            List<DOCTYPE> stagedAssignedDocType;
            if (MultipleAssignedDocTypeObject == null)
                stagedAssignedDocType = null;
            else if (MultipleAssignedDocTypeObject.GetType() == typeof(List<DOCTYPE>))
                stagedAssignedDocType = (List<DOCTYPE>)MultipleAssignedDocTypeObject;
            else
                stagedAssignedDocType = ((List<object>)MultipleAssignedDocTypeObject).Select(x => (DOCTYPE)x).ToList();

            return stagedAssignedDocType;
        }

        [NotMapped]
        public IEnumerable<DOCTYPE> MultipleAssignedDocumentTypes
        {
            get
            {
                if (multipleAssignedDocTypeObject == null)
                    return null;

                return multipleAssignedDocTypeObject.Select(x => (DOCTYPE)x);
            }
        }

        public void SetAssignedDocTypes(IEnumerable<DOCTYPE> DOC_TYPECollection, IEnumerable<DSTATUS_DOCTYPE> STATUS_DOCTYPES)
        {
            MultipleAssignedDocTypeObject = DOC_TYPECollection.Where(docType => STATUS_DOCTYPES.Where(status => status.GUID_STATUS == GUID).Any(status => status.GUID_DOCTYPE == docType.GUID)).ToList();
        }

        public string Office
        {
            get
            {
                if (this.PROJECT == null)
                    return BluePrintsResources.GlobalOffice;

                return this.PROJECT.NUMBER + " " + this.PROJECT.OfficeName;
            }
        }
    }
}