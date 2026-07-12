using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/contact")]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Submit([FromBody] ContactMessageDto dto, CancellationToken cancellationToken)
        {
            await _contactService.SubmitMessageAsync(dto, cancellationToken);
            return Ok(new { Message = "Your enquiry has been filed in the secretariat records." });
        }
    }
}
