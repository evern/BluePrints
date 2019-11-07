using BaseModel.Misc;
using BluePrints.Common.Base;
using BluePrints.Common.ViewModel.Reporting;
using BluePrints.Data;
using BluePrints.PrimeroData;
using BluePrints.PrimeroData.PrimeroEntitiesDataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;
using BaseModel.Data.Helpers;
using BluePrints.Common.Resources;
using BaseModel.DataModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.XtraEditors.DXErrorProvider;
using DevExpress.Mvvm;
using BaseModel.Attributes;
using BaseModel.ViewModel.Dialogs;

namespace BluePrints.Common.Projections
{
    //ExoSubJobProjection is not flat so this is created
    [ConstraintAttributes("RESOURCENAME")]
    public class ExoResourceProjection : EntityBase, IGuidEntityKey
    {
        public ExoResourceProjection()
        {
            IsNewRow = true;
        }

        [Key]
        public Guid GUID { get; set; }
        public int? RESOURCE_SEQNO { get; set; }
        public int? RESOURCE_STAFFNO { get; set; }
        public int? STAFFNO { get; set; }

        [Required]
        public string RESOURCENAME { get; set; }
        public string TITLE { get; set; }
        public string DEFAULT_STOCKCODE { get; set; }
        public string SHORTCODE { get; set; }

        [Required]
        public int SECURITYPROFILEID { get; set; }

        public string PAYROLL_ID { get; set; }

        [Required]
        public int USERPROFILEID { get; set; }
        public int? REPORTS_TO_STAFFNO { get; set; }

        public double? SELLPRICE1 { get; set; }
        public int? SALES_GL_CODE { get; set; }
        public int? PURCH_GL_CODE { get; set; }
        public int? COS_GL_CODE { get; set; }

        public double? STDCOST { get; set; }

        [Required]
        public int COSTTYPE { get; set; }

        [Required]
        public int COSTGROUP { get; set; }

        //indicate whether this row is commited to database
        public bool IsNewRow { get; set; }
    }
}
