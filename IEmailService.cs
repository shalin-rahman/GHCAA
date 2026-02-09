GHCAA.Application\Interfaces\IEmailService.cs
using System.Threading.Tasks;

namespace GHCAA.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}