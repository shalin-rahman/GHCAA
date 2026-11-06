namespace GHCAA.Domain;

// 84.68: a reissue that cannot go ahead because of the credential's current state. The API turns it into a 409.
public sealed class CredentialConflictException(string message) : InvalidOperationException(message);
