// =====================================================
// Entity Models for ThuyetMinhDaNgonNgu Database
// Usage: Use these models with Entity Framework Core
// =====================================================

using System;
using System.Collections.Generic;

namespace ThuyetMinhDaNgonNgu.Models
{
    /// <summary>
    /// Bảng Users - Quản lý người dùng Admin và POI Owner
    /// </summary>
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; } // 'Admin' or 'POIOwner'
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Navigation properties
        public virtual ICollection<PointOfInterest> OwnedPOIs { get; set; }
        public virtual ICollection<PointOfInterest> ApprovedPOIs { get; set; }
        public virtual ICollection<POIApprovalRequest> ApprovedRequests { get; set; }
        public virtual ICollection<POIApprovalRequest> SubmittedRequests { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; }
        public virtual ICollection<UserPreference> Preferences { get; set; }
    }

    /// <summary>
    /// Bảng PointsOfInterest - Quản lý các địa điểm du lịch
    /// </summary>
    public class PointOfInterest
    {
        public int POIId { get; set; }
        public string POIName { get; set; }
        public string Description { get; set; } // Mô tả tiếng Việt
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public float Radius { get; set; }
        public string Category { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public int? OwnerId { get; set; }
        public bool IsApproved { get; set; }
        public string Status { get; set; } // 'Active', 'Inactive', 'Pending'
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedBy { get; set; }

        // Navigation properties
        public virtual User Owner { get; set; }
        public virtual User ApprovedByUser { get; set; }
        public virtual ICollection<POITranslation> Translations { get; set; }
        public virtual ICollection<POIApprovalRequest> ApprovalRequests { get; set; }
        public virtual ICollection<POIMedia> Media { get; set; }
    }

    /// <summary>
    /// Bảng POITranslations - Lưu trữ bản dịch mô tả POI
    /// </summary>
    public class POITranslation
    {
        public int TranslationId { get; set; }
        public int POIId { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public string TranslatedDescription { get; set; }
        public string TranslatedName { get; set; }
        public int? TranslatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Navigation properties
        public virtual PointOfInterest POI { get; set; }
    }

    /// <summary>
    /// Bảng POIApprovalRequests - Quản lý yêu cầu duyệt POI
    /// </summary>
    public class POIApprovalRequest
    {
        public int RequestId { get; set; }
        public int POIId { get; set; }
        public int OwnerId { get; set; }
        public string RequestType { get; set; } // 'Create', 'Update'
        public string Status { get; set; } // 'Pending', 'Approved', 'Rejected'
        public string RequestData { get; set; } // JSON format
        public string AdminComments { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public int? ReviewedBy { get; set; }

        // Navigation properties
        public virtual PointOfInterest POI { get; set; }
        public virtual User Owner { get; set; }
        public virtual User ReviewedByUser { get; set; }
    }

    /// <summary>
    /// Bảng POIMedia - Lưu trữ hình ảnh và video của POI
    /// </summary>
    public class POIMedia
    {
        public int MediaId { get; set; }
        public int POIId { get; set; }
        public string MediaType { get; set; } // 'Image', 'Video'
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string Description { get; set; }
        public bool IsMainImage { get; set; }
        public DateTime UploadedDate { get; set; }

        // Navigation properties
        public virtual PointOfInterest POI { get; set; }
    }

    /// <summary>
    /// Bảng Languages - Danh sách các ngôn ngữ hỗ trợ
    /// </summary>
    public class Language
    {
        public int LanguageId { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public string NativeName { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Bảng AuditLogs - Ghi log hoạt động của người dùng
    /// </summary>
    public class AuditLog
    {
        public int LogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public int? RecordId { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Description { get; set; }
        public string IPAddress { get; set; }
        public DateTime Timestamp { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
    }

    /// <summary>
    /// Bảng UserPreferences - Lưu trữ tùy chọn cá nhân của người dùng
    /// </summary>
    public class UserPreference
    {
        public int PreferenceId { get; set; }
        public int UserId { get; set; }
        public string PreferenceKey { get; set; }
        public string PreferenceValue { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
    }
}
