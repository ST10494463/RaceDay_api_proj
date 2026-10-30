
using Microsoft.EntityFrameworkCore;
using RaceDay_api_proj.Models;

namespace RaceDay_api_proj.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        public DbSet<Organizer> Organizers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<EventElement> EventElements { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventElement>()
                .Property(e => e.Distance)
                .HasPrecision(6, 2);

            modelBuilder.Entity<Organizer>()
                .HasIndex(o => o.Email)
                .IsUnique();

            modelBuilder.Entity<Participant>()
                .HasIndex(p => p.Email)
                .IsUnique();

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany()
                .HasForeignKey(e => e.OrganizerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany()
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventElement>()
                .HasOne(ee => ee.Event)
                .WithMany(e => e.EventElements)
                .HasForeignKey(ee => ee.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventElement>()
                .HasOne(ee => ee.Category)
                .WithMany(c => c.EventElements)
                .HasForeignKey(ee => ee.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Participant)
                .WithMany()
                .HasForeignKey(en => en.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Event)
                .WithMany()
                .HasForeignKey(en => en.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Category)
                .WithMany()
                .HasForeignKey(en => en.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.EventElement)
                .WithMany()
                .HasForeignKey(en => en.EventElementID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Participant)
                .WithMany()
                .HasForeignKey(r => r.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.EventElement)
                .WithMany()
                .HasForeignKey(r => r.EventElementID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

