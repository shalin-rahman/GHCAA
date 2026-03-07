namespace GHCAA.Domain
{
    public class Enums
    {
        public enum MembershipStatus { Applied, Active, InactivePayment, InactiveResigned, Terminated }
        public enum MembershipType { Founding, Executive, General, Associate, Honorary, Advisory }
        public enum MemberCategory { None, Lifelong, Donor, Patron }
        public enum ECPosition { None, President, VicePresident, GeneralSecretary, OfficeSecretary, JointSecretary1, JointSecretary2, Treasurer, MediaCulturalAndSportsSecretary, OrganizationalSecretary, InformationAndTechnologySecretary, Member1, Member2, LawSecretary, ImmediatePastPresident, InstitutionalRepresentative }
        public enum BloodGroup { APositive, ANegative, BPositive, BNegative, OPositive, ONegative, ABPositive, ABNegative }
        public enum Gender { Male, Female, Other }
        public enum Degree { HSC, Bachelor, Masters, PhD, Other }

        // File / OTP / Payment support enums
        public enum FileUploadType { Photo, Certificate, PaymentProof, GalleryPhoto, NewsImage }
        public enum FileUploadStatus { Pending, Approved, Rejected }
        public enum OtpPurpose { Registration, PasswordReset }
        public enum PaymentStatus { Pending, Completed, Failed, Refunded }

        // Financial Ledger Enums
        public enum FinancialRecordType { Income, Expense }
        public enum FinancialCategory { MembershipFee, Donation, Event, Maintenance, Salary, Utilities, Other }

        // News & Updates Enums
        public enum NewsCategory { News, OrganisationalUpdate, BusinessInformation }

        // Job Hub Enums
        public enum JobCategory { IT, Finance, Engineering, Marketing, Education, Health, PublicSector, Mentorship, Other }

        // Event Registration Enums
        public enum EventRegistrationStatus { Pending, Approved, Rejected }
    }
}