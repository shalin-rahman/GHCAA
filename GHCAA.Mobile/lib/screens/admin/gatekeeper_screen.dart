import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile_scanner/mobile_scanner.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/app_scaffold.dart';
import '../../core/widgets/glass_container.dart';
import '../../core/api/api_client.dart';
import '../../core/config/app_config.dart';

class GatekeeperScreen extends ConsumerStatefulWidget {
  const GatekeeperScreen({super.key});

  @override
  ConsumerState<GatekeeperScreen> createState() => _GatekeeperScreenState();
}

class _GatekeeperScreenState extends ConsumerState<GatekeeperScreen> {
  bool _isScanning = true;
  Map<String, dynamic>? _scannedMember;
  bool _isLoading = false;
  String? _error;

  final MobileScannerController _controller = MobileScannerController(
    detectionSpeed: DetectionSpeed.noDuplicates,
    facing: CameraFacing.back,
  );

  Future<void> _verifyMember(int id) async {
    setState(() {
      _isLoading = true;
      _error = null;
      _isScanning = false;
    });

    try {
      final dio = ref.read(dioProvider);
      final response = await dio.get('/admin/members/$id');
      
      if (response.statusCode == 200) {
        setState(() {
          _scannedMember = response.data;
          _isLoading = false;
        });
        HapticFeedback.heavyImpact();
      } else {
        throw Exception('Member not found or unauthorized');
      }
    } catch (e) {
      setState(() {
        _error = 'Registry Error: Identity Node not found in archive.';
        _isLoading = false;
      });
      HapticFeedback.vibrate();
    }
  }

  void _reset() {
    setState(() {
      _scannedMember = null;
      _error = null;
      _isScanning = true;
    });
    _controller.start();
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      isAdmin: true,
      title: 'Identity Verification Matrix',
      breadcrumb: 'Admin Ops > Identity Matrix',
      child: Column(
        children: [
          Expanded(
            child: _isScanning 
              ? _buildScanner() 
              : _buildResultView(),
          ),
          if (!_isScanning)
            Padding(
              padding: const EdgeInsets.all(24.0),
              child: SizedBox(
                width: double.infinity,
                height: 56,
                child: ElevatedButton.icon(
                  onPressed: _reset,
                  icon: const Icon(Icons.qr_code_scanner_rounded, color: Colors.black),
                  label: const Text('INITIALIZE NEXT SCAN', style: TextStyle(color: Colors.black, fontWeight: FontWeight.w900, letterSpacing: 1)),
                  style: ElevatedButton.styleFrom(backgroundColor: AppTheme.royalGold),
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildScanner() {
    return Stack(
      children: [
        MobileScanner(
          controller: _controller,
          onDetect: (capture) {
            final List<Barcode> barcodes = capture.barcodes;
            for (final barcode in barcodes) {
              final String? code = barcode.rawValue;
              if (code != null) {
                final id = int.tryParse(code.replaceAll(RegExp(r'[^0-9]'), ''));
                if (id != null) {
                  _verifyMember(id);
                  break;
                }
              }
            }
          },
        ),
        _buildScannerOverlay(),
        Positioned(
          bottom: 40,
          left: 0,
          right: 0,
          child: Center(
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 10),
              decoration: BoxDecoration(color: Colors.black54, borderRadius: BorderRadius.circular(20)),
              child: const Text('Align QR code within the frame', style: TextStyle(color: Colors.white70, fontSize: 12)),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildScannerOverlay() {
    return Center(
      child: Container(
        width: 250,
        height: 250,
        decoration: BoxDecoration(
          border: Border.all(color: AppTheme.royalGold, width: 2),
          borderRadius: BorderRadius.circular(24),
        ),
        child: Stack(
          children: [
            _buildCorner(0, 0, 20, 0),
            _buildCorner(null, 0, 0, 20),
            _buildCorner(0, null, 20, 0),
            _buildCorner(null, null, 0, 20),
          ],
        ),
      ),
    );
  }

  Widget _buildCorner(double? left, double? top, double br, double bl) {
    return Positioned(
      left: left,
      top: top,
      child: Container(
        width: 40,
        height: 40,
        decoration: BoxDecoration(
          color: AppTheme.royalGold,
          borderRadius: BorderRadius.only(
            topLeft: Radius.circular(left == 0 && top == 0 ? 0 : 0),
          ),
        ),
      ),
    );
  }

  Widget _buildResultView() {
    if (_isLoading) {
      return const Center(child: CircularProgressIndicator(color: AppTheme.royalGold));
    }

    if (_error != null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.error_outline_rounded, size: 80, color: Colors.redAccent),
            const SizedBox(height: 24),
            Text(_error!, style: const TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.bold), textAlign: TextAlign.center),
          ],
        ),
      );
    }

    if (_scannedMember != null) {
      final m = _scannedMember!;
      final photoUrl = m['photoPath'] != null 
          ? '${AppConfig.apiBaseUrl}/${m['photoPath']}'.replaceAll('//', '/') 
          : null;

      return SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          children: [
            const Icon(Icons.check_circle_outline_rounded, size: 80, color: Colors.greenAccent),
            const SizedBox(height: 16),
            const Text('IDENTITY VERIFIED', style: TextStyle(color: Colors.greenAccent, fontWeight: FontWeight.w900, letterSpacing: 2, fontSize: 14)),
            const SizedBox(height: 32),
            GlassContainer(
              padding: const EdgeInsets.all(24),
              child: Column(
                children: [
                  CircleAvatar(
                    radius: 60,
                    backgroundColor: Colors.black26,
                    backgroundImage: photoUrl != null ? NetworkImage(photoUrl) : null,
                    child: photoUrl == null ? const Icon(Icons.person, size: 60, color: AppTheme.royalGold) : null,
                  ),
                  const SizedBox(height: 24),
                  Text(m['fullName']?.toString().toUpperCase() ?? 'UNKNOWN MEMBER', style: const TextStyle(fontSize: 20, fontWeight: FontWeight.w900, color: Colors.white)),
                  const SizedBox(height: 8),
                  Text('ID: ${m['membershipNumber'] ?? 'PENDING'}', style: const TextStyle(color: AppTheme.royalGold, fontWeight: FontWeight.bold, fontSize: 14)),
                  const Divider(color: Colors.white10, height: 40),
                  _buildInfoRow('STATUS', m['status'] ?? 'N/A', isBold: true),
                  _buildInfoRow('BATCH', m['passingYear']?.toString() ?? 'N/A'),
                  _buildInfoRow('DEPARTMENT', m['subject'] ?? 'N/A'),
                  _buildInfoRow('MEMBERSHIP', m['membershipType'] ?? 'N/A'),
                ],
              ),
            ),
          ],
        ),
      );
    }

    return const SizedBox();
  }

  Widget _buildInfoRow(String label, String value, {bool isBold = false}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: const TextStyle(color: AppTheme.textSecondaryDark, fontSize: 10, fontWeight: FontWeight.bold, letterSpacing: 1)),
          Text(value, style: TextStyle(color: isBold ? AppTheme.royalGold : Colors.white, fontSize: 12, fontWeight: isBold ? FontWeight.w900 : FontWeight.bold)),
        ],
      ),
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }
}
