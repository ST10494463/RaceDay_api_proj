
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
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ResultsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // Participants view their own results.
        // Organisers view results for their own events.
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Unauthorized();
            }

            var query = _context.Results
                .Include(r => r.EventElement)
                .AsQueryable();

            if (role == "Participant")
            {
                query = query.Where(r => r.ParticipantID == userId);
            }
            else if (role == "Organiser")
            {
                query = query.Where(r =>
                    r.EventElement != null &&
                    r.EventElement.EventID != 0);

                query = query.Where(r =>
                    _context.Events.Any(e =>
                        e.EventID == r.EventElement!.EventID &&
                        e.OrganizerID == userId));
            }
            else
            {
                return Forbid();
            }

            var results = await query
                .Select(r => new
                {
                    r.ResultID,
                    r.ParticipantID,
                    r.EventElementID,
                    r.Position,
                    r.FinishTime,
                    r.ResultStatus
                })
                .ToListAsync();

            return Ok(results);
        }

        // Organisers record results for their own events.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(Result newResult)
        {
            if (newResult.ResultID != 0)
            {
                return BadRequest(new
                {
                    message = "Do not supply a ResultID."
                });
            }

            if (newResult.Position.HasValue &&
                newResult.Position.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "Position must be greater than zero."
                });
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int organiserId))
            {
                return Unauthorized();
            }

            var element = await _context.EventElements
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e =>
                    e.EventElementID == newResult.EventElementID);

            if (element == null)
            {
                return NotFound(new
                {
                    message = "Event element not found."
                });
            }

            if (element.Event == null ||
                element.Event.OrganizerID != organiserId)
            {
                return Forbid();
            }

            // The participant must be enrolled in this event element.
            var enrolmentExists = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantID == newResult.ParticipantID &&
                    e.EventID == element.EventID &&
                    e.CategoryID == element.CategoryID &&
                    e.EventElementID == element.EventElementID &&
                    e.Status == "Active");

            if (!enrolmentExists)
            {
                return BadRequest(new
                {
                    message = "The participant must have an active enrolment in this event element."
                });
            }

            var resultExists = await _context.Results
                .AnyAsync(r =>
                    r.ParticipantID == newResult.ParticipantID &&
                    r.EventElementID == newResult.EventElementID);

            if (resultExists)
            {
                return Conflict(new
                {
                    message = "A result already exists for this participant and event element."
                });
            }

            newResult.ResultID = 0;
            newResult.ResultStatus = "Completed";

            _context.Results.Add(newResult);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result recorded successfully.",
                newResult.ResultID
            });
        }

        // Organisers can update results for their own events
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(
            int id, Result updatedResult)
        {
            if (id != updatedResult.ResultID)
            {
                return BadRequest(new
                {
                    message = "The result ID in the URL must match the ID in the body."
                });
            }

            if (updatedResult.Position.HasValue &&
                updatedResult.Position.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "Position must be greater than zero."
                });
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int organiserId))
            {
                return Unauthorized();
            }

            var result = await _context.Results
                .Include(r => r.EventElement)
                    .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result not found."
                });
            }

            if (result.EventElement?.Event == null ||
                result.EventElement.Event.OrganizerID != organiserId)
            {
                return Forbid();
            }

            // Keep the result linked to the same participant and event element
            result.Position = updatedResult.Position;
            result.FinishTime = updatedResult.FinishTime;
            result.ResultStatus = updatedResult.ResultStatus;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result updated successfully."
            });
        }
    }
}
