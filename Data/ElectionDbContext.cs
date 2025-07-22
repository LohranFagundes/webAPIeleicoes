using Microsoft.EntityFrameworkCore;
using ElectionApi.Net.Models;

namespace ElectionApi.Net.Data;

public class ElectionDbContext : DbContext
{
    public ElectionDbContext(DbContextOptions<ElectionDbContext> options) : base(options)
    {
    }

    public DbSet<Admin> Admins { get; set; }
    public DbSet<Voter> Voters { get; set; }
    public DbSet<Election> Elections { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<VoteReceipt> VoteReceipts { get; set; }
    public DbSet<SystemSeal> SystemSeals { get; set; }
    public DbSet<ZeroReport> ZeroReports { get; set; }
    public DbSet<SecureVote> SecureVotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Admin configuration
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Permissions).HasColumnType("json");
        });

        // Voter configuration
        modelBuilder.Entity<Voter>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Cpf).IsUnique();
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.VoteWeight).HasPrecision(10, 2);
        });

        // Election configuration
        modelBuilder.Entity<Election>(entity =>
        {
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
        });

        // Position configuration
        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasOne(p => p.Election)
                .WithMany(e => e.Positions)
                .HasForeignKey(p => p.ElectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Candidate configuration
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasOne(c => c.Position)
                .WithMany(p => p.Candidates)
                .HasForeignKey(c => c.PositionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Vote configuration
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasOne(v => v.Voter)
                .WithMany(vo => vo.Votes)
                .HasForeignKey(v => v.VoterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Election)
                .WithMany(e => e.Votes)
                .HasForeignKey(v => v.ElectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Position)
                .WithMany()
                .HasForeignKey(v => v.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Candidate)
                .WithMany(c => c.Votes)
                .HasForeignKey(v => v.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(v => v.VoteWeight).HasPrecision(10, 2);
            entity.HasIndex(v => new { v.VoterId, v.ElectionId, v.PositionId }).IsUnique();
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(e => e.LoggedAt).HasColumnType("datetime");
            entity.HasIndex(e => new { e.UserId, e.EntityType, e.Action });
            entity.HasIndex(e => e.LoggedAt);
        });

        // VoteReceipt configuration
        modelBuilder.Entity<VoteReceipt>(entity =>
        {
            entity.HasOne(vr => vr.Voter)
                .WithMany()
                .HasForeignKey(vr => vr.VoterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(vr => vr.Election)
                .WithMany()
                .HasForeignKey(vr => vr.ElectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(vr => vr.ReceiptToken).IsUnique();
            entity.HasIndex(vr => new { vr.VoterId, vr.ElectionId }).IsUnique();
        });

        // SystemSeal configuration
        modelBuilder.Entity<SystemSeal>(entity =>
        {
            entity.HasOne(ss => ss.Election)
                .WithMany()
                .HasForeignKey(ss => ss.ElectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ss => ss.Admin)
                .WithMany()
                .HasForeignKey(ss => ss.SealedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(ss => new { ss.ElectionId, ss.SealType });
        });

        // ZeroReport configuration
        modelBuilder.Entity<ZeroReport>(entity =>
        {
            entity.HasOne(zr => zr.Election)
                .WithMany()
                .HasForeignKey(zr => zr.ElectionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(zr => zr.Admin)
                .WithMany()
                .HasForeignKey(zr => zr.GeneratedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(zr => zr.ElectionId).IsUnique();
        });

        // SecureVote configuration
        modelBuilder.Entity<SecureVote>(entity =>
        {
            entity.HasOne(sv => sv.Voter)
                .WithMany()
                .HasForeignKey(sv => sv.VoterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sv => sv.Election)
                .WithMany()
                .HasForeignKey(sv => sv.ElectionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sv => sv.Position)
                .WithMany()
                .HasForeignKey(sv => sv.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(sv => sv.VoteId).IsUnique();
            entity.HasIndex(sv => new { sv.VoterId, sv.ElectionId }).IsUnique();
            entity.HasIndex(sv => sv.VotedAt);
            entity.HasIndex(sv => sv.ElectionId);
            entity.HasIndex(sv => sv.PositionId);
        });

        // Configure table names to match PHP conventions
        modelBuilder.Entity<Admin>().ToTable("admins");
        modelBuilder.Entity<Voter>().ToTable("voters");
        modelBuilder.Entity<Election>().ToTable("elections");
        modelBuilder.Entity<Position>().ToTable("positions");
        modelBuilder.Entity<Candidate>().ToTable("candidates");
        modelBuilder.Entity<Vote>().ToTable("votes");
        modelBuilder.Entity<AuditLog>().ToTable("audit_logs");
        modelBuilder.Entity<VoteReceipt>().ToTable("vote_receipts");
        modelBuilder.Entity<SystemSeal>().ToTable("system_seals");
        modelBuilder.Entity<ZeroReport>().ToTable("zero_reports");
        modelBuilder.Entity<SecureVote>().ToTable("secure_votes");

        // Configure column names to snake_case
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }

    private static string ToSnakeCase(string name)
    {
        return string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
    }
}