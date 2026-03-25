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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611070456_adminMigration')
BEGIN
    CREATE TABLE [tbl_admin] (
        [admin_id] int NOT NULL IDENTITY,
        [admin_name] nvarchar(max) NOT NULL,
        [admin_email] nvarchar(max) NOT NULL,
        [admin_password] nvarchar(max) NOT NULL,
        [admin_image] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_tbl_admin] PRIMARY KEY ([admin_id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611070456_adminMigration')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250611070456_adminMigration', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611071401_customerMigration')
BEGIN
    CREATE TABLE [tbl_customer] (
        [customer_id] int NOT NULL IDENTITY,
        [customer_name] nvarchar(max) NOT NULL,
        [customer_phone] nvarchar(max) NOT NULL,
        [customer_email] nvarchar(max) NOT NULL,
        [customer_password] nvarchar(max) NOT NULL,
        [customer_gender] nvarchar(max) NOT NULL,
        [customer_country] nvarchar(max) NOT NULL,
        [customer_city] nvarchar(max) NOT NULL,
        [customer_address] nvarchar(max) NOT NULL,
        [customer_image] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_tbl_customer] PRIMARY KEY ([customer_id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611071401_customerMigration')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250611071401_customerMigration', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611072201_category-and-productMigration')
BEGIN
    CREATE TABLE [tbl_category] (
        [category_id] int NOT NULL IDENTITY,
        [category_name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_tbl_category] PRIMARY KEY ([category_id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611072201_category-and-productMigration')
BEGIN
    CREATE TABLE [tbl_product] (
        [product_id] int NOT NULL IDENTITY,
        [product_name] nvarchar(max) NOT NULL,
        [product_price] nvarchar(max) NOT NULL,
        [product_description] nvarchar(max) NOT NULL,
        [product_image] nvarchar(max) NOT NULL,
        [cat_id] int NOT NULL,
        CONSTRAINT [PK_tbl_product] PRIMARY KEY ([product_id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611072201_category-and-productMigration')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250611072201_category-and-productMigration', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611072726_cartMigration')
BEGIN
    CREATE TABLE [tbl_cart] (
        [cart_id] int NOT NULL IDENTITY,
        [prod_id] int NOT NULL,
        [cust_id] int NOT NULL,
        [product_quantity] int NOT NULL,
        [cart_status] int NOT NULL,
        CONSTRAINT [PK_tbl_cart] PRIMARY KEY ([cart_id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611072726_cartMigration')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250611072726_cartMigration', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611073456_feedbackMigration')
BEGIN
    CREATE TABLE [tbl_feedback] (
        [feedback_id] int NOT NULL IDENTITY,
        [user_name] nvarchar(max) NOT NULL,
        [user_message] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_tbl_feedback] PRIMARY KEY ([feedback_id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611073456_feedbackMigration')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250611073456_feedbackMigration', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611073809_faqMigration')
BEGIN
    CREATE TABLE [tbl_faqs] (
        [faq_id] int NOT NULL IDENTITY,
        [faq_question] nvarchar(max) NOT NULL,
        [faq_answer] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_tbl_faqs] PRIMARY KEY ([faq_id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250611073809_faqMigration')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250611073809_faqMigration', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250612103940_updated-product-and-category-migration')
BEGIN
    CREATE INDEX [IX_tbl_product_cat_id] ON [tbl_product] ([cat_id]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250612103940_updated-product-and-category-migration')
BEGIN
    ALTER TABLE [tbl_product] ADD CONSTRAINT [FK_tbl_product_tbl_category_cat_id] FOREIGN KEY ([cat_id]) REFERENCES [tbl_category] ([category_id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250612103940_updated-product-and-category-migration')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250612103940_updated-product-and-category-migration', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250711073558_AddCreatedAtToProduct')
BEGIN
    ALTER TABLE [tbl_product] ADD [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE());
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250711073558_AddCreatedAtToProduct')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250711073558_AddCreatedAtToProduct', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250711143707_AddDiscountAndRatingToProduct')
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[tbl_product]') AND [c].[name] = N'product_price');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [tbl_product] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [tbl_product] ALTER COLUMN [product_price] int NOT NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250711143707_AddDiscountAndRatingToProduct')
BEGIN
    ALTER TABLE [tbl_product] ADD [product_discount_price] int NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250711143707_AddDiscountAndRatingToProduct')
BEGIN
    ALTER TABLE [tbl_product] ADD [product_rating] float NOT NULL DEFAULT 0.0E0;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250711143707_AddDiscountAndRatingToProduct')
BEGIN
    ALTER TABLE [tbl_product] ADD [product_review_count] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250711143707_AddDiscountAndRatingToProduct')
BEGIN
    ALTER TABLE [tbl_product] ADD [product_sold] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250711143707_AddDiscountAndRatingToProduct')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250711143707_AddDiscountAndRatingToProduct', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250712021529_MakeCatIdNullable')
BEGIN
    ALTER TABLE [tbl_product] DROP CONSTRAINT [FK_tbl_product_tbl_category_cat_id];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250712021529_MakeCatIdNullable')
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[tbl_product]') AND [c].[name] = N'cat_id');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [tbl_product] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [tbl_product] ALTER COLUMN [cat_id] int NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250712021529_MakeCatIdNullable')
BEGIN
    ALTER TABLE [tbl_product] ADD CONSTRAINT [FK_tbl_product_tbl_category_cat_id] FOREIGN KEY ([cat_id]) REFERENCES [tbl_category] ([category_id]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250712021529_MakeCatIdNullable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250712021529_MakeCatIdNullable', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250712122450_AddProductImageTable')
BEGIN
    CREATE TABLE [ProductImage] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [ImagePath] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_ProductImage] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductImage_tbl_product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [tbl_product] ([product_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250712122450_AddProductImageTable')
BEGIN
    CREATE INDEX [IX_ProductImage_ProductId] ON [ProductImage] ([ProductId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250712122450_AddProductImageTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250712122450_AddProductImageTable', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250713140122_AddBlogTable')
BEGIN
    CREATE TABLE [tbl_blog] (
        [blog_id] int NOT NULL IDENTITY,
        [blog_title] nvarchar(max) NULL,
        [blog_image] nvarchar(max) NULL,
        [blog_description] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_tbl_blog] PRIMARY KEY ([blog_id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250713140122_AddBlogTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250713140122_AddBlogTable', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250716113135_AddOrderEntities')
BEGIN
    CREATE TABLE [tbl_order] (
        [OrderID] int NOT NULL IDENTITY,
        [CreatedAt] datetime2 NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [PaymentStatus] nvarchar(max) NOT NULL,
        [OrderStatus] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_tbl_order] PRIMARY KEY ([OrderID])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250716113135_AddOrderEntities')
BEGIN
    CREATE TABLE [tbl_orderdetail] (
        [OrderDetailID] int NOT NULL IDENTITY,
        [OrderID] int NOT NULL,
        [ProductID] int NOT NULL,
        [Quantity] int NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_tbl_orderdetail] PRIMARY KEY ([OrderDetailID]),
        CONSTRAINT [FK_tbl_orderdetail_tbl_order_OrderID] FOREIGN KEY ([OrderID]) REFERENCES [tbl_order] ([OrderID]) ON DELETE CASCADE,
        CONSTRAINT [FK_tbl_orderdetail_tbl_product_ProductID] FOREIGN KEY ([ProductID]) REFERENCES [tbl_product] ([product_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250716113135_AddOrderEntities')
BEGIN
    CREATE INDEX [IX_tbl_orderdetail_OrderID] ON [tbl_orderdetail] ([OrderID]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250716113135_AddOrderEntities')
BEGIN
    CREATE INDEX [IX_tbl_orderdetail_ProductID] ON [tbl_orderdetail] ([ProductID]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250716113135_AddOrderEntities')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250716113135_AddOrderEntities', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250716132109_AddCustomerFKToOrder')
BEGIN
    ALTER TABLE [tbl_order] ADD [CustomerId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250716132109_AddCustomerFKToOrder')
BEGIN
    CREATE INDEX [IX_tbl_order_CustomerId] ON [tbl_order] ([CustomerId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250716132109_AddCustomerFKToOrder')
BEGIN
    ALTER TABLE [tbl_order] ADD CONSTRAINT [FK_tbl_order_tbl_customer_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [tbl_customer] ([customer_id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250716132109_AddCustomerFKToOrder')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250716132109_AddCustomerFKToOrder', N'6.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250718114414_AddWishList')
BEGIN
    CREATE TABLE [WishLists] (
        [WishListId] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WishLists] PRIMARY KEY ([WishListId]),
        CONSTRAINT [FK_WishLists_tbl_customer_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [tbl_customer] ([customer_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250718114414_AddWishList')
BEGIN
    CREATE TABLE [WishListItems] (
        [WishListItemId] int NOT NULL IDENTITY,
        [WishListId] int NOT NULL,
        [ProductId] int NOT NULL,
        [AddedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WishListItems] PRIMARY KEY ([WishListItemId]),
        CONSTRAINT [FK_WishListItems_tbl_product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [tbl_product] ([product_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WishListItems_WishLists_WishListId] FOREIGN KEY ([WishListId]) REFERENCES [WishLists] ([WishListId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250718114414_AddWishList')
BEGIN
    CREATE INDEX [IX_WishListItems_ProductId] ON [WishListItems] ([ProductId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250718114414_AddWishList')
BEGIN
    CREATE INDEX [IX_WishListItems_WishListId] ON [WishListItems] ([WishListId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250718114414_AddWishList')
BEGIN
    CREATE INDEX [IX_WishLists_CustomerId] ON [WishLists] ([CustomerId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20250718114414_AddWishList')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250718114414_AddWishList', N'6.0.0');
END;
GO

COMMIT;
GO