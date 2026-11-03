using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using investAPI.Models;

namespace investAPI.Data;

public partial class InvestmentDbContext : DbContext
{
    public InvestmentDbContext()
    {
    }

    public InvestmentDbContext(DbContextOptions<InvestmentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AssistantLog> AssistantLogs { get; set; }

    public virtual DbSet<Holding> Holdings { get; set; }

    public virtual DbSet<PriceHistory> PriceHistories { get; set; }

    public virtual DbSet<Security> Securities { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WatchlistItem> WatchlistItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=InvestmentDb;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssistantLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Assistan__3214EC07D5733217");

            entity.Property(e => e.RunLabel).HasMaxLength(100);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ToolName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.AssistantLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Assistant__UserI__6C190EBB");
        });

        modelBuilder.Entity<Holding>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Holdings__3214EC07FD8F2F3C");

            entity.HasIndex(e => new { e.UserId, e.SecurityId }, "UQ_Holdings").IsUnique();

            entity.Property(e => e.AvgCostPrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Security).WithMany(p => p.Holdings)
                .HasForeignKey(d => d.SecurityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Holdings__Securi__693CA210");

            entity.HasOne(d => d.User).WithMany(p => p.Holdings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Holdings__UserId__68487DD7");
        });

        modelBuilder.Entity<PriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PriceHis__3214EC075FA344FB");

            entity.ToTable("PriceHistory");

            entity.HasIndex(e => new { e.SecurityId, e.PriceDate }, "UQ_PriceHistory").IsUnique();

            entity.Property(e => e.ClosePrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Security).WithMany(p => p.PriceHistories)
                .HasForeignKey(d => d.SecurityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PriceHist__Secur__70DDC3D8");
        });

        modelBuilder.Entity<Security>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Securiti__3214EC074BA04874");

            entity.HasIndex(e => e.Ticker, "UQ__Securiti__42AC12F0A5C72858").IsUnique();

            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Sector).HasMaxLength(100);
            entity.Property(e => e.Ticker).HasMaxLength(20);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0781EBE41A");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053461732EB7").IsUnique();

            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(256);
        });

        modelBuilder.Entity<WatchlistItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Watchlis__3214EC07D69C3027");

            entity.HasIndex(e => new { e.UserId, e.SecurityId }, "UQ_Watchlist").IsUnique();

            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(d => d.Security).WithMany(p => p.WatchlistItems)
                .HasForeignKey(d => d.SecurityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Watchlist__Secur__6477ECF3");

            entity.HasOne(d => d.User).WithMany(p => p.WatchlistItems)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Watchlist__UserI__6383C8BA");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
