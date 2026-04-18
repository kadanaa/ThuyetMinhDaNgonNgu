-- Database: ThuyetMinhDaNgonNgu
-- Purpose: Database for Tourist POI Application with Admin Management and POI Owner Management
-- Created: 2024

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ThuyetMinhDaNgonNgu')
BEGIN
    CREATE DATABASE ThuyetMinhDaNgonNgu;
END
GO

USE ThuyetMinhDaNgonNgu;
GO

-- =============================================
-- 1. USERS TABLE - Bảng người dùng (Admin/POI Owner)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [UserId] INT PRIMARY KEY IDENTITY(1,1),
        [Username] NVARCHAR(100) NOT NULL UNIQUE,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [Email] NVARCHAR(100) NOT NULL UNIQUE,
        [FullName] NVARCHAR(200),
        [Role] NVARCHAR(50) NOT NULL CHECK ([Role] IN ('Admin', 'POIOwner')),
        [PhoneNumber] NVARCHAR(20),
        [IsActive] BIT DEFAULT 1,
        [CreatedDate] DATETIME DEFAULT GETUTCDATE(),
        [LastModifiedDate] DATETIME DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_Users_Username ON [dbo].[Users]([Username]);
    CREATE INDEX IX_Users_Email ON [dbo].[Users]([Email]);
    CREATE INDEX IX_Users_Role ON [dbo].[Users]([Role]);
END
GO

-- =============================================
-- 2. POINTS OF INTEREST (POI) TABLE - Bảng địa điểm
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PointsOfInterest]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PointsOfInterest]
    (
        [POIId] INT PRIMARY KEY IDENTITY(1,1),
        [POIName] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(MAX), -- Mô tả thuyết minh bằng tiếng Việt
        [Latitude] DECIMAL(10, 8) NOT NULL, -- Tọa độ X (Latitude)
        [Longitude] DECIMAL(11, 8) NOT NULL, -- Tọa độ Y (Longitude)
        [Radius] FLOAT NOT NULL, -- Bán kính r (tính bằng KM, ví dụ: 0.5 = 500m)
        [Category] NVARCHAR(50), -- Loại địa điểm: 'Restaurant', 'Cafe', 'Shop', 'Tourism', v.v.
        [Address] NVARCHAR(300),
        [PhoneNumber] NVARCHAR(20),
        [Website] NVARCHAR(200),
        [OwnerId] INT, -- Người sở hữu POI (POI Owner)
        [IsApproved] BIT DEFAULT 0, -- Trạng thái duyệt
        [Status] NVARCHAR(50) DEFAULT 'Active', -- 'Active', 'Inactive', 'Pending'
        [CreatedDate] DATETIME DEFAULT GETUTCDATE(),
        [LastModifiedDate] DATETIME DEFAULT GETUTCDATE(),
        [ApprovedDate] DATETIME,
        [ApprovedBy] INT, -- Admin đã duyệt

        FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[Users]([UserId]),
        FOREIGN KEY ([ApprovedBy]) REFERENCES [dbo].[Users]([UserId])
    );

    CREATE INDEX IX_POI_Status ON [dbo].[PointsOfInterest]([Status]);
    CREATE INDEX IX_POI_IsApproved ON [dbo].[PointsOfInterest]([IsApproved]);
    CREATE INDEX IX_POI_OwnerId ON [dbo].[PointsOfInterest]([OwnerId]);
    CREATE INDEX IX_POI_Coordinates ON [dbo].[PointsOfInterest]([Latitude], [Longitude]);
END
GO

