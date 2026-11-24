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
    public virtual DbSet<InvestmentTransaction> InvestmentTransactions { get; set; }
    public virtual DbSet<PendingTrade> PendingTrades { get; set; }

    public virtual DbSet<PriceHistory> PriceHistories { get; set; }

    public virtual DbSet<Security> Securities { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WatchlistItem> WatchlistItems { get; set; }

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

            entity.Property(e => e.Shares).HasColumnType("decimal(18, 6)");

            entity.HasOne(d => d.Security).WithMany(p => p.Holdings)
                .HasForeignKey(d => d.SecurityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Holdings__Securi__693CA210");

            entity.HasOne(d => d.User).WithMany(p => p.Holdings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Holdings__UserId__68487DD7");
        });

        modelBuilder.Entity<InvestmentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("InvestmentTransactions");

            entity.Property(e => e.TransactionType)
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(e => e.Shares)
                .HasColumnType("decimal(18, 6)");

            entity.Property(e => e.Price)
                .HasColumnType("decimal(12, 2)");

            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(12, 2)");

            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Security)
                .WithMany()
                .HasForeignKey(e => e.SecurityId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PendingTrade>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("PendingTrades");

            entity.Property(e => e.TransactionType)
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.Shares)
                .HasColumnType("decimal(18, 6)");

            entity.Property(e => e.Price)
                .HasColumnType("decimal(12, 2)");

            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(12, 2)");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Security)
                .WithMany()
                .HasForeignKey(e => e.SecurityId)
                .OnDelete(DeleteBehavior.ClientSetNull);
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
            entity.Property(e => e.CashBalance).HasColumnType("decimal(12, 2)").HasDefaultValue(0m);
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
