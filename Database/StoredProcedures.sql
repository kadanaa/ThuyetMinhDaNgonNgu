-- =====================================================
-- STORED PROCEDURES & UTILITIES
-- ThuyetMinhDaNgonNgu Database
-- =====================================================

USE ThuyetMinhDaNgonNgu;
GO

-- =====================================================
-- 1. AUTHENTICATION PROCEDURES
-- =====================================================

-- Procedure: Get user by username for login
-- Lưu ý: KHÔNG so sánh password trong SQL vì dùng BCrypt.
-- Application sẽ nhận PasswordHash về rồi dùng BCrypt.Verify() để kiểm tra.
CREATE OR ALTER PROCEDURE [dbo].[sp_VerifyLogin]
        @Username NVARCHAR(100)
    AS
    BEGIN
        SELECT 
            [UserId],
            [Username],
            [PasswordHash],
            [Email],
            [FullName],
            [Role],
            [IsActive]
        FROM [dbo].[Users]
        WHERE [Username] = @Username
          AND [IsActive] = 1;
END
GO

-- =====================================================
-- 2. POI PROCEDURES
-- =====================================================

-- Procedure: Get all approved POIs
CREATE OR ALTER PROCEDURE [dbo].[sp_GetAllApprovedPOIs]
    AS
    BEGIN
        SELECT 
            [POIId],
            [POIName],
            [Description],
            [Latitude],
            [Longitude],
            [Radius],
            [Category],
            [Address],
            [PhoneNumber],
            [Website]
        FROM [dbo].[PointsOfInterest]
        WHERE [IsApproved] = 1 AND [Status] = 'Active'
        ORDER BY [POIName];
END
GO

-- Procedure: Get POI by ID with all details
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPOIById]
        @POIId INT
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
            u.[FullName] AS [OwnerName],
            u.[Email] AS [OwnerEmail],
            p.[IsApproved],
            p.[Status],
            p.[CreatedDate]
        FROM [dbo].[PointsOfInterest] p
        LEFT JOIN [dbo].[Users] u ON p.[OwnerId] = u.[UserId]
        WHERE p.[POIId] = @POIId;
END
GO

-- Procedure: Get POI where tourist is INSIDE each POI's coverage radius
-- Logic: distance(tourist, POI) <= POI.Radius (km)
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPOINearLocationAdvanced]
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

-- Procedure: Get POI by category
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPOIByCategory]
        @Category NVARCHAR(50)
    AS
    BEGIN
        SELECT 
            [POIId],
            [POIName],
            [Description],
            [Latitude],
            [Longitude],
            [Radius],
            [Category],
            [Address],
            [PhoneNumber],
            [Website]
        FROM [dbo].[PointsOfInterest]
        WHERE [Category] = @Category 
          AND [IsApproved] = 1 
          AND [Status] = 'Active'
        ORDER BY [POIName];
END
GO

-- Procedure: Create new POI
CREATE OR ALTER PROCEDURE [dbo].[sp_CreatePOI]
        @POIName NVARCHAR(200),
        @Description NVARCHAR(MAX),
        @Latitude DECIMAL(10, 8),
        @Longitude DECIMAL(11, 8),
        @Radius FLOAT,
        @Category NVARCHAR(50),
        @Address NVARCHAR(300),
        @PhoneNumber NVARCHAR(20),
        @Website NVARCHAR(200),
        @OwnerId INT,
        @IsApproved BIT = 0
    AS
    BEGIN
        INSERT INTO [dbo].[PointsOfInterest]
            ([POIName], [Description], [Latitude], [Longitude], [Radius], 
             [Category], [Address], [PhoneNumber], [Website], [OwnerId], [IsApproved])
        VALUES
            (@POIName, @Description, @Latitude, @Longitude, @Radius,
             @Category, @Address, @PhoneNumber, @Website, @OwnerId, @IsApproved);

        SELECT SCOPE_IDENTITY() AS [NewPOIId];
END
GO

-- Procedure: Update POI
CREATE OR ALTER PROCEDURE [dbo].[sp_UpdatePOI]
        @POIId INT,
        @POIName NVARCHAR(200),
        @Description NVARCHAR(MAX),
        @Latitude DECIMAL(10, 8),
        @Longitude DECIMAL(11, 8),
        @Radius FLOAT,
        @Category NVARCHAR(50),
        @Address NVARCHAR(300),
        @PhoneNumber NVARCHAR(20),
        @Website NVARCHAR(200)
    AS
    BEGIN
        UPDATE [dbo].[PointsOfInterest]
        SET 
            [POIName] = @POIName,
            [Description] = @Description,
            [Latitude] = @Latitude,
            [Longitude] = @Longitude,
            [Radius] = @Radius,
            [Category] = @Category,
            [Address] = @Address,
            [PhoneNumber] = @PhoneNumber,
            [Website] = @Website,
            [LastModifiedDate] = GETUTCDATE()
        WHERE [POIId] = @POIId;
