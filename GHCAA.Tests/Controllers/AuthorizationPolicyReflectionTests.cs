using System.Reflection;
using GHCAA.API.Filters;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    // 84.44: reads every controller action's effective policy and step-up flag via reflection
    // and compares it with the expected list below, taken from
    // docs/specs/002-workflow-contracts-and-validation/evidence/authorization-catalog.md.
    // Changing a policy then means changing this list on purpose.
    [TestFixture]
    public class AuthorizationPolicyReflectionTests
    {
        private sealed record ExpectedAuth(string? Policy, bool AllowAnonymous, bool StepUp);

        // (Controller, Action) -> (expected policy, expected [AllowAnonymous], expected [RequireStepUp])
        private static readonly Dictionary<(string Controller, string Action), ExpectedAuth> Expected = new()
        {
            [("ActivityController", "GetGlobalActivity")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("ActivityController", "GetMemberActivity")] = new(Constants.Policies.AdminOnly, false, false),
            [("ActivityController", "GetMyActivity")] = new(null, false, false),
            [("AdminController", "ApproveMember")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "ArchiveMember")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("AdminController", "BulkArchiveInactive")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("AdminController", "DeleteContactMessage")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "GetAllMembers")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "GetAnalytics")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "GetContactMessages")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "GetDocuments")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "GetMemberById")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "GetMemberCertificate")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "GetMemberCertificatePdf")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "GetMemberIDCard")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "GetMemberIDCardPdf")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "GetStats")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "MarkMessageAsRead")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "ReactivateMember")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "RejectMember")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "ResetPasswordAdmin")] = new(Constants.Policies.AdminOnly, false, true),
            [("AdminController", "RestoreMember")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("AdminController", "RevertMemberApproval")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "SyncMembers")] = new(Constants.Policies.AdminOnly, false, true),
            [("AdminController", "UpdateMemberAdmin")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "UpdateMemberDocuments")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "UpdateMemberPhoto")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminController", "UpdateMemberSignature")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminDevTrackerController", "GetOpenItems")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("AdminElectionsController", "AddCandidate")] = new(Constants.Policies.ElectionStaff, false, true),
            [("AdminElectionsController", "Close")] = new(Constants.Policies.ElectionStaff, false, true),
            [("AdminElectionsController", "Create")] = new(Constants.Policies.AdminOnly, false, true),
            [("AdminElectionsController", "List")] = new(Constants.Policies.ElectionStaff, false, true),
            [("AdminElectionsController", "Publish")] = new(Constants.Policies.ElectionStaff, false, true),
            [("AdminElectionsController", "RemoveCandidate")] = new(Constants.Policies.ElectionStaff, false, true),
            [("AdminElectionsController", "SetBallotKey")] = new(Constants.Policies.ElectionStaff, false, true),
            [("AdminErrorLogsController", "GetErrorLogs")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("AdminGovernanceController", "ActivatePeriod")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminGovernanceController", "AssignMember")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminGovernanceController", "CreatePeriod")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminGovernanceController", "DeleteECMember")] = new(Constants.Policies.AdminOnly, false, true),
            [("AdminGovernanceController", "GetCommitteeMembers")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminGovernanceController", "GetPeriods")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminGovernanceController", "RemoveMember")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminGovernanceController", "UpdatePeriod")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminPollController", "CreatePoll")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminPollController", "DeletePoll")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminPollController", "GetAllPolls")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminPollController", "ToggleStatus")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminSocialAuthController", "GetConfigs")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminSocialAuthController", "Toggle")] = new(Constants.Policies.AdminOnly, false, false),
            [("AdminSocialAuthController", "UpdateConfig")] = new(Constants.Policies.AdminOnly, false, false),
            [("ArchiveController", "AdminCollections")] = new(Constants.Policies.AdminOnly, false, false),
            [("ArchiveController", "AdminItems")] = new(Constants.Policies.AdminOnly, false, false),
            [("ArchiveController", "CreateCollection")] = new(Constants.Policies.AdminOnly, false, false),
            [("ArchiveController", "CreateItem")] = new(null, false, false),
            [("ArchiveController", "DeleteCollection")] = new(Constants.Policies.AdminOnly, false, false),
            [("ArchiveController", "DeleteItem")] = new(Constants.Policies.AdminOnly, false, false),
            [("ArchiveController", "ModerateItem")] = new(Constants.Policies.AdminOnly, false, false),
            [("ArchiveController", "PublicCollections")] = new(null, true, false),
            [("ArchiveController", "PublicItem")] = new(null, true, false),
            [("ArchiveController", "UpdateCollection")] = new(Constants.Policies.AdminOnly, false, false),
            [("ArchiveController", "UpdateItem")] = new(Constants.Policies.AdminOnly, false, false),
            [("AssistantController", "Ask")] = new(null, false, false),
            [("AuthController", "FacebookLogin")] = new(null, true, false),
            [("AuthController", "ForgotPassword")] = new(null, true, false),
            [("AuthController", "GetProviders")] = new(null, true, false),
            [("AuthController", "GoogleLogin")] = new(null, true, false),
            [("AuthController", "Login")] = new(null, true, false),
            [("AuthController", "Logout")] = new(null, false, false),
            [("AuthController", "Me")] = new(null, false, false),
            [("AuthController", "Refresh")] = new(null, true, false),
            [("AuthController", "RefreshMobile")] = new(null, true, false),
            [("AuthController", "RequestStepUp")] = new(null, false, false),
            [("AuthController", "ResetPassword")] = new(null, true, false),
            [("AuthController", "VerifyStepUp")] = new(null, false, false),
            [("CampaignsController", "ConfirmReceipt")] = new(Constants.Policies.AdminOnly, false, false),
            [("CampaignsController", "Create")] = new(Constants.Policies.AdminOnly, false, false),
            [("CampaignsController", "CreatePledge")] = new(null, true, false),
            [("CampaignsController", "CreateTier")] = new(Constants.Policies.AdminOnly, false, false),
            [("CampaignsController", "GetAllForAdmin")] = new(Constants.Policies.AdminOnly, false, false),
            [("CampaignsController", "GetBySlug")] = new(null, true, false),
            [("CampaignsController", "GetHonourRoll")] = new(null, true, false),
            [("CampaignsController", "GetMyPledges")] = new(null, false, false),
            [("CampaignsController", "GetPledgesForAdmin")] = new(Constants.Policies.AdminOnly, false, false),
            [("CampaignsController", "GetPublicCampaigns")] = new(null, true, false),
            [("CampaignsController", "GetTiers")] = new(Constants.Policies.AdminOnly, false, false),
            [("CampaignsController", "Update")] = new(Constants.Policies.AdminOnly, false, false),
            [("CommunicationController", "CreateTemplate")] = new(Constants.Policies.AdminOnly, false, false),
            [("CommunicationController", "DeleteTemplate")] = new(Constants.Policies.AdminOnly, false, false),
            [("CommunicationController", "GetLogs")] = new(Constants.Policies.AdminOnly, false, false),
            [("CommunicationController", "GetMemberLogs")] = new(Constants.Policies.AdminOnly, false, false),
            [("CommunicationController", "GetTemplates")] = new(Constants.Policies.AdminOnly, false, false),
            [("CommunicationController", "SendBatch")] = new(Constants.Policies.AdminOnly, false, false),
            [("CommunicationController", "SendCustom")] = new(Constants.Policies.AdminOnly, false, false),
            [("CommunicationController", "SendType")] = new(Constants.Policies.AdminOnly, false, false),
            [("CommunicationController", "UpdateTemplate")] = new(Constants.Policies.AdminOnly, false, false),
            [("ContactController", "Submit")] = new(null, true, false),
            [("CredentialVerificationController", "Revoke")] = new(Constants.Policies.AdminOnly, false, false),
            [("CredentialVerificationController", "Verify")] = new(null, true, false),
            [("ElectionAppointmentsController", "Accept")] = new(null, false, true),
            [("ElectionAppointmentsController", "Appoint")] = new(Constants.Policies.ElectionStaff, false, true),
            [("ElectionAppointmentsController", "Decline")] = new(null, false, false),
            [("ElectionAppointmentsController", "List")] = new(Constants.Policies.ElectionStaff, false, false),
            [("ElectionAppointmentsController", "Mine")] = new(null, false, false),
            [("ElectionAppointmentsController", "Revoke")] = new(Constants.Policies.ElectionStaff, false, true),
            [("ElectionPersonasController", "Create")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("ElectionPersonasController", "Delete")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("ElectionPersonasController", "List")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("ElectionPersonasController", "SetActive")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("ElectionPersonasController", "Update")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("ElectionPersonasReadController", "List")] = new(null, false, false),
            [("ElectionsController", "AddSeat")] = new(Constants.Policies.ElectionStaff, false, true),
            [("ElectionsController", "Count")] = new(Constants.Policies.ElectionStaff, false, true),
            [("ElectionsController", "Create")] = new(Constants.Policies.AdminOnly, false, true),
            [("ElectionsController", "Declare")] = new(Constants.Policies.ElectionStaff, false, true),
            [("ElectionsController", "Document")] = new(null, true, false),
            [("ElectionsController", "FreezeRoll")] = new(Constants.Policies.ElectionStaff, false, true),
            [("ElectionsController", "Get")] = new(null, true, false),
            [("ElectionsController", "GetCurrent")] = new(null, true, false),
            [("ElectionsController", "Nominate")] = new(null, false, false),
            [("ElectionsController", "Nominations")] = new(null, true, false),
            [("ElectionsController", "Scrutinise")] = new(Constants.Policies.ElectionStaff, false, true),
            [("ElectionsController", "SetPhase")] = new(Constants.Policies.ElectionStaff, false, true),
            [("ElectionsController", "Vote")] = new(null, false, true),
            [("ElectionsController", "Withdraw")] = new(null, false, false),
            [("EventsController", "AddExpense")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "ApproveRegistration")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "CreateEvent")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "CreateTask")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "DeleteEvent")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "DeleteExpense")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "DeleteTask")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "GetActiveEvents")] = new(null, true, false),
            [("EventsController", "GetAllEventsForAdmin")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "GetAllRegistrations")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "GetEventBudget")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "GetEventById")] = new(null, true, false),
            [("EventsController", "GetEventTasks")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "GetMyRegistrations")] = new(null, false, false),
            [("EventsController", "GetPublicParticipants")] = new(null, true, false),
            [("EventsController", "GetRegistrationForInvitation")] = new(null, false, false),
            [("EventsController", "QRCodeCheckIn")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "RegisterForEventForm")] = new(null, true, false),
            [("EventsController", "RegisterForEventJson")] = new(null, true, false),
            [("EventsController", "SendInvitation")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "ToggleTask")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "UpdateBudget")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "UpdateEvent")] = new(Constants.Policies.AdminOnly, false, false),
            [("EventsController", "UploadEventLogo")] = new(Constants.Policies.AdminOnly, false, false),
            [("FamilyLinkController", "Cancel")] = new(null, false, false),
            [("FamilyLinkController", "GetFamily")] = new(null, false, false),
            [("FamilyLinkController", "GetPublicFamily")] = new(null, false, false),
            [("FamilyLinkController", "GetReceived")] = new(null, false, false),
            [("FamilyLinkController", "GetSent")] = new(null, false, false),
            [("FamilyLinkController", "Remove")] = new(null, false, false),
            [("FamilyLinkController", "Respond")] = new(null, false, false),
            [("FamilyLinkController", "Search")] = new(null, false, false),
            [("FamilyLinkController", "Send")] = new(null, false, false),
            [("FinancialLedgerController", "AddRecord")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("FinancialLedgerController", "DeleteRecord")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("FinancialLedgerController", "ExportCsv")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("FinancialLedgerController", "GetRecords")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("FinancialLedgerController", "GetSummary")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("FinancialLedgerController", "UpdateRecord")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("FinancialsController", "AddFeeConfig")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("FinancialsController", "AddSavedPaymentMethod")] = new(null, false, false),
            [("FinancialsController", "DeletePayment")] = new(Constants.Policies.AdminOnly, false, false),
            [("FinancialsController", "DeleteSavedPaymentMethod")] = new(null, false, false),
            [("FinancialsController", "DownloadReceipt")] = new(null, false, false),
            [("FinancialsController", "GenerateAnnualDues")] = new(Constants.Policies.AdminOnly, false, false),
            [("FinancialsController", "GetApplicableFee")] = new(null, true, false),
            [("FinancialsController", "GetFeeConfigs")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("FinancialsController", "GetMemberPaymentHistory")] = new(Constants.Policies.AdminOnly, false, false),
            [("FinancialsController", "GetMembershipHistory")] = new(null, false, false),
            [("FinancialsController", "GetMyDues")] = new(null, false, false),
            [("FinancialsController", "GetMyPaymentHistory")] = new(null, false, false),
            [("FinancialsController", "GetSavedPaymentMethods")] = new(null, false, false),
            [("FinancialsController", "RecordPayment")] = new(null, false, false),
            [("FinancialsController", "UpdateFeeConfig")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("FinancialsController", "UpdateStatus")] = new(Constants.Policies.AdminOnly, false, false),
            [("ForumController", "CreatePost")] = new(null, false, false),
            [("ForumController", "CreateTopic")] = new(null, false, false),
            [("ForumController", "DeletePost")] = new(null, false, false),
            [("ForumController", "DeleteTopic")] = new(null, false, false),
            [("ForumController", "GetCategories")] = new(null, false, false),
            [("ForumController", "GetPosts")] = new(null, false, false),
            [("ForumController", "GetTopic")] = new(null, false, false),
            [("ForumController", "GetTopics")] = new(null, false, false),
            [("GalleryController", "AddPhotoToAlbum")] = new(Constants.Policies.MemberOnly, false, false),
            [("GalleryController", "AddPhotos")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "ApproveGallery")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "ApprovePhoto")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "CreateAlbum")] = new(Constants.Policies.MemberOnly, false, false),
            [("GalleryController", "CreateGallery")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "DeleteGallery")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "GetAllGalleries")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "GetGalleries")] = new(null, true, false),
            [("GalleryController", "GetGallery")] = new(null, true, false),
            [("GalleryController", "GetMyAlbums")] = new(Constants.Policies.MemberOnly, false, false),
            [("GalleryController", "GetPendingApprovals")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "RejectGallery")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "RejectPhoto")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "RemovePhoto")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "SubmitMemberPhoto")] = new(Constants.Policies.MemberOnly, false, false),
            [("GalleryController", "ToggleActive")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "ToggleFeatured")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "UpdateGallery")] = new(Constants.Policies.AdminOnly, false, false),
            [("GalleryController", "UploadPhoto")] = new(Constants.Policies.AdminOnly, false, false),
            [("GatewaysController", "BkashCallbackGet")] = new(null, true, false),
            [("GatewaysController", "DGePayCallback")] = new(null, true, false),
            [("GatewaysController", "GatewayWebhook")] = new(null, true, false),
            [("GatewaysController", "InitiatePayment")] = new(null, true, false),
            [("GatewaysController", "SSLCommerzCallback")] = new(null, true, false),
            [("GovernanceController", "GetConstitutionHistory")] = new(null, true, false),
            [("GovernanceController", "GetCurrentConstitution")] = new(null, true, false),
            [("GovernanceController", "GetCurrentEC")] = new(null, true, false),
            [("GovernanceController", "GetECHistory")] = new(null, true, false),
            [("GovernanceController", "VoteOnAmendment")] = new(null, false, false),
            [("HealthController", "GetHealth")] = new(null, true, false),
            [("JobHubController", "ApproveJob")] = new(Constants.Policies.AdminOnly, false, false),
            [("JobHubController", "DeactivateJob")] = new(null, false, false),
            [("JobHubController", "GetActiveJobs")] = new(null, true, false),
            [("JobHubController", "GetJob")] = new(null, true, false),
            [("JobHubController", "GetPendingJobs")] = new(Constants.Policies.AdminOnly, false, false),
            [("JobHubController", "PostJob")] = new(null, false, false),
            [("JobHubController", "RejectJob")] = new(Constants.Policies.AdminOnly, false, false),
            [("JobHubController", "UpdateJob")] = new(null, false, false),
            [("LookupsController", "CreateLookup")] = new(Constants.Policies.AdminOnly, false, false),
            [("LookupsController", "DeleteLookup")] = new(Constants.Policies.AdminOnly, false, false),
            [("LookupsController", "GetAllLookups")] = new(null, true, false),
            [("LookupsController", "GetByGroup")] = new(null, true, false),
            [("LookupsController", "GetPublicStats")] = new(null, true, false),
            [("LookupsController", "UpdateLookup")] = new(Constants.Policies.AdminOnly, false, false),
            [("MemberCommunicationsController", "GetMine")] = new(null, false, false),
            [("MemberImportController", "Export")] = new(Constants.Policies.AdminOnly, false, false),
            [("MemberImportController", "Import")] = new(Constants.Policies.AdminOnly, false, false),
            [("MentorshipController", "GetAllForAdmin")] = new(Constants.Policies.AdminOnly, false, false),
            [("MentorshipController", "GetReceived")] = new(null, false, false),
            [("MentorshipController", "GetSent")] = new(null, false, false),
            [("MentorshipController", "MarkComplete")] = new(null, false, false),
            [("MentorshipController", "Respond")] = new(null, false, false),
            [("MentorshipController", "SendRequest")] = new(null, false, false),
            [("MessagingController", "GetChatHistory")] = new(null, false, false),
            [("MessagingController", "GetConversations")] = new(null, false, false),
            [("MessagingController", "GetUnreadCount")] = new(null, false, false),
            [("MessagingController", "MarkAsRead")] = new(null, false, false),
            [("MessagingController", "SendMessage")] = new(null, false, false),
            [("NetworkingController", "GetExecutiveCommittee")] = new(null, true, false),
            [("NetworkingController", "GetLatestUpdates")] = new(null, true, false),
            [("NetworkingController", "GetPeriods")] = new(null, true, false),
            [("NetworkingController", "GetPublicProfile")] = new(null, true, false),
            [("NetworkingController", "Search")] = new(null, true, false),
            [("NewsController", "AddCollaborator")] = new(Constants.Policies.AdminOnly, false, false),
            [("NewsController", "ApproveArticle")] = new(Constants.Policies.AdminOnly, false, false),
            [("NewsController", "CreateNews")] = new(Constants.Policies.AdminOnly, false, false),
            [("NewsController", "DeleteNews")] = new(Constants.Policies.AdminOnly, false, false),
            [("NewsController", "GetActiveNews")] = new(null, true, false),
            [("NewsController", "GetAllNewsForAdmin")] = new(Constants.Policies.AdminOnly, false, false),
            [("NewsController", "GetMySubmissions")] = new(null, false, false),
            [("NewsController", "GetNewsById")] = new(null, true, false),
            [("NewsController", "GetPendingSubmissions")] = new(Constants.Policies.AdminOnly, false, false),
            [("NewsController", "RejectArticle")] = new(Constants.Policies.AdminOnly, false, false),
            [("NewsController", "RemoveCollaborator")] = new(Constants.Policies.AdminOnly, false, false),
            [("NewsController", "SubmitArticle")] = new(null, false, false),
            [("NewsController", "UpdateNews")] = new(Constants.Policies.AdminOnly, false, false),
            [("NewsController", "UploadDocument")] = new(Constants.Policies.AdminOnly, false, false),
            [("NewsController", "UploadImage")] = new(null, false, false),
            [("NotificationController", "GetMyNotifications")] = new(null, false, false),
            [("NotificationController", "MarkAllAsRead")] = new(null, false, false),
            [("NotificationController", "MarkAsRead")] = new(null, false, false),
            [("NotificationController", "RegisterDeviceToken")] = new(null, false, false),
            [("OrgConfigController", "GetConfig")] = new(null, true, false),
            [("OrgConfigController", "UpdateConfig")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("PaymentConfigController", "CreateConfig")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("PaymentConfigController", "DeleteConfig")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("PaymentConfigController", "GetActivePaymentMethods")] = new(null, true, false),
            [("PaymentConfigController", "GetAllConfigs")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("PaymentConfigController", "SeedDefaults")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("PaymentConfigController", "ToggleConfig")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("PaymentConfigController", "UpdateConfig")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("PendingApprovalsController", "GetAdminSummary")] = new(Constants.Policies.AdminOnly, false, false),
            [("PendingApprovalsController", "GetMySummary")] = new(null, false, false),
            [("PollController", "GetActivePolls")] = new(null, false, false),
            [("PollController", "GetPoll")] = new(null, false, false),
            [("PollController", "Vote")] = new(null, false, false),
            [("ProfileController", "ChangePassword")] = new(null, false, false),
            [("ProfileController", "GetCertificate")] = new(null, false, false),
            [("ProfileController", "GetCertificatePdf")] = new(null, false, false),
            [("ProfileController", "GetIDCard")] = new(null, false, false),
            [("ProfileController", "GetIDCardPdf")] = new(null, false, false),
            [("ProfileController", "GetProfile")] = new(null, false, false),
            [("ProfileController", "UpdateProfile")] = new(null, false, false),
            [("ProfileController", "UploadPhoto")] = new(null, false, false),
            [("ProfileController", "UploadSignature")] = new(null, false, false),
            [("RegistrationController", "GetStatus")] = new(null, true, false),
            [("RegistrationController", "Register")] = new(null, true, false),
            [("RegistrationController", "ResendOtp")] = new(null, true, false),
            [("RegistrationController", "VerifyEmail")] = new(null, true, false),
            [("RolesController", "AssignRole")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("RolesController", "CreateAdmin")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("RolesController", "CreateRole")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("RolesController", "DeleteUser")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("RolesController", "DisableUser")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("RolesController", "EnableUser")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("RolesController", "GetRoles")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("RolesController", "GetUsers")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("RolesController", "RemoveRole")] = new(Constants.Policies.SuperAdminOnly, false, false),
            [("RolesController", "ResetPasswordAdmin")] = new(Constants.Policies.SuperAdminOnly, false, true),
            [("ScholarshipsController", "AdminApplications")] = new(Constants.Policies.AdminOnly, false, false),
            [("ScholarshipsController", "Apply")] = new(null, true, false),
            [("ScholarshipsController", "CreateAward")] = new(Constants.Policies.AdminOnly, false, false),
            [("ScholarshipsController", "CreateCall")] = new(Constants.Policies.AdminOnly, false, false),
            [("ScholarshipsController", "CreateFund")] = new(Constants.Policies.AdminOnly, false, false),
            [("ScholarshipsController", "Disburse")] = new(Constants.Policies.AdminOnly, false, false),
            [("ScholarshipsController", "PublicCalls")] = new(null, true, false),
            [("ScholarshipsController", "PublicFunds")] = new(null, true, false),
            [("ScholarshipsController", "ReviewApplication")] = new(Constants.Policies.MemberOnly, false, false),
            [("ScholarshipsController", "ReviewQueue")] = new(Constants.Policies.MemberOnly, false, false),
            [("ScholarshipsController", "Status")] = new(null, true, false),
            [("ScholarshipsController", "SubmitReview")] = new(Constants.Policies.MemberOnly, false, false),
            [("SecureFilesController", "GetSecureFile")] = new(null, false, false),
            [("SiteContentController", "Create")] = new(Constants.Policies.AdminOnly, false, false),
            [("SiteContentController", "Delete")] = new(Constants.Policies.AdminOnly, false, false),
            [("SiteContentController", "GetAll")] = new(Constants.Policies.AdminOnly, false, false),
            [("SiteContentController", "GetByGroup")] = new(null, true, false),
            [("SiteContentController", "GetById")] = new(Constants.Policies.AdminOnly, false, false),
            [("SiteContentController", "Update")] = new(Constants.Policies.AdminOnly, false, false),
            [("ThemeController", "CreateTheme")] = new(Constants.Policies.AdminOnly, false, false),
            [("ThemeController", "DeleteTheme")] = new(Constants.Policies.AdminOnly, false, false),
            [("ThemeController", "GetActiveTheme")] = new(null, true, false),
            [("ThemeController", "GetAllThemes")] = new(Constants.Policies.AdminOnly, false, false),
            [("ThemeController", "UpdateTheme")] = new(Constants.Policies.AdminOnly, false, false),
        };

        private static IEnumerable<(Type ControllerType, MethodInfo Action)> DiscoverActions()
        {
            var assembly = typeof(RequireStepUpAttribute).Assembly;
            var controllerTypes = assembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var controllerType in controllerTypes)
            {
                var actions = controllerType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => !m.IsSpecialName && m.GetCustomAttribute<NonActionAttribute>() == null);

                foreach (var action in actions)
                {
                    yield return (controllerType, action);
                }
            }
        }

        private static ExpectedAuth ResolveActual(Type controllerType, MethodInfo action)
        {
            var methodAuthorize = action.GetCustomAttribute<AuthorizeAttribute>();
            var classAuthorize = controllerType.GetCustomAttribute<AuthorizeAttribute>();
            var policy = methodAuthorize?.Policy ?? classAuthorize?.Policy;

            var allowAnonymous = action.GetCustomAttribute<AllowAnonymousAttribute>() != null
                || controllerType.GetCustomAttribute<AllowAnonymousAttribute>() != null;

            var stepUp = action.GetCustomAttribute<RequireStepUpAttribute>() != null
                || controllerType.GetCustomAttribute<RequireStepUpAttribute>() != null;

            return new ExpectedAuth(policy, allowAnonymous, stepUp);
        }

        [Test]
        public void EveryControllerAction_MatchesExpectedAuthorizationCatalog()
        {
            var actual = DiscoverActions()
                .ToDictionary(x => (Controller: x.ControllerType.Name, Action: x.Action.Name), x => ResolveActual(x.ControllerType, x.Action));

            var missingFromCode = Expected.Keys.Except(actual.Keys).ToList();
            var missingFromCatalog = actual.Keys.Except(Expected.Keys).ToList();
            var drifted = new List<string>();

            foreach (var key in Expected.Keys.Intersect(actual.Keys))
            {
                var expected = Expected[key];
                var found = actual[key];
                if (expected != found)
                {
                    drifted.Add($"{key.Controller}.{key.Action}: expected ({expected.Policy ?? "null"}, AllowAnonymous={expected.AllowAnonymous}, StepUp={expected.StepUp}) " +
                                $"but found ({found.Policy ?? "null"}, AllowAnonymous={found.AllowAnonymous}, StepUp={found.StepUp})");
                }
            }

            var message = new List<string>();
            if (missingFromCatalog.Count > 0)
                message.Add($"Actions in code but not in the expected catalog (new/unreviewed endpoints): {string.Join(", ", missingFromCatalog.Select(k => $"{k.Controller}.{k.Action}"))}");
            if (missingFromCode.Count > 0)
                message.Add($"Catalog entries with no matching action in code (stale entries): {string.Join(", ", missingFromCode.Select(k => $"{k.Controller}.{k.Action}"))}");
            if (drifted.Count > 0)
                message.Add($"Attribute drift: {string.Join("; ", drifted)}");

            Assert.That(message, Is.Empty, string.Join("\n", message));
        }

        // Spec 023 (37.12e): ElectionStaff alone lets any official in, so an election write must also
        // carry the per-election filter, unless it is AdminOnly. Member actions check the member instead.
        // Actions a member or appointee takes on their own row, not on the election.
        private static readonly HashSet<string> ElectionMemberActions = ["Nominate", "Withdraw", "Vote", "Accept", "Decline"];

        [Test]
        public void EveryElectionWriteAction_HasThePermissionFilterOrAdminOnly()
        {
            var controllers = new[] { typeof(GHCAA.API.Controllers.ElectionsController), typeof(GHCAA.API.Controllers.AdminElectionsController), typeof(GHCAA.API.Controllers.ElectionAppointmentsController) };
            var unguarded = DiscoverActions()
                .Where(x => controllers.Contains(x.ControllerType) && !ElectionMemberActions.Contains(x.Action.Name))
                .Where(x => x.Action.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>().Any(h => h is not HttpGetAttribute))
                .Where(x => x.Action.GetCustomAttribute<RequireElectionPermissionAttribute>() == null
                    && ResolveActual(x.ControllerType, x.Action).Policy != Constants.Policies.AdminOnly)
                .Select(x => $"{x.ControllerType.Name}.{x.Action.Name}")
                .ToList();

            Assert.That(unguarded, Is.Empty, $"Election writes with neither the permission filter nor AdminOnly: {string.Join(", ", unguarded)}");
        }
    }
}
