namespace GHCAA.Domain.Models
{
    // 95.2: a committee position. Rows replace the old ECPosition enum, so an organisation can
    // add, rename, reorder and retire positions without a code change. ECMember.Position and
    // ElectionSeat.Position hold the Id.
    public class EcPosition
    {
        public int Id { get; set; }

        // Stable name used in URLs, filters and the JSON "position" field. The seeded rows use
        // the old enum names so existing clients keep working.
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? NameBn { get; set; }
        public int DisplayOrder { get; set; }
        public int SeatCount { get; set; } = 1;

        // A retired position stays for past terms and results but cannot be picked again.
        public bool IsActive { get; set; } = true;

        // Shown in the top group of the public committee page.
        public bool ShowOnBoard { get; set; }

        // Forms, certificates and ID cards print the holder of these as the two signatories.
        public bool SignsAsHead { get; set; }
        public bool SignsAsSecretary { get; set; }
    }

    // Ids of the positions seeded with the schema. They equal the old ECPosition enum values, so
    // rows saved before 95.2 point at the same position. Only seeding, defaults and tests use them;
    // everything else reads the table.
    public static class EcPositionIds
    {
        public const int President = 1;
        public const int VicePresident = 2;
        public const int GeneralSecretary = 3;
        public const int OfficeSecretary = 4;
        public const int JointSecretary1 = 5;
        public const int JointSecretary2 = 6;
        public const int Treasurer = 7;
        public const int MediaCulturalAndSportsSecretary = 8;
        public const int OrganizationalSecretary = 9;
        public const int InformationAndTechnologySecretary = 10;
        public const int Member1 = 11;
        public const int Member2 = 12;
        public const int LawSecretary = 13;
        public const int ImmediatePastPresident = 14;
        public const int InstitutionalRepresentative = 15;

        public static EcPosition[] Seed() =>
        [
            Row(President, "President", "President", "সভাপতি", board: true, head: true),
            Row(VicePresident, "VicePresident", "Vice President", "সহ-সভাপতি", board: true),
            Row(GeneralSecretary, "GeneralSecretary", "General Secretary", "সাধারণ সম্পাদক", board: true, secretary: true),
            Row(OfficeSecretary, "OfficeSecretary", "Office Secretary", "কার্যালয় সম্পাদক", board: true),
            Row(JointSecretary1, "JointSecretary1", "Joint Secretary 1", "যুগ্ম সম্পাদক (১)", board: true),
            Row(JointSecretary2, "JointSecretary2", "Joint Secretary 2", "যুগ্ম সম্পাদক (২)", board: true),
            Row(Treasurer, "Treasurer", "Treasurer", "কোষাধ্যক্ষ", board: true),
            Row(MediaCulturalAndSportsSecretary, "MediaCulturalAndSportsSecretary", "Media Cultural & Sports Secretary", "মিডিয়া, সাংস্কৃতিক ও ক্রীড়া সম্পাদক", board: true),
            Row(OrganizationalSecretary, "OrganizationalSecretary", "Organizational Secretary", "সাংগঠনিক সম্পাদক", board: true),
            Row(InformationAndTechnologySecretary, "InformationAndTechnologySecretary", "Information and Technology Secretary", "তথ্য ও প্রযুক্তি সম্পাদক", board: true),
            Row(Member1, "Member1", "Member 1", "সদস্য-১"),
            Row(Member2, "Member2", "Member 2", "সদস্য-২"),
            Row(LawSecretary, "LawSecretary", "Law Secretary", "আইন সম্পাদক", board: true),
            Row(ImmediatePastPresident, "ImmediatePastPresident", "Immediate Past President", "সাবেক সভাপতি"),
            Row(InstitutionalRepresentative, "InstitutionalRepresentative", "Institutional Representative", "প্রাতিষ্ঠানিক প্রতিনিধি"),
        ];

        // Display order follows the old enum order, which is the order the committee lists used.
        private static EcPosition Row(int id, string key, string name, string nameBn,
            bool board = false, bool head = false, bool secretary = false) => new()
            {
                Id = id,
                Key = key,
                Name = name,
                NameBn = nameBn,
                DisplayOrder = id,
                SeatCount = 1,
                IsActive = true,
                ShowOnBoard = board,
                SignsAsHead = head,
                SignsAsSecretary = secretary,
            };
    }
}
