using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using BluePrints.Data;
using System.Collections.Generic;

namespace BluePrints.Common.Reports
{
    public partial class XtraReportConstructionVariation : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportConstructionVariation()
        {
            InitializeComponent();
        }

        private VARIATION_CONSTRUCTION VARIATION_CONSTRUCTION { get; set; }

        public void AssignProperties(PROJECT project, VARIATION_CONSTRUCTION variationConstruction, IEnumerable<VARIATION_CONSTRUCTION_ITEM> variationConstructionItems)
        {
            VARIATION_CONSTRUCTION = variationConstruction;
            objectDataSource1.DataSource = variationConstructionItems;
            Title.Value = project.NAME;
            DocumentNo.Value = variationConstruction.DocumentNumber;
            VariationDate.Value = variationConstruction.REQUEST_DATE;
            RequestedBy.Value = variationConstruction.REQUESTED_BY;
            Client.Value = project.CLIENT;
            Description.Value = variationConstruction.DESCRIPTION;
            VariationNo.Value = variationConstruction.NUMBER;
            Notes.Value = variationConstruction.NOTES;
        }
    }
}
