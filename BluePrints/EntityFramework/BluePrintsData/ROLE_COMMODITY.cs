using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrints.Data
{
    [Table("ROLE_COMMODITY")]
    public partial class ROLE_COMMODITY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ROLE_COMMODITY()
        {
        }

        [Key]
        public Guid GUID { get; set; }

        public Guid GUID_ROLE { get; set; }

        public Guid GUID_COMMODITY { get; set; }

        public DateTime CREATED { get; set; }

        public Guid CREATEDBY { get; set; }

        public DateTime? UPDATED { get; set; }

        public Guid? UPDATEDBY { get; set; }

        public DateTime? DELETED { get; set; }

        public Guid? DELETEDBY { get; set; }

        public virtual DOCTYPE DOCTYPE { get; set; }

        public virtual ROLE ROLE { get; set; }
    }
}
