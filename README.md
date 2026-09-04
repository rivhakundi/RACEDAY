# RaceDay - Part 1 Planning Documentation

This folder contains the three Part 1 deliverables for the RaceDay system.

## Contents

- **erd.png** / **erd.pdf** - Entity Relationship Diagram for the full RaceDay data model.
- **schema.sql** - SQL Server script that creates every table in the ERD, with all primary
  keys, foreign keys, and constraints, plus seed data (2 Organisers, 2 Participants,
  3 Events, categories per event, and sample enrolments/results).
- **api_endpoint_plan.md** - Full API endpoint plan covering Authentication, User Profile,
  Events, Categories, Event Enrolments, and Results.

## Data model summary (6 entities)

1. **Users** - both Organisers and Participants; role stored on the user record.
2. **UserProfiles** - 1:1 with Users; holds extended profile fields separate from
   login/auth data.
3. **Events** - 1:M from Users (Organiser); each event has a name, description, date,
   location, distance, and event type (Run, Walk, Cycle).
4. **Categories** - 1:M from Events; each event defines its own age/distance categories.
5. **Enrolments** - the link entity between a Participant (Users), an Event, and the
   Category they selected. A participant may enrol in a given event only once
   (enforced by a unique constraint on ParticipantId + EventId).
6. **Results** - 1:1 with Enrolments; captures finish time and finishing position once
   an event has taken place.

## Consistency note

`schema.sql` matches `erd.png` exactly - the same six entities, the same primary/foreign
keys, and the same cardinalities (1:1 for Users-UserProfiles and Enrolments-Results;
1:M everywhere else). No deliberate deviations were introduced between the diagram and
the script.

<img width="944" height="471" alt="image" src="https://github.com/user-attachments/assets/657a9763-3270-481e-a8db-0aad4d1769ba" />

