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
import 'package:ghcaa_mobile/features/support/support_service.dart';
import 'package:ghcaa_mobile/features/assistant/assistant_service.dart';
import 'package:ghcaa_mobile/features/admin/roles_service.dart';
import 'package:ghcaa_mobile/features/networking/family_service.dart';
import 'package:ghcaa_mobile/core/services/biometric_service.dart';
import 'package:ghcaa_mobile/core/theme/app_theme.dart';
import 'package:ghcaa_mobile/features/theme/dynamic_theme_service.dart';

import 'helpers/fake_services.dart';
import 'helpers/golden_test_utils.dart';

// --- FAKES ---
// FakeDropdownService (2-option fixture, differs from full_app's empty one),
// FakeGalleryService/FakeAdminGalleryService and FakeAssistantService (its
// reply text differs from full_app's) are only used here, so they stay local
// instead of moving into helpers/fake_services.dart.
class FakeDropdownService implements DropdownService {
  @override
  Future<List<Map<String, String>>> getOptions(String group) async {
    return [
      {'value': '1', 'label': 'Option A', 'instructions': 'Instruction text'},
      {'value': '2', 'label': 'Option B'},
    ];
  }
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
  Future<bool?> toggleActive(int id) async => true;
  @override
  Future<bool?> toggleFeatured(int id) async => true;
  @override
  Future<String?> uploadPhoto(String filePath) async => 'mock/photo.jpg';
  @override
  Future<bool> addPhotosToGallery(int galleryId, List<String> paths) async => true;
  @override
  Future<bool> removePhoto(int photoId) async => true;
  @override
  Future<bool> createAlbum(String title, String? description) async => true;
  @override
  Future<List<dynamic>> getMyAlbums() async => [];
  @override
  Future<bool> addPhotoToAlbum(int albumId, String filePath, {String? caption}) async => true;
  @override
  Future<Map<String, List<dynamic>>> getPendingGalleryApprovals() async => {'galleries': [], 'photos': []};
  @override
  Future<bool> resolveGalleryApproval(int id, bool approve, {String? reason}) async => true;
  @override
  Future<bool> resolvePhotoApproval(int photoId, bool approve, {String? reason}) async => true;
}

/// Returns one gallery with photos and captures toggle-call arguments, so the
/// admin active/featured/edit controls in [GalleryScreen] have something to
/// render against and can be asserted on without hitting a real Dio client.
class FakeAdminGalleryService extends FakeGalleryService {
  int? toggleActiveCalledWith;
  int? toggleFeaturedCalledWith;

  @override
  Future<List<dynamic>> getGalleries({bool onlyActive = true}) async => [
        {
          'id': 1,
          'title': 'Annual Picnic',
          'description': 'A day at the campus grounds.',
          'eventDate': '2026-01-01T00:00:00Z',
          'location': 'Campus Grounds',
          'isActive': true,
          'isFeatured': false,
          'photos': [
            {'id': 1, 'photoPath': '/assets/gallery/annual-picnic/01.jpg', 'uploadedAt': '2026-01-01T00:00:00Z'},
          ],
        },
      ];

  @override
  Future<bool?> toggleActive(int id) async {
    toggleActiveCalledWith = id;
    return false;
  }

  @override
  Future<bool?> toggleFeatured(int id) async {
    toggleFeaturedCalledWith = id;
    return true;
  }
}

