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
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Category] nvarchar(max) NULL,
        [Message] nvarchar(max) NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE TABLE [Participants] (
        [Id] int NOT NULL IDENTITY,
        [Type] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [CompanyName] nvarchar(max) NULL,
        [CvrNumber] nvarchar(max) NULL,
        [VatNumber] nvarchar(max) NULL,
        [ContactPerson] nvarchar(max) NULL,
        [ContactEmail] nvarchar(max) NULL,
        [ContactPhone] nvarchar(max) NULL,
        [CompanyAddress] nvarchar(max) NULL,
        [PaymentReference] nvarchar(max) NULL,
        [PayoutAccountInfo] nvarchar(max) NULL,
        [PaymentProvider] nvarchar(max) NULL,
        CONSTRAINT [PK_Participants] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE TABLE [FriendRelations] (
        [Id] int NOT NULL IDENTITY,
        [InitiatorId] int NOT NULL,
        [ReceiverId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FriendRelations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FriendRelations_Participants_InitiatorId] FOREIGN KEY ([InitiatorId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FriendRelations_Participants_ReceiverId] FOREIGN KEY ([ReceiverId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE TABLE [Messages] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ParticipantId] int NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Messages_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Messages_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE TABLE [OrderParticipants] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ParticipantId] int NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_OrderParticipants] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderParticipants_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderParticipants_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE TABLE [Payments] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ParticipantId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payments_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Payments_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FriendRelations_InitiatorId] ON [FriendRelations] ([InitiatorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FriendRelations_ReceiverId] ON [FriendRelations] ([ReceiverId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Messages_OrderId] ON [Messages] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Messages_ParticipantId] ON [Messages] ([ParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderParticipants_OrderId] ON [OrderParticipants] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_OrderParticipants_ParticipantId] ON [OrderParticipants] ([ParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_OrderId] ON [Payments] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payments_ParticipantId] ON [Payments] ([ParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260505153849_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260505153849_InitialCreate', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506184515_AddOrderCreatedBy'
)
BEGIN
    ALTER TABLE [Orders] ADD [CreatedByParticipantId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506184515_AddOrderCreatedBy'
)
BEGIN
    CREATE INDEX [IX_Orders_CreatedByParticipantId] ON [Orders] ([CreatedByParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506184515_AddOrderCreatedBy'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Participants_CreatedByParticipantId] FOREIGN KEY ([CreatedByParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506184515_AddOrderCreatedBy'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260506184515_AddOrderCreatedBy', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506185436_AddMerchantOrderDraft'
)
BEGIN
    CREATE TABLE [MerchantOrderDrafts] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [MerchantParticipantId] int NOT NULL,
        [MerchantDraftReference] nvarchar(max) NOT NULL,
        [SubtotalAmount] decimal(18,2) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(max) NOT NULL,
        [PaymentMode] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [ExpiresAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        CONSTRAINT [PK_MerchantOrderDrafts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MerchantOrderDrafts_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MerchantOrderDrafts_Participants_MerchantParticipantId] FOREIGN KEY ([MerchantParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506185436_AddMerchantOrderDraft'
)
BEGIN
    CREATE TABLE [MerchantOrderLines] (
        [Id] int NOT NULL IDENTITY,
        [MerchantOrderDraftId] int NOT NULL,
        [LineId] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_MerchantOrderLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MerchantOrderLines_MerchantOrderDrafts_MerchantOrderDraftId] FOREIGN KEY ([MerchantOrderDraftId]) REFERENCES [MerchantOrderDrafts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506185436_AddMerchantOrderDraft'
)
BEGIN
    CREATE INDEX [IX_MerchantOrderDrafts_MerchantParticipantId] ON [MerchantOrderDrafts] ([MerchantParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506185436_AddMerchantOrderDraft'
)
BEGIN
    CREATE INDEX [IX_MerchantOrderDrafts_OrderId] ON [MerchantOrderDrafts] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506185436_AddMerchantOrderDraft'
)
BEGIN
    CREATE INDEX [IX_MerchantOrderLines_MerchantOrderDraftId] ON [MerchantOrderLines] ([MerchantOrderDraftId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260506185436_AddMerchantOrderDraft'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260506185436_AddMerchantOrderDraft', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508204647_AddMerchantToOrder'
)
BEGIN
    ALTER TABLE [Participants] ADD [GroupOrderUrl] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508204647_AddMerchantToOrder'
)
BEGIN
    ALTER TABLE [Orders] ADD [JoinToken] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508204647_AddMerchantToOrder'
)
BEGIN
    ALTER TABLE [Orders] ADD [MerchantParticipantId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508204647_AddMerchantToOrder'
)
BEGIN
    CREATE INDEX [IX_Orders_MerchantParticipantId] ON [Orders] ([MerchantParticipantId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508204647_AddMerchantToOrder'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Participants_MerchantParticipantId] FOREIGN KEY ([MerchantParticipantId]) REFERENCES [Participants] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260508204647_AddMerchantToOrder'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260508204647_AddMerchantToOrder', N'9.0.15');
END;

COMMIT;
GO

