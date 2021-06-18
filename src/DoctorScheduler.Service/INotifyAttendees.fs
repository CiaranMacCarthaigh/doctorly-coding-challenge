namespace DoctorScheduler.Service

open DoctorScheduler.Domain

type INotifyAttendees =
    abstract member NotifyAttendee : attendee: Attendee -> unit
