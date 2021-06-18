# Doctor Scheduler

A simple API to scheduling events and notifying attendees about the new event.

## Requirements
- .NET 5 (available from https://dotnet.microsoft.com/)

## Build and running

The main application is `DoctorScheduler.Application`. By default, this starts listening on http://localhost:5000 and https://localhost:5001 

### From the CLI
From the solution folder, run:
```
dotnet run --project ./DoctorScheduler.Application/DoctorScheduler.Application.csproj
```

### From the IDE of your choice
Open the solution, build, and run the `DoctorScheduler.Application` project.

After the application has built and started, you can navigate to http://localhost:5000 to view an OpenAPI file for the API.

## Application structure

There are five main projects in the solution.

### Domain
Contains types necessary for the application. Contains minimal logic only for these types. At the moment it only has the ScheduledEvent, and the Attendee.
### Service
Defines interfaces used by the application. No implementation logic should be here: services that implement these interfaces should exist only in other libraries (e.g. Storage, Notification).
### Storage
Implements an storage layer using the Entity Framework and SqlLite.
### Notification
Implements an interface for 'notifications'. In reality, it just prints to the Console. To add a new notification provider, implement `INotifyAttendees` from Service.
### Application
The main application. This is an ASP .NET 5 server and provides three API endpoints for events.

## Tests

Currently only application tests exist, calling some of the endpoints through a test server. In a normal situation I would have added unit tests for the domain validation.

I didn't have time to test the `cancel` endpoint, only GET and the `create` endpoint.
