import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:share_plus/share_plus.dart';
import 'package:pdf/pdf.dart';
import 'package:pdf/widgets.dart' as pw;
import 'package:printing/printing.dart';
import '../../core/config/app_config.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/widgets/skeleton_loader.dart';
import '../../features/auth/auth_service.dart';

class DigitalIDScreen extends ConsumerWidget {
  const DigitalIDScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileAsync = ref.watch(userProfileProvider);
    
    return AppScaffold(
      title: 'Digital ID Registry',
      breadcrumb: 'Executive Hub > Digital ID Card',
      child: profileAsync.when(
        data: (profile) {
          final data = profile;
          if (data == null) return const Center(child: Text('Profile not found in registry.', style: TextStyle(color: Colors.white)));
          
          final photoPath = data['photoPath'];
          final photoUrl = photoPath != null ? '${AppConfig.apiBaseUrl}/$photoPath'.replaceAll('//', '/') : null;

          return Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 32.0),
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
                          Container(height: 14, color: AppTheme.royalGold),
                          Padding(
                            padding: const EdgeInsets.fromLTRB(32, 24, 32, 32),
                            child: Column(
                              children: [
                                const Row(
                                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                  children: [
                                    Icon(Icons.school_outlined, size: 28, color: AppTheme.royalGold),
                                    Text('Haragangian Alumni Digital Pass', style: TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold, fontSize: 13, letterSpacing: 1.2)),
                                  ],
                                ),
                                const SizedBox(height: 32),
                                Container(
                                  decoration: BoxDecoration(
                                    shape: BoxShape.circle,
                                    border: Border.all(color: AppTheme.royalGold, width: 2),
                                  ),
                                  child: CircleAvatar(
                                    radius: 64,
                                    backgroundColor: Colors.black,
                                    backgroundImage: photoUrl != null ? NetworkImage(photoUrl) : null,
                                    child: photoUrl == null ? Text(data['fullName']?[0] ?? '?', style: const TextStyle(fontSize: 48, color: AppTheme.royalGold, fontWeight: FontWeight.w900)) : null,
                                  ),
                                ),
                                const SizedBox(height: 24),
                                Text(data['fullName'] ?? 'N/A', style: const TextStyle(fontSize: 22, fontWeight: FontWeight.w900, color: Colors.white, height: 1.2), textAlign: TextAlign.center),
                                const SizedBox(height: 8),
                                Text('${data['currentDesignation'] ?? 'Alumnus'} • BATCH ${data['batch'] ?? ''}', style: const TextStyle(fontSize: 13, color: AppTheme.textSecondaryDark, fontWeight: FontWeight.w500), textAlign: TextAlign.center),
                                const SizedBox(height: 48),
                                Container(
                                  padding: const EdgeInsets.all(12),
                                  decoration: BoxDecoration(color: Colors.white.withValues(alpha: 0.9), borderRadius: BorderRadius.circular(12)),
                                  child: const Icon(Icons.qr_code_2, size: 90, color: Colors.black),
                                ),
                                const SizedBox(height: 16),
                                Text('ID: ${data['membershipId'] ?? 'PENDING'}'.toUpperCase(), style: const TextStyle(fontFamily: 'Courier', fontWeight: FontWeight.w900, fontSize: 13, color: Colors.white, letterSpacing: 2)),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 40),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                    children: [
                      Expanded(
                        child: ElevatedButton.icon(
                          onPressed: () {
                            HapticFeedback.lightImpact();
                            _handlePrint(data);
                          },
                          icon: const Icon(Icons.file_download_outlined, size: 18), 
                          label: const Text('PORTABLE PDF', style: TextStyle(fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 1)),
                          style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(vertical: 14)),
                        ),
                      ),
                      const SizedBox(width: 16),
                      Expanded(
                        child: OutlinedButton.icon(
                          onPressed: () {
                            HapticFeedback.lightImpact();
                            Share.share('Alumni Registry ID: ${data['membershipId'] ?? 'Pending'}\n${data['fullName']}');
                          },
                          icon: const Icon(Icons.share_outlined, size: 18), 
                          label: const Text('SHARE ID', style: TextStyle(fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 1)),
                          style: OutlinedButton.styleFrom(
                            padding: const EdgeInsets.symmetric(vertical: 14),
                            side: const BorderSide(color: AppTheme.royalGold, width: 2),
                          ),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          );
        },
        loading: () => const Center(
          child: Padding(
            padding: EdgeInsets.all(24.0),
            child: AspectRatio(
                aspectRatio: 0.63,
                child: SkeletonLoader(width: double.infinity, height: double.infinity, borderRadius: 24)),
          ),
        ),
        error: (e, s) => Center(child: Text('Error: $e')),
      ),
    );
  }

  Future<void> _handlePrint(Map<String, dynamic> data) async {
    final pdf = pw.Document();

    pdf.addPage(
      pw.Page(
        pageFormat: PdfPageFormat.a4,
        build: (pw.Context context) {
          return pw.Center(
            child: pw.Container(
              width: 300,
              height: 480,
              decoration: pw.BoxDecoration(
                border: pw.Border.all(color: PdfColors.amber800, width: 2),
                borderRadius: const pw.BorderRadius.all(pw.Radius.circular(20)),
              ),
              child: pw.Padding(
                padding: const pw.EdgeInsets.all(30),
                child: pw.Column(
                  children: [
                    pw.Text(AppConfig.organizationName.toUpperCase(), textAlign: pw.TextAlign.center, style: pw.TextStyle(fontSize: 10, fontWeight: pw.FontWeight.bold)),
                    pw.SizedBox(height: 10),
                    pw.Divider(color: PdfColors.amber800),
                    pw.SizedBox(height: 20),
                    pw.Text('OFFICIAL ID CARD', style: const pw.TextStyle(fontSize: 8, color: PdfColors.grey700)),
                    pw.SizedBox(height: 30),
                    pw.Container(
                      width: 100,
                      height: 100,
                      decoration: const pw.BoxDecoration(color: PdfColors.grey300, shape: pw.BoxShape.circle),
                      child: pw.Center(child: pw.Text(data['fullName']?[0] ?? '?', style: pw.TextStyle(fontSize: 40, fontWeight: pw.FontWeight.bold))),
                    ),
                    pw.SizedBox(height: 20),
                    pw.Text(data['fullName'] ?? 'N/A', style: pw.TextStyle(fontSize: 18, fontWeight: pw.FontWeight.bold)),
                    pw.Text(data['membershipId'] ?? 'PENDING', style: const pw.TextStyle(fontSize: 14)),
                    pw.SizedBox(height: 30),
                    pw.BarcodeWidget(
                      barcode: pw.Barcode.qrCode(),
                      data: data['membershipId'] ?? 'PENDING',
                      width: 80,
                      height: 80,
                    ),
                    pw.Spacer(),
                    pw.Text('Valid for ${DateTime.now().year + 1}', style: const pw.TextStyle(fontSize: 8, color: PdfColors.grey)),
                  ],
                ),
              ),
            ),
          );
        },
      ),
    );

    await Printing.layoutPdf(
      onLayout: (PdfPageFormat format) async => pdf.save(),
      name: 'ID_Card_${data['membershipId'] ?? 'Member'}.pdf',
    );
  }
}
