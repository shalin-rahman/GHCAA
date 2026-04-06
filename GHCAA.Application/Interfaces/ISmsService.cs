using System.Threading;
using System.Threading.Tasks;

namespace GHCAA.Application.Interfaces
{
    public interface ISmsService
    {
        /// <summary>
        /// Sends a generic SMS to a single recipient.
        /// </summary>
        Task<bool> SendSmsAsync(string mobileNo, string message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an OTP via SMS for registration or authentication.
        /// </summary>
        Task<bool> SendOtpSmsAsync(string mobileNo, string otpCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an alert notification to a member.
        /// </summary>
        Task<bool> SendAlertSmsAsync(int memberId, string message, CancellationToken cancellationToken = default);
    }
}
