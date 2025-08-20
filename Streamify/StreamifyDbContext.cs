using Microsoft.EntityFrameworkCore;
using Streamify.Data;

namespace Streamify;

public class StreamifyDbContext : DbContext
{
    public StreamifyDbContext(DbContextOptions<StreamifyDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<Episode> Episodes => Set<Episode>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();
    public DbSet<WatchHistory> WatchHistories => Set<WatchHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Email).IsRequired();
            b.Property(u => u.Role).IsRequired();
            b.HasMany(u => u.WatchHistoryEntries)
                .WithOne(h => h.User)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Series>(b =>
        {
            b.HasKey(s => s.SeriesId);
            b.Property(s => s.Title).IsRequired();
            b.Property(s => s.TmdbId).IsRequired();
            b.HasIndex(s => s.TmdbId).IsUnique();
            b.HasMany(s => s.Episodes)
                .WithOne(e => e.Series)
                .HasForeignKey(e => e.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Episode>(b =>
        {
            b.HasKey(e => e.EpisodeId);
            b.Property(e => e.Title).IsRequired();
            b.HasOne(e => e.MediaFile)
                .WithOne(m => m.Episode!)
                .HasForeignKey<MediaFile>(m => m.EpisodeId)
                .IsRequired(false);
        });

        modelBuilder.Entity<Movie>(b =>
        {
            b.HasKey(m => m.MovieId);
            b.Property(m => m.Title).IsRequired();
            b.Property(m => m.TmdbId).IsRequired();
            b.HasIndex(m => m.TmdbId).IsUnique();
            b.HasMany(m => m.MediaFiles)
                .WithOne(f => f.Movie)
                .HasForeignKey(f => f.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MediaFile>(b =>
        {
            b.HasKey(m => m.MediaFileId);
            b.Property(m => m.FilePath).IsRequired();
            b.HasOne(m => m.Movie)
                .WithMany(mv => mv.MediaFiles)
                .HasForeignKey(m => m.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(m => m.Episode)
                .WithOne(e => e.MediaFile)
                .HasForeignKey<MediaFile>(m => m.EpisodeId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasCheckConstraint("CK_MediaFile_ExactlyOneParent",
                "((\"MovieId\" IS NOT NULL AND \"EpisodeId\" IS NULL) OR (\"MovieId\" IS NULL AND \"EpisodeId\" IS NOT NULL))");
            b.HasIndex(m => m.MovieId).HasFilter("\"MovieId\" IS NOT NULL");
            b.HasIndex(m => m.EpisodeId).HasFilter("\"EpisodeId\" IS NOT NULL");
        });

        modelBuilder.Entity<WatchHistory>(b =>
        {
            b.HasKey(h => h.HistoryId);
            b.HasOne(h => h.Movie)
                .WithMany(m => m.WatchHistoryEntries)
                .HasForeignKey(h => h.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(h => h.Episode)
                .WithMany()
                .HasForeignKey(h => h.EpisodeId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasCheckConstraint("CK_WatchHistory_ExactlyOneTarget",
                "((\"MovieId\" IS NOT NULL AND \"EpisodeId\" IS NULL) OR (\"MovieId\" IS NULL AND \"EpisodeId\" IS NOT NULL))");
            b.HasIndex(h => h.MovieId).HasFilter("\"MovieId\" IS NOT NULL");
            b.HasIndex(h => h.EpisodeId).HasFilter("\"EpisodeId\" IS NOT NULL");
            b.HasIndex(h => new { h.UserId, h.MovieId }).HasFilter("\"MovieId\" IS NOT NULL");
            b.HasIndex(h => new { h.UserId, h.EpisodeId }).HasFilter("\"EpisodeId\" IS NOT NULL");
        });
    }
}