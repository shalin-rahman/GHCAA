import 'package:flutter/material.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';

class CommitteeScreen extends StatelessWidget {
  const CommitteeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Executive Committee',
      child: ListView.builder(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        itemCount: 15,
        itemBuilder: (context, index) {
          return Padding(
            padding: const EdgeInsets.only(bottom: 12.0),
            child: GlassContainer(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  const CircleAvatar(
                    radius: 24,
                    backgroundColor: AppTheme.royalGold,
                    child: Icon(Icons.person, color: Colors.white),
                  ),
                  const SizedBox(width: AppConstants.paddingMedium),
                  const Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text('Major Gen. John Doe', style: TextStyle(fontWeight: FontWeight.bold)),
                        Text('President | 2024-2026', style: TextStyle(fontSize: 12, color: AppTheme.royalGold)),
                      ],
                    ),
                  ),
                  IconButton(icon: const Icon(Icons.info_outline, size: 16), onPressed: () {}),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
