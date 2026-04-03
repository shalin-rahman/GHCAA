import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';
import '../../features/theme/dynamic_theme_service.dart';

class ThemeManagementScreen extends ConsumerWidget {
  const ThemeManagementScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final activeThemeAsync = ref.watch(activeSpecialThemeProvider);

    return AppScaffold(
      isAdmin: true,
      title: 'Special Themes',
      breadcrumb: 'ADMIN > THEMES',
      child: ListView(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        children: [
          const Text('Theme Configurations', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
          const SizedBox(height: 16),
          activeThemeAsync.when(
            data: (theme) => GlassContainer(
              child: ListTile(
                leading: Container(
                  width: 40,
                  height: 40,
                  decoration: BoxDecoration(
                    color: theme?.backgroundColor ?? AppTheme.royalGold,
                    shape: BoxShape.circle,
                  ),
                ),
                title: Text(theme?.title ?? 'No Special Theme Active', style: const TextStyle(fontWeight: FontWeight.bold)),
                subtitle: Text(theme != null ? 'Active Announcement: ${theme.announcement}' : 'Using default Midnight Gold'),
                trailing: Switch(
                  value: theme != null,
                  onChanged: (val) {},
                  activeThumbColor: AppTheme.royalGold,
                ),
              ),
            ),
            loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
            error: (e, s) => Center(child: Text('Error: $e')),
          ),
          const SizedBox(height: 32),
          _buildThemeCard(context, 'Bijoy Dibosh', Colors.red, 'Happy Victory Day 2024!'),
          _buildThemeCard(context, 'Independence Day', Colors.green, 'Long Live Bangladesh!'),
          _buildThemeCard(context, 'Internal Event', Colors.indigo, 'Annual Reunion 2025 is here!'),
        ],
      ),
    );
  }

  Widget _buildThemeCard(BuildContext context, String name, Color color, String msg) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12.0),
      child: GlassContainer(
        child: ListTile(
          leading: CircleAvatar(backgroundColor: color, radius: 12),
          title: Text(name),
          subtitle: Text(msg, maxLines: 1),
          onTap: () {},
          trailing: const Icon(Icons.palette_outlined, size: 16),
        ),
      ),
    );
  }
}