END
GO

-- Procedure: Delete POI
CREATE OR ALTER PROCEDURE [dbo].[sp_DeletePOI]
        @POIId INT
    AS
    BEGIN
        DELETE FROM [dbo].[POIMedia] WHERE [POIId] = @POIId;
        DELETE FROM [dbo].[POITranslations] WHERE [POIId] = @POIId;
        DELETE FROM [dbo].[POIApprovalRequests] WHERE [POIId] = @POIId;
        DELETE FROM [dbo].[PointsOfInterest] WHERE [POIId] = @POIId;
END
GO

-- =====================================================
-- 3. POI TRANSLATION PROCEDURES
-- =====================================================

-- Procedure: Get translations for POI
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPOITranslations]
        @POIId INT
    AS
    BEGIN
        SELECT 
            [TranslationId],
            [POIId],
            [LanguageCode],
            [LanguageName],
            [TranslatedDescription],
            [TranslatedName]
        FROM [dbo].[POITranslations]
        WHERE [POIId] = @POIId;
END
GO

-- Procedure: Get translation by language
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPOITranslationByLanguage]
        @POIId INT,
        @LanguageCode NVARCHAR(10)
    AS
    BEGIN
        SELECT 
            [TranslatedDescription],
            [TranslatedName]
        FROM [dbo].[POITranslations]
        WHERE [POIId] = @POIId AND [LanguageCode] = @LanguageCode;
END
GO

-- Procedure: Add or update translation
CREATE OR ALTER PROCEDURE [dbo].[sp_UpsertPOITranslation]
        @POIId INT,
        @LanguageCode NVARCHAR(10),
        @LanguageName NVARCHAR(50),
        @TranslatedDescription NVARCHAR(MAX),
        @TranslatedName NVARCHAR(200)
    AS
    BEGIN
        IF EXISTS (SELECT 1 FROM [dbo].[POITranslations] 
                   WHERE [POIId] = @POIId AND [LanguageCode] = @LanguageCode)
        BEGIN
            UPDATE [dbo].[POITranslations]
            SET 
                [TranslatedDescription] = @TranslatedDescription,
                [TranslatedName] = @TranslatedName,
                [LastModifiedDate] = GETUTCDATE()
            WHERE [POIId] = @POIId AND [LanguageCode] = @LanguageCode;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[POITranslations]
                ([POIId], [LanguageCode], [LanguageName], [TranslatedDescription], [TranslatedName])
            VALUES
                (@POIId, @LanguageCode, @LanguageName, @TranslatedDescription, @TranslatedName);
        END
END
GO

-- =====================================================
-- 4. APPROVAL REQUEST PROCEDURES
-- =====================================================

-- Procedure: Get pending approval requests
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPendingApprovalRequests]
    AS
    BEGIN
        SELECT 
            r.[RequestId],
            r.[POIId],
            r.[OwnerId],
            u.[FullName] AS [OwnerName],
            r.[RequestType],
            r.[Status],
            r.[RequestData],
            r.[RequestedDate],
            p.[POIName]
        FROM [dbo].[POIApprovalRequests] r
        JOIN [dbo].[Users] u ON r.[OwnerId] = u.[UserId]
        JOIN [dbo].[PointsOfInterest] p ON r.[POIId] = p.[POIId]
        WHERE r.[Status] = 'Pending'
        ORDER BY r.[RequestedDate] DESC;
END
GO

-- Procedure: Approve POI request
CREATE OR ALTER PROCEDURE [dbo].[sp_ApprovePOIRequest]
        @RequestId INT,
        @AdminId INT,
        @Comments NVARCHAR(MAX) = NULL
    AS
    BEGIN
        DECLARE @POIId INT;

        -- Get POI ID from request
        SELECT @POIId = [POIId] FROM [dbo].[POIApprovalRequests] WHERE [RequestId] = @RequestId;

        -- Update request status
        UPDATE [dbo].[POIApprovalRequests]
        SET 
            [Status] = 'Approved',
            [ReviewedDate] = GETUTCDATE(),
            [ReviewedBy] = @AdminId,
            [AdminComments] = @Comments
        WHERE [RequestId] = @RequestId;

        -- Update POI approval status
        UPDATE [dbo].[PointsOfInterest]
        SET 
            [IsApproved] = 1,
            [ApprovedDate] = GETUTCDATE(),
            [ApprovedBy] = @AdminId,
            [Status] = 'Active'
        WHERE [POIId] = @POIId;
END
GO

-- Procedure: Reject POI request
CREATE OR ALTER PROCEDURE [dbo].[sp_RejectPOIRequest]
        @RequestId INT,
        @AdminId INT,
        @Comments NVARCHAR(MAX)
    AS
    BEGIN
        UPDATE [dbo].[POIApprovalRequests]
        SET 
            [Status] = 'Rejected',
            [ReviewedDate] = GETUTCDATE(),
            [ReviewedBy] = @AdminId,
            [AdminComments] = @Comments
        WHERE [RequestId] = @RequestId;
