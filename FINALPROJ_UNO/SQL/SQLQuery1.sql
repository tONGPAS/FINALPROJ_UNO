-- UNO Card Game Database Setup
-- Run this in SQL Server Object Explorer > New Query
-- Server: (localdb)\MSSQLLocalDB

-- Create database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'UNOCardGame')
    CREATE DATABASE UNOCardGame;
GO

USE UNOCardGame;
GO

-- ── Players ───────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Players' AND xtype='U')
CREATE TABLE Players (
    PlayerId    INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(50)  NOT NULL UNIQUE,
    TotalWins   INT           NOT NULL DEFAULT 0,
    TotalGames  INT           NOT NULL DEFAULT 0,
    TotalScore  INT           NOT NULL DEFAULT 0,
    CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ── GameSessions ──────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='GameSessions' AND xtype='U')
CREATE TABLE GameSessions (
    SessionId    INT IDENTITY(1,1) PRIMARY KEY,
    GameMode     NVARCHAR(20)  NOT NULL,  -- 'Solo', '2P', '3P', 'Mixed'
    TotalRounds  INT           NOT NULL DEFAULT 0,
    WinnerName   NVARCHAR(50)  NOT NULL,
    StartTime    DATETIME      NOT NULL DEFAULT GETDATE(),
    EndTime      DATETIME      NULL
);
GO

-- ── SessionPlayers ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SessionPlayers' AND xtype='U')
CREATE TABLE SessionPlayers (
    SessionPlayerId  INT IDENTITY(1,1) PRIMARY KEY,
    SessionId        INT           NOT NULL FOREIGN KEY REFERENCES GameSessions(SessionId),
    PlayerId         INT           NULL     FOREIGN KEY REFERENCES Players(PlayerId),
    PlayerName       NVARCHAR(50)  NOT NULL,
    IsAI             BIT           NOT NULL DEFAULT 0,
    AIDifficulty     NVARCHAR(10)  NULL,    -- 'Easy', 'Medium', 'Hard'
    FinalScore       INT           NOT NULL DEFAULT 0,
    Placement        INT           NOT NULL DEFAULT 0   -- 1 = winner
);
GO

-- ── Rounds ────────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Rounds' AND xtype='U')
CREATE TABLE Rounds (
    RoundId      INT IDENTITY(1,1) PRIMARY KEY,
    SessionId    INT           NOT NULL FOREIGN KEY REFERENCES GameSessions(SessionId),
    RoundNumber  INT           NOT NULL,
    WinnerName   NVARCHAR(50)  NOT NULL,
    PointsScored INT           NOT NULL DEFAULT 0,
    Duration     INT           NOT NULL DEFAULT 0  -- seconds
);
GO

-- ── MoveLogs ──────────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MoveLogs' AND xtype='U')
CREATE TABLE MoveLogs (
    MoveId       INT IDENTITY(1,1) PRIMARY KEY,
    SessionId    INT           NOT NULL FOREIGN KEY REFERENCES GameSessions(SessionId),
    RoundNumber  INT           NOT NULL,
    PlayerName   NVARCHAR(50)  NOT NULL,
    MoveType     NVARCHAR(20)  NOT NULL,  -- 'Play', 'Draw', 'UNO', 'Challenge'
    CardPlayed   NVARCHAR(30)  NULL,      -- e.g. 'Red 7', 'Wild Draw Four'
    ColorChosen  NVARCHAR(10)  NULL,      -- color chosen after Wild
    Timestamp    DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- ── Verify all tables created ─────────────────────────────────────────────────
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_CATALOG = 'UNOCardGame';
GO