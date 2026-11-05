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
GO

CREATE TABLE [Amenities] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Amenities] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Cottages] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    [Location] nvarchar(250) NOT NULL,
    [PricePerNight] decimal(10,2) NOT NULL,
    [MaxGuests] int NOT NULL,
    [BookingVersion] int NOT NULL,
    [RowVersion] rowversion NOT NULL,
    CONSTRAINT [PK_Cottages] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Cottages_MaxGuests] CHECK ([MaxGuests] > 0),
    CONSTRAINT [CK_Cottages_PricePerNight] CHECK ([PricePerNight] > 0)
);
GO

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(150) NOT NULL,
    [Email] nvarchar(320) NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [Role] nvarchar(30) NOT NULL DEFAULT N'Customer',
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [CottageAmenities] (
    [CottageId] int NOT NULL,
    [AmenityId] int NOT NULL,
    CONSTRAINT [PK_CottageAmenities] PRIMARY KEY ([CottageId], [AmenityId]),
    CONSTRAINT [FK_CottageAmenities_Amenities_AmenityId] FOREIGN KEY ([AmenityId]) REFERENCES [Amenities] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CottageAmenities_Cottages_CottageId] FOREIGN KEY ([CottageId]) REFERENCES [Cottages] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CottageBookings] (
    [Id] int NOT NULL IDENTITY,
    [CottageId] int NOT NULL,
    [UserId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] nvarchar(30) NOT NULL DEFAULT N'Pending',
    CONSTRAINT [PK_CottageBookings] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_CottageBookings_DateRange] CHECK ([StartDate] < [EndDate]),
    CONSTRAINT [FK_CottageBookings_Cottages_CottageId] FOREIGN KEY ([CottageId]) REFERENCES [Cottages] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CottageBookings_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Amenities]'))
    SET IDENTITY_INSERT [Amenities] ON;
INSERT INTO [Amenities] ([Id], [Name])
VALUES (1, N'Wi-Fi'),
(2, N'Pool'),
(3, N'Hot tub');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Amenities]'))
    SET IDENTITY_INSERT [Amenities] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BookingVersion', N'Location', N'MaxGuests', N'Name', N'PricePerNight') AND [object_id] = OBJECT_ID(N'[Cottages]'))
    SET IDENTITY_INSERT [Cottages] ON;
INSERT INTO [Cottages] ([Id], [BookingVersion], [Location], [MaxGuests], [Name], [PricePerNight])
VALUES (1, 0, N'North', 4, N'Mountain Cottage', 450.0),
(2, 0, N'Center', 3, N'Garden Cottage', 380.0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BookingVersion', N'Location', N'MaxGuests', N'Name', N'PricePerNight') AND [object_id] = OBJECT_ID(N'[Cottages]'))
    SET IDENTITY_INSERT [Cottages] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'AmenityId', N'CottageId') AND [object_id] = OBJECT_ID(N'[CottageAmenities]'))
    SET IDENTITY_INSERT [CottageAmenities] ON;
INSERT INTO [CottageAmenities] ([AmenityId], [CottageId])
VALUES (1, 1),
(3, 1),
(1, 2),
(2, 2);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'AmenityId', N'CottageId') AND [object_id] = OBJECT_ID(N'[CottageAmenities]'))
    SET IDENTITY_INSERT [CottageAmenities] OFF;
GO

CREATE INDEX [IX_CottageAmenities_AmenityId] ON [CottageAmenities] ([AmenityId]);
GO

CREATE INDEX [IX_CottageBookings_CottageId_StartDate_EndDate] ON [CottageBookings] ([CottageId], [StartDate], [EndDate]);
GO

CREATE INDEX [IX_CottageBookings_UserId] ON [CottageBookings] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261031231540_InitialCreate', N'8.0.22');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [CottageBookings] ADD [GuestsCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [CottageBookings] ADD [Notes] nvarchar(1000) NULL;
GO

ALTER TABLE [CottageBookings] ADD CONSTRAINT [CK_CottageBookings_GuestsCount] CHECK ([GuestsCount] > 0);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261102040428_AddBookingGuestDetails', N'8.0.22');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [CottageBookings] DROP CONSTRAINT [FK_CottageBookings_Cottages_CottageId];
GO

ALTER TABLE [CottageBookings] DROP CONSTRAINT [FK_CottageBookings_Users_UserId];
GO

DROP INDEX [IX_Users_Email] ON [Users];
GO

ALTER TABLE [Cottages] DROP CONSTRAINT [CK_Cottages_MaxGuests];
GO

ALTER TABLE [Cottages] DROP CONSTRAINT [CK_Cottages_PricePerNight];
GO

