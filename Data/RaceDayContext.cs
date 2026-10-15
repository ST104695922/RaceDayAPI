
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Data
{
    public class RaceDayContext : DbContext
    {
        // Constructor receives the database configuration.
        public RaceDayContext(DbContextOptions<RaceDayContext> options)
            : base(options)
        {
        }

        // The seven existing database tables.
        public DbSet<User> Users { get; set; }
        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Result> Results { get; set; }

        // Configure the existing database schema.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USERS table
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS");
                entity.HasKey(u => u.UserId);

                entity.Property(u => u.UserId)
                    .HasColumnName("USER_ID")
                    .HasColumnType("char(5)");

                entity.Property(u => u.FullName)
                    .HasColumnName("FULL_NAME")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasColumnName("EMAIL")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.PasswordHash)
                    .HasColumnName("PASSWORD_HASH")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(u => u.Role)
                    .HasColumnName("ROLE")
                    .HasMaxLength(20)
                    .HasDefaultValue("Participant")
                    .IsRequired();
            });

            // ORGANISER table
            modelBuilder.Entity<Organiser>(entity =>
            {
                entity.ToTable("ORGANISER");
                entity.HasKey(o => o.OrganiserId);

                entity.Property(o => o.OrganiserId)
                    .HasColumnName("ORGANISER_ID")
                    .HasColumnType("char(5)");

                entity.Property(o => o.FullName)
                    .HasColumnName("FULL_NAME")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(o => o.Email)
                    .HasColumnName("EMAIL")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(o => o.Email).IsUnique();

                entity.Property(o => o.PasswordHash)
                    .HasColumnName("PASSWORD_HASH")
                    .HasMaxLength(255)
                    .IsRequired();
            });

            // EVENT table
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("EVENT");
                entity.HasKey(e => e.EventId);

                entity.Property(e => e.EventId)
                    .HasColumnName("EVENT_ID")
                    .HasColumnType("char(5)");

                entity.Property(e => e.EventName)
                    .HasColumnName("EVENT_NAME")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.EventDate)
                    .HasColumnName("EVENT_DATE")
                    .HasColumnType("date");

                entity.Property(e => e.Location)
                    .HasColumnName("LOCATION")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .HasColumnName("STATUS")
                    .HasMaxLength(20)
                    .HasDefaultValue("Upcoming")
                    .IsRequired();

                entity.Property(e => e.OrganiserId)
                    .HasColumnName("ORGANISER_ID")
                    .HasColumnType("char(5)");

                entity.HasOne<Organiser>()
                    .WithMany()
                    .HasForeignKey(e => e.OrganiserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CATEGORY table
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("CATEGORY");
                entity.HasKey(c => c.CategoryId);

                entity.Property(c => c.CategoryId)
                    .HasColumnName("CATEGORY_ID")
                    .HasColumnType("char(5)");

                entity.Property(c => c.EventId)
                    .HasColumnName("EVENT_ID")
                    .HasColumnType("char(5)");

                entity.Property(c => c.CategoryName)
                    .HasColumnName("CATEGORY_NAME")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(c => c.DistanceKm)
                    .HasColumnName("DISTANCE_KM")
                    .HasPrecision(5, 2);

                entity.Property(c => c.EntryFee)
                    .HasColumnName("ENTRY_FEE")
                    .HasPrecision(8, 2)
                    .HasDefaultValue(0);

                entity.HasOne<Event>()
                    .WithMany()
                    .HasForeignKey(c => c.EventId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ENROLMENT table
            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.ToTable("ENROLMENT");
                entity.HasKey(e => e.EnrolmentId);

                entity.Property(e => e.EnrolmentId)
                    .HasColumnName("ENROLMENT_ID")
                    .HasColumnType("char(5)");

                entity.Property(e => e.ParticipantId)
                    .HasColumnName("PARTICIPANT_ID")
                    .HasColumnType("char(5)");

                entity.Property(e => e.CategoryId)
                    .HasColumnName("CATEGORY_ID")
                    .HasColumnType("char(5)");

                entity.Property(e => e.EnrolmentDate)
                    .HasColumnName("ENROLMENT_DATE")
                    .HasColumnType("date")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Status)
                    .HasColumnName("STATUS")
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending")
                    .IsRequired();

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.ParticipantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Category>()
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PAYMENT table
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("PAYMENT");
                entity.HasKey(p => p.PaymentId);

                entity.Property(p => p.PaymentId)
                    .HasColumnName("PAYMENT_ID")
                    .HasColumnType("char(5)");

                entity.Property(p => p.EnrolmentId)
                    .HasColumnName("ENROLMENT_ID")
                    .HasColumnType("char(5)");

                entity.Property(p => p.Amount)
                    .HasColumnName("AMOUNT")
                    .HasPrecision(8, 2);

                entity.Property(p => p.PaymentDate)
                    .HasColumnName("PAYMENT_DATE")
                    .HasColumnType("date")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(p => p.PaymentStatus)
                    .HasColumnName("PAYMENT_STATUS")
                    .HasMaxLength(20)
                    .HasDefaultValue("Pending")
                    .IsRequired();

                entity.Property(p => p.TransactionRef)
                    .HasColumnName("TRANSACTION_REF")
                    .HasMaxLength(50)
                    .IsRequired(false);

                entity.HasOne<Enrolment>()
                    .WithMany()
                    .HasForeignKey(p => p.EnrolmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RESULT table
            modelBuilder.Entity<Result>(entity =>
            {
                entity.ToTable("RESULT");
                entity.HasKey(r => r.ResultId);

                entity.Property(r => r.ResultId)
                    .HasColumnName("RESULT_ID")
                    .HasColumnType("char(5)");

                entity.Property(r => r.EnrolmentId)
                    .HasColumnName("ENROLMENT_ID")
                    .HasColumnType("char(5)");

                entity.HasIndex(r => r.EnrolmentId)
                    .IsUnique();

                entity.Property(r => r.FinishTime)
                    .HasColumnName("FINISH_TIME")
                    .HasColumnType("time");

                entity.Property(r => r.Position)
                    .HasColumnName("POSITION");

                entity.HasOne<Enrolment>()
                    .WithMany()
                    .HasForeignKey(r => r.EnrolmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
