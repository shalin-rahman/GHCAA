namespace GHCAA.Domain;

// A service rejecting caller input. Controllers map only this to 400, so an ArgumentException
// from a bug still surfaces as a 500.
public sealed class InvalidRequestException(string message) : ArgumentException(message);
