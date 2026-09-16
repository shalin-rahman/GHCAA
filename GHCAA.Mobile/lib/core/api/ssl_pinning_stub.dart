import 'package:dio/dio.dart';

/// Web build of certificate pinning: a no-op. dart:io's HttpClient/
/// SecurityContext aren't available on web, and browsers already own and
/// verify the TLS connection below Dio, so there's no adapter to swap in
/// here. Selected instead of ssl_pinning_io.dart by the conditional import
/// in api_client.dart when compiling for web.
void configureDioCertificatePinning(Dio dio, String environment) {}
