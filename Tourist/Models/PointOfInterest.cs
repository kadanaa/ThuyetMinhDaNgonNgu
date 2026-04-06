namespace Tourist.Models
{
    public class PointOfInterest
    {
        public int POIId { get; set; }
        public string POIName { get; set; }       // NOT NULL trong DB
        public string? Description { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double Radius { get; set; }
        public string? Category { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Website { get; set; }
        public int? OwnerId { get; set; }
        public bool IsApproved { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedBy { get; set; }

        public virtual ICollection<POITranslation> Translations { get; set; } = new List<POITranslation>();
    }

    public class POITranslation
    {
        public int TranslationId { get; set; }
        public int POIId { get; set; }
        public string LanguageCode { get; set; }        // NOT NULL trong DB
        public string? LanguageName { get; set; }
        public string? TranslatedDescription { get; set; }
        public string? TranslatedName { get; set; }
        public int? TranslatedBy { get; set; }          // có trong DB, nullable
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }  // có trong DB, NOT NULL

        public virtual PointOfInterest POI { get; set; }
    }

    public class Language
    {
        public int LanguageId { get; set; }
        public string LanguageCode { get; set; }    // NOT NULL trong DB
        public string LanguageName { get; set; }    // NOT NULL trong DB
        public string? NativeName { get; set; }
        public bool IsActive { get; set; }
    }
}