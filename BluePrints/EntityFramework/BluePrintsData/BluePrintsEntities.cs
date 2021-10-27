namespace BluePrints.Data
{
    using EntityFramework.Functions;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;

    //dbcontext free of interceptors
    public class BluePrintsNativeEntities : DbContext
    {
        public BluePrintsNativeEntities()
            : base("name=BluePrintsEntities")
        {
        }

        public BluePrintsNativeEntities(string configString)
            : base(configString)
        {
        }

        public virtual DbSet<AREA> AREA { get; set; }
        public virtual DbSet<BASELINE> BASELINE { get; set; }
        public virtual DbSet<BASELINE_ITEM> BASELINE_ITEM { get; set; }
        public virtual DbSet<BASELINE_ITEM_WORK> BASELINE_ITEM_WORK { get; set; }
        public virtual DbSet<CLIENT> CLIENT { get; set; }
        public virtual DbSet<COMMODITY_CODE> COMMODITY_CODE { get; set; }
        public virtual DbSet<CONSTRUCTION_CONFIG> CONSTRUCTION_CONFIG { get; set; }
        public virtual DbSet<DataPoint> DataPoint { get; set; }
        public virtual DbSet<DAYWORK> DAYWORK { get; set; }
        public virtual DbSet<DAYWORK_EQUIPMENT> DAYWORK_EQUIPMENT { get; set; }
        public virtual DbSet<DAYWORK_MATERIAL> DAYWORK_MATERIAL { get; set; }
        public virtual DbSet<DAYWORK_LABOUR> DAYWORK_LABOUR { get; set; }
        public virtual DbSet<DAYWORK_STAFF_ROLE> DAYWORK_STAFF_ROLE { get; set; }
        public virtual DbSet<DELIVERABLES_STATUS> DELIVERABLES_STATUS { get; set; }
        public virtual DbSet<DEPARTMENT> DEPARTMENT { get; set; }
        public virtual DbSet<DISCIPLINE> DISCIPLINE { get; set; }
        public virtual DbSet<DISCIPLINE_DESC> DISCIPLINE_DESC { get; set; }
        public virtual DbSet<DOCTYPE> DOCTYPE { get; set; }
        public virtual DbSet<ESTIMATE> ESTIMATE { get; set; }
        public virtual DbSet<ESTIMATE_ITEM> ESTIMATE_ITEM { get; set; }
        public virtual DbSet<FORECAST_PO> FORECAST_PO { get; set; }
        public virtual DbSet<FORECAST_JOB> FORECAST_JOB { get; set; }
        public virtual DbSet<FORECAST_JOB_SETTING> FORECAST_JOB_SETTING { get; set; }
        public virtual DbSet<FORECAST> FORECAST { get; set; }
        public virtual DbSet<FORECAST_EAC> FORECAST_EAC { get; set; }
        public virtual DbSet<FORECAST_HISTORY> FORECAST_HISTORY { get; set; }
        public virtual DbSet<FORECAST_JOB_HOUR_SNAPSHOT> FORECAST_JOB_HOUR_SNAPSHOT { get; set; }
        public virtual DbSet<JOBCOST_LINES_AUDIT> JOBCOST_LINES_AUDIT { get; set; }
        public virtual DbSet<HSE> HSE { get; set; }
        public virtual DbSet<HSE_INCIDENT> HSE_INCIDENT { get; set; }
        public virtual DbSet<HSE_INJURY> HSE_INJURY { get; set; }
        public virtual DbSet<HOLIDAY> HOLIDAY { get; set; }
        public virtual DbSet<MEETING> MEETING { get; set; }
        public virtual DbSet<MEETING_USER> MEETING_USER { get; set; }
        public virtual DbSet<MINUTE_AGENDA> MINUTE_AGENDA { get; set; }
        public virtual DbSet<MINUTE_TITLE> MINUTE_TITLE { get; set; }
        public virtual DbSet<P6_ASSIGNMENT> P6_ASSIGNMENT { get; set; }
        public virtual DbSet<PHASE> PHASE { get; set; }
        public virtual DbSet<PIPELINE> PIPELINE { get; set; }
        public virtual DbSet<PROGRESS> PROGRESS { get; set; }
        public virtual DbSet<PROGRESS_ETC> PROGRESS_ETC { get; set; }
        public virtual DbSet<PROGRESS_ITEM> PROGRESS_ITEM { get; set; }
        public virtual DbSet<PROJECT> PROJECT { get; set; }
        public virtual DbSet<PROJECT_REVENUE> PROJECT_REVENUE { get; set; }
        public virtual DbSet<PROJECT_SUMMARY> PROJECT_SUMMARY { get; set; }
        public virtual DbSet<PROJECT_SUMMARY_SETTING> PROJECT_SUMMARY_SETTING { get; set; }
        public virtual DbSet<PROJECT_DISCIPLINE> PROJECT_DISCIPLINE { get; set; }
        public virtual DbSet<PROJECT_PERMISSION> PROJECT_PERMISSION { get; set; }
        public virtual DbSet<PROJECT_REPORT> PROJECT_REPORT { get; set; }
        public virtual DbSet<RA_GUIDE_PROMPT> RA_GUIDE_PROMPT { get; set; }
        public virtual DbSet<RA_GUIDE_SUBPROMPT> RA_GUIDE_SUBPROMPT { get; set; }
        public virtual DbSet<RA_STUDY> RA_STUDY { get; set; }
        public virtual DbSet<RA_STUDY_DATA> RA_STUDY_DATA { get; set; }
        public virtual DbSet<RA_STUDY_DRAWING> RA_STUDY_DRAWING { get; set; }
        public virtual DbSet<RA_STUDY_NODE> RA_STUDY_NODE { get; set; }
        public virtual DbSet<RA_STUDY_TYPE> RA_STUDY_TYPE { get; set; }
        public virtual DbSet<RA_STUDY_TEAM> RA_STUDY_TEAM { get; set; }
        public virtual DbSet<RATE> RATE { get; set; }
        public virtual DbSet<REGISTER> REGISTER { get; set; }
        public virtual DbSet<REGISTER_CHANGE> REGISTER_CHANGE { get; set; }
        public virtual DbSet<REGISTER_HOLD> REGISTER_HOLD { get; set; }
        public virtual DbSet<REGISTER_HOLD_REF> REGISTER_HOLD_REF { get; set; }
        public virtual DbSet<REGISTER_ISSUE> REGISTER_ISSUE { get; set; }
        public virtual DbSet<REGISTER_LL> REGISTER_LL { get; set; }
        public virtual DbSet<REGISTER_NC> REGISTER_NC { get; set; }
        public virtual DbSet<REGISTER_RISK> REGISTER_RISK { get; set; }
        public virtual DbSet<ROLE> ROLE { get; set; }
        public virtual DbSet<ROLE_PERMISSION> ROLE_PERMISSION { get; set; }
        public virtual DbSet<ROLE_COMMODITY> ROLE_COMMODITY { get; set; }
        public virtual DbSet<ROSTER_STAFF> ROSTER_STAFF { get; set; }
        public virtual DbSet<ROSTER_STAFF_STATUS> ROSTER_STAFF_STATUS { get; set; }
        public virtual DbSet<SETTINGS_GLOBAL> SETTINGS_GLOBAL { get; set; }
        public virtual DbSet<UOM> UOM { get; set; }
        public virtual DbSet<USER> USER { get; set; }
        public virtual DbSet<USER_PREFERENCE> USER_PREFERENCE { get; set; }
        public virtual DbSet<VARIATION> VARIATION { get; set; }
        public virtual DbSet<VARIATION_ITEM> VARIATION_ITEM { get; set; }
        public virtual DbSet<SUBJOB> SUBJOB { get; set; }
        public virtual DbSet<SUBJOB_ASSIGNMENT> SUBJOB_ASSIGNMENT { get; set; }
        public virtual DbSet<VARIATION_CONSTRUCTION> VARIATION_CONSTRUCTION { get; set; }
        public virtual DbSet<VARIATION_CONSTRUCTION_ITEM> VARIATION_CONSTRUCTION_ITEM { get; set; }
        public virtual DbSet<VARIATION_CONSTRUCTION_IMPACT> VARIATION_CONSTRUCTION_IMPACT { get; set; }
        public virtual DbSet<X_VARIATION_QUERY> X_VARIATION_QUERY { get; set; }
        public virtual DbSet<X_EARNED_QUERY> X_EARNED_QUERY { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<BluePrintsNativeEntities>(null);
            Database.SetInitializer<BluePrintsEntities>(null);
            modelBuilder.Conventions.Add(new FunctionConvention<BluePrintsEntities>());
           // modelBuilder.AddComplexTypesFromAssembly(typeof(BluePrintsEntities).Assembly);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            Database.CommandTimeout = 100000;

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.AREA1)
                .WithOptional(e => e.AREA2)
                .HasForeignKey(e => e.GUID_PARENT);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.BASELINE_ITEM1)
                .WithOptional(e => e.AREA1)
                .HasForeignKey(e => e.GUID_SUBAREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.ESTIMATE_ITEM)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.ESTIMATE_ITEM1)
                .WithOptional(e => e.AREA1)
                .HasForeignKey(e => e.GUID_SUBAREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.REGISTER_CHANGE)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.REGISTER_HOLD)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.REGISTER_ISSUE)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.REGISTER_LL)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.REGISTER_NC)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.REGISTER_RISK)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.SUBJOB)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_DAREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.SUBJOB1)
                .WithOptional(e => e.AREA1)
                .HasForeignKey(e => e.GUID_DSUBAREA);

            modelBuilder.Entity<BASELINE>()
                .Property(e => e.ACTUAL_UNITS)
                .HasPrecision(10, 2);

            modelBuilder.Entity<BASELINE>()
                .Property(e => e.BUDGETED_UNITS)
                .HasPrecision(10, 2);

            modelBuilder.Entity<BASELINE>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.BASELINE)
                .HasForeignKey(e => e.GUID_BASELINE);

            modelBuilder.Entity<BASELINE_ITEM>()
                .Property(e => e.P6_ASSIGNMENT_STARTUNIT)
                .HasPrecision(18, 0);

            modelBuilder.Entity<BASELINE_ITEM>()
                .HasMany(e => e.REGISTER_HOLD_REF)
                .WithRequired(e => e.BASELINE_ITEM)
                .HasForeignKey(e => e.GUID_BASELINE_ITEM)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BASELINE_ITEM_WORK>()
                .Property(e => e.WEIGHTING)
                .HasPrecision(5, 2);

            modelBuilder.Entity<CLIENT>()
                .HasMany(e => e.CLIENT_PROJECT)
                .WithRequired(e => e.CLIENT)
                .HasForeignKey(e => e.GUID_CLIENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DELIVERABLES_STATUS>()
                .Property(e => e.MAX_PERCENTAGE)
                .HasPrecision(5, 2);

            modelBuilder.Entity<DELIVERABLES_STATUS>()
                .Property(e => e.AUTO_PERCENTAGE)
                .HasPrecision(5, 2);

            modelBuilder.Entity<DELIVERABLES_STATUS>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.DELIVERABLES_STATUS)
                .HasForeignKey(e => e.GUID_STATUS);

            modelBuilder.Entity<DELIVERABLES_STATUS>()
                .HasMany(e => e.DSTATUS_DOCTYPE)
                .WithRequired(e => e.DELIVERABLES_STATUS)
                .HasForeignKey(e => e.GUID_STATUS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.DEPARTMENT)
                .HasForeignKey(e => e.GUID_DEPARTMENT);

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.TENDER_PROFILE_ITEM)
                .WithRequired(e => e.DEPARTMENT)
                .HasForeignKey(e => e.GUID_DEPARTMENT);

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.DOCTYPE)
                .WithRequired(e => e.DEPARTMENT)
                .HasForeignKey(e => e.GUID_DDEPARTMENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.ESTIMATE_ITEM)
                .WithOptional(e => e.DEPARTMENT)
                .HasForeignKey(e => e.GUID_DEPARTMENT);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.RATE)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.RATE1)
                .WithOptional(e => e.SUBAREA)
                .HasForeignKey(e => e.GUID_SUBAREA);

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.RATE)
                .WithOptional(e => e.DEPARTMENT)
                .HasForeignKey(e => e.GUID_DEPARTMENT);

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.USER)
                .WithOptional(e => e.DEPARTMENT1)
                .HasForeignKey(e => e.GUID_DEPARTMENT);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.COMMODITY_CODE)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.ESTIMATE_ITEM)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.PROJECT_DISCIPLINE)
                .WithRequired(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.RATE)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.REGISTER_ISSUE)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.REGISTER_LL)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.REGISTER_NC)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.USER)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.WORKPACK)
                .WithRequired(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DOCTYPE>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.DOCTYPE)
                .HasForeignKey(e => e.GUID_DOCTYPE);

            modelBuilder.Entity<DOCTYPE>()
                .HasMany(e => e.ROLE_COMMODITY)
                .WithRequired(e => e.DOCTYPE)
                .HasForeignKey(e => e.GUID_COMMODITY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DOCTYPE>()
                .HasMany(e => e.DSTATUS_DOCTYPE)
                .WithOptional(e => e.DOCTYPE)
                .HasForeignKey(e => e.GUID_DOCTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ESTIMATE>()
                .HasMany(e => e.ESTIMATE_ITEM)
                .WithOptional(e => e.ESTIMATE)
                .HasForeignKey(e => e.GUID_ESTIMATE);

            modelBuilder.Entity<FORECAST_JOB>()
                .HasMany(e => e.FORECAST_JOB_HOUR)
                .WithRequired(e => e.FORECAST_JOB)
                .HasForeignKey(e => e.GUID_FORECAST_JOB);

            modelBuilder.Entity<HSE>()
                .HasMany(e => e.HSE_INCIDENT)
                .WithRequired(e => e.HSE)
                .HasForeignKey(e => e.GUID_HSE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<HSE>()
                .HasMany(e => e.HSE_INJURY)
                .WithRequired(e => e.HSE)
                .HasForeignKey(e => e.GUID_HSE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MEETING>()
                .HasMany(e => e.MEETING_USER)
                .WithRequired(e => e.MEETING)
                .HasForeignKey(e => e.GUID_MEETING)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MEETING_ACTION>()
                .HasMany(e => e.MINUTE_AGENDA)
                .WithOptional(e => e.MEETING_ACTION)
                .HasForeignKey(e => e.GUID_ACTION);

            modelBuilder.Entity<MEETING_TYPE>()
                .HasMany(e => e.MEETING)
                .WithRequired(e => e.MEETING_TYPE)
                .HasForeignKey(e => e.GUID_MEETING_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MEETING_TYPE>()
                .HasMany(e => e.MINUTE_TITLE)
                .WithRequired(e => e.MEETING_TYPE)
                .HasForeignKey(e => e.GUID_MEETING_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MINUTE_AGENDA>()
                .HasMany(e => e.MINUTE_AGENDA1)
                .WithOptional(e => e.MINUTE_AGENDA2)
                .HasForeignKey(e => e.GUID_PARENT);

            modelBuilder.Entity<MINUTE_TITLE>()
                .HasMany(e => e.MINUTE_AGENDA)
                .WithOptional(e => e.MINUTE_TITLE)
                .HasForeignKey(e => e.GUID_MINUTE_TITLE);

            modelBuilder.Entity<MINUTE_TITLE>()
                .HasMany(e => e.MINUTE_TITLE1)
                .WithOptional(e => e.MINUTE_TITLE2)
                .HasForeignKey(e => e.GUID_PARENT);

            modelBuilder.Entity<OFFICE>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.OFFICE)
                .HasForeignKey(e => e.GUID_OFFICE);

            modelBuilder.Entity<OFFICE>()
                .HasMany(e => e.PROJECT)
                .WithOptional(e => e.OFFICE)
                .HasForeignKey(e => e.GUID_OFFICE);

            modelBuilder.Entity<OFFICE>()
                .HasMany(e => e.USER)
                .WithOptional(e => e.OFFICE)
                .HasForeignKey(e => e.GUID_OFFICE);

            modelBuilder.Entity<P6_ASSIGNMENT>()
                .Property(e => e.LOW_VALUE)
                .HasPrecision(10, 4);

            modelBuilder.Entity<P6_ASSIGNMENT>()
                .Property(e => e.HIGH_VALUE)
                .HasPrecision(10, 4);

            modelBuilder.Entity<PHASE>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.PHASE)
                .HasForeignKey(e => e.GUID_PHASE);

            modelBuilder.Entity<PHASE>()
                .HasMany(e => e.ESTIMATE_ITEM)
                .WithOptional(e => e.PHASE)
                .HasForeignKey(e => e.GUID_PHASE);

            modelBuilder.Entity<PHASE>()
                .HasMany(e => e.SUBJOB)
                .WithOptional(e => e.PHASE)
                .HasForeignKey(e => e.GUID_DPHASE);

            modelBuilder.Entity<PHASE>()
                .HasMany(e => e.RATE)
                .WithOptional(e => e.PHASE)
                .HasForeignKey(e => e.GUID_PHASE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PHASE>()
                .HasMany(e => e.DELIVERABLES_STATUS)
                .WithOptional(e => e.PHASE)
                .HasForeignKey(e => e.GUID_PHASE);

            modelBuilder.Entity<PIPELINE>()
                .HasMany(e => e.PIPELINE_PROFILE_ITEM)
                .WithRequired(e => e.PIPELINE)
                .HasForeignKey(e => e.GUID_PIPELINE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROGRESS>()
                .HasMany(e => e.PROGRESS_ITEM)
                .WithRequired(e => e.PROGRESS)
                .HasForeignKey(e => e.GUID_PROGRESS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROGRESS>()
                .HasMany(e => e.PROGRESS_ETC)
                .WithRequired(e => e.PROGRESS)
                .HasForeignKey(e => e.GUID_PROGRESS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROGRESS_ITEM>()
                .Property(e => e.EARNED_UNITS)
                .HasPrecision(18, 7);

            modelBuilder.Entity<PROGRESS_ETC>()
                .Property(e => e.ETC_UNITS)
                .HasPrecision(18, 7);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.CURRENCYCONVERSION)
                .HasPrecision(10, 2);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.REVIEWPERCENTAGE)
                .HasPrecision(2, 2);

            modelBuilder.Entity<PROJECT>()
                .Property(e => e.REVIEWPERIOD)
                .HasPrecision(2, 0);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.AREA)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.CONSTRUCTION_STAGE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJECT_REVENUE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.REGISTER_CLARIFICATION)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.BASELINE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.BASELINE_ITEM_WORK)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.CLIENT_PROJECT)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.DAYWORK)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.DAYWORK_EQUIPMENT)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.DAYWORK_LABOUR)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.DAYWORK_MATERIAL)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.FORECAST_JOB_HOUR_SNAPSHOT)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.DAYWORK_STAFF_ROLE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.DELIVERABLES_STATUS)
                .WithOptional(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.DISCIPLINE_DESC)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.ESTIMATE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.FORECAST_EAC)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.FORECAST_HISTORY)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.HOLIDAY)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.HSE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.MEETING)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.MEETING_TYPE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.MINUTE_AGENDA)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.P6_ASSIGNMENT)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROGRESS)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJECT_DISCIPLINE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.REGISTER)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.FORECAST)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJECT_REPORT)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.RA_STUDY)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.RATE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.REGISTER_CHANGE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.TENDER_PROFILE)
                .WithOptional(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.REGISTER_HOLD)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.REGISTER_ISSUE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.REGISTER_LL)
                .WithOptional(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.REGISTER_NC)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.REGISTER_RISK)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.SUBJOB)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.VARIATION)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJECT_SUMMARY)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJECT_SUMMARY_SETTINGS)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.FORECAST_PO)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.FORECAST_PO_SETTING)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.FORECAST_JOB)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.FORECAST_JOB_SETTING)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJECT_PERMISSION)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.VARIATION_CONSTRUCTION)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.REGISTER_TQ)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.REGISTER_TQ)
                .WithRequired(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RA_GUIDE_PROMPT>()
                .HasMany(e => e.RA_GUIDE_SUBPROMPT)
                .WithRequired(e => e.RA_GUIDE_PROMPT)
                .HasForeignKey(e => e.GUID_GUIDE_PROMPT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RA_GUIDE_PROMPT>()
                .HasMany(e => e.RA_STUDY_DATA)
                .WithOptional(e => e.RA_GUIDE_PROMPT)
                .HasForeignKey(e => e.GUID_GUIDE_PROMPT);

            modelBuilder.Entity<RA_GUIDE_SUBPROMPT>()
                .HasMany(e => e.RA_STUDY_DATA)
                .WithOptional(e => e.RA_GUIDE_SUBPROMPT)
                .HasForeignKey(e => e.GUID_GUIDE_SUBPROMPT);

            modelBuilder.Entity<RA_STUDY>()
                .HasMany(e => e.RA_STUDY_DRAWING)
                .WithRequired(e => e.RA_STUDY)
                .HasForeignKey(e => e.GUID_STUDY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RA_STUDY>()
                .HasMany(e => e.RA_STUDY_NODE)
                .WithRequired(e => e.RA_STUDY)
                .HasForeignKey(e => e.GUID_STUDY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RA_STUDY>()
                .HasMany(e => e.RA_STUDY_TEAM)
                .WithRequired(e => e.RA_STUDY)
                .HasForeignKey(e => e.GUID_STUDY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RA_STUDY_DRAWING>()
                .HasMany(e => e.RA_STUDY_NODE)
                .WithOptional(e => e.RA_STUDY_DRAWING)
                .HasForeignKey(e => e.GUID_DRAWING);

            modelBuilder.Entity<RA_STUDY_NODE>()
                .HasMany(e => e.RA_STUDY_DATA)
                .WithRequired(e => e.RA_STUDY_NODE)
                .HasForeignKey(e => e.GUID_NODE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RA_STUDY_TYPE>()
                .HasMany(e => e.RA_GUIDE_PROMPT)
                .WithRequired(e => e.RA_STUDY_TYPE)
                .HasForeignKey(e => e.GUID_STUDY_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RA_STUDY_TYPE>()
                .HasMany(e => e.RA_STUDY)
                .WithRequired(e => e.RA_STUDY_TYPE)
                .HasForeignKey(e => e.GUID_STUDY_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.ROSTER_STAFF)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ROSTER_STAFF>()
                .HasMany(e => e.ROSTER_STAFF_STATUS)
                .WithRequired(e => e.ROSTER_STAFF)
                .HasForeignKey(e => e.GUID_ROSTER_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<REGISTER>()
                .Property(e => e.UNIQUE_H_NUM)
                .IsFixedLength();

            modelBuilder.Entity<REGISTER_TQ>()
                .HasMany(e => e.REGISTER_TQ_ATTACHMENT)
                .WithRequired(e => e.REGISTER_TQ)
                .HasForeignKey(e => e.GUID_REGISTER_TQ)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<REGISTER_CHANGE>()
                .HasMany(e => e.REGISTER_CHANGE_ATTACHMENT)
                .WithRequired(e => e.REGISTER_CHANGE)
                .HasForeignKey(e => e.GUID_REGISTER_CHANGE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<REGISTER_HOLD>()
                .HasMany(e => e.REGISTER_HOLD_REF)
                .WithRequired(e => e.REGISTER_HOLD)
                .HasForeignKey(e => e.GUID_HOLD)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ROLE>()
                .HasMany(e => e.ROLE_COMMODITY)
                .WithRequired(e => e.ROLE)
                .HasForeignKey(e => e.GUID_ROLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ROLE>()
                .HasMany(e => e.ROLE_PERMISSION)
                .WithRequired(e => e.ROLE)
                .HasForeignKey(e => e.GUID_ROLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ROLE>()
                .HasMany(e => e.USER)
                .WithOptional(e => e.ROLE)
                .HasForeignKey(e => e.GUID_ROLE);

            modelBuilder.Entity<SETTINGS_GLOBAL>()
                .Property(e => e.REVIEW_PERCENTAGE)
                .HasPrecision(2, 2);

            modelBuilder.Entity<SETTINGS_GLOBAL>()
                .Property(e => e.REVIEW_PERIOD)
                .HasPrecision(2, 0);

            modelBuilder.Entity<SUBJOB>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.SUBJOB)
                .HasForeignKey(e => e.GUID_SUBJOB);

            modelBuilder.Entity<SUBJOB>()
                .HasMany(e => e.ESTIMATE_ITEM)
                .WithOptional(e => e.SUBJOB)
                .HasForeignKey(e => e.GUID_SUBJOB);

            modelBuilder.Entity<SUBJOB>()
                .HasMany(e => e.ESTIMATE_ITEM1)
                .WithOptional(e => e.SUBJOB1)
                .HasForeignKey(e => e.GUID_PSUBJOB);

            modelBuilder.Entity<SUBJOB>()
                .HasMany(e => e.SUBJOB_ASSIGNMENT)
                .WithRequired(e => e.SUBJOB)
                .HasForeignKey(e => e.GUID_SUBJOB)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SUBJOB>()
                .HasMany(e => e.WORKPACK)
                .WithRequired(e => e.SUBJOB)
                .HasForeignKey(e => e.GUID_SUBJOB)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<SUBJOB_ASSIGNMENT>()
                .Property(e => e.LOW_VALUE)
                .HasPrecision(10, 2);

            modelBuilder.Entity<SUBJOB_ASSIGNMENT>()
                .Property(e => e.HIGH_VALUE)
                .HasPrecision(10, 2);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.TENDER_PROFILE_ITEM)
                .WithRequired(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TENDER_PROFILE>()
                .HasMany(e => e.TENDER_PROFILE_ITEM)
                .WithOptional(e => e.TENDER_PROFILE)
                .HasForeignKey(e => e.GUID_TENDER_PROFILE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TENDER_PROFILE_ITEM>()
                .Property(e => e.HOURS_PERCENTAGE)
                .HasPrecision(10, 5);

            modelBuilder.Entity<TENDER_PROFILE_ITEM>()
                .Property(e => e.SCHEDULE_START_PERCENTAGE)
                .HasPrecision(10, 2);

            modelBuilder.Entity<TENDER_PROFILE_ITEM>()
                .Property(e => e.SCHEDULE_FINISH_PERCENTAGE)
                .HasPrecision(10, 2);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.USER)
                .HasForeignKey(e => e.GUID_USER);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.SUBORDINATES)
                .WithOptional(e => e.APPROVER)
                .HasForeignKey(e => e.GUID_APPROVER);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.BASELINE_ITEM_WORK)
                .WithRequired(e => e.USER)
                .HasForeignKey(e => e.GUID_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.PROJECT)
                .WithOptional(e => e.USER)
                .HasForeignKey(e => e.GUID_MANAGEUSER);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.RA_STUDY)
                .WithOptional(e => e.USER)
                .HasForeignKey(e => e.GUID_FACILITATOR);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.RA_STUDY1)
                .WithOptional(e => e.USER1)
                .HasForeignKey(e => e.GUID_MINUTESBY);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.RA_STUDY_DATA)
                .WithOptional(e => e.USER)
                .HasForeignKey(e => e.GUID_ACTION_BY);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.RA_STUDY_TEAM)
                .WithRequired(e => e.USER)
                .HasForeignKey(e => e.GUID_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.REGISTER_ISSUE)
                .WithOptional(e => e.USER)
                .HasForeignKey(e => e.GUID_RESPONSIBLE_PERSON);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.REGISTER_CHANGE)
                .WithOptional(e => e.USER)
                .HasForeignKey(e => e.GUID_RAISEDBY);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.PROJECT_PERMISSION)
                .WithRequired(e => e.USER)
                .HasForeignKey(e => e.GUID_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.USER_PREFERENCE)
                .WithRequired(e => e.USER)
                .HasForeignKey(e => e.GUID_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<VARIATION>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.VARIATION)
                .HasForeignKey(e => e.GUID_VARIATION);

            modelBuilder.Entity<VARIATION>()
                .HasMany(e => e.ESTIMATE_ITEM)
                .WithOptional(e => e.VARIATION)
                .HasForeignKey(e => e.GUID_VARIATION);

            modelBuilder.Entity<VARIATION>()
                .HasMany(e => e.VARIATION_ITEM)
                .WithRequired(e => e.VARIATION)
                .HasForeignKey(e => e.GUID_VARIATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<VARIATION_CONSTRUCTION>()
                .HasMany(e => e.VARIATION_CONSTRUCTION_ITEM)
                .WithRequired(e => e.VARIATION_CONSTRUCTION)
                .HasForeignKey(e => e.GUID_VARIATION_CONSTRUCTION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<VARIATION_CONSTRUCTION>()
                .HasMany(e => e.VARIATION_CONSTRUCTION_IMPACT)
                .WithRequired(e => e.VARIATION_CONSTRUCTION)
                .HasForeignKey(e => e.GUID_CONSTRUCTION_VARIATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<WORKPACK>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.WORKPACK)
                .HasForeignKey(e => e.GUID_WORKPACK);

            modelBuilder.Entity<WORKPACK>()
                .HasMany(e => e.ESTIMATE_ITEM)
                .WithOptional(e => e.WORKPACK)
                .HasForeignKey(e => e.GUID_WORKPACK);

            modelBuilder.Entity<X_EARNED_QUERY>()
                .Property(e => e.EARNED_UNITS)
                .HasPrecision(18, 7);
        }
    }

    public partial class BluePrintsEntities : BluePrintsNativeEntities
    {
        public BluePrintsEntities()
            : base("name=BluePrintsEntities")
        {
        }
    }
}

