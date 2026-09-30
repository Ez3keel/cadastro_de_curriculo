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
    WHERE [MigrationId] = N'20260930143858_InitialCreate'
)
BEGIN
    CREATE TABLE [Candidatos] (
        [Id] int NOT NULL IDENTITY,
        [NomeCompleto] nvarchar(150) NOT NULL,
        [Email] nvarchar(254) NOT NULL,
        [Telefone] nvarchar(20) NULL,
        [AreaInteresse] nvarchar(100) NULL,
        [ResumoProfissional] nvarchar(2000) NULL,
        [CriadoEm] datetime2 NOT NULL,
        CONSTRAINT [PK_Candidatos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930143858_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Candidatos_Email] ON [Candidatos] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260930143858_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260930143858_InitialCreate', N'10.0.12');
END;

COMMIT;
GO

