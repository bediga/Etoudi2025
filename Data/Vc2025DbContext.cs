using Microsoft.EntityFrameworkCore;
using VcBlazor.Data.Entities;

namespace VcBlazor.Data
{
    public class Vc2025DbContext : DbContext
    {
        public Vc2025DbContext(DbContextOptions<Vc2025DbContext> options) : base(options)
        {
        }

        // Administrative hierarchy
        public DbSet<Region> Regions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Arrondissement> Arrondissements { get; set; }
        public DbSet<Commune> Communes { get; set; }
        public DbSet<AdministrativeDivision> AdministrativeDivisions { get; set; }

        // Electoral infrastructure
        public DbSet<VotingCenter> VotingCenters { get; set; }
        public DbSet<PollingStation> PollingStations { get; set; }
        public DbSet<PollingStationHierarchy> PollingStationsHierarchy { get; set; }

        // Users & authorization
        public DbSet<User> Users { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserAssociation> UserAssociations { get; set; }

        // Candidates & results
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<ElectionResult> ElectionResults { get; set; }

        // Submissions
        public DbSet<ResultSubmission> ResultSubmissions { get; set; }
        public DbSet<ResultSubmissionDetail> ResultSubmissionDetails { get; set; }
        public DbSet<SubmissionResult> SubmissionResults { get; set; }
        public DbSet<SubmissionDocument> SubmissionDocuments { get; set; }
        public DbSet<Result> Results { get; set; }

        // Verification
        public DbSet<VerificationTask> VerificationTasks { get; set; }
        public DbSet<VerificationHistory> VerificationHistories { get; set; }

        // Monitoring & assignments
        public DbSet<HourlyTurnout> HourlyTurnouts { get; set; }
        public DbSet<BureauAssignment> BureauAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique constraints
            modelBuilder.Entity<Region>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.Role, rp.Permission })
                .IsUnique();

            modelBuilder.Entity<ResultSubmissionDetail>()
                .HasIndex(rsd => new { rsd.SubmissionId, rsd.CandidateId })
                .IsUnique();

            modelBuilder.Entity<HourlyTurnout>()
                .HasIndex(ht => new { ht.PollingStationId, ht.Hour })
                .IsUnique();

            modelBuilder.Entity<BureauAssignment>()
                .HasIndex(ba => new { ba.UserId, ba.PollingStationId })
                .IsUnique();

            // Configure check constraints for hour
            modelBuilder.Entity<HourlyTurnout>()
                .ToTable(t => t.HasCheckConstraint("CK_HourlyTurnout_Hour", "hour >= 0 AND hour <= 23"));

            // Configure cascade delete
            modelBuilder.Entity<ResultSubmissionDetail>()
                .HasOne(rsd => rsd.ResultSubmission)
                .WithMany(rs => rs.ResultSubmissionDetails)
                .HasForeignKey(rsd => rsd.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubmissionDocument>()
                .HasOne(sd => sd.ResultSubmission)
                .WithMany(rs => rs.SubmissionDocuments)
                .HasForeignKey(sd => sd.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure multiple foreign keys for User entity
            modelBuilder.Entity<ResultSubmission>()
                .HasOne(rs => rs.SubmittedByUser)
                .WithMany(u => u.SubmittedResults)
                .HasForeignKey(rs => rs.SubmittedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ResultSubmission>()
                .HasOne(rs => rs.VerifiedByUser)
                .WithMany(u => u.VerifiedResults)
                .HasForeignKey(rs => rs.VerifiedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BureauAssignment>()
                .HasOne(ba => ba.User)
                .WithMany(u => u.UserAssignments)
                .HasForeignKey(ba => ba.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BureauAssignment>()
                .HasOne(ba => ba.AssignedByUser)
                .WithMany(u => u.AssignedByUser)
                .HasForeignKey(ba => ba.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure decimal precision for monetary values
            modelBuilder.Entity<ElectionResult>()
                .Property(er => er.Percentage)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ResultSubmission>()
                .Property(rs => rs.TurnoutRate)
                .HasPrecision(5, 2);

            modelBuilder.Entity<PollingStation>()
                .Property(ps => ps.TurnoutRate)
                .HasPrecision(5, 2);

            modelBuilder.Entity<PollingStationHierarchy>()
                .Property(psh => psh.TurnoutRate)
                .HasPrecision(5, 2);

            modelBuilder.Entity<HourlyTurnout>()
                .Property(ht => ht.TurnoutRate)
                .HasPrecision(5, 2);

            modelBuilder.Entity<ResultSubmissionDetail>()
                .Property(rsd => rsd.Percentage)
                .HasPrecision(5, 2);
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}