namespace DoctorScheduler.Service

open System
open DoctorScheduler.Domain

type IStoreData =
    abstract member StoreScheduledEvent : scheduledEvent: ScheduledEvent -> unit
    
    abstract member DeleteScheduledEvent: eventId: Guid -> unit

