
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
    public class EventElementsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public EventElementsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // Public: view race elements for a category
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventElements(int categoryId)
        {
            var elements = await _context.EventElements
                .Where(e => e.CategoryID == categoryId)
                .Select(e => new
                {
                    e.EventElementID,
                    e.EventID,
                    e.CategoryID,
                    e.ElementName,
                    e.Distance,
                    e.StartTime,
                    e.Description
                })
                .ToListAsync();

            return Ok(elements);
        }

        // Organisers only: create an event element for their own event
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEventElement(
            EventElement newElement)
        {
            if (newElement.EventElementID != 0)
            {
                return BadRequest(new
                {
                    message = "Do not supply an EventElementID."
                });
            }

            if (newElement.Distance.HasValue &&
                newElement.Distance.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "Distance must be greater than zero."
                });
            }

            var organiserValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(organiserValue, out int organiserId))
            {
                return Unauthorized();
            }

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c =>
                    c.CategoryID == newElement.CategoryID);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            if (category.Event == null ||
                category.Event.OrganizerID != organiserId)
            {
                return Forbid();
            }

            // Ensure the selected event matches the category's event
            if (newElement.EventID != category.EventID)
            {
                return BadRequest(new
                {
                    message = "The event must match the category's event."
                });
            }

            newElement.EventElementID = 0;

            _context.EventElements.Add(newElement);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event element created successfully.",
                newElement.EventElementID
            });
        }

        // Organisers only: update their own event elements
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEventElement(
            int id, EventElement updatedElement)
        {
            if (id != updatedElement.EventElementID)
            {
                return BadRequest(new
                {
                    message = "The event element ID in the URL must match the ID in the body."
                });
            }

            if (updatedElement.Distance.HasValue &&
                updatedElement.Distance.Value <= 0)
            {
                return BadRequest(new
                {
                    message = "Distance must be greater than zero."
                });
            }

            var organiserValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(organiserValue, out int organiserId))
            {
                return Unauthorized();
            }

            var element = await _context.EventElements
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EventElementID == id);

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

            // Keep the element linked to its original event and category
            element.ElementName = updatedElement.ElementName;
            element.Distance = updatedElement.Distance;
            element.StartTime = updatedElement.StartTime;
            element.Description = updatedElement.Description;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event element updated successfully."
            });
        }

        // Organisers only: delete their own event elements
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEventElement(int id)
        {
            var organiserValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(organiserValue, out int organiserId))
            {
                return Unauthorized();
            }

            var element = await _context.EventElements
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EventElementID == id);

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

            _context.EventElements.Remove(element);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
