using System.Collections.Generic;
using System.Threading.Tasks;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThemeController : ControllerBase
    {
        private readonly IThemeService _themeService;

        public ThemeController(IThemeService themeService)
        {
            _themeService = themeService;
        }

        [HttpGet("active")]
        public async Task<ActionResult<SpecialDayTheme>> GetActiveTheme()
        {
            var theme = await _themeService.GetActiveThemeAsync();
            if (theme == null) return NoContent();
            return Ok(theme);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("all")]
        public async Task<ActionResult<List<SpecialDayTheme>>> GetAllThemes()
        {
            return Ok(await _themeService.GetAllThemesAsync());
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<ActionResult<SpecialDayTheme>> CreateTheme(SpecialDayTheme theme)
        {
            var created = await _themeService.CreateThemeAsync(theme);
            return CreatedAtAction(nameof(GetAllThemes), new { id = created.Id }, created);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTheme(int id, SpecialDayTheme theme)
        {
            if (id != theme.Id) return BadRequest();
            await _themeService.UpdateThemeAsync(theme);
            return NoContent();
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTheme(int id)
        {
            await _themeService.DeleteThemeAsync(id);
            return NoContent();
        }
    }
}
