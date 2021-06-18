using System;
using System.Linq;
using DoctorScheduler.Domain;
using DoctorScheduler.Service;

namespace DoctorScheduler.Storage
{
    public class StoreDataInEntityFramework : IStoreData
    {
        public void StoreScheduledEvent(ScheduledEvent scheduledEvent)
        {
            using (var db = new ScheduledEventsDbContext())
            {
                db.ScheduledEvents.Add(scheduledEvent);
                db.SaveChanges();
            }
        }

        public void DeleteScheduledEvent(Guid eventId)
        {
            using (var db = new ScheduledEventsDbContext())
            {
                var existingEvent = db.ScheduledEvents.FirstOrDefault(scheduledEvent => scheduledEvent.Id == eventId);
                if (existingEvent == null)
                {
                    return;
                }

                db.ScheduledEvents.Remove(existingEvent);
                db.SaveChanges();
            }
        }
    }
}
