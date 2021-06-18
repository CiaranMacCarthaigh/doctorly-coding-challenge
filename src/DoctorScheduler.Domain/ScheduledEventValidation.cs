namespace DoctorScheduler.Domain
{
    public enum ScheduledEventValidationError
    {
        StartDateIsAfterEndDate
    }
    
    public static class ScheduledEventValidation
    {
        public static ScheduledEventValidationError? IsScheduledEventValid(ScheduledEvent scheduledEvent)
        {
            if (scheduledEvent.StartTime > scheduledEvent.EndTime)
            {
                return ScheduledEventValidationError.StartDateIsAfterEndDate;
            }

            return null;
        }
    }
}
