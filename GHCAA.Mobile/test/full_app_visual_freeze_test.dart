import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:golden_toolkit/golden_toolkit.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:ghcaa_mobile/screens/member/dashboard_screen.dart';
import 'package:ghcaa_mobile/screens/member/directory_screen.dart';
import 'package:ghcaa_mobile/screens/member/events_screen.dart';
import 'package:ghcaa_mobile/screens/member/governance_screen.dart';
import 'package:ghcaa_mobile/screens/member/committee_screen.dart';
import 'package:ghcaa_mobile/screens/member/financial_portal_screen.dart';
import 'package:ghcaa_mobile/screens/member/news_screen.dart';
import 'package:ghcaa_mobile/screens/member/digital_id_screen.dart';
import 'package:ghcaa_mobile/screens/auth/login_screen.dart';
import 'package:ghcaa_mobile/screens/auth/register_screen.dart';
import 'package:image_picker/image_picker.dart';
import 'dart:io';
import 'dart:async';
import 'dart:convert';
import 'dart:typed_data';
import 'package:flutter_dotenv/flutter_dotenv.dart';

// Services & Models
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';
import 'package:ghcaa_mobile/features/lookups/dropdown_service.dart';
import 'package:ghcaa_mobile/features/files/file_service.dart';
import 'package:ghcaa_mobile/features/admin/admin_service.dart';
import 'package:ghcaa_mobile/features/networking/networking_service.dart';
import 'package:ghcaa_mobile/features/events/events_service.dart';
import 'package:ghcaa_mobile/features/content/content_service.dart';
import 'package:ghcaa_mobile/features/jobs/job_service.dart';
import 'package:ghcaa_mobile/features/networking/mentorship_service.dart';
import 'package:ghcaa_mobile/features/messaging/chat_service.dart';
import 'package:ghcaa_mobile/features/notifications/notification_service.dart';
import 'package:ghcaa_mobile/features/financials/financial_service.dart';
import 'package:ghcaa_mobile/features/financials/gateway_service.dart';
import 'package:ghcaa_mobile/features/activity/activity_service.dart';
import 'package:ghcaa_mobile/features/polls/poll_service.dart';
import 'package:ghcaa_mobile/features/support/support_service.dart' hide FamilyService, familyServiceProvider;
import 'package:ghcaa_mobile/features/assistant/assistant_service.dart';
import 'package:ghcaa_mobile/features/admin/roles_service.dart';
import 'package:ghcaa_mobile/features/networking/family_service.dart';
import 'package:ghcaa_mobile/core/services/biometric_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:local_auth/local_auth.dart';

// --- FAKES ---
class FakeStorageService implements StorageService {
  @override Future<void> saveToken(String token) async {}
  @override Future<String?> getToken() async => 'mock-token';
  @override Future<void> removeToken() async {}
  @override Future<void> saveRole(String role) async {}
  @override Future<String?> getRole() async => 'Member';
  @override Future<void> saveDashboardLayout(bool isCompact) async {}
  @override Future<bool> getDashboardLayout() async => false;
  @override Future<void> saveProfile(Map<String, dynamic> profile) async {}
  @override Future<Map<String, dynamic>?> getProfile() async => null;
  @override Future<void> clearAll() async {}
  @override Future<void> saveCredentials(String username, String password) async {}
  @override Future<Map<String, String>?> getCredentials() async => null;
  @override Future<void> clearCredentials() async {}
  @override Future<void> saveRefreshToken(String token) async {}
  @override Future<String?> getRefreshToken() async => null;
  @override Future<void> removeRefreshToken() async {}
}

class FakeDropdownService implements DropdownService {
  @override Future<List<Map<String, String>>> getOptions(String group) async => [];
}

class FakeFileService implements FileService {
  @override Future<String?> uploadProfilePhoto(dynamic file) async => 'mock/photo.png';
  @override Future<String?> uploadArticleImage(dynamic file) async => 'mock/article.png';
  @override Future<File?> pickImage({ImageSource source = ImageSource.gallery}) async => null;
}