DROP INDEX [IX_CottageBookings_CottageId_StartDate_EndDate] ON [CottageBookings];
GO

ALTER TABLE [CottageBookings] DROP CONSTRAINT [CK_CottageBookings_DateRange];
GO

ALTER TABLE [CottageBookings] DROP CONSTRAINT [CK_CottageBookings_GuestsCount];
GO

DELETE FROM [CottageAmenities]
WHERE [AmenityId] = 1 AND [CottageId] = 1;
SELECT @@ROWCOUNT;

GO

DELETE FROM [CottageAmenities]
WHERE [AmenityId] = 3 AND [CottageId] = 1;
SELECT @@ROWCOUNT;

GO

DELETE FROM [CottageAmenities]
WHERE [AmenityId] = 1 AND [CottageId] = 2;
SELECT @@ROWCOUNT;

GO

DELETE FROM [CottageAmenities]
WHERE [AmenityId] = 2 AND [CottageId] = 2;
SELECT @@ROWCOUNT;

GO

DELETE FROM [Amenities]
WHERE [Id] = 1;
SELECT @@ROWCOUNT;

GO

DELETE FROM [Amenities]
WHERE [Id] = 2;
SELECT @@ROWCOUNT;

GO

DELETE FROM [Amenities]
WHERE [Id] = 3;
SELECT @@ROWCOUNT;

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Role');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Users] ALTER COLUMN [Role] nvarchar(max) NOT NULL;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'PasswordHash');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Users] ALTER COLUMN [PasswordHash] nvarchar(max) NOT NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'FullName');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Users] ALTER COLUMN [FullName] nvarchar(max) NOT NULL;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Email');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Users] ALTER COLUMN [Email] nvarchar(max) NOT NULL;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cottages]') AND [c].[name] = N'PricePerNight');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Cottages] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Cottages] ALTER COLUMN [PricePerNight] decimal(18,2) NOT NULL;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cottages]') AND [c].[name] = N'Name');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Cottages] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Cottages] ALTER COLUMN [Name] nvarchar(max) NOT NULL;
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Cottages]') AND [c].[name] = N'Location');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Cottages] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Cottages] ALTER COLUMN [Location] nvarchar(max) NOT NULL;
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CottageBookings]') AND [c].[name] = N'Status');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [CottageBookings] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [CottageBookings] ALTER COLUMN [Status] nvarchar(max) NOT NULL;
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CottageBookings]') AND [c].[name] = N'Notes');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [CottageBookings] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [CottageBookings] ALTER COLUMN [Notes] nvarchar(max) NULL;
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Amenities]') AND [c].[name] = N'Name');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Amenities] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Amenities] ALTER COLUMN [Name] nvarchar(max) NOT NULL;
GO

UPDATE [Cottages] SET [Location] = N'אמירים', [Name] = N'אחוזת העץ בערפל', [PricePerNight] = 850.0
WHERE [Id] = 1;
SELECT @@ROWCOUNT;

GO

UPDATE [Cottages] SET [Location] = N'מג''דל שמס', [MaxGuests] = 2, [Name] = N'בקתת האורנים בחרמון', [PricePerNight] = 650.0
WHERE [Id] = 2;
SELECT @@ROWCOUNT;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BookingVersion', N'Location', N'MaxGuests', N'Name', N'PricePerNight') AND [object_id] = OBJECT_ID(N'[Cottages]'))
    SET IDENTITY_INSERT [Cottages] ON;
INSERT INTO [Cottages] ([Id], [BookingVersion], [Location], [MaxGuests], [Name], [PricePerNight])
VALUES (3, 0, N'מצפה רמון', 5, N'סוויטת נוף מדברי', 950.0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'BookingVersion', N'Location', N'MaxGuests', N'Name', N'PricePerNight') AND [object_id] = OBJECT_ID(N'[Cottages]'))
    SET IDENTITY_INSERT [Cottages] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Email', N'FullName', N'PasswordHash', N'Role') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([Id], [Email], [FullName], [PasswordHash], [Role])
VALUES (1, N'naomi.avni21@gmail.com', N'נעמי אבני', N'Naomi216421370', N'Admin');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Email', N'FullName', N'PasswordHash', N'Role') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;
GO

CREATE INDEX [IX_CottageBookings_CottageId] ON [CottageBookings] ([CottageId]);
GO

ALTER TABLE [CottageBookings] ADD CONSTRAINT [FK_CottageBookings_Cottages_CottageId] FOREIGN KEY ([CottageId]) REFERENCES [Cottages] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [CottageBookings] ADD CONSTRAINT [FK_CottageBookings_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261106113510_AddCottageSeed', N'8.0.22');
GO

COMMIT;
GO

