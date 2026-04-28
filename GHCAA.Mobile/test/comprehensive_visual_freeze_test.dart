import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:golden_toolkit/golden_toolkit.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter/services.dart';

// Core & Auth
import 'package:ghcaa_mobile/screens/app_home_screen.dart';
import 'package:ghcaa_mobile/screens/auth/login_screen.dart';
import 'package:ghcaa_mobile/features/auth/auth_service.dart';
import 'package:ghcaa_mobile/core/storage/storage_service.dart';

import 'package:ghcaa_mobile/screens/member/about_screen.dart';
import 'package:ghcaa_mobile/screens/member/activity_log_screen.dart';
import 'package:ghcaa_mobile/screens/member/ai_chat_screen.dart';
import 'package:ghcaa_mobile/screens/member/articles_screen.dart';
import 'package:ghcaa_mobile/screens/member/chat_room_screen.dart';
import 'package:ghcaa_mobile/screens/member/chats_screen.dart';
import 'package:ghcaa_mobile/screens/member/committee_screen.dart';
import 'package:ghcaa_mobile/screens/member/dashboard_screen.dart';
import 'package:ghcaa_mobile/features/admin/admin_service.dart';
import 'package:ghcaa_mobile/screens/member/digital_id_screen.dart';
import 'package:ghcaa_mobile/screens/member/directory_screen.dart';
import 'package:ghcaa_mobile/screens/member/event_details_screen.dart';
import 'package:ghcaa_mobile/screens/member/events_screen.dart';
import 'package:ghcaa_mobile/screens/member/family_link_screen.dart';
import 'package:ghcaa_mobile/screens/member/financial_portal_screen.dart';
import 'package:ghcaa_mobile/screens/member/gallery_screen.dart';
import 'package:ghcaa_mobile/screens/member/governance_screen.dart';
import 'package:ghcaa_mobile/screens/member/job_details_screen.dart';
import 'package:ghcaa_mobile/screens/member/jobs_screen.dart';
import 'package:ghcaa_mobile/screens/member/magazine_screen.dart';
import 'package:ghcaa_mobile/screens/member/member_activity_history_screen.dart';
import 'package:ghcaa_mobile/screens/member/member_details_screen.dart';
import 'package:ghcaa_mobile/screens/member/mentorship_hub_screen.dart';
import 'package:ghcaa_mobile/screens/member/news_details_screen.dart';
import 'package:ghcaa_mobile/screens/member/news_screen.dart';
import 'package:ghcaa_mobile/screens/member/notification_screen.dart';
import 'package:ghcaa_mobile/screens/member/professional_hub_screen.dart';
import 'package:ghcaa_mobile/screens/member/profile_edit_screen.dart';
import 'package:ghcaa_mobile/screens/member/profile_screen.dart';
import 'package:ghcaa_mobile/screens/member/submit_article_screen.dart';
import 'package:ghcaa_mobile/screens/member/support_screen.dart';
import 'package:ghcaa_mobile/screens/auth/register_screen.dart';
import 'package:ghcaa_mobile/features/polls/polls_screen.dart';

// Admin Screens
import 'package:ghcaa_mobile/screens/admin/admin_dashboard_screen.dart' hide adminAnalyticsProvider;
import 'package:ghcaa_mobile/screens/admin/admin_modules.dart';
import 'package:ghcaa_mobile/screens/admin/approval_queue_screen.dart';
import 'package:ghcaa_mobile/screens/admin/article_approval_screen.dart';
import 'package:ghcaa_mobile/screens/admin/audit_screen.dart';
import 'package:ghcaa_mobile/screens/admin/contact_messages_screen.dart';
import 'package:ghcaa_mobile/screens/admin/fee_config_screen.dart';
import 'package:ghcaa_mobile/screens/admin/gatekeeper_screen.dart';
import 'package:ghcaa_mobile/screens/admin/governance_registry_screen.dart';
import 'package:ghcaa_mobile/screens/admin/ledger_screen.dart';
import 'package:ghcaa_mobile/screens/admin/permissions_matrix_screen.dart';
import 'package:ghcaa_mobile/screens/admin/theme_management_screen.dart';

