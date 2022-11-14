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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221019013438_Autores')
BEGIN
    CREATE TABLE [Autores] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(max) NULL,
        CONSTRAINT [PK_Autores] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221019013438_Autores')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221019013438_Autores', N'6.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221019125519_ActualizacionAutores')
BEGIN
    ALTER TABLE [Autores] ADD [Apellido] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221019125519_ActualizacionAutores')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221019125519_ActualizacionAutores', N'6.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221019134619_Libros')
BEGIN
    CREATE TABLE [Libros] (
        [Id] int NOT NULL IDENTITY,
        [Titulo] nvarchar(max) NULL,
        [URLPDF] nvarchar(max) NULL,
        [AutorId] int NOT NULL,
        CONSTRAINT [PK_Libros] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Libros_Autores_AutorId] FOREIGN KEY ([AutorId]) REFERENCES [Autores] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221019134619_Libros')
BEGIN
    CREATE INDEX [IX_Libros_AutorId] ON [Libros] ([AutorId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221019134619_Libros')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221019134619_Libros', N'6.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221019141214_CorreccionAutor')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221019141214_CorreccionAutor', N'6.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221020130645_Inicial')
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Libros]') AND [c].[name] = N'URLPDF');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Libros] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Libros] ALTER COLUMN [URLPDF] nvarchar(max) NOT NULL;
    ALTER TABLE [Libros] ADD DEFAULT N'' FOR [URLPDF];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221020130645_Inicial')
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Libros]') AND [c].[name] = N'Titulo');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Libros] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Libros] ALTER COLUMN [Titulo] nvarchar(max) NOT NULL;
    ALTER TABLE [Libros] ADD DEFAULT N'' FOR [Titulo];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221020130645_Inicial')
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Autores]') AND [c].[name] = N'Nombre');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Autores] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Autores] ALTER COLUMN [Nombre] nvarchar(50) NOT NULL;
    ALTER TABLE [Autores] ADD DEFAULT N'' FOR [Nombre];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221020130645_Inicial')
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Autores]') AND [c].[name] = N'Apellido');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Autores] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Autores] ALTER COLUMN [Apellido] nvarchar(50) NOT NULL;
    ALTER TABLE [Autores] ADD DEFAULT N'' FOR [Apellido];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221020130645_Inicial')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221020130645_Inicial', N'6.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221025114424_Comentarios')
BEGIN
    CREATE TABLE [Comentarios] (
        [Id] int NOT NULL IDENTITY,
        [Contenido] nvarchar(max) NOT NULL,
        [LibroId] int NOT NULL,
        CONSTRAINT [PK_Comentarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comentarios_Libros_LibroId] FOREIGN KEY ([LibroId]) REFERENCES [Libros] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221025114424_Comentarios')
BEGIN
    CREATE INDEX [IX_Comentarios_LibroId] ON [Comentarios] ([LibroId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221025114424_Comentarios')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221025114424_Comentarios', N'6.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221028113530_RelacionMuchosMuchos')
BEGIN
    ALTER TABLE [Libros] DROP CONSTRAINT [FK_Libros_Autores_AutorId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221028113530_RelacionMuchosMuchos')
BEGIN
    DROP INDEX [IX_Libros_AutorId] ON [Libros];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221028113530_RelacionMuchosMuchos')
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Libros]') AND [c].[name] = N'AutorId');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Libros] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Libros] DROP COLUMN [AutorId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221028113530_RelacionMuchosMuchos')
BEGIN
    CREATE TABLE [AutoresLibros] (
        [AutorId] int NOT NULL,
        [LibroId] int NOT NULL,
        CONSTRAINT [PK_AutoresLibros] PRIMARY KEY ([AutorId], [LibroId]),
        CONSTRAINT [FK_AutoresLibros_Autores_AutorId] FOREIGN KEY ([AutorId]) REFERENCES [Autores] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AutoresLibros_Libros_LibroId] FOREIGN KEY ([LibroId]) REFERENCES [Libros] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221028113530_RelacionMuchosMuchos')
BEGIN
    CREATE INDEX [IX_AutoresLibros_LibroId] ON [AutoresLibros] ([LibroId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221028113530_RelacionMuchosMuchos')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221028113530_RelacionMuchosMuchos', N'6.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221031141639_FotoBiografiaAutores')
BEGIN
    ALTER TABLE [Autores] ADD [SourceBiografia] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221031141639_FotoBiografiaAutores')
BEGIN
    ALTER TABLE [Autores] ADD [SourceFoto] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221031141639_FotoBiografiaAutores')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221031141639_FotoBiografiaAutores', N'6.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221031143103_FechaNacimientoAutores')
BEGIN
    ALTER TABLE [Autores] ADD [FechaNacimiento] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221031143103_FechaNacimientoAutores')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221031143103_FechaNacimientoAutores', N'6.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221031144914_ActualizacionLibros')
BEGIN
    ALTER TABLE [Libros] ADD [FechaPublicacion] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221031144914_ActualizacionLibros')
BEGIN
    ALTER TABLE [Libros] ADD [SourcePortada] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221031144914_ActualizacionLibros')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221031144914_ActualizacionLibros', N'6.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221107111701_ActualizacionLibrosURL')
BEGIN
    EXEC sp_rename N'[Libros].[URLPDF]', N'URLIden', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221107111701_ActualizacionLibrosURL')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221107111701_ActualizacionLibrosURL', N'6.0.10');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20221107114435_CorreccionIdlibrosUrl')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20221107114435_CorreccionIdlibrosUrl', N'6.0.10');
END;
GO

COMMIT;
GO

