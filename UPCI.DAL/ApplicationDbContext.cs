using UPCI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace UPCI.DAL
{
    public class ApplicationDbContext : DbContext
    { 
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
            this.ChangeTracker.LazyLoadingEnabled = false;
        }
        public DbSet<ActivityLog>? ActivityLog { get; set; }
        public DbSet<ExceptionLog>? ExceptionLog { get; set; }
        public DbSet<AuditTrail>? AuditTrail { get; set; }
        public virtual DbSet<ActiveUser>? ActiveUser { get; set; }
        public virtual DbSet<Cell>? Cell { get; set; }
        public virtual DbSet<CivilStatus>? CivilStatus { get; set; } 
        public virtual DbSet<Member>? Member { get; set; }
        public virtual DbSet<MemberCell>? MemberCell { get; set; }
        public virtual DbSet<MemberType>? MemberType { get; set; } 
        public virtual DbSet<Ministry>? Ministry { get; set; }
        public virtual DbSet<Module>? Module { get; set; } 
        public virtual DbSet<ModuleAction>? ModuleAction { get; set; }
        public virtual DbSet<PEPSOLLevel>? PEPSOLLevel { get; set; }
        public virtual DbSet<PositionCell>? PositionCell { get; set; }
        public virtual DbSet<PositionMinistry>? PositionMinistry { get; set; }
        public virtual DbSet<User>? User { get; set; }
        
        public virtual DbSet<Role>? Role { get; set; }
        public virtual DbSet<RoleModule>? RoleModule { get; set; }

        public virtual DbSet<ApiClient>? ApiClient{ get; set; }
        public virtual DbSet<Company>? Company { get; set; }
        public virtual DbSet<Route>? Route { get; set; }
        public virtual DbSet<MapRouteClient>? MapRouteClient { get; set; }
        public virtual DbSet<MapRouteIp>? MapRouteIp { get; set; }

        public DbSet<HttpLog>? HttpLog { get; set; }
        public DbSet<TransactionLog>? TransactionLog { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
                .HasMany(r => r.RoleModule)
                .WithOne(rm => rm.Role)
                .HasForeignKey(rm => rm.RoleModuleCode)
                .HasPrincipalKey(r => r.Code);

            modelBuilder.Entity<Module>()
                .HasMany(m => m.RoleModules)
                .WithOne(rm => rm.Module)
                .HasForeignKey(rm => rm.ModuleCode)
                .HasPrincipalKey(m => m.Code);

          


            modelBuilder.Entity<ApiClient>()
                .Property(e => e.CompanyId)
                .HasConversion(
                    v => v.ToString().ToUpper(), // Converts Guid to string for storing in the database
                    v => string.IsNullOrEmpty(v) ? (Guid?)null : Guid.Parse(v)); // Converts string from the database back to Guid


            modelBuilder.Entity<MapRouteClient>()
               .Property(mrc => mrc.RouteId)
               .HasConversion(
                    v => v.ToString().ToUpper(), // Converts Guid to string for storing in the database
                    v => string.IsNullOrEmpty(v) ? (Guid?)null : Guid.Parse(v)); // Converts string from the database back to Guid

           modelBuilder.Entity<MapRouteIp>()
              .Property(mrc => mrc.RouteId)
              .HasConversion(
                   v => v.ToString().ToUpper(), // Converts Guid to string for storing in the database
                   v => string.IsNullOrEmpty(v) ? (Guid?)null : Guid.Parse(v)); // Converts string from the database back to Guid


            modelBuilder.Entity<Member>()
                  .HasMany(m => m.MemberCell)
                  .WithOne(rm => rm.Member)
                  .HasForeignKey(rm => rm.MemberCode)
                  .HasPrincipalKey(m => m.Code);


        }

    }
}