class FakeAdminService implements AdminService {
  @override Future<List<dynamic>> getPendingApprovals() async => [];
  @override Future<List<dynamic>> getContactMessages() async => [];
  @override Future<bool> markMessageAsRead(int messageId) async => true;
  @override Future<bool> resolveApproval(int memberId, bool approve, {required int adminId, String? reason}) async => true;
  @override Future<Map<String, dynamic>> getGlobalAnalytics() async => {'totalMembers': 1000, 'pendingApprovals': 5, 'totalEvents': 2};
  @override Future<List<dynamic>> getECPeriods() async => [];
  @override Future<bool> createECPeriod(Map<String, dynamic> data) async => true;
  @override Future<bool> updateECPeriod(int id, Map<String, dynamic> data) async => true;
  @override Future<List<dynamic>> getLedgerRecords({String? search, int page = 1}) async => [];
  @override Future<Map<String, dynamic>> getLedgerSummary(int year) async => {'totalRevenue': 1000, 'totalExpenses': 500, 'netPosition': 500};
  @override Future<bool> updateMember(int id, Map<String, dynamic> data) async => true;
  @override Future<List<dynamic>> getFeeConfigs() async => [];
  @override Future<bool> addFeeConfig(Map<String, dynamic> data) async => true;
  @override Future<bool> updateFeeConfig(Map<String, dynamic> data) async => true;
  @override Future<List<dynamic>> getCommitteeMembers(int periodId) async => [];
  @override Future<bool> assignMemberToCommittee(int periodId, Map<String, dynamic> data) async => true;
  @override Future<bool> removeMemberFromCommittee(int ecMemberId) async => true;
}

class FakeAuthService implements AuthService {
  @override Future<String?> login(String identifier, String password, {bool enableBiometric = false}) async => null;
  @override Future<void> logout() async {}
  @override Future<String?> register(Map<String, dynamic> data) async => null;
  @override Future<bool> forgotPassword(String identifier) async => true;
  @override Future<String?> getRole() async => 'Member';
  @override Future<bool> updateProfile(Map<String, dynamic> data) async => true;
  @override Future<List<Map<String, dynamic>>> getSocialProviders() async => [];
  @override Future<String?> googleLogin(String idToken) async => null;
  @override Future<String?> facebookLogin(String accessToken) async => null;
}

class FakeNetworkingService implements NetworkingService {
  @override Future<Map<String, dynamic>> searchAlumni({String? query, String? batch, String? department, String? membershipType, String? category, int pageNumber = 1, int pageSize = 20}) async => {'items': [], 'totalItems': 0};
  @override Future<Map<String, dynamic>?> getProfile() async => null;
  @override Future<List<dynamic>> getECPeriods() async => [];
  @override Future<List<dynamic>> getExecutiveCommittee({int? periodId}) async => [];
  @override Future<bool> updateProfile(Map<String, dynamic> data) async => true;
}

class FakeEventsService implements EventsService {
  @override Future<List<dynamic>> getUpcomingEvents() async => [];
  @override Future<bool> registerForEvent(int eventId, {double? amount, String? paymentRef, dynamic receipt}) async => true;
  @override Future<bool> createEvent(Map<String, dynamic> data) async => true;
  @override Future<bool> deleteEvent(int id) async => true;
}

class FakeNewsService implements NewsService {
  @override Future<List<dynamic>> getLatestNews() async => [];
  @override Future<List<dynamic>> getNewsByCategory(String category) async => [];
  @override Future<List<dynamic>> getMySubmissions() async => [];
  @override Future<void> deleteMySubmission(int id) async {}
  @override Future<List<dynamic>> getPendingSubmissions() async => [];
  @override Future<bool> resolveArticle(int id, bool approve) async => true;
  @override Future<List<dynamic>> getGalleryItems() async => [];
  @override Future<bool> uploadGalleryItem(Map<String, dynamic> data) async => true;
}

class FakeMentorshipService implements MentorshipService {
  @override Future<bool> sendRequest(int mentorId, String? message, String? domain) async => true;
  @override Future<List<dynamic>> getSentRequests() async => [];
  @override Future<List<dynamic>> getReceivedRequests() async => [];
  @override Future<bool> respondToRequest(int requestId, bool accept, String? note) async => true;
  @override Future<bool> markComplete(int requestId) async => true;
}

class FakeJobService implements JobService {
  @override Future<List<dynamic>> getAllJobs() async => [];
  @override Future<bool> postJob(Map<String, dynamic> data) async => true;
  @override Future<bool> deleteJob(int id) async => true;
}

class FakeChatService implements ChatService {
  @override Stream<Map<String, dynamic>> get messageStream => const Stream.empty();
  @override Future<void> initHub() async {}
  @override Future<void> sendDirectMessage(int receiverUserId, String message) async {}
  @override Future<List<dynamic>> getConversations() async => [];
  @override Future<List<dynamic>> getChatHistory(int otherUserId) async => [];
  @override void dispose() {}
}

