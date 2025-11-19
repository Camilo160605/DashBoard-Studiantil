using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Column> Columns => Set<Column>();
    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Board>(entity =>
        {
            entity.Property(b => b.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasOne(b => b.Owner)
                .WithMany(u => u.Boards)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(b => b.Columns)
                .WithOne(c => c.Board)
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(b => b.Cards)
                .WithOne(c => c.Board)
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Column>(entity =>
        {
            entity.Property(c => c.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(c => c.Position)
                .HasColumnType("numeric(18,4)");

            entity.HasIndex(c => new { c.BoardId, c.Position });

            entity.HasMany(c => c.Cards)
                .WithOne(card => card.Column)
                .HasForeignKey(card => card.ColumnId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Card>(entity =>
        {
            entity.Property(c => c.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(c => c.Position)
                .HasColumnType("numeric(18,4)");

            entity.HasIndex(c => new { c.BoardId, c.ColumnId, c.Position });

            entity.HasOne(c => c.Assignee)
                .WithMany(u => u.AssignedCards)
                .HasForeignKey(c => c.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