-- =============================================
-- 3. POI TRANSLATIONS TABLE - Bảng dịch thuyết minh POI
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POITranslations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[POITranslations]
    (
        [TranslationId] INT PRIMARY KEY IDENTITY(1,1),
        [POIId] INT NOT NULL,
        [LanguageCode] NVARCHAR(10) NOT NULL, -- 'en', 'es', 'fr', 'de', 'ja', 'zh', v.v.
        [LanguageName] NVARCHAR(50), -- Tên ngôn ngữ: 'English', 'Spanish', 'French', v.v.
        [TranslatedDescription] NVARCHAR(MAX), -- Mô tả dịch
        [TranslatedName] NVARCHAR(200), -- Tên địa điểm dịch
        [TranslatedBy] INT, -- Người dịch hoặc AI service
        [CreatedDate] DATETIME DEFAULT GETUTCDATE(),
        [LastModifiedDate] DATETIME DEFAULT GETUTCDATE(),

        FOREIGN KEY ([POIId]) REFERENCES [dbo].[PointsOfInterest]([POIId]) ON DELETE CASCADE,
        UNIQUE([POIId], [LanguageCode])
    );

    CREATE INDEX IX_POITranslations_POIId ON [dbo].[POITranslations]([POIId]);
    CREATE INDEX IX_POITranslations_LanguageCode ON [dbo].[POITranslations]([LanguageCode]);
END
GO

-- =============================================
-- 4. POI APPROVAL REQUESTS TABLE - Bảng yêu cầu duyệt POI
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POIApprovalRequests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[POIApprovalRequests]
    (
        [RequestId] INT PRIMARY KEY IDENTITY(1,1),
        [POIId] INT NOT NULL,
        [OwnerId] INT NOT NULL,
        [RequestType] NVARCHAR(50), -- 'Create', 'Update'
        [Status] NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Approved', 'Rejected'
        [RequestData] NVARCHAR(MAX), -- JSON format: chứa thông tin cần duyệt
        [AdminComments] NVARCHAR(MAX), -- Ghi chú từ admin khi duyệt/từ chối
        [RequestedDate] DATETIME DEFAULT GETUTCDATE(),
        [ReviewedDate] DATETIME,
        [ReviewedBy] INT, -- Admin đã duyệt

        FOREIGN KEY ([POIId]) REFERENCES [dbo].[PointsOfInterest]([POIId]),
        FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[Users]([UserId]),
        FOREIGN KEY ([ReviewedBy]) REFERENCES [dbo].[Users]([UserId])
    );

    CREATE INDEX IX_POIRequests_Status ON [dbo].[POIApprovalRequests]([Status]);
    CREATE INDEX IX_POIRequests_OwnerId ON [dbo].[POIApprovalRequests]([OwnerId]);
    CREATE INDEX IX_POIRequests_POIId ON [dbo].[POIApprovalRequests]([POIId]);
END
GO

-- =============================================
-- 5. AUDIT LOG TABLE - Bảng ghi log hoạt động
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AuditLogs]
    (
        [LogId] INT PRIMARY KEY IDENTITY(1,1),
        [UserId] INT,
        [Action] NVARCHAR(100), -- 'Create', 'Update', 'Delete', 'Approve', 'Reject', 'Login'
        [TableName] NVARCHAR(50),
        [RecordId] INT,
        [OldValue] NVARCHAR(MAX),
        [NewValue] NVARCHAR(MAX),
        [Description] NVARCHAR(MAX),
        [IPAddress] NVARCHAR(50),
        [Timestamp] DATETIME DEFAULT GETUTCDATE(),

        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId])
    );

    CREATE INDEX IX_AuditLogs_UserId ON [dbo].[AuditLogs]([UserId]);
    CREATE INDEX IX_AuditLogs_Timestamp ON [dbo].[AuditLogs]([Timestamp]);
    CREATE INDEX IX_AuditLogs_Action ON [dbo].[AuditLogs]([Action]);
END
GO

-- =============================================
-- 6. POI IMAGES/MEDIA TABLE - Bảng lưu hình ảnh POI
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POIMedia]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[POIMedia]
    (
        [MediaId] INT PRIMARY KEY IDENTITY(1,1),
        [POIId] INT NOT NULL,
        [MediaType] NVARCHAR(50), -- 'Image', 'Video'
        [FilePath] NVARCHAR(MAX),
        [FileName] NVARCHAR(255),
        [FileSize] BIGINT, -- Kích thước file (bytes)
        [Description] NVARCHAR(500),
        [IsMainImage] BIT DEFAULT 0,
        [UploadedDate] DATETIME DEFAULT GETUTCDATE(),

        FOREIGN KEY ([POIId]) REFERENCES [dbo].[PointsOfInterest]([POIId]) ON DELETE CASCADE
    );

    CREATE INDEX IX_POIMedia_POIId ON [dbo].[POIMedia]([POIId]);
