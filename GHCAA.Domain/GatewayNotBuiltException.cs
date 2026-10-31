namespace GHCAA.Domain;

public sealed class GatewayNotBuiltException(string message) : InvalidOperationException(message);
