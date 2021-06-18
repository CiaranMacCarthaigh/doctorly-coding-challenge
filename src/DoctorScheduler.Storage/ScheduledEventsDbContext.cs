using System;
using System.IO;
using DoctorScheduler.Domain;
using Microsoft.EntityFrameworkCore;

namespace DoctorScheduler.Storage
{
    public class ScheduledEventsDbContext : DbContext
    {
        public DbSet<ScheduledEvent> ScheduledEvents { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var databaseFilePath =
                Path.Combine(Path.GetTempPath(), "scheduledEvents.db");

            options.UseSqlite($@"Data Source={databaseFilePath}");
        }
    }
}
