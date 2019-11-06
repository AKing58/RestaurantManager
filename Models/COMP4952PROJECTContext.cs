using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace COMP4952.Models
{
    public partial class COMP4952PROJECTContext : DbContext
    {
        public COMP4952PROJECTContext()
        {
        }

        public COMP4952PROJECTContext(DbContextOptions<COMP4952PROJECTContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CurrentSchedule> CurrentSchedule { get; set; }
        public virtual DbSet<Days> Days { get; set; }
        public virtual DbSet<RegularAvailabilities> RegularAvailabilities { get; set; }
        public virtual DbSet<Staff> Staff { get; set; }
        public virtual DbSet<Title> Title { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=.\\SQLExpress;Data Source=EAMONNALPHIB094;Initial Catalog=COMP4952PROJECT;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurrentSchedule>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Date)
                    .HasColumnName("DATE")
                    .HasColumnType("date");

                entity.Property(e => e.StaffId).HasColumnName("StaffID");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.CurrentSchedule)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__CurrentSc__Staff__4E88ABD4");
            });

            modelBuilder.Entity<Days>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RegularAvailabilities>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DayId).HasColumnName("DayID");

                entity.Property(e => e.StaffId).HasColumnName("StaffID");

                entity.HasOne(d => d.Day)
                    .WithMany(p => p.RegularAvailabilities)
                    .HasForeignKey(d => d.DayId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RegularAv__DayID__5441852A");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.RegularAvailabilities)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__RegularAv__Staff__534D60F1");
            });

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("firstName")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("lastName")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.Rate).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.TitleId).HasColumnName("TitleID");

                entity.HasOne(d => d.Title)
                    .WithMany(p => p.Staff)
                    .HasForeignKey(d => d.TitleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Staff__TitleID__4BAC3F29");
            });

            modelBuilder.Entity<Title>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Title1)
                    .IsRequired()
                    .HasColumnName("Title")
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