END
GO

-- =============================================
-- 7. LANGUAGES TABLE - Bảng danh sách các ngôn ngữ hỗ trợ
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Languages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Languages]
    (
        [LanguageId] INT PRIMARY KEY IDENTITY(1,1),
        [LanguageCode] NVARCHAR(10) NOT NULL UNIQUE, -- 'en', 'vi', 'es', 'fr', 'de', 'ja', 'zh'
        [LanguageName] NVARCHAR(50) NOT NULL, -- 'English', 'Tiếng Việt', 'Spanish', v.v.
        [NativeName] NVARCHAR(50), -- Tên ngôn ngữ trong chính ngôn ngữ đó
        [IsActive] BIT DEFAULT 1
    );

    CREATE INDEX IX_Languages_LanguageCode ON [dbo].[Languages]([LanguageCode]);
END
GO

-- =============================================
-- 8. USER PREFERENCES TABLE - Bảng tùy chọn người dùng
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPreferences]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserPreferences]
    (
        [PreferenceId] INT PRIMARY KEY IDENTITY(1,1),
        [UserId] INT NOT NULL,
        [PreferenceKey] NVARCHAR(100),
        [PreferenceValue] NVARCHAR(MAX),

        FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) ON DELETE CASCADE,
        UNIQUE([UserId], [PreferenceKey])
    );

    CREATE INDEX IX_UserPreferences_UserId ON [dbo].[UserPreferences]([UserId]);
END
GO

-- =============================================
-- 9. TOURIST SESSIONS TABLE - Định danh anonymous tourist theo DeviceId + IP metadata
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TouristSessions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TouristSessions]
    (
        [SessionId] INT PRIMARY KEY IDENTITY(1,1),
        [DeviceId] NVARCHAR(100) NOT NULL UNIQUE,
        [IPAddress] NVARCHAR(50),
        [Platform] NVARCHAR(30),
        [CurrentLatitude] DECIMAL(10, 8),
        [CurrentLongitude] DECIMAL(11, 8),
        [IsActive] BIT NOT NULL DEFAULT 1,
        [FirstSeenUtc] DATETIME NOT NULL DEFAULT GETUTCDATE(),
        [LastSeenUtc] DATETIME NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_TouristSessions_LastSeenUtc ON [dbo].[TouristSessions]([LastSeenUtc]);
END
GO

IF COL_LENGTH('dbo.TouristSessions', 'CurrentLatitude') IS NULL
BEGIN
    ALTER TABLE [dbo].[TouristSessions] ADD [CurrentLatitude] DECIMAL(10, 8) NULL;
END
GO

IF COL_LENGTH('dbo.TouristSessions', 'CurrentLatitude') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TouristSessions] ALTER COLUMN [CurrentLatitude] DECIMAL(10, 8) NULL;
END
GO

IF COL_LENGTH('dbo.TouristSessions', 'CurrentLongitude') IS NULL
BEGIN
    ALTER TABLE [dbo].[TouristSessions] ADD [CurrentLongitude] DECIMAL(11, 8) NULL;
END
GO

IF COL_LENGTH('dbo.TouristSessions', 'CurrentLongitude') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TouristSessions] ALTER COLUMN [CurrentLongitude] DECIMAL(11, 8) NULL;
END
GO

