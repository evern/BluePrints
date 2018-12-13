using BluePrints.Common.ViewModel.Reporting;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;

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
            var replaceTo = "Units";

            foreach (var info in e.ParametersInformation)
                if (info.Parameter.Name == "reportBy")
                    replaceTo = (string) info.Parameter.Value;

            string strReplaceFrom;
            string strReplaceTo;
            string formatString;
            if (replaceTo == "Costs")
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

            string percentageFormatString = "{0:0.00%}";

            replaceDataMember(xrDataSummaryCumulativeEarnedPercent, strReplaceFrom, strReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryCumulativePlannedPercent, strReplaceFrom, strReplaceTo, percentageFormatString);
            //replaceDataMember(xrDataSummaryCumulativeBurnedPercent, strReplaceFrom, strReplaceTo, percentageFormatString);

            replaceDataMember(xrDataSummaryPeriodEarnedPercent, strReplaceFrom, strReplaceTo, percentageFormatString);
            replaceDataMember(xrDataSummaryPeriodPlannedPercent, strReplaceFrom, strReplaceTo, percentageFormatString);
            //replaceDataMember(xrDataSummaryPeriodBurnedPercent, strReplaceFrom, strReplaceTo, percentageFormatString);

            replaceDataMember(xrDataBaselineBudgeted, strReplaceFrom, strReplaceTo, formatString);

            replaceDataMember(xrDataCumulativePlannedUOM, strReplaceFrom, strReplaceTo, formatString);
            replaceDataMember(xrDataCumulativePlannedPercentage, strReplaceFrom, strReplaceTo, percentageFormatString);

            replaceDataMember(xrDataCumulativeEarnedUOM, strReplaceFrom, strReplaceTo, formatString);
            replaceDataMember(xrDataCumulativeEarnedPercentage, strReplaceFrom, strReplaceTo, percentageFormatString);

            replaceDataMember(xrDataPeriodPlannedUOM, strReplaceFrom, strReplaceTo, formatString);
            replaceDataMember(xrDataPeriodPlannedPercentage, strReplaceFrom, strReplaceTo, percentageFormatString);

            replaceDataMember(xrDataPeriodCurrentUOM, strReplaceFrom, strReplaceTo, formatString);
            replaceDataMember(xrDataPeriodCurrentPercentage, strReplaceFrom, strReplaceTo, percentageFormatString);


            xrChart1.Series["Planned"].ValueDataMembersSerializable = xrChart1.Series["Planned"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);
            xrChart1.Series["Earned"].ValueDataMembersSerializable = xrChart1.Series["Earned"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);
            xrChart1.Series["Burned"].ValueDataMembersSerializable = xrChart1.Series["Burned"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);
            xrChart1.Series["Remaining"].ValueDataMembersSerializable = xrChart1.Series["Remaining"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);

            xrChart1.Series["Period Planned"].ValueDataMembersSerializable = xrChart1.Series["Period Planned"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);
            xrChart1.Series["Period Earned"].ValueDataMembersSerializable = xrChart1.Series["Period Earned"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);
            xrChart1.Series["Period Burned"].ValueDataMembersSerializable = xrChart1.Series["Period Burned"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);
            xrChart1.Series["Period Remaining"].ValueDataMembersSerializable = xrChart1.Series["Period Remaining"].ValueDataMembersSerializable.Replace(strReplaceFrom, strReplaceTo);

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

        private void replaceDataMember(XRLabel label, string replaceFrom, string replaceTo, string formatString)
        {
            string propertyName = string.Empty;
            propertyName = label.DataBindings[0].DataMember;
            propertyName = propertyName.Replace(replaceFrom, replaceTo);
            label.DataBindings.Clear();
            label.DataBindings.Add(new XRBinding("Text", objectDataSource1,
                propertyName, formatString));
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