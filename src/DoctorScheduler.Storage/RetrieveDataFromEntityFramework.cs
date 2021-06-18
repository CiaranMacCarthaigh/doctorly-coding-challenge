using System;
using System.Linq;
using DoctorScheduler.Domain;
using DoctorScheduler.Service;

namespace DoctorScheduler.Storage
{
    public class RetrieveDataFromEntityFramework : IRetrieveData
    {
        public ScheduledEvent TryGetScheduledEvent(Guid id)
        {
            using (var db = new ScheduledEventsDbContext())
            {
                return
                    db.ScheduledEvents
                        .FirstOrDefault(scheduledEvent => scheduledEvent.Id == id);
            }
        }
    }
}