import 'package:ghcaa_mobile/features/lookups/dropdown_service.dart';
import 'package:ghcaa_mobile/features/files/file_service.dart';
import 'package:ghcaa_mobile/features/networking/networking_service.dart';
import 'package:ghcaa_mobile/features/events/events_service.dart';
import 'package:ghcaa_mobile/features/content/content_service.dart';
import 'package:ghcaa_mobile/features/jobs/job_service.dart';
import 'package:ghcaa_mobile/features/networking/mentorship_service.dart';
import 'package:ghcaa_mobile/features/messaging/chat_service.dart';
import 'package:ghcaa_mobile/features/notifications/notification_service.dart';
import 'package:ghcaa_mobile/features/financials/financial_service.dart';
import 'package:ghcaa_mobile/features/activity/activity_service.dart';
import 'package:ghcaa_mobile/features/polls/poll_service.dart';
import 'package:ghcaa_mobile/features/support/support_service.dart' hide familyServiceProvider, FamilyService;
import 'package:ghcaa_mobile/features/assistant/assistant_service.dart';
import 'package:ghcaa_mobile/features/admin/roles_service.dart';
import 'package:ghcaa_mobile/features/networking/family_service.dart';
import 'package:ghcaa_mobile/core/services/biometric_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:local_auth/local_auth.dart';
import 'package:image_picker/image_picker.dart';
import 'dart:io';

// --- FAKES ---
class FakeStorageService implements StorageService {
  @override
  Future<void> saveToken(String token) async {}
  @override
  Future<String?> getToken() async => 'mock-token';
  @override
  Future<void> removeToken() async {}
  @override
  Future<void> saveRole(String role) async {}
  @override
  Future<String?> getRole() async => 'Member';
  @override
  Future<void> saveDashboardLayout(bool isCompact) async {}
  @override
  Future<bool> getDashboardLayout() async => false;
  @override
  Future<void> saveProfile(Map<String, dynamic> profile) async {}
  @override
  Future<Map<String, dynamic>?> getProfile() async => null;
  @override
  Future<void> clearAll() async {}
  @override
  Future<void> saveCredentials(String username, String password) async {}
  @override
  Future<Map<String, String>?> getCredentials() async => null;
  @override
  Future<void> clearCredentials() async {}
}

class FakeDropdownService implements DropdownService {
  @override
  Future<List<Map<String, String>>> getOptions(String group) async {
    return [
      {'value': '1', 'label': 'Option A', 'instructions': 'Instruction text'},
      {'value': '2', 'label': 'Option B'},
    ];
  }
}

class FakeFileService implements FileService {
  @override
  Future<File?> pickImage({ImageSource source = ImageSource.gallery}) async => null;
  @override
  Future<String?> uploadProfilePhoto(File file) async => 'mock/photo.png';
  @override
  Future<String?> uploadArticleImage(File file) async => 'mock/article.png';
}

class FakeAdminService implements AdminService {
  @override
  Future<List<dynamic>> getPendingApprovals() async => [];
  @override
  Future<List<dynamic>> getContactMessages() async => [];
  @override
  Future<bool> markMessageAsRead(int messageId) async => true;
  @override
  Future<bool> resolveApproval(int memberId, bool approve, {required int adminId, String? reason}) async => true;
  @override
  Future<Map<String, dynamic>> getGlobalAnalytics() async => {
    'totalMembers': 1000,
    'pendingApprovals': 5,
    'totalEvents': 2,
  };
  @override
  Future<List<dynamic>> getECPeriods() async => [];
  @override
  Future<bool> createECPeriod(Map<String, dynamic> data) async => true;
  @override
  Future<bool> updateECPeriod(int id, Map<String, dynamic> data) async => true;
  @override
  Future<List<dynamic>> getLedgerRecords({String? search, int page = 1}) async => [];
  @override
  Future<Map<String, dynamic>> getLedgerSummary(int year) async => {'totalRevenue': 1000, 'totalExpenses': 500, 'netPosition': 500};
  @override
  Future<bool> updateMember(int id, Map<String, dynamic> data) async => true;
  @override
  Future<List<dynamic>> getFeeConfigs() async => [];
  @override
  Future<bool> addFeeConfig(Map<String, dynamic> data) async => true;
  @override
  Future<bool> updateFeeConfig(Map<String, dynamic> data) async => true;
  @override
  Future<List<dynamic>> getCommitteeMembers(int periodId) async => [];
  @override
  Future<bool> assignMemberToCommittee(int periodId, Map<String, dynamic> data) async => true;
  @override
  Future<bool> removeMemberFromCommittee(int ecMemberId) async => true;
}

