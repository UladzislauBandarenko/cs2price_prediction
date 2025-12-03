using cs2price_prediction.Domain.Meta;
using cs2price_prediction.Domain.Patterns;
using cs2price_prediction.Domain.Stickers;
using Microsoft.EntityFrameworkCore;

namespace cs2price_prediction.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<WeaponType> WeaponTypes => Set<WeaponType>();
        public DbSet<Weapon> Weapons => Set<Weapon>();
        public DbSet<WearTier> WearTiers => Set<WearTier>();
        public DbSet<Skin> Skins => Set<Skin>();

        public DbSet<SkinWearTier> SkinWearTiers => Set<SkinWearTier>();

        public DbSet<CaseHardenedGunPattern> CaseHardenedGunPatterns => Set<CaseHardenedGunPattern>();
        public DbSet<CaseHardenedKnifePattern> CaseHardenedKnifePatterns => Set<CaseHardenedKnifePattern>();
        public DbSet<FadeGunPattern> FadeGunPatterns => Set<FadeGunPattern>();
        public DbSet<FadeKnifePattern> FadeKnifePatterns => Set<FadeKnifePattern>();

        public DbSet<DopplerPhase> DopplerPhases => Set<DopplerPhase>();
        public DbSet<DopplerSkinPhase> DopplerSkinPhases => Set<DopplerSkinPhase>();

        public DbSet<Sticker> Stickers => Set<Sticker>();
        public DbSet<StickerPrice> StickerPrices => Set<StickerPrice>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("cs2");

            // WeaponType
            modelBuilder.Entity<WeaponType>(entity =>
            {
                entity.ToTable("weapon_types");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // Weapon
            modelBuilder.Entity<Weapon>(entity =>
            {
                entity.ToTable("weapons");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();

                entity.HasOne(e => e.WeaponType)
                      .WithMany(t => t.Weapons)
                      .HasForeignKey(e => e.WeaponTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // WearTier
            modelBuilder.Entity<WearTier>(entity =>
            {
                entity.ToTable("wear_tiers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Skin
            modelBuilder.Entity<Skin>(entity =>
            {
                entity.ToTable("skins");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PatternStyle).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Weapon)
                      .WithMany(w => w.Skins)
                      .HasForeignKey(e => e.WeaponId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.WeaponId, e.Name }).IsUnique();
            });

            // NEW: SkinWearTier
            modelBuilder.Entity<SkinWearTier>(entity =>
            {
                entity.ToTable("skin_wear_tiers");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Skin)
                      .WithMany(s => s.SkinWearTiers)
                      .HasForeignKey(e => e.SkinId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.WearTier)
                      .WithMany(w => w.SkinWearTiers)
                      .HasForeignKey(e => e.WearTierId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.SkinId, e.WearTierId }).IsUnique();
            });

            // CH gun
            modelBuilder.Entity<CaseHardenedGunPattern>(entity =>
            {
                entity.ToTable("case_hardened_gun_patterns");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Pattern).IsRequired();
                entity.Property(e => e.PlaysideBlue).IsRequired();
                entity.Property(e => e.BacksideBlue).IsRequired();

                entity.HasOne(e => e.Skin)
                      .WithMany(s => s.CaseHardenedGunPatterns)
                      .HasForeignKey(e => e.SkinId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.SkinId, e.Pattern }).IsUnique();
            });

            // CH knife
            modelBuilder.Entity<CaseHardenedKnifePattern>(entity =>
            {
                entity.ToTable("case_hardened_knife_patterns");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Pattern).IsRequired();
                entity.Property(e => e.BacksideBlue).IsRequired();
                entity.Property(e => e.PlaysideBlue).IsRequired();

                entity.HasOne(e => e.Skin)
                      .WithMany(s => s.CaseHardenedKnifePatterns)
                      .HasForeignKey(e => e.SkinId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.SkinId, e.Pattern }).IsUnique();
            });

            // Fade gun
            modelBuilder.Entity<FadeGunPattern>(entity =>
            {
                entity.ToTable("fade_gun_patterns");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Pattern).IsRequired();
                entity.Property(e => e.FadePercentage).IsRequired();
                entity.Property(e => e.FadeRank).IsRequired();

                entity.HasOne(e => e.Skin)
                      .WithMany(s => s.FadeGunPatterns)
                      .HasForeignKey(e => e.SkinId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.SkinId, e.Pattern }).IsUnique();
            });

            // Fade knife
            modelBuilder.Entity<FadeKnifePattern>(entity =>
            {
                entity.ToTable("fade_knife_patterns");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Pattern).IsRequired();
                entity.Property(e => e.FadePercentage).IsRequired();
                entity.Property(e => e.FadeRank).IsRequired();

                entity.HasOne(e => e.Skin)
                      .WithMany(s => s.FadeKnifePatterns)
                      .HasForeignKey(e => e.SkinId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.SkinId, e.Pattern }).IsUnique();
            });

            // Doppler phases
            modelBuilder.Entity<DopplerPhase>(entity =>
            {
                entity.ToTable("doppler_phases");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Doppler skin phases
            modelBuilder.Entity<DopplerSkinPhase>(entity =>
            {
                entity.ToTable("doppler_skin_phases");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Skin)
                      .WithMany(s => s.DopplerSkinPhases)
                      .HasForeignKey(e => e.SkinId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Phase)
                      .WithMany(p => p.DopplerSkinPhases)
                      .HasForeignKey(e => e.PhaseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.SkinId, e.PhaseId }).IsUnique();
            });

            // Stickers
            modelBuilder.Entity<Sticker>(entity =>
            {
                entity.ToTable("stickers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Sticker prices
            modelBuilder.Entity<StickerPrice>(entity =>
            {
                entity.ToTable("sticker_prices");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).IsRequired();

                entity.HasOne(e => e.Sticker)
                      .WithMany(s => s.Prices)
                      .HasForeignKey(e => e.StickerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
