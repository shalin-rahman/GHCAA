import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../screens/app_home_screen.dart';
import '../../screens/auth/login_screen.dart';
import '../../screens/auth/register_screen.dart';
import '../../screens/auth/forgot_password_screen.dart';
import '../../screens/member/dashboard_screen.dart';
import '../../screens/member/directory_screen.dart';
import '../../screens/member/member_details_screen.dart';
import '../../screens/member/digital_id_screen.dart';
import '../../screens/member/events_screen.dart';
import '../../screens/member/event_details_screen.dart';
import '../../screens/member/jobs_screen.dart';
import '../../screens/member/job_details_screen.dart';
import '../../screens/member/profile_screen.dart';
import '../../screens/member/profile_edit_screen.dart';
import '../../screens/member/financial_portal_screen.dart';
import '../../screens/financials/payment_web_page.dart';
import '../../screens/member/news_screen.dart';
import '../../screens/member/news_details_screen.dart';
import '../../screens/member/gallery_screen.dart';
import '../../screens/member/notification_screen.dart';
import '../../screens/member/communications_screen.dart';
import '../../screens/member/member_activity_history_screen.dart';
import '../../screens/member/family_link_screen.dart';
import '../../screens/member/support_screen.dart';
import '../../screens/member/articles_screen.dart';
import '../../screens/member/mentorship_hub_screen.dart';
import '../../screens/member/campaigns_screen.dart';
import '../../screens/member/campaign_detail_screen.dart';
import '../../screens/member/my_pledges_screen.dart';
import '../../screens/member/professional_hub_screen.dart';
import '../../screens/member/submit_article_screen.dart';
import '../../screens/member/magazine_screen.dart';
import '../../screens/member/about_screen.dart';
import '../storage/storage_service.dart';
import '../../screens/admin/approval_queue_screen.dart';
import '../../screens/admin/audit_screen.dart';
import '../../screens/admin/theme_management_screen.dart';
import '../../screens/admin/fee_config_screen.dart';
import '../../screens/admin/contact_messages_screen.dart';
import '../../screens/admin/admin_dashboard_screen.dart';
import '../../screens/admin/governance_registry_screen.dart';
import '../../screens/admin/election_management_screen.dart';
import '../../screens/admin/article_approval_screen.dart';
import '../../screens/admin/gallery_approval_screen.dart';
import '../../screens/admin/job_approval_screen.dart';
import '../../screens/admin/gatekeeper_screen.dart';
import '../../screens/admin/ledger_screen.dart';
import '../../screens/admin/admin_modules.dart';
import '../../screens/admin/permissions_matrix_screen.dart';
import '../../screens/member/ai_chat_screen.dart';
import '../../screens/member/chats_screen.dart';
import '../../screens/member/chat_room_screen.dart';
import '../../screens/member/governance_screen.dart';
import '../../core/widgets/main_shell.dart';
import '../../features/polls/polls_screen.dart';
import '../../screens/member/election_screen.dart';
import '../../screens/member/forum/forum_categories_screen.dart';
import '../../screens/member/forum/forum_topics_screen.dart';
import '../../screens/member/forum/forum_topic_detail_screen.dart';
import '../../screens/member/legacy_archive_screen.dart';
import '../../screens/credential_verification_screen.dart';

class AuthNotifier extends ChangeNotifier {
  final Ref _ref;
  String? _token;

  AuthNotifier(this._ref) {
    _ref.listen(authStateProvider, (previous, next) {
      if (next.value != _token) {
        _token = next.value;
        notifyListeners();
      }
    });
  }

  String? get token => _token;
}

final authNotifierProvider = Provider<AuthNotifier>((ref) => AuthNotifier(ref));

final authStateProvider = StreamProvider<String?>((ref) async* {
  final storage = ref.watch(storageServiceProvider);
  // Stream.periodic doesn't fire until the first interval elapses, so a plain
  // periodic stream leaves a 2s window where a fresh subscriber (e.g. right
  // after logout invalidates this provider) still sees the old token. Yield
  // the current value immediately, then fall back to polling for expiry.
  String? last = await storage.getToken();
  yield last;
  await for (final _ in Stream.periodic(const Duration(seconds: 2))) {
    final next = await storage.getToken();
    if (next != last) {
      last = next;
      yield next;
    }
  }
});