class FakeAuthService implements AuthService {
  @override
  Future<String?> login(String identifier, String password, {bool enableBiometric = false}) async => null;
  @override
  Future<void> logout() async {}
  @override
  Future<String?> register(Map<String, dynamic> data) async => null;
  @override
  Future<bool> forgotPassword(String identifier) async => true;
  @override
  Future<String?> getRole() async => 'Member';
  @override
  Future<bool> updateProfile(Map<String, dynamic> data) async => true;
  @override
  Future<List<Map<String, dynamic>>> getSocialProviders() async => [];
  @override
  Future<String?> googleLogin(String idToken) async => null;
  @override
  Future<String?> facebookLogin(String accessToken) async => null;
}

class FakeNetworkingService implements NetworkingService {
  @override
  Future<Map<String, dynamic>> searchAlumni({String? query, String? batch, String? department, String? membershipType, String? category, int pageNumber = 1, int pageSize = 20}) async => {'items': [], 'totalItems': 0};
  @override
  Future<Map<String, dynamic>?> getProfile() async => null;
  @override
  Future<List<dynamic>> getECPeriods() async => [];
  @override
  Future<List<dynamic>> getExecutiveCommittee({int? periodId}) async => [];
  @override
  Future<bool> updateProfile(Map<String, dynamic> data) async => true;
}

class FakeEventsService implements EventsService {
  @override
  Future<List<dynamic>> getUpcomingEvents() async => [];
  @override
  Future<bool> registerForEvent(int eventId, {double? amount, String? paymentRef, dynamic receipt}) async => true;
  @override
  Future<bool> createEvent(Map<String, dynamic> data) async => true;
  @override
  Future<bool> deleteEvent(int id) async => true;
}

class FakeNewsService implements NewsService {
  @override
  Future<List<dynamic>> getLatestNews() async => [];
  @override
  Future<List<dynamic>> getNewsByCategory(String category) async => [];
  @override
  Future<List<dynamic>> getMySubmissions() async => [];
  @override
  Future<void> deleteMySubmission(int id) async {}
  @override
  Future<List<dynamic>> getPendingSubmissions() async => [];
  @override
  Future<bool> resolveArticle(int id, bool approve) async => true;
  @override
  Future<List<dynamic>> getGalleryItems() async => [];
  @override
  Future<bool> uploadGalleryItem(Map<String, dynamic> data) async => true;
}

class FakeGalleryService implements GalleryService {
  @override
  Future<List<dynamic>> getGalleries({bool onlyActive = true}) async => [];
  @override
  Future<bool> createGallery(Map<String, dynamic> gallery) async => true;
  @override
  Future<bool> updateGallery(int id, Map<String, dynamic> gallery) async => true;
  @override
  Future<bool> deleteGallery(int id) async => true;
  @override
  Future<String?> uploadPhoto(String filePath) async => 'mock/photo.jpg';
  @override
  Future<bool> addPhotosToGallery(int galleryId, List<String> paths) async => true;
  @override
  Future<bool> removePhoto(int photoId) async => true;
}

