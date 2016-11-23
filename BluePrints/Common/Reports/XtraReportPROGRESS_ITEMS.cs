using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraSplashScreen;
using BluePrints.Common.ViewModel.Reporting;

namespace BluePrints.Reports
{
    public partial class XtraReportPROGRESS_ITEMS : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportPROGRESS_ITEMS()
        {
            InitializeComponent();
            this.ParametersRequestSubmit += rptProgressItem_ParametersRequestSubmit;
        }

        void rptProgressItem_ParametersRequestSubmit(object sender, ParametersRequestEventArgs e)
        {
            string reportBy = "Units";

            foreach (ParameterInfo info in e.ParametersInformation)
            {
                if (info.Parameter.Name == "reportBy")
                    reportBy = (string)info.Parameter.Value;
            }

            xrDataSummaryCumulativeBurnedPercent.DataBindings.Clear();
            xrDataSummaryCumulativeEarnedPercent.DataBindings.Clear();
            xrDataSummaryCumulativePlannedPercent.DataBindings.Clear();
            xrDataSummaryPeriodBurnedPercent.DataBindings.Clear();
            xrDataSummaryPeriodEarnedPercent.DataBindings.Clear();
            xrDataSummaryPeriodPlannedPercent.DataBindings.Clear();

            xrDataBaselineBudgeted.DataBindings.Clear();
            xrDataCumulativePlannedUOM.DataBindings.Clear();
            xrDataCumulativePlannedPercentage.DataBindings.Clear();
            xrDataCumulativeEarnedUOM.DataBindings.Clear();
            xrDataCumulativeEarnedPercentage.DataBindings.Clear();
            xrDataPeriodPlannedUOM.DataBindings.Clear();
            xrDataPeriodPlannedPercentage.DataBindings.Clear();
            xrDataPeriodCurrentUOM.DataBindings.Clear();
            xrDataPeriodCurrentPercentage.DataBindings.Clear();

            string strReplaceFrom;
            string strReplaceTo;
            string formatString;
            if (reportBy == "Costs")
            {
                strReplaceFrom = "Units";
                strReplaceTo = "Costs";
                formatString = "{0:c}";
            }
            else
            {
                strReplaceFrom = "Costs";
                strReplaceTo = "Units";
                formatString = "{0:n1}";
            }



            xrDataSummaryCumulativeBurnedPercent.DataBindings.Add(new XRBinding("Text", objectDataSource1, "Summary_CumulativeBurned." + reportBy + "Percentage", "{0:0.00%}"));
            xrDataSummaryCumulativeEarnedPercent.DataBindings.Add(new XRBinding("Text", objectDataSource1, "Summary_CumulativeEarned." + reportBy + "Percentage", "{0:0.00%}"));
            xrDataSummaryCumulativePlannedPercent.DataBindings.Add(new XRBinding("Text", objectDataSource1, "Summary_CumulativePlanned." + reportBy + "Percentage", "{0:0.00%}"));
            xrDataSummaryPeriodBurnedPercent.DataBindings.Add(new XRBinding("Text", objectDataSource1, "Summary_PeriodBurned." + reportBy + "Percentage", "{0:0.00%}"));
            xrDataSummaryPeriodEarnedPercent.DataBindings.Add(new XRBinding("Text", objectDataSource1, "Summary_PeriodEarned." + reportBy + "Percentage", "{0:0.00%}"));
            xrDataSummaryPeriodPlannedPercent.DataBindings.Add(new XRBinding("Text", objectDataSource1, "Summary_PeriodPlanned." + reportBy + "Percentage", "{0:0.00%}"));

            if(reportBy == "Units")
                xrDataBaselineBudgeted.DataBindings.Add(new XRBinding("Text", objectDataSource1, "ReportableObjects.BASELINE_ITEMJoinRATE.BASELINE_ITEM.ESTIMATED_HOURS", formatString));
            else
                xrDataBaselineBudgeted.DataBindings.Add(new XRBinding("Text", objectDataSource1, "ReportableObjects.BASELINE_ITEMJoinRATE.ESTIMATED_COSTS", formatString));

            xrDataCumulativePlannedUOM.DataBindings.Add(new XRBinding("Text", objectDataSource1, "ReportableObjects.Summary_CumulativeOriginal." + reportBy, formatString));
            xrDataCumulativePlannedPercentage.DataBindings.Add(new XRBinding("Text", objectDataSource1, "ReportableObjects.Summary_CumulativeOriginal." + reportBy + "Percentage", "{0:0.00%}"));

            xrDataCumulativeEarnedUOM.DataBindings.Add(new XRBinding("Text", objectDataSource1, "ReportableObjects.Summary_CumulativeEarned.Units", formatString));
            xrDataCumulativeEarnedPercentage.DataBindings.Add(new XRBinding("Text", objectDataSource1, "ReportableObjects.Summary_CumulativeEarned." + reportBy + "Percentage", "{0:0.00%}"));

            xrDataPeriodPlannedUOM.DataBindings.Add(new XRBinding("Text", objectDataSource1, "ReportableObjects.Summary_PeriodPlanned.Units", formatString));
            xrDataPeriodPlannedPercentage.DataBindings.Add(new XRBinding("Text", objectDataSource1, "ReportableObjects.Summary_PeriodPlanned." + reportBy + "Percentage", "{0:0.00%}"));

            xrDataPeriodCurrentUOM.DataBindings.Add(new XRBinding("Text", objectDataSource1, "ReportableObjects.Summary_PeriodEarned.Units", formatString));
            xrDataPeriodCurrentPercentage.DataBindings.Add(new XRBinding("Text", objectDataSource1, "ReportableObjects.Summary_PeriodEarned." + reportBy + "Percentage", "{0:0.00%}"));

            this.xrChart1.Series["Planned"].ValueDataMembersSerializable = "Summary_CumulativePlannedDataPoints." + reportBy + "Percentage";
            this.xrChart1.Series["Earned"].ValueDataMembersSerializable = "Summary_CumulativeEarnedDataPoints." + reportBy + "Percentage";
            this.xrChart1.Series["Burned"].ValueDataMembersSerializable = "Summary_CumulativeBurnedDataPoints." + reportBy + "Percentage";
            this.xrChart1.Series["Remaining"].ValueDataMembersSerializable = "Summary_CumulativeRemainingPlannedDataPoints." + reportBy + "Percentage";

            this.xrChart1.Series["Period Planned"].ValueDataMembersSerializable = "Summary_PeriodPlannedDataPoints." + reportBy;
            this.xrChart1.Series["Period Earned"].ValueDataMembersSerializable = "Summary_PeriodEarnedDataPoints." + reportBy;
            this.xrChart1.Series["Period Burned"].ValueDataMembersSerializable = "Summary_PeriodBurnedDataPoints." + reportBy;
            this.xrChart1.Series["Period Remaining"].ValueDataMembersSerializable = "Summary_PeriodRemainingPlannedDataPoints." + reportBy;

            //labels
            xrLblCumulativeEarnedUOM.Text = xrLblCumulativeEarnedUOM.Text.Replace(strReplaceFrom, strReplaceTo);
            xrLblCumulativePlannedUOM.Text = xrLblCumulativePlannedUOM.Text.Replace(strReplaceFrom, strReplaceTo);
            xrLblPeriodCurrentUOM.Text = xrLblPeriodCurrentUOM.Text.Replace(strReplaceFrom, strReplaceTo);
            xrLblPeriodPlannedUOM.Text = xrLblPeriodPlannedUOM.Text.Replace(strReplaceFrom, strReplaceTo);

            //conditional formatting
            this.ItemCumulativeEarnedEfficiency_Good.Condition = this.ItemCumulativeEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            this.ItemCumulativeEarnedEfficiency_Good.Condition = this.ItemCumulativeEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            this.ItemPeriodEarnedEfficiency_Good.Condition = this.ItemPeriodEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            this.SummaryPeriodBurnedEfficiency_Good.Condition = this.SummaryPeriodBurnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            this.SummaryCumulativeBurnedEfficiency_Good.Condition = this.SummaryCumulativeBurnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            this.SummaryCumulativeEarnedEfficiency_Good.Condition = this.SummaryCumulativeEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            this.SummaryPeriodEarnedEfficiency_Good.Condition = this.SummaryPeriodEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            this.ItemCumulativeEarnedEfficiency_Bad.Condition = this.ItemCumulativeEarnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            this.ItemPeriodEarnedEfficiency_Bad.Condition = this.ItemPeriodEarnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            this.SummaryPeriodBurnedEfficiency_Bad.Condition = this.SummaryPeriodBurnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            this.SummaryCumulativeBurnedEfficiency_Bad.Condition = this.SummaryCumulativeBurnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            this.SummaryCumulativeEarnedEfficiency_Bad.Condition = this.SummaryCumulativeEarnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            this.SummaryPeriodEarnedEfficiency_Bad.Condition = this.SummaryPeriodEarnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
        }

        SummarizableObject ReportData { get; set; }
        public void AssignProperties(SummarizableObject reportData, string title)
        {
            ReportData = reportData;
            objectDataSource1.DataSource = ReportData;
            title1.Value = title;
            datadate1.Value = ReportData.ReportingDataDate;
        }

        private void rptProgressItem_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //objectDataSource1.DataSource = this._ProgressItems;
        }
    }
}
