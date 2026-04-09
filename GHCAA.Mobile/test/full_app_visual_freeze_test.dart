import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:golden_toolkit/golden_toolkit.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:ghcaa_mobile/screens/member/governance_screen.dart';
import 'package:ghcaa_mobile/screens/member/committee_screen.dart';
import 'package:ghcaa_mobile/screens/member/directory_screen.dart';
import 'package:ghcaa_mobile/screens/member/events_screen.dart';
import 'package:ghcaa_mobile/screens/member/news_screen.dart';
import 'package:ghcaa_mobile/screens/member/gallery_screen.dart';
import 'package:ghcaa_mobile/screens/member/jobs_screen.dart';
import 'package:ghcaa_mobile/screens/member/magazine_screen.dart';
import 'package:ghcaa_mobile/screens/member/financial_portal_screen.dart';
import 'package:ghcaa_mobile/screens/member/profile_screen.dart';
import 'package:ghcaa_mobile/screens/member/support_screen.dart';
import 'package:ghcaa_mobile/screens/member/ai_chat_screen.dart';
import 'package:ghcaa_mobile/screens/member/chats_screen.dart';
import 'package:ghcaa_mobile/screens/member/notification_screen.dart';
import 'package:ghcaa_mobile/screens/member/activity_log_screen.dart';
import 'package:ghcaa_mobile/screens/member/articles_screen.dart';
import 'package:ghcaa_mobile/screens/member/mentorship_hub_screen.dart';
import 'package:ghcaa_mobile/screens/auth/login_screen.dart';
import 'package:ghcaa_mobile/screens/auth/register_screen.dart';

// Helper to wrap a widget in a standard app shell
Widget wrapInApp(Widget child) => ProviderScope(
      child: MaterialApp(
        debugShowCheckedModeBanner: false,
        home: child,
      ),
    );

void main() {
  group('Mobile Full-App Visual Freeze', () {
    // ---- AUTH ----
    testGoldens('Auth: Login screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const LoginScreen()));
      await screenMatchesGolden(tester, 'auth_login');
    });

    testGoldens('Auth: Register screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const RegisterScreen()));
      await screenMatchesGolden(tester, 'auth_register');
    });

    // ---- GOVERNANCE ----
    testGoldens('Governance: EC Constitution screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const GovernanceScreen()));
      await screenMatchesGolden(tester, 'governance_constitution');
    });

    testGoldens('Governance: Committee Registry screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const CommitteeScreen()));
      await screenMatchesGolden(tester, 'governance_committee');
    });

    // ---- DIRECTORY ----
    testGoldens('Directory: Member listing screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const DirectoryScreen()));
      await screenMatchesGolden(tester, 'directory_listing');
    });

    // ---- EVENTS ----
    testGoldens('Events: Event list screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const EventsScreen()));
      await screenMatchesGolden(tester, 'events_list');
    });

    // ---- CONTENT ----
    testGoldens('Content: News feed screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const NewsScreen()));
      await screenMatchesGolden(tester, 'content_news_feed');
    });

    testGoldens('Content: Gallery grid screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const GalleryScreen()));
      await screenMatchesGolden(tester, 'content_gallery');
    });

    testGoldens('Content: Magazine screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MagazineScreen()));
      await screenMatchesGolden(tester, 'content_magazine');
    });

    testGoldens('Content: Articles submission screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MemberArticlesScreen()));
      await screenMatchesGolden(tester, 'content_articles');
    });

    // ---- CAREER ----
    testGoldens('Career: Job listings screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const JobsScreen()));
      await screenMatchesGolden(tester, 'career_jobs');
    });

    testGoldens('Career: Mentorship hub screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const MentorshipHubScreen()));
      await screenMatchesGolden(tester, 'career_mentorship');
    });

    // ---- FINANCE ----
    testGoldens('Finance: Member payment portal screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const FinancialPortalScreen()));
      await screenMatchesGolden(tester, 'finance_portal');
    });

    // ---- PROFILE ----
    testGoldens('Profile: Member profile screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ProfileScreen()));
      await screenMatchesGolden(tester, 'profile_view');
    });

    // ---- SUPPORT ----
    testGoldens('Support: Help & support screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const SupportScreen()));
      await screenMatchesGolden(tester, 'support_screen');
    });

    testGoldens('Support: AI Chat screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const AIChatScreen()));
      await screenMatchesGolden(tester, 'support_ai_chat');
    });

    testGoldens('Support: Chats/Messaging inbox screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ChatsScreen()));
      await screenMatchesGolden(tester, 'support_chats');
    });

    // ---- NOTIFICATIONS & ACTIVITY ----
    testGoldens('Notifications: Notification center screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const NotificationScreen()));
      await screenMatchesGolden(tester, 'notifications_center');
    });

    testGoldens('Activity: Activity log screen', (tester) async {
      await tester.pumpWidgetBuilder(wrapInApp(const ActivityLogScreen()));
      await screenMatchesGolden(tester, 'activity_log');
    });
  });
}
