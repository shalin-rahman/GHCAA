import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';

class MagazineScreen extends StatelessWidget {
  const MagazineScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Haragangian Journal',
      breadcrumb: 'Member Portal > Annual Publications',
      child: GridView.builder(
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
        gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 2,
          childAspectRatio: 0.65,
          crossAxisSpacing: 16,
          mainAxisSpacing: 20,
        ),
        itemCount: 4,
        itemBuilder: (context, index) {
          return InkWell(
            onTap: () {
              HapticFeedback.lightImpact();
              // Open reader
            },
            child: GlassContainer(
              padding: EdgeInsets.zero,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Expanded(
                    child: Container(
                      decoration: BoxDecoration(
                        color: AppTheme.royalGold.withValues(alpha: 0.05),
                        borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
                        image: DecorationImage(
                          image: NetworkImage('https://via.placeholder.com/300x450?text=EDITION+${2024 - index}'),
                          fit: BoxFit.cover,
                          opacity: 0.8,
                        ),
                      ),
                      child: Container(
                        padding: const EdgeInsets.all(12),
                        alignment: Alignment.topRight,
                        child: Container(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                          decoration: BoxDecoration(color: AppTheme.royalGold, borderRadius: BorderRadius.circular(4)),
                          child: const Text('PDF', style: TextStyle(color: Colors.black, fontSize: 8, fontWeight: FontWeight.w900, letterSpacing: 1)),
                        ),
                      ),
                    ),
                  ),
                  Padding(
                    padding: const EdgeInsets.all(12.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'EDITION ${2024 - index}', 
                          style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 13, color: Colors.white, letterSpacing: 1)
                        ),
                        const SizedBox(height: 4),
                        const Text(
                          'OFFICIAL ANNUAL JOURNAL', 
                          style: TextStyle(fontSize: 8, color: AppTheme.royalGold, fontWeight: FontWeight.w900, letterSpacing: 1)
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
