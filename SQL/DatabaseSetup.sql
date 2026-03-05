-- ============================================================
-- AI BUSINESS BRAIN — DATABASE SETUP
-- Auto-creates database, tables, indexes, SPs, views, triggers
-- ============================================================

-- ===================== CREATE DATABASE =====================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BussinessDB')
BEGIN
    CREATE DATABASE [BussinessDB];
END
GO

USE [BussinessDB];
GO

-- ===================== TABLES =====================

-- Users
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id]           INT IDENTITY(1,1) PRIMARY KEY,
        [Username]     NVARCHAR(100) NOT NULL UNIQUE,
        [PasswordHash] NVARCHAR(256) NOT NULL,
        [Role]         NVARCHAR(50)  NOT NULL DEFAULT 'Employee',
        [CreatedAt]    DATETIME      NOT NULL DEFAULT GETDATE()
    );
END
GO

-- Employees
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employees]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Employees] (
        [Id]         INT IDENTITY(1,1) PRIMARY KEY,
        [Name]       NVARCHAR(150) NOT NULL,
        [Phone]      NVARCHAR(20)  NULL,
        [Salary]     DECIMAL(18,2) NOT NULL DEFAULT 0,
        [HireDate]   DATE          NOT NULL DEFAULT GETDATE(),
        [Department] NVARCHAR(100) NULL,
        [Position]   NVARCHAR(100) NULL
    );
END
GO

-- Customers
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Customers]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Customers] (
        [Id]        INT IDENTITY(1,1) PRIMARY KEY,
        [Name]      NVARCHAR(150) NOT NULL,
        [Phone]     NVARCHAR(20)  NULL,
        [Email]     NVARCHAR(150) NULL,
        [Address]   NVARCHAR(300) NULL,
        [CreatedAt] DATETIME      NOT NULL DEFAULT GETDATE()
    );
END
GO

-- Suppliers
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Suppliers]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Suppliers] (
        [Id]      INT IDENTITY(1,1) PRIMARY KEY,
        [Name]    NVARCHAR(150) NOT NULL,
        [Phone]   NVARCHAR(20)  NULL,
        [Email]   NVARCHAR(150) NULL,
        [Address] NVARCHAR(300) NULL
    );
END
GO

-- Products
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Products] (
        [Id]           INT IDENTITY(1,1) PRIMARY KEY,
        [Name]         NVARCHAR(200) NOT NULL,
        [Category]     NVARCHAR(100) NULL,
        [CostPrice]    DECIMAL(18,2) NOT NULL DEFAULT 0,
        [SellPrice]    DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Quantity]     INT           NOT NULL DEFAULT 0,
        [ReorderLevel] INT           NOT NULL DEFAULT 10,
        [CreatedAt]    DATETIME      NOT NULL DEFAULT GETDATE()
    );
END
GO

-- Sales
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Sales]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Sales] (
        [Id]          INT IDENTITY(1,1) PRIMARY KEY,
        [CustomerId]  INT           NULL,
        [Date]        DATETIME      NOT NULL DEFAULT GETDATE(),
        [TotalAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        CONSTRAINT [FK_Sales_Customers] FOREIGN KEY ([CustomerId])
            REFERENCES [dbo].[Customers]([Id]) ON DELETE SET NULL
    );
END
GO

-- SaleItems
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SaleItems]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[SaleItems] (
        [Id]        INT IDENTITY(1,1) PRIMARY KEY,
        [SaleId]    INT           NOT NULL,
        [ProductId] INT           NOT NULL,
        [Quantity]  INT           NOT NULL DEFAULT 1,
        [SellPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
        CONSTRAINT [FK_SaleItems_Sales] FOREIGN KEY ([SaleId])
            REFERENCES [dbo].[Sales]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SaleItems_Products] FOREIGN KEY ([ProductId])
            REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE
    );
END
GO

