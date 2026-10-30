
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
    [Authorize(Roles = "Participant")]
    public class ParticipantsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ParticipantsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // View the logged-in participant's profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int participantId))
            {
                return Unauthorized();
            }

            var participant = await _context.Participants
                .Where(p => p.ParticipantID == participantId)
                .Select(p => new
                {
                    p.ParticipantID,
                    p.FirstName,
                    p.LastName,
                    p.Email,
                    p.Phone,
                    p.DateOfBirth,
                    p.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (participant == null)
            {
                return NotFound(new
                {
                    message = "Participant profile not found."
                });
            }

            return Ok(participant);
        }

        // Update the logged-in participant's profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            Participant updatedParticipant)
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int participantId))
            {
                return Unauthorized();
            }

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p =>
                    p.ParticipantID == participantId);

            if (participant == null)
            {
                return NotFound(new
                {
                    message = "Participant profile not found."
                });
            }

            if (!string.IsNullOrWhiteSpace(updatedParticipant.Email) &&
                !string.Equals(
                    updatedParticipant.Email,
                    participant.Email,
                    StringComparison.OrdinalIgnoreCase))
            {
                var emailUsed = await _context.Participants
                    .AnyAsync(p =>
                        p.Email == updatedParticipant.Email &&
                        p.ParticipantID != participantId);

                var organiserEmailUsed = await _context.Organizers
                    .AnyAsync(o =>
                        o.Email == updatedParticipant.Email);

                if (emailUsed || organiserEmailUsed)
                {
                    return Conflict(new
                    {
                        message = "This email address is already registered."
                    });
                }

                participant.Email = updatedParticipant.Email;
            }

            participant.FirstName = updatedParticipant.FirstName;
            participant.LastName = updatedParticipant.LastName;
            participant.Phone = updatedParticipant.Phone;
            participant.DateOfBirth = updatedParticipant.DateOfBirth;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully."
            });
        }
    }
}