class FakeNotificationService implements NotificationService {
  @override Future<List<dynamic>> getMyNotifications() async => [];
  @override Future<bool> markAsRead(int id) async => true;
}

class FakeFinancialService implements FinancialService {
  @override Future<List<dynamic>> getLedger() async => [];
  @override Future<double> getOutstandingDues() async => 0.0;
  @override Future<List<dynamic>> getSavedMethods() async => [];
  @override Future<bool> deleteSavedMethod(int id) async => true;
  @override Future<String?> getReceiptUrl(int paymentId) async => 'mock/receipt';
  @override Future<List<dynamic>> getActivePaymentConfigs() async => [];
  @override Future<bool> recordPayment({required String transactionId, required double amount, required String paymentMethod, required String financialCategory, String? notes, dynamic receipt}) async => true;
}

class FakeGatewayService implements GatewayService {
  @override Future<PaymentInitiationResponse> initiate(double amount, PaymentGateway gateway, String reference) async {
    return PaymentInitiationResponse(success: true, gatewayUrl: 'https://mock-gateway.com');
  }
}

class FakeActivityService implements ActivityService {
  @override Future<List<dynamic>> getMyActivity() async => [];
  @override Future<List<dynamic>> getGlobalActivity() async => [];
  @override Future<List<dynamic>> getMemberActivity(int memberId) async => [];
}

class FakePollService implements PollService {
  @override Future<List<Poll>> getActivePolls() async => [];
  @override Future<bool> vote(int pollId, List<int> optionIds) async => true;
}

class FakeSupportService implements SupportService {
  @override Future<bool> checkSystemHealth() async => true;
  @override Future<bool> contactSupport(String message) async => true;
}

class FakeAssistantService implements AssistantService {
  @override Future<String> ask(String question) async => 'Mock response';
}

class FakeRolesService implements RolesService {
  @override Future<List<dynamic>> getUsers() async => [];
  @override Future<bool> createAdmin(String username, String password, String role) async => true;
  @override Future<List<dynamic>> getRoles() async => [];
  @override Future<bool> createRole(String roleName) async => true;
  @override Future<bool> assignRole(int userId, String roleName) async => true;
  @override Future<bool> removeRole(int userId, String roleName) async => true;
}

class FakeFamilyService implements FamilyService {
  @override Future<List<dynamic>> getMyFamily() async => [];
  @override Future<List<dynamic>> getSentRequests() async => [];
  @override Future<List<dynamic>> getReceivedRequests() async => [];
  @override Future<bool> sendRequest(String membershipNo, int relationshipType, {String? note}) async => true;
  @override Future<bool> respondToRequest(int requestId, bool approve) async => true;
  @override Future<bool> cancelRequest(int requestId) async => true;
  @override Future<bool> removeLink(int requestId) async => true;
  @override Future<List<dynamic>> searchFamilyMembers(String name) async => [];
}

class FakeBiometricService implements BiometricService {
  @override Future<bool> isBiometricsAvailable() async => false;
  @override Future<List<BiometricType>> getAvailableBiometrics() async => [];
  @override Future<bool> authenticate({required String reason}) async => true;
}

