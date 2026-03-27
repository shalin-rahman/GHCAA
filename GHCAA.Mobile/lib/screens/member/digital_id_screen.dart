import 'package:flutter/material.dart';
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
                                    Text('${AppConfig.organizationAcronym} OFFICIAL'.toUpperCase(), 
                                      style: TextStyle(fontWeight: FontWeight.w900, fontSize: 10, letterSpacing: 1.2, color: AppTheme.textSecondaryDark.withValues(alpha: 0.5))),
                                  ],
                                ),
                                const SizedBox(height: 32),
                                CircleAvatar(
                                  radius: 60,
                                  backgroundColor: AppTheme.royalGold.withValues(alpha: 0.1),
                                  child: Text(data['fullName']?[0] ?? '?', style: const TextStyle(fontSize: 40, color: AppTheme.royalGold, fontWeight: FontWeight.w900)),
                                ),
                                const SizedBox(height: 24),
                                Text(data['fullName'] ?? 'N/A', style: const TextStyle(fontSize: 24, fontWeight: FontWeight.w900, color: Colors.white), textAlign: TextAlign.center),
                                const SizedBox(height: 8),
                                Text('${data['currentDesignation'] ?? 'Alumnus'} • ${data['passingYear'] ?? ''}', style: const TextStyle(fontSize: 14, color: AppTheme.textSecondaryDark), textAlign: TextAlign.center),
                                const SizedBox(height: 40),
                                Container(
                                  padding: const EdgeInsets.all(16),
                                  decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(16)),
                                  child: const Icon(Icons.qr_code_2, size: 100, color: Colors.black),
                                ),
                                const SizedBox(height: 16),
                                Text('MEMBER ID: ${data['membershipId'] ?? 'PENDING'}'.toUpperCase(), style: const TextStyle(fontFamily: 'Courier', fontWeight: FontWeight.w900, fontSize: 12, color: Colors.white)),
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
                        child: ElevatedButton.icon(
                          onPressed: () => _handlePrint(data), 
                          icon: const Icon(Icons.picture_as_pdf), 
                          label: const Text('Export PDF')
                        ),
                      ),
                      SizedBox(
                        width: (MediaQuery.of(context).size.width - 64) / 2.2,
                        child: OutlinedButton.icon(
                          onPressed: () => Share.share('My ${AppConfig.organizationAcronym} Membership: ${data['membershipId'] ?? 'Pending'}'),
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
