namespace BluePrints.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using Common;
    using Attributes;
    using System.ComponentModel;
    using Common.ViewModel;

    [ConstraintAttributes("NUMBER")]
    public partial class PROJECT : IHaveGUID
    {
        public PROJECT()
        {
            AREA = new HashSet<AREA>();
            BASELINE = new HashSet<BASELINE>();
            COMMODITY_CODE = new HashSet<COMMODITY_CODE>();
            ESTIMATION_DIRECT = new HashSet<ESTIMATION_DIRECT>();
            ESTIMATION_INDIRECT = new HashSet<ESTIMATION_INDIRECT>();
            ESTIMATION_SETTING = new HashSet<ESTIMATION_SETTING>();
            PHASE = new HashSet<PHASE>();
            PROGRESS = new HashSet<PROGRESS>();
            REGISTER = new HashSet<REGISTER>();
            PROJECT_REPORT = new HashSet<PROJECT_REPORT>();
            RATE = new HashSet<RATE>();
            VARIATION = new HashSet<VARIATION>();
            WORKPACK = new HashSet<WORKPACK>();

            STATUS = ProjectStatus.Active;
            CONTRACTTYPE = ContractType.LumpSum;
            USELEGACYWORKPACK = true;
            CURRENCYCONVERSION = 1;
            REVIEWPERCENTAGE = 0.7M;
            REVIEWPERIOD = 5;
        }
    }
}