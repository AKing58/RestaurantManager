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

        public virtual DbSet<CurrentAvailabilities> CurrentAvailabilities { get; set; }
        public virtual DbSet<CurrentSchedule> CurrentSchedule { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<FurnitureType> FurnitureType { get; set; }
        public virtual DbSet<Item> Item { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<Staff> Staff { get; set; }
        public virtual DbSet<TableInfo> TableInfo { get; set; }
        public virtual DbSet<Title> Title { get; set; }
        public virtual DbSet<Wall> Wall { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=.\\SQLExpress;Data Source=DESKTOP-9UGA0PL;Initial Catalog=COMP4952PROJECT;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurrentAvailabilities>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BlockEndTime).HasColumnType("datetime");

                entity.Property(e => e.BlockStartTime).HasColumnType("datetime");

                entity.Property(e => e.StaffId).HasColumnName("StaffID");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.CurrentAvailabilities)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__CurrentAv__Staff__59FA5E80");
            });

            modelBuilder.Entity<CurrentSchedule>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AvailabilityId).HasColumnName("AvailabilityID");

                entity.Property(e => e.BlockEndTime).HasColumnType("datetime");

                entity.Property(e => e.BlockStartTime).HasColumnType("datetime");

                entity.HasOne(d => d.Availability)
                    .WithMany(p => p.CurrentSchedule)
                    .HasForeignKey(d => d.AvailabilityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__CurrentSc__Avail__5CD6CB2B");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.TableId).HasColumnName("TableID");

                entity.HasOne(d => d.Table)
                    .WithMany(p => p.Customer)
                    .HasForeignKey(d => d.TableId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Customer__TableI__656C112C");
            });

            modelBuilder.Entity<FurnitureType>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Cost).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Orders>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CustId).HasColumnName("CustID");

                entity.Property(e => e.ItemId).HasColumnName("ItemID");

                entity.HasOne(d => d.Cust)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CustId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Orders__CustID__68487DD7");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Orders__ItemID__693CA210");
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
                    .HasConstraintName("FK__Staff__TitleID__571DF1D5");
            });

            modelBuilder.Entity<TableInfo>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.TypeId).HasColumnName("TypeID");

                entity.Property(e => e.Xloc).HasColumnName("XLoc");

                entity.Property(e => e.Yloc).HasColumnName("YLoc");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.TableInfo)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__TableInfo__TypeI__619B8048");
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

            modelBuilder.Entity<Wall>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.X1loc).HasColumnName("X1Loc");

                entity.Property(e => e.X2loc).HasColumnName("X2Loc");

                entity.Property(e => e.Y1loc).HasColumnName("Y1Loc");

                entity.Property(e => e.Y2loc).HasColumnName("Y2Loc");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
