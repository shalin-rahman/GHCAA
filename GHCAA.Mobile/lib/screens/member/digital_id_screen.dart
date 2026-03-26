import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:share_plus/share_plus.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../features/auth/auth_service.dart';

class DigitalIDScreen extends ConsumerWidget {
  const DigitalIDScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileAsync = ref.watch(userProfileProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AppScaffold(
      title: 'Digital ID Card',
      child: profileAsync.when(
        data: (profile) {
          final data = profile;
          if (data == null) return const Center(child: Text('Profile not found.', style: TextStyle(color: Colors.white)));
          return Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(24.0),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  AspectRatio(
                    aspectRatio: 0.63,
                    child: GlassContainer(
                      padding: EdgeInsets.zero,
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          Container(height: 12, color: AppTheme.royalGold),
                          Padding(
                            padding: const EdgeInsets.all(32.0),
                            child: Column(
                              children: [
                                Row(
                                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                  children: [
                                    const Icon(Icons.school, size: 32, color: AppTheme.royalGold),
                                    Text('GHCAA MEMBER', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 10, letterSpacing: 1.2, color: AppTheme.textSecondaryDark.withOpacity(0.5))),
                                  ],
                                ),
                                const SizedBox(height: 32),
                                CircleAvatar(
                                  radius: 60,
                                  backgroundColor: AppTheme.royalGold.withOpacity(0.1),
                                  child: Text(data['fullName']?[0] ?? '?', style: const TextStyle(fontSize: 40, color: AppTheme.royalGold, fontWeight: FontWeight.w900)),

                                ),
                                const SizedBox(height: 24),
                                Text(data['fullName'] ?? 'N/A', style: const TextStyle(fontSize: 24, fontWeight: FontWeight.w900, color: Colors.white), textAlign: TextAlign.center),

                                const SizedBox(height: 8),
                                Text('${data['currentDesignation'] ?? 'Alumnus'} • ${data['passingYear'] ?? ''}', style: TextStyle(fontSize: 14, color: AppTheme.textSecondaryDark), textAlign: TextAlign.center),

                                const SizedBox(height: 40),
                                Container(
                                  padding: const EdgeInsets.all(16),
                                  decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(16)),
                                  child: const Icon(Icons.qr_code_2, size: 100, color: Colors.black),
                                ),
                                const SizedBox(height: 16),
                                Text('ID: ${data['membershipId'] ?? 'PENDING'}'.toUpperCase(), style: const TextStyle(fontFamily: 'Courier', fontWeight: FontWeight.w900, fontSize: 12, color: Colors.white)),

                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 48),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                    children: [
                      SizedBox(
                        width: (MediaQuery.of(context).size.width - 64) / 2.2,
                        child: ElevatedButton.icon(onPressed: () {}, icon: const Icon(Icons.print), label: const Text('Print')),
                      ),
                      SizedBox(
                        width: (MediaQuery.of(context).size.width - 64) / 2.2,
                        child: OutlinedButton.icon(
                          onPressed: () => Share.share('My GHCAA Membership: ${data['membershipId'] ?? 'Pending'}'),

                          icon: const Icon(Icons.share_outlined), 
                          label: const Text('Share')
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          );
        },
        loading: () => const Center(child: CircularProgressIndicator(color: AppTheme.royalGold)),
        error: (e, s) => Center(child: Text('Error: $e')),
      ),
    );
  }
}
