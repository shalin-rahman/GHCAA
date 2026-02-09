namespace GHCAA.Domain
{
    public class Enums
    {
        public enum MembershipStatus { Applied, Active, InactivePayment, InactiveResigned, Terminated }
        public enum MembershipType { Founding, Executive, General, Associate, Honorary, Advisory }
        public enum ECPosition { President, GeneralSecretary, Member, None }
        public enum BloodGroup { APositive, ANegative, BPositive, BNegative, OPositive, ONegative, ABPositive, ABNegative }
        public enum Gender { Male, Female, Other }
        public enum Degree { HSC, Bachelor, Masters, PhD, Other }

        // File / OTP / Payment support enums
        public enum FileUploadType { Photo, Certificate, PaymentProof }
        public enum FileUploadStatus { Pending, Approved, Rejected }
        public enum OtpPurpose { Registration, PasswordReset }
        public enum PaymentStatus { Pending, Completed, Failed, Refunded }
    }
}