IF COL_LENGTH('dbo.TouristSessions', 'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[TouristSessions] ADD [IsActive] BIT NOT NULL CONSTRAINT DF_TouristSessions_IsActive DEFAULT 1;
END
GO

-- =============================================
-- INSERT INITIAL DATA
-- =============================================

-- Insert Languages
IF NOT EXISTS (SELECT * FROM [dbo].[Languages] WHERE LanguageCode = 'en')
BEGIN
    INSERT INTO [dbo].[Languages] ([LanguageCode], [LanguageName], [NativeName], [IsActive])
    VALUES 
        ('en', 'English', 'English', 1),
        ('vi', 'Vietnamese', 'Tiếng Việt', 1),
        ('es', 'Spanish', 'Español', 1),
        ('fr', 'French', 'Français', 1),
        ('de', 'German', 'Deutsch', 1),
        ('ja', 'Japanese', '日本語', 1),
        ('zh', 'Chinese', '中文', 1),
        ('ko', 'Korean', '한국어', 1),
        ('th', 'Thai', 'ไทย', 1),
        ('id', 'Indonesian', 'Bahasa Indonesia', 1);
END
GO

-- Insert Default Admin User
-- Default: username = 'admin', password = 'Admin@123' (hash này là hash của 'Admin@123')
-- Lưu ý: Trong production, sử dụng proper password hashing (bcrypt, PBKDF2, v.v.)
IF NOT EXISTS (SELECT * FROM [dbo].[Users] WHERE Username = 'admin')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [PasswordHash], [Email], [FullName], [Role], [PhoneNumber], [IsActive])
    VALUES 
        ('admin', '$2a$11$JqDHSJJPDrzMRqKYk3EJj.S0XtQNXyxxO9zKG9HxsN5qQTZSfnOTK', 'admin@thuyetminh.vn', 'Administrator', 'Admin', '0123456789', 1);
END
GO

-- Insert Sample POI Owner User
IF NOT EXISTS (SELECT * FROM [dbo].[Users] WHERE Username = 'poiowner01')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [PasswordHash], [Email], [FullName], [Role], [PhoneNumber], [IsActive])
    VALUES 
        ('poiowner01', '$2a$11$JqDHSJJPDrzMRqKYk3EJj.S0XtQNXyxxO9zKG9HxsN5qQTZSfnOTK', 'owner1@thuyetminh.vn', 'Trần Văn A', 'POIOwner', '0987654321', 1);
END
GO

-- Insert Sample POI Data
INSERT INTO [dbo].[PointsOfInterest] 
    ([POIName], [Description], [Latitude], [Longitude], [Radius], 
     [Category], [Address], [PhoneNumber], [OwnerId], [IsApproved], [Status])
VALUES 
    (N'Phở Bắc Hà', 
     N'Quán phở nổi tiếng với nước dùng được nấu trong 12 tiếng. Phục vụ phở bò ngon lành với giá hợp lý. Mở từ 5h sáng đến 11h đêm.', 
     21.028511, 105.854100, 0.5, N'Restaurant', N'45 Hàng Mành, Hoàn Kiếm, Hà Nội', N'024 3938 1485', 2, 1, N'Active'),

    (N'Cafe Trời Xanh', 
     N'Cafe nhỏ xinh với không gian thoáng mát, thích hợp để học tập hoặc gặp gỡ bạn bè. Cà phê được rang tại chỗ hàng ngày. Có WiFi miễn phí và điều hòa.', 
     21.027500, 105.853800, 0.4, N'Cafe', N'23 Cửa Bắc, Hoàn Kiếm, Hà Nội', N'024 3935 9283', 2, 1, N'Active'),

    (N'Tạp Hóa Minh Phúc', 
     N'Cửa hàng tạp hóa đầy đủ các mặt hàng ăn uống, mỹ phẩm, nước rửa tay, mặt nạ y tế, và các sản phẩm tiêu dùng hàng ngày khác. Mở từ 6h sáng đến 11h đêm hàng ngày.', 
     21.029000, 105.855200, 0.3, N'Shop', N'67 Hàng Bông, Hoàn Kiếm, Hà Nội', N'024 3936 2015', 2, 1, N'Active');

-- Create View for POI with Owner Information
CREATE OR ALTER VIEW [dbo].[vw_POIWithOwner] AS
SELECT 
    p.[POIId],
    p.[POIName],
    p.[Description],
    p.[Latitude],
    p.[Longitude],
    p.[Radius],
    p.[Category],
    p.[Address],
    p.[PhoneNumber],
    p.[Website],
    p.[OwnerId],
    u.[FullName] AS [OwnerName],
    u.[Email] AS [OwnerEmail],
    p.[IsApproved],
    p.[Status],
    p.[CreatedDate],
    p.[LastModifiedDate]
FROM [dbo].[PointsOfInterest] p
LEFT JOIN [dbo].[Users] u ON p.[OwnerId] = u.[UserId];
GO

