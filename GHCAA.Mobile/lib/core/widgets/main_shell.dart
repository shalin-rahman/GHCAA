import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../theme/app_theme.dart';

class MainShell extends StatelessWidget {
  final Widget child;
  const MainShell({super.key, required this.child});

  @override
  Widget build(BuildContext context) {
    final location = GoRouterState.of(context).uri.path;

    int calculateSelectedIndex(String location) {
      if (location == '/dashboard') return 0;
      if (location == '/directory') return 1;
      if (location == '/digital_id') return 2;
      if (location == '/profile') return 3;
      return 0;
    }

    void onItemTapped(int index, BuildContext context) {
      switch (index) {
        case 0: context.go('/dashboard'); break;
        case 1: context.go('/directory'); break;
        case 2: context.go('/digital_id'); break;
        case 3: context.go('/profile'); break;
      }
    }

    // Only show bottom nav for main member screens
    final showBottomNav = ['/dashboard', '/directory', '/digital_id', '/profile'].contains(location);

    if (!showBottomNav) return child;

    return Scaffold(
      body: child,
      bottomNavigationBar: Container(
        decoration: BoxDecoration(
          color: AppTheme.midnightBase,
          border: Border(top: BorderSide(color: AppTheme.royalGold.withOpacity(0.1), width: 0.5)),
        ),
        child: BottomNavigationBar(
          currentIndex: calculateSelectedIndex(location),
          onTap: (idx) => onItemTapped(idx, context),
          backgroundColor: AppTheme.midnightBase,
          selectedItemColor: AppTheme.royalGold,
          unselectedItemColor: Colors.grey,
          type: BottomNavigationBarType.fixed,
          selectedFontSize: 12,
          unselectedFontSize: 10,
          elevation: 0,
          items: const [
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
