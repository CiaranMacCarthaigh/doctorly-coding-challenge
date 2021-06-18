namespace DoctorScheduler.Service

open System
open DoctorScheduler.Domain

type IRetrieveData =
    abstract member TryGetScheduledEvent : id: Guid -> ScheduledEvent

