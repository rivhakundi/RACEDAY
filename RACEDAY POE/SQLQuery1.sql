/*ZZS ============================================================
   RaceDay Database Schema
   SQL Server Management Studio (SSMS)
   Matches erd.png exactly - 6 entities, all PKs/FKs/constraints
   ============================================================ */

IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END
GO

USE RaceDayDB;
GO

/* Drop tables in FK-safe order if re-running the script */
IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL DROP TABLE dbo.Enrolments;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.UserProfiles', 'U') IS NOT NULL DROP TABLE dbo.UserProfiles;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

/* ============================================================
   1. Users
   ============================================================ */
CREATE TABLE dbo.Users (
    UserId          INT IDENTITY(1,1)      NOT NULL,
    FullName        NVARCHAR(100)          NOT NULL,
    Email           NVARCHAR(150)          NOT NULL,
    PasswordHash    NVARCHAR(255)          NOT NULL,
    Role            NVARCHAR(20)           NOT NULL DEFAULT 'Participant',
    CreatedAt       DATETIME               NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Organiser', 'Participant'))
);
GO

/* ============================================================
   2. UserProfiles  (1:1 with Users)
   ============================================================ */
CREATE TABLE dbo.UserProfiles (
    ProfileId       INT IDENTITY(1,1)      NOT NULL,
    UserId          INT                    NOT NULL,
    PhoneNumber     NVARCHAR(20)           NULL,
    DateOfBirth     DATE                   NULL,
    Bio             NVARCHAR(500)          NULL,
    CONSTRAINT PK_UserProfiles PRIMARY KEY (ProfileId),
    CONSTRAINT UQ_UserProfiles_UserId UNIQUE (UserId),
    CONSTRAINT FK_UserProfiles_Users FOREIGN KEY (UserId)
        REFERENCES dbo.Users (UserId) ON DELETE CASCADE
);
GO

/* ============================================================
   3. Events  (1:M from Users as Organiser)
   ============================================================ */
CREATE TABLE dbo.Events (
    EventId         INT IDENTITY(1,1)      NOT NULL,
    OrganiserId     INT                    NOT NULL,
    Name            NVARCHAR(150)          NOT NULL,
    Description     NVARCHAR(1000)         NULL,
    EventDate       DATE                   NOT NULL,
    Location        NVARCHAR(200)          NOT NULL,
    Distance        DECIMAL(6,2)           NOT NULL,
    EventType       NVARCHAR(20)           NOT NULL,
    CreatedAt       DATETIME               NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Events PRIMARY KEY (EventId),
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (OrganiserId)
        REFERENCES dbo.Users (UserId),
    CONSTRAINT CK_Events_EventType CHECK (EventType IN ('Run', 'Walk', 'Cycle')),
    CONSTRAINT CK_Events_Distance CHECK (Distance > 0)
);
GO

/* ============================================================
   4. Categories  (1:M from Events)
   ============================================================ */
CREATE TABLE dbo.Categories (
    CategoryId          INT IDENTITY(1,1)  NOT NULL,
    EventId             INT                NOT NULL,
    Name                NVARCHAR(100)      NOT NULL,
    MinAge              INT                NULL,
    MaxAge              INT                NULL,
    CategoryDistance    DECIMAL(6,2)       NULL,
    CONSTRAINT PK_Categories PRIMARY KEY (CategoryId),
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId)
        REFERENCES dbo.Events (EventId) ON DELETE CASCADE
);
GO

/* ============================================================
   5. Enrolments  (M:1 Users-as-Participant, M:1 Events, M:1 Categories)
   ============================================================ */
CREATE TABLE dbo.Enrolments (
    EnrolmentId     INT IDENTITY(1,1)      NOT NULL,
    ParticipantId   INT                    NOT NULL,
    EventId         INT                    NOT NULL,
    CategoryId      INT                    NOT NULL,
    EnrolmentDate   DATETIME               NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Enrolments PRIMARY KEY (EnrolmentId),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantId)
        REFERENCES dbo.Users (UserId),
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventId)
        REFERENCES dbo.Events (EventId),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId)
        REFERENCES dbo.Categories (CategoryId),
    CONSTRAINT UQ_Enrolments_Participant_Event UNIQUE (ParticipantId, EventId)
);
GO

