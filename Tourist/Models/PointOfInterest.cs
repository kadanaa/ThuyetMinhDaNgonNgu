namespace Tourist.Models
{
    public class PointOfInterest
    {
        public int POIId { get; set; }
        public string POIName { get; set; }
        public string Description { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public float Radius { get; set; }
        public string Category { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public bool IsApproved { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<POITranslation> Translations { get; set; } = new List<POITranslation>();
    }

    public class POITranslation
    {
        public int TranslationId { get; set; }
        public int POIId { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public string TranslatedDescription { get; set; }
        public string TranslatedName { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual PointOfInterest POI { get; set; }
    }

    public class Language
    {
        public int LanguageId { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public string NativeName { get; set; }
        public bool IsActive { get; set; }
    }
}
