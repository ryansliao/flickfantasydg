IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [Players] (
        [PDGANumber] int NOT NULL,
        [ResultId] int NOT NULL,
        [Name] nvarchar(max) NULL,
        CONSTRAINT [PK_Players] PRIMARY KEY ([PDGANumber])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [Tournaments] (
        [Id] int NOT NULL,
        [Division] nvarchar(450) NOT NULL,
        [Date] datetime2 NOT NULL,
        [Name] nvarchar(max) NULL,
        [Weight] float NOT NULL,
        CONSTRAINT [PK_Tournaments] PRIMARY KEY ([Id], [Division])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [Leagues] (
        [LeagueId] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [OwnerId] nvarchar(450) NOT NULL,
        [PlayerNumber] int NOT NULL,
        [PlacementWeight] float NOT NULL,
        [FairwayWeight] float NOT NULL,
        [C1InRegWeight] float NOT NULL,
        [C2InRegWeight] float NOT NULL,
        [ParkedWeight] float NOT NULL,
        [ScrambleWeight] float NOT NULL,
        [C1PuttWeight] float NOT NULL,
        [C1xPuttWeight] float NOT NULL,
        [C2PuttWeight] float NOT NULL,
        [OBWeight] float NOT NULL,
        [BirdieWeight] float NOT NULL,
        [BirdieMinusWeight] float NOT NULL,
        [EagleMinusWeight] float NOT NULL,
        [ParWeight] float NOT NULL,
        [BogeyPlusWeight] float NOT NULL,
        [DoubleBogeyPlusWeight] float NOT NULL,
        [TotalPuttDistWeight] float NOT NULL,
        [AvgPuttDistWeight] float NOT NULL,
        [LongThrowInWeight] float NOT NULL,
        [TotalSGWeight] float NOT NULL,
        [PuttingSGWeight] float NOT NULL,
        [TeeToGreenSGWeight] float NOT NULL,
        [C1xSGWeight] float NOT NULL,
        [C2SGWeight] float NOT NULL,
        CONSTRAINT [PK_Leagues] PRIMARY KEY ([LeagueId]),
        CONSTRAINT [FK_Leagues_AspNetUsers_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [PlayerTournaments] (
        [ResultId] int NOT NULL,
        [PDGANumber] int NOT NULL,
        [TournamentId] int NOT NULL,
        [Division] nvarchar(450) NOT NULL,
        [Place] int NOT NULL,
        [TotalToPar] int NOT NULL,
        [Fairway] float NOT NULL,
        [C1InReg] float NOT NULL,
        [C2InReg] float NOT NULL,
        [Parked] float NOT NULL,
        [Scramble] float NOT NULL,
        [C1Putting] float NOT NULL,
        [C1xPutting] float NOT NULL,
        [C2Putting] float NOT NULL,
        [ObRate] float NOT NULL,
        [BirdieMinus] float NOT NULL,
        [DoubleBogeyPlus] float NOT NULL,
        [BogeyPlus] float NOT NULL,
        [Par] float NOT NULL,
        [Birdie] float NOT NULL,
        [EagleMinus] float NOT NULL,
        [TotalPuttDistance] int NOT NULL,
        [LongThrowIn] int NOT NULL,
        [AvgPuttDistance] float NOT NULL,
        [StrokesGainedTotal] float NOT NULL,
        [StrokesGainedTeeToGreen] float NOT NULL,
        [StrokesGainedPutting] float NOT NULL,
        [StrokesGainedC1xPutting] float NOT NULL,
        [StrokesGainedC2Putting] float NOT NULL,
        CONSTRAINT [PK_PlayerTournaments] PRIMARY KEY ([ResultId]),
        CONSTRAINT [FK_PlayerTournaments_Players_PDGANumber] FOREIGN KEY ([PDGANumber]) REFERENCES [Players] ([PDGANumber]) ON DELETE CASCADE,
        CONSTRAINT [FK_PlayerTournaments_Tournaments_TournamentId_Division] FOREIGN KEY ([TournamentId], [Division]) REFERENCES [Tournaments] ([Id], [Division]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [LeagueInvitations] (
        [LeagueInvitationId] int NOT NULL IDENTITY,
        [LeagueId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [SentAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LeagueInvitations] PRIMARY KEY ([LeagueInvitationId]),
        CONSTRAINT [FK_LeagueInvitations_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LeagueInvitations_Leagues_LeagueId] FOREIGN KEY ([LeagueId]) REFERENCES [Leagues] ([LeagueId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [LeagueMembers] (
        [LeagueId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_LeagueMembers] PRIMARY KEY ([LeagueId], [UserId]),
        CONSTRAINT [FK_LeagueMembers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LeagueMembers_Leagues_LeagueId] FOREIGN KEY ([LeagueId]) REFERENCES [Leagues] ([LeagueId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [LeagueOwnershipTransfers] (
        [LeagueOwnershipTransferId] int NOT NULL IDENTITY,
        [LeagueId] int NOT NULL,
        [NewOwnerId] nvarchar(450) NOT NULL,
        [RequestedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LeagueOwnershipTransfers] PRIMARY KEY ([LeagueOwnershipTransferId]),
        CONSTRAINT [FK_LeagueOwnershipTransfers_AspNetUsers_NewOwnerId] FOREIGN KEY ([NewOwnerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LeagueOwnershipTransfers_Leagues_LeagueId] FOREIGN KEY ([LeagueId]) REFERENCES [Leagues] ([LeagueId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [LeaguePlayerFantasyPoints] (
        [LeagueId] int NOT NULL,
        [PDGANumber] int NOT NULL,
        [TournamentId] int NOT NULL,
        [LeaguePDGANumber] int NOT NULL,
        [Division] nvarchar(450) NOT NULL,
        [Points] real NOT NULL,
        CONSTRAINT [PK_LeaguePlayerFantasyPoints] PRIMARY KEY ([LeagueId], [PDGANumber], [TournamentId]),
        CONSTRAINT [FK_LeaguePlayerFantasyPoints_Leagues_LeagueId] FOREIGN KEY ([LeagueId]) REFERENCES [Leagues] ([LeagueId]) ON DELETE CASCADE,
        CONSTRAINT [FK_LeaguePlayerFantasyPoints_Players_PDGANumber] FOREIGN KEY ([PDGANumber]) REFERENCES [Players] ([PDGANumber]) ON DELETE CASCADE,
        CONSTRAINT [FK_LeaguePlayerFantasyPoints_Tournaments_TournamentId_Division] FOREIGN KEY ([TournamentId], [Division]) REFERENCES [Tournaments] ([Id], [Division]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [Teams] (
        [TeamId] int NOT NULL IDENTITY,
        [Name] nvarchar(20) NOT NULL,
        [LeagueId] int NOT NULL,
        [OwnerId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_Teams] PRIMARY KEY ([TeamId]),
        CONSTRAINT [FK_Teams_AspNetUsers_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Teams_Leagues_LeagueId] FOREIGN KEY ([LeagueId]) REFERENCES [Leagues] ([LeagueId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE TABLE [TeamPlayers] (
        [TeamId] int NOT NULL,
        [PDGANumber] int NOT NULL,
        [LeagueId] int NOT NULL,
        CONSTRAINT [PK_TeamPlayers] PRIMARY KEY ([TeamId], [PDGANumber]),
        CONSTRAINT [FK_TeamPlayers_Players_PDGANumber] FOREIGN KEY ([PDGANumber]) REFERENCES [Players] ([PDGANumber]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TeamPlayers_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([TeamId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeagueInvitations_LeagueId] ON [LeagueInvitations] ([LeagueId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeagueInvitations_UserId] ON [LeagueInvitations] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeagueMembers_UserId] ON [LeagueMembers] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeagueOwnershipTransfers_LeagueId] ON [LeagueOwnershipTransfers] ([LeagueId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeagueOwnershipTransfers_NewOwnerId] ON [LeagueOwnershipTransfers] ([NewOwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeaguePlayerFantasyPoints_PDGANumber] ON [LeaguePlayerFantasyPoints] ([PDGANumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeaguePlayerFantasyPoints_TournamentId_Division] ON [LeaguePlayerFantasyPoints] ([TournamentId], [Division]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Leagues_OwnerId] ON [Leagues] ([OwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlayerTournaments_PDGANumber] ON [PlayerTournaments] ([PDGANumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlayerTournaments_TournamentId_Division] ON [PlayerTournaments] ([TournamentId], [Division]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TeamPlayers_PDGANumber] ON [TeamPlayers] ([PDGANumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TeamPlayers_TeamId_PDGANumber] ON [TeamPlayers] ([TeamId], [PDGANumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Teams_LeagueId_OwnerId] ON [Teams] ([LeagueId], [OwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Teams_OwnerId] ON [Teams] ([OwnerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604021856_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250604021856_InitialCreate', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604050356_RosterSpots'
)
BEGIN
    ALTER TABLE [TeamPlayers] ADD [Status] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604050356_RosterSpots'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250604050356_RosterSpots', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604051921_RosterSettings'
)
BEGIN
    ALTER TABLE [Leagues] ADD [InjuryReserveLimit] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604051921_RosterSettings'
)
BEGIN
    ALTER TABLE [Leagues] ADD [RosterLimit] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604051921_RosterSettings'
)
BEGIN
    ALTER TABLE [Leagues] ADD [StarterCount] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604051921_RosterSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250604051921_RosterSettings', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604165900_TeamPlayerTournament'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250604165900_TeamPlayerTournament', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604170359_TeamPlayerTournamentRelations'
)
BEGIN
    CREATE TABLE [TeamPlayerTournaments] (
        [TeamId] int NOT NULL,
        [PDGANumber] int NOT NULL,
        [TournamentId] int NOT NULL,
        [Division] nvarchar(450) NOT NULL,
        [IsLocked] bit NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_TeamPlayerTournaments] PRIMARY KEY ([TeamId], [PDGANumber], [TournamentId], [Division]),
        CONSTRAINT [FK_TeamPlayerTournaments_Players_PDGANumber] FOREIGN KEY ([PDGANumber]) REFERENCES [Players] ([PDGANumber]) ON DELETE CASCADE,
        CONSTRAINT [FK_TeamPlayerTournaments_Teams_TeamId] FOREIGN KEY ([TeamId]) REFERENCES [Teams] ([TeamId]) ON DELETE CASCADE,
        CONSTRAINT [FK_TeamPlayerTournaments_Tournaments_TournamentId_Division] FOREIGN KEY ([TournamentId], [Division]) REFERENCES [Tournaments] ([Id], [Division]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604170359_TeamPlayerTournamentRelations'
)
BEGIN
    CREATE INDEX [IX_TeamPlayerTournaments_PDGANumber] ON [TeamPlayerTournaments] ([PDGANumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604170359_TeamPlayerTournamentRelations'
)
BEGIN
    CREATE INDEX [IX_TeamPlayerTournaments_TournamentId_Division] ON [TeamPlayerTournaments] ([TournamentId], [Division]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250604170359_TeamPlayerTournamentRelations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250604170359_TeamPlayerTournamentRelations', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620224359_RoundNumber'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tournaments]') AND [c].[name] = N'Weight');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Tournaments] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Tournaments] DROP COLUMN [Weight];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620224359_RoundNumber'
)
BEGIN
    ALTER TABLE [Tournaments] ADD [RoundNumber] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620224359_RoundNumber'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250620224359_RoundNumber', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620225440_Tier'
)
BEGIN
    ALTER TABLE [Tournaments] ADD [Tier] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620225440_Tier'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250620225440_Tier', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620231852_scoringmode'
)
BEGIN
    ALTER TABLE [Leagues] ADD [LeagueScoringMode] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620231852_scoringmode'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250620231852_scoringmode', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620232211_weight'
)
BEGIN
    ALTER TABLE [Tournaments] ADD [Weight] float NOT NULL DEFAULT 0.0E0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620232211_weight'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250620232211_weight', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620233841_divisionoptions'
)
BEGIN
    ALTER TABLE [Leagues] ADD [IncludeFPO] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620233841_divisionoptions'
)
BEGIN
    ALTER TABLE [Leagues] ADD [IncludeMPO] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250620233841_divisionoptions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250620233841_divisionoptions', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250621161825_defaultweight'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tournaments]') AND [c].[name] = N'Weight');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Tournaments] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Tournaments] ADD DEFAULT 1.0E0 FOR [Weight];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250621161825_defaultweight'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250621161825_defaultweight', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250621170654_IRremove'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Leagues]') AND [c].[name] = N'InjuryReserveLimit');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Leagues] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Leagues] DROP COLUMN [InjuryReserveLimit];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250621170654_IRremove'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250621170654_IRremove', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250621172227_BenchCount'
)
BEGIN
    EXEC sp_rename N'[Leagues].[RosterLimit]', N'BenchCount', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250621172227_BenchCount'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250621172227_BenchCount', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250621190800_TeamPoints'
)
BEGIN
    ALTER TABLE [Teams] ADD [Points] float NOT NULL DEFAULT 0.0E0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250621190800_TeamPoints'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250621190800_TeamPoints', N'9.0.5');
END;

COMMIT;
GO

