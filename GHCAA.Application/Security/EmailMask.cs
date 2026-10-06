namespace GHCAA.Application.Security
{
    // Shows enough of an address for the owner to recognise it ("sha*****@gmail.com") without
    // handing the whole thing to whoever is looking at the screen.
    public static class EmailMask
    {
        public const int VisibleChars = 3;

        public static string Mask(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return string.Empty;

            var trimmed = email.Trim();
            var at = trimmed.LastIndexOf('@');
            var local = at > 0 ? trimmed[..at] : trimmed;
            var domain = at > 0 ? trimmed[at..] : string.Empty;

            // A short local part would be fully revealed by the first three characters, so keep one.
            var visible = local.Length > VisibleChars ? VisibleChars : 1;
            var hidden = Math.Max(local.Length - visible, 3);

            return local[..visible] + new string('*', hidden) + domain;
        }
    }
}