// Helper to wrap a widget in a standard app shell with GoRouter
Widget wrapInApp(Widget child, {List<Override> overrides = const []}) {
  final router = GoRouter(
    initialLocation: '/',
    routes: [
      GoRoute(path: '/', builder: (context, state) => child),
      GoRoute(path: '/login', builder: (context, state) => const SizedBox()),
      GoRoute(path: '/dashboard', builder: (context, state) => const SizedBox()),
      GoRoute(path: '/admin_dashboard', builder: (context, state) => const SizedBox()),
    ],
  );

  return ProviderScope(
    overrides: [
      userProfileProvider.overrideWith((ref) => {
        'fullName': 'Test Member',
        'membershipType': 'Life Member',
        'category': 'Active',
        'batch': '2010',
        'membershipId': 'L-9999'
      }),
      roleProvider.overrideWith((ref) => Future.value('Member')),
      storageServiceProvider.overrideWith((ref) => FakeStorageService()),
      dropdownDataProvider.overrideWith((ref) => FakeDropdownService()),
      fileServiceProvider.overrideWith((ref) => FakeFileService()),
      adminServiceProvider.overrideWith((ref) => FakeAdminService()),
      authServiceProvider.overrideWith((ref) => FakeAuthService()),
      networkingServiceProvider.overrideWith((ref) => FakeNetworkingService()),
      eventsServiceProvider.overrideWith((ref) => FakeEventsService()),
      newsServiceProvider.overrideWith((ref) => FakeNewsService()),
      jobServiceProvider.overrideWith((ref) => FakeJobService()),
      mentorshipServiceProvider.overrideWith((ref) => FakeMentorshipService()),
      chatServiceProvider.overrideWith((ref) => FakeChatService()),
      notificationServiceProvider.overrideWith((ref) => FakeNotificationService()),
      financialServiceProvider.overrideWith((ref) => FakeFinancialService()),
      gatewayServiceProvider.overrideWith((ref) => FakeGatewayService()),
      activityServiceProvider.overrideWith((ref) => FakeActivityService()),
      pollServiceProvider.overrideWith((ref) => FakePollService()),
      supportServiceProvider.overrideWith((ref) => FakeSupportService()),
      assistantServiceProvider.overrideWith((ref) => FakeAssistantService()),
      rolesServiceProvider.overrideWith((ref) => FakeRolesService()),
      familyServiceProvider.overrideWith((ref) => FakeFamilyService()),
      biometricServiceProvider.overrideWith((ref) => FakeBiometricService()),
      activeSpecialThemeProvider.overrideWith((ref) => Future.value(null)),
      globalAppBarVisibilityProvider.overrideWith((ref) => true),
      ...overrides,
    ],
    child: MaterialApp.router(
      routerConfig: router,
      debugShowCheckedModeBanner: false,
      theme: AppTheme.midnightTheme,
    ),
  );
}

/// Global custom pump: two explicit frames instead of pumpAndSettle.
Future<void> _pump(WidgetTester tester) async {
  await tester.pump();
  await tester.pump(const Duration(milliseconds: 50));
}

void main() {
  setUpAll(() async {
    HttpOverrides.global = _MockHttpOverrides();
    dotenv.testLoad(fileInput: 'ORG_ACRONYM=GHCAA\nBASE_API_URL=http://localhost:5087/api');
    await loadAppFonts();
  });

  group('Mobile Full-App Visual Freeze', () {
    testGoldens('Auth: Login Portal', (tester) async {
      await tester.pumpWidgetBuilder(
        wrapInApp(const LoginScreen()),
        surfaceSize: const Size(390, 844),
      );
      await _pump(tester);
      await screenMatchesGolden(tester, 'login_portal', customPump: _pump);
    });

    testGoldens('Auth: Registration Wizard', (tester) async {
      await tester.pumpWidgetBuilder(
        wrapInApp(const RegisterScreen()),
        surfaceSize: const Size(390, 844),
      );
      await _pump(tester);
      await screenMatchesGolden(tester, 'registration_wizard', customPump: _pump);
    });

    testGoldens('Member: Governance Portal', (tester) async {
      await tester.pumpWidgetBuilder(
        wrapInApp(const GovernanceScreen()),
        surfaceSize: const Size(390, 844),
      );
      await _pump(tester);
      await screenMatchesGolden(tester, 'governance_portal', customPump: _pump);
    });

    testGoldens('Member: Committee Registry', (tester) async {
      await tester.pumpWidgetBuilder(
        wrapInApp(const CommitteeScreen()),
        surfaceSize: const Size(390, 844),
      );
      await _pump(tester);
      await screenMatchesGolden(tester, 'committee_registry', customPump: _pump);
    });

    testGoldens('Directory: Member Listing', (tester) async {
      await tester.pumpWidgetBuilder(
        wrapInApp(const DirectoryScreen()),
        surfaceSize: const Size(390, 844),
      );
      await _pump(tester);
      await screenMatchesGolden(tester, 'directory_listing', customPump: _pump);
    });

    testGoldens('Events: Event Listing', (tester) async {
      await tester.pumpWidgetBuilder(
        wrapInApp(const EventsScreen()),
        surfaceSize: const Size(390, 844),
      );
      await _pump(tester);
      await screenMatchesGolden(tester, 'events_listing', customPump: _pump);
    });

    testGoldens('News: News Portal', (tester) async {
      await tester.pumpWidgetBuilder(
        wrapInApp(const NewsScreen()),
        surfaceSize: const Size(390, 844),
      );
      await _pump(tester);
      await screenMatchesGolden(tester, 'news_portal', customPump: _pump);
    });

    testGoldens('Financials: Payment Portal', (tester) async {
      await tester.pumpWidgetBuilder(
        wrapInApp(const FinancialPortalScreen()),
        surfaceSize: const Size(390, 844),
      );
      await _pump(tester);
      await screenMatchesGolden(tester, 'financial_portal', customPump: _pump);
    });

    testGoldens('Member: Dashboard Overview', (tester) async {
      await tester.pumpWidgetBuilder(
        wrapInApp(const DashboardScreen()),
        surfaceSize: const Size(390, 844),
      );
      await _pump(tester);
      await screenMatchesGolden(tester, 'dashboard_overview', customPump: _pump);
    });

    testGoldens('Member: Dashboard Overview (Tablet)', (tester) async {
      await tester.pumpWidgetBuilder(
        wrapInApp(const DashboardScreen()),
        surfaceSize: const Size(1024, 768),
      );
      await _pump(tester);
      await screenMatchesGolden(tester, 'dashboard_overview_tablet', customPump: _pump);
    });

    testGoldens('Member: Digital ID Pass', (tester) async {
      await tester.pumpWidgetBuilder(
        wrapInApp(const DigitalIDScreen()),
        surfaceSize: const Size(390, 844),
      );
      await _pump(tester);
      await screenMatchesGolden(tester, 'digital_id_pass', customPump: _pump);
    });
  });
}