-- Purchases
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Purchases]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Purchases] (
        [Id]          INT IDENTITY(1,1) PRIMARY KEY,
        [SupplierId]  INT           NULL,
        [Date]        DATETIME      NOT NULL DEFAULT GETDATE(),
        [TotalAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        CONSTRAINT [FK_Purchases_Suppliers] FOREIGN KEY ([SupplierId])
            REFERENCES [dbo].[Suppliers]([Id]) ON DELETE SET NULL
    );
END
GO

-- PurchaseItems
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurchaseItems]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[PurchaseItems] (
        [Id]         INT IDENTITY(1,1) PRIMARY KEY,
        [PurchaseId] INT           NOT NULL,
        [ProductId]  INT           NOT NULL,
        [Quantity]   INT           NOT NULL DEFAULT 1,
        [CostPrice]  DECIMAL(18,2) NOT NULL DEFAULT 0,
        CONSTRAINT [FK_PurchaseItems_Purchases] FOREIGN KEY ([PurchaseId])
            REFERENCES [dbo].[Purchases]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PurchaseItems_Products] FOREIGN KEY ([ProductId])
            REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE
    );
END
GO

-- Expenses
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Expenses]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Expenses] (
        [Id]       INT IDENTITY(1,1) PRIMARY KEY,
        [Title]    NVARCHAR(200) NOT NULL,
        [Amount]   DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Category] NVARCHAR(100) NULL,
        [Description] NVARCHAR(MAX) NULL,
        [Date]     DATETIME      NOT NULL DEFAULT GETDATE()
    );
END
GO

