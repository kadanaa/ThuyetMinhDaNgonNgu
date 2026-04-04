using System;
using Microsoft.EntityFrameworkCore;
using Tourist.Models;

namespace Tourist.Data
{
    public class ThuyetMinhDbContext : DbContext
    {
        private readonly string _connectionString;

        public ThuyetMinhDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<PointOfInterest> PointsOfInterest { get; set; }
        public DbSet<POITranslation> POITranslations { get; set; }
        public DbSet<Language> Languages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PointOfInterest
            modelBuilder.Entity<PointOfInterest>(entity =>
            {
                entity.HasKey(e => e.POIId);
                entity.Property(e => e.POIName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Latitude).HasPrecision(10, 8);
                entity.Property(e => e.Longitude).HasPrecision(11, 8);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.HasMany(e => e.Translations).WithOne(t => t.POI).HasForeignKey(t => t.POIId).OnDelete(DeleteBehavior.Cascade);
            });

            // Configure POITranslation
            modelBuilder.Entity<POITranslation>(entity =>
            {
                entity.HasKey(e => e.TranslationId);
                entity.Property(e => e.LanguageCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.LanguageName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TranslatedDescription).HasMaxLength(2000);
                entity.Property(e => e.TranslatedName).HasMaxLength(255);
            });

            // Configure Language
            modelBuilder.Entity<Language>(entity =>
            {
                entity.HasKey(e => e.LanguageId);
                entity.Property(e => e.LanguageCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.LanguageName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NativeName).HasMaxLength(100);
            });
        }
    }
}