END
GO

-- =====================================================
-- 5. USER PROCEDURES
-- =====================================================

-- Procedure: Create new user
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateUser]
        @Username NVARCHAR(100),
        @PasswordHash NVARCHAR(MAX),
        @Email NVARCHAR(100),
        @FullName NVARCHAR(200),
        @Role NVARCHAR(50),
        @PhoneNumber NVARCHAR(20) = NULL
    AS
    BEGIN
        INSERT INTO [dbo].[Users]
            ([Username], [PasswordHash], [Email], [FullName], [Role], [PhoneNumber])
        VALUES
            (@Username, @PasswordHash, @Email, @FullName, @Role, @PhoneNumber);

        SELECT SCOPE_IDENTITY() AS [NewUserId];
END
GO

-- Procedure: Get user by username
CREATE OR ALTER PROCEDURE [dbo].[sp_GetUserByUsername]
        @Username NVARCHAR(100)
    AS
    BEGIN
        SELECT 
            [UserId],
            [Username],
            [Email],
            [FullName],
            [Role],
            [PhoneNumber],
            [IsActive],
            [CreatedDate]
        FROM [dbo].[Users]
        WHERE [Username] = @Username;
END
GO

-- Procedure: Get all POI owners
CREATE OR ALTER PROCEDURE [dbo].[sp_GetAllPOIOwners]
    AS
    BEGIN
        SELECT 
            [UserId],
            [Username],
            [Email],
            [FullName],
            [PhoneNumber],
            [IsActive],
            [CreatedDate]
        FROM [dbo].[Users]
        WHERE [Role] = 'POIOwner'
        ORDER BY [FullName];
END
GO

-- =====================================================
-- 6. AUDIT LOG PROCEDURES
-- =====================================================

-- Procedure: Add audit log
CREATE OR ALTER PROCEDURE [dbo].[sp_AddAuditLog]
        @UserId INT,
        @Action NVARCHAR(100),
        @TableName NVARCHAR(50),
        @RecordId INT,
        @OldValue NVARCHAR(MAX) = NULL,
        @NewValue NVARCHAR(MAX) = NULL,
        @Description NVARCHAR(MAX) = NULL,
        @IPAddress NVARCHAR(50) = NULL
    AS
    BEGIN
        INSERT INTO [dbo].[AuditLogs]
            ([UserId], [Action], [TableName], [RecordId], [OldValue], [NewValue], [Description], [IPAddress])
        VALUES
            (@UserId, @Action, @TableName, @RecordId, @OldValue, @NewValue, @Description, @IPAddress);
END
GO

-- Procedure: Get audit logs for user
CREATE OR ALTER PROCEDURE [dbo].[sp_GetAuditLogsByUser]
        @UserId INT,
        @Days INT = 30
    AS
    BEGIN
        SELECT 
            [LogId],
            [UserId],
            [Action],
            [TableName],
            [RecordId],
            [Description],
            [Timestamp]
        FROM [dbo].[AuditLogs]
        WHERE [UserId] = @UserId 
          AND [Timestamp] >= DATEADD(DAY, -@Days, GETUTCDATE())
        ORDER BY [Timestamp] DESC;
END
GO

-- =====================================================
-- 7. STATISTICS PROCEDURES
-- =====================================================

-- Procedure: Get dashboard statistics
CREATE OR ALTER PROCEDURE [dbo].[sp_GetDashboardStats]
    AS
    BEGIN
        SELECT 
            (SELECT COUNT(*) FROM [dbo].[PointsOfInterest] WHERE [IsApproved] = 1) AS [ApprovedPOICount],
            (SELECT COUNT(*) FROM [dbo].[PointsOfInterest] WHERE [IsApproved] = 0) AS [PendingPOICount],
            (SELECT COUNT(*) FROM [dbo].[Users] WHERE [Role] = 'POIOwner') AS [POIOwnerCount],
            (SELECT COUNT(*) FROM [dbo].[POIApprovalRequests] WHERE [Status] = 'Pending') AS [PendingRequestsCount],
            (SELECT COUNT(DISTINCT [Category]) FROM [dbo].[PointsOfInterest]) AS [CategoriesCount];
END
GO

-- Procedure: Get POI statistics
CREATE OR ALTER PROCEDURE [dbo].[sp_GetPOIStatisticsByCategory]
    AS
    BEGIN
        SELECT 
            [Category],
            COUNT(*) AS [Count],
            COUNT(CASE WHEN [IsApproved] = 1 THEN 1 END) AS [ApprovedCount],
            COUNT(CASE WHEN [IsApproved] = 0 THEN 1 END) AS [PendingCount]
        FROM [dbo].[PointsOfInterest]
        GROUP BY [Category]
        ORDER BY [Count] DESC;
END
GO

PRINT 'All stored procedures created successfully!';