class FakeJobService implements JobService {
  @override
  Future<List<dynamic>> getAllJobs() async => [];
  @override
  Future<bool> postJob(Map<String, dynamic> data) async => true;
  @override
  Future<bool> deleteJob(int id) async => true;
}

class FakeMentorshipService implements MentorshipService {
  @override
  Future<bool> sendRequest(int mentorId, String? message, String? domain) async => true;
  @override
  Future<List<dynamic>> getSentRequests() async => [];
  @override
  Future<List<dynamic>> getReceivedRequests() async => [];
  @override
  Future<bool> respondToRequest(int requestId, bool accept, String? note) async => true;
  @override
  Future<bool> markComplete(int requestId) async => true;
}

class FakeChatService implements ChatService {
  @override
  Stream<Map<String, dynamic>> get messageStream => const Stream.empty();
  @override
  Future<void> initHub() async {}
  @override
  Future<void> sendDirectMessage(int receiverUserId, String message) async {}
  @override
  Future<List<dynamic>> getConversations() async => [];
  @override
  Future<List<dynamic>> getChatHistory(int otherUserId) async => [];
  @override
  void dispose() {}
}

class FakeNotificationService implements NotificationService {
  @override
  Future<List<dynamic>> getMyNotifications() async => [];
  @override
  Future<bool> markAsRead(int id) async => true;
}

class FakeFinancialService implements FinancialService {
  @override
  Future<List<dynamic>> getLedger() async => [];
  @override
  Future<double> getOutstandingDues() async => 0.0;
  @override
  Future<List<dynamic>> getSavedMethods() async => [];
  @override
  Future<bool> deleteSavedMethod(int id) async => true;
  @override
  Future<String?> getReceiptUrl(int paymentId) async => 'mock/receipt';
}

class FakeActivityService implements ActivityService {
  @override
  Future<List<dynamic>> getMyActivity() async => [];
  @override
  Future<List<dynamic>> getGlobalActivity() async => [];
  @override
  Future<List<dynamic>> getMemberActivity(int memberId) async => [];
}

class FakePollService implements PollService {
  @override
  Future<List<Poll>> getActivePolls() async => [];
  @override
  Future<bool> vote(int pollId, List<int> optionIds) async => true;
}

class FakeSupportService implements SupportService {
  @override
  Future<bool> checkSystemHealth() async => true;
  @override
  Future<bool> contactSupport(String message) async => true;
}

class FakeAssistantService implements AssistantService {
  @override
  Future<String> ask(String question) async => 'I am your AI assistant.';
}

class FakeRolesService implements RolesService {
  @override
  Future<List<dynamic>> getUsers() async => [];
  @override
  Future<bool> createAdmin(String username, String password, String role) async => true;
  @override
  Future<List<dynamic>> getRoles() async => [];
  @override
  Future<bool> createRole(String roleName) async => true;
  @override
  Future<bool> assignRole(int userId, String roleName) async => true;
  @override
  Future<bool> removeRole(int userId, String roleName) async => true;
}

class FakeFamilyService implements FamilyService {
  @override
  Future<List<dynamic>> getMyFamily() async => [];
  @override
  Future<List<dynamic>> getSentRequests() async => [];
  @override
  Future<List<dynamic>> getReceivedRequests() async => [];
  @override
  Future<bool> sendRequest(String membershipNo, int relationshipType, {String? note}) async => true;
  @override
  Future<bool> respondToRequest(int requestId, bool approve) async => true;
  @override
  Future<bool> cancelRequest(int requestId) async => true;
  @override
  Future<bool> removeLink(int requestId) async => true;
  @override
  Future<List<dynamic>> searchFamilyMembers(String name) async => [];
}

class FakeBiometricService implements BiometricService {
  @override
  Future<bool> isBiometricsAvailable() async => false;
  @override
  Future<List<BiometricType>> getAvailableBiometrics() async => [];
  @override
  Future<bool> authenticate({required String reason}) async => true;
}

