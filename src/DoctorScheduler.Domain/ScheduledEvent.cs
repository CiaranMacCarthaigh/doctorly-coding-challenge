using System;
using System.Collections.Generic;

namespace DoctorScheduler.Domain
{
    public class Attendee
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public bool IsAttending { get; set; }
    }
        
    public class ScheduledEvent
    {
        public Guid Id { get; set; } 
        public string Title { get; set; } 
        public string Description { get; set; } 
        public DateTime StartTime { get; set; } 
        public DateTime EndTime { get; set; }
        public List<Attendee> Attendees { get; } = new List<Attendee>();
    }
}
