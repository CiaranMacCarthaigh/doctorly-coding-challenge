using System;
using System.Linq;
using DoctorScheduler.Domain;
using DoctorScheduler.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DoctorScheduler.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IRetrieveData _retrieveEvents;
        private readonly IStoreData _storeEvents;
        private readonly INotifyAttendees _notifyAttendees;
        private readonly ILogger<EventsController> _logger;

        public EventsController(
            IRetrieveData retrieveEvents,
            IStoreData storeEvents,
            INotifyAttendees notifyAttendees,
            ILogger<EventsController> logger)
        {
            _retrieveEvents = retrieveEvents;
            _storeEvents = storeEvents;
            _notifyAttendees = notifyAttendees;
            _logger = logger;
        }

        [HttpGet("{eventId}")]
        public ActionResult Get(Guid eventId)
        {
            var retrievedEvent = _retrieveEvents.TryGetScheduledEvent(eventId);
            if (retrievedEvent == null)
            {
                return NotFound();
            }

            return Ok(retrievedEvent);
        }

        [HttpPost("create")]
        public ActionResult Create([FromBody] ScheduledEvent scheduledEvent)
        {
            var validationError =
                ScheduledEventValidation.IsScheduledEventValid(scheduledEvent);

            if (validationError != null)
            {
                return BadRequest(validationError.Value);
            }
            
            _storeEvents.StoreScheduledEvent(scheduledEvent);

            foreach (var attendee in scheduledEvent.Attendees.Where(attendee => attendee.IsAttending))
            {
                try
                {
                    _notifyAttendees.NotifyAttendee(attendee);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error when notifying attendee");
                }
            }
            
            return Ok();
        }

        [HttpPost("{eventId}/cancel")]
        public ActionResult Cancel(Guid eventId)
        {
            var retrievedEvent = _retrieveEvents.TryGetScheduledEvent(eventId);
            if (retrievedEvent == null)
            {
                return Ok();
            }
            
            _storeEvents.DeleteScheduledEvent(eventId);

            foreach (var attendee in retrievedEvent.Attendees.Where(attendee => attendee.IsAttending))
            {
                try
                {
                    _notifyAttendees.NotifyAttendee(attendee);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error when notifying attendee");
                }
            }
            
            return Ok();
        }
    }
}
