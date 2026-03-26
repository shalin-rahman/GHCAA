import 'package:flutter/material.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_tile.dart';
import '../../core/constants/app_constants.dart';

class AdminAuditScreen extends StatelessWidget {
  const AdminAuditScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      isAdmin: true,
      title: 'Global Audit Trail',
      child: ListView.builder(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        itemCount: 20,
        itemBuilder: (context, index) {
          return Padding(
            padding: const EdgeInsets.only(bottom: 12.0),
            child: GlassTile(
              icon: Icons.history_edu,
              title: index % 2 == 0 ? 'Member Approved' : 'System Configuration Updated',
              subtitle: 'By Admin #101 | 2026-03-27 10:45',
              onTap: () {},
              trailing: const Icon(Icons.info_outline, size: 16, color: AppTheme.royalGold),
            ),
          );
        },
      ),
    );
  }
}
