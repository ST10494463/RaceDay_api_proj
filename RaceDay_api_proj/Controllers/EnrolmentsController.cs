
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
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public EnrolmentsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // Participants can view their own enrolments
        // Organisers can view enrolments for their own events
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized();
            }

            var query = _context.Enrolments
                .Include(e => e.Event)
                .AsQueryable();

            if (role == "Participant")
            {
                query = query.Where(e => e.ParticipantID == userId);
            }
            else if (role == "Organiser")
            {
                query = query.Where(e =>
                    e.Event != null &&
                    e.Event.OrganizerID == userId);
            }
            else
            {
                return Forbid();
            }

            var enrolments = await query
                .Select(e => new
                {
                    e.EnrolmentID,
                    e.ParticipantID,
                    e.EventID,
                    e.CategoryID,
                    e.EventElementID,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // Participants can enrol themselves in an event
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEnrolment(
            Enrolment newEnrolment)
        {
            if (newEnrolment.EnrolmentID != 0)
            {
                return BadRequest(new
                {
                    message = "Do not supply an EnrolmentID."
                });
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int participantId))
            {
                return Unauthorized();
            }

            var eventRecord = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.EventID == newEnrolment.EventID);

            if (eventRecord == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryID == newEnrolment.CategoryID &&
                    c.EventID == newEnrolment.EventID);

            if (category == null)
            {
                return BadRequest(new
                {
                    message = "The category must belong to the selected event."
                });
            }

            if (newEnrolment.EventElementID.HasValue)
            {
                var element = await _context.EventElements
                    .FirstOrDefaultAsync(e =>
                        e.EventElementID ==
                            newEnrolment.EventElementID.Value &&
                        e.EventID == newEnrolment.EventID &&
                        e.CategoryID == newEnrolment.CategoryID);

                if (element == null)
                {
                    return BadRequest(new
                    {
                        message = "The event element must belong to the selected event and category."
                    });
                }
            }

            // Prevent duplicate enrolments in the same event and category
            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantID == participantId &&
                    e.EventID == newEnrolment.EventID &&
                    e.CategoryID == newEnrolment.CategoryID);

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this event category."
                });
            }

            newEnrolment.EnrolmentID = 0;
            newEnrolment.ParticipantID = participantId;
            newEnrolment.EnrolmentDate = DateTime.Now;
            newEnrolment.Status = "Active";

            _context.Enrolments.Add(newEnrolment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment created successfully.",
                newEnrolment.EnrolmentID
            });
        }

        // Participants can cancel their own enrolments
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int participantId))
            {
                return Unauthorized();
            }

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentID == id &&
                    e.ParticipantID == participantId);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found."
                });
            }

            if (enrolment.Status == "Cancelled")
            {
                return BadRequest(new
                {
                    message = "This enrolment is already cancelled."
                });
            }

            enrolment.Status = "Cancelled";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment cancelled successfully."
            });
        }

        // Organisers can update enrolment status for their own events
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEnrolmentStatus(
            int id, [FromBody] string status)
        {
            var allowedStatuses = new[]
            {
                "Active",
                "Cancelled",
                "Completed"
            };

            if (string.IsNullOrWhiteSpace(status) ||
                !allowedStatuses.Contains(
                    status,
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Status must be Active, Cancelled, or Completed."
                });
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int organiserId))
            {
                return Unauthorized();
            }

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == id);

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found."
                });
            }

            if (enrolment.Event == null ||
                enrolment.Event.OrganizerID != organiserId)
            {
                return Forbid();
            }

            enrolment.Status = allowedStatuses.First(
                s => string.Equals(
                    s, status, StringComparison.OrdinalIgnoreCase));

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment status updated successfully.",
                enrolment.Status
            });
        }
    }
}
