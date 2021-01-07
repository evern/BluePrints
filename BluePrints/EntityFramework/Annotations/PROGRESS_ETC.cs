namespace BluePrints.Data
{
    using BaseModel.DataModel;
    using BaseModel.Misc;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PROGRESS_ETC : EntityBase, IGuidEntityKey, ICanSync, IHaveCreatedDate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PROGRESS_ETC()
        {
        }

        [NotMapped]
        public DateTime EntityCreatedDate
        {
            get { return CREATED; }
            set { CREATED = value; }
        }

        public string Office => this.PROGRESS.PROJECT.NUMBER + " " + this.PROGRESS.PROJECT.OfficeName;

        Guid IGuidEntityKey.GUID { get => GUID; set => GUID = value; }
    }
}