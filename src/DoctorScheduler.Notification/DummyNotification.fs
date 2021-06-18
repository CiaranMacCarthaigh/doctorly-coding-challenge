namespace DoctorScheduler.Notification

open DoctorScheduler.Domain
open DoctorScheduler.Service

type DummyNotification () =
    interface INotifyAttendees with
        member this.NotifyAttendee (attendee: Attendee) =
            printfn $"Notification for: {attendee.Name} ({attendee.EmailAddress})"
