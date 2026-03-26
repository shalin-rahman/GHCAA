import 'package:flutter/material.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/constants/app_constants.dart';

class AdminCommunicationHub extends StatelessWidget {
  const AdminCommunicationHub({super.key});

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      isAdmin: true,
      title: 'Communication Hub',
      child: Padding(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('Draft Notification/SMS', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
            const SizedBox(height: AppConstants.paddingMedium),
            TextField(decoration: InputDecoration(labelText: 'Title', border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)))),
            const SizedBox(height: AppConstants.paddingMedium),
            TextField(maxLines: 4, decoration: InputDecoration(labelText: 'Message Body', border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)))),
            const SizedBox(height: AppConstants.paddingMedium),
            Row(
              children: [
                Checkbox(value: true, onChanged: (v) {}),
                const Text('Push Notification'),
                Checkbox(value: false, onChanged: (v) {}),
                const Text('SMS Blast'),
              ],
            ),
            const SizedBox(height: AppConstants.paddingExtraLarge),
            SizedBox(width: double.infinity, child: ElevatedButton(onPressed: () {}, child: const Text('Broadcast Now'))),
          ],
        ),
      ),
    );
  }
}

class AdminCMS extends StatelessWidget {
  const AdminCMS({super.key});

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      isAdmin: true,
      title: 'Media CMS',
      child: ListView.builder(
        padding: const EdgeInsets.all(AppConstants.paddingLarge),
        itemCount: 4,
        itemBuilder: (context, index) {
          return Padding(
            padding: const EdgeInsets.only(bottom: 12.0),
            child: GlassContainer(
              padding: const EdgeInsets.all(16),
              child: Row(
                children: [
                  Container(width: 80, height: 60, color: AppTheme.royalGold.withOpacity(0.1), child: const Icon(Icons.image)),
                  const SizedBox(width: AppConstants.paddingMedium),
                  const Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [Text('Gallery Item #12', style: TextStyle(fontWeight: FontWeight.bold)), Text('Mar 24, 2026', style: TextStyle(fontSize: 10))])),
                  IconButton(icon: const Icon(Icons.edit, size: 16), onPressed: () {}),
                  IconButton(icon: const Icon(Icons.delete, size: 16, color: Colors.red), onPressed: () {}),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