final routerProvider = Provider<GoRouter>((ref) {
  final authNotifier = ref.watch(authNotifierProvider);

  return GoRouter(
    initialLocation: '/',
    refreshListenable: authNotifier,
    redirect: (context, state) {
      final token = authNotifier.token;
      final isLoggingIn = state.uri.path == '/login' || state.uri.path == '/register' || state.uri.path == '/forgot-password' || state.uri.path == '/' || state.uri.path == '/verify' || state.uri.path.startsWith('/verify/');
      
      if (token == null && !isLoggingIn) {
        return '/login';
      }
      
      // If logged in and hitting landing/login/register, go to appropriate dashboard
      if (token != null && isLoggingIn) {
        return '/dashboard'; // Login screen logic handles specific role redirection
      }

      return null;
    },
    routes: [
      GoRoute(
        path: '/', 
        name: 'home', 
        pageBuilder: (context, state) => CustomTransitionPage(
          key: state.pageKey,
          child: const AppHomeScreen(),
          transitionsBuilder: (context, animation, secondaryAnimation, child) => 
            FadeTransition(opacity: animation, child: child),
        ),
      ),
      GoRoute(path: '/login', name: 'login', builder: (context, state) => const LoginScreen()),
      GoRoute(path: '/register', name: 'register', builder: (context, state) => const RegisterScreen()),
      GoRoute(path: '/forgot-password', name: 'forgot-password', builder: (context, state) => const ForgotPasswordScreen()),
      GoRoute(
        path: '/verify',
        name: 'verify',
        builder: (context, state) => const CredentialVerificationScreen(),
      ),
      GoRoute(
        path: '/verify/:shortCode',
        name: 'verify_code',
        builder: (context, state) => CredentialVerificationScreen(
          initialCode: state.pathParameters['shortCode'],
        ),
      ),
      
      ShellRoute(
        builder: (context, state, child) => MainShell(child: child),
        routes: [
          GoRoute(path: '/dashboard', name: 'dashboard', builder: (context, state) => const DashboardScreen()),
          GoRoute(path: '/directory', name: 'directory', builder: (context, state) => const DirectoryScreen()),
          GoRoute(path: '/digital_id', name: 'digital_id', builder: (context, state) => const DigitalIDScreen()),
          GoRoute(path: '/profile', name: 'profile', builder: (context, state) => const ProfileScreen()),
          GoRoute(path: '/profile/edit', name: 'profile_edit', builder: (context, state) => ProfileEditScreen(initialData: state.extra as Map<String, dynamic>?)),
          GoRoute(
            path: '/directory/:id',
            name: 'member_details',
            builder: (context, state) => MemberDetailsScreen(memberId: int.parse(state.pathParameters['id']!)),
          ),
          GoRoute(path: '/events', name: 'events', builder: (context, state) => const EventsScreen()),
          GoRoute(
            path: '/events/:id',
            name: 'event_details',
            builder: (context, state) => EventDetailsScreen(eventId: int.parse(state.pathParameters['id']!)),
          ),
          GoRoute(path: '/jobs', name: 'jobs', builder: (context, state) => const JobsScreen()),
          GoRoute(
            path: '/jobs/:id',
            name: 'job_details',
            builder: (context, state) => JobDetailsScreen(jobId: int.parse(state.pathParameters['id']!)),
          ),
          GoRoute(path: '/financials', name: 'financials', builder: (context, state) => const FinancialPortalScreen()),
          GoRoute(
            path: '/financials/payment',
            name: 'payment_web',
            builder: (context, state) => PaymentWebPage(url: state.extra as String),
          ),
          GoRoute(path: '/news', name: 'news', builder: (context, state) => const NewsScreen()),
          GoRoute(path: '/legacy', name: 'legacy', builder: (context, state) => const LegacyArchiveScreen()),
          GoRoute(
            path: '/news/:id',
            name: 'news_details',
            builder: (context, state) => NewsDetailsScreen(newsId: int.parse(state.pathParameters['id']!)),
          ),
          GoRoute(path: '/gallery', name: 'gallery', builder: (context, state) => const GalleryScreen()),
          GoRoute(path: '/committee', name: 'governance', builder: (context, state) => const GovernanceScreen()),
          GoRoute(path: '/notifications', name: 'notifications', builder: (context, state) => const NotificationScreen()),
          GoRoute(path: '/communications', name: 'communications', builder: (context, state) => const CommunicationsScreen()),
          GoRoute(path: '/activity', name: 'activity', builder: (context, state) => const MemberActivityHistoryScreen()),
          GoRoute(path: '/family', name: 'family', builder: (context, state) => const FamilyLinkScreen()),
          GoRoute(path: '/support', name: 'support', builder: (context, state) => const SupportScreen()),
          GoRoute(path: '/articles', name: 'articles', builder: (context, state) => const MemberArticlesScreen()),
          GoRoute(path: '/submit_article', name: 'submit_article', builder: (context, state) => const SubmitArticleScreen()),
          GoRoute(path: '/magazine', name: 'magazine', builder: (context, state) => const MagazineScreen()),
          GoRoute(path: '/about', name: 'about', builder: (context, state) => const AboutScreen()),
          GoRoute(path: '/mentorship', name: 'mentorship', builder: (context, state) => const MentorshipHubScreen()),
          GoRoute(path: '/campaigns', name: 'campaigns', builder: (context, state) => const CampaignsScreen()),
          GoRoute(path: '/campaigns/my-pledges', name: 'campaigns_my_pledges', builder: (context, state) => const MyPledgesScreen()),
          GoRoute(
            path: '/campaigns/:slug',
            name: 'campaign_detail',
            builder: (context, state) => CampaignDetailScreen(slug: state.pathParameters['slug']!),
          ),
          GoRoute(path: '/professionals', name: 'professionals', builder: (context, state) => const ProfessionalHubScreen()),
          GoRoute(path: '/polls', name: 'polls', builder: (context, state) => const PollsScreen()),
          GoRoute(path: '/election', name: 'election', builder: (context, state) => const ElectionScreen()),
          GoRoute(path: '/messages', name: 'messages', builder: (context, state) => const ChatsScreen()),
          GoRoute(path: '/forum', name: 'forum', builder: (context, state) => const ForumCategoriesScreen()),
          GoRoute(
            path: '/forum/topics/:id',
            name: 'forum_topics',
            builder: (context, state) => ForumTopicsScreen(categoryId: int.parse(state.pathParameters['id']!)),
          ),
          GoRoute(
            path: '/forum/topic/:id',
            name: 'forum_topic_detail',
            builder: (context, state) => ForumTopicDetailScreen(topicId: int.parse(state.pathParameters['id']!)),
          ),
          
          // Admin Routes
          GoRoute(path: '/admin_dashboard', name: 'admin_dashboard', builder: (context, state) => const AdminDashboardScreen()),
          GoRoute(path: '/admin/approvals', name: 'admin_approvals', builder: (context, state) => const ApprovalQueueScreen()),
          GoRoute(path: '/admin/audit', name: 'admin_audit', builder: (context, state) => const AdminAuditScreen()),
          GoRoute(path: '/admin/ledger', name: 'admin_ledger', builder: (context, state) => const AdminLedgerScreen()),
          GoRoute(path: '/admin/themes', name: 'admin_themes', builder: (context, state) => const ThemeManagementScreen()),
          GoRoute(path: '/admin/fees', name: 'admin_fees', builder: (context, state) => const FeeConfigScreen()),
          GoRoute(path: '/admin/messages', name: 'admin_messages', builder: (context, state) => const ContactMessagesScreen()),
          GoRoute(path: '/admin/gatekeeper', name: 'admin_gatekeeper', builder: (context, state) => const GatekeeperScreen()),
          GoRoute(path: '/admin/governance', name: 'admin_governance', builder: (context, state) => const AdminGovernanceScreen()),
          GoRoute(path: '/admin/elections', name: 'admin_elections', builder: (context, state) => const ElectionManagementScreen()),
          GoRoute(path: '/admin/articles', name: 'admin_articles', builder: (context, state) => const ArticleApprovalScreen()),
          GoRoute(path: '/admin/gallery-approvals', name: 'admin_gallery_approvals', builder: (context, state) => const GalleryApprovalScreen()),
          GoRoute(path: '/admin/job-approvals', name: 'admin_job_approvals', builder: (context, state) => const JobApprovalScreen()),
          GoRoute(path: '/admin/permissions', name: 'admin_permissions', builder: (context, state) => const PermissionsMatrixScreen()),
          GoRoute(path: '/admin/cms', name: 'admin_cms', builder: (context, state) => const AdminCMS()),
          GoRoute(path: '/chats', name: 'chats', builder: (context, state) => const ChatsScreen()),
          GoRoute(
            path: '/chat/:id',
            name: 'chat_room',
            builder: (context, state) => ChatRoomScreen(otherUserId: int.parse(state.pathParameters['id']!)),
          ),
          GoRoute(path: '/assistant', name: 'assistant', builder: (context, state) => const AIChatScreen()),
        ],
      ),
    ],
  );
});
