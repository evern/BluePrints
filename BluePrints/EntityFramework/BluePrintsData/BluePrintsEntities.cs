namespace BluePrints.Data
{
    using EntityFramework.Functions;
    using System.Data.Entity;

    public partial class BluePrintsEntities : DbContext
    {
        public BluePrintsEntities()
            : base("name=BluePrintsEntities")
        {
        }

        public virtual DbSet<AREA> AREA { get; set; }
        public virtual DbSet<BASELINE> BASELINE { get; set; }
        public virtual DbSet<BASELINE_ITEM> BASELINE_ITEM { get; set; }
        public virtual DbSet<BASELINE_ITEM_ASSIGNMENT> BASELINE_ITEM_ASSIGNMENT { get; set; }
        public virtual DbSet<BASELINE_ITEM_WORK> BASELINE_ITEM_WORK { get; set; }
        public virtual DbSet<COMMODITY_CODE> COMMODITY_CODE { get; set; }
        public virtual DbSet<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECT { get; set; }
        public virtual DbSet<DELIVERABLES_STATUS> DELIVERABLES_STATUS { get; set; }
        public virtual DbSet<DEPARTMENT> DEPARTMENT { get; set; }
        public virtual DbSet<DISCIPLINE> DISCIPLINE { get; set; }
        public virtual DbSet<DOCTYPE> DOCTYPE { get; set; }
        public virtual DbSet<ESTIMATION_DIRECT> ESTIMATION_DIRECT { get; set; }
        public virtual DbSet<ESTIMATION_DIRECT_ITEM> ESTIMATION_DIRECT_ITEM { get; set; }
        public virtual DbSet<ESTIMATION_INDIRECT> ESTIMATION_INDIRECT { get; set; }
        public virtual DbSet<ESTIMATION_INDIRECT_ITEM> ESTIMATION_INDIRECT_ITEM { get; set; }
        public virtual DbSet<ESTIMATION_SETTING> ESTIMATION_SETTING { get; set; }
        public virtual DbSet<INDIRECT_TYPE> INDIRECT_TYPE { get; set; }
        public virtual DbSet<PHASE> PHASE { get; set; }
        public virtual DbSet<PROGRESS> PROGRESS { get; set; }
        public virtual DbSet<PROGRESS_ITEM> PROGRESS_ITEM { get; set; }
        public virtual DbSet<PROJECT> PROJECT { get; set; }
        public virtual DbSet<PROJECT_REPORT> PROJECT_REPORT { get; set; }
        public virtual DbSet<RATE> RATE { get; set; }
        public virtual DbSet<REGISTER> REGISTER { get; set; }
        public virtual DbSet<REGISTER_CHANGE> REGISTER_CHANGE { get; set; }
        public virtual DbSet<REGISTER_HOLD> REGISTER_HOLD { get; set; }
        public virtual DbSet<REGISTER_ISSUE> REGISTER_ISSUE { get; set; }
        public virtual DbSet<REGISTER_LL> REGISTER_LL { get; set; }
        public virtual DbSet<REGISTER_NC> REGISTER_NC { get; set; }
        public virtual DbSet<REGISTER_RISK> REGISTER_RISK { get; set; }
        public virtual DbSet<ROLE> ROLE { get; set; }
        public virtual DbSet<ROLE_PERMISSION> ROLE_PERMISSION { get; set; }
        public virtual DbSet<SETTINGS_GLOBAL> SETTINGS_GLOBAL { get; set; }
        public virtual DbSet<TIMEGROUP> TIMEGROUP { get; set; }
        public virtual DbSet<UOM> UOM { get; set; }
        public virtual DbSet<USER> USER { get; set; }
        public virtual DbSet<VARIATION> VARIATION { get; set; }
        public virtual DbSet<VARIATION_ITEM> VARIATION_ITEM { get; set; }
        public virtual DbSet<WORKPACK> WORKPACK { get; set; }
        public virtual DbSet<WORKPACK_ASSIGNMENT> WORKPACK_ASSIGNMENT { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<BluePrintsEntities>(null);
            modelBuilder.Conventions.Add(new FunctionConvention<BluePrintsEntities>());
            modelBuilder.ComplexType<StoredProcedure_DeliverablesDataPoints>();
            modelBuilder.AddComplexTypesFromAssembly(typeof(BluePrintsEntities).Assembly);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.ESTIMATION_DIRECT_ITEM)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

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
                .HasMany(e => e.WORKPACK)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_DAREA);

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

            modelBuilder.Entity<BASELINE>()
                .HasMany(e => e.VARIATION)
                .WithOptional(e => e.BASELINE)
                .HasForeignKey(e => e.GUID_BASELINE);

            modelBuilder.Entity<BASELINE>()
                .HasMany(e => e.VARIATION1)
                .WithOptional(e => e.BASELINE1)
                .HasForeignKey(e => e.GUID_ORIBASELINE);

            modelBuilder.Entity<BASELINE_ITEM>()
                .Property(e => e.P6_ASSIGNMENT_STARTUNIT)
                .HasPrecision(18, 0);

            modelBuilder.Entity<BASELINE_ITEM_ASSIGNMENT>()
                .Property(e => e.LOW_VALUE)
                .HasPrecision(10, 2);

            modelBuilder.Entity<BASELINE_ITEM_ASSIGNMENT>()
                .Property(e => e.HIGH_VALUE)
                .HasPrecision(10, 2);

            modelBuilder.Entity<BASELINE_ITEM_WORK>()
                .Property(e => e.WEIGHTING)
                .HasPrecision(5, 2);

            modelBuilder.Entity<COMMODITY_CODE>()
                .HasMany(e => e.COMMODITY_GROUP_DIRECT)
                .WithOptional(e => e.COMMODITY_CODE)
                .HasForeignKey(e => e.GUID_COMMODITYCODE);

            modelBuilder.Entity<COMMODITY_CODE>()
                .HasMany(e => e.ESTIMATION_DIRECT_ITEM)
                .WithOptional(e => e.COMMODITY_CODE)
                .HasForeignKey(e => e.GUID_COMMODITY_CODE);

            modelBuilder.Entity<COMMODITY_CODE>()
                .HasMany(e => e.ESTIMATION_INDIRECT_ITEM)
                .WithRequired(e => e.COMMODITY_CODE)
                .HasForeignKey(e => e.GUID_COMMODITY_CODE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<COMMODITY_GROUP_DIRECT>()
                .HasMany(e => e.ESTIMATION_DIRECT_ITEM)
                .WithOptional(e => e.COMMODITY_GROUP_DIRECT)
                .HasForeignKey(e => e.GUID_COMMODITY_GROUP_DIRECT);

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

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.DEPARTMENT)
                .HasForeignKey(e => e.GUID_DEPARTMENT);

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.COMMODITY_CODE)
                .WithOptional(e => e.DEPARTMENT)
                .HasForeignKey(e => e.GUID_DEPARTMENT);

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.DOCTYPE)
                .WithRequired(e => e.DEPARTMENT)
                .HasForeignKey(e => e.GUID_DDEPARTMENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.RATE)
                .WithRequired(e => e.DEPARTMENT)
                .HasForeignKey(e => e.GUID_DEPARTMENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.WORKPACK)
                .WithRequired(e => e.DEPARTMENT)
                .HasForeignKey(e => e.GUID_DDEPARTMENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.COMMODITY_CODE)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.ESTIMATION_DIRECT_ITEM)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.RATE)
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
                .HasMany(e => e.WORKPACK)
                .WithRequired(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DDISCIPLINE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DOCTYPE>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.DOCTYPE)
                .HasForeignKey(e => e.GUID_DOCTYPE);

            modelBuilder.Entity<DOCTYPE>()
                .HasMany(e => e.DELIVERABLES_STATUS)
                .WithOptional(e => e.DOCTYPE)
                .HasForeignKey(e => e.GUID_DOCTYPE);

            modelBuilder.Entity<DOCTYPE>()
                .HasMany(e => e.WORKPACK)
                .WithOptional(e => e.DOCTYPE)
                .HasForeignKey(e => e.GUID_DDOCTYPE);

            modelBuilder.Entity<ESTIMATION_DIRECT>()
                .HasMany(e => e.ESTIMATION_DIRECT_ITEM)
                .WithRequired(e => e.ESTIMATION_DIRECT)
                .HasForeignKey(e => e.GUID_ESTIMATION_DIRECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ESTIMATION_INDIRECT>()
                .HasMany(e => e.ESTIMATION_INDIRECT_ITEM)
                .WithRequired(e => e.ESTIMATION_INDIRECT)
                .HasForeignKey(e => e.GUID_ESTIMATION_INDIRECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ESTIMATION_INDIRECT_ITEM>()
                .Property(e => e.PLANT_RATE)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ESTIMATION_INDIRECT_ITEM>()
                .Property(e => e.HOURSAWEEK)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ESTIMATION_INDIRECT_ITEM>()
                .Property(e => e.FREIGHT_FOOTPRINT)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.COST_PER_FREIGHT)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.ADD_FREIGHT_ALLOWANCE)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.MAN_HOUR_PER_SHIFT)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.AVERAGE_DAYS_PER_WEEK)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.DIRECT_DAYS_ON_SITE)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.INDIRECT_DAYS_ON_SITE)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.DIRECT_DAYS_ON_RNR)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.INDIRECT_DAYS_ON_RNR)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.FLIGHT_COST)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.ADDITIONAL_FLIGHT)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.ONSITE_ACC_COST)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.ONSITE_MAN_ACC_COST)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.RNR_ACC_COST)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.CONTRACT_VALUE)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.DEFECTS_LIABILITY_PERIOD)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.TENDER_COST)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.SMALL_TOOLS_COST)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.WEEKLY_SITE_HOURS)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ESTIMATION_SETTING>()
                .Property(e => e.WEEKLY_OFFSITE_HOURS)
                .HasPrecision(10, 2);

            modelBuilder.Entity<INDIRECT_TYPE>()
                .HasMany(e => e.COMMODITY_CODE)
                .WithOptional(e => e.INDIRECT_TYPE)
                .HasForeignKey(e => e.GUID_INDIRECTTYPE);

            modelBuilder.Entity<PHASE>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.PHASE)
                .HasForeignKey(e => e.GUID_PHASE);

            modelBuilder.Entity<PHASE>()
                .HasMany(e => e.WORKPACK)
                .WithOptional(e => e.PHASE)
                .HasForeignKey(e => e.GUID_DPHASE);

            modelBuilder.Entity<PROGRESS>()
                .HasMany(e => e.PROGRESS_ITEM)
                .WithRequired(e => e.PROGRESS)
                .HasForeignKey(e => e.GUID_PROGRESS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROGRESS_ITEM>()
                .Property(e => e.EARNED_UNITS)
                .HasPrecision(18, 4);

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
                .HasMany(e => e.BASELINE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.BASELINE_ITEM_ASSIGNMENT)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.COMMODITY_CODE)
                .WithOptional(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.DELIVERABLES_STATUS)
                .WithOptional(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.ESTIMATION_DIRECT)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.ESTIMATION_INDIRECT)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.ESTIMATION_SETTING)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PHASE)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROGRESS)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.REGISTER)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.PROJECT_REPORT)
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
                .HasMany(e => e.VARIATION)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.WORKPACK)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RATE>()
                .Property(e => e.RATE1)
                .HasPrecision(5, 2);

            modelBuilder.Entity<REGISTER>()
                .Property(e => e.UNIQUE_H_NUM)
                .IsFixedLength();

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

            modelBuilder.Entity<TIMEGROUP>()
                .HasMany(e => e.ESTIMATION_DIRECT_ITEM)
                .WithOptional(e => e.TIMEGROUP)
                .HasForeignKey(e => e.GUID_TIMEGROUP);

            modelBuilder.Entity<TIMEGROUP>()
                .HasMany(e => e.ESTIMATION_INDIRECT_ITEM)
                .WithOptional(e => e.TIMEGROUP)
                .HasForeignKey(e => e.GUID_TIMEGROUP);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.USER)
                .HasForeignKey(e => e.GUID_USER);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.BASELINE_ITEM_WORK)
                .WithRequired(e => e.USER)
                .HasForeignKey(e => e.GUID_USER)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<USER>()
                .HasMany(e => e.PROJECTS)
                .WithOptional(e => e.USER)
                .HasForeignKey(e => e.GUID_MANAGEUSER);

            modelBuilder.Entity<VARIATION>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.VARIATION)
                .HasForeignKey(e => e.GUID_VARIATION);

            modelBuilder.Entity<VARIATION>()
                .HasMany(e => e.VARIATION_ITEM)
                .WithRequired(e => e.VARIATION)
                .HasForeignKey(e => e.GUID_VARIATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<WORKPACK>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.WORKPACK)
                .HasForeignKey(e => e.GUID_WORKPACK);

            modelBuilder.Entity<WORKPACK>()
                .HasMany(e => e.ESTIMATION_DIRECT_ITEM)
                .WithOptional(e => e.WORKPACK)
                .HasForeignKey(e => e.GUID_INSTALLWORKPACK);

            modelBuilder.Entity<WORKPACK>()
                .HasMany(e => e.ESTIMATION_DIRECT_ITEM1)
                .WithOptional(e => e.WORKPACK1)
                .HasForeignKey(e => e.GUID_SUPPLYWORKPACK);

            modelBuilder.Entity<WORKPACK>()
                .HasMany(e => e.ESTIMATION_INDIRECT_ITEM)
                .WithOptional(e => e.WORKPACK)
                .HasForeignKey(e => e.GUID_WORKPACK);

            modelBuilder.Entity<WORKPACK>()
                .HasMany(e => e.WORKPACK_ASSIGNMENT)
                .WithRequired(e => e.WORKPACK)
                .HasForeignKey(e => e.GUID_WORKPACK)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<WORKPACK_ASSIGNMENT>()
                .Property(e => e.LOW_VALUE)
                .HasPrecision(10, 2);

            modelBuilder.Entity<WORKPACK_ASSIGNMENT>()
                .Property(e => e.HIGH_VALUE)
                .HasPrecision(10, 2);
        }
    }
}
