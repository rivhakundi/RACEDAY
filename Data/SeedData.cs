using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

/// <summary>
/// EF Core seed data mirroring the INSERT statements in docs/schema.sql:
/// 2 Organisers, 2 Participants, 3 Events, 6 Categories, 4 Enrolments, 2 Results.
///
/// Deliberate difference from schema.sql: the SQL script uses placeholder PasswordHash text,
/// which can never match a real password. Here every seeded account shares one real
/// ASP.NET Core Identity (PBKDF2) hash so the seeded users can actually log in.
/// Development password for ALL seeded accounts: Password123!
/// </summary>
public static class SeedData
{
    private const string SeedPasswordHash =
        "AQAAAAEAAYagAAAAEKstEcFGc0TE5/55Wpr7gdX7qedgHDIJZEjJjWC5nCXsHNWyXgoYCRpIwShkBy+5Vw==";

    // Fixed timestamps: HasData needs deterministic values, otherwise every migration would
    // detect a "change" in the seed rows.
    private static readonly DateTime SeededAt = new(2026, 7, 1, 8, 0, 0);

    public static void SeedRaceDayData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User { UserId = 1, FullName = "Sarah Nkosi",   Email = "sarah.nkosi@raceday.co.za", PasswordHash = SeedPasswordHash, Role = UserRole.Organiser,   CreatedAt = SeededAt },
            new User { UserId = 2, FullName = "David Botha",   Email = "david.botha@raceday.co.za", PasswordHash = SeedPasswordHash, Role = UserRole.Organiser,   CreatedAt = SeededAt },
            new User { UserId = 3, FullName = "Lindiwe Dube",  Email = "lindiwe.dube@example.com",  PasswordHash = SeedPasswordHash, Role = UserRole.Participant, CreatedAt = SeededAt },
            new User { UserId = 4, FullName = "Michael Reddy", Email = "michael.reddy@example.com", PasswordHash = SeedPasswordHash, Role = UserRole.Participant, CreatedAt = SeededAt }
        );

        modelBuilder.Entity<UserProfile>().HasData(
            new UserProfile { ProfileId = 1, UserId = 1, PhoneNumber = "0821234567", DateOfBirth = new DateOnly(1985, 3, 12),  Bio = "Event organiser specialising in road running." },
            new UserProfile { ProfileId = 2, UserId = 2, PhoneNumber = "0837654321", DateOfBirth = new DateOnly(1979, 11, 2),  Bio = "Organiser for cycling and multi-sport events." },
            new UserProfile { ProfileId = 3, UserId = 3, PhoneNumber = "0715558899", DateOfBirth = new DateOnly(1998, 6, 21),  Bio = "Keen 10km and half-marathon runner." },
            new UserProfile { ProfileId = 4, UserId = 4, PhoneNumber = "0723334455", DateOfBirth = new DateOnly(1990, 1, 15),  Bio = "Weekend cyclist training for first century ride." }
        );

        modelBuilder.Entity<Event>().HasData(
            new Event { EventId = 1, OrganiserId = 1, Name = "Johannesburg City Run",    Description = "Annual road running event through the city centre.", EventDate = new DateOnly(2026, 9, 12),  Location = "Johannesburg, Gauteng",      Distance = 21.10m, EventType = EventType.Run,   CreatedAt = SeededAt },
            new Event { EventId = 2, OrganiserId = 1, Name = "Sunrise Fun Walk",         Description = "Community walk supporting local charities.",         EventDate = new DateOnly(2026, 8, 30),  Location = "Pretoria, Gauteng",          Distance = 5.00m,  EventType = EventType.Walk,  CreatedAt = SeededAt },
            new Event { EventId = 3, OrganiserId = 2, Name = "Highveld Cycle Challenge", Description = "Road cycling race across the Highveld region.",      EventDate = new DateOnly(2026, 10, 4),  Location = "Bronkhorstspruit, Gauteng",  Distance = 90.00m, EventType = EventType.Cycle, CreatedAt = SeededAt }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, EventId = 1, Name = "Senior 21km",     MinAge = 20, MaxAge = 39, CategoryDistance = 21.10m },
            new Category { CategoryId = 2, EventId = 1, Name = "Veteran 21km",    MinAge = 40, MaxAge = 99, CategoryDistance = 21.10m },
            new Category { CategoryId = 3, EventId = 1, Name = "Under 20",        MinAge = 13, MaxAge = 19, CategoryDistance = 21.10m },
            new Category { CategoryId = 4, EventId = 2, Name = "5km Walk",        MinAge = 0,  MaxAge = 99, CategoryDistance = 5.00m },
            new Category { CategoryId = 5, EventId = 3, Name = "90km Road Race",  MinAge = 18, MaxAge = 99, CategoryDistance = 90.00m },
            new Category { CategoryId = 6, EventId = 3, Name = "45km Fun Ride",   MinAge = 14, MaxAge = 99, CategoryDistance = 45.00m }
        );

        modelBuilder.Entity<Enrolment>().HasData(
            new Enrolment { EnrolmentId = 1, ParticipantId = 3, EventId = 1, CategoryId = 1, EnrolmentDate = SeededAt },
            new Enrolment { EnrolmentId = 2, ParticipantId = 4, EventId = 1, CategoryId = 2, EnrolmentDate = SeededAt },
            new Enrolment { EnrolmentId = 3, ParticipantId = 3, EventId = 3, CategoryId = 5, EnrolmentDate = SeededAt },
            new Enrolment { EnrolmentId = 4, ParticipantId = 4, EventId = 2, CategoryId = 4, EnrolmentDate = SeededAt }
        );

        modelBuilder.Entity<Result>().HasData(
            new Result { ResultId = 1, EnrolmentId = 1, FinishTime = new TimeOnly(1, 45, 32), FinishingPosition = 12, CapturedAt = SeededAt },
            new Result { ResultId = 2, EnrolmentId = 2, FinishTime = new TimeOnly(1, 58, 10), FinishingPosition = 27, CapturedAt = SeededAt }
        );
    }
}
