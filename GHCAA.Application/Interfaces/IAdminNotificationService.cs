namespace GHCAA.Application.Interfaces
{
    public interface IAdminNotificationService
    {
        Task NotifyPendingApprovalAsync(string itemType, string itemTitle, string submitterName, string approvalUrl, CancellationToken cancellationToken = default);
    }
}