/* ============================================================
   6. Results  (1:1 with Enrolments)
   ============================================================ */
CREATE TABLE dbo.Results (
    ResultId            INT IDENTITY(1,1)  NOT NULL,
    EnrolmentId         INT                NOT NULL,
    FinishTime          TIME               NULL,
    FinishingPosition   INT                NULL,
    CapturedAt          DATETIME           NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Results PRIMARY KEY (ResultId),
    CONSTRAINT UQ_Results_EnrolmentId UNIQUE (EnrolmentId),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId)
        REFERENCES dbo.Enrolments (EnrolmentId) ON DELETE CASCADE
);
GO

/* ============================================================
   SEED DATA
   Note: PasswordHash values below are placeholder bcrypt-style
   strings for seed/demo purposes only - real hashes are produced
   by the API's registration endpoint, not typed here directly.
   ============================================================ */

-- 2 Organisers + 2 Participants
INSERT INTO dbo.Users (FullName, Email, PasswordHash, Role) VALUES
('Sarah Nkosi',     'sarah.nkosi@raceday.co.za',   '$2a$11$examplehash.organiser.one', 'Organiser'),
('David Botha',     'david.botha@raceday.co.za',   '$2a$11$examplehash.organiser.two', 'Organiser'),
('Lindiwe Dube',    'lindiwe.dube@example.com',    '$2a$11$examplehash.participant.a', 'Participant'),
('Michael Reddy',   'michael.reddy@example.com',   '$2a$11$examplehash.participant.b', 'Participant');
GO

INSERT INTO dbo.UserProfiles (UserId, PhoneNumber, DateOfBirth, Bio) VALUES
(1, '0821234567', '1985-03-12', 'Event organiser specialising in road running.'),
(2, '0837654321', '1979-11-02', 'Organiser for cycling and multi-sport events.'),
(3, '0715558899', '1998-06-21', 'Keen 10km and half-marathon runner.'),
(4, '0723334455', '1990-01-15', 'Weekend cyclist training for first century ride.');
GO

-- 3 Events (owned by the 2 organisers)
INSERT INTO dbo.Events (OrganiserId, Name, Description, EventDate, Location, Distance, EventType) VALUES
(1, 'Johannesburg City Run',   'Annual road running event through the city centre.', '2026-09-12', 'Johannesburg, Gauteng', 21.10, 'Run'),
(1, 'Sunrise Fun Walk',        'Community walk supporting local charities.',         '2026-08-30', 'Pretoria, Gauteng',     5.00,  'Walk'),
(2, 'Highveld Cycle Challenge','Road cycling race across the Highveld region.',      '2026-10-04', 'Bronkhorstspruit, Gauteng', 90.00, 'Cycle');
GO

-- Categories per event
INSERT INTO dbo.Categories (EventId, Name, MinAge, MaxAge, CategoryDistance) VALUES
(1, 'Senior 21km',   20, 39, 21.10),
(1, 'Veteran 21km',  40, 99, 21.10),
(1, 'Under 20',      13, 19, 21.10),
(2, '5km Walk',       0, 99, 5.00),
(3, '90km Road Race', 18, 99, 90.00),
(3, '45km Fun Ride',  14, 99, 45.00);
GO

-- Sample enrolments
INSERT INTO dbo.Enrolments (ParticipantId, EventId, CategoryId) VALUES
(3, 1, 1),   -- Lindiwe enters Johannesburg City Run, Senior 21km
(4, 1, 2),   -- Michael enters Johannesburg City Run, Veteran 21km
(3, 3, 5),   -- Lindiwe enters Highveld Cycle Challenge, 90km Road Race
(4, 2, 4);   -- Michael enters Sunrise Fun Walk, 5km Walk
GO

-- Sample results for a couple of completed enrolments
INSERT INTO dbo.Results (EnrolmentId, FinishTime, FinishingPosition) VALUES
(1, '01:45:32', 12),
(2, '01:58:10', 27);
GO