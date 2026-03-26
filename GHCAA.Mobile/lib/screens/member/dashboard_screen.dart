import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/action_card.dart';
import '../../core/constants/app_constants.dart';

class DashboardScreen extends StatelessWidget {
  const DashboardScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Portal',
      actions: [
        IconButton(icon: const Icon(Icons.notifications_none), onPressed: () => context.go('/notifications')),
      ],
      bottomNavigationBar: _buildBottomNav(context, isDark),
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        child: Column(
          children: [
            GlassContainer(
              padding: const EdgeInsets.all(AppConstants.paddingLarge),
              child: Row(
                children: [
                   CircleAvatar(
                     radius: 30,
                     backgroundColor: AppTheme.royalGold.withOpacity(0.1),
                     child: const Icon(Icons.person, color: AppTheme.royalGold),
                   ),
                   const SizedBox(width: AppConstants.paddingMedium),
                   Expanded(
                     child: Column(
                       crossAxisAlignment: CrossAxisAlignment.start,
                       children: [
                         Text('Membership Status', style: Theme.of(context).textTheme.bodySmall),
                         const SizedBox(height: 4),
                         Text('LIFE MEMBER', style: Theme.of(context).textTheme.bodyLarge?.copyWith(fontWeight: FontWeight.bold, color: AppTheme.royalGold)),
                       ],
                     ),
                   ),
                ],
              ),
            ),
            const SizedBox(height: AppConstants.paddingExtraLarge),
            Wrap(
              spacing: AppConstants.paddingMedium,
              runSpacing: AppConstants.paddingMedium,
              children: [
                ActionCard(icon: Icons.people, title: 'Directory', onTap: () => context.go('/directory')),
                ActionCard(icon: Icons.event, title: 'Events', onTap: () => context.go('/events')),
                ActionCard(icon: Icons.work, title: 'Job Hub', onTap: () => context.go('/jobs')),
                ActionCard(icon: Icons.receipt_long, title: 'Economic Portal', onTap: () => context.go('/financials')),
                ActionCard(icon: Icons.newspaper, title: 'Notice Board', onTap: () => context.go('/news')),
                ActionCard(icon: Icons.photo_library, title: 'Gallery', onTap: () => context.go('/gallery')),
                ActionCard(icon: Icons.family_restroom, title: 'Family Link', onTap: () => context.go('/family')),
                ActionCard(icon: Icons.badge, title: 'Digital ID', onTap: () => context.go('/digital_id')),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildBottomNav(BuildContext context, bool isDark) {
    return BottomNavigationBar(
      backgroundColor: isDark ? AppTheme.midnightSurface : AppTheme.daylightSurface,
      selectedItemColor: AppTheme.royalGold,
      unselectedItemColor: isDark ? AppTheme.textSecondaryDark : AppTheme.textSecondaryLight,
      currentIndex: 0,
      onTap: (index) {
        switch (index) {
          case 0: break;
          case 1: context.go('/directory'); break;
          case 2: context.go('/profile'); break;
        }
      },
      type: BottomNavigationBarType.fixed,
      items: const [
        BottomNavigationBarItem(icon: Icon(Icons.dashboard), label: 'Home'),
        BottomNavigationBarItem(icon: Icon(Icons.search), label: 'Directory'),
        BottomNavigationBarItem(icon: Icon(Icons.person), label: 'Profile'),
      ],
    );
  }
}