Widget wrapInApp(Widget child, {List<Override> overrides = const []}) {
  final router = GoRouter(
    initialLocation: '/',
    routes: [
      GoRoute(
        path: '/',
        builder: (context, state) => child,
      ),
      // Dummy routes for any context.go calls
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
      galleryServiceProvider.overrideWith((ref) => FakeGalleryService()),
      jobServiceProvider.overrideWith((ref) => FakeJobService()),
      mentorshipServiceProvider.overrideWith((ref) => FakeMentorshipService()),
      chatServiceProvider.overrideWith((ref) => FakeChatService()),
      notificationServiceProvider.overrideWith((ref) => FakeNotificationService()),
      financialServiceProvider.overrideWith((ref) => FakeFinancialService()),
      activityServiceProvider.overrideWith((ref) => FakeActivityService()),
      pollServiceProvider.overrideWith((ref) => FakePollService()),
      supportServiceProvider.overrideWith((ref) => FakeSupportService()),
      assistantServiceProvider.overrideWith((ref) => FakeAssistantService()),
      rolesServiceProvider.overrideWith((ref) => FakeRolesService()),
      familyServiceProvider.overrideWith((ref) => FakeFamilyService()),
      biometricServiceProvider.overrideWith((ref) => FakeBiometricService()),
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
/// This prevents the semantics cascade (19 exceptions per test) that occurs
/// when pumpAndSettle loops indefinitely after any rendering overflow.
Future<void> _pump(WidgetTester tester) async {
  await tester.pump();                              // settle microtasks / FutureProviders
  await tester.pump(const Duration(milliseconds: 50)); // second raster frame
}

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() async {
    // Load real fonts so golden images have stable metrics
    await loadAppFonts();

    // Initialize dotenv for tests
    dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA\nPORTAL_SUBTITLE=ALUMNI');
    
    // Mock local_auth platform channel
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger.setMockMethodCallHandler(
      const MethodChannel('plugins.flutter.io/local_auth'),
      (methodCall) async {
        if (methodCall.method == 'getAvailableBiometrics') {
          return <String>[];
        }
        if (methodCall.method == 'isDeviceSupported') {
          return false;
        }
        return null;
      },
    );

    // Mock connectivity channel
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger.setMockMethodCallHandler(
      const MethodChannel('dev.fluttercommunity.plus/connectivity'),
      (methodCall) async {
        if (methodCall.method == 'check') {
          return 'wifi';
        }
        return null;
      },
    );

    // Mock mobile_scanner channel
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger.setMockMethodCallHandler(
      const MethodChannel('dev.steenbakker.mobile_scanner/scanner/method'),
      (methodCall) async {
        if (methodCall.method == 'state') {
          return 0; // CameraState.uninitialized
        }
        return null;
      },
    );
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger.setMockMethodCallHandler(
      const MethodChannel('dev.steenbakker.mobile_scanner/scanner/event'),
      (methodCall) async => null,
    );
  });

  group('Comprehensive Mobile Visual Freeze', () {
    
    // ---- CORE & AUTH ----
    testGoldens('Core: App Home / Welcome', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AppHomeScreen()));
      await screenMatchesGolden(tester, 'core_app_home', customPump: _pump);
    });

    testGoldens('Auth: Login Screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const LoginScreen()));
      await screenMatchesGolden(tester, 'auth_login', customPump: _pump);
    });

    testGoldens('Auth: Register Screen (Wizard)', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const RegisterScreen()));
      await screenMatchesGolden(tester, 'auth_register', customPump: _pump);
    });

    // ---- MEMBER PORTAL ----
    testGoldens('Member: Dashboard', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const DashboardScreen()));
      await screenMatchesGolden(tester, 'member_dashboard', customPump: _pump);
    });

    testGoldens('Member: Family Link', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const FamilyLinkScreen()));
      await screenMatchesGolden(tester, 'member_family_link', customPump: _pump);
    });

    testGoldens('Member: Activity History', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MemberActivityHistoryScreen()));
      await screenMatchesGolden(tester, 'member_activity_history', customPump: _pump);
    });

    testGoldens('Member: Digital ID Card', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const DigitalIDScreen()));
      await screenMatchesGolden(tester, 'member_digital_id', customPump: _pump);
    });

    testGoldens('Member: Directory', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const DirectoryScreen()));
      await screenMatchesGolden(tester, 'member_directory', customPump: _pump);
    });

    testGoldens('Member: Events Hub', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const EventsScreen()));
      await screenMatchesGolden(tester, 'member_events', customPump: _pump);
    });

    testGoldens('Member: News Feed', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const NewsScreen()));
      await screenMatchesGolden(tester, 'member_news', customPump: _pump);
    });

    testGoldens('Member: Magazine / Publications', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MagazineScreen()));
      await screenMatchesGolden(tester, 'member_magazine', customPump: _pump);
    });

    testGoldens('Member: Gallery Hub', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const GalleryScreen()));
      await screenMatchesGolden(tester, 'member_gallery', customPump: _pump);
    });

    testGoldens('Member: Job Hub', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const JobsScreen()));
      await screenMatchesGolden(tester, 'member_jobs', customPump: _pump);
    });

    testGoldens('Member: Mentorship Hub', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MentorshipHubScreen()));
      await screenMatchesGolden(tester, 'member_mentorship', customPump: _pump);
    });

    testGoldens('Member: Professional Hub', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ProfessionalHubScreen()));
      await screenMatchesGolden(tester, 'member_professional_hub', customPump: _pump);
    });

    testGoldens('Member: Governance (Committee)', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const GovernanceScreen()));
      await screenMatchesGolden(tester, 'member_governance', customPump: _pump);
    });

    testGoldens('Member: Committee Detail', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const CommitteeScreen()));
      await screenMatchesGolden(tester, 'member_committee', customPump: _pump);
    });

    testGoldens('Member: Financial Portal', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const FinancialPortalScreen()));
      await screenMatchesGolden(tester, 'member_financials', customPump: _pump);
    });

    testGoldens('Member: My Profile', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ProfileScreen()));
      await screenMatchesGolden(tester, 'member_profile', customPump: _pump);
    });

    testGoldens('Member: Profile Edit', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ProfileEditScreen()));
      await screenMatchesGolden(tester, 'member_profile_edit', customPump: _pump);
    });

    testGoldens('Member: My Articles', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MemberArticlesScreen()));
      await screenMatchesGolden(tester, 'member_articles', customPump: _pump);
    });

    testGoldens('Member: Submit Article', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const SubmitArticleScreen()));
      await screenMatchesGolden(tester, 'member_submit_article', customPump: _pump);
    });

    testGoldens('Member: Chats List', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ChatsScreen()));
      await screenMatchesGolden(tester, 'member_chats_list', customPump: _pump);
    });

    testGoldens('Member: Chat Room', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ChatRoomScreen(otherUserId: 123)));
      await screenMatchesGolden(tester, 'member_chat_room', customPump: _pump);
    });

    testGoldens('Member: Notifications', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const NotificationScreen()));
      await screenMatchesGolden(tester, 'member_notifications', customPump: _pump);
    });

    testGoldens('Member: Activity Log', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ActivityLogScreen()));
      await screenMatchesGolden(tester, 'member_activity_log', customPump: _pump);
    });

    testGoldens('Member: AI Assistant', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AIChatScreen()));
      await screenMatchesGolden(tester, 'member_ai_assistant', customPump: _pump);
    });

    testGoldens('Member: Polls Hub', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const PollsScreen()));
      await screenMatchesGolden(tester, 'member_polls', customPump: _pump);
    });

    testGoldens('Member: Support / Helpdesk', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const SupportScreen()));
      await screenMatchesGolden(tester, 'member_support', customPump: _pump);
    });

    testGoldens('Member: About GHCAA', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AboutScreen()));
      await screenMatchesGolden(tester, 'member_about', customPump: _pump);
    });

    // ---- DETAIL SCREENS ----
    testGoldens('Member: Chat Room detail', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ChatRoomScreen(otherUserId: 123)));
      await screenMatchesGolden(tester, 'member_chat_room', customPump: _pump);
    });

    testGoldens('Member: Event detail view', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const EventDetailsScreen(eventId: 456)));
      await screenMatchesGolden(tester, 'member_event_details', customPump: _pump);
    });

    testGoldens('Member: Job detail view', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const JobDetailsScreen(jobId: 789)));
      await screenMatchesGolden(tester, 'member_job_details', customPump: _pump);
    });

    testGoldens('Member: Alumni detail view', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MemberDetailsScreen(memberId: 101)));
      await screenMatchesGolden(tester, 'member_details_view', customPump: _pump);
    });

    testGoldens('Member: News detail view', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const NewsDetailsScreen(newsId: 321)));
      await screenMatchesGolden(tester, 'member_news_details', customPump: _pump);
    });

    // ---- ADMIN SCREENS ----
    testGoldens('Admin: Dashboard screen', (tester) async {
      final overrides = [
        adminAnalyticsProvider.overrideWith((ref) => {
          'totalMembers': 1250,
          'pendingApprovals': 15,
          'activeEvents': 3,
        }),
        roleProvider.overrideWith((ref) => Future.value('Admin')),
      ];
      await tester.pumpWidgetBuilder(wrapInApp(const AdminDashboardScreen(), overrides: overrides));
      await screenMatchesGolden(tester, 'admin_dashboard', customPump: _pump);
    });

    testGoldens('Admin: Modules registry screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AdminCMS()));
      await screenMatchesGolden(tester, 'admin_modules_registry', customPump: _pump);
    });

    testGoldens('Admin: Approval Queue screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ApprovalQueueScreen()));
      await screenMatchesGolden(tester, 'admin_approval_queue', customPump: _pump);
    });

    testGoldens('Admin: Article Approval screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ArticleApprovalScreen()));
      await screenMatchesGolden(tester, 'admin_article_approval', customPump: _pump);
    });

    testGoldens('Admin: Audit Logs screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AdminAuditScreen()));
      await screenMatchesGolden(tester, 'admin_audit_logs', customPump: _pump);
    });

    testGoldens('Admin: Contact Messages screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ContactMessagesScreen()));
      await screenMatchesGolden(tester, 'admin_contact_messages', customPump: _pump);
    });

    testGoldens('Admin: Fee Configuration screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const FeeConfigScreen()));
      await screenMatchesGolden(tester, 'admin_fee_config', customPump: _pump);
    });

    testGoldens('Admin: Gatekeeper screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const GatekeeperScreen()));
      await screenMatchesGolden(tester, 'admin_gatekeeper', customPump: _pump);
    });

    testGoldens('Admin: Governance Registry screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AdminGovernanceScreen()));
      await screenMatchesGolden(tester, 'admin_governance_registry', customPump: _pump);
    });

    testGoldens('Admin: Financial Ledger screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AdminLedgerScreen()));
      await screenMatchesGolden(tester, 'admin_ledger', customPump: _pump);
    });

    testGoldens('Admin: Permissions Matrix screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const PermissionsMatrixScreen()));
      await screenMatchesGolden(tester, 'admin_permissions_matrix', customPump: _pump);
    });

    testGoldens('Admin: Theme Management screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ThemeManagementScreen()));
      await screenMatchesGolden(tester, 'admin_theme_management', customPump: _pump);
    });
    group('Specific Layout Tests', () {
      testGoldens('Admin Dashboard: Mobile Viewport', (tester) async {
        // Use DeviceBuilder for proper viewport simulation
        final builder = DeviceBuilder()
          ..addScenario(
            name: 'Admin Dashboard',
            widget: const AdminDashboardScreen(),
          );
        
        await tester.pumpDeviceBuilder(builder, wrapper: (child) => wrapInApp(child));
        await screenMatchesGolden(tester, 'admin_dashboard_multi_device', customPump: _pump);
      });
    });
  });
}


