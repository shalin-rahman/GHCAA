namespace GHCAA.Domain
{
    public class Enums
    {
        public enum MembershipStatus { Applied, Active, InactivePayment, InactiveResigned, Terminated, Rejected }
        public enum MembershipType { Founding, Executive, General, Associate, Honorary, Advisory, Guest }
        public enum MemberCategory { None, LifelongPatron, Sponsor, Advisor, Mentor, Recruiter, Active, Volunteer, Contributor, Guest, Student }
        public enum ECPosition { None, President, VicePresident, GeneralSecretary, OfficeSecretary, JointSecretary1, JointSecretary2, Treasurer, MediaCulturalAndSportsSecretary, OrganizationalSecretary, InformationAndTechnologySecretary, Member1, Member2, LawSecretary, ImmediatePastPresident, InstitutionalRepresentative }
        public enum BloodGroup { Unknown, APositive, ANegative, BPositive, BNegative, OPositive, ONegative, ABPositive, ABNegative }
        public enum Gender { None, Male, Female, Other }
        public enum Degree { HSC, Bachelor, Masters, PhD, Other }

        // File / OTP / Payment support enums
        public enum FileUploadType { Photo, Certificate, PaymentProof, Signature, GalleryPhoto, NewsImage, NoticeDocument }
        public enum FileUploadStatus { Pending, Approved, Rejected }
        public enum OtpPurpose { Registration, PasswordReset, AdminStepUp }
        public enum PaymentStatus { Pending, Completed, Failed, Refunded }

        // Financial Ledger Enums
        public enum FinancialRecordType { Income, Expense }
        public enum FinancialCategory { MembershipFee, RegistrationFee, Donation, Event, Maintenance, Salary, Utilities, ReunionFee, Sponsorship, Grant, Refund, Other }

        // News & Updates Enums
        public enum NewsCategory { News, OrganisationalUpdate, BusinessInformation }

        // Job Hub Enums
        public enum JobCategory { IT, Finance, Engineering, Marketing, Education, Health, PublicSector, Mentorship, Other }

        // Event Registration Enums
        public enum EventRegistrationStatus { Pending, Approved, Rejected, Waitlisted }

        // Payment Method Enums
        public enum PaymentMethod { ManualReceipt, BKash, Nagad, Rocket, CreditCard, BankTransfer, CashOnHand }

        // Payment Gateway Enums
        public enum PaymentGateway { None, Stripe, PayPal, SSLCommerz, BkashGateway, NagadGateway, RocketGateway, BankTransferGateway, Manual, DGePay }

        public enum SubmissionStatus { Draft, Pending, Approved, Rejected }
        public enum ArticleCategory { Event, Magazine, Regular }
        public enum PostType { News, Notice }

        // Family Linking Enums
        public enum RelationshipType { Spouse, Parent, Child, Sibling, Other }
        public enum FamilyLinkStatus { Requested, Accepted, Rejected, Cancelled }
        public enum EventStatus { Draft, Published, Archived }


        // Volunteer Enums
        public enum VolunteerRole { EventOrganizer, GuestManagement, ContentCreator, Mentor, TechnicalSupport, Other }

        public enum NotificationType { EventCreation, ParticipationApproval, RegistrationUpdate, GeneralSystem, DirectMessage, ApprovalRequest }
        public enum SocialProvider { Google, Facebook }

        // Communication Template Enums
        public enum MessageChannel { Email, Sms }

        // Fundraising Enums (TODO 37.3)
        public enum PledgeStatus { Pledged, PartiallyPaid, Paid, Lapsed, Cancelled }
    }
}