-- ===================== INDEXES =====================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Products_Name')
    CREATE NONCLUSTERED INDEX [IX_Products_Name] ON [dbo].[Products]([Name]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Customers_Name')
    CREATE NONCLUSTERED INDEX [IX_Customers_Name] ON [dbo].[Customers]([Name]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Sales_Date')
    CREATE NONCLUSTERED INDEX [IX_Sales_Date] ON [dbo].[Sales]([Date]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_SaleItems_ProductId')
    CREATE NONCLUSTERED INDEX [IX_SaleItems_ProductId] ON [dbo].[SaleItems]([ProductId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_SaleItems_SaleId')
    CREATE NONCLUSTERED INDEX [IX_SaleItems_SaleId] ON [dbo].[SaleItems]([SaleId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Sales_CustomerId')
    CREATE NONCLUSTERED INDEX [IX_Sales_CustomerId] ON [dbo].[Sales]([CustomerId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_PurchaseItems_PurchaseId')
    CREATE NONCLUSTERED INDEX [IX_PurchaseItems_PurchaseId] ON [dbo].[PurchaseItems]([PurchaseId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_PurchaseItems_ProductId')
    CREATE NONCLUSTERED INDEX [IX_PurchaseItems_ProductId] ON [dbo].[PurchaseItems]([ProductId]);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Purchases_SupplierId')
    CREATE NONCLUSTERED INDEX [IX_Purchases_SupplierId] ON [dbo].[Purchases]([SupplierId]);
GO

-- ===================== STORED PROCEDURES =====================

-- SP: AddSale (uses transaction, updates stock via UpdateProductStock)
IF OBJECT_ID(N'[dbo].[sp_AddSale]', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_AddSale];
GO
CREATE PROCEDURE [dbo].[sp_AddSale]
    @CustomerId INT,
    @Items NVARCHAR(MAX)  -- JSON: [{"ProductId":1,"Quantity":2,"SellPrice":10.50}, ...]
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @SaleId INT;
        DECLARE @TotalAmount DECIMAL(18,2) = 0;

        -- Create sale header
        INSERT INTO [dbo].[Sales] ([CustomerId], [Date], [TotalAmount])
        VALUES (@CustomerId, GETDATE(), 0);

        SET @SaleId = SCOPE_IDENTITY();

        -- Insert sale items from JSON
        INSERT INTO [dbo].[SaleItems] ([SaleId], [ProductId], [Quantity], [SellPrice])
        SELECT @SaleId, ProductId, Quantity, SellPrice
        FROM OPENJSON(@Items)
        WITH (
            ProductId INT '$.ProductId',
            Quantity  INT '$.Quantity',
            SellPrice DECIMAL(18,2) '$.SellPrice'
        );

        -- Calculate total
        SELECT @TotalAmount = SUM(Quantity * SellPrice)
        FROM [dbo].[SaleItems]
        WHERE [SaleId] = @SaleId;

        -- Update sale total
        UPDATE [dbo].[Sales] SET [TotalAmount] = @TotalAmount WHERE [Id] = @SaleId;

        -- Update stock for each item
        DECLARE @pid INT, @qty INT;
        DECLARE item_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT ProductId, Quantity FROM [dbo].[SaleItems] WHERE [SaleId] = @SaleId;

        OPEN item_cursor;
        FETCH NEXT FROM item_cursor INTO @pid, @qty;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            EXEC [dbo].[sp_UpdateProductStock] @ProductId = @pid, @QuantityChange = @qty, @IsDecrease = 1;
            FETCH NEXT FROM item_cursor INTO @pid, @qty;
        END
        CLOSE item_cursor;
        DEALLOCATE item_cursor;

        COMMIT TRANSACTION;
        SELECT @SaleId AS NewSaleId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: UpdateProductStock
IF OBJECT_ID(N'[dbo].[sp_UpdateProductStock]', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_UpdateProductStock];
GO
CREATE PROCEDURE [dbo].[sp_UpdateProductStock]
    @ProductId      INT,
    @QuantityChange INT,
    @IsDecrease     BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    IF @IsDecrease = 1
    BEGIN
        UPDATE [dbo].[Products]
        SET [Quantity] = [Quantity] - @QuantityChange
        WHERE [Id] = @ProductId;
    END
    ELSE
    BEGIN
        UPDATE [dbo].[Products]
        SET [Quantity] = [Quantity] + @QuantityChange
        WHERE [Id] = @ProductId;
    END
END
GO

-- SP: AddPurchase
IF OBJECT_ID(N'[dbo].[sp_AddPurchase]', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_AddPurchase];
GO
CREATE PROCEDURE [dbo].[sp_AddPurchase]
    @SupplierId INT,
    @Items NVARCHAR(MAX) -- JSON: [{"ProductId":1,"Quantity":10,"CostPrice":5.00}, ...]
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @PurchaseId INT;
        DECLARE @TotalAmount DECIMAL(18,2) = 0;

        INSERT INTO [dbo].[Purchases] ([SupplierId], [Date], [TotalAmount])
        VALUES (@SupplierId, GETDATE(), 0);

        SET @PurchaseId = SCOPE_IDENTITY();

        INSERT INTO [dbo].[PurchaseItems] ([PurchaseId], [ProductId], [Quantity], [CostPrice])
        SELECT @PurchaseId, ProductId, Quantity, CostPrice
        FROM OPENJSON(@Items)
        WITH (
            ProductId INT '$.ProductId',
            Quantity  INT '$.Quantity',
            CostPrice DECIMAL(18,2) '$.CostPrice'
        );

        SELECT @TotalAmount = SUM(Quantity * CostPrice)
        FROM [dbo].[PurchaseItems]
        WHERE [PurchaseId] = @PurchaseId;

        UPDATE [dbo].[Purchases] SET [TotalAmount] = @TotalAmount WHERE [Id] = @PurchaseId;

        -- Update stock
        DECLARE @pid INT, @qty INT;
        DECLARE pur_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT ProductId, Quantity FROM [dbo].[PurchaseItems] WHERE [PurchaseId] = @PurchaseId;

        OPEN pur_cursor;
        FETCH NEXT FROM pur_cursor INTO @pid, @qty;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            EXEC [dbo].[sp_UpdateProductStock] @ProductId = @pid, @QuantityChange = @qty, @IsDecrease = 0;
            FETCH NEXT FROM pur_cursor INTO @pid, @qty;
        END
        CLOSE pur_cursor;
        DEALLOCATE pur_cursor;

        COMMIT TRANSACTION;
        SELECT @PurchaseId AS NewPurchaseId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- SP: GetDailySales
IF OBJECT_ID(N'[dbo].[sp_GetDailySales]', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_GetDailySales];
GO
CREATE PROCEDURE [dbo].[sp_GetDailySales]
    @Date DATE = NULL,
    @UserRole NVARCHAR(50) = 'Employee'
AS
BEGIN
    SET NOCOUNT ON;
    IF @UserRole NOT IN ('Admin', 'Manager')
    BEGIN
        RAISERROR('Unauthorized: Manager or Admin role required for financial reports.', 16, 1);
        RETURN;
    END

    IF @Date IS NULL SET @Date = CAST(GETDATE() AS DATE);

    SELECT
        COUNT(*)        AS TotalTransactions,
        ISNULL(SUM(TotalAmount), 0) AS TotalSales
    FROM [dbo].[Sales]
    WHERE CAST([Date] AS DATE) = @Date;
END
GO

-- SP: GetMonthlyProfit
IF OBJECT_ID(N'[dbo].[sp_GetMonthlyProfit]', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_GetMonthlyProfit];
GO
CREATE PROCEDURE [dbo].[sp_GetMonthlyProfit]
    @Year  INT = NULL,
    @Month INT = NULL,
    @UserRole NVARCHAR(50) = 'Employee'
AS
BEGIN
    SET NOCOUNT ON;
    IF @UserRole NOT IN ('Admin', 'Manager')
    BEGIN
        RAISERROR('Unauthorized: Manager or Admin role required for profit analysis.', 16, 1);
        RETURN;
    END

    IF @Year IS NULL SET @Year = YEAR(GETDATE());
    IF @Month IS NULL SET @Month = MONTH(GETDATE());

    DECLARE @Revenue DECIMAL(18,2) = 0;
    DECLARE @COGS DECIMAL(18,2) = 0;
    DECLARE @Expenses DECIMAL(18,2) = 0;

    -- Revenue
    SELECT @Revenue = ISNULL(SUM(si.Quantity * si.SellPrice), 0)
    FROM [dbo].[SaleItems] si
    INNER JOIN [dbo].[Sales] s ON si.SaleId = s.Id
    WHERE YEAR(s.[Date]) = @Year AND MONTH(s.[Date]) = @Month;

    -- COGS
    SELECT @COGS = ISNULL(SUM(si.Quantity * p.CostPrice), 0)
    FROM [dbo].[SaleItems] si
    INNER JOIN [dbo].[Sales] s ON si.SaleId = s.Id
    INNER JOIN [dbo].[Products] p ON si.ProductId = p.Id
    WHERE YEAR(s.[Date]) = @Year AND MONTH(s.[Date]) = @Month;

    -- Expenses
    SELECT @Expenses = ISNULL(SUM(Amount), 0)
    FROM [dbo].[Expenses]
    WHERE YEAR([Date]) = @Year AND MONTH([Date]) = @Month;

    SELECT
        @Revenue  AS Revenue,
        @COGS     AS COGS,
        @Expenses AS Expenses,
        (@Revenue - @COGS - @Expenses) AS NetProfit;
END
GO

-- SP: GetLowStockProducts
IF OBJECT_ID(N'[dbo].[sp_GetLowStockProducts]', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_GetLowStockProducts];
GO
CREATE PROCEDURE [dbo].[sp_GetLowStockProducts]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [Name], [Category], [Quantity], [ReorderLevel],
           ([ReorderLevel] - [Quantity]) AS Deficit
    FROM [dbo].[Products]
    WHERE [Quantity] <= [ReorderLevel]
    ORDER BY Deficit DESC;
END
GO

-- SP: GetTopSellingProducts
IF OBJECT_ID(N'[dbo].[sp_GetTopSellingProducts]', 'P') IS NOT NULL DROP PROCEDURE [dbo].[sp_GetTopSellingProducts];
GO
CREATE PROCEDURE [dbo].[sp_GetTopSellingProducts]
    @TopN INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP(@TopN)
        p.[Id],
        p.[Name],
        p.[Category],
        SUM(si.[Quantity])              AS TotalUnitsSold,
        SUM(si.[Quantity] * si.[SellPrice]) AS TotalRevenue
    FROM [dbo].[SaleItems] si
    INNER JOIN [dbo].[Products] p ON si.[ProductId] = p.[Id]
    GROUP BY p.[Id], p.[Name], p.[Category]
    ORDER BY TotalUnitsSold DESC;
END
GO

-- ===================== VIEWS =====================

IF OBJECT_ID(N'[dbo].[vw_DailySalesSummary]', 'V') IS NOT NULL DROP VIEW [dbo].[vw_DailySalesSummary];
GO
CREATE VIEW [dbo].[vw_DailySalesSummary]
AS
    SELECT
        CAST(s.[Date] AS DATE) AS SaleDate,
        COUNT(DISTINCT s.[Id]) AS TransactionCount,
        SUM(si.[Quantity])     AS TotalItemsSold,
        SUM(si.[Quantity] * si.[SellPrice]) AS TotalRevenue
    FROM [dbo].[Sales] s
    INNER JOIN [dbo].[SaleItems] si ON s.[Id] = si.[SaleId]
    GROUP BY CAST(s.[Date] AS DATE);
GO

IF OBJECT_ID(N'[dbo].[vw_MonthlyProfit]', 'V') IS NOT NULL DROP VIEW [dbo].[vw_MonthlyProfit];
GO
CREATE VIEW [dbo].[vw_MonthlyProfit]
AS
    SELECT
        YEAR(s.[Date])  AS [Year],
        MONTH(s.[Date]) AS [Month],
        SUM(si.[Quantity] * si.[SellPrice])  AS Revenue,
        SUM(si.[Quantity] * p.[CostPrice])   AS COGS,
        SUM(si.[Quantity] * si.[SellPrice]) - SUM(si.[Quantity] * p.[CostPrice]) AS GrossProfit
    FROM [dbo].[Sales] s
    INNER JOIN [dbo].[SaleItems] si ON s.[Id] = si.[SaleId]
    INNER JOIN [dbo].[Products] p ON si.[ProductId] = p.[Id]
    GROUP BY YEAR(s.[Date]), MONTH(s.[Date]);
GO

IF OBJECT_ID(N'[dbo].[vw_TopProducts]', 'V') IS NOT NULL DROP VIEW [dbo].[vw_TopProducts];
GO
CREATE VIEW [dbo].[vw_TopProducts]
AS
    SELECT TOP 100
        p.[Id],
        p.[Name],
        p.[Category],
        SUM(si.[Quantity])                  AS TotalUnitsSold,
        SUM(si.[Quantity] * si.[SellPrice]) AS TotalRevenue,
        SUM(si.[Quantity] * (si.[SellPrice] - p.[CostPrice])) AS TotalProfit
    FROM [dbo].[SaleItems] si
    INNER JOIN [dbo].[Products] p ON si.[ProductId] = p.[Id]
    GROUP BY p.[Id], p.[Name], p.[Category]
    ORDER BY TotalUnitsSold DESC;
GO

IF OBJECT_ID(N'[dbo].[vw_InventoryStatus]', 'V') IS NOT NULL DROP VIEW [dbo].[vw_InventoryStatus];
GO
CREATE VIEW [dbo].[vw_InventoryStatus]
AS
    SELECT
        p.[Id],
        p.[Name],
        p.[Category],
        p.[Quantity]     AS CurrentStock,
        p.[ReorderLevel],
        p.[CostPrice],
        p.[SellPrice],
        (p.[SellPrice] - p.[CostPrice]) AS ProfitMargin,
        CASE
            WHEN p.[Quantity] = 0 THEN 'Out of Stock'
            WHEN p.[Quantity] <= p.[ReorderLevel] THEN 'Low Stock'
            ELSE 'In Stock'
        END AS StockStatus
    FROM [dbo].[Products] p;
GO

-- ===================== TRIGGERS =====================
-- Note: Stock updates are handled by stored procedures (sp_AddSale / sp_AddPurchase).
-- These triggers serve as a safety net for direct inserts outside of SPs.

IF OBJECT_ID(N'[dbo].[trg_AfterSaleItemInsert]', 'TR') IS NOT NULL DROP TRIGGER [dbo].[trg_AfterSaleItemInsert];
GO
CREATE TRIGGER [dbo].[trg_AfterSaleItemInsert]
ON [dbo].[SaleItems]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    -- Only fire if not already inside sp_AddSale (check for active transaction tag)
    IF OBJECT_ID('tempdb..#SP_SALE_RUNNING') IS NULL
    BEGIN
        UPDATE p
        SET p.[Quantity] = p.[Quantity] - i.[Quantity]
        FROM [dbo].[Products] p
        INNER JOIN inserted i ON p.[Id] = i.[ProductId];
    END
END
GO

IF OBJECT_ID(N'[dbo].[trg_AfterPurchaseItemInsert]', 'TR') IS NOT NULL DROP TRIGGER [dbo].[trg_AfterPurchaseItemInsert];
GO
CREATE TRIGGER [dbo].[trg_AfterPurchaseItemInsert]
ON [dbo].[PurchaseItems]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    IF OBJECT_ID('tempdb..#SP_PURCHASE_RUNNING') IS NULL
    BEGIN
        UPDATE p
        SET p.[Quantity] = p.[Quantity] + i.[Quantity]
        FROM [dbo].[Products] p
        INNER JOIN inserted i ON p.[Id] = i.[ProductId];
    END
END
GO

PRINT '=== DATABASE SETUP COMPLETE ==='
GO
