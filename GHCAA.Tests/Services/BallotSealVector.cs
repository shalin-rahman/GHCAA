namespace GHCAA.Tests.Services;

// TODO 37.13e. A fixed ballot sealed outside .NET (Python cryptography 46) with a throwaway
// test key, so a change to the sealed layout or the associated data breaks a test. The key
// seals nothing real. Docs: docs/Elections/11-Election-Key-Procedure.md, "Ballot protocol".
internal static class BallotSealVector
{
    public const int ElectionId = 7;
    public const string Plaintext = "[{\"S\":1,\"N\":[2]}]";
    public const string Fingerprint = "c3679f5a7272d3974469168c0e8cc6932718933071921cd2be96c990eb6e9f2e";

    public const string PrivateKeyPkcs8 = """
        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
        """;

    public const string Sealed = """
        v2:RwuBW8Gj1HTeUouh1IYQhCOB+AckOmoLVEQ/sEH82D33uH+RfDwWd2xQmnKUISUvg93q4doZ88slV6VpSHDWtpHMJEMM2
        5cTvPyLsnyj6VCzQz5wjTYXawuBac/lOPiVwnp6W8E7keVZ1bxPKlSk+IlJnQAa4vF84It14XbXTZwReECjdF31p1n4JVGf5
        +ZxhkH+BYUpHFI2PLRxrsi3cAgrYQm/TWzbAYz18MiOmUU12gtgolEyTUMmGEmI4+is91u/K9KWZOhPspRN7Wcn37ZnfEFMU
        gw37/rOVddZ9h4npTqpwTBx90QxCnURVa39MM9+CiFDuHNjbQ/mKQcX4iQcf5NXLI+tnuSRfX7OJCyQ1IoQyy42MG3+qGWS9
        ZqGS+04/4hreRsgd2HGEL/rK8KFqWlElo9aiu1BLM7UPcuoVS5iI2iarzXgohLCGR5rMNspYcHL3mHWlkWo1V7/BFRNKJ6jx
        kKNsrhjqo4ZrmAxMfMgeZnBb9aKvCeZqNEovmsgyEmfXxAnDOM6OcVIPcexhTiQDHSKzZ5Lcx1UgqkRg4U2luhXdktcT1BQ+
        PvuvpbfJI4ejpMAZezoAYeHvKUVwdSDo7wfOuxjiDEYpK80CKSutREw0b8dGVGir+nk4X3EboB8/e1SShdwFx046UnK/8Art
        dgtK0lsLhj3XAe80CLw2moELA9S5WChG0HaSwl3UaOuDq7BXWPUrcUq3b44UBVp7ZSIHMQI05exLf69WUGl2+D8H9KEUr7Ln
        1ewA6Hq8Mel3+RBvIFYqqR7Z1QaPxkM2W+O4JEY8VIG+xg3JBkUkzFL3qrrwhG8Q5TM3R14YtRlR5ivIcCZgIJYR+iAw4Kt/
        rObCPDKa/FhANJ3lNO71CfcAotlshHBIjKt5vttD1p//jJRqUhj7oFRY8W4HHKl3VonIATjo/XjUkgVJicONUACR5G+veDwW
        BW5wY7B+WnYG8XqVXEVgTPGdjeUhFM/OmzLPKARwQvn/f/3nB1YJ5tq/Lc+KuFl2roY2C0RIxeGX0BAN7laPcjwnrcZgupBI
        yvy0ph2D6FlGN4FMrGlZ86/vaqimdQEPUGv/jT1YGaO9yOoZOBCc8KlhD03jHvWzikwwWLNTpzfUUzMRQQQt3F2aCqas/4nz
        PJ77E3FVAksXzghYTgwojY7DdLCpN1oEqTNrBSLJEkHkhGr57164GtGuXyK3yTP24bRsP5vR8AI/4ePtvDG
        """;
}