class FakeAssistantService implements AssistantService {
  @override
  Future<String> ask(String question) async => 'I am your AI assistant.';
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
      // Resolve the special-theme future to null (= default theme) so no test
      // depends on a live `/Theme/active` Dio call. Without this it stays in the
      // loading state through the 2-frame pump; ThemeManagementScreen then renders
      // its `LogoSpinner` loading branch (infinite AnimationController.repeat() +
      // an un-precached Image.asset) which throws during pump on Linux CI only.
      activeSpecialThemeProvider.overrideWith((ref) async => null),
      ...overrides,
    ],
    child: MaterialApp.router(
      routerConfig: router,
      debugShowCheckedModeBanner: false,
      theme: AppTheme.midnightTheme,
    ),
  );
}

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  setUpAll(() async {
    // Load real fonts so golden images have stable metrics
    await loadAppFonts();

    // Initialize dotenv for tests
    dotenv.testLoad(fileInput: 'PORTAL_TITLE=GHCAA\nPORTAL_SUBTITLE=ALUMNI');

    mockLocalAuthChannel();

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
      await screenMatchesGolden(tester, 'core_app_home', customPump: pumpAndSettleShort);
    });

    testGoldens('Auth: Login Screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const LoginScreen()));
      await screenMatchesGolden(tester, 'auth_login', customPump: pumpAndSettleShort);
    });

    testGoldens('Auth: Register Screen (Wizard)', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const RegisterScreen()));
      await screenMatchesGolden(tester, 'auth_register', customPump: pumpAndSettleShort);
    });

    // ---- MEMBER PORTAL ----
    testGoldens('Member: Dashboard', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const DashboardScreen()));
      await screenMatchesGolden(tester, 'member_dashboard', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Family Link', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const FamilyLinkScreen()));
      await screenMatchesGolden(tester, 'member_family_link', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Activity History', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MemberActivityHistoryScreen()));
      await screenMatchesGolden(tester, 'member_activity_history', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Digital ID Card', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const DigitalIDScreen()));
      await screenMatchesGolden(tester, 'member_digital_id', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Directory', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const DirectoryScreen()));
      await screenMatchesGolden(tester, 'member_directory', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Events Hub', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const EventsScreen()));
      await screenMatchesGolden(tester, 'member_events', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: News Feed', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const NewsScreen()));
      await screenMatchesGolden(tester, 'member_news', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Magazine / Publications', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MagazineScreen()));
      await screenMatchesGolden(tester, 'member_magazine', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Gallery Hub', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const GalleryScreen()));
      await screenMatchesGolden(tester, 'member_gallery', customPump: pumpAndSettleShort);
    });

    testWidgets('Admin: Gallery Hub shows active/featured toggles and wires them to the service', (tester) async {
      final gallery = FakeAdminGalleryService();
      await tester.pumpWidget(wrapInApp(
        const GalleryScreen(),
        overrides: [
          roleProvider.overrideWith((ref) => Future.value('Admin')),
          galleryServiceProvider.overrideWith((ref) => gallery),
        ],
      ));
      await pumpAndSettleShort(tester);

      expect(find.byIcon(Icons.visibility_rounded), findsOneWidget);
      expect(find.byIcon(Icons.star_border_rounded), findsOneWidget);
      expect(find.byIcon(Icons.edit_rounded), findsOneWidget);

      await tester.tap(find.byIcon(Icons.visibility_rounded));
      await pumpAndSettleShort(tester);
      expect(gallery.toggleActiveCalledWith, 1);

      await tester.tap(find.byIcon(Icons.star_border_rounded));
      await pumpAndSettleShort(tester);
      expect(gallery.toggleFeaturedCalledWith, 1);
    });

    testGoldens('Member: Job Hub', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const JobsScreen()));
      await screenMatchesGolden(tester, 'member_jobs', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Mentorship Hub', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MentorshipHubScreen()));
      await screenMatchesGolden(tester, 'member_mentorship', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Professional Hub', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ProfessionalHubScreen()));
      await screenMatchesGolden(tester, 'member_professional_hub', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Governance (Committee)', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const GovernanceScreen()));
      await screenMatchesGolden(tester, 'member_governance', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Committee Detail', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const CommitteeScreen()));
      await screenMatchesGolden(tester, 'member_committee', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Financial Portal', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const FinancialPortalScreen()));
      await screenMatchesGolden(tester, 'member_financials', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: My Profile', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ProfileScreen()));
      await screenMatchesGolden(tester, 'member_profile', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Profile Edit', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ProfileEditScreen()));
      await screenMatchesGolden(tester, 'member_profile_edit', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: My Articles', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MemberArticlesScreen()));
      await screenMatchesGolden(tester, 'member_articles', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Submit Article', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const SubmitArticleScreen()));
      await screenMatchesGolden(tester, 'member_submit_article', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Chats List', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ChatsScreen()));
      await screenMatchesGolden(tester, 'member_chats_list', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Chat Room', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ChatRoomScreen(otherUserId: 123)));
      await screenMatchesGolden(tester, 'member_chat_room', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Notifications', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const NotificationScreen()));
      await screenMatchesGolden(tester, 'member_notifications', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Activity Log', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ActivityLogScreen()));
      await screenMatchesGolden(tester, 'member_activity_log', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: AI Assistant', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AIChatScreen()));
      await screenMatchesGolden(tester, 'member_ai_assistant', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Polls Hub', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const PollsScreen()));
      await screenMatchesGolden(tester, 'member_polls', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Support / Helpdesk', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const SupportScreen()));
      await screenMatchesGolden(tester, 'member_support', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: About GHCAA', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AboutScreen()));
      await screenMatchesGolden(tester, 'member_about', customPump: pumpAndSettleShort);
    });

    // ---- DETAIL SCREENS ----
    testGoldens('Member: Chat Room detail', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ChatRoomScreen(otherUserId: 123)));
      await screenMatchesGolden(tester, 'member_chat_room', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Event detail view', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const EventDetailsScreen(eventId: 456)));
      await screenMatchesGolden(tester, 'member_event_details', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Job detail view', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const JobDetailsScreen(jobId: 789)));
      await screenMatchesGolden(tester, 'member_job_details', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: Alumni detail view', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MemberDetailsScreen(memberId: 101)));
      await screenMatchesGolden(tester, 'member_details_view', customPump: pumpAndSettleShort);
    });

    testGoldens('Member: News detail view', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const NewsDetailsScreen(newsId: 321)));
      await screenMatchesGolden(tester, 'member_news_details', customPump: pumpAndSettleShort);
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
      await screenMatchesGolden(tester, 'admin_dashboard', customPump: pumpAndSettleShort);
    });

    testGoldens('Admin: Modules registry screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AdminCMS()));
      await screenMatchesGolden(tester, 'admin_modules_registry', customPump: pumpAndSettleShort);
    });

    testGoldens('Admin: Approval Queue screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ApprovalQueueScreen()));
      await screenMatchesGolden(tester, 'admin_approval_queue', customPump: pumpAndSettleShort);
    });

    testGoldens('Admin: Article Approval screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ArticleApprovalScreen()));
      await screenMatchesGolden(tester, 'admin_article_approval', customPump: pumpAndSettleShort);
    });

    testGoldens('Admin: Audit Logs screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AdminAuditScreen()));
      await screenMatchesGolden(tester, 'admin_audit_logs', customPump: pumpAndSettleShort);
    });

    testGoldens('Admin: Contact Messages screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ContactMessagesScreen()));
      await screenMatchesGolden(tester, 'admin_contact_messages', customPump: pumpAndSettleShort);
    });

    testGoldens('Admin: Fee Configuration screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const FeeConfigScreen()));
      await screenMatchesGolden(tester, 'admin_fee_config', customPump: pumpAndSettleShort);
    });

    testGoldens('Admin: Gatekeeper screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const GatekeeperScreen()));
      await screenMatchesGolden(tester, 'admin_gatekeeper', customPump: pumpAndSettleShort);
    });

    testGoldens('Admin: Governance Registry screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AdminGovernanceScreen()));
      await screenMatchesGolden(tester, 'admin_governance_registry', customPump: pumpAndSettleShort);
    });

    testGoldens('Admin: Financial Ledger screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AdminLedgerScreen()));
      await screenMatchesGolden(tester, 'admin_ledger', customPump: pumpAndSettleShort);
    });

    testGoldens('Admin: Permissions Matrix screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const PermissionsMatrixScreen()));
      await screenMatchesGolden(tester, 'admin_permissions_matrix', customPump: pumpAndSettleShort);
    });

    testGoldens('Admin: Theme Management screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ThemeManagementScreen()));
      await screenMatchesGolden(tester, 'admin_theme_management', customPump: pumpAndSettleShort);
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
        await screenMatchesGolden(tester, 'admin_dashboard_multi_device', customPump: pumpAndSettleShort);
      });
    });
  });
}


