namespace RaceDay.Api.Models;

// Participant MUST stay first (value 0).
// Users.Role has a database DEFAULT of 'Participant' (matching schema.sql). EF Core treats a
// value equal to the CLR default (0) as "not set" and lets the database default apply, so if
// Organiser were 0, every Organiser would silently be saved as a Participant.
public enum UserRole
{
    Participant = 0,
    Organiser = 1
}
