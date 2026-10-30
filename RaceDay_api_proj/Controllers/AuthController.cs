
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay_api_proj.Data;
using RaceDay_api_proj.DTOs;
using RaceDay_api_proj.Models;
namespace RaceDay_api_proj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<Organizer> _organizerHasher = new();
        private readonly PasswordHasher<Participant> _participantHasher = new();

        public AuthController(
            ApplicationDBContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string email = request.Email.Trim();

            if (request.Role.Equals("Organiser",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (await _context.Organizers.AnyAsync(o => o.Email == email)
                    || await _context.Participants.AnyAsync(p => p.Email == email))
                {
                    return Conflict(new { message = "Email already registered." });
                }

                var organiser = new Organizer
                {
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = email,
                    Phone = request.Phone
                };

                organiser.PasswordHash = _organizerHasher.HashPassword(
                    organiser, request.Password);

                _context.Organizers.Add(organiser);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Organiser registered successfully."
                });
            }

            if (request.Role.Equals("Participant",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (await _context.Organizers.AnyAsync(o => o.Email == email)
                    || await _context.Participants.AnyAsync(p => p.Email == email))
                {
                    return Conflict(new { message = "Email already registered." });
                }

                var participant = new Participant
                {
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = email,
                    Phone = request.Phone,
                    DateOfBirth = request.DateOfBirth
                };

                participant.PasswordHash = _participantHasher.HashPassword(
                    participant, request.Password);

                _context.Participants.Add(participant);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Participant registered successfully."
                });
            }

            return BadRequest(new
            {
                message = "Role must be Organiser or Participant."
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string email = request.Email.Trim();

            var organiser = await _context.Organizers
                .SingleOrDefaultAsync(o => o.Email == email);

            if (organiser != null)
            {
                var verification = _organizerHasher.VerifyHashedPassword(
                    organiser, organiser.PasswordHash, request.Password);

                if (verification != PasswordVerificationResult.Failed)
                    return Ok(CreateAuthResponse(
                        organiser.OrganizerID,
                        organiser.Email,
                        "Organiser"));
            }

            var participant = await _context.Participants
                .SingleOrDefaultAsync(p => p.Email == email);

            if (participant != null)
            {
                var verification = _participantHasher.VerifyHashedPassword(
                    participant, participant.PasswordHash, request.Password);

                if (verification != PasswordVerificationResult.Failed)
                    return Ok(CreateAuthResponse(
                        participant.ParticipantID,
                        participant.Email,
                        "Participant"));
            }

            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        private AuthResponse CreateAuthResponse(
            int userId, string email, string role)
        {
            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT key is missing.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                signingKey, SecurityAlgorithms.HmacSha256);

            var expiryMinutes = int.TryParse(
                _configuration["Jwt:ExpiryMinutes"], out var minutes)
                ? minutes : 60;

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials);

            return new AuthResponse
            {
                Message = "Authentication successful.",
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = email,
                Role = role
            };
        }
    }
}
