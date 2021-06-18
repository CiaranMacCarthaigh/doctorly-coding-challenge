module EventsController.Tests

open System
open System.Collections.Generic
open System.ComponentModel
open System.Net
open System.Net.Http
open System.Text
open DoctorScheduler.Domain
open DoctorScheduler.Service
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Mvc;

open DoctorScheduler.Application
open NUnit.Framework
open DoctorScheduler.Tests.Helpers.Assertions
open Newtonsoft.Json

open Microsoft.Extensions.DependencyInjection
open Newtonsoft.Json

let nonExistentScheduledEventId = "a01e95be-7218-4030-ae62-22237497bd70" |> Guid
let scheduledEventId = "3e024a14-7d85-4db5-9832-d36e2522fcd9" |> Guid

let attendeeThatIsAttending =
    Attendee(
        Id = ("c1e18b6d-14ad-4c54-81fc-e79ed97083d8" |> Guid),
        Name = "John Douglas",
        EmailAddress = "JohnDouglas@acme.com",
        IsAttending = true
        )
let attendeeThatIsNotAttending =
    Attendee(
        Id = ("dddddddd-dddd-dddd-dddd-dddddddddddd" |> Guid),
        Name = "Dougal Johnny",
        EmailAddress = "dougalj@yahoo.com",
        IsAttending = false
        )

let scheduledEvent =
    let event =
        ScheduledEvent(
            Id = scheduledEventId,
            Title = "Generic event title",
            StartTime = DateTime(2021, 06, 18, 14, 00, 00),
            EndTime = DateTime(2021, 06, 18, 17, 30, 00)
        )
        
        
    [|
        attendeeThatIsAttending
        attendeeThatIsNotAttending
    |]
    |> event.Attendees.AddRange
    
    event

let defaultEventRetrieval =
    {
        new IRetrieveData with
            member this.TryGetScheduledEvent(eventId) =
                [ scheduledEvent.Id, scheduledEvent ]
                |> Map.ofList
                |> Map.tryFind eventId
                |> function
                    | None -> null
                    | Some value -> value
    }

let defaultEventStorage =
    {
        new IStoreData with
            member this.StoreScheduledEvent(event) = ()
            member this.DeleteScheduledEvent(id) = ()
    }
    
let defaultNotification =
    {
        new INotifyAttendees with
            member this.NotifyAttendee _ = ()
    }

let createTestServerWith (eventRetrieval: IRetrieveData) (eventStorage: IStoreData) (notification: INotifyAttendees) =
    WebHostBuilder()
        .ConfigureTestServices(fun services ->
            services.AddSingleton<IRetrieveData> eventRetrieval
            services.AddSingleton<IStoreData> eventStorage
            services.AddSingleton<INotifyAttendees> notification
            ())
//        .UseConfiguration(ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build())
        .UseStartup<Startup>()
    |> fun builder -> new TestServer(builder)

let shouldEqualAttendees (expected: Attendee seq) (actual: Attendee seq) =
    let actualIds = actual |> Seq.map (fun attendee -> attendee.Id)
    let expectedIds = expected |> Seq.map (fun attendee -> attendee.Id)
    actualIds |> shouldEqual expectedIds

let shouldEqualEvent (expected: ScheduledEvent) (actual: ScheduledEvent) =
    actual.Id |> shouldEqual expected.Id
    actual.Title |> shouldEqual expected.Title
    actual.StartTime |> shouldEqual expected.StartTime
    actual.EndTime |> shouldEqual expected.EndTime
    actual.Attendees |> shouldEqualAttendees expected.Attendees

let createHttpClientFrom (testServer: TestServer) = testServer.CreateClient()

