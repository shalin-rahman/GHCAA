using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GHCAA.Infrastructure.Data.Migrations.PgSql
{
    /// <inheritdoc />
    public partial class SeedPolicySiteContent : Migration
    {
        // 100.6: first text of the privacy, terms and FAQ pages. Admins edit it in Site Content.
        // {branding.*} and {contact.*} placeholders are filled from OrgConfig when the page renders.
        // The text is frozen here; PolicyContentSeeder reads these rows for databases built without migrations.
        public static readonly (string Key, string Title, int Order, string Body)[] Rows =
        {
            ("privacy-policy", "Privacy Policy", 1, """
                <p><em>Last reviewed: 17-11-2026.</em></p>
                <p>This policy explains what personal information {branding.fullName} ({branding.shortName}) collects through this website and its mobile app, why it is collected, who can see it, and the choices you have.</p>
                <h2>Who we are</h2>
                <p>{branding.fullName} is the body that controls the personal information described here. You can reach us at {contact.supportEmail} or at our registered office: {contact.registeredOffice}.</p>
                <h2>What we collect</h2>
                <ul>
                <li><strong>Registration and profile:</strong> your name, parents' names, date of birth, gender, blood group, national ID number, mobile number, email address, present and permanent addresses, emergency contact, photograph, signature, academic and professional history, and the certificate you upload as proof of study.</li>
                <li><strong>Payments:</strong> the amount, method, date and reference of each membership or event payment, and any receipt or proof you upload. Card and mobile wallet details are entered with the payment provider and are not stored by us.</li>
                <li><strong>Participation:</strong> the events you register for, election forms you submit, and articles, comments or photos you post.</li>
                <li><strong>Account and device:</strong> your sign-in details, verification codes, notification choices and, if you use the mobile app, a device token used to send push notices.</li>
                <li><strong>Technical records:</strong> server logs of requests and errors, kept to run and protect the service.</li>
                </ul>
                <h2>Why we use it</h2>
                <ul>
                <li>To check your eligibility and manage your membership.</li>
                <li>To record dues and payments and send receipts.</li>
                <li>To run the member directory, events and elections under the constitution.</li>
                <li>To send notices about your account, your membership and the association.</li>
                <li>To keep the service secure and meet legal and accounting duties.</li>
                </ul>
                <p>We rely on the consent you give when you register, on running your membership, and on our legal duties. You can withdraw consent by writing to us; some records must still be kept, as explained below.</p>
                <h2>What other members can see</h2>
                <p>Signed-in members can see your name, photograph and membership details in the directory. Profile photos are stored as ordinary web files, so anyone who has the link to a photo can open it. Your mobile number, email, address, national ID and family links are shown only if you choose to show them in your profile privacy settings. Committee members and administrators can see your full record to manage the association.</p>
                <h2>Who we share it with</h2>
                <p>We do not sell personal information. We share it only with services that help us run the association, such as email delivery, payment processing, hosting and file storage, and only as far as each service needs. We may also disclose information when the law requires it.</p>
                <h2>Cookies and storage</h2>
                <p>The website uses a sign-in cookie and the browser's own storage to keep you signed in and remember choices such as the colour theme. We do not use advertising or tracking cookies.</p>
                <h2>How long we keep it</h2>
                <p>We keep your record while you are a member. When membership ends, the record is archived. Payment and election records are kept for as long as the association's rules and the law require.</p>
                <h2>How we protect it</h2>
                <p>Access is limited by role, passwords are stored as one-way hashes, certificates, payment proofs and signatures are kept in storage that needs a sign-in, and connections are encrypted. No system is perfectly secure, so please tell us at once if you think your account has been misused.</p>
                <h2>Your rights</h2>
                <p>You can see and correct most of your details in your profile. You can also ask us for a copy of your information, ask us to correct or delete it, or object to a use of it. We will answer within a reasonable time and explain any record we must keep.</p>
                <h2>Changes to this policy</h2>
                <p>We may update this policy. The date at the top shows when it was last reviewed, and we will tell members about important changes.</p>
                <h2>Contact</h2>
                <p>Questions or requests about your information: {contact.supportEmail}.</p>
                """),
            ("terms-of-use", "Terms of Use", 2, """
                <p><em>Last reviewed: 17-11-2026.</em></p>
                <p>These terms apply to anyone who uses the website and mobile app of {branding.fullName} ({branding.shortName}). By registering or using the service you agree to them. The constitution of the association also applies to members, and where the two differ the constitution prevails.</p>
                <h2>Membership</h2>
                <p>Membership is open to people who meet the eligibility rules in the constitution. Every application is checked, and the association may approve or refuse it. You must give true and complete details and keep them up to date.</p>
                <h2>Your account</h2>
                <p>Keep your password and verification codes private. You are responsible for what happens under your account. Tell us at once at {contact.supportEmail} if you think someone else has used it.</p>
                <h2>Fees and payments</h2>
                <p>Membership and event fees are set by the association and shown before you pay. A payment counts once it is confirmed by the payment provider or verified by the treasurer. Refunds are handled under the association's rules.</p>
                <h2>Acceptable use</h2>
                <ul>
                <li>Use member details only for association purposes. Do not copy the directory or use it for marketing.</li>
                <li>Do not post anything unlawful, abusive, misleading or that infringes someone else's rights.</li>
                <li>Do not try to reach accounts or data that are not yours, or disrupt the service.</li>
                </ul>
                <h2>What you post</h2>
                <p>You keep ownership of the articles, comments and photos you post. You allow the association to show them on its website, app and publications. Administrators may edit or remove content that breaks these terms.</p>
                <h2>Elections</h2>
                <p>Elections run under the constitution and the election rules published on this site. The election commission's decisions on conduct and results apply.</p>
                <h2>Suspension</h2>
                <p>The association may suspend or close an account that breaks these terms or the constitution, following the constitution's procedures where they apply.</p>
                <h2>Service and liability</h2>
                <p>We try to keep the service available and accurate, but it is provided as it is, without a promise that it will always be available or free of errors. As far as the law allows, the association is not liable for indirect loss from using the service.</p>
                <h2>Changes</h2>
                <p>We may update these terms. The date at the top shows when they were last reviewed. Continuing to use the service after a change means you accept it.</p>
                <h2>Governing law and contact</h2>
                <p>These terms are governed by the laws of the country where the association is registered. Questions: {contact.supportEmail}, or {contact.registeredOffice}.</p>
                """),
            ("faq", "Frequently Asked Questions", 3, """
                <h2>Who can join?</h2>
                <p>Former students who meet the eligibility rules in the constitution. Choose Register, fill in your details and upload proof of study.</p>
                <h2>What happens after I register?</h2>
                <p>You confirm your email with a code, then the association checks your application. You get an email when it is approved.</p>
                <h2>How do I pay my membership fee?</h2>
                <p>Pay during registration or later from the payments page of the member portal. The accepted methods are shown there. You get a receipt once the payment is confirmed.</p>
                <h2>I forgot my password. What do I do?</h2>
                <p>Choose Forgot password on the sign-in page and follow the link sent to your email.</p>
                <h2>How do I update my details?</h2>
                <p>Sign in and open your profile. If a detail you cannot edit is wrong, such as your membership number, contact the association.</p>
                <h2>Who can see my phone number and address?</h2>
                <p>Only you decide. Your profile privacy settings control whether other members can see your mobile number, email, address, national ID and family links. See the Privacy Policy for details.</p>
                <h2>How do I join an event?</h2>
                <p>Open the event in the member portal and register. Some events need approval or a fee, and the event page says which.</p>
                <h2>Who can vote in elections?</h2>
                <p>Members who meet the voting rules in the constitution and the election schedule. The Elections page explains the rules and dates for each election.</p>
                <h2>How do I contact the association?</h2>
                <p>Use the Contact page or write to {contact.supportEmail}.</p>
                """),
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The baseline inserted SiteContents rows with explicit Ids and never moved the
            // sequence, so it must catch up before these rows take generated Ids.
            migrationBuilder.Sql(AdvanceSequenceSql);

            // A key an admin already created by hand is left alone.
            var values = string.Join(",\n", Rows.Select(r =>
                $"('{r.Key}', 'policy', '{Quote(r.Title)}', '{Quote(r.Body)}', {r.Order}, TRUE, NOW())"));
            migrationBuilder.Sql($"""
                INSERT INTO "SiteContents" ("Key", "Group", "Title", "BodyHtml", "DisplayOrder", "IsActive", "LastModified")
                VALUES
                {values}
                ON CONFLICT ("Key") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Every admin save sets UpdatedByAdminId, so a row an admin has edited or created stays.
            var keys = string.Join(", ", Rows.Select(r => $"'{r.Key}'"));
            migrationBuilder.Sql($"""
                DELETE FROM "SiteContents"
                WHERE "Key" IN ({keys}) AND "UpdatedByAdminId" IS NULL;
                """);
        }

        // nextval is in the GREATEST so the sequence only moves forward, even after rows were deleted.
        public const string AdvanceSequenceSql = """
            DO $sv$ BEGIN PERFORM setval(s, GREATEST(COALESCE((SELECT MAX("Id") FROM "SiteContents"), 1), nextval(s)))
            FROM pg_get_serial_sequence('"SiteContents"', 'Id') AS s; END $sv$;
            """;

        private static string Quote(string s) => s.Replace("'", "''");
    }
}
