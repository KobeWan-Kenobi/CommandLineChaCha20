using System;
using System.Collections.Generic;
using FileEncryptionWebApp.DataAccess.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace FileEncryptionWebApp.DataAccess.EF.Context;

public partial class FileEncryptionProjectDatabaseContext : DbContext
{
    public FileEncryptionProjectDatabaseContext()
    {
    }

    public FileEncryptionProjectDatabaseContext(DbContextOptions<FileEncryptionProjectDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EncryptionKey> EncryptionKeys { get; set; }

    public virtual DbSet<Entry> Entries { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=KOBEEWORK\\SQLEXPRESS;Database=FileEncryptionProjectDatabase;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EncryptionKey>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EncryptionKey1)
                .HasMaxLength(32)
                .IsFixedLength()
                .HasColumnName("EncryptionKey");
            entity.Property(e => e.Nonce)
                .HasMaxLength(4)
                .IsFixedLength();
        });

        modelBuilder.Entity<Entry>(entity =>
        {
            entity.HasKey(e => e.EntryId).HasName("PK_Files");

            entity.Property(e => e.CreationDate).HasColumnType("datetime");
            entity.Property(e => e.Entry1)
                .HasMaxLength(255)
                .HasColumnName("Entry");

            entity.HasOne(d => d.EncryptionKey).WithMany(p => p.Entries)
                .HasForeignKey(d => d.EncryptionKeyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Entries_EncryptionKeys");

            entity.HasOne(d => d.User).WithMany(p => p.Entries)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Entries_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Password)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Username)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.HasOne(d => d.EncryptionKey).WithMany(p => p.Users)
                .HasForeignKey(d => d.EncryptionKeyId)
                .HasConstraintName("FK_Users_EncryptionKeys");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