class _MockHttpOverrides extends HttpOverrides {
  @override
  HttpClient createHttpClient(SecurityContext? context) {
    return _MockHttpClient();
  }
}

class _MockHttpClient implements HttpClient {
  @override
  bool autoUncompress = true;
  @override
  Duration? connectionTimeout;
  @override
  Duration idleTimeout = const Duration(seconds: 15);
  @override
  int? maxConnectionsPerHost;
  @override
  String? userAgent;

  @override
  void addCredentials(Uri url, String realm, HttpClientCredentials credentials) {}
  @override
  void addProxyCredentials(String host, int port, String realm, HttpClientCredentials credentials) {}
  @override
  set authenticate(Future<bool> Function(Uri url, String scheme, String realm)? f) {}
  @override
  set authenticateProxy(Future<bool> Function(String host, int port, String scheme, String realm)? f) {}
  @override
  set findProxy(String Function(Uri url)? f) {}
  @override
  set badCertificateCallback(bool Function(X509Certificate cert, String host, int port)? callback) {}
  @override
  void close({bool force = false}) {}
  @override
  Future<HttpClientRequest> delete(String host, int port, String path) => _mockRequest();
  @override
  Future<HttpClientRequest> deleteUrl(Uri url) => _mockRequest();
  @override
  Future<HttpClientRequest> get(String host, int port, String path) => _mockRequest();
  @override
  Future<HttpClientRequest> getUrl(Uri url) => _mockRequest();
  @override
  Future<HttpClientRequest> head(String host, int port, String path) => _mockRequest();
  @override
  Future<HttpClientRequest> headUrl(Uri url) => _mockRequest();
  @override
  Future<HttpClientRequest> patch(String host, int port, String path) => _mockRequest();
  @override
  Future<HttpClientRequest> patchUrl(Uri url) => _mockRequest();
  @override
  Future<HttpClientRequest> post(String host, int port, String path) => _mockRequest();
  @override
  Future<HttpClientRequest> postUrl(Uri url) => _mockRequest();
  @override
  Future<HttpClientRequest> put(String host, int port, String path) => _mockRequest();
  @override
  Future<HttpClientRequest> putUrl(Uri url) => _mockRequest();
  @override
  Future<HttpClientRequest> open(String method, String host, int port, String path) => _mockRequest();
  @override
  Future<HttpClientRequest> openUrl(String method, Uri url) => _mockRequest();
  @override
  set connectionFactory(Future<ConnectionTask<Socket>> Function(Uri url, String? proxyHost, int? proxyPort)? f) {}
  @override
  set keyLog(Function(String line)? callback) {}

  Future<HttpClientRequest> _mockRequest() async {
    return _MockHttpClientRequest();
  }
}

class _MockHttpClientRequest implements HttpClientRequest {
  @override
  bool followRedirects = true;
  @override
  int maxRedirects = 5;
  @override
  bool persistentConnection = true;
  @override
  final HttpHeaders headers = _MockHttpHeaders();

