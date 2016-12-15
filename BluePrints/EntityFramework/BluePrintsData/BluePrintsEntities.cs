namespace BluePrints.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;
    using BluePrints.Data.Helpers;

    public partial class BluePrintsEntities : DbContext
    {
        public BluePrintsEntities()
            : base("name=BluePrintsEntities")
        {
        }

        public virtual DbSet<AREA> AREA { get; set; }
        public virtual DbSet<BASELINE> BASELINE { get; set; }
        public virtual DbSet<BASELINE_ITEM> BASELINE_ITEM { get; set; }
        public virtual DbSet<COMMODITY_CODE> COMMODITY_CODE { get; set; }
        public virtual DbSet<COMMODITY_GROUP_DIRECT> COMMODITY_GROUP_DIRECT { get; set; }
        public virtual DbSet<DEPARTMENT> DEPARTMENT { get; set; }
        public virtual DbSet<DISCIPLINE> DISCIPLINE { get; set; }
        public virtual DbSet<DOCTYPE> DOCTYPE { get; set; }
        public virtual DbSet<ESTIMATION> ESTIMATION { get; set; }
        public virtual DbSet<ESTIMATION_ITEM> ESTIMATION_ITEM { get; set; }
        public virtual DbSet<INDIRECT_TYPE> INDIRECT_TYPE { get; set; }
        public virtual DbSet<PHASE> PHASE { get; set; }
        public virtual DbSet<PROGRESS> PROGRESS { get; set; }
        public virtual DbSet<PROGRESS_ITEM> PROGRESS_ITEM { get; set; }
        public virtual DbSet<PROJECT> PROJECT { get; set; }
        public virtual DbSet<PROJECT_REPORT> PROJECT_REPORT { get; set; }
        public virtual DbSet<RATE> RATE { get; set; }
        public virtual DbSet<REGISTER> REGISTER { get; set; }
        public virtual DbSet<ROLE> ROLE { get; set; }
        public virtual DbSet<ROLE_PERMISSION> ROLE_PERMISSION { get; set; }
        public virtual DbSet<SETTINGS_GLOBAL> SETTINGS_GLOBAL { get; set; }
        public virtual DbSet<UOM> UOM { get; set; }
        public virtual DbSet<USER> USER { get; set; }
        public virtual DbSet<VARIATION> VARIATION { get; set; }
        public virtual DbSet<VARIATION_ITEM> VARIATION_ITEM { get; set; }
        public virtual DbSet<WORKPACK> WORKPACK { get; set; }
        public virtual DbSet<WORKPACK_ASSIGNMENT> WORKPACK_ASSIGNMENT { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<BluePrintsEntities>(null);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.ESTIMATION_ITEM)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_AREA);

            modelBuilder.Entity<AREA>()
                .HasMany(e => e.WORKPACK)
                .WithOptional(e => e.AREA)
                .HasForeignKey(e => e.GUID_DAREA)
                .WillCascadeOnDelete(false);

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
                .WithOptional(e => e.FROMBASELINE)
                .HasForeignKey(e => e.GUID_BASELINE);

            modelBuilder.Entity<BASELINE>()
                .HasMany(e => e.VARIATION1)
                .WithOptional(e => e.TOBASELINE)
                .HasForeignKey(e => e.GUID_ORIBASELINE);

            modelBuilder.Entity<COMMODITY_CODE>()
                .HasMany(e => e.COMMODITY_GROUP_DIRECT)
                .WithOptional(e => e.COMMODITY_CODE)
                .HasForeignKey(e => e.GUID_COMMODITYCODE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.BASELINE_ITEM)
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

            modelBuilder.Entity<DEPARTMENT>()
                .HasMany(e => e.COMMODITY_CODE)
                .WithOptional(e => e.DEPARTMENT)
                .HasForeignKey(e => e.GUID_DEPARTMENT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.BASELINE_ITEM)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.COMMODITY_CODE)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.ESTIMATION_ITEM)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.COMMODITY_GROUP_DIRECT)
                .WithOptional(e => e.DISCIPLINE)
                .HasForeignKey(e => e.GUID_DISCIPLINE);

            modelBuilder.Entity<DISCIPLINE>()
                .HasMany(e => e.RATE)
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
                .HasMany(e => e.WORKPACK)
                .WithOptional(e => e.DOCTYPE)
                .HasForeignKey(e => e.GUID_DDOCTYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ESTIMATION>()
                .Property(e => e.MARGIN)
                .HasPrecision(9, 2);

            modelBuilder.Entity<ESTIMATION>()
                .Property(e => e.CONTINGENCY)
                .HasPrecision(9, 2);

            modelBuilder.Entity<ESTIMATION>()
                .HasMany(e => e.ESTIMATION_ITEM)
                .WithRequired(e => e.ESTIMATION)
                .HasForeignKey(e => e.GUID_ESTIMATION)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<INDIRECT_TYPE>()
                .HasMany(e => e.COMMODITY_CODE)
                .WithOptional(e => e.INDIRECT_TYPE)
                .HasForeignKey(e => e.GUID_INDIRECTTYPE)
                .WillCascadeOnDelete(false);

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
                .HasMany(e => e.ESTIMATION)
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
                .HasMany(e => e.VARIATION)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.WORKPACK)
                .WithRequired(e => e.PROJECT)
                .HasForeignKey(e => e.GUID_PROJECT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<PROJECT>()
                .HasMany(e => e.COMMODITY_GROUP_DIRECT)
                .WithOptional(e => e.PROJECT)
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
                .HasMany(e => e.ESTIMATION_ITEM_SUPPLY)
                .WithOptional(e => e.SUPPLYWORKPACK)
                .HasForeignKey(e => e.GUID_SUPPLYWORKPACK);

            modelBuilder.Entity<WORKPACK>()
                .HasMany(e => e.ESTIMATION_ITEM_INSTALL)
                .WithOptional(e => e.INSTALLWORKPACK)
                .HasForeignKey(e => e.GUID_INSTALLWORKPACK);

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
