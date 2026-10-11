using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Constraint, index and column-type names below deliberately mirror docs/schema.sql
        // so the Code-First schema matches the Part 1 ERD and SQL script.

        // ---------- Users ----------
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", t => t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Organiser','Participant')"));
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.Email).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique().HasDatabaseName("UQ_Users_Email");
            entity.Property(u => u.Role)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .HasDefaultValue(UserRole.Participant);
            entity.Property(u => u.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
        });

        // ---------- UserProfiles (1:1 with Users) ----------
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("UserProfiles");
            entity.HasKey(p => p.ProfileId);
            entity.HasIndex(p => p.UserId).IsUnique().HasDatabaseName("UQ_UserProfiles_UserId");
            entity.HasOne(p => p.User)
                  .WithOne(u => u.Profile)
                  .HasForeignKey<UserProfile>(p => p.UserId)
                  .HasConstraintName("FK_UserProfiles_Users")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Events (1:M from Users as Organiser) ----------
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events", t =>
            {
                t.HasCheckConstraint("CK_Events_EventType", "[EventType] IN ('Run','Walk','Cycle')");
                t.HasCheckConstraint("CK_Events_Distance", "[Distance] > 0");
            });
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.Distance).HasColumnType("decimal(6,2)");
            entity.Property(e => e.EventType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            entity.HasOne(e => e.Organiser)
                  .WithMany(u => u.OrganisedEvents)
                  .HasForeignKey(e => e.OrganiserId)
                  .HasConstraintName("FK_Events_Organiser")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Categories (1:M from Events) ----------
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(c => c.CategoryId);
            entity.Property(c => c.CategoryDistance).HasColumnType("decimal(6,2)");
            entity.HasOne(c => c.Event)
                  .WithMany(e => e.Categories)
                  .HasForeignKey(c => c.EventId)
                  .HasConstraintName("FK_Categories_Events")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Enrolments (link: Participant, Event, Category) ----------
        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.ToTable("Enrolments");
            entity.HasKey(en => en.EnrolmentId);
            entity.Property(en => en.EnrolmentDate).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");

            // A participant may enrol in a given event only once.
            entity.HasIndex(en => new { en.ParticipantId, en.EventId })
                  .IsUnique()
                  .HasDatabaseName("UQ_Enrolments_Participant_Event");

            entity.HasOne(en => en.Participant)
                  .WithMany(u => u.Enrolments)
                  .HasForeignKey(en => en.ParticipantId)
                  .HasConstraintName("FK_Enrolments_Participant")
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(en => en.Event)
                  .WithMany(ev => ev.Enrolments)
                  .HasForeignKey(en => en.EventId)
                  .HasConstraintName("FK_Enrolments_Events")
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(en => en.Category)
                  .WithMany(c => c.Enrolments)
                  .HasForeignKey(en => en.CategoryId)
                  .HasConstraintName("FK_Enrolments_Categories")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Results (1:1 with Enrolments) ----------
        modelBuilder.Entity<Result>(entity =>
        {
            entity.ToTable("Results");
            entity.HasKey(r => r.ResultId);
            entity.HasIndex(r => r.EnrolmentId).IsUnique().HasDatabaseName("UQ_Results_EnrolmentId");
            entity.Property(r => r.CapturedAt).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            entity.HasOne(r => r.Enrolment)
                  .WithOne(en => en.Result)
                  .HasForeignKey<Result>(r => r.EnrolmentId)
                  .HasConstraintName("FK_Results_Enrolments")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Seed data (mirrors the INSERTs in docs/schema.sql) ----------
        modelBuilder.SeedRaceDayData();
    }
}
