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
    public partial class XtraReportPROGRESS_ITEMS : XtraReport
    {
        public XtraReportPROGRESS_ITEMS()
        {
            InitializeComponent();
            ParametersRequestSubmit += rptProgressItem_ParametersRequestSubmit;
        }

        private void rptProgressItem_ParametersRequestSubmit(object sender, ParametersRequestEventArgs e)
        {
            var reportBy = "Units";

            foreach (var info in e.ParametersInformation)
                if (info.Parameter.Name == "reportBy")
                    reportBy = (string) info.Parameter.Value;

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


            xrDataSummaryCumulativeBurnedPercent.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "Summary_CumulativeBurned." + reportBy + "Percentage", "{0:0.00%}"));
            xrDataSummaryCumulativeEarnedPercent.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "Summary_CumulativeEarned." + reportBy + "Percentage", "{0:0.00%}"));
            xrDataSummaryCumulativePlannedPercent.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "Summary_CumulativePlanned." + reportBy + "Percentage", "{0:0.00%}"));
            xrDataSummaryPeriodBurnedPercent.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "Summary_PeriodBurned." + reportBy + "Percentage", "{0:0.00%}"));
            xrDataSummaryPeriodEarnedPercent.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "Summary_PeriodEarned." + reportBy + "Percentage", "{0:0.00%}"));
            xrDataSummaryPeriodPlannedPercent.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "Summary_PeriodPlanned." + reportBy + "Percentage", "{0:0.00%}"));

            if (reportBy == "Units")
                xrDataBaselineBudgeted.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                    "ReportableObjects.BASELINE_ITEMJoinRATE.BASELINE_ITEM.ESTIMATED_HOURS", formatString));
            else
                xrDataBaselineBudgeted.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                    "ReportableObjects.BASELINE_ITEMJoinRATE.ESTIMATED_COSTS", formatString));

            xrDataCumulativePlannedUOM.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "ReportableObjects.Summary_CumulativeOriginal." + reportBy, formatString));
            xrDataCumulativePlannedPercentage.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "ReportableObjects.Summary_CumulativeOriginal." + reportBy + "Percentage", "{0:0.00%}"));

            xrDataCumulativeEarnedUOM.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "ReportableObjects.Summary_CumulativeEarned." + strReplaceTo, formatString));

            xrDataCumulativeEarnedPercentage.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "ReportableObjects.Summary_CumulativeEarned." + reportBy + "Percentage", "{0:0.00%}"));

            xrDataPeriodPlannedUOM.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "ReportableObjects.Summary_PeriodPlanned." + strReplaceTo, formatString));

            xrDataPeriodPlannedPercentage.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "ReportableObjects.Summary_PeriodPlanned." + reportBy + "Percentage", "{0:0.00%}"));

            xrDataPeriodCurrentUOM.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "ReportableObjects.Summary_PeriodEarned." + strReplaceTo, formatString));

            xrDataPeriodCurrentPercentage.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                "ReportableObjects.Summary_PeriodEarned." + reportBy + "Percentage", "{0:0.00%}"));

            xrChart1.Series["Planned"].ValueDataMembersSerializable = "Stats.Current.CumulativeDataPoints." +
                                                                           reportBy + "Percentage";
            xrChart1.Series["Earned"].ValueDataMembersSerializable = "Stats.Earned.CumulativeDataPoints." +
                                                                          reportBy + "Percentage";
            xrChart1.Series["Burned"].ValueDataMembersSerializable = "Stats.Burned.CumulativeDataPoints." +
                                                                          reportBy + "Percentage";
            xrChart1.Series["Remaining"].ValueDataMembersSerializable =
                "Stats.Remaining.CumulativeDataPoints." + reportBy + "Percentage";

            xrChart1.Series["Period Planned"].ValueDataMembersSerializable = "Stats.Current.DataPoints." +
                                                                                  reportBy;
            xrChart1.Series["Period Earned"].ValueDataMembersSerializable = "Stats.Earned.DataPoints." +
                                                                                 reportBy;
            xrChart1.Series["Period Burned"].ValueDataMembersSerializable = "Stats.Burned.DataPoints." +
                                                                                 reportBy;
            xrChart1.Series["Period Remaining"].ValueDataMembersSerializable =
                "Stats.Remaining.DataPoints." + reportBy;

            //labels
            xrLblCumulativeEarnedUOM.Text = xrLblCumulativeEarnedUOM.Text.Replace(strReplaceFrom, strReplaceTo);
            xrLblCumulativePlannedUOM.Text = xrLblCumulativePlannedUOM.Text.Replace(strReplaceFrom, strReplaceTo);
            xrLblPeriodCurrentUOM.Text = xrLblPeriodCurrentUOM.Text.Replace(strReplaceFrom, strReplaceTo);
            xrLblPeriodPlannedUOM.Text = xrLblPeriodPlannedUOM.Text.Replace(strReplaceFrom, strReplaceTo);

            //conditional formatting
            ItemCumulativeEarnedEfficiency_Good.Condition =
                ItemCumulativeEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            ItemCumulativeEarnedEfficiency_Good.Condition =
                ItemCumulativeEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            ItemPeriodEarnedEfficiency_Good.Condition =
                ItemPeriodEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryPeriodBurnedEfficiency_Good.Condition =
                SummaryPeriodBurnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryCumulativeBurnedEfficiency_Good.Condition =
                SummaryCumulativeBurnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryCumulativeEarnedEfficiency_Good.Condition =
                SummaryCumulativeEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryPeriodEarnedEfficiency_Good.Condition =
                SummaryPeriodEarnedEfficiency_Good.Condition.Replace(strReplaceFrom, strReplaceTo);
            ItemCumulativeEarnedEfficiency_Bad.Condition =
                ItemCumulativeEarnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            ItemPeriodEarnedEfficiency_Bad.Condition =
                ItemPeriodEarnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryPeriodBurnedEfficiency_Bad.Condition =
                SummaryPeriodBurnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryCumulativeBurnedEfficiency_Bad.Condition =
                SummaryCumulativeBurnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryCumulativeEarnedEfficiency_Bad.Condition =
                SummaryCumulativeEarnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
            SummaryPeriodEarnedEfficiency_Bad.Condition =
                SummaryPeriodEarnedEfficiency_Bad.Condition.Replace(strReplaceFrom, strReplaceTo);
        }

        private SummaryStats ReportData { get; set; }

        public void AssignProperties(SummaryStats reportData, DateTime reportingDataDate, string title)
        {
            ReportData = reportData;
            objectDataSource1.DataSource = ReportData;
            title1.Value = title;
            datadate1.Value = reportingDataDate;
        }

        private void rptProgressItem_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            //objectDataSource1.DataSource = this._ProgressItems;
        }
    }
}