
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay_api_proj.Data;
using RaceDay_api_proj.Models;
using System.Security.Claims;

namespace RaceDay_api_proj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public EventsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // Public: anyone can view events
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.Status,
                    e.Distance,
                    e.EventType
                })
                .ToListAsync();

            return Ok(events);
        }

        // Organisers only: create an event
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent(Event newEvent)
        {
            if (newEvent.EventID != 0)
            {
                return BadRequest(new
                {
                    message = "Do not supply an EventID when creating an event."
                });
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int organiserId))
            {
                return Unauthorized();
            }

            newEvent.OrganizerID = organiserId;
            newEvent.EventID = 0;

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvents),
                new { id = newEvent.EventID },
                newEvent);
        }

        // Organisers only: update their own events
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(
            int id, Event updatedEvent)
        {
            if (id != updatedEvent.EventID)
            {
                return BadRequest(new
                {
                    message = "The event ID in the URL must match the event ID in the body."
                });
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int organiserId))
            {
                return Unauthorized();
            }

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (existingEvent.OrganizerID != organiserId)
            {
                return Forbid();
            }

            existingEvent.EventName = updatedEvent.EventName;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.EventDate = updatedEvent.EventDate;
            existingEvent.Location = updatedEvent.Location;
            existingEvent.Status = updatedEvent.Status;
            existingEvent.Distance = updatedEvent.Distance;
            existingEvent.EventType = updatedEvent.EventType;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Event updated successfully." });
        }

        // Organisers only: delete their own events
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int organiserId))
            {
                return Unauthorized();
            }

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (existingEvent.OrganizerID != organiserId)
            {
                return Forbid();
            }

            _context.Events.Remove(existingEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}