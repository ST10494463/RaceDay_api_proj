
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
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public CategoriesController(ApplicationDBContext context)
        {
            _context = context;
        }

        // Public: view categories for an event
        [HttpGet("event/{eventId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            var categories = await _context.Categories
                .Where(c => c.EventID == eventId)
                .Select(c => new
                {
                    c.CategoryID,
                    c.EventID,
                    c.CategoryName,
                    c.Description,
                    c.MinimumAge,
                    c.MaximumAge,
                    c.EntryFee
                })
                .ToListAsync();

            return Ok(categories);
        }

        // Organisers only: create a category for their own event
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(
            Category newCategory)
        {
            if (newCategory.CategoryID != 0)
            {
                return BadRequest(new
                {
                    message = "Do not supply a CategoryID."
                });
            }

            if (newCategory.MinimumAge.HasValue &&
                newCategory.MaximumAge.HasValue &&
                newCategory.MinimumAge > newCategory.MaximumAge)
            {
                return BadRequest(new
                {
                    message = "Minimum age cannot exceed maximum age."
                });
            }

            if (newCategory.EntryFee < 0)
            {
                return BadRequest(new
                {
                    message = "Entry fee cannot be negative."
                });
            }

            var organiserValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(organiserValue, out int organiserId))
            {
                return Unauthorized();
            }

            var eventRecord = await _context.Events
                .FirstOrDefaultAsync(e =>
                    e.EventID == newCategory.EventID);

            if (eventRecord == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (eventRecord.OrganizerID != organiserId)
            {
                return Forbid();
            }

            newCategory.CategoryID = 0;

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category created successfully.",
                newCategory.CategoryID
            });
        }


        // Organisers only: update categories for their own events
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(
            int id, Category updatedCategory)
        {
            if (id != updatedCategory.CategoryID)
            {
                return BadRequest(new
                {
                    message = "The category ID in the URL must match the ID in the body."
                });
            }

            if (updatedCategory.MinimumAge.HasValue &&
                updatedCategory.MaximumAge.HasValue &&
                updatedCategory.MinimumAge > updatedCategory.MaximumAge)
            {
                return BadRequest(new
                {
                    message = "Minimum age cannot exceed maximum age."
                });
            }

            if (updatedCategory.EntryFee < 0)
            {
                return BadRequest(new
                {
                    message = "Entry fee cannot be negative."
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
                .FirstOrDefaultAsync(c => c.CategoryID == id);

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

            // Keep the category linked to its original event
            category.CategoryName = updatedCategory.CategoryName;
            category.Description = updatedCategory.Description;
            category.MinimumAge = updatedCategory.MinimumAge;
            category.MaximumAge = updatedCategory.MaximumAge;
            category.EntryFee = updatedCategory.EntryFee;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully."
            });
        }

        // Organisers only: delete categories for their own events
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var organiserValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(organiserValue, out int organiserId))
            {
                return Unauthorized();
            }

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

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

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
