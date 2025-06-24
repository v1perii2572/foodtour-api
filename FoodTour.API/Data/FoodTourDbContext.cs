using System;
using System.Collections.Generic;
using FoodTour.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace FoodTour.API.Data
{
    public partial class FoodTourDbContext : DbContext
    {
        public FoodTourDbContext()
        {
        }

        public FoodTourDbContext(DbContextOptions<FoodTourDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ChatMessage> ChatMessages { get; set; }
        public virtual DbSet<ChatSession> ChatSessions { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<SavedRoute> SavedRoutes { get; set; }
        public virtual DbSet<SavedRoutePlace> SavedRoutePlaces { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<PostImage> PostImages { get; set; }
        public virtual DbSet<PostComment> PostComments { get; set; }
        public virtual DbSet<PostLike> PostLikes { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__ChatMess__3214EC07A873D6F9");

                entity.Property(e => e.Role)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Timestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");

                entity.HasOne(d => d.Session).WithMany(p => p.ChatMessages)
                    .HasForeignKey(d => d.SessionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChatMessa__Sessi__412EB0B6");
            });

            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__ChatSess__3214EC079EDCB152");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.StartedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValue("InProgress");

                entity.HasOne(d => d.User).WithMany(p => p.ChatSessions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChatSessi__UserI__3D5E1FD2");
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Feedback__3214EC0793A9AD32");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");

                entity.Property(e => e.RelatedPlaceName).HasMaxLength(255);

                entity.HasOne(d => d.Session).WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.SessionId)
                    .HasConstraintName("FK__Feedbacks__Sessi__44FF419A");

                entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Feedbacks__UserI__45F365D3");
            });

            modelBuilder.Entity<SavedRoute>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__SavedRou__3214EC0735231D1C");

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.SavedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");

                entity.HasOne(d => d.Session).WithMany(p => p.SavedRoutes)
                    .HasForeignKey(d => d.SessionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__SavedRout__Sessi__49C3F6B7");

                entity.HasOne(d => d.User).WithMany(p => p.SavedRoutes)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__SavedRout__UserI__4AB81AF0");
            });

            modelBuilder.Entity<SavedRoutePlace>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__SavedRou__3214EC07757FEEB3");

                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.Name).HasMaxLength(255);
                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.TimeSlot)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Route).WithMany(p => p.SavedRoutePlaces)
                    .HasForeignKey(d => d.RouteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__SavedRout__Route__4D94879B");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07BF59CE01");

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.Role)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValue("Free");

                entity.Property(e => e.SubscriptionDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
            });

            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__PaymentT__3214EC072029B684");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");

                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.OrderId).HasMaxLength(100);
                entity.Property(e => e.RequestId).HasMaxLength(100);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PostComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasColumnType("timestamptz");

                entity.HasOne(e => e.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(e => e.PostId);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.PostComments)
                    .HasForeignKey(e => e.UserId);
            });

            modelBuilder.Entity<PostLike>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LikedAt).HasColumnType("timestamptz");

                entity.HasOne(e => e.Post)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(e => e.PostId);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.PostLikes)
                    .HasForeignKey(e => e.UserId);
            });

            modelBuilder.Entity<PostImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);

                entity.HasOne(e => e.Post)
                    .WithMany(p => p.Images)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