[<Test>]
let ``When retrieving a scheduled event with a specified ID, and the a scheduled event exists, return the scheduled event`` () =
    let httpClient =
        createTestServerWith defaultEventRetrieval defaultEventStorage defaultNotification
        |> createHttpClientFrom
    
    let response =
        $"/api/events/{scheduledEventId}"
        |> httpClient.GetAsync
        |> Async.AwaitTask
        |> Async.RunSynchronously
    
    response.StatusCode |> shouldEqual (HttpStatusCode.OK)
    response.Content.Headers.ContentType.MediaType |> shouldEqual "application/json"
    let actualEvent =
        response.Content.ReadAsStringAsync()
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> JsonConvert.DeserializeObject<ScheduledEvent>
        
    actualEvent |> shouldEqualEvent scheduledEvent

[<Test>]
let ``When retrieving a scheduled event with a specified ID, and the a scheduled event does not exist, return Not Found`` () =
    let httpClient =
        createTestServerWith defaultEventRetrieval defaultEventStorage defaultNotification
        |> createHttpClientFrom
        
    let response =
        $"/api/events/{nonExistentScheduledEventId}"
        |> httpClient.GetAsync
        |> Async.AwaitTask
        |> Async.RunSynchronously
    
    response.StatusCode |> shouldEqual (HttpStatusCode.NotFound)

[<Test>]
let ``When creating a new event, store the event, notify all attending attendees, and return 200`` () =
    let mutable storedEvents = []
    let eventStorage =
        {
            new IStoreData with
                member this.StoreScheduledEvent(event) =
                    storedEvents <- event :: storedEvents
                member this.DeleteScheduledEvent(id) =
                    storedEvents <- storedEvents |> List.where (fun event -> event.Id <> id)
        }
        
    let mutable notifiedAttendees = []
    let notificationService =
        {
            new INotifyAttendees with
                member this.NotifyAttendee (attendee: Attendee) =
                    notifiedAttendees <- attendee :: notifiedAttendees
        }
    
    let httpClient =
        createTestServerWith defaultEventRetrieval eventStorage notificationService
        |> createHttpClientFrom
    
    let postRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/create")
    postRequest.Content <- new StringContent(
        JsonConvert.SerializeObject(scheduledEvent),
        Encoding.UTF8,
        "application/json")
    
    let response =
        httpClient.SendAsync postRequest |> Async.AwaitTask |> Async.RunSynchronously
    
    response.StatusCode |> shouldEqual (HttpStatusCode.OK)
    
    storedEvents
    |> List.tryHead
    |> function
        | None -> Assert.Fail("Expected a new scheduled event to be saved, but none was.")
        | Some actualEvent -> actualEvent |> shouldEqualEvent scheduledEvent
        
    notifiedAttendees
    |> shouldEqualAttendees [ attendeeThatIsAttending ]
    
[<Test>]
let ``When creating a new event, but the event's start date is later than the end date, return 400`` () =
    let mutable storedEvents = []
    let eventStorage =
        {
            new IStoreData with
                member this.StoreScheduledEvent(event) =
                    storedEvents <- event :: storedEvents
                member this.DeleteScheduledEvent(id) =
                    storedEvents <- storedEvents |> List.where (fun event -> event.Id <> id)
        }    
        
    let mutable notifiedAttendees = []
    let notificationService =
        {
            new INotifyAttendees with
                member this.NotifyAttendee (attendee: Attendee) =
                    notifiedAttendees <- attendee :: notifiedAttendees
        }
    
    let httpClient =
        createTestServerWith defaultEventRetrieval eventStorage notificationService
        |> createHttpClientFrom
    
    let badScheduledEvent =
        ScheduledEvent(
            Id = scheduledEventId,
            Title = "Generic event title",
            StartTime = DateTime(2022, 12, 31, 14, 00, 00),
            EndTime = DateTime(2021, 06, 18, 17, 30, 00)
        )
    
    let postRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/events/create")
    postRequest.Content <- new StringContent(
        JsonConvert.SerializeObject(badScheduledEvent),
        Encoding.UTF8,
        "application/json")
    
    let response =
        httpClient.SendAsync postRequest |> Async.AwaitTask |> Async.RunSynchronously
    
    response.StatusCode |> shouldEqual (HttpStatusCode.BadRequest)
    
    Assert.IsEmpty storedEvents
    Assert.IsEmpty notifiedAttendees
