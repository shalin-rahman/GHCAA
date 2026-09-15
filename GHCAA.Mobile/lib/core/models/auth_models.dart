/// Typed shape for /auth/login, /auth/refresh-mobile, /auth/google and /auth/facebook
/// responses. `token` is what GHCAA.Domain's login flow always returns on a 200 — a
/// missing or renamed token field should throw here, not get silently stored as null.
class LoginResponse {
  final String token;
  final String? refreshToken;
  final String role;

  LoginResponse({
    required this.token,
    this.refreshToken,
    this.role = 'Member',
  });

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    return LoginResponse(
      token: json['token'] as String,
      refreshToken: json['refreshToken'] as String?,
      role: json['role'] as String? ?? 'Member',
    );
  }
}