-- Stored Procedure: Get POI near location (dùng POI.Radius làm ngưỡng lọc)
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPOINearLocation]
    @Latitude DECIMAL(10, 8),
    @Longitude DECIMAL(11, 8)
AS
BEGIN
    SELECT 
        p.[POIId],
        p.[POIName],
        p.[Description],
        p.[Latitude],
        p.[Longitude],
        p.[Radius],
        p.[Category],
        p.[Address],
        p.[PhoneNumber],
        p.[Website],
        (6371 * ACOS(
            COS(RADIANS(@Latitude)) * COS(RADIANS(p.[Latitude])) *
            COS(RADIANS(p.[Longitude]) - RADIANS(@Longitude)) +
            SIN(RADIANS(@Latitude)) * SIN(RADIANS(p.[Latitude]))
        )) AS [DistanceKm]
    FROM [dbo].[PointsOfInterest] p
    WHERE p.[IsApproved] = 1
      AND p.[Status] = 'Active'
      AND (6371 * ACOS(
            COS(RADIANS(@Latitude)) * COS(RADIANS(p.[Latitude])) *
            COS(RADIANS(p.[Longitude]) - RADIANS(@Longitude)) +
            SIN(RADIANS(@Latitude)) * SIN(RADIANS(p.[Latitude]))
          )) <= p.[Radius]
    ORDER BY [DistanceKm] ASC;
END
GO

-- Stored Procedure: Create POI Approval Request
CREATE OR ALTER PROCEDURE [dbo].[sp_CreatePOIApprovalRequest]
    @POIId INT,
    @OwnerId INT,
    @RequestType NVARCHAR(50),
    @RequestData NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO [dbo].[POIApprovalRequests] ([POIId], [OwnerId], [RequestType], [RequestData])
    VALUES (@POIId, @OwnerId, @RequestType, @RequestData);

    SELECT SCOPE_IDENTITY() AS [NewRequestId];
END
GO

-- Stored Procedure: Upsert Tourist Session by DeviceId
CREATE OR ALTER PROCEDURE [dbo].[sp_UpsertTouristSession]
    @DeviceId NVARCHAR(100),
    @IPAddress NVARCHAR(50) = NULL,
    @Platform NVARCHAR(30) = NULL,
    @CurrentLatitude DECIMAL(10, 8) = NULL,
    @CurrentLongitude DECIMAL(11, 8) = NULL,
    @IsActive BIT = 1,
    @OfflineAfterMinutes INT = 1
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[TouristSessions]
    SET [IsActive] = 0
    WHERE [LastSeenUtc] < DATEADD(MINUTE, -@OfflineAfterMinutes, GETUTCDATE())
      AND [IsActive] = 1;

    IF EXISTS (SELECT 1 FROM [dbo].[TouristSessions] WHERE [DeviceId] = @DeviceId)
    BEGIN
        UPDATE [dbo].[TouristSessions]
        SET
            [IPAddress] = COALESCE(@IPAddress, [IPAddress]),
            [Platform] = COALESCE(@Platform, [Platform]),
            [CurrentLatitude] = COALESCE(@CurrentLatitude, [CurrentLatitude]),
            [CurrentLongitude] = COALESCE(@CurrentLongitude, [CurrentLongitude]),
            [IsActive] = 1,
            [LastSeenUtc] = GETUTCDATE()
        WHERE [DeviceId] = @DeviceId;
    END
    ELSE
    BEGIN
        INSERT INTO [dbo].[TouristSessions] ([DeviceId], [IPAddress], [Platform], [CurrentLatitude], [CurrentLongitude], [IsActive])
        VALUES (@DeviceId, @IPAddress, @Platform, @CurrentLatitude, @CurrentLongitude, 1);
    END
END
GO

-- =============================================
-- INDEXES FOR PERFORMANCE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_POI_Status_Approved')
BEGIN
    CREATE NONCLUSTERED INDEX IX_POI_Status_Approved ON [dbo].[PointsOfInterest]([Status], [IsApproved]) WHERE [IsApproved] = 1;
END
GO

PRINT 'Database creation completed successfully!';