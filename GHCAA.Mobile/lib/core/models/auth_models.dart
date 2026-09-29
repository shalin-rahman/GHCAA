/// Typed shape for /auth/login, /auth/refresh-mobile, /auth/google and /auth/facebook
/// responses. `token` is what GHCAA.Domain's login flow always returns on a 200 — a
/// missing or renamed token field should throw here, not get silently stored as null.
class LoginResponse {
  final String token;
  final String? refreshToken;
  final String role;

  /// Every role the user holds. An older API sends only `role`, so it falls back to that.
  final List<String> roles;
  final List<ElectionAppointmentSummary> electionAppointments;

  LoginResponse({
    required this.token,
    this.refreshToken,
    this.role = 'Member',
    List<String>? roles,
    this.electionAppointments = const [],
  }) : roles = roles ?? [role];

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    final role = json['role'] as String? ?? 'Member';
    return LoginResponse(
      token: json['token'] as String,
      refreshToken: json['refreshToken'] as String?,
      role: role,
      roles: (json['roles'] as List?)?.map((r) => r as String).toList() ?? [role],
      electionAppointments: (json['electionAppointments'] as List?)
              ?.map((a) => ElectionAppointmentSummary.fromJson(Map<String, dynamic>.from(a as Map)))
              .toList() ??
          const [],
    );
  }
}

/// An accepted, live election appointment. /auth/me sends these; 37.12d fills the list.
class ElectionAppointmentSummary {
  final int electionId;
  final String electionTitle;
  final String personaName;
  final int permissions;

  const ElectionAppointmentSummary({
    required this.electionId,
    required this.electionTitle,
    required this.personaName,
    required this.permissions,
  });

  factory ElectionAppointmentSummary.fromJson(Map<String, dynamic> json) => ElectionAppointmentSummary(
        electionId: json['electionId'] as int,
        electionTitle: json['electionTitle'] as String? ?? '',
        personaName: json['personaName'] as String? ?? '',
        permissions: json['permissions'] as int? ?? 0,
      );
}

/// True when [name] is in [roles], or equals [role] for a session saved before the roles list.
bool rolesInclude(List<String>? roles, String? role, String name) =>
    (roles ?? [if (role != null) role]).contains(name);
