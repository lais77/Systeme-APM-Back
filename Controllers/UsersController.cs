using APM.API.Data;
using APM.API.DTOs.Admin;
using APM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AppDbContext _context;

        public UsersController(UserService userService, AppDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAll() =>
            Ok(await _userService.GetAllAsync());

        [HttpGet("responsables")]
        [Authorize]
        public async Task<IActionResult> GetResponsables()
        {
            var users = await _context.Users
                .Where(u => u.IsActive && u.Role == "RESPONSABLE")
                .OrderBy(u => u.FullName)
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    email = u.Email,
                    role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                var result = await _userService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            var result = await _userService.UpdateAsync(id, dto);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.DeleteAsync(id);
            return result ? Ok(new { message = "Utilisateur désactivé." }) : NotFound();
        }

        [HttpPut("{id}/activer")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Activer(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.IsActive = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Utilisateur activé." });
        }

        [HttpPatch("{id}/desactiver")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Desactiver(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Utilisateur désactivé." });
        }
    }
}
