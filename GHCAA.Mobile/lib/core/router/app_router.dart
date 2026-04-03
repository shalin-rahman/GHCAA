import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../screens/app_home_screen.dart';
import '../../screens/auth/login_screen.dart';
import '../../screens/auth/register_screen.dart';
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
import '../../screens/member/news_screen.dart';
import '../../screens/member/news_details_screen.dart';
import '../../screens/member/gallery_screen.dart';
import '../../screens/member/committee_screen.dart';
import '../../screens/member/notification_screen.dart';
import '../../screens/member/member_activity_history_screen.dart';
import '../../screens/member/family_link_screen.dart';
import '../../screens/member/support_screen.dart';
import '../../screens/member/articles_screen.dart';
import '../../screens/member/submit_article_screen.dart';
import '../../screens/member/magazine_screen.dart';
import '../../screens/member/about_screen.dart';
import '../../screens/admin/approval_queue_screen.dart';
import '../../screens/admin/audit_screen.dart';
import '../../screens/admin/ledger_screen.dart';
import '../../screens/admin/theme_management_screen.dart';
import '../../screens/admin/fee_config_screen.dart';
import '../../screens/admin/contact_messages_screen.dart';
import '../../screens/admin/admin_dashboard_screen.dart';
import '../../screens/admin/governance_registry_screen.dart';
import '../../screens/admin/article_approval_screen.dart';
import '../../screens/admin/gatekeeper_screen.dart';
import '../../core/widgets/main_shell.dart';

final routerProvider = Provider<GoRouter>((ref) {
  return GoRouter(
    initialLocation: '/',
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
          GoRoute(path: '/news', name: 'news', builder: (context, state) => const NewsScreen()),
          GoRoute(
            path: '/news/:id',
            name: 'news_details',
            builder: (context, state) => NewsDetailsScreen(newsId: int.parse(state.pathParameters['id']!)),
          ),
          GoRoute(path: '/gallery', name: 'gallery', builder: (context, state) => const GalleryScreen()),
          GoRoute(path: '/committee', name: 'committee', builder: (context, state) => const CommitteeScreen()),
          GoRoute(path: '/notifications', name: 'notifications', builder: (context, state) => const NotificationScreen()),
          GoRoute(path: '/activity', name: 'activity', builder: (context, state) => const MemberActivityHistoryScreen()),
          GoRoute(path: '/family', name: 'family', builder: (context, state) => const FamilyLinkScreen()),
          GoRoute(path: '/support', name: 'support', builder: (context, state) => const SupportScreen()),
          GoRoute(path: '/articles', name: 'articles', builder: (context, state) => const MemberArticlesScreen()),
          GoRoute(path: '/articles/submit', name: 'submit_article', builder: (context, state) => const SubmitArticleScreen()),
          GoRoute(path: '/magazine', name: 'magazine', builder: (context, state) => const MagazineScreen()),
          GoRoute(path: '/about', name: 'about', builder: (context, state) => const AboutScreen()),
          
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
          GoRoute(path: '/admin/articles', name: 'admin_articles', builder: (context, state) => const ArticleApprovalScreen()),
        ],
      ),
    ],
  );
});
