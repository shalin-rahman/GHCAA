import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../theme/app_theme.dart';
import '../../features/auth/auth_service.dart';
import '../../features/messaging/chat_service.dart';
import '../../core/real_time/notification_hub_service.dart';

class MainShell extends ConsumerStatefulWidget {
  final Widget child;
  const MainShell({super.key, required this.child});

  @override
  ConsumerState<MainShell> createState() => _MainShellState();
}

class _MainShellState extends ConsumerState<MainShell> {
  @override
  void initState() {
    super.initState();
    _initHubs();
  }

  void _initHubs() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(chatServiceProvider).initHub();
      ref.read(notificationHubServiceProvider).initHub();
    });
  }

  @override
  Widget build(BuildContext context) {
    final location = GoRouterState.of(context).uri.path;
    final roleAsync = ref.watch(roleProvider);
    final isPageAdmin = roleAsync.value?.isStaffAdminRole ?? false;

    int calculateSelectedIndex(String location) {
      if (isPageAdmin) {
        if (location.startsWith('/admin/approvals')) return 1;
        if (location.startsWith('/admin/audit')) return 2;
        if (location.startsWith('/admin/ledger')) return 3;
        return 0; // admin_dashboard
      }
      if (location.startsWith('/directory')) return 1;
      if (location.startsWith('/digital_id')) return 2;
      if (location.startsWith('/profile')) return 3;
      return 0; // dashboard
    }

    void onItemTapped(int index, BuildContext context) {
      HapticFeedback.selectionClick();
      if (isPageAdmin) {
        switch (index) {
          case 0: context.go('/admin_dashboard'); break;
          case 1: context.go('/admin/approvals'); break;
          case 2: context.go('/admin/audit'); break;
          case 3: context.go('/admin/ledger'); break;
        }
      } else {
        switch (index) {
          case 0: context.go('/dashboard'); break;
          case 1: context.go('/directory'); break;
          case 2: context.go('/digital_id'); break;
          case 3: context.go('/profile'); break;
        }
      }
    }

    final primaryColor = isPageAdmin ? Colors.redAccent : AppTheme.royalGold;

    return Scaffold(
      body: widget.child,
      bottomNavigationBar: Container(
        decoration: BoxDecoration(
          color: AppTheme.midnightBase,
          border: Border(top: BorderSide(color: primaryColor.withValues(alpha: 0.4), width: 1.0)),
          boxShadow: isPageAdmin ? [BoxShadow(color: Colors.redAccent.withValues(alpha: 0.15), blurRadius: 15, offset: const Offset(0, -2))] : null,
        ),
        child: BottomNavigationBar(
          currentIndex: calculateSelectedIndex(location),
          onTap: (idx) => onItemTapped(idx, context),
          backgroundColor: AppTheme.midnightBase,
          selectedItemColor: primaryColor,
          unselectedItemColor: Colors.grey,
          type: BottomNavigationBarType.fixed,
          selectedFontSize: 12,
          unselectedFontSize: 10,
          elevation: 0,
          items: isPageAdmin ? const [
            BottomNavigationBarItem(icon: Icon(Icons.admin_panel_settings_outlined), activeIcon: Icon(Icons.admin_panel_settings), label: 'Base'),
            BottomNavigationBarItem(icon: Icon(Icons.task_alt), activeIcon: Icon(Icons.task_alt), label: 'Audit'),
            BottomNavigationBarItem(icon: Icon(Icons.analytics_outlined), activeIcon: Icon(Icons.analytics), label: 'Logs'),
            BottomNavigationBarItem(icon: Icon(Icons.account_balance_wallet_outlined), activeIcon: Icon(Icons.account_balance_wallet), label: 'Fiscal'),
          ] : const [
            BottomNavigationBarItem(icon: Icon(Icons.dashboard_outlined), activeIcon: Icon(Icons.dashboard), label: 'Home'),
            BottomNavigationBarItem(icon: Icon(Icons.people_outlined), activeIcon: Icon(Icons.people), label: 'Alumni'),
            BottomNavigationBarItem(icon: Icon(Icons.badge_outlined), activeIcon: Icon(Icons.badge), label: 'My ID'),
            BottomNavigationBarItem(icon: Icon(Icons.person_outline), activeIcon: Icon(Icons.person), label: 'Member'),
          ],
        ),
      ),
    );
  }
}