  @override
  void add(List<int> data) {}
  @override
  void addError(Object error, [StackTrace? stackTrace]) {}
  @override
  Future<void> addStream(Stream<List<int>> stream) async {}
  @override
  Future<HttpClientResponse> close() async => _MockHttpClientResponse();
  @override
  Future<HttpClientResponse> get done async => _MockHttpClientResponse();
  @override
  void write(Object? obj) {}
  @override
  void writeAll(Iterable objects, [String separator = ""]) {}
  @override
  void writeCharCode(int charCode) {}
  @override
  void writeln([Object? obj = ""]) {}
  @override
  set bufferOutput(bool bufferOutput) {}
  @override
  bool get bufferOutput => true;
  @override
  set contentLength(int contentLength) {}
  @override
  int get contentLength => 0;
  @override
  set encoding(Encoding encoding) {}
  @override
  Encoding get encoding => utf8;
  @override
  List<Cookie> get cookies => [];
  @override
  Uri get uri => Uri();
  @override
  String get method => 'GET';
  @override
  void abort([Object? exception, StackTrace? stackTrace]) {}
  @override
  HttpConnectionInfo? get connectionInfo => null;
  @override
  Future<void> flush() async {}
}

class _MockHttpClientResponse extends Stream<List<int>> implements HttpClientResponse {
  @override
  int get statusCode => 200;
  @override
  int get contentLength => 0;
  @override
  HttpClientResponseCompressionState get compressionState => HttpClientResponseCompressionState.notCompressed;
  @override
  final HttpHeaders headers = _MockHttpHeaders();
  @override
  final List<Cookie> cookies = [];
  @override
  bool get isRedirect => false;
  @override
  List<RedirectInfo> get redirects => [];
  @override
  Future<HttpClientResponse> redirect([String? method, Uri? url, bool? followRedirects]) async => this;
  @override
  String get reasonPhrase => 'OK';
  @override
  bool get persistentConnection => true;

  @override
  StreamSubscription<List<int>> listen(void Function(List<int> event)? onData, {Function? onError, void Function()? onDone, bool? cancelOnError}) {
    return Stream<List<int>>.fromIterable([]).listen(onData, onError: onError, onDone: onDone, cancelOnError: cancelOnError);
  }

  @override
  Future<Socket> detachSocket() async => _MockSocket();
  @override
  X509Certificate? get certificate => null;
  @override
  HttpConnectionInfo? get connectionInfo => null;
}

class _MockHttpHeaders implements HttpHeaders {
  @override
  bool chunkedTransferEncoding = false;
  @override
  int contentLength = 0;
  @override
  ContentType? contentType;
  @override
  DateTime? date;
  @override
  DateTime? expires;
  @override
  bool persistentConnection = true;
  @override
  DateTime? ifModifiedSince;
  @override
  set host(String? host) {}
  @override
  String? get host => null;
  @override
  set port(int? port) {}
  @override
  int? get port => null;
  @override
  List<String>? operator [](String name) => null;
  @override
  void add(String name, Object value, {bool preserveHeaderCase = false}) {}
  @override
  void clear() {}
  @override
  void forEach(void Function(String name, List<String> values) f) {}
  @override
  void noFolding(String name) {}
  @override
  void remove(String name, Object value) {}
  @override
  void removeAll(String name) {}
  @override
  void set(String name, Object value, {bool preserveHeaderCase = false}) {}
  @override
  String? value(String name) => null;
}

class _MockSocket extends Stream<Uint8List> implements Socket {
  @override
  InternetAddress get address => InternetAddress.loopbackIPv4;
  @override
  InternetAddress get remoteAddress => InternetAddress.loopbackIPv4;
  @override
  int get port => 0;
  @override
  int get remotePort => 0;
  @override
  void add(List<int> data) {}
  @override
  void addError(Object error, [StackTrace? stackTrace]) {}
  @override
  Future addStream(Stream<List<int>> stream) async {}
  @override
  Future close() async {}
  @override
  Future get done async {}
  @override
  void destroy() {}
  @override
  set encoding(Encoding encoding) {}
  @override
  Encoding get encoding => utf8;
  @override
  void write(Object? obj) {}
  @override
  void writeAll(Iterable objects, [String separator = ""]) {}
  @override
  void writeCharCode(int charCode) {}
  @override
  void writeln([Object? obj = ""]) {}
  @override
  bool setOption(SocketOption option, bool enabled) => true;
  @override
  void setRawOption(RawSocketOption option) {}
  @override
  Uint8List getRawOption(RawSocketOption option) => Uint8List(0);
  @override
  Future<void> flush() async {}

  @override
  StreamSubscription<Uint8List> listen(void Function(Uint8List event)? onData, {Function? onError, void Function()? onDone, bool? cancelOnError}) {
    return Stream<Uint8List>.fromIterable([]).listen(onData, onError: onError, onDone: onDone, cancelOnError: cancelOnError);
  }
}
