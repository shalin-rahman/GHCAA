namespace GHCAA.Domain;

// 84.102: a request the current data refuses, such as a duplicate application or a second
// registration for the same event. Controllers turn it into a 409 with this message. It derives
// from InvalidOperationException so code that already catches that type still does.
public sealed class RequestConflictException(string message) : InvalidOperationException(message);
